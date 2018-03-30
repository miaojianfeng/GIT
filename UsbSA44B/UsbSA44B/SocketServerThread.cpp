// SocketThread.cpp : implementation file
//

#include "stdafx.h"
#include "SocketServerThread.h"
#include "GlobalDisplayMessage.h"

#include <algorithm>

// CSocketServerThread

IMPLEMENT_DYNCREATE(CSocketServerThread, CWinThread)


const UINT CSocketServerThread::msgStartListenServer = ::RegisterWindowMessage(_T("msgStartListenServer"));
const UINT CSocketServerThread::msgStartClientHandler = ::RegisterWindowMessage(_T("msgStartClientHandler"));
const UINT CSocketServerThread::msgStopClientHandler = ::RegisterWindowMessage(_T("msgStopClientHandler"));
const UINT CSocketServerThread::msgParseMessage = ::RegisterWindowMessage(_T("msgParseMessage"));

std::list<CSocketServerThread*> CSocketServerThread::m_listClientThreadHandle;

CSocketServerThread::CSocketServerThread() :
m_bAttemptingToShutdownThread(false),
m_bClientHandler(false)
{
}

CSocketServerThread::~CSocketServerThread()
{
}

BOOL CSocketServerThread::InitInstance()
{
   CWinThread::InitInstance();

   LogMsg(m_bClientHandler ? typeMsgSaEmulatorClientThread : typeMsgSaEmulatorListenThread, m_socket.GetDeviceNumber(), _T("::InitInstance"));

   if (!AfxSocketInit())
   {
      AfxMessageBox(IDP_SOCKETS_INIT_FAILED, MB_OK | MB_ICONSTOP);
      return FALSE;
   }

   // store in our static list for clean up
   m_listClientThreadHandle.push_back(this);

	return TRUE;
}

int CSocketServerThread::ExitInstance()
{ 
   // Find in our static list, remove it
   m_listClientThreadHandle.remove(this);

   LogMsg(m_bClientHandler ? typeMsgSaEmulatorClientThread : typeMsgSaEmulatorListenThread, m_socket.GetDeviceNumber(), _T("::ExitInstance"));

   if (m_socket.m_hSocket!=INVALID_SOCKET)
   {
      m_socket.ShutDown();
      m_socket.Close();
   }

   return CWinThread::ExitInstance();
}

void CSocketServerThread::StartListening(int nPort)
{
   LogMsg(typeMsgSaEmulatorThread, 0, _T("::StartListening - Create Listen Thread"));

   // Start the listener 
   PostThreadMessage(msgStartListenServer, nPort, 0);
}

