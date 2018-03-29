#include "stdAfx.h"
#include "SocketServerThreadManager.h"
#include "SocketServerThread.h"

CSocketServerThreadManager::CSocketServerThreadManager(void) :
	m_nPort(5025),
   m_bServerRunning(false)
{
}

CSocketServerThreadManager::~CSocketServerThreadManager(void)
{
   Stop();
}

void CSocketServerThreadManager::Start()
{
   if (m_bServerRunning)
      return;

   m_bServerRunning = true;

   LogMsg(typeMsgSaEmulatorThread, 0, _T("::Start - AfxBeginThread"));

   // Create Suspended
	auto pThread = AfxBeginThread(RUNTIME_CLASS(CSocketServerThread),THREAD_PRIORITY_NORMAL,0,CREATE_SUSPENDED);
   auto pSocketThread = dynamic_cast<CSocketServerThread*>(pThread);
   if (!pSocketThread)
   {
      LogMsg(typeMsgSaEmulatorThread, 0, _T("::Start - AfxBeginThread Failed"));
      delete pThread;
      return;
   }
   
   pSocketThread->StartListening(m_nPort);

   pSocketThread->ResumeThread();
}

void CSocketServerThreadManager::Stop()
{
   if (!m_bServerRunning)
      return;

   m_bServerRunning = false;

   // Check to see if we have a client thread, clean it up
   while(!CSocketServerThread::m_listClientThreadHandle.empty())
	{
      auto pSocketThread = CSocketServerThread::m_listClientThreadHandle.back();

      CSocketServerThread::StopThread(pSocketThread);
   }
}


