// dllmain.cpp : 定义 DLL 应用程序的入口点。
//#include "pch.h"
#include <windows.h>
#include <fstream>
#include <iostream>
#include <filesystem>
#include <string>

namespace fs = std::filesystem;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    std::ofstream log;
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        log.open(".\\CE_DATA\\GECV_HELLO.log", std::ios::trunc | std::ios::out);
        log << "Hello Game For Process!\n";

        log << "Pass Check Game Version.\n";

        log.flush();

        if (fs::exists(".\\CE_DATA\\GECV_HELLO.bin")) {

            std::ifstream str_bin(L".\\CE_DATA\\GECV_HELLO.bin", std::ios::in);

            std::string message;

            std::getline(str_bin, message);

            std::string br = "<br>";
            std::string replace_br = "\n";

            log << "Origin Welcome:" << message << "\n";

            size_t pos = 0;

            while ((pos = message.find(br, pos)) != std::string::npos) {

                message.replace(pos, br.length(), replace_br);

            }
            log << "Parse Welcome:" << message << "\n";
            int size_needed = MultiByteToWideChar(CP_UTF8, 0, message.c_str(), int(message.size()), NULL, 0);
            std::wstring wstr(size_needed, 0);
            MultiByteToWideChar(CP_UTF8, 0, message.c_str(), int(message.size()), &wstr[0], size_needed);

            

            if (MessageBox(0, wstr.c_str(), L"GECV:Hello!", MB_OKCANCEL) == IDCANCEL){
            
                
            
            }

            str_bin.close();
            fs::remove(".\\CE_DATA\\GECV_HELLO.bin");

        }
        else {
            log << "No Welcome!\n";
        }


        break;
    case DLL_THREAD_ATTACH:
        log.open(".\\CE_DATA\\GECV_HELLO.log", std::ios::app | std::ios::out);
        log << "Hello Game For Thread!\n";
        break;
    case DLL_THREAD_DETACH:
        log.open(".\\CE_DATA\\GECV_HELLO.log", std::ios::app | std::ios::out);
        log << "Bye Game For Thread!\n";
        break;
    case DLL_PROCESS_DETACH:
        log.open(".\\CE_DATA\\GECV_HELLO.log", std::ios::app | std::ios::out);
        log << "Bye Game For Process!\n";
        break;
    }

    log.close();



    return TRUE;
}

