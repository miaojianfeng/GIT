#include "stdafx.h"
#include "vcl.h"
#pragma hdrstop
#include <math.h>

#include "TSignalAnalyzerEngine.h"

TSignalAnalyzerEngine::TSignalAnalyzerEngine()	
{
	InitParams();
}

void TSignalAnalyzerEngine::InitParams()
{
	m_bStatus = FALSE;
	m_bLocalMode = FALSE;
	m_idNum = -1;
	m_id = "Not Assigned";
}

AnsiString TSignalAnalyzerEngine::GetID()
{
	return m_id;
}

void TSignalAnalyzerEngine::UpdateID()
{
	const int cSize = 128;
	m_id.reserve(cSize);
	int nSize = sprintf_s((LPSTR)m_id.data(), m_id.capacity(), "ETS-Lindgren Inc., SA Emulator for Signal Hound USB-SA44B,0,REV 1.00 March-30-18 12:30 SN = %d", m_idNum);
	m_id.resize(nSize);
}

