#include "stdafx.h"
#include "UsbSA44B.h"

#include "GlobalDisplayMessage.h"

void LogMsg(enumTypeMsg typeMsg, int nInstance, LPCTSTR lpszText, enumTypeMsgLevel typeMsgLevel /*= typeMsgLevelNormal*/)
{
   auto pApp = AfxGetApp();
   if (!pApp)
      return;

   auto pUsbSA44BApp = dynamic_cast<CUsbSA44BApp*>(pApp);

   if (pUsbSA44BApp)
	   pUsbSA44BApp->LogMsg(typeMsg, nInstance, lpszText, typeMsgLevel);
}


void TrimMsg(LPTSTR lpszText, size_t nLen)
{
   if (nLen == 0)
      return;

   // trim message by removing trailing CR, LF or period
   while (lpszText[nLen-1] == _T('\n') || lpszText[nLen-1] == _T('\r') || lpszText[nLen-1] == _T('.'))
			lpszText[--nLen] = _T('\0');
}