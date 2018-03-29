/**************************************************************************************/
/* Source Code Module for JPA-SCPI PARSER V1.3.1																			*/
/*																																										*/
/* (C) JPA Consulting Ltd., 2004	(www.jpacsoft.com)																	*/
/*																																										*/
/* View this file with tab spacings set to 2																					*/
/*																																										*/
/* scpi.c																																							*/
/* ======																																							*/
/*																																										*/
/* Module Description																																	*/
/* ------------------																																	*/
/* Main implementation source code module of JPA-SCPI Parser													*/
/*																																										*/
/* Do not modify this file except where instructed in the code and/or documentation		*/
/*																																										*/
/* Refer to the JPA-SCPI Parser documentation for instructions as to using this code	*/
/* and notes on its design.																														*/
/*																																										*/
/* JPA-SCPI Parser Revision History																										*/
/* --------------------------------																										*/
/* Refer to scpi.h for revision history																								*/
/**************************************************************************************/

#include  <limits.h>

/* USER: Include any headers required by your compiler here														*/
#include "cmds.h"
#include "scpi.h"


/**************************************************************************************/
/* Constants used in this module																											*/
/**************************************************************************************/
#define CMD_SEP									';'						/* Symbol used to separate commands within the Input String 						 */
#define CMD_ROOT								':'						/* Symbol used to tell SCPI parser to reset to root of Command Tree 		 */
#define KEYW_SEP								':'						/* Symbol used to separate keywords within a command 										 */
#define PARAM_SEP								','						/* Symbol used to separate parameters within the Input Parameters String */
#define SINGLE_QUOTE						'\''					/* Single quote symbol used to delimit quoted strings 									 */
#define DOUBLE_QUOTE						'"'						/* Double quote symbol used to delimit quoted strings 									 */
#define OPEN_BRACKET						'('						/* Symbol used as opening bracket at start of an Expression parameter 	 */
#define CLOSE_BRACKET						')'						/* Symbol used as closing bracket at end of an Expression parameter 	 	 */
#define ENTRY_SEP								','						/* Symbol used to separate entries in a numeric list or a channel list 	 */
#define RANGE_SEP								':'						/* Symbol used to separate start and finish values of an entry in a list */
#define DIM_SEP									'!'						/* Symbol used to separate mulit-dimensioned entries in a list					 */
#define NUM_SUF_SYMBOL					'#'						/* Symbol used to represent a numeric suffix in command keywords spec	   */
#define CMD_COMMON_START				'*'						/* Symbol used as first character of common command */

#define BOOL_ON									"ON"					/* Textual representation of the Boolean value ON  (1) */
#define BOOL_OFF								"OFF"					/* Textual representation of the Boolean value OFF (0) */
#define BOOL_ON_LEN							(2)
#define BOOL_OFF_LEN						(3)

#define MAX_EXPONENT						(43)					/* Largest value of exponent allowed  */
#define MIN_EXPONENT						(-43)					/* Smallest value of exponent allowed */

#define BASE_BIN								(2)						/* Number bases that can be used in SCPI */
#define BASE_OCT								(8)
#define BASE_DEC								(10)
#define BASE_HEX								(16)

/* Types of Keyword */
enum enKeywordType
{
	KEYWORD_COMMAND,														/* Keyword in command section					 */
	KEYWORD_CHAR_DATA														/* Keyword in character data parameter */
};

const struct strSpecAttrNumericVal sSpecAttrBoolNum = {U_NONE, (const enum enUnits *)0, 0};
																							/* Parameter Spec's Numeric Value attributes used to translate
																							   Numeric Values into Booleans 															 */


/**************************************************************************************/
/* Module-level Variables																															*/
/**************************************************************************************/
const char *mSCommandTree;										/* Pointer to start of command tree															 */
SCPI_CHAR_IDX mCommandTreeSize;								/* Size of command tree, in chars (including square brackets)		 */
SCPI_CHAR_IDX mCommandTreeLen;								/* Length of command tree (size minus number of square brackets) */


/**************************************************************************************/
/* Forward declarations of private functions																					*/
/**************************************************************************************/
#ifdef SUPPORT_NUM_SUFFIX
UCHAR ParseSingleCommand (char *SInpCmd, SCPI_CHAR_IDX InpCmdLen, BOOL bResetTree,
 SCPI_CMD_NUM *pCmdSpecNum, struct strParam sParam[], UCHAR *pNumSufCnt, unsigned int uiNumSuf[]);
#else
UCHAR ParseSingleCommand (char *SInpCmd, SCPI_CHAR_IDX InpCmdLen, BOOL bResetTree,
 SCPI_CMD_NUM *pCmdSpecNum, struct strParam sParam[]);
#endif
SCPI_CHAR_IDX LenOfKeywords (char *SInpCmd, SCPI_CHAR_IDX InpCmdLen);
#ifdef SUPPORT_NUM_SUFFIX
UCHAR KeywordsMatchSpec (const char *SSpec, SCPI_CHAR_IDX LenSpec, char *SInp, SCPI_CHAR_IDX LenInp, enum enKeywordType eKeyword,
 unsigned int uiNumSuf[], UCHAR *pNumSuf);
#else
UCHAR KeywordsMatchSpec (const char *SSpec, SCPI_CHAR_IDX LenSpec, char *SInp, SCPI_CHAR_IDX LenInp, enum enKeywordType eKeyword);
#endif
SCPI_CHAR_IDX SkipToNextRequiredChar (const char *SSpec, SCPI_CHAR_IDX Pos);
SCPI_CHAR_IDX SkipToEndOfOptionalKeyword (const char *SSpec, SCPI_CHAR_IDX Pos);
UCHAR MatchesParamsCount (SCPI_CMD_NUM CmdSpecNum, UCHAR InpCmdParamsCnt);
void GetParamsInfo (char *SInpCmd, SCPI_CHAR_IDX InpCmdLen, SCPI_CHAR_IDX InpCmdKeywordsLen, UCHAR *pParamsCnt, char **SParams,
 SCPI_CHAR_IDX *pParamsLen);
#ifdef SUPPORT_NUM_SUFFIX
UCHAR TranslateParameters (SCPI_CMD_NUM CmdSpecNum, char *SInpParams,
 SCPI_CHAR_IDX InpParamsLen, struct strParam sParam[], unsigned int uiNumSuf[], UCHAR *pNumSuf);
#else
UCHAR TranslateParameters (SCPI_CMD_NUM CmdSpecNum, char *SInpParams, SCPI_CHAR_IDX InpParamsLen,
 struct strParam sParam[]);
#endif
#ifdef SUPPORT_NUM_SUFFIX
UCHAR TranslateCharDataParam (char *SParam, SCPI_CHAR_IDX ParLen,
	const struct strSpecParam *psSpecParam, struct strParam *psParam, unsigned int uiNumSuf[], UCHAR *pNumSuf);
#else
UCHAR TranslateCharDataParam (char *SParam, SCPI_CHAR_IDX ParLen,
	const struct strSpecParam *psSpecParam, struct strParam *psParam);
#endif
UCHAR TranslateBooleanParam (char *SParam, SCPI_CHAR_IDX ParLen, const struct strSpecAttrBoolean *psSpecAttr,
 struct strParam *psParam);
UCHAR TranslateNumericValueParam (char *SParam, SCPI_CHAR_IDX ParLen, const struct strSpecAttrNumericVal *psSpecAttr,
 struct strParam *psParam);
UCHAR TranslateNumber (char *SNum, SCPI_CHAR_IDX Len, struct strAttrNumericVal *psNum, SCPI_CHAR_IDX *pNextPos);
UCHAR TranslateUnits (char *SUnits, SCPI_CHAR_IDX UnitsLen, const struct strSpecAttrNumericVal *pSpecAttr,
 enum enUnits *peUnits, signed char *pUnitExp);
UCHAR TranslateStringParam (char *SParam, SCPI_CHAR_IDX ParLen, const enum enParamType ePType, struct strParam *psParam);
#ifdef SUPPORT_EXPR
UCHAR TranslateExpressionParam (char *SParam, SCPI_CHAR_IDX ParLen, struct strParam *psParam);
#endif
#ifdef SUPPORT_NUM_LIST
UCHAR TranslateNumericListParam (char *SParam, SCPI_CHAR_IDX ParLen, const struct strSpecAttrNumList *pSpecAttr,
 struct strParam *psParam);
#endif
#ifdef SUPPORT_CHAN_LIST
UCHAR TranslateChannelListParam (char *SParam, SCPI_CHAR_IDX ParLen, const struct strSpecAttrChanList *pSpecAttr,
 struct strParam *psParam);
#endif
void ResetCommandTree (void);
void SetCommandTree (SCPI_CMD_NUM CmdSpecNum);
UCHAR CharFromFullCommand (char *SInpCmd, SCPI_CHAR_IDX Pos, SCPI_CHAR_IDX Len);
BOOL AppendToULong (unsigned long *pulVal, char Digit, UCHAR Base);
#ifdef SUPPORT_NUM_SUFFIX
BOOL AppendToUInt (unsigned int *puiVal, char Digit);
#endif
BOOL AppendToInt (int *piVal, char Digit);
BOOL StringsEqual (const char *S1, SCPI_CHAR_IDX Len1, const char *S2, SCPI_CHAR_IDX Len2);
UCHAR cabs (signed char Val);
long round (double fdVal);
BOOL iswhitespace (char c);

/* Substitutable Functions																		*/
/* -----------------------																		*/
/* Refer to text near end of module regarding these functions	*/
#if 0
	SCPI_CHAR_IDX strlen (const char *S);
	char tolower(char c);
	BOOL islower (char c);
	BOOL isdigit (char c);
#else
#include <string.h>
#include <ctype.h>
#endif


/**************************************************************************************/
/************************ JPA-SCPI PARSER ACCESS FUNCTIONS ****************************/
/**************************************************************************************/


/**************************************************************************************/
/* Parses an Input Command in the Input String.																				*/
/* If a match is found then returns number of matching Command Spec, and returns			*/
/* values and attributes of any Input Parameters.																			*/
/*																																										*/
/* Parameters:																																				*/
/*	[in/out] pSInput 	-	Pointer to first char of Input String to be parsed.						*/
/*											Input String must be a null-terminated string.								*/
/*											This is returned modified, so as to point to first char of		*/
/*											the next Input Command of the Input String to be parsed.			*/
/*	[in] bResetTree	 	-	If TRUE then the Command Tree is reset to the Root node;			*/
/*											if FALSE then the Command Tree stays at the node set by the		*/
/*											previous command.																							*/
/*											Note: Set this to TRUE when parsing the first command of the	*/
/*											Input String,	and FALSE otherwise.														*/
/*	[out] pCmdSpecNum - Pointer to returned number of the	Command Spec that matches		*/
/*											the Input Command in the Input String that was parsed.				*/
/*											Value is undefined if no matching Command Spec is found.			*/
/*	[out] sParam			-	Array [0..MAX_PARAM-1] of returned parameters containing the	*/
/*											parsed Input Parameter values and attributes.									*/
/*											Contents of returned parameters are undefined if no matching	*/
/*											Command Spec is found.																				*/
/*  ONLY PRESENT IF SUPPORT_NUM_SUFFIX IS DEFINED:																		*/
/*	[out] pNumSufCnt	- Pointer to returned count of numeric suffices encountered			*/
/*	[out] uiNumSuf		- Array [0..MAX_NUM_SUFFIX-1] of returned numeric suffices			*/
/*											present	in the Input Command.																	*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK:			A matching Command Spec was found.							*/
/*	SCPI_ERR_NO_COMMAND			- Error:	There was no command to be parsed in the Input 	*/
/*																		String.																					*/
/*	SCPI_ERR_INVALID_CMD		- Error:	The Input Command keywords did not match any 		*/
/*																		Command Spec command keywords.									*/
/*	SCPI_ERR_PARAM_CNT			- Error:	The Input Command keywords matched a Command		*/
/*																		Spec but the wrong number of parameter given in */
/*																		the Input Command for the Command Spec.					*/
/*	SCPI_ERR_PARAM_TYPE			- Error:	Parameter within the Input Command did not match*/
/*																		a	valid type of parameter in the Command Spec.	*/
/*	SCPI_ERR_PARAM_UNITS		- Error:	A parameter within the Input Command had the		*/
/*																		wrong type of units for the Command Spec.				*/
/*	SCPI_ERR_PARAM_OVERFLOW - Error:	The Input Command contained a parameter of type */
/*																		Numeric Value that could not be stored				``*/
/*																		internally. This occurs if the value has an 		*/
/*																		exponent greater than +/-43.										*/
/*	SCPI_ERR_UNMATCHED_BRACKET-Error: The parameters in the Input Command contained		*/
/*																		an unmatched bracket														*/
/*	SCPI_ERR_UNMATCHED_QUOTE- Error:	The parameters in the Input Command contained		*/
/*																		an unmatched single or double quote.						*/
/*	SCPI_ERR_TOO_MANY_NUM_SUF-Error:	Too many numeric suffices in Input Command to		*/
/*																		be returned in return parameter array.					*/
/*	SCPI_ERR_NUM_SUF_INVALID-	Error:  Numeric suffix in Input Command is invalid			*/
/*  SCPI_ERR_INVALID_VALUE	- Error:  One or more values in a numeric/channel list is	*/
/*																		invalid, e.g.	floating point when not allowed		*/
/*	SCPI_ERR_INVALID_DIMS		- Error:	Invalid number of dimensions in one of more			*/
/*																		of the channel list's entries										*/
/**************************************************************************************/
#ifdef SUPPORT_NUM_SUFFIX
UCHAR SCPI_Parse (char **pSInput, BOOL bResetTree, SCPI_CMD_NUM *pCmdSpecNum, struct strParam sParam[],
 UCHAR *pNumSufCnt, unsigned int uiNumSuf[])
#else
UCHAR SCPI_Parse (char **pSInput, BOOL bResetTree, SCPI_CMD_NUM *pCmdSpecNum, struct strParam sParam[])
#endif
{
	UCHAR Err;
	SCPI_CHAR_IDX Pos = 0;
	char Ch; 
	BOOL bDone = FALSE;
	SCPI_CHAR_IDX Len = 0;
	BOOL bWithinQuotes = FALSE;
	BOOL bDoubleQuotes = FALSE;

	/* Set Pos to beginning of Input Command in Input String, skipping whitespace and command separators */
	while (!bDone)
	{
		Ch = (*pSInput)[Pos];											/* Read character from string											*/
		if ((Ch == CMD_SEP) || (iswhitespace (Ch))) /* If found command separator or whitespace			*/
			Pos++;																	/* then try next character												*/
		else																			/* If found any other character										*/
			bDone = TRUE;														/* then this is the first char of Input Command 	*/
	}

	/* Set Len to length of the Input Command */
	bDone = FALSE;
	while (!bDone)
	{
		Ch = (*pSInput)[Pos+Len];									/* Read char from string */
		switch (Ch)
		{
			case DOUBLE_QUOTE :												/* Double-quote encountered										*/
				if (!(bWithinQuotes && !bDoubleQuotes)) /* If it is not embedded within single-quotes */
				{
					bWithinQuotes = !bWithinQuotes;				/* then toggle the in/out-of-quotes state			*/
					bDoubleQuotes = TRUE;
				}
				Len++;
				break;
			case SINGLE_QUOTE :												/* Single-quote encountered 									*/
				if (!(bWithinQuotes && bDoubleQuotes))	/* If it is not embedded within double-quotes	*/
				{
					bWithinQuotes = !bWithinQuotes;				/* then toggle the in/out-of-quotes state			*/
					bDoubleQuotes = FALSE;
				}
				Len++;
				break;
			case CMD_SEP:														/* Found command separator											*/
				if (!bWithinQuotes)										/* and it's not inside quotes										*/
					bDone = TRUE;												/* so we've reached the end of the Input Command*/
				break;
			case '\0':															/* Found end of Input String										*/
				bDone = TRUE;													/* so we've reached the end of the Input Command*/
				break;
			default:																/* Any other character													*/
				Len++;

			if (!Len)																/* Prevent perpetual looping if Input Command exceeded limit */
				break;
		}
	}

	if (Len)																		/* If Input Command is not zero-length then parse it */
#ifdef SUPPORT_NUM_SUFFIX
		Err = ParseSingleCommand (&((*pSInput)[Pos]), Len, bResetTree, pCmdSpecNum, sParam, pNumSufCnt, uiNumSuf);
#else
		Err = ParseSingleCommand (&((*pSInput)[Pos]), Len, bResetTree, pCmdSpecNum, sParam);
#endif
	else																				/* If Input Command is zero-length		*/
		Err = SCPI_ERR_NO_COMMAND;								/* Return error - no command to parse */

	*pSInput = &((*pSInput)[Pos+Len]);					/* Set returned pointer to first char of next command in Input String */

	return Err;
}


/**************************************************************************************/
/* Returns the type of a parameter returned by SCPI_Parse().													*/
/* If parameter is type Numeric Value, then also returns its sub-type attributes.			*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] psParam -			Pointer to parameter returned by SCPI_Parse (must not be null)*/
/*	[out] pePType -			Pointer to returned type of parameter													*/
/*	[out] pNumSubtype - Pointer to returned parameter's	sub-type attributes,					*/
/*											if parameter is type Numeric Value														*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE			- OK (always)																										*/
/**************************************************************************************/
UCHAR SCPI_ParamType (struct strParam *psParam, enum enParamType *pePType, UCHAR *pNumSubtype)
{
	*pePType = psParam->eType;									/* Set returned type of parameter	*/

	if (psParam->eType == P_NUM)								/* If parameter is Numeric Value  */
	{
		*pNumSubtype = 0;
		if (psParam->unAttr.sNumericVal.bNeg)			/* If parameter value is negative 													*/
			*pNumSubtype |= SCPI_NUM_ATTR_NEG;			/* then set Negative sub-type attribute											*/
		if (psParam->unAttr.sNumericVal.bNegExp)	/* If parameter has a decimal-point (i.e. exponent is -ve)	*/
			*pNumSubtype |= SCPI_NUM_ATTR_REAL;			/* then set Real sub-type attribute													*/
	}

	return SCPI_ERR_NONE;
}


