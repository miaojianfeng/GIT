#pragma once

#include <vector>

#include "ThreadSafeSocket.h"
#include "C2090Protocol.h"
#include "SCPIMsg.h"

class CAVLZollnerDoc;

// CClientHandlerSocket command target

class CClientHandlerSocket : public CThreadSafeSocket
{
   DECLARE_DYNAMIC(CClientHandlerSocket); 

public:
   CClientHandlerSocket();
   virtual ~CClientHandlerSocket();
   int GetDeviceNumber() { return m_2090Protocol.GetDeviceNumber(); }
   void DoParseMessage();

protected:
   virtual int Send(const void* lpBuf, int nBufLen, int nFlags = 0);
   virtual void OnReceive(int nErrorCode);
   virtual void OnClose(int nErrorCode);

private:
   void LogPacket(bool bInput, const unsigned char* lpInputArray, const int nBytes);

private:
   std::vector<unsigned char> m_buffer;
   C2090Protocol m_2090Protocol;
   CSCPIMsg m_msgSCPI;
};