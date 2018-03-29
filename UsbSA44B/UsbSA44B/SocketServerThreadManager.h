#pragma once

class CAVLZollnerDoc;

class CSocketServerThreadManager
{
public:
   CSocketServerThreadManager();
   ~CSocketServerThreadManager(void);

public:
	int m_nPort;

   bool m_bServerRunning;

public:
   void Start();
   void Stop();
};

