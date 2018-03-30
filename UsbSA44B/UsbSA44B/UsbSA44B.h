#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"       // main symbols
#include "GlobalDisplayMessage.h"
#include <queue>


// CUsbSA44BApp:
// See UsbSA44B.cpp for the implementation of this class
//
class CUsbSA44BDoc;

class CUsbSA44BApp : public CWinAppEx
{
public:
	CUsbSA44BApp();

	CUsbSA44BDoc* m_pDoc;
	static const UINT m_msgLogMsg;

	CMutex m_mutexNewLogMsg;
	std::queue<CAppendToLogParams*> m_newLogMsg;

private: 
	const DWORD m_dwThreadID;

public:
	void LogMsg(enumTypeMsg typeMsg, int nInstance, LPCTSTR lpszText, enumTypeMsgLevel typeMsgLevel = typeMsgLevelNormal);

private:
	void AppendToLog(LPCTSTR lpszTime, LPCTSTR lpszMessage, enumTypeMsgLevel typeMsgLevel = typeMsgLevelNormal);

// Overrides
public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();

// Implementation
	UINT  m_nAppLook;
	virtual void PreLoadState();
	virtual void LoadCustomState();
	virtual void SaveCustomState();
	
	afx_msg void OnAppAbout();
	afx_msg void OnRegisteredLogMsg(WPARAM wParam, LPARAM lParam);
	DECLARE_MESSAGE_MAP()
//	afx_msg void OnFileNew();
//	afx_msg void OnUpdateFileNew(CCmdUI *pCmdUI);

private:
	//bool m_bServerStarted;
};

extern CUsbSA44BApp theApp;
