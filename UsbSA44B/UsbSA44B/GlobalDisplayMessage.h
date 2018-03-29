// GlobalDisplayMessage.h   
#pragma once
#include <afxstr.h>

enum enumTypeMsg {
   typeMsgScpiEmulatorThread,
   typeMsgScpiEmulatorListenThread,
   typeMsgScpiEmulatorClientThread,
   typeMsgScpiEmulatorParser,
   typeMsgScpiEmulatorSocket,
   typeMsgScpiEmulatorSocketInput,
   typeMsgScpiEmulatorSocketOutput,
   typeMsgNone
} ;

enum enumTypeMsgLevel {
   typeMsgLevelNormal,
   typeMsgLevelInformation,
   typeMsgLevelWarning,
   typeMsgLevelError,
   typeMsgLevelCriticalError,
   typeMsgLevelNone
} ;


class CAppendToLogParams
{
public:
   CString strTime;
   CString strMessage;
   enumTypeMsgLevel typeMsgLevel;
};


void LogMsg(enumTypeMsg typeMsg, int nInstance, LPCTSTR lpszText, enumTypeMsgLevel typeMsgLevel = typeMsgLevelNormal);
void TrimMsg(LPTSTR lpszText, size_t nLen);