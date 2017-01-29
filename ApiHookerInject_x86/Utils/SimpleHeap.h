#pragma once
#include <windows.h>

// TODO: replace with a real heap implementation / use MinHook's one?
struct SimpleHeap {
	char* startPtr;
	char* currPtr;
	char* endPtr;

	SimpleHeap(int size = 1 * 1024 * 1024) {
		currPtr = startPtr = (char*)VirtualAlloc(NULL, size, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
		endPtr = (char*)startPtr + size;
	}

	void* alloc(int size) {
		if (currPtr + size >= endPtr) return NULL;
		void* retPtr = currPtr;
		currPtr += size;
		return retPtr;
	}
};