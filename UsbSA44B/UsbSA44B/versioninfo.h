#pragma once

/////////////////////////////////////////////////////////////////////////////
// CVersionInfo

class CVersionInfo : public CObject
{
	DECLARE_DYNAMIC(CVersionInfo)
public:
	CVersionInfo();
	void CleanUp();

// Attributes
private:
	// Structure used to store enumerated languages and code pages.
	struct LANGANDCODEPAGE
	{
		WORD wLanguage;
		WORD wCodePage;
	} *m_pTranslate;

	LPBYTE m_pVerInfo;
	
	BOOL Initialize();

// Operations
public:
	CString GetVersionString(LPCTSTR lpszQuery);
	BOOL GetVersionNumber(WORD* pwMajorVer, WORD* pwMinorVer, WORD* pwBuild, WORD* pwQFE);
	static CString StaticGetVersionString(LPCTSTR lpszQuery);
	static BOOL StaticGetVersionNumber(WORD* pwMajorVer, WORD* pwMinorVer, WORD* pwBuild, WORD* pwQFE);
	static CString StaticGetCompanyName();	// calls GetVersionString

// Implementation
public:
	virtual ~CVersionInfo();

protected:
};
