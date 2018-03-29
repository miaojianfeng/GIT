#pragma once
#include <wtypes.h>
#include <string>

#define AnsiString std::string
#define TThread CObject

#define Variant _variant_t
#define String std::string

#define __FUNC__  __func__

char* ConvertBSTRToLPSTR (BSTR bstrIn);

std::string str_printf(const char* const format, ...);