/**************************************************************************************/
/* Returns the units of a parameter of type Numeric Value.														*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] psParam -			Pointer to parameter returned by SCPI_Parse (must not be null)*/
/*	[out] peUnits -			Pointer to returned type of units of the parameter						*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK																											*/
/*	SCPI_ERR_PARAM_TYPE			- Error:	Parameter was not of type Numeric Value					*/
/**************************************************************************************/
UCHAR SCPI_ParamUnits (struct strParam *psParam, enum enUnits *peUnits)
{
	if (psParam->eType != P_NUM)								/* If parameter is not type Numeric Value */
		return SCPI_ERR_PARAM_TYPE;								/* return error code 												*/

	*peUnits = psParam->unAttr.sNumericVal.eUnits;	/* Return units of parameter */

	return SCPI_ERR_NONE;
}


/**************************************************************************************/
/* Converts a parameter of type Character Data into its Character Data Item Number.		*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] psParam -			Pointer to parameter returned by SCPI_Parse (must not be null)*/
/*	[out] pItemNum -		Pointer to returned parameter's Character Data Item Number		*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK																											*/
/*	SCPI_ERR_PARAM_TYPE			- Error:	Parameter was not of type Character Data				*/
/**************************************************************************************/
UCHAR SCPI_ParamToCharDataItem (struct strParam *psParam, UCHAR *pItemNum)
{
	if (psParam->eType != P_CH_DAT)							/* If parameter is not of type Character Data */
		return SCPI_ERR_PARAM_TYPE;								/* return error code													*/

	*pItemNum = (UCHAR)(psParam->unAttr.sCharData.ItemNum); /*	set returned Item Number */

	return SCPI_ERR_NONE;
}


/**************************************************************************************/
/* Converts a parameter of type Boolean into a BOOL																		*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] psParam -			Pointer to parameter returned by SCPI_Parse (must not be null)*/
/*	[out] pbVal -				Pointer to returned BOOL value																*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK																											*/
/*	SCPI_ERR_PARAM_TYPE			- Error:	Parameter was not of type Boolean								*/
/**************************************************************************************/
UCHAR SCPI_ParamToBOOL (struct strParam *psParam, BOOL *pbVal)
{
	if (psParam->eType != P_BOOL)								/* If parameter is not of type Boolean */
		return SCPI_ERR_PARAM_TYPE;								/* return error code									 */

	if (psParam->unAttr.sBoolean.bVal)					/* If parameter value is non-zero			 */
		*pbVal = TRUE;														/* then set returned value to TRUE		 */
	else
		*pbVal = FALSE;														/* else set returned value to FALSE		 */

	return SCPI_ERR_NONE;
}


/**************************************************************************************/
/* Converts a parameter of type Numeric Value into an unsigned integer.								*/
/* If parameter's value is negative, then sign is ignored. If parameter's value is		*/
/* real (non-integer) then digits after the decimal point are ignored.								*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] psParam -			Pointer to parameter returned by SCPI_Parse (must not be null)*/
/*	[out] puiVal -			Pointer to returned unsigned int value												*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK																											*/
/*	SCPI_ERR_PARAM_TYPE			- Error:	Parameter was not of type Numeric Value					*/
/*	SCPI_ERR_PARAM_OVERFLOW - Error:	Value cannot be stored in a variable of type 		*/
/*																		unsigned int																		*/
/**************************************************************************************/
UCHAR SCPI_ParamToUnsignedInt (struct strParam *psParam, unsigned int *puiVal)
{
	unsigned long ulVal;
	UCHAR Err;

	Err = SCPI_ParamToUnsignedLong (psParam, &ulVal);	/* Convert parameter to unsigned long int */
	if (Err == SCPI_ERR_NONE)										/* If conversion worked ok */
	{
		if (ulVal > UINT_MAX)											/* If value is too big to be stored in an unsigned int */
			Err = SCPI_ERR_PARAM_OVERFLOW;					/* then return error code															 */
		else
			*puiVal = (unsigned int)ulVal;					/* otherwise set returned value */
	}

	return Err;
}


/**************************************************************************************/
/* Converts a parameter of type Numeric Value into a signed integer										*/
/* If parameter's value is  real (non-integer) then digits after the decimal point		*/
/* are ignored.																																				*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] psParam -			Pointer to parameter returned by SCPI_Parse (must not be null)*/
/*	[out] piVal -				Pointer to returned signed int value													*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK																											*/
/*	SCPI_ERR_PARAM_TYPE			- Error:	Parameter was not of type Numeric Value					*/
/*	SCPI_ERR_PARAM_OVERFLOW - Error:	Value cannot be stored in a variable of type int*/
/**************************************************************************************/
UCHAR SCPI_ParamToInt (struct strParam *psParam, int *piVal)
{
	long lVal;
	UCHAR Err;

	Err = SCPI_ParamToLong (psParam, &lVal);		/* Convert parameter to a signed long int */

	if (Err == SCPI_ERR_NONE)										/* If conversion worked ok */
	{
		if ((lVal > INT_MAX) || (lVal < 0-INT_MAX))	/* If value is too big or too small to be stored in a signed int */
			Err = SCPI_ERR_PARAM_OVERFLOW;						/* then return error code																				 */
		else
			*piVal = (int)lVal;											/* otherwise set returned value */
	}

	return Err;
}


/**************************************************************************************/
/* Converts a parameter of type Numeric Value into an unsigned long										*/
/* If parameter's value is negative, then sign is ignored. If parameter's value is		*/
/* real (non-integer) then digits after the decimal point are ignored.								*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] psParam -			Pointer to parameter returned by SCPI_Parse (must not be null)*/
/*	[out] pulVal -			Pointer to returned unsigned long value												*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK																											*/
/*	SCPI_ERR_PARAM_TYPE			- Error:	Parameter was not of type Numeric Value					*/
/*	SCPI_ERR_PARAM_OVERFLOW - Error:	Value cannot be stored in a variable of type		*/
/*																		unsigned long																		*/
/**************************************************************************************/
UCHAR SCPI_ParamToUnsignedLong (struct strParam *psParam, unsigned long *pulVal)
{
	UCHAR Exp;
	UCHAR Err = SCPI_ERR_NONE;

	if (psParam->eType != P_NUM)								/* If parameter is not type Numeric Value */
		Err = SCPI_ERR_PARAM_TYPE;								/* then return error code									*/

	else
	{
		*pulVal = psParam->unAttr.sNumericVal.ulSigFigs;	/* Get significant-figures component of value */

		/* Scale according to exponent component of value */
		for (Exp = 0; Exp < cabs(psParam->unAttr.sNumericVal.Exp); Exp++)
		{
			if (psParam->unAttr.sNumericVal.bNegExp) /* If exponent is negative	*/
				*pulVal /= 10;												/* divide value by 10				*/
			else																		/* If exponent is positive 	*/
			{
				if (*pulVal <= ULONG_MAX/10)					/* If another multiplication will not cause an overflow */
					*pulVal *= 10;											/* then multiply value by 10														*/
				else
				{
					Err = SCPI_ERR_PARAM_OVERFLOW;			/* Overflow would occur if multiplied again, so return error */
					break;
				}
			}
		}
	}

	return Err;
}


/**************************************************************************************/
/* Converts a parameter of type Numeric Value into a signed long											*/
/* If parameter's value is  real (non-integer) then digits after the decimal point		*/
/* are ignored.																																				*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] psParam -			Pointer to parameter returned by SCPI_Parse (must not be null)*/
/*	[out] plVal -				Pointer to returned signed long value													*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK																											*/
/*	SCPI_ERR_PARAM_TYPE			- Error:	Parameter was not of type Numeric Value					*/
/*	SCPI_ERR_PARAM_OVERFLOW - Error:	Value cannot be stored in variable of type long */
/**************************************************************************************/
UCHAR SCPI_ParamToLong (struct strParam *psParam, long *plVal)
{
	unsigned long ulVal;
	UCHAR Err;

	Err = SCPI_ParamToUnsignedLong (psParam, &ulVal);		/* Convert parameter to an unsigned long int */

	if (Err == SCPI_ERR_NONE)										/* If conversion worked ok */
	{
		if (ulVal > LONG_MAX)											/* If value is too big to represent as a signed long */
			Err = SCPI_ERR_PARAM_OVERFLOW;					/* then return error code														 */
		else
		{
			if (psParam->unAttr.sNumericVal.bNeg) 	/* If value is negative								*/
				*plVal = 0 - (long)ulVal;							/* then make returned value negative	*/
			else
				*plVal = (long)ulVal;									/* else make returned value positive	*/
		}
	}
	return Err;
}


/**************************************************************************************/
/* Converts a parameter of type Numeric Value into a double-precision float						*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] psParam -			Pointer to parameter returned by SCPI_Parse (must not be null)*/
/*	[out] pfdVal -			Pointer to returned double value															*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK																											*/
/*	SCPI_ERR_PARAM_TYPE			- Error:	Parameter was not of type Numeric Value					*/
/**************************************************************************************/
UCHAR SCPI_ParamToDouble (struct strParam *psParam, double *pfdVal)
{
	UCHAR Exp;

	if (psParam->eType != P_NUM)								/* If parameter is not of type Numeric Value */
		return SCPI_ERR_PARAM_TYPE;								/* return error code													 */

	*pfdVal = (double)(psParam->unAttr.sNumericVal.ulSigFigs);	/* Get significant-figures component of value */

	/* Scale according to exponent component of value */
	for (Exp = 0; Exp < cabs(psParam->unAttr.sNumericVal.Exp); Exp++)
	{
		if (psParam->unAttr.sNumericVal.bNegExp)	/* If exponent is negative */
			*pfdVal /= 10;													/* divide value by 10			 */
		else																			/* If exponent is positive */
			*pfdVal *= 10;													/* multiply value by 10		 */
	}
	if (psParam->unAttr.sNumericVal.bNeg)				/* If parameter value is negative */
		*pfdVal = 0 - (*pfdVal);									/* then negate the returned value	*/

	return SCPI_ERR_NONE;
}


/**************************************************************************************/
/* Converts a parameter of type String, Unquoted String or Expression into a pointer	*/
/* to a string and a character count.																									*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] psParam -			Pointer to parameter returned by SCPI_Parse (must not be null)*/
/*	[out] pSString -		Returned pointer to an array of characters containing the			*/
/*											returned string. The array of characters is always within the */
/*											Input String that contained the Input Parameter, and so the 	*/
/*											Input String must still be valid when calling this function.	*/
/*	[out] pLen -				Pointer to number of characters that within returned string.	*/
/*	[out] pDelimiter -	Pointer to returned character that delimited the entered			*/
/*											string parameter, i.e. a double-quote (") or single quote (').*/
/*											This is not applicable if string parameter is unquoted.				*/
/*																																										*/
/* Return Code:																																				*/
/*	SCPI_ERR_NONE						- OK																											*/
/*	SCPI_ERR_PARAM_TYPE			- Error:	Parameter was not type String or Unquoted String*/
/**************************************************************************************/
UCHAR SCPI_ParamToString (struct strParam *psParam, char **pSString, SCPI_CHAR_IDX *pLen, char *pDelimiter)
{
	if ((psParam->eType != P_STR) && (psParam->eType != P_UNQ_STR) 	/* If parameter is not type String or Unquoted String */
#ifdef SUPPORT_EXPR
	 && (psParam->eType != P_EXPR)																	/* or Expression																			*/
#endif
#ifdef SUPPORT_NUM_LIST
	 && (psParam->eType != P_NUM_LIST)															/* or Numeric List																		*/
#endif
#ifdef SUPPORT_CHAN_LIST
	 && (psParam->eType != P_CHAN_LIST)															/* or Channel List																		*/
#endif
	 )
		return SCPI_ERR_PARAM_TYPE;																		/* return error code																	*/

	*pSString = psParam->unAttr.sString.pSString;			/* Set returned pointer to start of string */
	*pLen = psParam->unAttr.sString.Len;							/* Set returned length of string */
	*pDelimiter = psParam->unAttr.sString.Delimiter;	/* Set returned delimiter character */

	return SCPI_ERR_NONE;
}


#ifdef SUPPORT_NUM_LIST
/**************************************************************************************/
/* Retrieves an entry from a Numeric List parameter.																	*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] psParam -			Pointer to parameter returned by SCPI_Parse (must not be null)*/
/*	[in] Index				- Index (0..255) of entry to retrieve, where 0 is first entry		*/
/*	[out] pbRange			- Pointer to returned flag: 1=Entry is a range of values; 			*/
/*											0=Entry is a single value																			*/
/*	[out] psFirst			- Pointer to returned parameter containing entry's value				*/
/*											(or	first value in range if *pbRange==TRUE)										*/
/*	[out] psLast			- Pointer to returned parameter containing entry's last value		*/
/*											in range - only used if *pbRange==TRUE												*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK:			Entry converted ok.															*/
/*	SCPI_ERR_NO_ENTRY				- Error:	There was no entry to get - the index was				*/
/*																		beyond the end of the entries						 				*/
/*	SCPI_ERR_PARAM_TYPE			- Error:	Parameter was not type Numeric List							*/
/**************************************************************************************/
UCHAR SCPI_GetNumListEntry (struct strParam *psParam, UCHAR Index, BOOL *pbRange, struct strParam *psFirst,
 struct strParam *psLast)
{
	UCHAR Err = SCPI_ERR_NONE;
	SCPI_CHAR_IDX Pos;
	SCPI_CHAR_IDX Len;
	SCPI_CHAR_IDX NextPos;
	UCHAR EntryNum;
	char *pSStr;

	if (psParam->eType == P_NUM_LIST)						/* If parameter is type Numeric List */
	{
		Pos = 0;
		EntryNum = 0;
		pSStr = psParam->unAttr.sString.pSString;	/* Get pointer to start of Numeric List string */
		Len = psParam->unAttr.sString.Len;				/* Get length of Numeric List string */

		/* Loop through characters of Numeric List string until start of required entry is reached */
		while ((EntryNum < Index) && (Pos < Len))
		{
			if (pSStr[Pos] == ENTRY_SEP)						/* If encountered an entry separating symbol */
				EntryNum++;														/* then increment entry number 							 */
			Pos++;																	/* Go to next character in Numeric List string */
		}

		if (Pos >= Len)														/* If reached end of string without getting to required entry */
			Err = SCPI_ERR_NO_ENTRY;								/* then return error 																					*/
		else
		{
			if (TranslateNumber (&(pSStr[Pos]), Len-Pos, &(psFirst->unAttr.sNumericVal), &NextPos) == SCPI_ERR_NONE)
																							/* Translate number at start of entry into returned First value */
			{
				psFirst->eType = P_NUM;								/* Set returned parameter as type Numeric Value */
				*pbRange = FALSE;											/* Assume for now that entry is a single value, not a range */
				Pos += NextPos;												/* Go to first character after number */
				if (Pos < Len-1)
				{
					if (pSStr[Pos] == RANGE_SEP)				/* If character is a range separator symbol */
					{
						*pbRange = TRUE;									/* Entry is a range, not a single value */
						Pos++;														/* Go to character after range separator */

						if (TranslateNumber (&(pSStr[Pos]), Len-Pos, &(psLast->unAttr.sNumericVal), &NextPos) == SCPI_ERR_NONE)
																							/* Translate number after range separator into returned Last value */
							psLast->eType = P_NUM;
            else															/* If translation of number failed (can't happen with a valid Numeric List) */
							Err = SCPI_ERR_PARAM_TYPE;			/* then return error 							 */
					}
				}
			}
			else																		/* Translation of number failed - can't happen with a valid Numeric List */
				Err = SCPI_ERR_PARAM_TYPE;						/* Return error	*/
		}
	}
	else																				/* Parameter is not type Numeric List */
		Err = SCPI_ERR_PARAM_TYPE;								/* so return error									 	*/

	return Err;
}
#endif