void CSocketServerThread::StopThread(CSocketServerThread* pThread)
{
   LogMsg(typeMsgSaEmulatorThread, pThread->m_socket.GetDeviceNumber(), _T("::StopThread"));

   pThread->m_bAttemptingToShutdownThread = true;

   // if Listner thread has a blocking call
   if (pThread->m_socket.IsBlocking())
      pThread->m_socket.CancelBlockingCall(); // so just cancel, and the thread will unwind and quit

   pThread->SuspendThread(); // suspend while we manipulate thread object and duplicate handle

   if (pThread->m_bClientHandler)
      pThread->PostThreadMessage(msgStopClientHandler, 0, 0); // for client thread, we just need to post a quit

   const int nArraySize = 2;
   HANDLE arrayHandles[nArraySize];
   HANDLE& hTheadHandle = arrayHandles[0]; // ref to inside array, ie only have one copy of the handle
   HANDLE& hTimer = arrayHandles[1];

   // Create timer object for object loop waiting
   const int nTimerUnitsPerSecond = 10000000;
   const int nTimeoutInSeconds = 10;

   LARGE_INTEGER  li;
   li.QuadPart = -(nTimeoutInSeconds * nTimerUnitsPerSecond); // negative for relative
   hTimer = CreateWaitableTimer(NULL, FALSE, NULL);
   VERIFY(SetWaitableTimer(hTimer, &li, 0, NULL, NULL, FALSE));

   // Duplicate handle for object loop waiting
   // Duplicate handle, so we can check on it's state later
   VERIFY(DuplicateHandle(GetCurrentProcess(),pThread->m_hThread,GetCurrentProcess(),&hTheadHandle,0,FALSE,DUPLICATE_SAME_ACCESS));

   pThread->ResumeThread(); // after finishing dealing with pThread, resume it
   const auto nDeviceNumber = pThread->m_socket.GetDeviceNumber();
   pThread = NULL; // set to null to remind everyone not to use it past this point

   bool bWait = true;
   while(bWait)
   {
       // Stoping thread from main thread, so we need to do a secondary message loop, use MsgWait
       DWORD dwRet = MsgWaitForMultipleObjectsEx(nArraySize, arrayHandles, INFINITE, QS_ALLINPUT|QS_ALLPOSTMESSAGE, MWMO_INPUTAVAILABLE);
       switch (dwRet)
       {
          case WAIT_OBJECT_0 + 0: // thread
             bWait = false;
             break;

          case WAIT_OBJECT_0 + 1: // timer
             bWait = false;
             break;

          case WAIT_OBJECT_0 + nArraySize: // Msg
#ifdef _DEBUG
             {
                  _AFX_THREAD_STATE *pState = AfxGetThreadState();
                 if (pState->m_nDisablePumpCount != 0)
                 {
                    ASSERT(FALSE);
                    break;
                 }
             }
#endif

             if (!AfxPumpMessage()) // see if our thread got a WM_QUIT
                bWait = false;
             break;

          case WAIT_FAILED:
             bWait = false;
             break;

         case WAIT_ABANDONED_0 + 0:
         case WAIT_ABANDONED_0 + 1:
            bWait = false;
            break;

          default:
             bWait = false;
             ASSERT(FALSE);
             break;
       }
   }

   VERIFY(CancelWaitableTimer(hTimer));
   VERIFY(CloseHandle(hTimer));

   // check exit code again
   DWORD dwExitCode;
	GetExitCodeThread(hTheadHandle,&dwExitCode);
   if (dwExitCode==STILL_ACTIVE)
   {
      ASSERT(FALSE);
      LogMsg(typeMsgSaEmulatorThread, nDeviceNumber, _T("Abnormal Cleanup - TerminateThread called!"));
      // Force thread to exit - NOTE: this may result in memory leaks
		TerminateThread(hTheadHandle,3);
      WaitForSingleObject(hTheadHandle, INFINITE);
   }

   VERIFY(CloseHandle(hTheadHandle));
}

BEGIN_MESSAGE_MAP(CSocketServerThread, CWinThread)
   ON_REGISTERED_THREAD_MESSAGE(msgStartListenServer, &OnStartListening)
   ON_REGISTERED_THREAD_MESSAGE(msgStartClientHandler, &OnStartClient)
   ON_REGISTERED_THREAD_MESSAGE(msgStopClientHandler, &OnStopClient)
   ON_REGISTERED_THREAD_MESSAGE(msgParseMessage, &OnParseMessage)
END_MESSAGE_MAP()

