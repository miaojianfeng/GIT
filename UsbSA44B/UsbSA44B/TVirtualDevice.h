#pragma once
#include  "TSignalAnalyzerEngine.h"

#include <map>

#include "cmds.h"
#include "scpi.h"
#include "CSaProtocol.h"

using namespace std;

class TVirtualDevice;
// Set this define to use a software OPC when hardware does not work correctly!
#define _EMULATE_OPC 0

//---------------------------------------------------------------------------
//typedef Variant (TVirtualDevice::*MachineMethod)(struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[]);
typedef void (TVirtualDevice::*MachineMethod)(struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[]);

//   using MachineMethod = Variant (TVirtualDevice::*)(struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[]);

//---------------------------------------------------------------------------
class TVirtualDevice 
{
protected:
  char                      m_DebugBuffer[256];
  TSignalAnalyzerEngine*    m_pEngine;
  map< int, MachineMethod>  m_operation;
  CSaProtocol&              m_protocol;
   
public:
  //TVirtualDevice();
  TVirtualDevice(CSaProtocol& protocol);
  virtual ~TVirtualDevice();
  virtual AnsiString Parse(char* SInput);
  virtual void Execute(SCPI_CMD_NUM iCmdSpecNum, struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[]);
  virtual void Error(struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[]);

  char* GetDebugBuffer()  { return m_DebugBuffer; }
  int GetDeviceNumber() { return m_pEngine->GetDeviceNumber(); }

  // please leave these methods in alphabetical order
  void  SetLocalMode() { m_pEngine->SetLocalMode(); }
  
  int   GetStatus() { return m_pEngine->GetStatus(); }
  void  SetStatus(int v) { m_pEngine->SetStatus(v); }
  
  //  2090 SCPI commands
  // ===========================================================================
  virtual void  GetID(struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[]);
  virtual void  GetStatus(struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[]);
  virtual void  Reset(struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[]);  
  
  virtual void  SetLocalMode(struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[]);
};