#ifdef SUPPORT_CHAN_LIST
/**************************************************************************************/
/* Retrieves an entry from a Channel List parameter.																	*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] psParam 			-	Pointer to parameter returned by SCPI_Parse (must not be null)*/
/*	[in] Index				- Index (0..255) of entry to retrieve, where 0 is first entry		*/
/*	[in/out] pDims		- Inwards: Pointer to maximum dimensions possible in an entry;	*/
/*											returned as the number of dimensions in the entry							*/
/*	[out] pbRange			- Pointer to returned flag: 1=Entry is a range of values; 			*/
/*											0=Entry is a single value																			*/
/*	[out] sFirst[]		- Array [0..Dims-1] of returned parameters containing entry's		*/
/*											value (or	first value in range if *pbRange==TRUE)							*/
/*	[out] sLast[]			- Array [0..Dims-1] of returned parameters containing entry's		*/
/*											last value in range - only used if *pbRange==TRUE							*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK:			Entry converted ok.															*/
/*	SCPI_ERR_NO_ENTRY				- Error:	There was no entry to get - the index was				*/
/*																		beyond the end of the entries										*/
/*	SCPI_ERR_TOO_MANY_DIMS	- Error:	Too many dimensions in entry to be returned 		*/
/*																		in parameters.																	*/
/*	SCPI_ERR_PARAM_TYPE			- Error:	Parameter was not type Channel List							*/
/**************************************************************************************/
UCHAR SCPI_GetChanListEntry (struct strParam *psParam, UCHAR Index, UCHAR *pDims,
 BOOL *pbRange, struct strParam sFirst[], struct strParam sLast[])
{
	UCHAR Err = SCPI_ERR_NONE;
	SCPI_CHAR_IDX Pos;
	SCPI_CHAR_IDX Len;
	SCPI_CHAR_IDX NextPos;
	UCHAR EntryNum;
	char *pSStr;
	UCHAR Dim;
	struct strParam *psCurParam;

	if (psParam->eType == P_CHAN_LIST)					/* If parameter is type Channel List */
	{
		Pos = 0;
		EntryNum = 0;
		pSStr = psParam->unAttr.sString.pSString;	/* Get pointer to start of Channel List string */
		Len = psParam->unAttr.sString.Len;				/* Get length of Channel List string */

		/* Loop through characters of Numeric List string until start of required entry is reached */
		while ((EntryNum < Index) && (Pos < Len))
		{
			if (pSStr[Pos] == ENTRY_SEP)						/* If encountered an entry separating symbol */
				EntryNum++;														/* then increment entry number 							 */
			Pos++;																	/* Go to next character in Channel List string */
		}

		if (Pos >= Len)														/* If reached end of string without getting to required entry */
			Err = SCPI_ERR_NO_ENTRY;								/* then return error 																					*/

		else																			/* If reached start of required entry */
		{
			Dim = 0;
			*pbRange = FALSE;

			while ((Pos < Len) && (Err == SCPI_ERR_NONE))		/* Loop through all characters of entry or until error */
			{
				if (Dim < *pDims)											/* If current dimension is within allowed dimension count */
				{
					if (!*pbRange)											/* Set pointer to returned parameter to be populated */
						psCurParam = &(sFirst[Dim]);
					else
						psCurParam = &(sLast[Dim]);
				}
				else																	/* If reached too many dimensions */
				{
					Err = SCPI_ERR_TOO_MANY_DIMS;				/* then set error code						*/
					break;															/* and exit the loop							*/
				}

				if (TranslateNumber (&(pSStr[Pos]), Len-Pos, &(psCurParam->unAttr.sNumericVal), &NextPos) == SCPI_ERR_NONE)
																							/* Translate number */
				{
					psCurParam->eType = P_NUM;					/* And set returned parameter type as numeric value */

					Pos += NextPos;											/* Go to first character after number */
					if (Pos < Len-1)
					{
						switch (pSStr[Pos])
						{
							case DIM_SEP:										/* If character is a dimension separator */
								Dim++;												/* then increment dimension counter 		 */
								Pos++;												/* Go forward to next character */
								break;
							case RANGE_SEP:									/* If character is a range separator 	*/
								*pbRange = TRUE;							/* then now into second part of range */
								Dim = 0;											/* Reset dimension counter */
								Pos++;												/* Go forward to next character */
								break;
							case ENTRY_SEP:									/* If character is an entry separator */
								Pos = Len;										/* then set position to be end of parameter in order to exit the loop */
                break;
							default:												/* Any other character - this can't happen with a valid Channel List */
								Err = SCPI_ERR_PARAM_TYPE;		/* return error code in this case */
						}
					}
				}
				else																	/* Could not translate number - this can't happen with a valid Channel List */
					Err = SCPI_ERR_PARAM_TYPE;					/* return error code in this case */
			}
			*pDims = Dim + 1;												/* Return the number of dimensions in the entry */
		}
	}
	else																					/* Parameter is not type Numeric List */
		Err = SCPI_ERR_PARAM_TYPE;									/* so return error									 	*/

	return Err;
}
#endif


/**************************************************************************************/
/********************************* PRIVATE FUNCTIONS **********************************/
/**************************************************************************************/


/**************************************************************************************/
/* Parses a single Input Command.																											*/
/* Finds the Command Spec that matches the Input Command and returns its number.			*/
/* Also returns the parsed parameters of the Input Command, if there are any.					*/
/* If no match is found, then returns an error code indicating nature of mis-match.		*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] SInpCmd -			Pointer to start of Input Command to be parsed.								*/
/*	[in] InpCmdLen -		Number of characters that make up the Input Command.					*/
/*	[in] bResetTree -		If TRUE then resets the Command Tree to the root before				*/
/*											parsing the Input Command; if FALSE then keeps the Command		*/
/*											Tree at the node set by the previous command.									*/
/*											Note: Set bReset to TRUE when parsing the first Input Command */
/*											in the Input String, otherwise set to FALSE.									*/
/*	[out] pCmdSpecNum - Pointer to returned number of the Command Spec that	matches		*/
/*											the Input Command. This value is undefined if no match found. */
/*	[out] sParam			- Array [0..MAX_PARAM-1] of returned parameters parsed in the		*/
/*											Input Command. Their contents is undefined if no match found.	*/
/*  ONLY PRESENT IF SUPPORT_NUM_SUFFIX IS DEFINED:																		*/
/*	[out] pNumSufCnt	- Pointer to returned count of numeric suffices encountered			*/
/*	[out] uiNumSuf		- Array [0..MAX_NUM_SUFFIX-1] of returned numeric suffices			*/
/*											present in the Input Command.																	*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK:			A matching Command Spec was found								*/
/*	SCPI_ERR_INVALID_CMD		- Error:	The Input Command keywords did not match any		*/
/*																		Command Spec command keywords										*/
/*	SCPI_ERR_PARAM_CNT			- Error:	The Input Command keywords matched a Command		*/
/*																		Spec but the wrong number of parameters were 		*/
/*																		present for the Command Spec										*/
/*	SCPI_ERR_PARAM_TYPE			- Error:	A parameter within the Input Command did not 		*/
/*																		match the type of parameter in the Command Spec */
/*	SCPI_ERR_PARAM_UNITS		- Error:	A parameter within the Input Command had the 		*/
/*																		wrong type of units for the Command Spec				*/
/*	SCPI_ERR_PARAM_OVERFLOW - Error:	The Input Command contained a parameter of type */
/*																		Numeric Value that could not be stored. This		*/
/*																		occurs if the value has an exponent greater			*/
/*																		than +/-43.																			*/
/*	SCPI_ERR_UNMATCHED_BRACKET-Error: The parameters in the Input Command contained		*/
/*																		an unmatched bracket														*/
/*	SCPI_ERR_UNMATCHED_QUOTE- Error:	The parameters in the Input Command contained 	*/
/*																		an unmatched single or double quote							*/
/*	SCPI_ERR_TOO_MANY_NUM_SUF-Error:	Too many numeric suffices in Input Command to		*/
/*																		be returned in return parameter array.					*/
/*	SCPI_ERR_NUM_SUF_INVALID- Error:  Numeric suffix in Input Command is invalid			*/
/*  SCPI_ERR_INVALID_VALUE	- Error:  One or more values in a numeric/channel list is	*/
/*																		invalid, e.g.	floating point when not allowed		*/
/*	SCPI_ERR_INVALID_DIMS		- Error:	Invalid number of dimensions in one of more			*/
/*																		of the channel list's entries										*/
/**************************************************************************************/
#ifdef SUPPORT_NUM_SUFFIX
UCHAR ParseSingleCommand (char *SInpCmd, SCPI_CHAR_IDX InpCmdLen, BOOL bResetTree,
 SCPI_CMD_NUM *pCmdSpecNum, struct strParam sParam[], UCHAR *pNumSufCnt, unsigned int uiNumSuf[])
#else
UCHAR ParseSingleCommand (char *SInpCmd, SCPI_CHAR_IDX InpCmdLen, BOOL bResetTree,
 SCPI_CMD_NUM *pCmdSpecNum, struct strParam sParam[])
#endif
{
	UCHAR RetErr;
	UCHAR Err;
	SCPI_CMD_NUM CmdSpecNum;
	SCPI_CHAR_IDX InpCmdKeywordsLen;
	UCHAR InpCmdParamsCnt;
	char *SInpCmdParams;
	SCPI_CHAR_IDX InpCmdParamsLen;
#ifdef SUPPORT_NUM_SUFFIX
	UCHAR NumSufIndex;
#endif

	if (SInpCmd[0] == CMD_ROOT)									/* If Input Command starts with colon								*/
	{
		bResetTree = TRUE;												/* then in SCPI this means "reset command tree"			*/
		SInpCmd = &(SInpCmd[1]);									/* Set start of Input Command string to second char	*/
		InpCmdLen--;															/* and also decrement Input Command length					*/
	}

	if (bResetTree)															/* Reset the command tree if required */
		ResetCommandTree ();

	RetErr = SCPI_ERR_INVALID_CMD;

	InpCmdKeywordsLen = LenOfKeywords (SInpCmd, InpCmdLen);	/* Get length of keywords in Input Command (plus Command Tree) */
	GetParamsInfo (SInpCmd, InpCmdLen, InpCmdKeywordsLen, &InpCmdParamsCnt, &SInpCmdParams, &InpCmdParamsLen);
																													/* Get information about parameters in the Input Command */

	/* Loop thru all Command Specs until a match is found or all possible matches have failed */
	for (CmdSpecNum = 0; (RetErr != SCPI_ERR_NONE) && (SSpecCmdKeywords[CmdSpecNum][0]); CmdSpecNum++)
	{
#ifdef SUPPORT_NUM_SUFFIX
		NumSufIndex = 0;

		Err = KeywordsMatchSpec (SSpecCmdKeywords[CmdSpecNum], (unsigned char)strlen(SSpecCmdKeywords[CmdSpecNum]), SInpCmd,
		 InpCmdKeywordsLen, KEYWORD_COMMAND, uiNumSuf, &NumSufIndex);
#else
		Err = KeywordsMatchSpec (SSpecCmdKeywords[CmdSpecNum], strlen(SSpecCmdKeywords[CmdSpecNum]), SInpCmd,
		 InpCmdKeywordsLen, KEYWORD_COMMAND);
#endif

		if (Err == SCPI_ERR_NONE)									/* If the command keywords match */
		{
			Err = MatchesParamsCount (CmdSpecNum, InpCmdParamsCnt);
																							/* Compare number of parameters in Input Command with Command Spec */
			if (Err == SCPI_ERR_NONE)								/* If parameter counts match */
			{
#ifdef SUPPORT_NUM_SUFFIX
				Err = TranslateParameters (CmdSpecNum, SInpCmdParams, InpCmdParamsLen, sParam, uiNumSuf, &NumSufIndex);
#else
				Err = TranslateParameters (CmdSpecNum, SInpCmdParams, InpCmdParamsLen, sParam);
#endif																				/* Translate parameters in Input Command */

				if (Err == SCPI_ERR_NONE)							/* If parameters were translated ok 														*/
				{
					*pCmdSpecNum = CmdSpecNum;					/* set returned value to the number of the matching Command Spec */
#ifdef SUPPORT_NUM_SUFFIX
					*pNumSufCnt = NumSufIndex;					/* set returned value of numeric suffices 											 */
#endif																				/* Translate parameters in Input Command */
					Err = SCPI_ERR_NONE;
					if (SInpCmd[0] != CMD_COMMON_START)	/* If Input Command is not a common command */
						SetCommandTree (CmdSpecNum);			/* then set the Command Tree for use with the next Input Command  */
				}
			}
		}

		switch (Err)
		{
			case SCPI_ERR_NONE : RetErr = SCPI_ERR_NONE; break;		/* If a matching Command Spec was found then exit loop 		*/
			default						 : if (RetErr > Err) RetErr = Err;	/* If no match was found then set returned error code to
																															 the least significant error code encountered (error
																															 values are in order of significance)										*/
		}
	}

	return RetErr;
}


/**************************************************************************************/
/* Returns the length of the command keywords in an Input Command.										*/
/* The count includes the keywords of the current Command Tree.												*/
/* Note: The end of the command keywords is determined to be the first whitespace or	*/
/* when the end of the string is reached, whichever is occurs first.									*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] SInpCmd -			Pointer to start of Input Command string											*/
/*	[in] InpCmdLen -		Length of Input Command string																*/
/*																																										*/
/* Return Value:																																			*/
/*	Length of command keywords																												*/
/**************************************************************************************/
SCPI_CHAR_IDX LenOfKeywords (char *SInpCmd, SCPI_CHAR_IDX InpCmdLen)
{
	SCPI_CHAR_IDX Len;
	char Ch;

	Len = 0;
	do
	{
		Ch = CharFromFullCommand (SInpCmd, Len, mCommandTreeLen + InpCmdLen);	/* Get a char from the concatenation of the
																																						 Command Tree and the Input Command keywords */
		if ((iswhitespace(Ch)) || !Ch)						/* If found whitespace or reached end of string */
			break;																	/* then reached end of keywords sequence        */
		Len++;
	} while (Len);															/* Stop looping if Len overflows back to 0 */
	return Len;
}


/**************************************************************************************/
/* Determines if there is an exact match between the keywords of a Command Spec or a	*/
/* Character Data Spec and the keywords of the Input Command.													*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] SSpec							- Start of Specification string being compared						*/
/*	[in] LenSpec						- Length of Specification string (>=1)										*/
/*	[in] SInp								- Pointer to start of part of Input Command being compared*/
/*	[in] LenInp							- Length of part of Input Command													*/
/*	[in] eKeyword						- Type of keywords being compared - Command or Char Data	*/
/*  ONLY PRESENT IF SUPPORT_NUM_SUFFIX IS DEFINED:																		*/
/*	[out] uiNumSuf					- Array [0..MAX_NUM_SUFFIX-1] of returned numeric 				*/
/*														suffices present	in the Input Command.									*/
/*	[in/out] *pNumSuf				- Pointer to index of first element in uiNumSuf array to 	*/
/*														populate; if this function returns error code						*/
/*                            SCPI_ERR_NONE then this is returned as next element to	*/
/*														populate, otherwise it is returned unchanged.						*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						 - OK:		Match found																			*/
/*	SCPI_ERR_INVALID_CMD 		 - Error: No match found																	*/
/*	SCPI_ERR_TOO_MANY_NUM_SUF- Error:	Too many numeric suffices in Input Command to		*/
/*																		be returned in return parameter array.					*/
/*	SCPI_ERR_NUM_SUF_INVALID - Error: Numeric suffix in Input Command is invalid			*/
/**************************************************************************************/
#ifdef SUPPORT_NUM_SUFFIX
UCHAR KeywordsMatchSpec (const char *SSpec, SCPI_CHAR_IDX LenSpec, char *SInp, SCPI_CHAR_IDX LenInp, enum enKeywordType eKeyword, unsigned int uiNumSuf[], UCHAR *pNumSuf)
#else
UCHAR KeywordsMatchSpec (const char *SSpec, SCPI_CHAR_IDX LenSpec, char *SInp, SCPI_CHAR_IDX LenInp, enum enKeywordType eKeyword)
#endif
{
	UCHAR Err = SCPI_ERR_NONE;
	SCPI_CHAR_IDX PosSpec = 0;
	SCPI_CHAR_IDX PosInp = 0;
	SCPI_CHAR_IDX OldPosInp;
	char ChSpec;
	char ChInp;
	BOOL bOptional = FALSE;
	BOOL bLongForm = FALSE;
#ifdef SUPPORT_NUM_SUFFIX
	UCHAR NumSufIndex = *pNumSuf;
	BOOL bNumSufBegun = FALSE;
#endif

	if (eKeyword == KEYWORD_COMMAND)						/* If keyword being matched is from the command keywords */
		if (SInp[0] == CMD_COMMON_START)					/* If Input Command is a common command (its first char being an asterisk) */
			PosInp += mCommandTreeLen;							/* then ignore the current Command Tree, since does not apply to common commands */

	/* Loop while a match is possible */
	while (Err != SCPI_ERR_INVALID_CMD)
	{
		if (eKeyword == KEYWORD_COMMAND)					/* If keyword being matched is from the command keywords  */
			ChInp = CharFromFullCommand (SInp, PosInp, LenInp);
																							/* Get character from Input Command (including Command Tree) */
		else																			/* If keyword being matched is in the parameter section */
		{
			if (PosInp < LenInp)										/* If still within Input Command 							*/
				ChInp = SInp[PosInp];									/* then get character from Input Command 			*/
			else
				ChInp = '\0';													/* otherwise treat character as end-of-string */
		}

		if (PosSpec < LenSpec)										/* If still within Specification 		 */
			ChSpec = SSpec[PosSpec];								/* read character from Specification */
		else
			break;																	/* otherwise exit the loop 					 */

		if (ChSpec == '[')												/* If found beginning of optional keyword in Spec 				*/
		{
			bOptional = TRUE;												/* Flag the current keyword in Spec as optional 					*/
			OldPosInp = PosInp;											/* Remember position in Input Command where this happend 	*/
			PosSpec++;															/* Go to next character in Spec														*/
			continue;
		}

		if (ChSpec == ']')												/* If found end of optional keyword in Spec		*/
		{
			bOptional = FALSE;											/* Flag current keyword in Spec as required		*/
			PosSpec++;															/* Go to next character in Spec								*/
			continue;
		}

#ifdef SUPPORT_NUM_SUFFIX
		if (ChSpec == NUM_SUF_SYMBOL)							/* If found numeric suffix symbol in Specification */
		{
			if (NumSufIndex < MAX_NUM_SUFFIX)				/* If not too many numeric suffices in Specification */
			{
				if (isdigit (ChInp))									/* If input character is a decimal digit */
				{
					if (!bNumSufBegun)									/* If this is the start of a new numeric suffix */
						uiNumSuf[NumSufIndex] = 0;				/* then initialise it to zero				 						*/

					bNumSufBegun = TRUE;								/* Set flag to indicate have begun parsing numeric suffix digits */

					if (!AppendToUInt (&(uiNumSuf[NumSufIndex]), ChInp-'0'))	/* If unable to append input digit to numeric suffix */
						Err = SCPI_ERR_NUM_SUF_INVALID;													/* then it is invalid 															 */

					PosInp++;														/* Go to next character in Input Command */
					continue;
				}
				else																	/* If input character is not a decimal digit */
				{
					if (!bNumSufBegun)															/* If never started reading digits of numeric suffix  */
						uiNumSuf[NumSufIndex] = NUM_SUF_DEFAULT_VAL;	/* then return it as the default numeric suffix value */
          // ALP 
					//else
					//	if ((uiNumSuf[NumSufIndex] < NUM_SUF_MIN_VAL) || (uiNumSuf[NumSufIndex] > NUM_SUF_MAX_VAL))
					//																		/* If numeric suffix is outside allowed values */
					//		Err = SCPI_ERR_NUM_SUF_INVALID;	/* then it is invalid 												 */

					bNumSufBegun = FALSE;								/* Clear flag for next time */
					NumSufIndex++;											/* Increment numeric suffix counter */
					PosSpec++;													/* Go to next character in Specification */
					continue;
				}
			}
			else																		/* If there are more '#'s in the specification than allowed */
			{
				Err = SCPI_ERR_TOO_MANY_NUM_SUF;			/* then return error code																		*/
				break;
			}
		}
#endif

		if (ChSpec == KEYW_SEP)										/* If found keyword separator in Specification														*/
			bLongForm = FALSE;											/* then this is a new keyword starting so reset "Long-Form required" flag */

		if (tolower (ChSpec) == tolower (ChInp))	/* If characters in Spec and Input Command match */
		{
			if (islower (ChSpec))										/* If character in Spec is only applicable to Long Form */
				bLongForm = TRUE;											/* then we now require the Long Form from the Input Command */

			PosSpec++;															/* Go to next character in Spec 					*/
			PosInp++;																/* Go to next character in Input Command 	*/
		}

		else																			/* If characters in Spec and Input Command do not match */
		{
			if (islower (ChSpec))										/* If Spec character is only applicable to Long Form */
			{
				if (!bLongForm)												/* If we do not require Long Form from the Input Command */
				{
					PosSpec = SkipToNextRequiredChar (SSpec, PosSpec); /* then move to next required character in Specification */
				}
				else																	/* If we require Long Form from the Input Command */
				{
					if (bOptional)											/* If keyword in the Specification is optional */
					{
						PosInp = OldPosInp;								/* Revert to character position in Input Command before attempting
																									 to match with the optional keyword															 			*/
						PosSpec = SkipToEndOfOptionalKeyword (SSpec, PosSpec);
																							/* and move position in Command Spec to the end of the optional keyword */
					}
					else																/* If keyword in Spec is not optional									 				 */
					{
						Err = SCPI_ERR_INVALID_CMD;				/* then this is a definte mis-match, so set flag and exit loop */
					}
				}
			}
			else																		/* If character in Spec is required for Long or Short Form */
			{
				if (bOptional)												/* If keyword in Command Spec is optional */
				{
					PosInp = OldPosInp;									/* Revert to character position in Input Command before attempting
																								 to match with the optional keyword															 */
					PosSpec = SkipToEndOfOptionalKeyword (SSpec, PosSpec);
																							/* and move position in Command Spec to the end of the optional keyword */
				}
				else																	/* If keyword in Spec is not optional 								 					*/
					Err = SCPI_ERR_INVALID_CMD;					/* then this is a definite mis-match, so set flag and exit loop */
			}
		}
	}

	if (ChInp && (Err != SCPI_ERR_TOO_MANY_NUM_SUF))	/* If not reached the end of the keyword characters in Input Command */
		Err = SCPI_ERR_INVALID_CMD;											/* then this can't be a match																		 		 */

#ifdef SUPPORT_NUM_SUFFIX
	if (Err == SCPI_ERR_NONE)										/* If no error occurred 																		*/
		*pNumSuf = NumSufIndex;										/* then return new index of next numeric suffix to populate */
#endif

	return Err;
}


