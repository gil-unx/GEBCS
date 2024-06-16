// dllmain.cpp : 定义 DLL 应用程序的入口点。
//#include "pch.h"
#include <Windows.h>
#include <fstream>
#include <iostream>
#include <filesystem>
#include <string>

namespace fs = std::filesystem;

static std::string titlebin = ".\\CE_DATA\\GE1\\GECV_TITLE.bin";

BOOL
WINAPI
Hook_SetWindowTextA(
    _In_ HWND hWnd,
    _In_opt_ LPCSTR lpString);

DWORD WINAPI HookThread(LPVOID lpReserved);

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
  

    if (ul_reason_for_call == DLL_PROCESS_ATTACH) {

        std::ofstream log;
        log.open(".\\CE_DATA\\GECV_TITLE.log", std::ios::trunc | std::ios::out);


        if (fs::exists(".\\ge2rb.exe")) {

            log << "God Eater 2!\n";

            titlebin = ".\\CE_DATA\\GE2\\GECV_TITLE.bin";


        }
        else {
            log << "God Eater 1!\n";
        }
        log.flush();


        BYTE OLD_hook_bytes[5];
        BYTE NEW_hook_bytes[5];
        FARPROC title_hook = GetProcAddress(GetModuleHandleA("user32"), "SetWindowTextA");

        HANDLE process = OpenProcess(PROCESS_ALL_ACCESS, FALSE, GetCurrentProcessId());

        
        DisableThreadLibraryCalls(hModule);
        


        //SetWindowTextW(GetConsoleWindow(),L"GECV");

        //CreateThread(nullptr, 0, HookThread, hModule, 0, nullptr);

        if (title_hook == NULL) {

            log << "Faild Get Hook Address!\n";


        }
        else {
        log << "Success Get Hook Address!\n";
        DWORD memory_ret = 0;
        ReadProcessMemory(process, title_hook, OLD_hook_bytes, 5, &memory_ret);
        log << "Read Hook Result:" << memory_ret << "\n";
        NEW_hook_bytes[0] = '\xE9';

        DWORD offset = (DWORD)Hook_SetWindowTextA - (DWORD)title_hook - 5;

        memcpy_s(&NEW_hook_bytes[1],4, &offset, 4);

        VirtualProtect(title_hook, 5, PAGE_EXECUTE_READWRITE, &memory_ret);
        log << "Protect Hook Result:" << memory_ret << "\n";
        WriteProcessMemory(process, title_hook, NEW_hook_bytes, 5, &memory_ret);
        log << "Write Hook Result:" << memory_ret << "\n";


            

        }
        

        

    }

    return true;

}



BOOL WINAPI Hook_SetWindowTextA(
    _In_ HWND hWnd,
    _In_opt_ LPCSTR lpString
) {

    std::ofstream log;
    log.open(".\\CE_DATA\\GECV_TITLE.log", std::ios::app | std::ios::out);
    log << "Start Hook Function." << "\n";
    std::string title;

    std::ifstream file(titlebin);

    

    wchar_t* wide_string;

    if (file.is_open()) {
        
        log << "Get GECV_TITLE.bin" << "\n";
        std::getline(file, title);
        log << "Set New Title:" << title << "\n";
        int size_need = MultiByteToWideChar(CP_UTF8, 0, title.c_str(), -1, NULL, 0);
        wide_string = new wchar_t[size_need];
        MultiByteToWideChar(CP_UTF8, 0, title.c_str(), -1, wide_string, size_need);
    }
    else {
        log << "Not Get GECV_TITLE.bin" << "\n";
        log << "Set Origin Title:" <<lpString << "\n";
        return SetWindowTextW(hWnd, L"GECV");
    }


    return SetWindowTextW(hWnd, (LPCWSTR)wide_string);
}