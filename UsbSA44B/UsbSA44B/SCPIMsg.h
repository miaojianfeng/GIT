#pragma once

#include <vector>
#include <queue>

// CSCPIMsg command target

class CSCPIMsg : public CObject
{
protected:
	DECLARE_DYNCREATE(CSCPIMsg)
public:
	CSCPIMsg();

// Attributes
public:
   std::queue<std::vector<unsigned char>> m_msgQueue;

private:
   std::vector<unsigned char> m_msgPartial;
   bool m_bBinaryMode;

// Operations
public:
	void Init();

// Implementation
public:
	virtual ~CSCPIMsg();
	void ProcessReceiveBuffer(std::vector<unsigned char>& buffer, CSocket& socket);
   void ClearInternalBuffers()
   { 
      m_msgPartial.clear(); 
      while (!m_msgQueue.empty())
          m_msgQueue.pop();
   }

#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif


};

/////////////////////////////////////////////////////////////////////////////
