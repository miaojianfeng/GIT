#pragma once

/////////////////////////////////////////////////////////////////////////////
// CAboutDlg (base class for CAboutBox and CSplashWnd)

class CAboutDlg : public CDialog
{
// Construction
public:
	CAboutDlg(UINT nIDTemplate, CWnd* pParent = NULL);    // standard constructor

// Dialog Data
	//{{AFX_DATA(CAboutDlg)
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA

// Implementation
protected:
	CAboutDlg(); // constructor for modeless dialogs
	
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	// Generated message map functions
	//{{AFX_MSG(CAboutDlg)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};