void CSocketServerThread::OnStartListening(WPARAM wParam, LPARAM lParam)
{
   ASSERT(!m_bClientHandler); // should not be a client handler

   LogMsg(typeMsgSaEmulatorListenThread, m_socket.GetDeviceNumber(), _T("::OnStartListening - Entering"));

   const int nPort = (int)wParam;

   int nEnable = 1;
   m_socket.SetSockOpt(SO_REUSEADDR,&nEnable,sizeof(int));

   CString strMsg;
   strMsg.Format(_T("::OnStartListening - Create Socket & Listen on Port %d"),nPort);
   LogMsg(typeMsgSaEmulatorListenThread, m_socket.GetDeviceNumber(), strMsg);

   if(m_socket.Create(nPort)==0)
	{
      //AfxEndThread(1);
      //PostQuitMessage(1);
      PostThreadMessage(WM_QUIT, 0, 0);
      return;
	}

   if (!m_socket.Listen())
   {
      m_socket.Close();

      //AfxEndThread(1);
      //PostQuitMessage(1);
      PostThreadMessage(WM_QUIT, 0, 0);
      return;
   }

   bool bErrorExit = false;

   bool bWaitInListenLoop = true;
	while(bWaitInListenLoop)
	{
      auto pAcceptSock = new CThreadSafeSocket(typeMsgSaEmulatorSocketInput);

      LogMsg(typeMsgSaEmulatorListenThread, m_socket.GetDeviceNumber(), _T("::OnStartListening - Execute Socket Accept()"));

      // Blocking call, which is why we are running in a thread
		if(m_socket.Accept(*pAcceptSock)) // wait for someone to accept
		{
         LogMsg(typeMsgSaEmulatorListenThread, m_socket.GetDeviceNumber(), _T("::OnStartListening - Accept() sucessful"));

         // Setup params for new thread to deal with client
  			auto hSocketHandle = pAcceptSock->Detach();
            
         LogMsg(typeMsgSaEmulatorListenThread, m_socket.GetDeviceNumber(), _T("::OnStartListening - Create Client Thread"));

         // Create Suspended, so we get a chance to set auto delete & duplicate the thread handle
         auto pThread = AfxBeginThread(RUNTIME_CLASS(CSocketServerThread),THREAD_PRIORITY_NORMAL,0,CREATE_SUSPENDED);
         auto pSocketThread = dynamic_cast<CSocketServerThread*>(pThread);
         if (!pSocketThread)
         {
            LogMsg(typeMsgSaEmulatorListenThread, m_socket.GetDeviceNumber(), _T("::OnStartListening - AfxBeginThread failed"));
            bWaitInListenLoop = true;
            bErrorExit = true;
            delete pThread;
            break;
         }

         // Start client
         pThread->PostThreadMessage(msgStartClientHandler, hSocketHandle, 0);

         pThread->ResumeThread(); // Resume thread after setting delete & duplicating handle
		}
      else
      {
         if (!m_bAttemptingToShutdownThread)
         {
            LogMsg(typeMsgSaEmulatorListenThread, m_socket.GetDeviceNumber(), _T("::OnStartListening - Accept() failed"));
            bErrorExit = true;
            bWaitInListenLoop = false; // exit as soon as possible
         }
      }

      if (pAcceptSock)
         delete pAcceptSock;

      // before we loop again, set loop exit flag if needed
      if (m_bAttemptingToShutdownThread)
         bWaitInListenLoop = false;
	}

   m_socket.ShutDown(); // send out FIN Packet to any listeners

   LogMsg(typeMsgSaEmulatorListenThread, m_socket.GetDeviceNumber(), _T("Thread::OnStartListening - Exiting"));

   //AfxEndThread(bErrorExit ? 1 : 0);
   //PostQuitMessage(bErrorExit ? 1 : 0);
   PostThreadMessage(WM_QUIT, 0, 0);
}



void CSocketServerThread::OnStartClient(WPARAM wParam, LPARAM lParam)
{
   LogMsg(typeMsgSaEmulatorClientThread, m_socket.GetDeviceNumber(), _T("::OnStartClient"));

   // This socket will be a client handler
   m_bClientHandler = true;

   auto hSocketHandle = (SOCKET)wParam;
   m_socket.Attach(hSocketHandle); // attach, and let it work...
}

void CSocketServerThread::OnStopClient(WPARAM wParam, LPARAM lParam)
{
   LogMsg(typeMsgSaEmulatorClientThread, m_socket.GetDeviceNumber(), _T("::OnStopClient"));

   if (m_socket.IsBlocking())
      m_socket.CancelBlockingCall();

   if (m_bClientHandler)
   {
      // we need to kill the accept
      m_socket.ShutDown(); // Controlled shutdown, tell otherside of socket(ie client) we are going away
      m_socket.Close();
   }

   //AfxEndThread(0);
   //PostQuitMessage(0);
   PostThreadMessage(WM_QUIT, 0, 0);
}

void CSocketServerThread::OnParseMessage(WPARAM wParam, LPARAM lParam)
{
   m_socket.DoParseMessage();
}

