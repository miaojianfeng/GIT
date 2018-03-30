// aboutdlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "aboutdlg.h"
#include "versioninfo.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#ifdef _DEBUG
#undef THIS_FILE
static char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CAboutDlg (base class of CAboutBox and CSplashWnd)

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CAboutDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

CAboutDlg::CAboutDlg()
{
	// constructor for modeless dialogs
}

CAboutDlg::CAboutDlg(UINT nIDTemplate, CWnd* pParent /*=NULL*/)
	: CDialog(nIDTemplate, pParent)
{
	//{{AFX_DATA_INIT(CAboutDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutDlg)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}

/////////////////////////////////////////////////////////////////////////////
// CAboutDlg message handlers

BOOL CAboutDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// initialize app name, company name, comments, and copyright
	CVersionInfo versionInfo;
	SetDlgItemText(IDC_APPLICATION_NAME, AfxGetAppName());
//	SetDlgItemText(IDC_COMPANY_NAME, versionInfo.GetVersionString(_T("CompanyName")));
//	SetDlgItemText(IDC_COMMENTS_MFX, versionInfo.GetVersionString(_T("Comments")));
	SetDlgItemText(IDC_COPYRIGHT, versionInfo.GetVersionString(_T("LegalCopyright")));

	// initialize version
	CString strFullVersion, strVersionFormat;
	CString strVersionNumber = versionInfo.GetVersionString(_T("FileVersion"));
	GetDlgItemText(IDC_FILE_VERSION, strVersionFormat);
	strFullVersion.Format(strVersionFormat, strVersionNumber);
	SetDlgItemText(IDC_FILE_VERSION, strFullVersion);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

