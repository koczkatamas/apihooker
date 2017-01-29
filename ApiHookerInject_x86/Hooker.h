#pragma once

#include <vector>
#include <MinHook/MinHook.h>
#include <Utils/BinaryReader.h>
#include <Utils/BinaryWriter.h>
#include <Utils/SpinLockMutex.h>
#include <Utils/SimpleHeap.h>
#include <StructSerialization/Model.h>
#include <StructSerialization/SerializationHelper.h>

void HookHandlerPure();

struct Hooker {
	struct PreCallValues {
		uint32_t EDI, ESI, EBP, oESP, EBX, EDX, ECX, EAX, hookCallAddr, retAddr;
		uint32_t arguments[16];
	};

	struct HookedFunctionInfo {
		uint8_t callCode[5]; // E8 = call
		LPVOID original;
		int id;
		bool hookActive;
		std::vector<FieldDescriptor> arguments;
	};

	static BinaryWriter bw;
	static SpinLockMutex bwLock;
	static SimpleHeap funcHeap;
	static std::vector<HookedFunctionInfo*> funcs;
	static volatile LONG lastCallId;

	static void HookHandler() {
		PreCallValues* preCall;
		HookedFunctionInfo* info;
		__asm lea eax, dword ptr[ebp + 8]
		__asm mov preCall, eax
		info = (HookedFunctionInfo*)(preCall->hookCallAddr - 5);
		auto runOriginal = true;
		int threadId = __readfsdword(0x24);

		// TODO: add thread-safety

		//	if (info->argumentCount > 0) {
		//		uint32_t* currPtr = &preCall->arguments[0];
		//		void* endPtr = &preCall->arguments[info->argumentCount];
		//		__asm mov eax, currPtr
		//		__asm mov ebx, endPtr
		//argPush:
		//		__asm push dword ptr[eax];
		//		__asm add eax, 4
		//		__asm cmp eax, ebx
		//		__asm jl argPush
		//	}

		//info->arguments.fields

		auto callId = InterlockedIncrement(&lastCallId);

		auto argStartPtr = (uint8_t*)&preCall->arguments[0];
		BinaryWriter localBw;
		localBw.writeUint32(callId);
		localBw.writeUint32(info->id);
		localBw.writeUint32(threadId);
		SerializationHelper::writeFields(localBw, info->arguments, argStartPtr);

		bwLock.Enter();
		bw.write(&localBw.data[0], localBw.data.size(), true);
		bwLock.Leave();

		if (runOriginal) {
			for (int i = info->arguments.size() - 1; i >= 0; i--) {
				uint32_t argValue = preCall->arguments[i];
				__asm push argValue;
			}
		}

		// we simulate ret N
		preCall->EDI = preCall->oESP + 4 /* retVal */ + 4 * info->arguments.size();
		*(uint32_t*)preCall->EDI = preCall->retAddr;

		if (runOriginal) {
			auto original = info->original;
			__asm call original
		}
	}

	static HookedFunctionInfo* createHookInfo(std::vector<FieldDescriptor> arguments) {
		auto info = (HookedFunctionInfo*) funcHeap.alloc(sizeof(HookedFunctionInfo));
		info->arguments = arguments;
		info->id = funcs.size();
		info->hookActive = false;
		info->callCode[0] = 0xE8; // E8 = call
		*(uint32_t*)&info->callCode[1] = (uint32_t)&HookHandlerPure - (uint32_t)&info->callCode - 5; // 5 = sizeof call
		funcs.push_back(info);
		return info;
	}

	static HookedFunctionInfo* hookThis(LPVOID pTarget, std::vector<FieldDescriptor> arguments) {
		auto info = createHookInfo(arguments);
		if (MH_CreateHook(pTarget, &info->callCode, &info->original) != MH_OK) return nullptr;
		info->hookActive = true;
		return info;
	}
};

volatile LONG Hooker::lastCallId = 0;
BinaryWriter Hooker::bw;
SpinLockMutex Hooker::bwLock;
SimpleHeap Hooker::funcHeap;
std::vector<Hooker::HookedFunctionInfo*> Hooker::funcs;

void __declspec(naked) HookHandlerPure() {
	__asm pushad
	__asm call Hooker::HookHandler
	__asm mov esp, dword ptr[esp] // new esp == dword ptr[esp] == preCall->EDI
	__asm ret
}