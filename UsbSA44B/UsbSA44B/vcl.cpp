#include "stdafx.h"
#include "vcl.h"

char* ConvertBSTRToLPSTR (BSTR bstrIn)
   {
   	LPSTR pszOut = NULL;
   
   	if (bstrIn != NULL)
   	{
   		int nInputStrLen = SysStringLen (bstrIn);
   
   		// Double NULL Termination
   		int nOutputStrLen = WideCharToMultiByte(CP_ACP, 0, bstrIn, nInputStrLen, NULL, 0, 0, 0) + 2;	
   
   		pszOut = new char [nOutputStrLen];
   
   		if (pszOut)
   		{
   		    memset (pszOut, 0x00, sizeof (char)*nOutputStrLen);
   
 		 WideCharToMultiByte (CP_ACP, 0, bstrIn, nInputStrLen, pszOut, nOutputStrLen, 0, 0);
   		}
   	 }
   
   	return pszOut;
   }


/* from http://www.ohfuji.name/?p=4 */
#include <cstdio>
#include <cstdlib>
#include <cstdarg>
#include <vector>
#include <string>
#include <stdexcept>

std::string str_printf(const char* const format, ...)
{
  int bufsize = 1024;
  std::vector<char> buff(bufsize);
  va_list args;

  /// first try
  va_start(args, format);
  int vssize = vsnprintf( &buff[0], bufsize, format, args);
  va_end(args);

  /// if bufsize is sufficient, return
  if(vssize>=0 && vssize<bufsize) {
    buff.resize(vssize);
    return std::string(buff.begin(), buff.end());
  }

  if(vssize<0) throw std::runtime_error(format);

  /// resize and retry
  buff.resize(vssize + 1);

  va_start(args, format);
  vssize = vsnprintf(&buff[0], vssize + 1, format, args);
  va_end(args);

  if(vssize<0) throw std::runtime_error(format);
  buff.resize(vssize);

  return std::string(buff.begin(), buff.end());
}