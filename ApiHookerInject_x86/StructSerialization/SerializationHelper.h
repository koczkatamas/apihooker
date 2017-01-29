#pragma once

#include <vector>
#include <StructSerialization/Model.h>
#include <Utils/BinaryReader.h>
#include <Utils/BinaryWriter.h>

struct SerializationHelper {
	static std::vector<FieldDescriptor> readTypeDesc(BinaryReader& reader) {
		std::vector<FieldDescriptor> result;

		int arrayLen = reader.readUint32();
		for (int i = 0; i < arrayLen; i++) {
			FieldDescriptor fd;
			fd.type = (FieldType)reader.readUint32();
			fd.length = reader.readUint32();
			fd.isArray = *reader.read(1) != 0;
			fd.isPointer = *reader.read(1) != 0;
			fd.fields = readTypeDesc(reader);
			result.push_back(fd);
		}

		return result;
	}

	static bool writeFields(BinaryWriter& writer, std::vector<FieldDescriptor>& fields, uint8_t*& data) {
		for (size_t i = 0; i < fields.size(); i++)
			if (!writeField(writer, fields[i], data)) return false;
		return true;
	}

	static bool writeField(BinaryWriter& writer, FieldDescriptor& desc, uint8_t*& data) {
		uint8_t* dataPtr = data;
		if (desc.isPointer) {
			dataPtr = (uint8_t*)*(uint32_t*)data;
			data += 4;
		}

		if (desc.type == FieldType::ByteArray) {
			if (desc.length == -1) return false;
			writer.write((uint8_t*)dataPtr, desc.length);
			dataPtr += desc.length;
		}
		else if (desc.type == FieldType::NullTerminatedAnsiString) {
			auto str = (char*)dataPtr;
			uint32_t strLen = strlen(str);
			writer.write((uint8_t*)str, strLen, true);
		}
		else if (desc.type == FieldType::Type) {
			if (desc.fields.size() == 0) return false;
			writeFields(writer, desc.fields, dataPtr);
		}
		else
			return false;

		if (!desc.isPointer)
			data = dataPtr;

		return true;
	}
};
