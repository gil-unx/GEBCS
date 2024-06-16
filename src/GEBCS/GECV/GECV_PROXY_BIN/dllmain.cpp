// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include <Windows.h>
#include <fstream>
#include <iostream>
#include <sstream>
#include <filesystem>
#include <string>
#include <vector>

using namespace std;

static string DataFolder = ".\\CE_DATA\\GE1\\GECV_BIN";


namespace fs = std::filesystem;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    
    if (ul_reason_for_call == DLL_PROCESS_ATTACH) {

        ofstream log;
        string line;
        log.open(".\\CE_DATA\\GECV_BIN.log", std::ios::trunc | std::ios::out);

        log << "Start!\n";


        if (fs::exists(".\\ge2rb.exe")) {

            DataFolder = ".\\CE_DATA\\GE2\\GECV_BIN";

            log << "God Eater 2!\n";
            

        }
        else {
            log << "God Eater 1!\n";
        }
        log.flush();

        log << "Get Data Bin From " << DataFolder << "\n";

        HANDLE process = OpenProcess(PROCESS_ALL_ACCESS, FALSE, GetCurrentProcessId());

        DWORD base_address = (DWORD)GetModuleHandle(NULL);

        log << "Base Address:" << std::hex << base_address << "\n";

        if (!fs::exists(DataFolder)) {
            fs::create_directory(DataFolder);
        }


        for (const auto& item : fs::recursive_directory_iterator(DataFolder)) {


            if (item.path().extension() == ".bin") {

                log << "Get Bin Data:" << item.path().generic_string() << '\n';

                fs::path file_hex_offset = item.path().stem();


                log << "Get Bin Name:" << file_hex_offset.u8string() << '\n';

                uint32_t int_addr = stoi(file_hex_offset.c_str(), 0, 16);

                log << "Get Bin int32:10:" << std::dec << int_addr << ",16:" << std::hex << int_addr << "\n";


                ifstream bin_file(item.path(), ios_base::in);

                if (!bin_file.is_open()) {

                    log << "Error To Open File!\n";
                    continue;
                }

                string line;
                getline(bin_file, line);

                istringstream iss(line);
                std::vector<std::uint8_t> tokens;
                std::string token;
                
                log << "Read:" << line;
                while (iss >> token) {
                    unsigned int byte;
                    std::stringstream ss;
                    ss << std::hex << token;
                    ss >> byte;
                    tokens.push_back(static_cast<uint8_t>(byte));
                }
                log << '\n';

                DWORD ret = 0;
                SIZE_T token_size = tokens.size();

                int_addr += base_address;

                log << "+Base Address:" << std::hex << int_addr << "\n";


                if (VirtualProtectEx(process, (LPVOID)int_addr, token_size, PAGE_EXECUTE_READWRITE, &ret)) {

                    log << "Protect Size:" << ret <<"\n";

                }

                if (WriteProcessMemory(process, (LPVOID)int_addr, tokens.data(), tokens.size(), &ret)) {
                
                    log << "Written Size:" << ret << "\n";
                }

                



                }
                




            }


        log.flush();
        log.close();

        }




    return true;


    

}

