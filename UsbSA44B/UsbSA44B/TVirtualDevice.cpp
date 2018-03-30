// -----------------------------------------------------------------------------
// Filename : TVirtualDevice.cpp
// Copyright: 2008 by ETS-Lindgren L.P.
// Date     : Oct-11-2008
// Author   : Antonio Peloso
// Overview : 
//------------------------------------------------------------------------------

#include "stdafx.h"
#include "vcl.h"

#include <string.h>
#include "TVirtualDevice.h"

#define ALLOW_EMULATION_OF_UNSUPPORTED_COMMANDS 0

#define NOT_SUPPORTED_ERROR\
   USES_CONVERSION; \
   CA2CT msg(__FUNCTION__ " NOT SUPPORTED"); \
   LogMsg(typeMsgSaEmulatorParser, GetDeviceNumber(), msg, typeMsgLevelError); \
   return;

//---------------------------------------------------------------------------

//#pragma package(smart_init)

//---------------------------------------------------------------------------
TVirtualDevice::TVirtualDevice(CSaProtocol& protocol) :
	m_protocol(protocol),
	m_pEngine(new TSignalAnalyzerEngine())
{
  memset(m_DebugBuffer, 0x00, sizeof(m_DebugBuffer));

  // so we don't get a function pointing to nothing
  for (int i = 0; i < 128; i++) {
    m_operation[i] = &TVirtualDevice::Error;
  }
  //
  // get command numbers from cmds.c in COMMAND SPECS - Part 1: Command Keywords
  m_operation[4] = &TVirtualDevice::GetID;
  m_operation[6] = &TVirtualDevice::GetStatus;
  m_operation[7] = &TVirtualDevice::Reset;
  
  m_operation[24] = &TVirtualDevice::SetLocalMode;  
// -----------------------------------------------------------------------------

}

TVirtualDevice::~TVirtualDevice()
{ 
   if (m_pEngine)
      delete m_pEngine;
   m_pEngine = NULL;
}

#include <OleAuto.h>
//---------------------------------------------------------------------------
void TVirtualDevice::Execute(SCPI_CMD_NUM iCmdSpecNum, struct strParam sParam[],
        UCHAR ucNumSufCnt, unsigned int uiNumSuf[])
{
  if (!m_operation[iCmdSpecNum])
  {
     NOT_SUPPORTED_ERROR;
  }
  else
      (this->*m_operation[iCmdSpecNum])(sParam, ucNumSufCnt, uiNumSuf);
}

//---------------------------------------------------------------------------
void TVirtualDevice::Error(struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[])
{
   NOT_SUPPORTED_ERROR
}

//---------------------------------------------------------------------------
AnsiString  TVirtualDevice::Parse(char* SInput)
{
  AnsiString s;
  char *SCmd;           // Pointer to command to be parsed
  UCHAR Err;            // Returned Error code
  BOOL bResetCmdTree; // Resets command tree if TRUE
  SCPI_CMD_NUM CmdNum; // Returned number of matching cmd
  struct strParam sParams[MAX_PARAMS]; // Returned parameters
  unsigned int uiNumSuf[MAX_NUM_SUFFIX]; // Returned numeric suffices
  UCHAR NumSufCnt; // Returned numeric suffix count

  // Parsing Loop
  SCmd = SInput; // Point to first command in line
  bResetCmdTree = TRUE; // Reset tree for first command
  do {// Loop for each command in line

    Err = SCPI_Parse (&SCmd, bResetCmdTree, &CmdNum, sParams, &NumSufCnt, uiNumSuf);
    // Parse command
    if (Err == SCPI_ERR_NONE) { //command is valid  - Dispatch Table
        sprintf_s( m_DebugBuffer, sizeof(m_DebugBuffer), "Command = %d  Sufix = %d\n\r", CmdNum, NumSufCnt);
        Execute(CmdNum, sParams, NumSufCnt, uiNumSuf);
    }
    else if (Err == SCPI_ERR_NO_COMMAND) {
      continue;
    }
    else { // command is invalid
       s = AnsiString(std::to_string(long long(Err))) + ": Invalid Command";
        break;
        //throw(Err);
    }
    if (bResetCmdTree)        // Don’t reset command tree
      bResetCmdTree = FALSE; // after first command in line
  } while (Err == SCPI_ERR_NONE); // Parse while no errors and
  // commands left to be parsed

  return s;
}

//---------------------------------------------------------------------------
// 4
// *IDN?
// Query Identity
//----------------------------------------------------------------------------
void TVirtualDevice::GetID(struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[])
{
   AnsiString s = m_pEngine->GetID();
   m_protocol.Send(s);
}

//---------------------------------------------------------------------------
// 6
// *OPC?
// Operation Complete Query
//----------------------------------------------------------------------------
void TVirtualDevice::GetStatus(struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[])
{
	short nStatus = 0;

	if (m_protocol.IsSimulationEnabled())
	{
		nStatus = 1;
	}
	else
	{
		nStatus = m_pEngine->GetStatus();		
	}	

	short nOPC = nStatus == 0 ? 1 : 0; // we return back 1 when we have completed motion, and 0 when moving

	auto s = str_printf("%hd", nOPC); // %hd short int
	m_protocol.Send(s);
}


//---------------------------------------------------------------------------
// 7
// *RST
// Reset Instrument
//----------------------------------------------------------------------------
void TVirtualDevice::Reset(struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[])
{
#if ALLOW_EMULATION_OF_UNSUPPORTED_COMMANDS   
	Variant vr(__func__ " Not implemented yet");
	return vr;
#else
	NOT_SUPPORTED_ERROR
#endif
}

//---------------------------------------------------------------------------
// 24
// RTL
// Causes the device to return to local mode.
//---------------------------------------------------------------------------
void TVirtualDevice::SetLocalMode(struct strParam sParam[], UCHAR ucNumSufCnt, unsigned int uiNumSuf[])
{
#if ALLOW_EMULATION_OF_UNSUPPORTED_COMMANDS  
	SetLocalMode();
	Variant vr(__func__ " OK");
	return vr;
#else
	NOT_SUPPORTED_ERROR
#endif
}