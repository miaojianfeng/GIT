#pragma once
#include <afxsock.h>
#include <afxmt.h>

// CThreadSafeSocket command target

class CThreadSafeSocket : public CSocket
{
public:
   CThreadSafeSocket(enumTypeMsg typeMsg);
   //CThreadSafeSocket(enumTypeMsg typeMsg, const CThreadSafeSocket& src) : CSocket(src), m_typeMsg(typeMsg) { }
	virtual ~CThreadSafeSocket();

   BOOL Create(UINT nSocketPort = 0, int nSocketType=SOCK_STREAM,
		LPCTSTR lpszSocketAddress = NULL);

   void LogSocketError(LPCTSTR szMsg, LPTSTR szExceptionMsg);

private:
   static CCriticalSection m_CriticalSection;
   enumTypeMsg m_typeMsg;
};


