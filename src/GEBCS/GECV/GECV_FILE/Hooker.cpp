#include <windows.h>

class Hooker {
private:
	BYTE OLD_hook_bytes[5];
	BYTE NEW_hook_bytes[5];
	HANDLE process;
	FARPROC origin;
	DWORD hook;
public:


	Hooker(HANDLE Process,FARPROC OriginAddress,DWORD HookAddress ) {

		process = Process;
		origin = OriginAddress;
		hook = HookAddress;

        DWORD memory_ret = 0;

        ReadProcessMemory(process, origin, OLD_hook_bytes, 5, &memory_ret);

        NEW_hook_bytes[0] = '\xE9';

        DWORD offset = hook - (DWORD)origin - 5;

        memcpy_s(&NEW_hook_bytes[1], 4, &offset, 4);

        VirtualProtect(origin, 5, PAGE_EXECUTE_READWRITE, &memory_ret);

	}

	void Install() {

		DWORD memory_ret = 0;
		WriteProcessMemory(process, origin, NEW_hook_bytes, 5, NULL);

	}

	void UnInstall() {

		DWORD memory_ret = 0;
		WriteProcessMemory(process, origin, OLD_hook_bytes, 5, NULL);

	}

};