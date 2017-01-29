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
		union {
			uint32_t EDI;
			uint32_t returnESP;
		};
		
		union {
			uint32_t ESI;
			uint32_t returnEAX;
		};

		uint32_t EBP, oESP, EBX, EDX, ECX, EAX, hookCallAddr, retAddr;
		uint32_t arguments[16];
	};

	struct HookedFunctionInfo {
		uint8_t callCode[5]; // E8 = call
		LPVOID original;
		int id;
		bool hookActive;
		bool saveCallStack;
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
		auto callId = InterlockedIncrement(&lastCallId);

		auto argStartPtr = (uint8_t*)&preCall->arguments[0];
		BinaryWriter localBw;
		localBw.writeUint32(callId);
		localBw.writeUint32(info->id);
		localBw.writeUint32(threadId);

		if (info->saveCallStack) {
			std::vector<void*> callStack;
			callStack.push_back((void*)preCall->retAddr);

			void** currEbp;
			__asm mov currEbp, ebp

			while (true) {
				currEbp = ((void***)currEbp)[0];

				if (IsBadReadPtr(currEbp, sizeof(void*) * 2))
					break;

				callStack.push_back(currEbp[1]);
			}

			localBw.writeUint32(callStack.size());
			localBw.write((uint8_t*)callStack.data(), callStack.size() * sizeof(callStack[0]));
		}

		SerializationHelper::writeFields(localBw, info->arguments, argStartPtr);

		int argumentCount = info->arguments.size();

		uint32_t returnValue = 0;
		if (runOriginal) {
			uint32_t espBeforeOriginalCall;
			__asm mov espBeforeOriginalCall, esp

			int callArgCount = 16;
			for (int i = callArgCount - 1; i >= 0; i--) {
				uint32_t argValue = preCall->arguments[i];
				__asm push argValue
			}

			auto original = info->original;
			__asm call original
			__asm mov returnValue, eax

			uint32_t espAfterOriginalCall;
			__asm mov espAfterOriginalCall, esp
			__asm mov esp, espBeforeOriginalCall

			argumentCount = callArgCount - (espBeforeOriginalCall - espAfterOriginalCall) / sizeof(int);

			localBw.writeUint32(returnValue);
			argStartPtr = (uint8_t*)&preCall->arguments[0];
			SerializationHelper::writeFields(localBw, info->arguments, argStartPtr);
		}

		bwLock.Enter();
		bw.write(&localBw.data[0], localBw.data.size(), true);
		bwLock.Leave();

		// helper to convert "ret N" into simple "ret" in HookHandlerPure, puts the return address into the stack and moves it to the right position
		preCall->returnESP = preCall->oESP + 4 /* retVal */ + argumentCount * sizeof(int);
		*(uint32_t*)preCall->returnESP = preCall->retAddr;
		preCall->returnEAX = returnValue;
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
	// stack here: EDI | ESI | EBP | oESP | EBX | EDX | ECX | EAX | hookCallAddr | retAddr | args[0] | args[1] | args[2] | args[3] | ...
	// oESP contains the address of hookCallAddr, etc (so everything except the values put there with pushad)
	__asm call Hooker::HookHandler
	__asm mov eax, dword ptr[esp + 4] // preCall->returnEAX
	__asm mov esp, dword ptr[esp] // simulate ret N using preCall->returnESP
	__asm ret
}