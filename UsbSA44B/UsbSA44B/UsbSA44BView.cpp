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

// UsbSA44BView.cpp : implementation of the CUsbSA44BView class
//

#include "stdafx.h"
// SHARED_HANDLERS can be defined in an ATL project implementing preview, thumbnail
// and search filter handlers and allows sharing of document code with that project.
#ifndef SHARED_HANDLERS
#include "UsbSA44B.h"
#endif

#include "UsbSA44BDoc.h"
#include "resource.h"
#include "UsbSA44BView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CUsbSA44BView

IMPLEMENT_DYNCREATE(CUsbSA44BView, CRichEditView)

BEGIN_MESSAGE_MAP(CUsbSA44BView, CRichEditView)		
	ON_WM_CONTEXTMENU()
	ON_WM_RBUTTONUP()
END_MESSAGE_MAP()

// CUsbSA44BView construction/destruction

CUsbSA44BView::CUsbSA44BView():
	m_nMaxLogLines(10000)
{
	// TODO: add construction code here

}

CUsbSA44BView::~CUsbSA44BView()
{
}

BOOL CUsbSA44BView::PreCreateWindow(CREATESTRUCT& cs)
{
	BOOL bReturn = CRichEditView::PreCreateWindow(cs);
	cs.style = AFX_WS_DEFAULT_VIEW | WS_VSCROLL | ES_AUTOHSCROLL |
		ES_AUTOVSCROLL | ES_MULTILINE | ES_NOHIDESEL | ES_READONLY;
	return bReturn;
}

void CUsbSA44BView::OnRButtonUp(UINT /* nFlags */, CPoint point)
{
	ClientToScreen(&point);
	OnContextMenu(this, point);
}

void CUsbSA44BView::OnContextMenu(CWnd* /* pWnd */, CPoint point)
{
#ifndef SHARED_HANDLERS
	theApp.GetContextMenuManager()->ShowPopupMenu(IDR_POPUP_EDIT, point.x, point.y, this, TRUE);
#endif
}


// CUsbSA44BView diagnostics

#ifdef _DEBUG
void CUsbSA44BView::AssertValid() const
{
	CRichEditView::AssertValid();
}

void CUsbSA44BView::Dump(CDumpContext& dc) const
{
	CRichEditView::Dump(dc);
}

CUsbSA44BDoc* CUsbSA44BView::GetDocument() const // non-debug version is inline
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CUsbSA44BDoc)));
	return (CUsbSA44BDoc*)m_pDocument;
}
#endif //_DEBUG


// CUsbSA44BView message handlers
void CUsbSA44BView::DoAppendToLog(LPCTSTR szTime, LPCTSTR lpszMessage, enumTypeMsgLevel typeMsgLevel /*= typeMsgLevelNormal*/)
{
	CRichEditCtrl& edit = GetRichEditCtrl();

	try
	{
		// Implement hysterisis delete for efficiency
		const auto nLineCount = edit.GetLineCount();
		if (nLineCount>m_nMaxLogLines)
		{
			const auto nLastIndex = (int)floor(m_nMaxLogLines * 0.10); // remove 10% of the lines

			const auto nBeginCharIndex = 0; // select the first character of the first line index
											// find the 100th line index first character index, and subtract 1
											// ie, select the characters that make up the first 100 lines, which means line index 0-99
			const auto nEndCharIndex = edit.LineIndex(nLastIndex) - 1; // select last character of the 99th line index

																	   // delete the first 100 line
			edit.SetSel(nBeginCharIndex, nEndCharIndex);
			edit.ReplaceSel(_T(""));

		}
	}
	catch (...)
	{
	}

	edit.SetSel(-1, -1); // make sure next insertion point is at the end

	CString strHeader;
	strHeader = _T("\r\n[ ");
	strHeader += szTime;

	switch (typeMsgLevel)
	{
	case typeMsgLevelWarning:
		strHeader += _T(" W ] ");
		break;
	case typeMsgLevelError:
		strHeader += _T(" E ] ");
		break;
	case typeMsgLevelCriticalError:
		strHeader += _T("CE ] ");
		break;
	default:
		strHeader += _T(" ] ");
		break;
	}

	// Avoid leaving it locked on crash, use try/catch
	try
	{

		CHARFORMAT cf = { sizeof(cf) };

		// Set insertion point to end of text
		//edit.SetSel(-1,-1);

		// Initialize character format structure
		cf.dwMask = CFM_COLOR | CFM_BOLD | CFM_FACE;
		cf.crTextColor = RGB(0, 0, 0xFF)/*CFE_AUTOCOLOR*/; // blue
		cf.dwEffects = 0;
		_tcscpy_s(cf.szFaceName, _T("Courier New"));

		//  Set the character format
		VERIFY(edit.SetSelectionCharFormat(cf));

		edit.ReplaceSel(strHeader);

		switch (typeMsgLevel)
		{
		case typeMsgLevelError:
		case typeMsgLevelCriticalError:
			cf.crTextColor = RGB(0xFF, 0, 0); // Red
			break;
		case typeMsgLevelWarning:
			cf.crTextColor = RGB(0x00, 0xff, 0xff); // Cyan
			break;
		default:
			cf.crTextColor = RGB(0, 0, 0);  // black
			break;
		}

		//  Set the character format
		VERIFY(edit.SetSelectionCharFormat(cf));

		edit.ReplaceSel(lpszMessage);

		switch (typeMsgLevel)
		{
		case typeMsgLevelError:
		case typeMsgLevelCriticalError:
			MessageBeep(MB_ICONSTOP);
			break;
		case typeMsgLevelWarning:
			MessageBeep(MB_ICONWARNING);
			break;
		}
	}
	catch (...)
	{
	}
}

void CUsbSA44BView::DoAppendToLog(CMutex& mutexNewLogMsg, std::queue<CAppendToLogParams*>& newLogMsg)
{
	auto& edit = GetRichEditCtrl();
	edit.SetRedraw(FALSE);

	CSingleLock sl(&mutexNewLogMsg);

	while (true)
	{
		sl.Lock();
		auto bDone = newLogMsg.empty(); // must be locked when we check empty status

		if (bDone)
		{
			sl.Unlock();
			break;
		}

		const auto pAppendToLogParams = newLogMsg.front();
		newLogMsg.pop(); // must be locked when we pop

		sl.Unlock();

		if (pAppendToLogParams)
		{
			DoAppendToLog(pAppendToLogParams->strTime, pAppendToLogParams->strMessage, pAppendToLogParams->typeMsgLevel);
			delete pAppendToLogParams;
		}
	}

	edit.SetRedraw();
	edit.Invalidate(FALSE);
	//edit.UpdateWindow();
}
