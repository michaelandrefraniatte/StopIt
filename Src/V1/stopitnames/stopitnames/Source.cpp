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
#pragma comment(lib, "Wtsapi32.lib")
#pragma comment(lib, "winmm.lib")
#pragma comment(lib, "ntdll.lib")
extern "C"
{
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