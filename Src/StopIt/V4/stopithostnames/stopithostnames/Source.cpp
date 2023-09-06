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
#include <windows.h>
#include <windns.h>
#include <stdio.h>
#include <winsock.h>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>
#include <cstdio>

using namespace std;

#define BUFFER_LEN 255

#pragma comment(lib, "Ws2_32.lib")
#pragma comment(lib, "Dnsapi.lib")
#pragma comment(lib, "Msvcrt.lib")
#pragma comment(lib, "Wtsapi32.lib")
#pragma comment(lib, "winmm.lib")
#pragma comment(lib, "ntdll.lib")
extern "C"
{
    __declspec(dllexport) BSTR getHostNames(const wchar_t* IP)
    {
        std::wstring hostname;
        wstring your_wchar_in_ws(IP);
        string your_wchar_in_str(your_wchar_in_ws.begin(), your_wchar_in_ws.end());
        const char* pOwnerName = your_wchar_in_str.c_str();
        char pReversedIP[255];//Reversed IP address.
        DNS_STATUS status; //Return value of DnsQuery_A() function.
        PDNS_RECORD pDnsRecord; //Pointer to DNS_RECORD structure.
        PIP4_ARRAY pSrvList = NULL; //Pointer to IP4_ARRAY structure.
        WORD wType; //Type of the record to be queried.
        DNS_FREE_TYPE freetype;
        freetype = DnsFreeRecordListDeep;
        IN_ADDR ipaddr;

        //You must reverse the IP address to request a Reverse Lookup 
        //of a host name.
        sprintf(pReversedIP, "%s", pOwnerName);
        char* pIP = pReversedIP;
        char seps[] = ".";
        char* token;
        char pIPSec[4][4];
        int i = 0;
        token = strtok(pIP, seps);
        while (token != NULL)
        {
            /* While there are "." characters in "string"*/
            sprintf(pIPSec[i], "%s", token);
            /* Get next "." character: */
            token = strtok(NULL, seps);
            i++;
        }
        sprintf(pIP, "%s.%s.%s.%s.%s", pIPSec[3], pIPSec[2], pIPSec[1], pIPSec[0], "IN-ADDR.ARPA");
        pOwnerName = pReversedIP;
        wType = DNS_TYPE_PTR; //Query PTR records to resolve an IP address

        // Calling function DnsQuery to query Host or PTR records 
        status = DnsQuery(pOwnerName, //Pointer to OwnerName. 
            wType, //Type of the record to be queried.
            DNS_QUERY_BYPASS_CACHE, // Bypasses the resolver cache on the lookup. 
            pSrvList, //Contains DNS server IP address.
            &pDnsRecord, //Resource record that contains the response.
            NULL); //Reserved for future use.

        if (status)
        {
            hostname = L"error";
            LocalFree(pSrvList);
            return SysAllocString(hostname.c_str());
        }
        else
        {
            const char* str = pDnsRecord->Data.DNAME.pNameHost;
            wstring ws(str, str + strlen(str));
            hostname = ws;
            LocalFree(pSrvList);
            // Free memory allocated for DNS records. 
            DnsRecordListFree(pDnsRecord, freetype);
            return SysAllocString(hostname.c_str());
        }
    }
}