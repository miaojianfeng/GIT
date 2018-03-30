// UsbSA44B.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "afxwinappex.h"
#include "afxdialogex.h"
#include "UsbSA44B.h"
#include "MainFrm.h"

#include "UsbSA44BDoc.h"
#include "UsbSA44BView.h"
#include "LimitSingleInstance.h" 
#include "aboutbox.h"

#include <strstream>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// The one and only CLimitSingleInstance object
// Change what is passed to constructor. GUIDGEN Tool may be of help.
CLimitSingleInstance g_SingleInstanceObj(TEXT("{CBD67201-BB4F-4A01-B092-FE48737C46EA}"));

const UINT CUsbSA44BApp::m_msgLogMsg = ::RegisterWindowMessage(_T("msgLogMsg"));

// CUsbSA44BApp

BEGIN_MESSAGE_MAP(CUsbSA44BApp, CWinAppEx)
	ON_COMMAND(ID_APP_ABOUT, &CUsbSA44BApp::OnAppAbout)
	// Standard file based document commands
//	ON_COMMAND(ID_FILE_NEW, &CWinAppEx::OnFileNew)
	ON_COMMAND(ID_FILE_OPEN, &CWinAppEx::OnFileOpen)
	ON_REGISTERED_THREAD_MESSAGE(m_msgLogMsg, &OnRegisteredLogMsg)	
	ON_COMMAND(ID_FILE_NEW, &CUsbSA44BApp::OnFileNew)
//	ON_UPDATE_COMMAND_UI(ID_FILE_NEW, &CUsbSA44BApp::OnUpdateFileNew)
END_MESSAGE_MAP()


// CUsbSA44BApp construction

