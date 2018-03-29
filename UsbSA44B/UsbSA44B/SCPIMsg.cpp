// SCPIMsg.cpp : implementation file
//

#include "stdafx.h"
#include "SCPIMsg.h"

#ifdef _DEBUG
#undef THIS_FILE
static char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CSCPIMsg

IMPLEMENT_DYNCREATE(CSCPIMsg, CObject)

/////////////////////////////////////////////////////////////////////////////
// CSCPIMsg construction/destruction

CSCPIMsg::CSCPIMsg() :
m_bBinaryMode(false)
{
	Init();
}

CSCPIMsg::~CSCPIMsg()
{
}

/////////////////////////////////////////////////////////////////////////////
// CSCPIMsg Operations

void CSCPIMsg::Init()
{
   ClearInternalBuffers();
}

/////////////////////////////////////////////////////////////////////////////
// CSCPIMsg serialization


void CSCPIMsg::ProcessReceiveBuffer(std::vector<unsigned char>& buffer, CSocket& socket)
{
   if (m_bBinaryMode)
   {
      ASSERT(FALSE); // need custom processing for binary
      return;
   }

   for(auto it = buffer.begin(); it!=buffer.end(); ++it)
	{
      const auto byInput = *it;

		// Message terminates with LF, CR
		if (  (byInput==0x0A 
               || byInput==0x0D 
               /*|| byInput==0x3B*/)  // warning! using ';' as the buffer terminator will confuse the parser
               )
		{
         // Found a terminator
         if (!m_msgPartial.empty()) // message is not null
         {
            m_msgQueue.push(m_msgPartial); // put it on the list
            m_msgPartial.clear();
         }
         continue;
		}

      m_msgPartial.push_back(byInput);
	}
}

/////////////////////////////////////////////////////////////////////////////
// CSCPIMsg diagnostics

#ifdef _DEBUG
void CSCPIMsg::AssertValid() const
{
	CObject::AssertValid();
}

void CSCPIMsg::Dump(CDumpContext& dc) const
{
	CObject::Dump(dc);
}
#endif //_DEBUG
