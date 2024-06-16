// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include <Windows.h>
#include <fstream>
static DWORD game_baseaddress;
static DWORD dll_baseaddress;
static HMODULE dllModule;
static HANDLE process;
static std::ofstream logg;

static DWORD FixNpcAction1_Jmp;
static DWORD FixNpcAction1_Ret;

static DWORD FixNpcAction2_Jmp;
static DWORD FixNpcAction2_Ret;

__declspec(naked) void FIX_NPC_ACTION_1(void)
{
    __asm {
        movzx eax, byte ptr[edi]
        add eax, eax
        add esp,0x08
        add bl,-01
        jmp FixNpcAction1_Ret
    }
}

__declspec(naked) void FIX_NPC_ACTION_2(void)
{
    __asm {
        movzx eax, byte ptr[edi]
        add eax, eax
        add esp, 0x08
        add bl, -01
        jmp FixNpcAction2_Ret
    }
}


// GOD EATER 2

void Install(DWORD OriginAddress, DWORD FunctionAddress) {

    DWORD ret;
    BYTE hook_bytes[6];
    VirtualProtect(reinterpret_cast<LPVOID>(OriginAddress), 6, PAGE_EXECUTE_READWRITE, &ret);


    hook_bytes[0] = 0xE9;


    logg << "Game Function Address:" << std::hex << OriginAddress << "\n";

    logg << "Dll Function Address:" << std::hex << FunctionAddress << "\n";

    DWORD jmp_offset = FunctionAddress - (OriginAddress + 5);

    logg << "Jump Address:" << std::hex << jmp_offset << "\n";

    *(DWORD*)(hook_bytes + 1) = jmp_offset;

    hook_bytes[5] = 0x90;

    WriteProcessMemory(process, reinterpret_cast<LPVOID>(OriginAddress), hook_bytes, 6, &ret);

    logg.flush();
}

BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{

    if (ul_reason_for_call == DLL_PROCESS_ATTACH) {


        logg.open(".\\CE_DATA\\game_fix.log", std::ios::trunc | std::ios::out);


        process = OpenProcess(PROCESS_ALL_ACCESS, FALSE, GetCurrentProcessId());

        logg << "Process:" << std::hex << GetCurrentProcessId() << "\n";

        game_baseaddress = (DWORD)GetModuleHandle(NULL);
        logg << "Game Base Address:" << std::hex << game_baseaddress << "\n";
        dll_baseaddress = (DWORD)hModule;
        logg << "Dll Base Address:" << std::hex << game_baseaddress << "\n";
        dllModule = hModule;





    }

    return TRUE;
}