CUsbSA44BApp::CUsbSA44BApp():
	//m_bServerStarted(FALSE),
	m_pDoc(NULL),
	m_mutexNewLogMsg(FALSE),
	m_dwThreadID(GetThreadId(AfxGetThread()->m_hThread))
{
	// TODO: replace application ID string below with unique ID string; recommended
	// format for string is CompanyName.ProductName.SubProduct.VersionInformation
	//SetAppID(_T("UsbSA44B.AppID.NoVersion"));

	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

// The one and only CUsbSA44BApp object

CUsbSA44BApp theApp;


// CUsbSA44BApp initialization

BOOL CUsbSA44BApp::InitInstance()
{
	if (g_SingleInstanceObj.IsAnotherInstanceRunning())
		return FALSE; // sorry only one is allowed

	if (!AfxSocketInit())
	{
		AfxMessageBox(IDP_SOCKETS_INIT_FAILED);
		return FALSE;
	}

	// InitCommonControlsEx() is required on Windows XP if an application
	// manifest specifies use of ComCtl32.dll version 6 or later to enable
	// visual styles.  Otherwise, any window creation will fail.
	INITCOMMONCONTROLSEX InitCtrls;
	InitCtrls.dwSize = sizeof(InitCtrls);
	// Set this to include all the common control classes you want to use
	// in your application.
	InitCtrls.dwICC = ICC_WIN95_CLASSES;
	InitCommonControlsEx(&InitCtrls);

	CWinAppEx::InitInstance();


	// Initialize OLE libraries
	if (!AfxOleInit())
	{
		AfxMessageBox(IDP_OLE_INIT_FAILED);
		return FALSE;
	}

	AfxEnableControlContainer();

	EnableTaskbarInteraction(FALSE);

	// AfxInitRichEdit2() is required to use RichEdit control	
	// AfxInitRichEdit2();

	// Standard initialization
	// If you are not using these features and wish to reduce the size
	// of your final executable, you should remove from the following
	// the specific initialization routines you do not need
	// Change the registry key under which our settings are stored
	// TODO: You should modify this string to be something appropriate
	// such as the name of your company or organization
	SetRegistryKey(_T("ETS-Lindgren"));
	LoadStdProfileSettings(0);  // Load standard INI file options (including MRU)


	InitContextMenuManager();

	InitKeyboardManager();

	InitTooltipManager();
	CMFCToolTipInfo ttParams;
	ttParams.m_bVislManagerTheme = TRUE;
	theApp.GetTooltipManager()->SetTooltipParams(AFX_TOOLTIP_TYPE_ALL,
		RUNTIME_CLASS(CMFCToolTipCtrl), &ttParams);

	// Register the application's document templates.  Document templates
	//  serve as the connection between documents, frame windows and views
	CSingleDocTemplate* pDocTemplate;
	pDocTemplate = new CSingleDocTemplate(
		IDR_MAINFRAME,
		RUNTIME_CLASS(CUsbSA44BDoc),
		RUNTIME_CLASS(CMainFrame),       // main SDI frame window
		RUNTIME_CLASS(CUsbSA44BView));
	if (!pDocTemplate)
		return FALSE;
	pDocTemplate->SetContainerInfo(IDR_CNTR_INPLACE);
	AddDocTemplate(pDocTemplate);

	// Parse command line for standard shell commands, DDE, file open
	CCommandLineInfo cmdInfo;
	ParseCommandLine(cmdInfo);

	// Dispatch commands specified on the command line.  Will return FALSE if
	// app was launched with /RegServer, /Register, /Unregserver or /Unregister.
	if (!ProcessShellCommand(cmdInfo))
		return FALSE;

	// The one and only window has been initialized, so show and update it
	m_pMainWnd->ShowWindow(SW_SHOW);
	m_pMainWnd->UpdateWindow();

	LogMsg(typeMsgNone, 0, _T("Hello World"), typeMsgLevelInformation);
	LogMsg(typeMsgNone, 0, _T("Start SCPI Server..."), typeMsgLevelInformation);

	return TRUE;
}

int CUsbSA44BApp::ExitInstance()
{
	//TODO: handle additional resources you may have added
	AfxOleTerm(FALSE);

	return CWinAppEx::ExitInstance();
}

void CUsbSA44BApp::LogMsg(enumTypeMsg typeMsg, int nInstance, LPCTSTR lpszText, enumTypeMsgLevel typeMsgLevel /*= typeMsgLevelNormal*/)
{
	// First record time(NOTE: locking may move things arround)
	TCHAR szTime[128];
	_tstrtime_s(szTime);

	CString strMsg;

	switch (typeMsg)
	{
	case typeMsgSaEmulatorThread:
		strMsg.Format(_T("SaEmulator::Thread(%d)"), nInstance);
		break;
	case typeMsgSaEmulatorListenThread:
		strMsg.Format(_T("SaEmulator::ListenThread(%d)"), nInstance);
		break;
	case typeMsgSaEmulatorClientThread:
		strMsg.Format(_T("SaEmulator::ClientThread(%d)"), nInstance);
		break;
	case typeMsgSaEmulatorParser:
		strMsg.Format(_T("SaEmulator::Parser(%d) "), nInstance);
		break;
	case typeMsgSaEmulatorSocket:
		strMsg.Format(_T("SaEmulator::Socket(%d) "), nInstance);
		break;
	case typeMsgSaEmulatorSocketInput:
		strMsg.Format(_T("SaEmulator::Socket(%d)<-"), nInstance);
		break;
	case typeMsgSaEmulatorSocketOutput:
		strMsg.Format(_T("SaEmulator::Socket(%d)->"), nInstance);
		break;	
	}

	// Append the text
	strMsg += lpszText;

	AppendToLog(szTime, strMsg, typeMsgLevel);
}

void CUsbSA44BApp::AppendToLog(LPCTSTR szTime, LPCTSTR lpszMessage, enumTypeMsgLevel typeMsgLevel /*= typeMsgLevelNormal*/)
{
	// This is designed so multiple threads can updte the log
	// avoids threading issues

	// We need to post the request to the GUI thread
	const auto pAppendToLogParams = new CAppendToLogParams(); // NOTE: this could result in a small memory leak! - depends on timeing of message loop shutdown
	pAppendToLogParams->strTime = szTime;
	pAppendToLogParams->strMessage = lpszMessage;
	pAppendToLogParams->typeMsgLevel = typeMsgLevel;

	CSingleLock sl(&m_mutexNewLogMsg, TRUE);

	const auto nMsgCount = m_newLogMsg.size(); // must be locked when we check empty status
	m_newLogMsg.push(pAppendToLogParams); // must be locked when we push

	sl.Unlock(); // must release lock/ aka unlock outside of try/catch block

	if (m_dwThreadID == GetThreadId(AfxGetThread()->m_hThread))
	{
		OnRegisteredLogMsg(0, 0); // we are in the right thread, no need to post
		return;
	}

	// If there are messages in the quque, then the AppendToLog is running
	if (nMsgCount > 1 && nMsgCount < 10)
		return; // no need to kick start the append

	const auto bSuccess = ::PostThreadMessage(m_dwThreadID, CUsbSA44BApp::m_msgLogMsg, 0, 0);
	if (!bSuccess)
	{
		ASSERT(FALSE);
		if (GetLastError() == ERROR_NOT_ENOUGH_QUOTA)
			Sleep(500); // slow the calling thread down, our message quque is full!
	}
}

void CUsbSA44BApp::OnRegisteredLogMsg(WPARAM wParam, LPARAM lParam)
{
	auto pos = GetFirstDocTemplatePosition();
	if (!pos)
		return;

	auto pTemplate = GetNextDocTemplate(pos);
	if (!pTemplate)
		return;

	auto posDoc = pTemplate->GetFirstDocPosition();
	if (!posDoc)
		return;

	auto pDoc = dynamic_cast<CUsbSA44BDoc*>(pTemplate->GetNextDoc(posDoc));
	if (!pDoc)
		return;

	for (POSITION pos = pDoc->GetFirstViewPosition(); pos != NULL;)
	{
		CView* pView = pDoc->GetNextView(pos);
		auto pAVLZollnerView = dynamic_cast<CUsbSA44BView*>(pView);

		if (!pAVLZollnerView)
			continue;

		pAVLZollnerView->DoAppendToLog(m_mutexNewLogMsg, m_newLogMsg);
		break;
	}

}

// CAboutDlg dialog used for App About
void CUsbSA44BApp::OnAppAbout()
{
	CAboutBox aboutBox;
	aboutBox.DoModal();
}



// CUsbSA44BApp customization load/save methods
void CUsbSA44BApp::PreLoadState()
{
	BOOL bNameValid;
	CString strName;
	bNameValid = strName.LoadString(IDS_EDIT_MENU);
	ASSERT(bNameValid);
	GetContextMenuManager()->AddMenu(strName, IDR_POPUP_EDIT);
}

void CUsbSA44BApp::LoadCustomState()
{
}

void CUsbSA44BApp::SaveCustomState()
{
}

// CUsbSA44BApp message handlers

//void CUsbSA44BApp::OnFileNew()
//{
//	//m_bServerStarted = FALSE;
//	LogMsg(typeMsgNone, 0, _T("Hello World"), typeMsgLevelInformation);
//}


//void CUsbSA44BApp::OnUpdateFileNew(CCmdUI *pCmdUI)
//{
//	// TODO: Add your command update UI handler code here
//	//pCmdUI->Enable(m_bServerStarted);
//}
