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
#pragma comment(lib, "Msvcrt.lib")
#pragma comment(lib, "Wtsapi32.lib")
#pragma comment(lib, "winmm.lib")
#pragma comment(lib, "ntdll.lib")

__int64 file_time_2_utc(const FILETIME* ftime)
{
    LARGE_INTEGER li;
    li.LowPart = ftime->dwLowDateTime;
    li.HighPart = ftime->dwHighDateTime;
    return li.QuadPart;
}