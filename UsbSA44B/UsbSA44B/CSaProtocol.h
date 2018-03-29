#pragma once

#include <afxsock.h>
#include <string>
#include <vector>
#include "ThreadSafeSocket.h"
class TVirtualDevice;

// CSaProtocol command target

class CSaProtocol : public CObject
{
public:
	CSaProtocol(CThreadSafeSocket& socket);
	virtual ~CSaProtocol();

public:
   int GetDeviceNumber();

   UCHAR Parse(std::vector<BYTE>& msg);
   void Send(std::string& s);

   bool IsSimulationEnabled() { return m_bEnableSimulation; }
   static void SetSimulation(bool bEnable) { m_bEnableSimulation = bEnable; }

private:
      static bool m_bEnableSimulation;
      std::vector<BYTE> m_buffer;
      TVirtualDevice*            m_pVirtualDevice;
      CThreadSafeSocket&         m_socket;
 };


