// UsbSA44BView.h : interface of the CUsbSA44BView class
//

#pragma once
#include <afxrich.h>
#include <string>
#include "UsbSA44BDoc.h"

//class CUsbSA44BCntrItem;
//class CUsbSA44BDoc;

class CUsbSA44BView : public CRichEditView
{
protected: // create from serialization only
	CUsbSA44BView();
	DECLARE_DYNCREATE(CUsbSA44BView)

// Attributes
public:
	CUsbSA44BDoc* GetDocument() const;

// Operations


// Overrides
public:
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
protected:	
	

// Implementation
public:
	virtual ~CUsbSA44BView();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:

// Generated message map functions
protected:
	afx_msg void OnRButtonUp(UINT nFlags, CPoint point);
	afx_msg void OnContextMenu(CWnd* pWnd, CPoint point);
	DECLARE_MESSAGE_MAP()

private:
	const int m_nMaxLogLines;

public:
	void DoAppendToLog(CMutex& mutexNewLogMsg, std::queue<CAppendToLogParams*>& newLogMsg);

private:
	void DoAppendToLog(LPCTSTR lpszTime, LPCTSTR lpszMessage, enumTypeMsgLevel typeMsgLevel = typeMsgLevelNormal);
};

#ifndef _DEBUG  // debug version in UsbSA44BView.cpp
inline CUsbSA44BDoc* CUsbSA44BView::GetDocument() const
   { return reinterpret_cast<CUsbSA44BDoc*>(m_pDocument); }
#endif

