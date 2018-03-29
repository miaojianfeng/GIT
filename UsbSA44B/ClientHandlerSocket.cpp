// ClientHandlerSocket.cpp : implementation file
//

#include "stdafx.h"
#include "ClientHandlerSocket.h"

#include "AVLZollnerDoc.h"
#include "GlobalDisplayMessage.h"
#include "SocketServerThread.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#endif

IMPLEMENT_DYNAMIC(CClientHandlerSocket, CThreadSafeSocket)

// CClientHandlerSocket

CClientHandlerSocket::CClientHandlerSocket() :
CThreadSafeSocket(typeMsg2090EmulatorSocket),
m_2090Protocol(*this)
{
}

CClientHandlerSocket::~CClientHandlerSocket()
{
}

// CClientHandlerSocket member functions
int CClientHandlerSocket::Send(const void* lpBuf, int nBufLen, int nFlags)
//void CClientHandlerSocket::OnSend(int nErrorCode)
{
	BOOL bReturn = TRUE;

   CThreadSafeSocket::Send(lpBuf,nBufLen,nFlags);
   LogPacket(false,(unsigned char*)lpBuf,nBufLen);
	
   CThreadSafeSocket::Send("\n",1,nFlags);
   LogPacket(false,(unsigned char*)"\n",1);

	return bReturn;
}

void CClientHandlerSocket::LogPacket(bool bInput, const unsigned char* lpInputArray, const int nBytes)
{
   CString strLogMsg;

   auto pData = (LPSTR)lpInputArray;
   int cCount = nBytes;
   bool bIsASCIIData = true;
   while(cCount-- && bIsASCIIData)
      bIsASCIIData = __isascii(*pData++);

   if (!bIsASCIIData)
   {
      CString strBinary = _T("<Binary Data>");

      strBinary +=_T("[");

      TCHAR szHex[3];
      for (int i = 0; i < nBytes; i++)
      {
         _sntprintf_s(szHex, sizeof(szHex), _T("%x"), lpInputArray[i]);
         strBinary += szHex;
      }
       strBinary += _T("]");

       strLogMsg += strBinary;
   }
   else // display ASCII strings
   {
      // Replace CR & LF
      CString temp_string;
      // Convert A2T
      USES_CONVERSION;

      temp_string.SetString(A2T((LPSTR)lpInputArray),nBytes);
      temp_string.Replace(_T("\r"),_T("\\r"));
      temp_string.Replace(_T("\n"),_T("\\n"));
      strLogMsg += _T("\"");
      strLogMsg += temp_string;
      strLogMsg += _T("\"");
   }

   LogMsg(bInput ? typeMsg2090EmulatorSocketInput : typeMsg2090EmulatorSocketOutput, m_2090Protocol.GetDeviceNumber(), strLogMsg);
}

void CClientHandlerSocket::OnReceive(int nErrorCode)
{
   //LogMsg(typeMsg2090EmulatorSocket,m_2090Protocol.GetDeviceNumber(),_T("::OnReceive"));

    CString strDisplayMsg;
    CString strErrorMsg;

   if (nErrorCode!=0)
      return;

   DWORD dwBytesAvailable;
   IOCtl(FIONREAD,&dwBytesAvailable);

   // Use heap buffer, and "reserve" to grow as needed; this avoids future memory limitations
   m_buffer.resize(dwBytesAvailable);
   auto pBuffer = m_buffer.data();
   
   int nRead = Receive(pBuffer, dwBytesAvailable);
   switch (nRead)
   {
      case SOCKET_ERROR:
         LogMsg(typeMsg2090EmulatorSocketInput,m_2090Protocol.GetDeviceNumber(),_T("::OnReceive - Socket Error detected"),typeMsgLevelError);
         //if (GetLastError() != WSAEWOULDBLOCK) 
         //AfxMessageBox (_T("Socket Error detected")); // display error message
         Close(); // on error, or on zero byte read, close, which will kill thread
         break;
      case 0:
         LogMsg(typeMsg2090EmulatorSocketInput,m_2090Protocol.GetDeviceNumber(),_T("::OnReceive - NULL bytes detected"),typeMsgLevelError);
         Close(); // on error, or on zero byte read, close, which will kill thread
         break;

      default:
         LogPacket(true,pBuffer,nRead);

         m_msgSCPI.ProcessReceiveBuffer(m_buffer,*this); // parse each message and put in string table

         CSocketServerThread::MessageReadyForParser();

         //if (!m_msgSCPI.m_strText.IsEmpty())
         //   LogMsg(typeMsg2090EmulatorSocketInput,m_2090Protocol.GetDeviceNumber(),_T("::OnReceive - Split packet detected"),typeMsgLevelWarning);
   }

   CThreadSafeSocket::OnReceive(nErrorCode);
}

void CClientHandlerSocket::OnClose(int nErrorCode)
{
   // Client may have initate close, so close

   LogMsg(typeMsg2090EmulatorSocket,m_2090Protocol.GetDeviceNumber(),_T("::OnClose"));

   CThreadSafeSocket::OnClose(nErrorCode);
   //m_buffer.clear(); // we closed it, so there is no point to keeping the unprocessed incomming data
   //m_msgSCPI.ClearInternalBuffers();
   //AfxEndThread(0);
   //PostQuitMessage(0);
   AfxGetThread()->PostThreadMessage(WM_QUIT, 0, 0);
}

void CClientHandlerSocket::DoParseMessage()
{
   static bool bExecuting = false;

   if (bExecuting)
      return; // igore request

   bExecuting = true;
   
   while (!m_msgSCPI.m_msgQueue.empty())
   {
      auto& msg = m_msgSCPI.m_msgQueue.front();

      auto Error = m_2090Protocol.Parse(msg);

      m_msgSCPI.m_msgQueue.pop();
   }

   bExecuting = false;
}
