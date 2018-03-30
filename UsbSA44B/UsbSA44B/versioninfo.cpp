// verinfo.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "versioninfo.h"

#pragma comment(lib, "version.lib")

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#ifdef _DEBUG
#undef THIS_FILE
static char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#ifdef _WIN32
#include <winver.h>
#else
#include <ver.h>
#endif


/////////////////////////////////////////////////////////////////////////////
// CVersionInfo

IMPLEMENT_DYNAMIC(CVersionInfo, CObject)


CVersionInfo::CVersionInfo() :
	m_pVerInfo(NULL),
	m_pTranslate(NULL)
{
}

	
CVersionInfo::~CVersionInfo()
{
	CleanUp();
}


BOOL CVersionInfo::Initialize()
{
	static TCHAR szTranslation[] = _T("\\VarFileInfo\\Translation");

	CleanUp();
	
	TCHAR szFullPath[_MAX_PATH];
	::GetModuleFileName(AfxGetInstanceHandle(), szFullPath, sizeof(szFullPath));
	CString strFileName = szFullPath;

	if (strFileName.IsEmpty()) // no file
		return FALSE;

    LPTSTR lpszTempName = _tcsdup(strFileName); // temp copy to pass to GetFileVersionInfoSize
	if (lpszTempName == NULL)
		return FALSE;

	BOOL bResult = FALSE;
	DWORD dwHandle = 0;
	DWORD dwVerInfoSize = GetFileVersionInfoSize(lpszTempName, &dwHandle);

	if (dwVerInfoSize > 0)		// resource information exists.
	{
		// Allocate a memory block large enough to hold the version information.
		m_pVerInfo = new BYTE[dwVerInfoSize];

		if (m_pVerInfo != NULL)
		{
			bResult = GetFileVersionInfo(lpszTempName, dwHandle, dwVerInfoSize, m_pVerInfo);

			if (bResult)
			{
				UINT nLength = 0;
				bResult = VerQueryValue(m_pVerInfo, szTranslation, reinterpret_cast<LPVOID*>(&m_pTranslate), &nLength) &&
					m_pTranslate && nLength > 0;
			}

			if (!bResult)
				CleanUp();
		}
	}

	free(lpszTempName);
	return bResult;
}


void CVersionInfo::CleanUp()
{
	m_pTranslate = NULL;

	if (m_pVerInfo != NULL)
	{
		delete [] m_pVerInfo;
		m_pVerInfo = NULL;
	}
}


CString CVersionInfo::GetVersionString(LPCTSTR lpszQuery)// const
{
	static const TCHAR szSubBlock[] = _T("\\StringFileInfo\\%04x%04x\\%s");

	if (m_pVerInfo != NULL || Initialize())
	{
		TCHAR szName[512];
		wsprintf(szName, szSubBlock, m_pTranslate[0].wLanguage, m_pTranslate[0].wCodePage, lpszQuery);
		UINT nLength = 0;
		LPVOID lpData = NULL;

		if (VerQueryValue(m_pVerInfo, szName, &lpData, &nLength))
			return reinterpret_cast<LPTSTR>(lpData);
	}

	return _T("");
}

/*static*/CString CVersionInfo::StaticGetVersionString(LPCTSTR lpszQuery)
// static version just calls the non-static version.
{
	CVersionInfo versionInfo;
	CString strVersionString = versionInfo.GetVersionString(lpszQuery);

	return strVersionString;
		
}


BOOL CVersionInfo::GetVersionNumber(WORD* pwMajorVer, WORD* pwMinorVer, WORD* pwBuild, WORD* pwQFE)
{
	BOOL bResult = FALSE;

	if (m_pVerInfo != NULL || Initialize())
	{
		// Copy version numbers
		UINT nLen = 0;
		VS_FIXEDFILEINFO* pVer = NULL;

		bResult = VerQueryValue(m_pVerInfo, _T("\\"), reinterpret_cast<LPVOID*>(&pVer), &nLen) && (pVer != NULL);
		if (bResult)
		{
			if (pwMajorVer != NULL)
				*pwMajorVer = HIWORD( pVer->dwFileVersionMS );
			
			if (pwMinorVer != NULL)
				*pwMinorVer = LOWORD( pVer->dwFileVersionMS );
			
			if (pwBuild != NULL)
				*pwBuild = HIWORD( pVer->dwFileVersionLS );
			
			if (pwQFE != NULL)
				*pwQFE = LOWORD( pVer->dwFileVersionLS );
		}
	}

	return bResult;
}


/*static*/ BOOL CVersionInfo::StaticGetVersionNumber(WORD* pwMajorVer, WORD* pwMinorVer, WORD* pwBuild, WORD* pwQFE)
// static version just calls the non-static version.
{
	CVersionInfo versionInfo;
	return versionInfo.GetVersionNumber(pwMajorVer, pwMinorVer, pwBuild, pwQFE);
}

/*static*/ CString CVersionInfo::StaticGetCompanyName()
// static function just calls non-static version of GetVersionString() for the CompanyName.
{
	CVersionInfo versionInfo;
	CString strCompanyName = versionInfo.GetVersionString(_T("CompanyName"));
	return strCompanyName;
}

