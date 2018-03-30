// aboutbox.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "aboutbox.h"

//#include <dos.h>
//#include <direct.h>

#ifdef _DEBUG
#undef THIS_FILE
static char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CAboutBox dialog

BEGIN_MESSAGE_MAP(CAboutBox, CAboutDlg)
	//{{AFX_MSG_MAP(CAboutBox)
	ON_WM_CTLCOLOR()
	//}}AFX_MSG_MAP
	ON_MESSAGE(WM_CTLCOLORDLG, OnCtlColorDlg)
END_MESSAGE_MAP()

CAboutBox::CAboutBox(CWnd* pParent /*=NULL*/)
	: CAboutDlg(IDD_ABOUTBOX, pParent)
{
	//{{AFX_DATA_INIT(CAboutBox)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT

	// create solid white brush with which to paint the dialog box's background white
	m_hDialogBrush = CreateSolidBrush(RGB(255, 255, 255));
}


void CAboutBox::PostNcDestroy()
{
	if (m_hDialogBrush)
		DeleteObject(m_hDialogBrush);

	CAboutDlg::PostNcDestroy();
}


void CAboutBox::DoDataExchange(CDataExchange* pDX)
{
	CAboutDlg::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutBox)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}

/////////////////////////////////////////////////////////////////////////////
// CAboutBox message handlers

BOOL CAboutBox::OnInitDialog()
{
	CAboutDlg::OnInitDialog();

	// fill title
	CString strOldTitle, strNewTitle;
	GetWindowText(strOldTitle);
	const CString strAppName(AfxGetAppName());
	const LPCTSTR lpszAppName = strAppName;
	AfxFormatStrings(strNewTitle, strOldTitle, &lpszAppName, 1);
	SetWindowText(strNewTitle);

	return TRUE;  // return TRUE  unless you set the focus to a control
}


LRESULT CAboutBox::OnCtlColorDlg(WPARAM, LPARAM)
// handler for the WM_CTLCOLORDLG message; customized to paint the dialog box's background white
{
	// returning the handle to our solid white brush will cause the dialog box's background to be painted white
	return reinterpret_cast<LRESULT>(m_hDialogBrush);
}


HBRUSH CAboutBox::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor) 
// handler for the WM_CTLCOLOR message; customized to paint the controls' backgrounds white (except for the OK button)
{
	CAboutDlg::OnCtlColor(pDC, pWnd, nCtlColor);
	
	// Set the background mode for text to transparent 
	// so background will show thru.
	pDC->SetBkMode(TRANSPARENT);

	// returning the handle to our solid white brush will cause the controls' backgrounds to be painted white
	// (except for the OK button)
	return m_hDialogBrush;
}
