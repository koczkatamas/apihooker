#pragma once

#include <vector>

enum class FieldType : uint32_t { ByteArray = 1, NullTerminatedAnsiString = 2, Type = 3 };

struct FieldDescriptor {
	FieldType type;
	int length;
	bool isArray;
	bool isPointer;
	std::vector<FieldDescriptor> fields;

	FieldDescriptor(int length = -1) :type(FieldType::ByteArray), length(length), isArray(false), isPointer(false) { }
	FieldDescriptor(FieldType type) :type(type), length(-1), isArray(false), isPointer(type == FieldType::NullTerminatedAnsiString) { }
	FieldDescriptor(std::vector<FieldDescriptor> fields, bool isPointer = true) :type(FieldType::Type), length(-1), isArray(false), isPointer(isPointer), fields(fields) { }
};
