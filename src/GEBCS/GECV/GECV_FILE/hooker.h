#include <Windows.h>
#include <fstream>


class Hooker {
private:
	BYTE OLD_hook_bytes[5];
	BYTE NEW_hook_bytes[5];
	HANDLE process;
	FARPROC origin;
	DWORD hook;


	void Bind() {

		DWORD memory_ret = 0;

		ReadProcessMemory(process, origin, OLD_hook_bytes, 5, &memory_ret);

		//log << "Read:" << memory_ret << "\n";

		NEW_hook_bytes[0] = '\xE9';

		DWORD offset = hook - (DWORD)origin - 5;

		memcpy_s(&NEW_hook_bytes[1], 4, &offset, 4);

		VirtualProtect(origin, 5, PAGE_EXECUTE_READWRITE, &memory_ret);
	}

public:

	Hooker() {

	}

	void Create(HANDLE Process, LPCSTR library, LPCSTR function, DWORD HookAddress) {

		process = Process;
		
		origin = GetProcAddress(GetModuleHandleA(library), function);

		hook = HookAddress;

		Bind();

	}

	void Create(HANDLE Process, FARPROC OriginAddress, DWORD HookAddress) {

		//std::ofstream log;

		//log.open(".\\CE_DATA\\GECV_FILE.log", std::ios::app | std::ios::out);

		process = Process;
		origin = OriginAddress;
		hook = HookAddress;

		Bind();

		//log << "Protect:" << memory_ret << "\n";
		//log.close();
	}

	DWORD Install() {
		//std::ofstream log;
		//log.open(".\\CE_DATA\\GECV_FILE.log", std::ios::app | std::ios::out);
		DWORD memory_ret = 0;
		WriteProcessMemory(process, origin, NEW_hook_bytes, 5,&memory_ret);
		//log << "Install:" << memory_ret << "\n";
		//log.close();
		return memory_ret;
	}

	DWORD UnInstall() {
		std::ofstream log;
		//log.open(".\\CE_DATA\\GECV_FILE.log", std::ios::app | std::ios::out);
		//log << "Process:" << process << "\n";
		//log << "WriteAddress:" << origin << "\n";
		DWORD memory_ret = 0;
		WriteProcessMemory(process, origin, OLD_hook_bytes, 5, &memory_ret);
		//log << "UnInstall:" << memory_ret << "\n";
		//log.close();
		return memory_ret;

	}

	VOID Set(BOOL status) {

		if (status) {
			Install();
		}
		else {
			UnInstall();
		}


	}

	

};