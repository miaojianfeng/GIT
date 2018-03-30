#pragma once
#include "SocketServerThreadManager.h"
#include <afxrich.h>

class CUsbSA44BDoc : public CRichEditDoc
{
protected: // create from serialization only
	CUsbSA44BDoc();
	DECLARE_DYNCREATE(CUsbSA44BDoc)

// Attributes
public:

// Operations
public:
	void StartThreads();
	void ShowProgram();
	virtual void PreCloseFrame(CFrameWnd* pFrameWnd);
	BOOL IsClosing() { return m_bClosing; }

private:
	void CloseAllSockets();

private:
	CSocketServerThreadManager m_serverSA;

// Overrides
public:
	virtual BOOL OnNewDocument();
	virtual void Serialize(CArchive& ar);	

	virtual void DeleteContents();
	//virtual BOOL CanCloseFrame(CFrameWnd* pFrame);
	virtual CRichEditCntrItem* CreateClientItem(REOBJECT* preo) const;

	//virtual CRichEditCntrItem* CreateClientItem(REOBJECT* preo) const;

#ifdef SHARED_HANDLERS
	virtual void InitializeSearchContent();
	virtual void OnDrawThumbnail(CDC& dc, LPRECT lprcBounds);
#endif // SHARED_HANDLERS

// Implementation
public:
	virtual ~CUsbSA44BDoc();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:
	virtual BOOL SaveModified();

// Generated message map functions
protected:
	DECLARE_MESSAGE_MAP()

#ifdef SHARED_HANDLERS
	// Helper function that sets search content for a Search Handler
	void SetSearchContent(const CString& value);
#endif // SHARED_HANDLERS

public:
	afx_msg void OnUpdateTestButtons(CCmdUI *pCmdUI);
	afx_msg void OnUpdateOpenSocket(CCmdUI *pCmdUI);
};
