#pragma once

#include "aboutdlg.h"

/////////////////////////////////////////////////////////////////////////////
// CAboutBox dialog

class CAboutBox : public CAboutDlg
{
// Construction
public:
	CAboutBox(CWnd* pParent = NULL);    // standard constructor

// Dialog Data
	//{{AFX_DATA(CAboutBox)
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA

// Implementation
protected:
	HBRUSH m_hDialogBrush;
	// solid white brush that is used to paint the dialog box's background white

	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual void PostNcDestroy();

	// Generated message map functions
	//{{AFX_MSG(CAboutBox)
	virtual BOOL OnInitDialog();
	afx_msg HBRUSH OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor);
	//}}AFX_MSG

	LRESULT OnCtlColorDlg(WPARAM wParam, LPARAM lParam);
	// handler for the WM_CTLCOLORDLG message; customized to paint the dialog box's background white

	DECLARE_MESSAGE_MAP()
};
