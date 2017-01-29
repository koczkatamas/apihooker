#pragma once

#include <windows.h>
#include <string>

#include <MinHook/MinHook.h>
#include <Communication/ControllerClient.h>
#include <StructSerialization/Model.h>
#include <StructSerialization/SerializationHelper.h>
#include <Hooker.h>

#define RETERR(a) { auto retCode = (int)(a); if(retCode != 0) return retCode; }

struct ApiHooker {
	struct InitParams {
		uint32_t tcpPort;
	};

	static DWORD WINAPI ThreadFuncMsgBox(InitParams* params) {
		auto result = ThreadFunc(params);
		if (result != 0)
			MessageBoxA(0, (std::string("ApiHookerThreadFunc result: ") + std::to_string(result)).c_str(), "Error", 0);
		return result;
	}

	static DWORD WINAPI ThreadFunc(InitParams* params) {
		using PacketType = ControllerClient::PacketType;

		ControllerClient client(params == nullptr ? 1337 : params->tcpPort);
		RETERR(client.connect());

		MH_Initialize();

		while (true) {
			ControllerClient::Packet packet;
			RETERR(client.readPacket(packet));

			if (packet.type == PacketType::MsgBox) {
				packet.payload.push_back(0);
				MessageBoxA(0, reinterpret_cast<char*>(packet.payload.data()), "Message from server", 0);
			}
			else if (packet.type == PacketType::ReadBuffer) {
				Hooker::bwLock.Enter();
				auto data = std::move(Hooker::bw.data);
				Hooker::bwLock.Leave();

				RETERR(client.send(data));
			}
			else if (packet.type == PacketType::HookFuncs) {
				BinaryWriter response;
				BinaryReader br(packet.payload);
				auto funcLen = br.readUint32();
				for (uint32_t i = 0; i < funcLen; i++) {
					auto moduleName = br.readLengthPrefixedStr();
					auto functionName = br.readLengthPrefixedStr();
					auto saveCallStack = br.readByte();
					auto typeDescBytes = br.readLengthPrefixed();
					auto typeDesc = SerializationHelper::readTypeDesc(BinaryReader(typeDescBytes));

					auto hModule = GetModuleHandle(moduleName.c_str());
					auto pTarget = GetProcAddress(hModule, functionName.c_str());
					auto info = Hooker::hookThis(pTarget, typeDesc);
					info->saveCallStack = saveCallStack == 1;
					response.writeUint32(info == nullptr ? -1 : info->id);
				}
				MH_EnableHook(MH_ALL_HOOKS);
				client.send(response.data);
			}
			else if (packet.type == PacketType::Terminate)
				break;
		}

		return 0;
	}
};