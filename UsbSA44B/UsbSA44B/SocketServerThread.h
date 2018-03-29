#pragma once


#include "ClientHandlerSocket.h"
#include "SocketServerThreadManager.h"

#include <list>

// CSocketServerThread
class CSocketServerThread : public CWinThread
{
    friend CSocketServerThreadManager;
	DECLARE_DYNCREATE(CSocketServerThread)

protected:
   CSocketServerThread();           // protected constructor used by dynamic creation
	virtual ~CSocketServerThread();

public:
   virtual BOOL InitInstance();
   virtual int ExitInstance();
   static void MessageReadyForParser() { ::PostThreadMessage(GetCurrentThreadId(),msgParseMessage,0,0); }

protected:
   void StartListening(int nPort = 5025); // used by CSocketThreadManger

private:
   static void StopThread(CSocketServerThread* pThread);

protected:
   // Handles/sockets clients, need to store for a clean exit
   static std::list<CSocketServerThread*> m_listClientThreadHandle; // used by CSocketThreadManger

private:
   CClientHandlerSocket m_socket; // run socket server in a seperate thread, to avoid thread/blocking issues

   static const UINT msgStartListenServer;
   static const UINT msgStartClientHandler;
   static const UINT msgStopClientHandler;
   static const UINT msgParseMessage;

   bool m_bAttemptingToShutdownThread; // set to true to tell code to exit gracefully
   bool m_bClientHandler;

protected:
   afx_msg void OnStartListening(WPARAM wParam, LPARAM lParam);
   afx_msg void OnStartClient(WPARAM wParam, LPARAM lParam);
   afx_msg void OnStopClient(WPARAM wParam, LPARAM lParam);
   afx_msg void OnParseMessage(WPARAM wParam, LPARAM lParam);
	DECLARE_MESSAGE_MAP()

};


