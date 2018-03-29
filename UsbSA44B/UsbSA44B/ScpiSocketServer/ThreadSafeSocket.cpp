// ThreadSafeSocket.cpp : implementation file
//

#include "stdafx.h"
#include "AVLZollner.h"
#include "ThreadSafeSocket.h"

CCriticalSection CThreadSafeSocket::m_CriticalSection;

// CThreadSafeSocket

CThreadSafeSocket::CThreadSafeSocket(enumTypeMsg typeMsg) :
m_typeMsg(typeMsg)
{
}

CThreadSafeSocket::~CThreadSafeSocket()
{
}


// CThreadSafeSocket member functions
BOOL CThreadSafeSocket::Create(UINT nSocketPort /*= 0*/, int nSocketType /*=SOCK_STREAM*/, LPCTSTR lpszSocketAddress /*= NULL*/)
{
   CSingleLock sl(&m_CriticalSection);

   BOOL bReturn = FALSE;

   TCHAR szExceptionMsg[256];
   szExceptionMsg[0]=NULL;
   bool bCreateFailed = false;
   try
   {
      sl.Lock();
      bReturn = CSocket::Create(nSocketPort, nSocketType, lpszSocketAddress);
      sl.Unlock();
      bCreateFailed = bReturn==0; // if return is zero, we had an issue!
   }
   catch(CException* e)
   {
      sl.Unlock();
      e->GetErrorMessage(szExceptionMsg,sizeof(szExceptionMsg));
      e->Delete();
      bCreateFailed = true;
   }
   catch(...)
   {
      sl.Unlock();
      bCreateFailed = true;
   }

   if (bCreateFailed)
   {
      CString strMsg;
   	strMsg.Format(_T("Unable to create socket on port %d."),nSocketPort);

      LogSocketError(strMsg,szExceptionMsg);
   }

   return bReturn;
}

void CThreadSafeSocket::LogSocketError(LPCTSTR szMsg, LPTSTR szExceptionMsg)
{
   auto dwLastError = GetLastError(); // get before we close

   LPTSTR szSystemMessage = nullptr;
   auto nLen = FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
                              NULL,
                              dwLastError,
                              MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
                              (LPTSTR)&szSystemMessage,
                              0,
                              NULL);


   TrimMsg(szExceptionMsg,_tcslen(szExceptionMsg));
   TrimMsg(szSystemMessage,nLen);

   CString strMsg;
	strMsg.Format(_T("%s (%s)(%s)"),szMsg,szExceptionMsg,szSystemMessage);
   
   //Free the buffer.
   if (szSystemMessage)
      VERIFY(HeapFree(GetProcessHeap(),0,szSystemMessage));

   LogMsg(m_typeMsg, 0, strMsg.GetString(), typeMsgLevelError);
}