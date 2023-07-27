#include "pch.h"
#include <stdio.h>
#include <stdlib.h>
#include <malloc.h>
#include <windows.h>
#include <mmsystem.h>
#include <winioctl.h>
#include <iostream>
#include <Windows.h>
#include <WtsApi32.h>
#include <sddl.h>
#include <string>
#include <wtypes.h>
#include <oleauto.h>
#include <process.h>
#include <winbase.h>
#include <string.h>
#include <windows.h>
#include <tlhelp32.h>
#include <stdio.h>
#include <string>
#pragma comment(lib, "Wtsapi32.lib")
#pragma comment(lib, "winmm.lib")
#pragma comment(lib, "ntdll.lib")
extern "C"
{
    __declspec(dllexport) void killProcessByName(const wchar_t* filename)
    {
        HANDLE hSnapShot = CreateToolhelp32Snapshot(TH32CS_SNAPALL, NULL);
        if (!hSnapShot)
            return;
        PROCESSENTRY32W pEntry;
        pEntry.dwSize = sizeof(pEntry);
        BOOL hRes = Process32FirstW(hSnapShot, &pEntry);
        while (hRes)
        {
            if (wcscmp(pEntry.szExeFile, filename) == 0)
            {
                HANDLE hProcess = OpenProcess(PROCESS_TERMINATE, 0, pEntry.th32ProcessID);
                if (hProcess)
                {
                    TerminateProcess(hProcess, 9);
                    CloseHandle(hProcess);
                }
            }
            hRes = Process32NextW(hSnapShot, &pEntry);
        }
        CloseHandle(hSnapShot);
    }
}