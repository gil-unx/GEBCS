// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include <Windows.h>
#include <fstream>
#include <iostream>
#include <filesystem>
#include <string>
#include <map>
#include <Shlwapi.h>

#include "hooker.h"

#pragma comment(lib, "Shlwapi.lib")

namespace fs = std::filesystem;

static std::ofstream logg;
static HANDLE process;

static Hooker Hooker_CreateFileW;
static Hooker Hooker_CreateFileA;

static Hooker Hooker_PathFileExistsA;
static Hooker Hooker_PathFileExistsW;

static Hooker Hooker_WriteFile;

static Hooker Hooker_OutputDebugStringA;

static Hooker HK0printf, HK0_vsnprintf_s, HK0_snprintf, HK0_sprintf_s, HK0_vsnprintf, HK0_swprintf_s, HK0_vscprintf, HK0sprintf, HK0vsprintf, HK0vsprintf_s, HK0wsprintfW;


static std::map<HANDLE,LPCWSTR> WFileMap;
static long unknown_filename_count = 0;

#pragma warning(disable : 4996)

void SetPrintfStatus(BOOL status) {
   
        HK0printf.Set(status);
        //HK0sprintf.Set(status);
        HK0vsprintf.Set(status);
        HK0vsprintf_s.Set(status);
        HK0wsprintfW.Set(status);
        HK0_snprintf.Set(status);
        HK0_sprintf_s.Set(status);
        HK0_swprintf_s.Set(status);
        HK0_vscprintf.Set(status);
        HK0_vsnprintf.Set(status);
        HK0_vsnprintf_s.Set(status);
    

}

int Hprintf(const char* const Format, ...) {
    SetPrintfStatus(false);
    va_list args, args_copy;
    va_start(args, Format);
    va_copy(args_copy, args);
    char Buffer[4096];
    int result = vsprintf(Buffer, Format, args_copy);
    logg << "printf:" << Buffer << "\n";
    logg.flush();


    SetPrintfStatus(true);
    return result;
};

int __cdecl Hvsnprintf_s(char* const Buffer, const size_t BufferCount, const size_t MaxCount, const char* const Format, va_list ArgList) {
    SetPrintfStatus(false);


    int ret = vsnprintf_s(Buffer, BufferCount,MaxCount, Format, ArgList);
    logg << "vsnprintf_s:" << Buffer << "\n";
    logg.flush();


    SetPrintfStatus(true);
    return ret;
};

int Hsnprintf(char* const Buffer, const size_t BufferCount, const char* const Format, ...) {
    va_list args, args_copy;
    va_start(args, Format);
    va_copy(args_copy, args);

    int ret = snprintf(Buffer, BufferCount, Format, args_copy);
    logg << "snprintf:" << Buffer << "\n";
    logg.flush();
    va_end(args_copy);

    SetPrintfStatus(true);
    return ret;
};

int Hsprintf_s(char* const Buffer, const size_t BufferCount, const char* const Format, ...) {
    SetPrintfStatus(false);

    va_list args, args_copy;
    va_start(args, Format);
    va_copy(args_copy, args);

    int ret = sprintf_s(Buffer, BufferCount, Format, args_copy);
    logg << "sprintf_s:" << Buffer << "\n";
    logg.flush();
    va_end(args_copy);

    SetPrintfStatus(true);
    return ret;
};

int __cdecl Hvsnprintf(char* const Buffer, const size_t BufferCount, const char* const Format, va_list ArgList) {
    SetPrintfStatus(false);


    int ret = vsnprintf(Buffer, BufferCount, Format, ArgList);
    logg << "vsnprintf:" << Buffer << "\n";
    logg.flush();


    SetPrintfStatus(true);
    return ret;
};

int Hswprintf_s(wchar_t* const Buffer, const size_t BufferCount, const wchar_t* const Format, ...) {
    SetPrintfStatus(false);
    
    va_list args, args_copy;
    va_start(args, Format);
    va_copy(args_copy, args);

    int ret = swprintf_s(Buffer, BufferCount, Format, args_copy);
    logg << "swprintf_s:" << Buffer << "\n";
    logg.flush();
    va_end(args_copy);

    SetPrintfStatus(true);
    return ret;
};