/**************************************************************************************/
/* Given the current position within the command keywords of a Command Specification,	*/
/* returns the position of the next character that is required for a match						*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] SSpec 			-		Pointer to first char in command keywords of Command Spec			*/
/*	[in] Pos				-		Current position (where 0 is first character) within					*/
/*											command	keywords of Command Spec															*/
/*																																										*/
/* Return Value:																																			*/
/*	Position of first character in command keywords after end of the present keyword	*/
/**************************************************************************************/
SCPI_CHAR_IDX SkipToNextRequiredChar (const char *SSpec, SCPI_CHAR_IDX Pos)
{
	char Ch;

	do
	{
		Pos++;
		Ch = SSpec[Pos];													/* Get character from specification */
	} while (islower (Ch));											/* Loop while character is only required in Long Form */

	return Pos;
}


/**************************************************************************************/
/* Given the current position within the command keywords of a Command Spec,					*/
/* returns position of the first char after the end of the present optional keyword.	*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] SSpec 			-		Pointer to first char in command keywords of Command Spec			*/
/*	[in] Pos				-		Current position (where 0 is first char) within								*/
/*											command keywords of Command Spec															*/
/*																																										*/
/* Return Value:																																			*/
/*	Position of first character in command keywords after the end of the present			*/
/*  optional keyword																																	*/
/**************************************************************************************/
SCPI_CHAR_IDX SkipToEndOfOptionalKeyword (const char *SSpec, SCPI_CHAR_IDX Pos)
{
	char Ch;

	do
	{
		Pos++;
		Ch = SSpec[Pos];													/* Get character from specification */
	} while ((Ch != ']') && Ch);								/* Loop until closing square bracket or end of string */

	return Pos;
}


/**************************************************************************************/
/* Determines if the parameter count of an Input Command is a valid match for a 			*/
/* Command Spec																																				*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] CmdSpecNum -				Number of Command Spec being matched to Input Command			*/
/*	[in] InpCmdParamsCnt -	Number of parameters in the Input Command									*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE					- OK:			Parameter count is a valid match									*/
/*	SCPI_ERR_PARAM_COUNT	- Error:	Parameter count is not a valid match							*/
/**************************************************************************************/
UCHAR MatchesParamsCount (SCPI_CMD_NUM CmdSpecNum, UCHAR InpCmdParamsCnt)
{
	UCHAR Err;
	UCHAR ParIdx;
	UCHAR MinCnt = 0;
	UCHAR MaxCnt = 0;
	const struct strSpecParam *psSpec;

	for (ParIdx = 0; (ParIdx < MAX_PARAMS); ParIdx++) /* Loop thru all the Command Spec's parameters */
	{
		psSpec = &(sSpecCommand[CmdSpecNum].sSpecParam[ParIdx]);	/* Create pointer to Parameter Spec of Command Spec */
		if (psSpec->eType == P_NONE)							/* If Parameter Spec is type No Parameter 							*/
			break;																	/* then reached end of list of parameters, so exit loop */
		if (psSpec->bOptional)										/* If parameter is optional																		 */
			MaxCnt = ParIdx + 1;										/* then this is currently the maximum possible parameter count */

		else																			/* If this parameter is required																	 */
		{
			MinCnt = ParIdx + 1;										/* then all parameters up to this one are also required						 */
			MaxCnt = MinCnt;												/* and this is also currently the maximum possible parameter count */
		}
	}

	/* At this point, MinCnt is the minimum number of parameters the Command Spec allows,
		 and MaxCnt is the maximum number of parameters the Command Spec allows.						*/

	if ((InpCmdParamsCnt >= MinCnt) && (InpCmdParamsCnt <= MaxCnt)) /* If parameter count of Input Command is within
																																		 the allowable range of the Command Spec			 */
		Err = SCPI_ERR_NONE;																					/* then return no error													 */
	else
		Err = SCPI_ERR_PARAM_COUNT;																		/* otherwise return error code									 */

	return Err;
}


/**************************************************************************************/
/* Returns information on the Input Parameters within an Input Command.								*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] SInpCmd -					 Pointer to Input Command																	*/
/*	[in] InpCmdLen -				 Length of Input Command																	*/
/*	[in] InpCmdKeywordsLen - Length of command keywords in Input Command, including		*/
/*													 Command Tree																							*/
/*	[out] pParamsCnt -			 Pointer to returned number of Input Parameters						*/
/*	[out] SParams -					 Returned pointer to first character of the Input					*/
/*													 Parameters string within Input Command										*/
/*	[out] pParamsLen -			 Pointer to returned variable containing length of Input	*/
/*													 Parameters string within Input Command										*/
/**************************************************************************************/
void GetParamsInfo (char *SInpCmd, SCPI_CHAR_IDX InpCmdLen, SCPI_CHAR_IDX InpCmdKeywordsLen,
 UCHAR *pParamsCnt, char **SParams, SCPI_CHAR_IDX *pParamsLen)
{
	SCPI_CHAR_IDX ParamsStart;
	SCPI_CHAR_IDX Pos = 0;
	BOOL bWithinQuotes = FALSE;
	BOOL bDoubleQuotes = FALSE;
	UCHAR Brackets = 0;
	char Ch;

	ParamsStart = InpCmdKeywordsLen - mCommandTreeLen;	/* Calculate start position of Input Parameters string */

	*SParams = &(SInpCmd[ParamsStart]);					/* Set returned pointer to start of Input Parameters string */
	*pParamsLen = InpCmdLen - ParamsStart;			/* Set returned value of Input Parameters string length */

	*pParamsCnt = 0;

	/* Find start of first Input Parameter within Input Parameters string, if any */
	while (Pos < *pParamsLen)
	{
		if (!iswhitespace((*SParams)[Pos]))				/* If encountered a non-whitespace character */
		{
			*pParamsCnt = 1;												/* then found a parameter */
			break;																	/* and exit this loop			*/
		}
		Pos++;
	}

	/* Count subsequent Input Parameters by counting commas (don't count ones within quotes or brackets) */
	for (; Pos < *pParamsLen; Pos++)
	{
		Ch = (*SParams)[Pos];
		switch (Ch)
		{
			case DOUBLE_QUOTE :												/* Double-quote encountered */
				if (!(bWithinQuotes && !bDoubleQuotes)) /* If it is not embedded within single-quotes */
				{
					bWithinQuotes = !bWithinQuotes;				/* then toggle the in/out-of-quotes state 		*/
					bDoubleQuotes = TRUE;
				}
				break;
			case SINGLE_QUOTE :												/* Single-quote encountered */
				if (!(bWithinQuotes && bDoubleQuotes))	/* If it is not embedded within double-quotes	*/
				{
					bWithinQuotes = !bWithinQuotes;				/* then toggle the in/out-of-quotes state			*/
					bDoubleQuotes = FALSE;
				}
				break;
			case OPEN_BRACKET :											/* Opening bracket encountered */
				Brackets++;														/* Increment bracket nesting level counter */
				break;
			case CLOSE_BRACKET :										/* Closing bracket encountered */
				if (Brackets)
					Brackets--;													/* Decrement bracket nesting level counter, not further than 0 */
				break;
			case ',' :															/* If encounted a comma 							*/
				if (!bWithinQuotes && !Brackets)			/* that is outside quotes and brackets*/
					(*pParamsCnt)++;										/* then increment the parameter count	*/
		}
	}
}


/**************************************************************************************/
/* Translates the Input Parameters according to a Command Spec.												*/
/* If translation succeeds, then returns populated parameter structures.							*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] CmdSpecNum			- Number of Command Spec to use for translation								*/
/*	[in] SInpParams			- Pointer to first character of Input Parameters string				*/
/*	[in] InpParamsLen		- Number of characters in Input Parameters string							*/
/*	[out] sParam				- Array [0..MAX_PARAM-1] of returned parameter structures			*/
/*												(contents of structures is undefined if error code returned)*/
/*  ONLY PRESENT IF SUPPORT_NUM_SUFFIX IS DEFINED:																		*/
/*	[out] uiNumSuf			- Array [0..MAX_NUM_SUFFIX-1] of returned numeric suffices		*/
/*												present	in the Input Command.																*/
/*	[in/out]  pNumSuf		- Pointer to index of first element in uiNumSuf array to			*/
/*												populate. Returned as pointer to index of next element.			*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE 					-	OK: 		Translation succeeded														*/
/*	SCPI_ERR_PARAM_TYPE			-	Error:	One or more of the Input Parameters was the 		*/
/*																		wrong type for the Command Spec									*/
/*	SCPI_ERR_PARAM_OVERFLOW	- Error:	An overflow occurred while parsing a						*/
/*																		numeric value (exponent > +/-43)								*/
/*	SCPI_ERR_TOO_MANY_NUM_SUF- Error:	Too many numeric suffices in Input Parameter to	*/
/*																		be returned in return parameter array.					*/
/*	SCPI_ERR_UNMATCHED_BRACKET-Error: Unmatched bracket in Input Parameter						*/
/*	SCPI_ERR_UNMATCHED_QUOTE- Error:	Unmatched quote (single/double) in Input Param	*/
/*	SCPI_ERR_NUM_SUF_INVALID- Error:  Numeric suffix in Input Parameter is invalid		*/
/*  SCPI_ERR_INVALID_VALUE	- Error:  One or more values in a numeric/channel list is	*/
/*																		invalid, e.g.	floating point when not allowed		*/
/*	SCPI_ERR_INVALID_DIMS		- Error:	Invalid number of dimensions in one of more			*/
/*																		of the channel list's entries										*/
/**************************************************************************************/
#ifdef SUPPORT_NUM_SUFFIX
UCHAR TranslateParameters (SCPI_CMD_NUM CmdSpecNum, char *SInpParams,
 SCPI_CHAR_IDX InpParamsLen, struct strParam sParam[], unsigned int uiNumSuf[], UCHAR *pNumSuf)
#else
UCHAR TranslateParameters (SCPI_CMD_NUM CmdSpecNum, char *SInpParams,
 SCPI_CHAR_IDX InpParamsLen, struct strParam sParam[])
