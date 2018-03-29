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

// CntrItem.cpp : implementation of the CUsbSA44BCntrItem class
//

#include "stdafx.h"
#include "UsbSA44B.h"

#include "UsbSA44BDoc.h"
#include "UsbSA44BView.h"
#include "CntrItem.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CUsbSA44BCntrItem implementation

IMPLEMENT_SERIAL(CUsbSA44BCntrItem, CRichEditCntrItem, 0)

CUsbSA44BCntrItem::CUsbSA44BCntrItem(REOBJECT* preo, CUsbSA44BDoc* pContainer)
	: CRichEditCntrItem(preo, pContainer)
{
	// TODO: add one-time construction code here
}

CUsbSA44BCntrItem::~CUsbSA44BCntrItem()
{
	// TODO: add cleanup code here
}


// CUsbSA44BCntrItem diagnostics

#ifdef _DEBUG
void CUsbSA44BCntrItem::AssertValid() const
{
	CRichEditCntrItem::AssertValid();
}

void CUsbSA44BCntrItem::Dump(CDumpContext& dc) const
{
	CRichEditCntrItem::Dump(dc);
}
#endif

