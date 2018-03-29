// C2090Protocol.cpp : implementation file
//

#include "stdafx.h"
#include "C2090Protocol.h"
#include "cmds.h"
#include "scpi.h"

#include <iterator>     // std::distance
#include <algorithm>    // std::find


#include "TVirtualDevice.h"

// C2090Protocol

bool C2090Protocol::m_bEnableSimulation = false;

C2090Protocol::C2090Protocol(CThreadSafeSocket& socket) :
m_socket(socket),
m_pVirtualDevice(new TVirtualDevice(*this))
{
}

C2090Protocol::~C2090Protocol()
{
   if (m_pVirtualDevice)
      delete m_pVirtualDevice;
   m_pVirtualDevice = NULL;
}


int C2090Protocol::GetDeviceNumber()
{
   return m_pVirtualDevice ? m_pVirtualDevice->GetDeviceNumber() : 0;
}

void C2090Protocol::Send(std::string& s)
{
   m_socket.Send(s.c_str(),s.length());
}


UCHAR C2090Protocol::Parse(std::vector<unsigned char>& msg)
{
   const int nBufferSize = 256;
   char SInput[nBufferSize]; // Copy of command line
   char *SCmd; // Pointer to command to be parsed
   UCHAR Err = SCPI_ERR_NONE; // Returned Error code
   BOOL bResetCmdTree; // Resets command tree if TRUE
   SCPI_CMD_NUM CmdNum; // Returned number of matching cmd
   struct strParam sParams[MAX_PARAMS]; // Returned parameters
   unsigned int uiNumSuf[MAX_NUM_SUFFIX]; // Returned numeric suffices
   UCHAR NumSufCnt; // Returned numeric suffix count

   // Prevent buffer overruns
   const auto nMsgLength = min(nBufferSize-1,msg.size()); // leave room for NULL

   //copy input buffer into SInput[], Add null terminator
   memcpy(SInput, msg.data(), nMsgLength);
   SInput[nMsgLength] = NULL; // null terminate it

   CString strLogMsg = _T("\"");
   strLogMsg += SInput;
   strLogMsg += _T("\"");

   LogMsg(::typeMsg2090EmulatorParser,GetDeviceNumber(),strLogMsg);


   // Parsing Loop
   SCmd = &(SInput[0]); // Point to first command in line
   bResetCmdTree = TRUE; // Reset tree for first command
   do // Loop for each command in line
   {
      Err = SCPI_Parse(&SCmd, bResetCmdTree, &CmdNum, sParams, &NumSufCnt, uiNumSuf);
      static __int64 nExecute = 0;

      CString strLogMsg;
      switch (Err)
      {
      case SCPI_ERR_NONE:
         nExecute++;
         strLogMsg.Format(_T("Command Found #%d"),CmdNum);
         LogMsg(typeMsg2090EmulatorParser,GetDeviceNumber(),strLogMsg);

         strLogMsg.Format(_T("Start Execute (SEQ #%ld)"),nExecute);
         LogMsg(typeMsg2090EmulatorParser,GetDeviceNumber(),strLogMsg);

         if (m_pVirtualDevice)
            m_pVirtualDevice->Execute(CmdNum, sParams, NumSufCnt, uiNumSuf);

         strLogMsg.Format(_T("End Execute (SEQ #%ld)"),nExecute);
         LogMsg(typeMsg2090EmulatorParser,GetDeviceNumber(),strLogMsg);
         break;
      case SCPI_ERR_TOO_MANY_NUM_SUF:
         // handle too many numeric suffices in command;
         strLogMsg = _T("too many numeric suffices in command");
         break;
      case SCPI_ERR_NUM_SUF_INVALID: 
         // handle invalid numeric suffix;
         strLogMsg = _T("invalid numeric suffix");
         break;
      case SCPI_ERR_INVALID_VALUE:
         // handle invalid value in list; break; case SCPI_ERR_INVALID_DIMS: handle invalid dimensions in channel list entry;
         strLogMsg = _T("invalid value in list; break; case SCPI_ERR_INVALID_DIMS: handle invalid dimensions in channel list entry");
         break;
      case SCPI_ERR_PARAM_OVERFLOW: 
         // handle overflow; 
         strLogMsg = _T("overflow");
         break;
      case SCPI_ERR_PARAM_UNITS: 
         // handle wrong units; 
         strLogMsg = _T("wrong units");
         break;
      case SCPI_ERR_PARAM_TYPE: 
         // handle wrong param type; 
         strLogMsg = _T("wrong param type");
         break;
      case SCPI_ERR_PARAM_COUNT: 
         // handle wrong param count; 
         strLogMsg = _T("wrong param count");
         break;
      case SCPI_ERR_UNMATCHED_QUOTE: 
         // handle unmatched quote; 
         strLogMsg = _T("unmatched quote");
         break;
      case SCPI_ERR_UNMATCHED_BRACKET: 
         // handle unmatched bracket; 
         strLogMsg = _T("unmatched bracket");
         break;
      case SCPI_ERR_INVALID_CMD: 
         // handle invalid command; 
         strLogMsg = _T("invalid command");
         break;
      }

      if (Err!=SCPI_ERR_NONE)
         LogMsg(typeMsg2090EmulatorParser,GetDeviceNumber(),strLogMsg,typeMsgLevelError);

      if (bResetCmdTree) // Don’t reset command tree
         bResetCmdTree = FALSE; // after first command in line
   } while (Err == SCPI_ERR_NONE && *SCmd); // Parse while no errors and
   // commands left to be parsed

   return Err;
}