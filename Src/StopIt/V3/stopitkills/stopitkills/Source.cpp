#include "pch.h"
#include <stdlib.h>
#include <malloc.h>
#include <winioctl.h>
#include <iostream>
#include <Windows.h>
#include <WtsApi32.h>
#include <sddl.h>
#include <wtypes.h>
#include <oleauto.h>
#include <process.h>
#include <winbase.h>
#include <string.h>
#include <tlhelp32.h>
#include <locale.h>
#include <wchar.h>
#include <stdio.h>
#include <unordered_map>
#include <psapi.h>
#include "call.h"
#pragma comment(lib, "Msvcrt.lib")
#pragma comment(lib, "Wtsapi32.lib")
#pragma comment(lib, "winmm.lib")
#pragma comment(lib, "ntdll.lib")
extern "C"
{
    __declspec(dllexport) BSTR killProcessByNames(const wchar_t* filenames)
    {
        std::wstring names;
        WTS_PROCESS_INFO* pWPIs = NULL;
        DWORD dwProcCount = 0;
        if (WTSEnumerateProcesses(WTS_CURRENT_SERVER_HANDLE, NULL, 1, &pWPIs, &dwProcCount))
        {
            for (DWORD i = 0; i < dwProcCount; i++)
            {
                const wchar_t* name = pWPIs[i].pProcessName;
                if (wcsstr(filenames, name) == 0)
                {
                    HANDLE hProcess = OpenProcess(PROCESS_TERMINATE, 0, pWPIs[i].ProcessId);
                    if (hProcess)
                    {
                        TerminateProcess(hProcess, 9);
                        CloseHandle(hProcess);
                        names += std::wstring(name) + L",";
                    }
                }
            }
        }
        if (pWPIs)
        {
            WTSFreeMemory(pWPIs);
            pWPIs = NULL;
        }
        return SysAllocString(names.c_str());
    }
    __declspec(dllexport) BSTR processnames()
    {
        std::wstring names;
        WTS_PROCESS_INFO* pWPIs = NULL;
        DWORD dwProcCount = 0;
        if (WTSEnumerateProcesses(WTS_CURRENT_SERVER_HANDLE, NULL, 1, &pWPIs, &dwProcCount))
        {
            for (DWORD i = 0; i < dwProcCount; i++)
            {
                names += std::wstring(pWPIs[i].pProcessName) + L",";
            }
        }
        if (pWPIs)
        {
            WTSFreeMemory(pWPIs);
            pWPIs = NULL;
        }
        return SysAllocString(names.c_str());
    }
    __declspec(dllexport) BSTR get_cpu_usage(int pid)
    {
        static int processor_count_ = -1;
        static std::unordered_map<int, __int64> last_time_;
        static std::unordered_map<int, __int64> last_system_time_;
        FILETIME now;
        FILETIME creation_time;
        FILETIME exit_time;
        FILETIME kernel_time;
        FILETIME user_time;
        __int64 system_time;
        __int64 time;
        __int64 system_time_delta;
        __int64 time_delta;
        int cpu = -1;
        if (processor_count_ == -1)
        {
            SYSTEM_INFO info;
            GetSystemInfo(&info);
            processor_count_ = (int)info.dwNumberOfProcessors;
        }
        GetSystemTimeAsFileTime(&now);
        HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, pid);
        if (!GetProcessTimes(hProcess, &creation_time, &exit_time, &kernel_time, &user_time))
        {
            std::cout << "Unable to getProcessTime\n";
            return 0;
        }
        system_time = (file_time_2_utc(&kernel_time) + file_time_2_utc(&user_time)) / processor_count_;
        time = file_time_2_utc(&now);
        if ((last_system_time_[pid] == 0) || (last_time_[pid] == 0))
        {
            last_system_time_[pid] = system_time;
            last_time_[pid] = time;
            return 0;
        }
        system_time_delta = system_time - last_system_time_[pid];
        time_delta = time - last_time_[pid];
        if (time_delta == 0)
        {
            std::cout << "timedelta=0";
            return 0;
        }
        cpu = int((system_time_delta * 100 + time_delta / 2) / time_delta);
        last_system_time_[pid] = system_time;
        last_time_[pid] = time;
        wchar_t buf[20];
        int len = swprintf(buf, 20, L"%d", cpu);
        return SysAllocStringLen(buf, len);
    }
    __declspec(dllexport) BSTR get_memory_usage(int pid)
    {
        HANDLE hProcess;
        PROCESS_MEMORY_COUNTERS pmc;
        int memory = 0;
        hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, pid);
        if (hProcess == NULL)
            return 0;
        if (GetProcessMemoryInfo(hProcess, &pmc, sizeof(pmc)))
        {
            memory = pmc.PagefileUsage;
        }
        CloseHandle(hProcess);
        wchar_t buf[20];
        int len = swprintf(buf, 20, L"%d", memory);
        return SysAllocStringLen(buf, len);
    }
}