#endif
{
	UCHAR Err = SCPI_ERR_NONE;
	const struct strSpecParam *psSpecParam;
	UCHAR ParamIdx;
	SCPI_CHAR_IDX ParamLen;
	SCPI_CHAR_IDX ParamStart;
	SCPI_CHAR_IDX Pos = 0;
	char Ch;
	UCHAR State;
	UCHAR Brackets;

	/* Initially set all returned parameters to type No Parameter */
	for (ParamIdx = 0; ParamIdx < MAX_PARAMS; ParamIdx++)
		sParam[ParamIdx].eType = P_NONE;

	/* Loop once for each possible parameter, or until an error occurs */
	for (ParamIdx = 0; (ParamIdx < MAX_PARAMS) && (Err == SCPI_ERR_NONE); ParamIdx++)
	{
		psSpecParam = &(sSpecCommand[CmdSpecNum].sSpecParam[ParamIdx]); /* Set pointer to Parameter Spec in Command Spec */

		if (psSpecParam->eType == P_NONE)					/* If reached end of the parameters in the Parameter Spec */
			break;																	/* then stop translation																	*/

		Err = SCPI_ERR_NONE;
		Brackets = 0;															/* Reset bracket nesting level count */

		/* FSM (Finite State Machine) to parse a single Input Parameter in Input Parameters string */
		/* Refer to Design Notes for a description of this FSM																		 */
		State = 0;

		while (State != 9)												/* Loop until reached Exit state */
		{
			switch (State)
			{
				/* State 0 - Awaiting first character of parameter */
				case 0:																/* Start state */
					ParamStart = Pos;										/* Initially, set start of Input Parameter to current position in string */
					ParamLen = 0;												/* and set parameter length to zero */

					if (Pos >= InpParamsLen)						/* If gone past end of Input Parameters string */
					{
						State = 9;												/* go to Exit state														 */
						break;
					}
					Ch = SInpParams[Pos];								/* Read character at current position in Input Parameters string */
					switch (Ch)													/* Examine character */
					{
						case PARAM_SEP:
							State = 5;											/* Parameter separator - go to state 5 */
							break;
						case SINGLE_QUOTE:								/* Opening single quote -	go to state 2 */
							State = 2;
							break;
						case DOUBLE_QUOTE:								/* Opening double quote - go to state 3 */
							State = 3;
							break;
						case OPEN_BRACKET:								/* Opening bracket */
							State = 6;											/* Go to state 6 */
							break;
						case CLOSE_BRACKET:								/* Closing bracket */
							Err = SCPI_ERR_UNMATCHED_BRACKET;	/* It is invalid to start parameter with a closing bracket */
							State = 9;												/* so exit FSM */
							break;
						default:
							if (iswhitespace (Ch))					/* If whitespace */
								State = 1;										/* go to state 1 */
							else														/* If any other character (i.e. a part of the parameter) */
								State = 4;										/* go to state 4 */
					}
					break;

				/* State 1 - Within leading whitespace */
				case 1:
					Pos++;															/* Go to next position in Input Parameters string */
					State = 0;													/* Always go back to state 0 */
					break;

				/* State 2 - Inside single quotes	*/
				case 2:
					ParamLen++;													/* Increment length of Input Parameter found */
					Pos++;															/* Go to next char position in Input Parameters string */
					if (Pos >= InpParamsLen)						/* If gone past end of Input Parameters string */
					{
						Err = SCPI_ERR_UNMATCHED_QUOTE;		/* Error - no matching quote found within string */
						State = 9;												/* go to Exit state */
						break;
					}
					switch (SInpParams[Pos])
					{
						case SINGLE_QUOTE:								/* Closing single quote - go to state 4 */
							State = 4;
							break;
						default:													/* Any other character, just repeat this state */
							break;
					}
					break;

				/* State 3 - Inside double quotes	*/
				case 3:
					ParamLen++;													/* Increment length of Input Parameter found */
					Pos++;															/* Go to next char position in Input Parameters string */
					if (Pos >= InpParamsLen)						/* If gone past end of Input Parameters string */
					{
						Err = SCPI_ERR_UNMATCHED_QUOTE;		/* Error - no matching quote found within string */
						State = 9;												/* go to Exit state */
						break;
					}
					switch (SInpParams[Pos])
					{
						case DOUBLE_QUOTE:
							State = 4;											/* Closing double quote - go to state 4 */
							break;
						default:
							break;													/* Any other character, just repeat this state */
					}
					break;

				/* State 4 - In characters of parameter (not within quotes) */
				case 4:
					ParamLen++;													/* Increment length of Input Parameter found */
					Pos++;															/* Go to next char position in Input Parameters string */
					if (Pos >= InpParamsLen)						/* If gone past end of Input Parameters string */
					{
						State = 9;												/* go to Exit state */
						break;
					}
					switch (SInpParams[Pos])
					{
						case PARAM_SEP:										/* Parameter separator (comma) - go to state 5 */
							State = 5;
							break;
						case SINGLE_QUOTE:								/* Opening single quote - go to state 2 */
							State = 2;
							break;
						case DOUBLE_QUOTE:								/* Opening double quote - go to state 3 */
							State = 3;
							break;
						case OPEN_BRACKET:								/* Opening bracket */
							State = 6;
							break;
						case CLOSE_BRACKET:									/* Closing bracket */
							Err = SCPI_ERR_UNMATCHED_BRACKET;	/* Error - closing bracket has no matching opening bracket */
							State = 9;												/* and exit */
							break;
						default:													/* Any other character, just repeat this state */
							break;
					}
					break;

				/* State 5 - Parameter delimiter (comma) encountered */
				case 5:
					Pos++;															/* Go to next char position in Input Parameters string */
					State = 9;													/* Go to Exit state */
					break;

				/* State 6 - Opening bracket encountered */
				case 6:
					Brackets++;													/* Increment bracket nesting level count */
					ParamLen++;													/* Increment length of Input Parameter found */
					Pos++;															/* Go to next char position in Input Parameters string */
					if (Pos >= InpParamsLen)						/* If gone past end of Input Parameters string */
					{
						Err = SCPI_ERR_UNMATCHED_BRACKET;	/* Error - opening bracket has no matching closing bracket */
						State = 9;												/* go to Exit state */
						break;
					}
					switch (SInpParams[Pos])
					{
						case OPEN_BRACKET:								/* Opening bracket */
							break;													/* then repeat this state */
						case CLOSE_BRACKET:								/* Closing bracket */
							State = 7;
							break;
						default:													/* Any other character */
							State = 8;
							break;
					}
					break;

				/* State 7 - Closing bracket encountered within brackets */
				case 7:
					Brackets--;													/* Decrement bracket nesting level count */
					if (!Brackets)											/* If no longer within brackets */
						State = 4;
					else
						State = 8;
          break;

				/* State 8 - Within brackets */
				case 8:
					ParamLen++;													/* Increment length of Input Parameter found */
					Pos++;															/* Go to next char position in Input Parameters string */
					if (Pos >= InpParamsLen)						/* If gone past end of Input Parameters string */
					{
						Err = SCPI_ERR_UNMATCHED_BRACKET;	/* Error - opening bracket has no matching closing bracket */
						State = 9;												/* go to Exit state */
						break;
					}
					switch (SInpParams[Pos])
					{
						case OPEN_BRACKET:								/* Opening bracket */
							State = 6;
							break;
						case CLOSE_BRACKET:								/* Closing bracket */
							State = 7;
							break;
						default:													/* Any other character */
							break;													/* then repeat state */
					}
					break;
			}
		};

		if (Err == SCPI_ERR_NONE)									/* If no error so far */
		{
			/* At this point: SInpParams[ParamStart] is the first character of the Input Parameter (no leading whitespace)
												ParamLen is number of characters in Input Parameter, including trailing whitespace					 */

			/* Trim trailing whitespace from Input Parameter */
			while (ParamLen > 0)
				if (iswhitespace (SInpParams[ParamStart + ParamLen - 1]))
					ParamLen--;
				else
					break;

			/* Note: ParamLen is zero if Input Parameter is zero length or Input Parameter is simply not present */

			/* Attempt to translate the Input Parameter according to its type in the Parameter Spec */
			switch (psSpecParam->eType)
			{
				case P_CH_DAT :
#ifdef SUPPORT_NUM_SUFFIX
					Err = TranslateCharDataParam (&(SInpParams[ParamStart]), ParamLen, psSpecParam, &(sParam[ParamIdx]), uiNumSuf, pNumSuf);
#else
					Err = TranslateCharDataParam (&(SInpParams[ParamStart]), ParamLen, psSpecParam, &(sParam[ParamIdx]));
#endif
					break;
				case P_BOOL :
					Err = TranslateBooleanParam (&(SInpParams[ParamStart]), ParamLen, (const struct strSpecAttrBoolean *)(psSpecParam->pAttr), &(sParam[ParamIdx]));
					break;
				case P_NUM :
					Err = TranslateNumericValueParam (&(SInpParams[ParamStart]), ParamLen, (const struct strSpecAttrNumericVal *)(psSpecParam->pAttr), &(sParam[ParamIdx]));
					break;
				case P_STR :
				case P_UNQ_STR :
					Err = TranslateStringParam (&(SInpParams[ParamStart]), ParamLen, psSpecParam->eType, &(sParam[ParamIdx]));
					break;
#ifdef SUPPORT_EXPR
				case P_EXPR :
					Err = TranslateExpressionParam (&(SInpParams[ParamStart]), ParamLen, &(sParam[ParamIdx]));
					break;
#endif
#ifdef SUPPORT_NUM_LIST
				case P_NUM_LIST :
					Err = TranslateNumericListParam (&(SInpParams[ParamStart]), ParamLen, (const struct strSpecAttrNumList *)(psSpecParam->pAttr), &(sParam[ParamIdx]));
					break;
#endif
#ifdef SUPPORT_CHAN_LIST
				case P_CHAN_LIST :
					Err = TranslateChannelListParam (&(SInpParams[ParamStart]), ParamLen, (const struct strSpecAttrChanList *)(psSpecParam->pAttr), &(sParam[ParamIdx]));
					break;
#endif
				default:																		/* This should never happen, unless Command Spec is defined wrongly */
					Err = SCPI_ERR_PARAM_TYPE;								/* Return error code if it does occur 															*/
			}
			if ((Err != SCPI_ERR_NONE) &&									/* Input Parameter could not be translated													*/
			 !ParamLen && psSpecParam->bOptional)					/* and the Input Parameter is empty and Command Spec says parameter */
					Err = SCPI_ERR_NONE;											/* is optional, so this is NOT an error condition										*/
		}
	}
	return Err;
}


/**************************************************************************************/
/* Translates an Input Parameter string into a Character Data parameter, translated		*/
/* according to a Parameter Spec.																											*/
/*																																										*/
/* Parameters:																																				*/
/*	[in]	SParam			- Pointer to start of Input Parameter string										*/
/*	[in]	ParLen			- Length of Input Parameter string															*/
/*	[in]	psSpecParam	- Pointer to Parameter Spec																			*/
/*	[out]	psParam			- Pointer to returned parameter structure												*/
/*											(contents are undefined if an error code is returned)					*/
/*  ONLY PRESENT IF SUPPORT_NUM_SUFFIX IS DEFINED:																		*/
/*	[out] uiNumSuf		- Array [0..MAX_NUM_SUFFIX-1] of returned numeric suffices			*/
/*											present	in the Input Command.																	*/
/*	[in/out] pNumSuf	- Pointer to index of first element of uiNumSuf array to				*/
/*											populate. Returned as pointer to index of next element.				*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK:			Translation succeeded														*/
/*	SCPI_ERR_PARAM_TYPE			-	Error:	The Input Parameter was the wrong type for the	*/
/*																		Parameter Spec																	*/
/*	SCPI_ERR_PARAM_OVERFLOW	- Error:	The Input Parameter was a numeric value, 				*/
/*																		allowed by the Parameter Spec, but the value		*/
/*																		overflowed (exponent was greater than +/-43)		*/
/*	SCPI_ERR_TOO_MANY_NUM_SUF- Error:	Too many numeric suffices in Input Command to		*/
/*																		be returned in return parameter array.					*/
/*	SCPI_ERR_NUM_SUF_INVALID- Error:  Numeric suffix in Input Command is invalid 			*/
/*  SCPI_ERR_INVALID_VALUE	- Error:  One or more values in a numeric/channel list is	*/
/*																		invalid, e.g.	floating point when not allowed		*/
/*	SCPI_ERR_INVALID_DIMS		- Error:	Invalid number of dimensions in one of more			*/
/*																		of the channel list's entries										*/
/**************************************************************************************/
#ifdef SUPPORT_NUM_SUFFIX
UCHAR TranslateCharDataParam (char *SParam, SCPI_CHAR_IDX ParLen,
	const struct strSpecParam *psSpecParam, struct strParam *psParam, unsigned int uiNumSuf[], UCHAR *pNumSuf)
#else
UCHAR TranslateCharDataParam (char *SParam, SCPI_CHAR_IDX ParLen,
	const struct strSpecParam *psSpecParam, struct strParam *psParam)
#endif
{
	UCHAR RetErr;
	UCHAR Err;
	const struct strSpecAttrCharData *pSpecAttr;
	UCHAR ItemNum;
	SCPI_CHAR_IDX ItemStart;
	SCPI_CHAR_IDX ItemLen;
	SCPI_CHAR_IDX SeqLen;
	const char *SSeq;
	char SeqCh;
	BOOL bMatch = FALSE;
	BOOL bItemEnded;
	enum enParamType eAlt;

	pSpecAttr = ((const struct strSpecAttrCharData *)(psSpecParam->pAttr));	/* Set pointer to Parameter Spec
																																						 Attributes structure						*/

	if (ParLen == 0)														/* If Input Parameter string is zero-length */
	{
		if (pSpecAttr->DefItemNum != C_NO_DEF)		/* If Parameter Spec has a default Character Data Item Number */
		{
			bMatch = TRUE;													/* then this is a valid translation */
			ItemNum = pSpecAttr->DefItemNum;				/* and Item Number returned is the default Item Number */
			Err = SCPI_ERR_NONE;
		}
		else																			/* If Parameter Spec has no default 								 */
			Err = SCPI_ERR_INVALID_CMD;							/* then set error flag to force other match attempts */
	}

	else																				/* If Input Parameter string is not zero-length */
	{
		SSeq = pSpecAttr->SSeq;										/* Set pointer to Character Data Sequence */
		SeqLen = (SCPI_CHAR_IDX)strlen (SSeq);		/* Get length of sequence */

		ItemNum = 0;
		ItemStart = 0;

		/* Loop thru Character Data Items until reached end of Character Data Sequence */
		while ((ItemStart < SeqLen) && !bMatch)
		{
			ItemLen = 0;														/* Reset length counter of current item */

			bItemEnded = FALSE;

			/* Loop thru characters in Sequence until end of Sequence or end of Item reached */
			while ((ItemStart+ItemLen < SeqLen) && !bItemEnded)
			{
				SeqCh = SSeq[ItemStart+ItemLen];			/* Read character from Character Data Sequence */
				if (SeqCh == '|')											/* If reached end of Character Data Item */
					bItemEnded = TRUE;									/* then flag this and so exit loop 			 */
				else																	/* If still within the same Character Data Item */
					ItemLen++;													/* then increment item length										*/
			};

			if (ItemLen > 0)												/* If item is not zero-length	*/
#ifdef SUPPORT_NUM_SUFFIX
				Err = KeywordsMatchSpec (&(SSeq[ItemStart]), ItemLen, SParam, ParLen, KEYWORD_CHAR_DATA, uiNumSuf, pNumSuf);
#else
      	Err = KeywordsMatchSpec (&(SSeq[ItemStart]), ItemLen, SParam, ParLen, KEYWORD_CHAR_DATA);
#endif																				/* then check if input parameter matches character data item specification */
			bMatch = (Err == SCPI_ERR_NONE);				/* A match exists if no error occurred in the match attempt */

			if (Err != SCPI_ERR_NONE)								/* If a match was not found        */
			{
				ItemNum++;														/* then go to next item number 		 */
				ItemStart += ItemLen;									/* and move to start of next item  */
				if (ItemStart < UCHAR_MAX)
					ItemStart++;												/* Also skip leading "|" character */
			}
		}
	}

	if (Err != SCPI_ERR_NONE)										/* If no match was found between Input Parameter and a Character Data Item */
	{
		RetErr = Err;															/* Remember old error code */

		eAlt = pSpecAttr->eAltType;								/* Read the Alternative Parameter Type from the Parameter Spec */
		if (eAlt != P_NONE)												/* If there is an Alternative Parameter Type allowed */
		{
			switch (eAlt)														/* then attempt to translate Input Parameter as that type */
			{
				case P_BOOL :													/* Alternative Parameter Type is Boolean */
					Err = TranslateBooleanParam (SParam, ParLen, (const struct strSpecAttrBoolean *)(pSpecAttr->pAltAttr), psParam);
					break;
				case P_NUM :													/* Alternative Parameter Type is Numeric Value */
					Err = TranslateNumericValueParam (SParam, ParLen,
					 (const struct strSpecAttrNumericVal *)(pSpecAttr->pAltAttr), psParam);
					break;
				case P_STR :													/* Alternative Parameter Type is String or Unquoted String */
				case P_UNQ_STR :
					Err = TranslateStringParam (SParam, ParLen, eAlt, psParam);
					break;
#ifdef SUPPORT_EXPR
				case P_EXPR :
					Err = TranslateExpressionParam (SParam, ParLen, psParam);
					break;
#endif
#ifdef SUPPORT_NUM_LIST
				case P_NUM_LIST :
					Err = TranslateNumericListParam (SParam, ParLen,
					 (const struct strSpecAttrNumList *)(pSpecAttr->pAltAttr), psParam);
					break;
#endif
#ifdef SUPPORT_CHAN_LIST
				case P_CHAN_LIST :
					Err = TranslateChannelListParam (SParam, ParLen,
					 (const struct strSpecAttrChanList *)(pSpecAttr->pAltAttr), psParam);
					break;
#endif
				default:															/* This should never happen, unless the Alternative Parameter Type is */
					Err = SCPI_ERR_PARAM_TYPE;					/* defined wrongly. Return error code																	*/
			}
		}
		else																			/* No match found and no Alternative Parameter Type in Parameter Spec */
			Err = SCPI_ERR_PARAM_TYPE;							/* so error code																											*/

		switch (Err)
		{
			case SCPI_ERR_NONE : RetErr = SCPI_ERR_NONE; break;		/* If a matching parameter type was found then return no error */
			default						 : if (RetErr > Err) RetErr = Err;	/* If no match was found then set returned error code to
																															 the least significant error code encountered (error
																															 values are in order of significance)										*/
		}
	}
	else																				/* If match was found between Input Parameter and a Character Data Item */
	{
		psParam->eType = P_CH_DAT;								/* Populate returned parameter structure with Character Data information */
		psParam->unAttr.sCharData.ItemNum = ItemNum;
		psParam->unAttr.sCharData.SSeq = SSeq;

		RetErr = SCPI_ERR_NONE;
	}

	return RetErr;
}


/**************************************************************************************/
/* Translates an Input Parameter string into a Boolean parameter, translated according*/
/* to a Parameter Spec's Boolean Attributes.																					*/
/* Note: The SCPI standard defines Boolean as ON|OFF|<Numeric Value>.									*/
/*			 In the case of Numeric Value, if the integer conversion of it is non-zero		*/
/*       then the Boolean value is 1 (ON), otherwise the Boolean value is 0 (OFF).		*/
/*																																										*/
/* Parameters:																																				*/
/*	[in]	SParam			- Pointer to start of Input Parameter string										*/
/*	[in]	ParLen			- Length of Input Parameter string															*/
/*	[in]	psSpecAttr	- Pointer to Parameter Spec's Boolean Attributes								*/
/*	[out]	psParam			- Pointer to returned parameter structure												*/
/*											(contents are undefined if an error code is returned)					*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK:			Translation succeeded														*/
/*	SCPI_ERR_PARAM_TYPE			-	Error:	The Input Parameter was the wrong type for the 	*/
/*																		Parameter Spec																	*/
/*	SCPI_ERR_PARAM_OVERFLOW	- Error:	The Input Parameter was a numeric value, 				*/
/*																		but the value	overflowed (exponent was greater	*/
/*																		than +/-43)																			*/
/**************************************************************************************/
UCHAR TranslateBooleanParam (char *SParam, SCPI_CHAR_IDX ParLen,
 const struct strSpecAttrBoolean *psSpecAttr, struct strParam *psParam)
{
	UCHAR Err;
	BOOL bValid = FALSE;												/* Flag set to TRUE if Input Parameter is a valid Boolean */
	BOOL bVal;
	double fdVal;

	if (ParLen == 0)														/* If Input Parameter is zero-length */
	{
		if (psSpecAttr->bHasDef)									/* If Parameter Spec Boolean Attributes has default value */
		{
			bValid = TRUE;													/* Then Input Parameter is valid 	*/
			if (psSpecAttr->bDefOn)									/* If default value is On  				*/
				bVal = 1;															/* then Boolean value is 1 				*/
			else
				bVal = 0;															/* otherwise Boolean value is 0 	*/
		}
	}

	else																				/* If Input Parameter is not zero-length */
	{
		/* First check if Input Parameter string matches one of the string representations of a Boolean (ON or OFF) */

		if (StringsEqual (SParam, ParLen, BOOL_ON, BOOL_ON_LEN))	/* If Input Parameter matches "ON" 	*/
		{
			bValid = TRUE;													/* then Input Parameter is valid 		*/
			bVal = 1;																/* and set Boolean value to 1		 		*/
		}
		if (StringsEqual (SParam, ParLen, BOOL_OFF, BOOL_OFF_LEN))	/* If Input Parameter matches "OFF" */
		{
			bValid = TRUE;													/* then Input Parameter is valid 		*/
			bVal = 0;																/* and set Boolean value is 0 			*/
		}

		if (!bValid)															/* If Input Parameter did not match either "ON" or "OFF" */
		{
			Err = TranslateNumericValueParam (SParam, ParLen, &sSpecAttrBoolNum, psParam);
																							/* Try to translate Input Parameter as a Numeric Value */
			if (Err == SCPI_ERR_NONE)								/* If translated succeeded */
			{
				Err = SCPI_ParamToDouble (psParam, &fdVal);	/* Convert value to a double (double allows greatest range) */
				if (Err == SCPI_ERR_NONE)							/* If conversion went ok */
				{
					bValid = 1;													/* then the Input Parameter is a valid Boolean*/
					if (round(fdVal))										/* If integer value is non-zero	*/
						bVal = 1;													/* then Boolean value is 1			*/
					else
						bVal = 0;													/* else Boolean value is 0			*/
				}
			}
		}
	}

	if (bValid)																	/* If Input Parameter is a valid Boolean value */
	{
		psParam->eType = P_BOOL;									/* Populate returned parameter structure with */
		psParam->unAttr.sBoolean.bVal = bVal;			/* Boolean value information									*/

		Err = SCPI_ERR_NONE;
	}

	else																				/* If Input Parameter is not a valid Boolean value */
		Err = SCPI_ERR_PARAM_TYPE;								/* then return an error code											 */

	return Err;
}