int __cdecl Hvscprintf(const char* const Format, va_list ArgList) {
    SetPrintfStatus(false);

    int ret = _vscprintf(Format, ArgList);
    logg << "vscprintf:" << ret << "\n";
    logg.flush();


    SetPrintfStatus(true);
    return ret;
};

int Hsprintf(char* const Buffer, const char* const Format, ...) {
    SetPrintfStatus(false);
    va_list args, args_copy;
    va_start(args, Format);
    va_copy(args_copy, args);
    int result = sprintf(Buffer, Format, args_copy);
    va_end(args_copy);
    logg << "sprintf:" << Buffer;
    logg.flush();
    SetPrintfStatus(true);
    return result;
};

int __cdecl Hvsprintf(char* const Buffer, const char* const Format, va_list ArgList) {
    SetPrintfStatus(false);

    int ret = sprintf(Buffer,  Format, ArgList);
    logg << "sprintf:" << Buffer << "\n";
    logg.flush();
    SetPrintfStatus(true);
    return ret;
};

int __cdecl Hvsprintf_s(char* const Buffer, const size_t BufferCount, const char* const Format, va_list ArgList) {
    SetPrintfStatus(false);


    int ret = sprintf_s(Buffer,BufferCount,Format,ArgList);
    logg << "sprintf_s:" << Buffer << "\n";
    logg.flush();
    SetPrintfStatus(true);
    return ret;
};

int HwsprintfW(LPWSTR unamedParam1, LPCWSTR unamedParam2, ...) {
    SetPrintfStatus(false);
    va_list args, args_copy;
    va_start(args, unamedParam2);
    va_copy(args_copy, args);
    int result = wsprintfW(unamedParam1, unamedParam2, va_arg(args_copy, char*));
    va_end(args_copy);
    logg << "wsprintfW:" << unamedParam1 << "\n";
    logg.flush();
    SetPrintfStatus(true);
    return result;

};

HANDLE
WINAPI
Hook_CreateFileA(
    _In_ LPCSTR lpFileName,
    _In_ DWORD dwDesiredAccess,
    _In_ DWORD dwShareMode,
    _In_opt_ LPSECURITY_ATTRIBUTES lpSecurityAttributes,
    _In_ DWORD dwCreationDisposition,
    _In_ DWORD dwFlagsAndAttributes,
    _In_opt_ HANDLE hTemplateFile
);

HANDLE
WINAPI
Hook_CreateFileW(
    _In_ LPCWSTR lpFileName,
    _In_ DWORD dwDesiredAccess,
    _In_ DWORD dwShareMode,
    _In_opt_ LPSECURITY_ATTRIBUTES lpSecurityAttributes,
    _In_ DWORD dwCreationDisposition,
    _In_ DWORD dwFlagsAndAttributes,
    _In_opt_ HANDLE hTemplateFile
);

VOID
WINAPI
Hook_OutputDebugStringA(
    _In_opt_ LPCSTR lpOutputString
);

BOOL
WINAPI
Hook_WriteFile(
    _In_ HANDLE hFile,
    _In_reads_bytes_opt_(nNumberOfBytesToWrite) LPCVOID lpBuffer,
    _In_ DWORD nNumberOfBytesToWrite,
    _Out_opt_ LPDWORD lpNumberOfBytesWritten,
    _Inout_opt_ LPOVERLAPPED lpOverlapped
) {
    Hooker_WriteFile.UnInstall();
    Hooker_CreateFileW.UnInstall();
    std::wofstream log;
    std::ofstream unk_set;
    log.open(".\\CE_DATA\\GECV_FILE.log", std::ios::app | std::ios::out);
    unk_set.open(".\\CE_DATA\\unk_set.bin", std::ios::app | std::ios::binary);
    log << "Pass Check Game Version.\n";
    log << "WriteFile Handle:" << hFile << "\n";
    log.flush();

    bool IsExists = false;
    std::wstring savepath(L".\\CE_DATA\\GECV_FILE\\");
    for (const auto& pair : WFileMap) {

        if (pair.first == hFile) {

            IsExists = true;
            log << "Get FileName By Handle:" << pair.second << "\n";
            savepath.append(fs::path(pair.second).filename());
            break;

        }


    }

    if (!IsExists) {
        log << "Warning:Handle " << hFile << " Is New Handle.\n";
        savepath.append(L"\\UNK\\unknown_");
        savepath.append(std::to_wstring(unknown_filename_count));
        unknown_filename_count++;
        log << "Set Save File Name:" << savepath.c_str() << "\n";

        unk_set.write(reinterpret_cast<const char*>(lpBuffer), nNumberOfBytesToWrite);
        unk_set.flush();
    }


    log << "WriteFile Save Path:" << savepath.c_str() << "\n";
    HANDLE system_write_file = CreateFileW(savepath.c_str(), GENERIC_ALL, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    WriteFile(system_write_file, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped);

    


    log.flush();
    log.close();

    auto result = WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped);


    Hooker_WriteFile.Install();
    Hooker_CreateFileW.Install();
    return result;

}

