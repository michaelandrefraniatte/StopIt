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
                if (wcsstr(filenames, name) != 0)
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
}