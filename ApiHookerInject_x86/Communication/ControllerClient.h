#pragma once

#include <vector>
#include <winsock2.h>

// Need to link with Ws2_32.lib, Mswsock.lib, and Advapi32.lib
#pragma comment (lib, "Ws2_32.lib")
#pragma comment (lib, "Mswsock.lib")
#pragma comment (lib, "AdvApi32.lib")

struct ControllerClient {
	enum class PacketType : uint32_t {
		Invalid = 0,
		MsgBox = 1,
		ReadBuffer = 2,
		Terminate = 3,
		HookFuncs = 4
	};

	struct Packet {
		PacketType type = PacketType::Invalid;
		std::vector<uint8_t> payload;
	};

	enum class Result {
		OK = 0,
		WSAStartupError = 1,
		ConnectError = 2,
		TypeReadError = 3,
		LengthReadError = 4,
		PayloadReadError = 5,
		LengthWriteError = 6,
		PayloadWriteError = 7
	};

	uint16_t port;
	SOCKET socket;

	ControllerClient(uint16_t port) : port(port), socket(0) { }

	Result connect() {
		WSAData wsaData;
		if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
			return Result::WSAStartupError;

		socket = ::socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

		SOCKADDR_IN sockAddr;
		sockAddr.sin_port = htons(port);
		sockAddr.sin_family = AF_INET;
		sockAddr.sin_addr.s_addr = htonl(INADDR_LOOPBACK);

		if (::connect(socket, (SOCKADDR*)(&sockAddr), sizeof(sockAddr)) != 0)
			return Result::ConnectError;

		return Result::OK;
	}

	Result readPacket(Packet& result) {
		if (recv(socket, reinterpret_cast<char*>(&result.type), sizeof(result.type), 0) != sizeof(PacketType))
			return Result::TypeReadError;

		uint32_t packetLen;
		if (recv(socket, reinterpret_cast<char*>(&packetLen), sizeof(packetLen), 0) != sizeof(packetLen))
			return Result::LengthReadError;

		result.payload.resize(packetLen);
		if (recv(socket, reinterpret_cast<char*>(result.payload.data()), packetLen, 0) != packetLen)
			return Result::PayloadReadError;

		return Result::OK;
	}

	Packet readPacket() {
		Packet result;
		readPacket(result);
		return result;
	}

	Result send(const std::vector<uint8_t>& data) {
		uint32_t dataLen = data.size();

		if (::send(socket, (const char*)&dataLen, 4, 0) != 4)
			return Result::LengthWriteError;

		if (dataLen > 0 && ::send(socket, (const char*)&data[0], dataLen, 0) != dataLen)
			return Result::PayloadWriteError;

		return Result::OK;
	}

	~ControllerClient() {
		if (socket) {
			shutdown(socket, SD_SEND);
			closesocket(socket);
		}

		socket = 0;
	}
};