BOOL Hook_PathFileExistsA(_In_ LPCSTR pszPath);
BOOL Hook_PathFileExistsW(_In_ LPCWSTR pszPath);

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{


    //MessageBox(0, L"TEST",L"TEST",0);

    if (ul_reason_for_call == DLL_PROCESS_ATTACH) {
        //MessageBox(0, L"TEST2", L"TEST2", 0);
        

        std::ofstream log;
        

        //fs::remove(".\\CE_DATA\\GECV_FILE");
        log.open(".\\CE_DATA\\GECV_FILE_FIRST.log", std::ios::trunc | std::ios::out);
        try{
            log << "GAME DEBUG FIX ON!\n";
            fs::create_directory(".\\CE_DATA\\GECV_FILE");
        fs::create_directory(".\\CE_DATA\\GECV_FILE\\UNK");
        }
        catch (const fs::filesystem_error& e) {
            
            log << e.what() << "\n";
            

            log.close();
        }
        //MessageBox(0, L"TEST3", L"TEST3", 0);
        

        logg.open(".\\CE_DATA\\GAME_DEBUG.log", std::ios::trunc | std::ios::out);
        log.open(".\\CE_DATA\\GECV_FILE.log", std::ios::trunc | std::ios::out);
        log.close();


        //MessageBox(0, L"TEST4", L"TEST4", 0);
        DisableThreadLibraryCalls(hModule);

        FARPROC File_Hook_A = GetProcAddress(GetModuleHandleA("kernel32"), "CreateFileA");
        FARPROC File_Hook_W = GetProcAddress(GetModuleHandleA("kernel32"), "CreateFileW");

        FARPROC Path_Hook_A = GetProcAddress(GetModuleHandleA("shlwapi"), "PathFileExistsA");
        FARPROC Path_Hook_W = GetProcAddress(GetModuleHandleA("shlwapi"), "PathFileExistsW");

        FARPROC Output_Debug_A = GetProcAddress(GetModuleHandleA("kernel32"), "OutputDebugStringA");

        

        process = OpenProcess(PROCESS_ALL_ACCESS, FALSE, GetCurrentProcessId());


        Hooker_CreateFileA.Create(process, File_Hook_A, (DWORD)Hook_CreateFileA);
        Hooker_CreateFileW.Create(process, File_Hook_W, (DWORD)Hook_CreateFileW);

        Hooker_PathFileExistsA.Create(process, Path_Hook_A, (DWORD)Hook_PathFileExistsA);
        Hooker_PathFileExistsW.Create(process, Path_Hook_W, (DWORD)Hook_PathFileExistsW);

        Hooker_OutputDebugStringA.Create(process, Output_Debug_A,(DWORD)Hook_OutputDebugStringA);


        HK0printf.Create(process, "msvcr120", "printf",(DWORD)Hprintf);
        HK0sprintf.Create(process, "msvcr120", "sprintf", (DWORD)Hsprintf);
        HK0vsprintf.Create(process, "msvcr120", "vsprintf", (DWORD)Hvsprintf);
        HK0vsprintf_s.Create(process, "msvcr120", "vsprintf_s", (DWORD)Hvsprintf_s);
        HK0wsprintfW.Create(process, "user32", "wsprintfW", (DWORD)HwsprintfW);
        HK0_snprintf.Create(process, "msvcr120", "_snprintf", (DWORD)Hsnprintf);
        HK0_sprintf_s.Create(process, "msvcr120", "_sprintf_s", (DWORD)Hsprintf_s);
        HK0_swprintf_s.Create(process, "msvcr120", "_swprintf_s", (DWORD)Hswprintf_s);
        HK0_vscprintf.Create(process, "msvcr120", "_vsprintf", (DWORD)Hvsprintf);
        HK0_vsnprintf.Create(process, "msvcr120", "_vsnprintf", (DWORD)Hvsnprintf);
        HK0_vsnprintf_s.Create(process, "msvcr120", "_vsnprintf_s", (DWORD)Hvsnprintf_s);

        Hooker_WriteFile.Create(process, "kernel32", "WriteFile", (DWORD)Hook_WriteFile);


        log << "Created Hook.\n";

        Hooker_CreateFileA.Install();
        Hooker_CreateFileW.Install();
        Hooker_PathFileExistsA.Install();
        Hooker_PathFileExistsW.Install();
        Hooker_OutputDebugStringA.Install();
        
        Hooker_WriteFile.Install();

        log << "Install Hook.\n";
        
        //Hooker_CreateFileA.UnInstall();
        //Hooker_CreateFileW.UnInstall();

        //log << "UnInstall Hook.\n";

       SetPrintfStatus(true);

       

    }

    return true;
}



