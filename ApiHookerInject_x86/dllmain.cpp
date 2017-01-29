#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#include <ApiHooker.h>

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	//if (ul_reason_for_call == DLL_PROCESS_ATTACH)
	//	CreateThread(NULL, 0, &ApiHooker::ThreadFuncMsgBox, NULL, 0, NULL);

	return TRUE;
}

extern "C" __declspec(dllexport) void Init(ApiHooker::InitParams* params) {
	ApiHooker::ThreadFuncMsgBox(params);
}