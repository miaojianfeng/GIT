// This MFC Samples source code demonstrates using MFC Microsoft Office Fluent User Interface 
// (the "Fluent UI") and is provided only as referential material to supplement the 
// Microsoft Foundation Classes Reference and related electronic documentation 
// included with the MFC C++ library software.  
// License terms to copy, use or distribute the Fluent UI are available separately.  
// To learn more about our Fluent UI licensing program, please visit 
// http://go.microsoft.com/fwlink/?LinkId=238214.
//
// Copyright (C) Microsoft Corporation
// All rights reserved.

// UsbSA44BView.h : interface of the CUsbSA44BView class
//

#pragma once
#include <afxrich.h>
#include <string>

class CUsbSA44BCntrItem;

class CUsbSA44BView : public CRichEditView
{
protected: // create from serialization only
	CUsbSA44BView();
	DECLARE_DYNCREATE(CUsbSA44BView)

// Attributes
public:
	CUsbSA44BDoc* GetDocument() const;

// Operations
public:

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