HANDLE
WINAPI
Hook_CreateFileA(
    _In_ LPCSTR lpFileName,
    _In_ DWORD dwDesiredAccess,
    _In_ DWORD dwShareMode,
    _In_opt_ LPSECURITY_ATTRIBUTES lpSecurityAttributes,
    _In_ DWORD dwCreationDisposition,
    _In_ DWORD dwFlagsAndAttributes,
    _In_opt_ HANDLE hTemplateFile
) {
    
    std::ofstream log;
    log.open(".\\CE_DATA\\GECV_FILE.log", std::ios::app | std::ios::out);

    

    Hooker_CreateFileA.UnInstall();
    HANDLE result =  CreateFileA(lpFileName,dwDesiredAccess,dwShareMode,lpSecurityAttributes,dwCreationDisposition,dwFlagsAndAttributes,hTemplateFile);
    Hooker_CreateFileA.Install();

    log << "CreateFileA:" << lpFileName << "\n";

    log.flush();
    log.close();

    return result;
}

HANDLE
WINAPI
Hook_CreateFileW(
    _In_ LPCWSTR lpFileName,
    _In_ DWORD dwDesiredAccess,
    _In_ DWORD dwShareMode,
    _In_opt_ LPSECURITY_ATTRIBUTES lpSecurityAttributes,
    _In_ DWORD dwCreationDisposition,
    _In_ DWORD dwFlagsAndAttributes,
    _In_opt_ HANDLE hTemplateFile
) {
    

    std::wofstream log;
    log.open(".\\CE_DATA\\GECV_FILE.log", std::ios::app | std::ios::out);

    

    Hooker_CreateFileW.UnInstall();
    
    
    HANDLE result = CreateFileW(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);


    

    

    log << "CreateFileW:" << lpFileName << ",Access:" << dwDesiredAccess <<",ShareMode:" << dwShareMode  << "\n";
    WFileMap[result] = lpFileName;
    log << "CreateFileW:" << result << "," << lpFileName << "\n";
    Hooker_CreateFileW.Install();

    log.flush();
    log.close();

    return result;
}

BOOL Hook_PathFileExistsA(_In_ LPCSTR pszPath) {

    std::ofstream log;
    log.open(".\\CE_DATA\\GECV_FILE.log", std::ios::app | std::ios::out);

    log << "PathFileExistsA:" << pszPath << "\n";
    log.flush();
    log.close();

    Hooker_PathFileExistsA.UnInstall();

    BOOL result = PathFileExistsA(pszPath);
    Hooker_PathFileExistsA.Install();
    return result;
}
BOOL Hook_PathFileExistsW(_In_ LPCWSTR pszPath) {
    std::wofstream log;
    log.open(".\\CE_DATA\\GECV_FILE.log", std::ios::app | std::ios::out);

    log << "PathFileExistsW:" << pszPath << "\n";
    log.flush();
    log.close();

    Hooker_PathFileExistsW.UnInstall();
    BOOL result = PathFileExistsW(pszPath);
    Hooker_PathFileExistsW.Install();
    return result;

}

VOID
WINAPI
Hook_OutputDebugStringA(
    _In_opt_ LPCSTR lpOutputString
){

    

    logg << "OutputDebugStringA:" << lpOutputString;
    logg.flush();
    


    Hooker_OutputDebugStringA.UnInstall();

    OutputDebugStringA(lpOutputString);

    Hooker_OutputDebugStringA.Install();

    return ;
    


}