/**************************************************************************************/
/* Translates an Input Parameter string into a Numeric Value parameter, translated		*/
/* according to a Parameter Spec's Numeric Value Attributes 													*/
/*																																										*/
/* Parameters:																																				*/
/*	[in]	SParam			- Pointer to start of Input Parameter string										*/
/*	[in]	ParLen			- Length of Input Parameter string															*/
/*	[in]	psSpecAttr	- Pointer to Parameter Spec's Numeric Value Attributes					*/
/*	[out]	psParam			- Pointer to returned parameter structure												*/
/*											(contents are undefined if an error code is returned)					*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK:			Translation succeeded														*/
/*	SCPI_ERR_PARAM_TYPE			-	Error:	The Input Parameter was the wrong type for the	*/
/*																		Parameter Spec																	*/
/*	SCPI_ERR_PARAM_OVERFLOW	- Error:	The Input Parameter value	overflowed (exponent	*/
/*																		was greater	than +/-43)													*/
/**************************************************************************************/
UCHAR TranslateNumericValueParam (char *SParam, SCPI_CHAR_IDX ParLen,
 const struct strSpecAttrNumericVal *psSpecAttr, struct strParam *psParam)
{
	UCHAR Err = SCPI_ERR_NONE;
	struct strAttrNumericVal *psNum = &(psParam->unAttr.sNumericVal);
	SCPI_CHAR_IDX Pos;
	signed char UnitExp;
	int iExp;
	BOOL bNegExp;

	Err = TranslateNumber (SParam, ParLen, psNum, &Pos);	/* Translate characters into a numeric value */

	if (Err == SCPI_ERR_NONE)										/* If translation succeeded */
	{
		if (Pos < ParLen)													/* If there are characters in the parameter after the number */
		{
			Err = TranslateUnits (&(SParam[Pos]), ParLen-Pos, psSpecAttr, &(psNum->eUnits), &UnitExp);
																							/* Translate following characters as the units of the parameter */
		}
		else																			/* If number was not followed by any non-numeric characters 			*/
		{
			psNum->eUnits = psSpecAttr->eUnits;			/* then use default units from the Parameter Spec	*/
			UnitExp = psSpecAttr->Exp;							/* and use the default unit exponent							*/
		}

    iExp = (int)(psNum->Exp);									/* Get exponent of numeric value (before adjusted for units) */
    if (psNum->bNegExp)
    	iExp = 0 - iExp;												/* Make exponent negative if flag is set */

    iExp += (int)UnitExp;											/* Add exponent of units */

		if (iExp > MAX_EXPONENT)									/* If resulting exponent is too big	*/
    {
				Err = SCPI_ERR_PARAM_OVERFLOW;				/* then return overflow error code	*/
    		iExp = 0;
    }
		if (iExp < MIN_EXPONENT)									/* If exponent is too small (i.e. too many decimal places */
		{			 																		/* before the first significant digit)										*/
			psNum->ulSigFigs = 0;				 						/* then the number is effectively zero										*/
			iExp = 0;
		}

		if (iExp < 0)															/* If exponent is negative							*/
		{
			iExp = 0 - iExp;												/* Make it positive											*/
			bNegExp = TRUE;													/* and set flag "exponent is negative"	*/
		}
		else																			/* If exponent is positive						*/
			bNegExp = FALSE;												/* clear flag "exponent is negative"	*/

		/* Populate members of returned numeric value structure */
		psNum->Exp = cabs((signed char)iExp);
		psNum->bNegExp = bNegExp;
		psParam->eType = P_NUM;										/* Set returned parameter structure type to Numeric Value */
	}

	return Err;
}


/**************************************************************************************/
/* Translates part of a string into a Number. Translation stops when a non-numeric		*/
/* character is encountered or the length of string to be translated has been reached.*/
/*																																										*/
/* Parameters:																																				*/
/*	[in]			SNum			- Pointer to start of string containing the number.						*/
/*	[in]			Len				- Length of string to translate up to.												*/
/*	[out]			psNum			- Pointer to returned numeric value structure									*/
/*												(contents are undefined if an error code is returned)				*/
/*	[out]			pNextPos 	- Pointer to returned index position within string of first		*/
/*											 	non-whitespace character after the number.									*/
/*												(contents are undefined if an error code is returned)				*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK:			Translation succeeded														*/
/*	SCPI_ERR_PARAM_TYPE			-	Error:	The Input Parameter was the wrong type for the	*/
/*																		Parameter Spec																	*/
/*	SCPI_ERR_PARAM_OVERFLOW	- Error:	The Input Parameter value	overflowed (exponent	*/
/*																		was greater	than +/-43)													*/
/**************************************************************************************/
UCHAR TranslateNumber (char *SNum, SCPI_CHAR_IDX Len, struct strAttrNumericVal *psNum, SCPI_CHAR_IDX *pNextPos)
{
	UCHAR Err = SCPI_ERR_NONE;
	UCHAR State = 0;
	char Ch;
	SCPI_CHAR_IDX Pos = 0;
	BOOL bFoundDigit = FALSE;
	BOOL bDigit;
	char Digit;
	BOOL bIsWhitespace;
	unsigned long ulSigFigs = 0;
	BOOL bNeg = FALSE;
	BOOL bNegExp = FALSE;
	int iExp = 0;
	int iDecPlaces = 0;
	BOOL bSigFigsLost = FALSE;									/* Flag - If TRUE then significant figures have been lost in translation */

	/* Parse each character in string, until end of string, an error occurs, or finished parsing */
	while ((Pos < Len) && (Err == SCPI_ERR_NONE) && (State != 8))
	{
		Ch = tolower(SNum[Pos]);									/* Read character from Input Parameter string */
		bDigit = isdigit (Ch);										/* Set bDigit to TRUE if character is a decimal digit */
		if (bDigit)																/* If character is a digit */
			Digit = Ch - '0';												/* then get value of digit (0..9) */
		bIsWhitespace = iswhitespace (Ch);				/* Set bIsWhitespace to TRUE if character is whitespace */

		/* FSM (Finite State Machine) for parsing Numeric Values				*/
		/* Refer to Design Notes document for a description of this FSM	*/
		switch (State)
		{
			case 0 :
				if (bDigit)
				{
					ulSigFigs = (unsigned long)Digit;		/* Set first digit of significant figures */
					bFoundDigit = TRUE;
					State = 2;
				}
				else
					switch (Ch)
					{
						case '.': State = 3; break;				/* Decimal point */
						case '+': State = 1; break;				/* Positive decimal number */
						case '-': bNeg = TRUE; State = 1; /* Negative decimal number */
							break;
						case '#': State = 10; break;			/* Symbol that precedes non-decimal number */
						default:
							if (!bIsWhitespace)	 						/* Whitespace is allowed - just repeat this state */
								Err = SCPI_ERR_PARAM_TYPE;		/* But any other character is invalid */
					}
				break;

			case 1 :
				if (bDigit)														/* If character is a digit */
				{
					AppendToULong (&ulSigFigs, Digit, BASE_DEC);	/* Append digit to significant figures
																													 Note: Cannot lose significant figures at this stage */
					bFoundDigit = TRUE;
					State = 2;
				}
				else
					if (Ch == '.')											/* Decimal point */
						State = 3;
					else																/* Invalid character encountered for this state */
						Err = SCPI_ERR_PARAM_TYPE;				/* so abort translation												  */
				break;

			case 2 :
				if (bDigit)														/* If character is a digit */
				{
					if (!AppendToULong (&ulSigFigs, Digit, BASE_DEC)) /* Append digit to significant figures 	*/
					{																		/* If it was not possible to append the digit 				*/
						bSigFigsLost = TRUE;							/* then flag that significant figures have been lost 	*/
						iDecPlaces--;											/* and decrement decimal places (so it goes negative)	*/
					}
				}
				else																	/* If character is not a digit */
					if (Ch == '.')											/* Decimal point */
						State = 3;
					else
					{
						if (Ch == 'e')										/* Exponent character encountered */
							State = 5;
						else
							if (bIsWhitespace)							/* Whitespace encountered */
								State = 9;
							else														/* Other character encountered (the start of the Units) */
								State = 8;
					}
				break;

			case 3 :
				if (bDigit)														/* If character is a digit */
				{
					if (!bSigFigsLost)									/* Only append decimal place digits if no significant figures lost yet */
					{
						if (AppendToULong (&ulSigFigs, Digit, BASE_DEC))	/* Append digit to significant figures	 	*/
							iDecPlaces++;																		/* and increment count of decimal places 	*/
						else															/* If not possible to append digit to significant figures */
							bSigFigsLost = TRUE;						/* then flag that significant figures have been lost			*/
					}
					bFoundDigit = TRUE;
					State = 4;
				}
				else																	/* If character is not a digit */
				{
					if (Ch == 'e')											/* Exponent character encountered */
						State = 5;
					else																/* Whitespace or other char encountered */
						State = 8;												/* so finished reading number						*/

				}
				break;

			case 4 :
				if (bDigit)														/* If character is a digit */
				{
					if (!bSigFigsLost)									/* Only append decimal place digits if no significant figures lost yet */
					{
						if (AppendToULong (&ulSigFigs, Digit, BASE_DEC))	/* Append digit to significant figures		*/
							iDecPlaces++;																		/* and increment count of decimal places	*/
						else															/* If not possible to append digit to significant figures	*/
							bSigFigsLost = TRUE;						/* then flag that significant figures have been lost			*/
					}
				}
				else																	/* If character is not a digit */
					if (Ch == 'e')											/* Exponent character encountered */
						State = 5;
					else
						if (bIsWhitespace)								/* Whitespace or other char encountered */
							State = 9;
						else															/* Any other char encountered */
							State = 8;											/* so finished reading number */
				break;

			case 5 :
				if (bDigit)														/* If character is a digit */
				{
					iExp = (int)Digit;									/* Set first digit of exponent */
					State = 7;
				}
				else																	/* If character is not a digit */
					if (Ch == '+')											/* Plus-sign encountered */
						State = 6;
					else
						if (Ch == '-')										/* Minus-sign encountered */
						{
							bNegExp = TRUE;									/* Exponent is negative */
							State = 6;
						}
						else
							if (!bIsWhitespace)							/* Whitespace encountered - ignore it (stay in present state) */
								Err = SCPI_ERR_PARAM_TYPE;		/* Any other char is invalid in this state */
				break;

			case 6 :
				if (bDigit)														/* If character is a digit */
				{
					iExp = (int)Digit;									/* Set first digit of exponent */
					State = 7;
				}
				else																	/* Invalid character encountered for this state */
					Err = SCPI_ERR_PARAM_TYPE;
				break;

			case 7 :
				if (bDigit)														/* If character is a digit */
				{
					if (!AppendToInt (&iExp, Digit))		/* Append digit to end of exponent */
						Err = SCPI_ERR_PARAM_OVERFLOW;		/* If not possible to append digit, return an overflow error */
				}
				else																	/* Whitespace of other char encountered */
					State = 8;													/* so finished reading number 					*/
				break;

			case 9 :
				if (Ch == 'e')												/* Exponent character encountered */
					State = 5;
				else
					if (!bIsWhitespace)									/* If any other char apart from whitespace */
						State = 8;												/* then finished reading number						 */
				break;

			case 10 :
				switch (Ch)
				{
					case 'b': State = 11; break;				/* Symbol for Binary number 			*/
					case 'q': State = 12; break;				/* Symbol for Octal number				*/
					case 'h': State = 13; break;				/* Symbol for Hexadecimal number	*/
					default: Err = SCPI_ERR_PARAM_TYPE; /* Any other char is invalid for this state */
				}
				break;

			case 11 :
				if (bDigit && (Digit < BASE_BIN))			/* If character is a valid binary digit */
				{
					bFoundDigit = TRUE;
					if (!AppendToULong (&ulSigFigs, Digit, BASE_BIN)) /* Append binary digit to significant figures */
						Err = SCPI_ERR_PARAM_OVERFLOW;		/* If cannot append digit then return overflow error */
				}
				else																	/* If character is not a valid binary digit */
					State = 8;													/* stop reading number											*/
				break;

			case 12 :
				if (bDigit && (Digit < BASE_OCT))			/* If character is a valid octal digit */
				{
					bFoundDigit = TRUE;
					if (!AppendToULong (&ulSigFigs, Digit, BASE_OCT)) /* Append octal digit to significant figures */
						Err = SCPI_ERR_PARAM_OVERFLOW;		/* If cannot append digit then return overflow error code */
				}
				else																	/* If character is not a valid octal digit */
					State = 8;													/* stop reading number 										 */
				break;

			case 13 :
				if (Ch >= 'a' && Ch <= 'f')						/* If character in range a-f */
				{
					bDigit = TRUE;											/* then this is a valid hexadecimal digit */
					Digit = Ch - 'a' + 10;							/* and set Digit to value (10-15) */
				}
				if (bDigit)														/* If valid hexadecimal digit */
				{
					bFoundDigit = TRUE;
					if (!AppendToULong (&ulSigFigs, Digit, BASE_HEX)) /* Append hexadecimal digit to significant figures */
						Err = SCPI_ERR_PARAM_OVERFLOW;		/* If cannot append digit then return overflow error */
				}
				else																	/* If character is not a valid hexadecimal digit */
					State = 8;													/* stop reading number													 */
				break;
		}

		if (State != 8)														/* If not at End state 							*/
			Pos++;																	/* then move position to next char  */
	}

	while (Pos < Len)
	{
		if (iswhitespace (SNum[Pos]))							/* Move position to first non-whitespace char after number */
			Pos++;
		else
			break;
	}

	if (!bFoundDigit)														/* If Input Parameter string contains no digits 											*/
		Err = SCPI_ERR_PARAM_TYPE;								/* then parameter is not a number (e.g. "+." or "." are not numbers)	*/

	/* Determine if the exit state from FSM was a valid one to end on */
	switch (State)
	{
		case 2: case	3: case	 4: case	7: case  8:
	  case 9: case 11: case 12: case 13:				/* It is valid to exit the FSM in any of these states */
			break;
		default:																	/* It is invalid to exit the FSM in any other state 	*/
			Err = SCPI_ERR_PARAM_TYPE;							/* so return error code																*/
	}

	if (Err == SCPI_ERR_NONE)
	{
		if (bNegExp)															/* If exponent is flagged as negative */
			iExp = 0 - iExp;												/* then make the exponent negative		*/

		iExp -= iDecPlaces;												/* Adjust exponent by the number of decimal places */

		/* Get rid of all non-significant figures in integer component */
		while ((ulSigFigs % 10 == 0) && ulSigFigs) /* Loop while least significant figure digit is zero */
		{
			ulSigFigs = ulSigFigs / 10;							/* Remove last zero 											*/
			iExp++;																	/* and increment exponent to balance this	*/
		}

		if (iExp > MAX_EXPONENT)									/* If exponent is too big															*/
			Err = SCPI_ERR_PARAM_OVERFLOW;					/* then exit function, returning overflow error code	*/

		if (iExp < MIN_EXPONENT)									/* If exponent is too small (i.e. too many decimal places */
		{																					/* before the first significant digit)										*/
			ulSigFigs = 0;													/* then the number is effectively zero										*/
			iExp = 0;
		}

		if (iExp < 0)															/* If exponent is negative							*/
		{
			iExp = 0 - iExp;												/* Make it positive											*/
			bNegExp = TRUE;													/* and set flag "exponent is negative"	*/
		}
		else																			/* If exponent is positive						*/
			bNegExp = FALSE;												/* clear flag "exponent is negative"	*/
	}

	if (Err == SCPI_ERR_NONE)
	{
		/* Populate members of returned numeric value structure */
		psNum->ulSigFigs = ulSigFigs;
		psNum->bNeg = bNeg;
		psNum->Exp = cabs((signed char)iExp);
		psNum->bNegExp = bNegExp;

		*pNextPos = Pos;													/* Return index of next char after number	*/
	}

	return Err;
}


