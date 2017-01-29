#pragma once

#include <vector>

struct BinaryReader {
	std::vector<uint8_t> data;
	int offset;

	BinaryReader(const std::vector<uint8_t>& data) : data(data), offset(0) { }

	uint8_t* read(int length) {
		uint8_t* ptr = &data.at(offset);
		offset += length;
		return ptr;
	}

	uint32_t readUint32() {
		return *(uint32_t*)read(4);
	}

	uint8_t readByte() {
		return *read(1);
	}

	std::vector<uint8_t> readLengthPrefixed() {
		auto len = readUint32();
		auto ptr = read(len);
		return std::vector<uint8_t>(ptr, ptr + len);
	}

	std::string readLengthPrefixedStr() {
		auto len = readUint32();
		auto ptr = read(len);
		return std::string((const char*)ptr, (size_t)len);
	}
};
