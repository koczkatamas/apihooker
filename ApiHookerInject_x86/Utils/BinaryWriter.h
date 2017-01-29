#pragma once

#include <vector>

struct BinaryWriter {
	std::vector<uint8_t> data;

	void write(uint8_t* data, int dataLen, bool storeSize = false) {
		if (storeSize)
			writeUint32(dataLen);

		this->data.insert(this->data.end(), data, data + dataLen);
	}

	void writeUint32(uint32_t value) {
		write((uint8_t*)&value, 4);
	}
};
