// UsbSA44BDoc.cpp : implementation of the CUsbSA44BDoc class
//

#include "stdafx.h"
// SHARED_HANDLERS can be defined in an ATL project implementing preview, thumbnail
// and search filter handlers and allows sharing of document code with that project.
#ifndef SHARED_HANDLERS
#include "UsbSA44B.h"
#endif

#include "UsbSA44BDoc.h"
#include "UsbSA44BView.h"
#include "MainFrm.h"

#include <propkey.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// CUsbSA44BDoc

IMPLEMENT_DYNCREATE(CUsbSA44BDoc, CRichEditDoc)

BEGIN_MESSAGE_MAP(CUsbSA44BDoc, CRichEditDoc)
	// Enable default OLE container implementation
	ON_UPDATE_COMMAND_UI(ID_OLE_EDIT_LINKS, &CRichEditDoc::OnUpdateEditLinksMenu)
	ON_UPDATE_COMMAND_UI(ID_OLE_VERB_POPUP, &CUsbSA44BDoc::OnUpdateObjectVerbPopup)
	ON_COMMAND(ID_OLE_EDIT_LINKS, &CRichEditDoc::OnEditLinks)
	ON_UPDATE_COMMAND_UI_RANGE(ID_OLE_VERB_FIRST, ID_OLE_VERB_LAST, &CRichEditDoc::OnUpdateObjectVerbMenu)
END_MESSAGE_MAP()

static CString strAppConnectionSettings = _T("ServerConnection");
static CString strEntryIPAddress = _T("IPAddress");
static CString strEntryPort = _T("Port");

// CUsbSA44BDoc construction/destruction

CUsbSA44BDoc::CUsbSA44BDoc()
{
	ASSERT(!theApp.m_pDoc);
	theApp.m_pDoc = this; // Link back to app for Automation Object tracking
}

CUsbSA44BDoc::~CUsbSA44BDoc()
{
	theApp.m_pDoc = NULL; // close the link
}

BOOL CUsbSA44BDoc::OnNewDocument()
{
	if (!CRichEditDoc::OnNewDocument())
		return FALSE;

	// TODO: add reinitialization code here
	// (SDI documents will reuse this document)

	return TRUE;
}

CRichEditCntrItem* CUsbSA44BDoc::CreateClientItem(REOBJECT* preo) const
{
	// cast away constness of this
	return NULL;
}

// CUsbSA44BDoc serialization

void CUsbSA44BDoc::Serialize(CArchive& ar)
{
	if (ar.IsStoring())
	{
		for (POSITION pos = GetFirstViewPosition(); pos != NULL;)
		{
			CView* pView = GetNextView(pos);
			CUsbSA44BView* pUsbSA44BView = DYNAMIC_DOWNCAST(CUsbSA44BView, pView);

			if (pUsbSA44BView != NULL)
			{
				m_bRTF = FALSE; // save as text
				CRichEditDoc::Serialize(ar);
			}
		}
	}
}

#ifdef SHARED_HANDLERS

// Support for thumbnails
void CUsbSA44BDoc::OnDrawThumbnail(CDC& dc, LPRECT lprcBounds)
{
	// Modify this code to draw the document's data
	dc.FillSolidRect(lprcBounds, RGB(255, 255, 255));

	CString strText = _T("TODO: implement thumbnail drawing here");
	LOGFONT lf;

	CFont* pDefaultGUIFont = CFont::FromHandle((HFONT) GetStockObject(DEFAULT_GUI_FONT));
	pDefaultGUIFont->GetLogFont(&lf);
	lf.lfHeight = 36;

	CFont fontDraw;
	fontDraw.CreateFontIndirect(&lf);

	CFont* pOldFont = dc.SelectObject(&fontDraw);
	dc.DrawText(strText, lprcBounds, DT_CENTER | DT_WORDBREAK);
	dc.SelectObject(pOldFont);
}

// Support for Search Handlers
void CUsbSA44BDoc::InitializeSearchContent()
{
	CString strSearchContent;
	// Set search contents from document's data. 
	// The content parts should be separated by ";"

	// For example:  strSearchContent = _T("point;rectangle;circle;ole object;");
	SetSearchContent(strSearchContent);
}

void CUsbSA44BDoc::SetSearchContent(const CString& value)
{
	if (value.IsEmpty())
	{
		RemoveChunk(PKEY_Search_Contents.fmtid, PKEY_Search_Contents.pid);
	}
	else
	{
		CMFCFilterChunkValueImpl *pChunk = NULL;
		ATLTRY(pChunk = new CMFCFilterChunkValueImpl);
		if (pChunk != NULL)
		{
			pChunk->SetTextValue(PKEY_Search_Contents, value, CHUNK_TEXT);
			SetChunkValue(pChunk);
		}
	}
}

#endif // SHARED_HANDLERS

// CUsbSA44BDoc diagnostics

#ifdef _DEBUG
void CUsbSA44BDoc::AssertValid() const
{
	CRichEditDoc::AssertValid();
}

void CUsbSA44BDoc::Dump(CDumpContext& dc) const
{
	CRichEditDoc::Dump(dc);
}
#endif //_DEBUG

void CUsbSA44BDoc::StartThreads()
{
	m_serverSA.Start();
}

BOOL CUsbSA44BDoc::SaveModified()
{
	return TRUE; // don't bother saving...let user close us
}

void CUsbSA44BDoc::ShowProgram()
{
	POSITION pos = GetFirstViewPosition();
	CView* pView = GetNextView(pos);
	if (pView != NULL)
	{
		CFrameWnd* pFrameWnd = pView->GetParentFrame();
		pFrameWnd->ActivateFrame(SW_SHOW);
		pFrameWnd = pFrameWnd->GetParentFrame();
		if (pFrameWnd != NULL)
			pFrameWnd->ActivateFrame(SW_SHOW);
	}
}

void CUsbSA44BDoc::PreCloseFrame(CFrameWnd* pFrameWnd)
{
	CloseAllSockets();

	CRichEditDoc::PreCloseFrame(pFrameWnd);
}

void CUsbSA44BDoc::CloseAllSockets()
{
	m_serverSA.Stop();	
}

void CUsbSA44BDoc::DeleteContents()
{
	CloseAllSockets();

	CRichEditDoc::DeleteContents();
}