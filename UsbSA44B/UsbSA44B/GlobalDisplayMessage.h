// GlobalDisplayMessage.h   
#pragma once
#include <afxstr.h>

enum enumTypeMsg {
   typeMsgSaEmulatorThread,
   typeMsgSaEmulatorListenThread,
   typeMsgSaEmulatorClientThread,
   typeMsgSaEmulatorParser,
   typeMsgSaEmulatorSocket,
   typeMsgSaEmulatorSocketInput,
   typeMsgSaEmulatorSocketOutput,
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