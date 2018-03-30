#pragma once
#include <afx.h>
#include "vcl.h"

#define ERROR_IF_NOT(_type) \
if (m_Type!=_type) \
{ \
   LogMsg(typeMsgSaEmulatorParser,m_idNum,_T("Wrong Type")); \
   return 0.00; \
}

#define ERROR_IF_NOTINT(_type) \
if (m_Type!=_type) \
{ \
   LogMsg(typeMsgSaEmulatorParser,m_idNum,_T("Wrong Type")); \
   return 0; \
}

class TSignalAnalyzerEngine : public TThread
{
public:
	TSignalAnalyzerEngine();
	virtual ~TSignalAnalyzerEngine() {}
	//void __fastcall Execute();

	//
	void  SetID(int id) { m_idNum = id; UpdateID(); }
	AnsiString  GetID();
	int GetDeviceNumber() { return m_idNum; }
	
	// *OPC? Query
	bool GetStatus() { return m_bStatus; } // returns false if still moving
	void SetStatus(bool status) { m_bStatus = status;  }

	void  SetLocalMode() { m_bLocalMode = true; }  // Causes the device to return to local mode.

	//AnsiString  Parse(AnsiString& cmd);
	void UpdateID();

private:
	void InitParams();

private:
	bool          m_bStatus;
	bool          m_bLocalMode;

	static int    m_idLast;
	int           m_idNum;
	std::string   m_id;
};