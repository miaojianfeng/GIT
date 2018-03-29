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
#include "CntrItem.h"
#include "resource.h"
#include "UsbSA44BView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CUsbSA44BView

IMPLEMENT_DYNCREATE(CUsbSA44BView, CRichEditView)

BEGIN_MESSAGE_MAP(CUsbSA44BView, CRichEditView)
	ON_WM_DESTROY()
	// Standard printing commands
	ON_COMMAND(ID_FILE_PRINT, &CRichEditView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_DIRECT, &CRichEditView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_PREVIEW, &CUsbSA44BView::OnFilePrintPreview)
	ON_WM_CONTEXTMENU()
	ON_WM_RBUTTONUP()
END_MESSAGE_MAP()

// CUsbSA44BView construction/destruction

CUsbSA44BView::CUsbSA44BView()
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

void CUsbSA44BView::OnInitialUpdate()
{
	CRichEditView::OnInitialUpdate();


	// Set the printing margins (720 twips = 1/2 inch)
	SetMargins(CRect(720, 720, 720, 720));
}


// CUsbSA44BView printing


void CUsbSA44BView::OnFilePrintPreview()
{
#ifndef SHARED_HANDLERS
	AFXPrintPreview(this);
#endif
}

BOOL CUsbSA44BView::OnPreparePrinting(CPrintInfo* pInfo)
{
	// default preparation
	return DoPreparePrinting(pInfo);
}


void CUsbSA44BView::OnDestroy()
{
	// Deactivate the item on destruction; this is important
	// when a splitter view is being used
   COleClientItem* pActiveItem = GetDocument()->GetInPlaceActiveItem(this);
   if (pActiveItem != NULL && pActiveItem->GetActiveView() == this)
   {
      pActiveItem->Deactivate();
      ASSERT(GetDocument()->GetInPlaceActiveItem(this) == NULL);
   }
   CRichEditView::OnDestroy();
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