/**************************************************************************************/
/* Translates an Input Units string into units, translating according to a						*/
/* Parameter Spec's Nuerical Value Attributes.																				*/
/*																																										*/
/* Parameters:																																				*/
/*	[in]	SUnits			- Pointer to start of Input Units string												*/
/*	[in]	UnitsLen		- Length of Input Units string																	*/
/*	[in]	pSpecAttr		- Pointer to Parameter Spec's Numeric Value Attributes					*/
/*	[out]	peUnits			- Pointer to returned type of units															*/
/*	[out] pUnitExp		- Pointer to returned exponent of units													*/
/*											(contents are undefined if an error code is returned)					*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK:			Translation succeeded														*/
/*	SCPI_ERR_PARAM_UNITS		-	Error:	The units were invalid for the Parameter Spec		*/
/**************************************************************************************/
UCHAR TranslateUnits (char *SUnits, SCPI_CHAR_IDX UnitsLen, const struct strSpecAttrNumericVal *pSpecAttr,
 enum enUnits *peUnits, signed char *pUnitExp)
{
	UCHAR Err = SCPI_ERR_NONE;
	BOOL bMatch = FALSE;
	UCHAR IdxUnit = 0;
	SCPI_CHAR_IDX PosSpec;
	SCPI_CHAR_IDX PosInpUnits;
	char ChSpec;
	char ChInpUnits;
	const enum enUnits *peAltUnits;

	while (!bMatch && (sSpecUnits[IdxUnit].SUnit[0])) /* Loop until a match is found or have tried all Unit Spec strings */
	{
		PosSpec = 0;															/* Reset position within Units Spec string to start of units */
		PosInpUnits = 0;													/* Start at first character in the Input Units string */

		ChSpec = sSpecUnits[IdxUnit].SUnit[PosSpec]; /* Get character from Unit Spec string */

		do
		{
			ChInpUnits = SUnits[PosInpUnits];				/* Get character from Input Units string */

			if (tolower (ChInpUnits) == tolower (ChSpec))	/* If characters in Input Units string and Unit Spec string match */
			{
				PosSpec++;																	/* Go to next position in Unit Spec string 												*/
				ChSpec = sSpecUnits[IdxUnit].SUnit[PosSpec];
				PosInpUnits++;															/* Go to next position in Input Units string											*/
			}
			else																		/* If characters do not match */
			{
				if (iswhitespace (ChInpUnits))				/* If character in Input Units string is whitespace */
					PosInpUnits++;											/* just skip it																			*/
				else																	/* If characters do not match and are not whitespace 				*/
					break;															/* then this Unit Spec string does not match - try next one	*/
			}
		} while ((PosInpUnits < UnitsLen) && ChSpec);	/* Loop until end of Input Units string or end of Unit Spec string */

		if ((PosInpUnits == UnitsLen) && !ChSpec)	/* If reached end of Input Units string and end of Unit Spec string */
			bMatch = TRUE;													/* then it is a successful match																		*/
		else
			IdxUnit++;															/* If not a match then try the next Unit Spec string */
	}

	if (bMatch)																	/* If a match was found */
	{
		*peUnits = sSpecUnits[IdxUnit].eUnits;		/* Get type of unit from Unit Spec */
		*pUnitExp = sSpecUnits[IdxUnit].Exp;			/* and get unit exponent from Unit Spec */

		/* Now determine if units are valid for the Parameter Spec's Numeric Value Attributes */

		if (pSpecAttr->eUnits != *peUnits)				/* If units are not the same as Parameter Spec's default units */
		{
			/* Check if units match any of Parameter Spec's alternative units */
			bMatch = FALSE;
			peAltUnits = pSpecAttr->peAlternateUnits; /* Set pointer to start of list of spec's alternative units */
			if (peAltUnits)													/* If there are some alternate units */
			{
				while (!bMatch && (*peAltUnits != U_END)) /* Loop until match found or reached end of list of alternative units */
				{
					if (*peAltUnits == *peUnits)				/* If alternative units match the units from the Input Units string */
						bMatch = TRUE;										/* then found a match																								*/
					else
						peAltUnits++;											/* If alternative units don't match then look at next alternative units */
				};
			};
			if (!bMatch)														/* If units do not match any of the units allowed by the Parameter Spec */
				Err =	 SCPI_ERR_PARAM_UNITS;					/* then return error code 																							*/
		}
	}
	else																				/* If Input Unit string does not match any known units 	*/
		Err = SCPI_ERR_PARAM_UNITS;								/* then return error code																*/

	return Err;
}


/**************************************************************************************/
/* Translates an Input Parameter string into a String or Unquoted String parameter.		*/
/*																																										*/
/* Parameters:																																				*/
/*	[in]	SParam			- Pointer to start of Input Parameter string										*/
/*	[in]	ParLen			- Length of Input Parameter string 															*/
/*	[in]	ePType			- Type of parameter to translate to (String or Unquoted String)	*/
/*	[out]	psParam			- Pointer to returned parameter structure												*/
/*											(contents are undefined if an error code is returned)					*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK:			Translation succeeded														*/
/*	SCPI_ERR_UNMATCHED_QUOTE- Error:  Unmatched quotation mark in Input Parameter			*/
/*	SCPI_ERR_PARAM_TYPE			-	Error:	The Input Parameter was the wrong type for the	*/
/*																	  type of parameter requested											*/
/**************************************************************************************/
UCHAR TranslateStringParam (char *SParam, SCPI_CHAR_IDX ParLen,
 const enum enParamType ePType, struct strParam *psParam)
{
	UCHAR Err = SCPI_ERR_NONE;
	BOOL bInsideQuotes = TRUE;
	char ChQuote;
	SCPI_CHAR_IDX Pos;

	if (ePType == P_STR)												/* If required parameter type is a quoted string */
	{
		if (ParLen > 0)														/* If parameter is not zero length */
		{
			switch (SParam[0])											/* Look at the first character of the parameter */
			{
				case SINGLE_QUOTE:	ChQuote = SINGLE_QUOTE;	break;	/* Expression delimited by single quotes */
				case DOUBLE_QUOTE:	ChQuote = DOUBLE_QUOTE; break;	/* Expression delimited by double quotes */
				default:						Err = SCPI_ERR_PARAM_TYPE;			/* Any other character is invalid */
			}

			/* Loop through each char in the parameter, or until an error */
			for (Pos = 1; (Pos < ParLen) && (Err == SCPI_ERR_NONE); Pos++)
			{
				if (SParam[Pos] == ChQuote)						/* If a delimiting quote is encountered */
					bInsideQuotes = !bInsideQuotes;			/* then toggle "inside quotes" state 		*/

				else																	/* If encountered any other character */
					if (!bInsideQuotes)									/* and it is not inside quotes 				*/
						Err = SCPI_ERR_PARAM_TYPE;				/* then the parameter is not a valid quoted string */
			}
			if ((Err == SCPI_ERR_NONE) && bInsideQuotes)	/* If still inside quotes at the end of the parameter */
				Err = SCPI_ERR_UNMATCHED_QUOTE;							/* then the quote was not matched to another one		  */
		}
		else																			/* Parameter is zero length						*/
			Err = SCPI_ERR_PARAM_TYPE;							/* so it is not a valid quoted string */
	}

	if (Err == SCPI_ERR_NONE)										/* If tranlsation is valid */
	{
		psParam->eType = ePType;									/* Populate returned parameter structure */

		if (ePType == P_STR)																/* If quoted string 																	*/
		{
			psParam->unAttr.sString.pSString = &(SParam[1]); 	/* Set pointer to first character after opening quote */
			psParam->unAttr.sString.Len = ParLen - 2;					/* Set length to exclude opening & closing quotes 		*/
			psParam->unAttr.sString.Delimiter = ChQuote;			/* Set character that was used to delimit the string	*/
		}
		else																								/* If unquoted string 							*/
		{
			psParam->unAttr.sString.pSString = SParam; 				/* Set pointer to start of parameter */
			psParam->unAttr.sString.Len = ParLen;							/* Set length to length of parameter */
			psParam->unAttr.sString.Delimiter = 0;						/* Clear delimiter - not applicable for unquoted strings */
		}
	}

	return Err;
}


#ifdef SUPPORT_EXPR
/**************************************************************************************/
/* Translates an Input Parameter string into an Expression parameter.									*/
/*																																										*/
/* Parameters:																																				*/
/*	[in]	SParam			- Pointer to start of Input Parameter string										*/
/*	[in]	ParLen			- Length of Input Parameter string															*/
/*	[out]	psParam			- Pointer to returned parameter structure												*/
/*											(contents are undefined if an error code is returned)					*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE							- OK:			Translation succeeded													*/
/*	SCPI_ERR_UNMATCHED_BRACKET- Error:  Unmatched bracket in Input Parameter					*/
/*	SCPI_ERR_PARAM_TYPE				-	Error:	The Input Parameter could not be translated		*/
/*																	  	as an Expression															*/
/**************************************************************************************/
UCHAR TranslateExpressionParam (char *SParam, SCPI_CHAR_IDX ParLen, struct strParam *psParam)
{
	UCHAR Err = SCPI_ERR_NONE;
	UCHAR Brackets = 0;
	SCPI_CHAR_IDX Pos;

	if (ParLen > 0)															/* If parameter is not zero-length */
	{
		/* Loop through each character in Input Parameter, or until an error */
		for (Pos = 0; (Pos < ParLen) && (Err == SCPI_ERR_NONE); Pos++)
		{
			switch (SParam[Pos])
			{
				case OPEN_BRACKET:
					Brackets++;													/* Increment bracket nesting level counter */
					break;
				case CLOSE_BRACKET:
					if (Brackets)												/* If within one bracket or more */
					{
						Brackets--;												/* then decrement the bracket nesting level counter */
						if (!Brackets && (Pos < ParLen-1))	/* If now outside the brackets but there are more characters to go */
							Err = SCPI_ERR_PARAM_TYPE;				/* then this is not a valid expression 										 				 */
					}
					else																/* If not within a bracket */
						Err = SCPI_ERR_UNMATCHED_BRACKET;	/* then this closing bracket is unmatched */
					break;
				default:
					if (!Brackets)											/* If any other charcater encountered outside all brackets */
						Err = SCPI_ERR_PARAM_TYPE;				/* then this is not a valid expression 										 */
			}
		}
		if ((Err == SCPI_ERR_NONE) && Brackets)		/* If at end of parameter the bracket nesting level is not zero */
			Err = SCPI_ERR_UNMATCHED_BRACKET;				/* then there is at least one opening bracket without a matching close */
	}

	else																				/* If parameter is zero-length */
		Err = SCPI_ERR_PARAM_TYPE;								/* then return error code			 */

	if (Err == SCPI_ERR_NONE)										/* If the Input Parameter is a valid Expression */
	{
		psParam->eType = P_EXPR;									/* Populate returned parameter structure */
		psParam->unAttr.sString.pSString = SParam;	/* Set pointer to first character of Input Parameter */
		psParam->unAttr.sString.Len = ParLen;			/* Set length to length of Input Parameter */
	}

	return Err;
}
#endif


#ifdef SUPPORT_NUM_LIST
/**************************************************************************************/
/* Translates an Input Parameter string into a Numeric List parameter.								*/
/*																																										*/
/* Parameters:																																				*/
/*	[in]	SParam			- Pointer to start of Input Parameter string										*/
/*	[in]	ParLen			- Length of Input Parameter string															*/
/*	[in]	pSpecAttr		- Pointer to numeric list attributes parameter specification		*/
/*	[out]	psParam			- Pointer to returned parameter structure												*/
/*											(contents are undefined if an error code is returned)					*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK:			Translation succeeded														*/
/*	SCPI_ERR_PARAM_TYPE			-	Error:	The Input Parameter was the wrong type for the	*/
/*																	  type of parameter requested											*/
/*  SCPI_ERR_INVALID_VALUE	- Error:  One or more values in the list is invalid, i.e.	*/
/*																		floating point when not allowed, out of range		*/
/**************************************************************************************/
UCHAR TranslateNumericListParam (char *SParam, SCPI_CHAR_IDX ParLen, const struct strSpecAttrNumList *pSpecAttr,
 struct strParam *psParam)
{
	struct strParam sNumParam;
	struct strAttrNumericVal *psNum;
	long lNum;
	SCPI_CHAR_IDX NextPos;
	SCPI_CHAR_IDX Pos;
	BOOL bRange = FALSE;
	UCHAR Err = SCPI_ERR_NONE;

	sNumParam.eType = P_NUM;										/* Set up temporary numeric value parameter for validation purposes */
	psNum = &(sNumParam.unAttr.sNumericVal);

	if (ParLen > 2)															/* A Numeric List is at least 3 characters long */
	{
		if ((SParam[0] == OPEN_BRACKET) && (SParam[ParLen-1] == CLOSE_BRACKET))	/* Must be enclosed by brackets */
		{
			Pos = 1;
			while ((Pos < ParLen-1) && (Err != SCPI_ERR_PARAM_TYPE))	/* Loop through all characters or until error */
			{
				if (TranslateNumber (&(SParam[Pos]), ParLen - Pos, psNum, &NextPos) != SCPI_ERR_NONE)
				{																			/* If could not translate as a number 							 */
					Err = SCPI_ERR_PARAM_TYPE;					/* then not a numeric list, so set return error code */
					break;															/* and exit the loop 																 */
				}

				/* Validate number according to parameter specifications */

				if (!(pSpecAttr->bReal))							/* If real (non-integer) numbers are not allowed */
					if (psNum->bNegExp)									/* If number has a negative exponent i.e. has digits after decimal place */
						Err = SCPI_ERR_INVALID_VALUE;			/* then the value is invalid 																						 */

				if (!(pSpecAttr->bNeg))								/* If negative numbers are not allowed */
					if (psNum->bNeg)										/* If number is negative			*/
						Err = SCPI_ERR_INVALID_VALUE;			/* then the value is invalid	*/

				if (pSpecAttr->bRangeChk)							/* If range checking is required */
				{
					if (SCPI_ParamToLong (&sNumParam, &lNum) == SCPI_ERR_NONE)	/* If value is a valid long number */
					{
						if ((lNum < pSpecAttr->lMin) || (lNum > pSpecAttr->lMax))	/* If value is outside allowed range */
							Err = SCPI_ERR_INVALID_VALUE;														/* then value is invalid 						 */
					}
					else																/* If value is not a valid long integer, e.g. overflows */
						Err = SCPI_ERR_INVALID_VALUE;			/* then value is invalid 																*/
				}

				Pos += NextPos;												/* Move to first character after the number */

				if (Pos < ParLen-1)										/* If there are more characters inside the brackets */
				{
					switch (SParam[Pos])
					{
						case RANGE_SEP:										/* If encountered a range separator character */
							if (!bRange)										/* If not already decoding a range */
								bRange = TRUE;								/* then set range flag 						 */
							else														/* If already in a range					 														*/
								Err = SCPI_ERR_PARAM_TYPE;		/* then error - cannot have two range separators in one entry */
							break;
						case ENTRY_SEP:										/* If encountered an entry separator character */
							bRange = FALSE;									/* Clear range flag */
							break;
						default:													/* Any other character */
							Err = SCPI_ERR_PARAM_TYPE;			/* is an error 				 */
					}
					Pos++;															/* Go to next position in parameter */
					if (Pos == ParLen-1)								/* If reached end of characters within brackets */
						Err = SCPI_ERR_PARAM_TYPE;				/* then error - must finish on a number 				*/
				}
			}
		}
		else
			Err = SCPI_ERR_PARAM_TYPE;							/* Not enclosed by brackets */
	}
	else
		Err = SCPI_ERR_PARAM_TYPE;								/* Too short to be a Numeric List */

	if (Err == SCPI_ERR_NONE)										/* If parameter translated ok as a Numeric List */
	{
		psParam->eType = P_NUM_LIST;											/* Populate returned parameter */
		psParam->unAttr.sString.pSString = &(SParam[1]);	/* Set pointer to first char within brackets */
		psParam->unAttr.sString.Len = ParLen - 2;					/* Set length to exclude brackets */
	}

	return Err;
}
#endif


#ifdef SUPPORT_CHAN_LIST
/**************************************************************************************/
/* Translates an Input Parameter string into a Channel List parameter.								*/
/*																																										*/
/* Parameters:																																				*/
/*	[in]	SParam			- Pointer to start of Input Parameter string										*/
/*	[in]	ParLen			- Length of Input Parameter string															*/
/*	[in]	pSpecAttr		- Pointer to channel list attributes parameter specification		*/
/*	[out]	psParam			- Pointer to returned parameter structure												*/
/*											(contents are undefined if an error code is returned)					*/
/*																																										*/
/* Return Values:																																			*/
/*	SCPI_ERR_NONE						- OK:			Translation succeeded														*/
/*	SCPI_ERR_PARAM_TYPE			-	Error:	The Input Parameter was the wrong type for the	*/
/*																	  type of parameter requested											*/
/*  SCPI_ERR_INVALID_VALUE	- Error:  One or more values in the list is invalid, i.e.	*/
/*																		floating point when not allowed, out of range		*/
/*	SCPI_ERR_INVALID_DIMS		- Error:	Invalid number of dimensions in one of more			*/
/*																		of the channel list's entries										*/
/**************************************************************************************/
UCHAR TranslateChannelListParam (char *SParam, SCPI_CHAR_IDX ParLen, const struct strSpecAttrChanList *pSpecAttr,
 struct strParam *psParam)
{
	struct strParam sNumParam;
	struct strAttrNumericVal *psNum;
	long lNum;
	SCPI_CHAR_IDX NextPos;
	SCPI_CHAR_IDX Pos;
	BOOL bRange = FALSE;
	UCHAR DimFirst = 1;
	UCHAR DimLast = 1;
	UCHAR Err = SCPI_ERR_NONE;

	sNumParam.eType = P_NUM;										/* Set up temporary numeric value parameter for validation purposes */
	psNum = &(sNumParam.unAttr.sNumericVal);

	if (ParLen > 3)															/* A Channel List is at least 4 characters long */
	{
		if ((SParam[0] == OPEN_BRACKET) && (SParam[ParLen-1] == CLOSE_BRACKET)	/* Must be enclosed by brackets  */
		 && (SParam[1] == '@'))																									/* and have @ in second position */
		{
			Pos = 2;
			while ((Pos < ParLen-1) && (Err != SCPI_ERR_PARAM_TYPE))	/* Loop through all characters or until serious error */
			{
				if (TranslateNumber (&(SParam[Pos]), ParLen - Pos, psNum, &NextPos) != SCPI_ERR_NONE)
				{																			/* If could not translate as a number 							 */
					Err = SCPI_ERR_PARAM_TYPE;					/* then not a channel list, so set return error code */
					break;															/* and exit the loop 																 */
				}

				/* Validate number according to parameter specifications */

				if (!(pSpecAttr->bReal))							/* If real (non-integer) numbers are not allowed */
					if (psNum->bNegExp)									/* If number has a negative exponent i.e. has digits after decimal place */
						Err = SCPI_ERR_INVALID_VALUE;			/* then the value is invalid 																						 */

				if (!(pSpecAttr->bNeg))								/* If negative numbers are not allowed */
					if (psNum->bNeg)										/* If number is negative			*/
						Err = SCPI_ERR_INVALID_VALUE;			/* then the value is invalid	*/

				if (pSpecAttr->bRangeChk)							/* If range checking is required */
				{
					if (SCPI_ParamToLong (&sNumParam, &lNum) == SCPI_ERR_NONE)	/* If value is a valid long number */
					{
						if ((lNum < pSpecAttr->lMin) || (lNum > pSpecAttr->lMax))	/* If value is outside allowed range */
							Err = SCPI_ERR_INVALID_VALUE;														/* then value is invalid 						 */
					}
					else																/* If value is not a valid long integer, e.g. overflows */
						Err = SCPI_ERR_INVALID_VALUE;			/* then value is invalid 																*/
				}

				Pos += NextPos;												/* Move to first character after the number */

				if (Pos < ParLen-1)										/* If there are more characters inside the brackets */
				{
					switch (SParam[Pos])
					{
						case DIM_SEP:											/* If encountered dimension separator character */
							if (bRange)											/* If in the last part of the entry's range */
								DimLast++;										/* then increment last dimensions counter		*/
							else
								DimFirst++;										/* otherwise increment first dimensions counter */
							break;
						case RANGE_SEP:										/* If encountered a range separator character */
							if (!bRange)										/* If not already decoding a range */
								bRange = TRUE;								/* then set range flag 						 */
							else														/* If already in a range					 														*/
								Err = SCPI_ERR_PARAM_TYPE;		/* then error - cannot have two range separators in one entry */
							break;
						case ENTRY_SEP:										/* If encountered an entry separator character */
							if ((DimFirst < pSpecAttr->DimMin) || (DimFirst > pSpecAttr->DimMax))
								Err = SCPI_ERR_INVALID_DIMS;	/* Error if first dimensions are outside allowed limits */
							if (bRange)											/* If entry is a range */
								if ((DimLast < pSpecAttr->DimMin) || (DimLast > pSpecAttr->DimMax) || (DimFirst != DimLast))
									Err = SCPI_ERR_INVALID_DIMS; /* Error if last dimensions outside limits or dimensions do not match */
							bRange = FALSE;									/* Clear range flag */
							DimFirst = 1;										/* Reset dimensions counters for next entry */
							DimLast = 1;
							break;
						default:													/* Any other character */
							Err = SCPI_ERR_PARAM_TYPE;			/* is an error 				 */
					}
					Pos++;															/* Go to next position in parameter */
					if (Pos == ParLen-1)								/* If reached end of characters within brackets */
						Err = SCPI_ERR_PARAM_TYPE;				/* then error - must finish on a number 				*/
				}
				else																	/* If there are nmo more characters after the number */
				{
					if ((DimFirst < pSpecAttr->DimMin) || (DimFirst > pSpecAttr->DimMax))
						Err = SCPI_ERR_INVALID_DIMS;			/* Error if first dimensions are outside allowed limits */
					if (bRange)													/* If entry is a range */
						if ((DimLast < pSpecAttr->DimMin) || (DimLast > pSpecAttr->DimMax) || (DimFirst != DimLast))
							Err = SCPI_ERR_INVALID_DIMS;		/* Error if last dimensions outside limits or dimensions do not match */
				}
			}
		}
		else
			Err = SCPI_ERR_PARAM_TYPE;							/* Not enclosed by brackets */
	}
	else
		Err = SCPI_ERR_PARAM_TYPE;								/* Too short to be a Channel List */

	if (Err == SCPI_ERR_NONE)										/* If parameter translated ok as a Channel List */
	{
		psParam->eType = P_CHAN_LIST;											/* Populate returned parameter */
		psParam->unAttr.sString.pSString = &(SParam[2]);	/* Set pointer to first char after '@' symbol */
		psParam->unAttr.sString.Len = ParLen - 3;					/* Set length to exclude brackets and '@' */
	}

	return Err;
}
#endif


/**************************************************************************************/
/* Resets Command Tree to root																												*/
/**************************************************************************************/
void ResetCommandTree (void)
{
	mCommandTreeSize = 0;
	mCommandTreeLen = 0;
}


/**************************************************************************************/
/* Sets Command Tree attributes to the command tree of a Command Spec.								*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] CmdSpecNum		- Number of Command Spec																				*/
/**************************************************************************************/
void SetCommandTree (SCPI_CMD_NUM CmdSpecNum)
{
	SCPI_CHAR_IDX Pos;

	mSCommandTree = SSpecCmdKeywords[CmdSpecNum];			/* Set start of Command Tree to first character in
																										 	 command keyowrds of Command Spec								*/

	mCommandTreeSize = (SCPI_CHAR_IDX)strlen (mSCommandTree);	/* Set size of Command Tree to length of keywords */

	/* Reduce size of Command Tree to exclude last keyword in Command Spec's command keywords */
	while (mCommandTreeSize > 0)
	{
		if (mSCommandTree[mCommandTreeSize-1] == KEYW_SEP)
			break;
		mCommandTreeSize--;
	}

	/* Determine length of Command Tree, where length equals size minus count of square-bracket characters */
	mCommandTreeLen = 0;
	for (Pos = 0; Pos < mCommandTreeSize; Pos++)
		if ((mSCommandTree[Pos] != '[') && (mSCommandTree[Pos] != ']'))
			mCommandTreeLen++;
}


/**************************************************************************************/
/* Returns a character from a position within the Full Command version of an Input		*/
/* Command string (where position 0 is the first character).													*/
/* Note: The Full Command version of the Input Command string is the Command Tree			*/
/* concatenated with the Input Command string.																				*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] SInpCmd		- Pointer to start of Input Command string												*/
/*	[in] Pos				- Position within Full Command																		*/
/*	[in] Len				- Length of Full Command version of the Input Command string			*/
/*																																										*/
/* Return Value:																																			*/
/*	Character at position, or '\0' if position is beyond end of Full Command version	*/
/*  of Input Command string																														*/
/**************************************************************************************/
UCHAR CharFromFullCommand (char *SInpCmd, SCPI_CHAR_IDX Pos, SCPI_CHAR_IDX Len)
{
	char Ch;
	SCPI_CHAR_IDX Cnt = 0;
	SCPI_CHAR_IDX j = 0;

	if (Pos < mCommandTreeLen)									/* If character position is within Command Tree */
	{
		do
		{
			Ch = mSCommandTree[j];									/* Read character from Command Tree */
			if ((Ch != '[') && (Ch != ']'))					/* If character is not a square bracket						*/
				Cnt++;																/* then increment the count of characters reached */
			j++;																		/* Increment character position inside Command Tree */
		}
		while (Cnt < Pos + 1);										/* Loop until reached Pos'th character in Command Tree */
	}
	else																				/* If character position is beyond Command Tree */
	{
		if (Pos < Len)														/* If position within Full Command version of Input Command string 		*/
			Ch = SInpCmd[Pos - mCommandTreeLen];		/* then read character from Input Command string, taking into account
																							   the length of the Command Tree																			*/
		else																			/* If position beyond end of Full Command version of Input Command string */
			Ch = '\0';															/* then return the null char																							*/
	}

	return Ch;
}


/**************************************************************************************/
/* Appends a least significant digit to an unsigned long variable's existing value.		*/
/* i.e.:  ulVal(new) = ulVal(old) * Base + Digit																			*/
/*																																										*/
/* Parameters:																																				*/
/*	[in/out] pulVal	- Pointer to unsigned long variable to append digit to;						*/
/*									  if no overflow occurs then this is returned as the new value;		*/
/*										if an overflow does occur then this is returned unchanged.			*/
/*	[in] Digit			- Digit to append (0 to Base-1)																		*/
/*	[in] Base				- Base of number system that Digit is using.											*/
/*																																										*/
/* Return Values:																																			*/
/*	TRUE	- Digit was appended ok																											*/
/*	FALSE - Digit was not be appended as it would have resulted in an overflow of			*/
/*					the unsigned long variable																								*/
/**************************************************************************************/
BOOL AppendToULong (unsigned long *pulVal, char Digit, UCHAR Base)
{
	unsigned long ulNewVal;

	if (*pulVal <= ULONG_MAX / Base)						/* If shifting unsigned long a place to the left will not cause overflow */
	{
		ulNewVal = *pulVal * Base;								/* then shift the value by one place to the left 												 */
		if (ulNewVal <= ULONG_MAX - (unsigned long)Digit)	/* If appending digit will not cause an overflow 	 */
		{
			*pulVal = ulNewVal + (unsigned long)Digit;			/* then append digit to the unsigned long variable */
			return TRUE;																		/* Return no error 																 */
		}
	}
	return FALSE;																/* If overflow would have occurred then return error */
}


#ifdef SUPPORT_NUM_SUFFIX
/**************************************************************************************/
/* Appends a least significant decimal digit to an unsigned integer variable's				*/
/* existing value.																																		*/
/* i.e.:  Val(new) = Val(old) * 10 + Digit																						*/
/*																																										*/
/* Parameters:																																				*/
/*	[in/out] puiVal	- Pointer to unsigned integer variable to append digit to;				*/
/*										if no overflow occurs then this is returned as the new value;		*/
/*										if an overflow does occur then this is returned unchanged.			*/
/*	[in] Digit			- Digit to append (0-9).																					*/
/*																																										*/
/* Return Values:																																			*/
/*	TRUE	- Digit was appended ok																											*/
/*	FALSE - Digit was not be appended as it would have resulted in an overflow of 		*/
/*					the unsigned integer variable																							*/
/**************************************************************************************/
BOOL AppendToUInt (unsigned int *puiVal, char Digit)
{
	unsigned int uiNewVal;

	if (*puiVal <= UINT_MAX/10)									/* If shifting unsigned integer a place to the left will not cause overflow */
	{
		uiNewVal = *puiVal * 10;									/* then shift value one place to the left													 					*/
		if (uiNewVal <= UINT_MAX - (unsigned int)Digit)	/* If appending digit will not cause an onverflow */
		{
			*puiVal = uiNewVal + (unsigned int)Digit;			/* then append digit to the integer variable			*/
			return TRUE;																	/* Return no error																*/
		}
	}
	return FALSE;																/* If overflow would have occurred then return error */
}
#endif


/**************************************************************************************/
/* Appends a least significant decimal digit to an integer variable's existing value.	*/
/* i.e.:  Val(new) = Val(old) * 10 + Digit																						*/
/*																																										*/
/* Parameters:																																				*/
/*	[in/out] piVal	- Pointer to integer variable to append digit to;									*/
/*										if no overflow occurs then this is returned as the new value;		*/
/*										if an overflow does occur then this is returned unchanged.			*/
/*	[in] Digit			- Digit to append (0-9).																					*/
/*																																										*/
/* Return Values:																																			*/
/*	TRUE	- Digit was appended ok																											*/
/*	FALSE - Digit was not be appended as it would have resulted in an overflow of 		*/
/*					the integer variable																											*/
/**************************************************************************************/
BOOL AppendToInt (int *piVal, char Digit)
{
	int iNewVal;

	if (*piVal <= INT_MAX/10)										/* If shifting integer a place to the left will not cause overflow */
	{
		iNewVal = *piVal * 10;										/* then shift value one place to the left													 */
		if (iNewVal <= INT_MAX - (int)Digit)			/* If appending digit will not cause an onverflow */
		{
			*piVal = iNewVal + (int)Digit;					/* then append digit to the integer variable			*/
			return TRUE;														/* Return no error																*/
		}
	}
	return FALSE;																/* If overflow would have occurred then return error */
}


/**************************************************************************************/
/* Compares two strings (case-insensitive comparison)																	*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] S1		- Pointer to start of first string to compare														*/
/*	[in] Len1	- Length of first string																								*/
/*	[in] S2		- Pointer to start of second string to compare													*/
/*	[in] Len2 - Length of second string																								*/
/*																																										*/
/* Return Values:																																			*/
/*	TRUE	-	Strings are exactly equal (using case-insensitve comparison)							*/
/*	FALSE	- Strings are not equal																											*/
/**************************************************************************************/
BOOL StringsEqual (const char *S1, SCPI_CHAR_IDX Len1, const char *S2, SCPI_CHAR_IDX Len2)
{
	BOOL bEqual;
	SCPI_CHAR_IDX Pos;

	if (Len1 != Len2)
		return FALSE;

	bEqual = TRUE;
	for (Pos = 0; Pos < Len1; Pos++)
	{
		if (tolower (S1[Pos]) != tolower (S2[Pos]))
			bEqual = FALSE;
	}

	return bEqual;
}


/**************************************************************************************/
/* Returns the absolute (positive) value of a signed char variable.										*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] Val	- Signed char value																											*/
/*																																										*/
/* Return Value:																																			*/
/* 	Absolute version of signed value																									*/
/**************************************************************************************/
UCHAR cabs (signed char Val)
{
	if (Val > 0)
		return (UCHAR)Val;
	else
		return (UCHAR)(0 - Val);
}


/**************************************************************************************/
/* Returns value of a double-precision floating-point number rounded to the nearest		*/
/* integer, according to SCPI standard (defined in IEEE488.2 specification).					*/
/* The specification says: Round to nearest integer, rouding up if greater or equal		*/
/* to half values (.5), regardless of the sign of the number													*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] fdVal	- Double-precsion number to be rounded																*/
/*																																										*/
/* Return Value:																																			*/
/*	Rounded number																																		*/
/**************************************************************************************/
long round (double fdVal)
{
	if (fdVal > 0)
		fdVal += 0.5;
	else
		fdVal -= 0.5;
	return (long)fdVal;														/* Casting to long truncates the value to an integer */
}


/**************************************************************************************/
/* Determines if a character is whitespace																						*/
/*																																										*/
/* Parameters:																																				*/
/*	[in]	c - Character																																*/
/*																																										*/
/* Return Values:																																			*/
/*	TRUE 	- Character is whitespace (ASCII codes 1..32)																*/
/*	FALSE - Character is not whitespace																								*/
/**************************************************************************************/
BOOL iswhitespace (char c)
{
	if (c && (c < 33))
		return TRUE;
	else
		return FALSE;
}


/**************************************************************************************/
/* Substitutable Functions																														*/
/* -----------------------																														*/
/* The following functions are also found in the standard C function libraries.				*/
/* If you wish to use those versions of the functions instead, e.g. if your						*/
/* application already makes use those function elsewhere, then:											*/
/*																																										*/
/* 1) Comment out these functions here, and the corresponding function declarations		*/
/*    at the start of this module.																										*/
/* 2) Include the required header files of the standard C libraries in this module.		*/
/**************************************************************************************/
#if 0
#pragma function(strlen)
/**************************************************************************************/
/* Determines length of a string																											*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] S	- Null-terminated string.																									*/
/*																																										*/
/* Return Values:																																			*/
/*	Length of string																																	*/
/**************************************************************************************/
SCPI_CHAR_IDX strlen (const char *S)
{
	SCPI_CHAR_IDX Len;

   if (!S)
      return 0;

	Len = 0;
	while (S[Len])															/* Loop until reach a null char */
	{
		Len++;
		if (!Len)
			break;																	/* Break if Len overflowed back to 0 (i.e. string too long) */
	}
	return Len;
}


/**************************************************************************************/
/* Converts a character to its lowercase version																			*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] c	- Character																																*/
/*																																										*/
/* Return Value:																																			*/
/*	Lowercase version of character																										*/
/**************************************************************************************/
char tolower (char c)
{
	if (c > 0x40 && c < 0x5B)										/* Characters A..Z				*/
		c += 0x20;																/* are returned as a..z		*/
	return c;
}


/**************************************************************************************/
/* Determines if a character is a lowercase letter																		*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] c	- Character																																*/
/*																																										*/
/* Return Value:																																			*/
/*	TRUE 	- Character is a lowercase letter (a..z)																		*/
/*	FALSE	- Character is not a lowercase letter																				*/
/**************************************************************************************/
BOOL islower (char c)
{
	return ((c > 0x60) && (c < 0x7B)) ? TRUE : FALSE;
}


/**************************************************************************************/
/* Determines if a character is a decimal digit																				*/
/*																																										*/
/* Parameters:																																				*/
/*	[in] c	- Character																																*/
/*																																										*/
/* Return Value:																																			*/
/*	TRUE	- Character is a decimal digit (0..9)																				*/
/*	FALSE	- Character is not a decimal digit																					*/
/**************************************************************************************/
BOOL isdigit (char c)
{
	return ((c > 0x2F) && c < (0x3A)) ? TRUE : FALSE;
}
#endif