/**************************************************************************************/
/* Source Code Module for JPA-SCPI PARSER V1.3.1																			*/
/*																																										*/
/* (C) JPA Consulting Ltd., 2004	(www.jpacsoft.com)																	*/
/*																																										*/
/* View this file with tab spacings set to 2																					*/
/*																																										*/
/* scpi.h																																							*/
/* ======																																							*/
/*																																										*/
/* Module Description																																	*/
/* ------------------																																	*/
/* Header file for scpi.c. Contains definitions used throughout JPA-SCPI Parser				*/
/*																																										*/
/* Do not modify this file except where instructed in the code and/or documentation		*/
/*																																										*/
/* Refer to the JPA-SCPI Parser documentation for instructions as to using this code	*/
/* and notes on its design.																														*/
/*																																										*/
/* JPA-SCPI Parser Revision History																										*/
/* --------------------------------																										*/
/* V1.0.0:22/03/02:Release Candidate 1																								*/
/* V1.1.0:16/05/02:First Release Version - Fix bug when parsing non-decimal numbers,	*/
/*								 prefixes are now correct: #b, #q, #h; Fix bug matching sequences		*/
/*								 of optional command keywords so no longer possible to incorrectly	*/
/*								 match incomplete stems of keywords; Allow command keywords to			*/
/*								 include fixed suffixes.																						*/
/* V1.2.0:26/08/02:Summary of changes: Add support for channel lists; add support for */
/*								 numeric lists; add support for numeric suffices; add optional			*/
/*								 support feature #defines; allow optional text within character data*/
/*								 parameters. For full details refer to JPA-SCPI Parser documentation*/
/* V1.2.1:24/11/03:Fix bug when handling common commands embedded within compound 		*/
/*								 SCPI command strings - now recognises common commands, regardless	*/
/*								 of current command tree.																						*/
/* V1.3.0:08/07/04:Allow more than 255 command strings and more than 255 characters in*/
/*								 command line; Fix bug in SCPI_ParamToUnsignedLong().								*/
/* V1.3.1:24/11/04:Change type of element Len (in struct strAttrString) into					*/
/*								 SCPI_CHAR_IDX (was unsigned char) to be consistent with selectable */
/*								 limit of command line length; Removed unused parameter InpParamCnt */
/*								 from function TranslateParameters();	Change declaration of function*/
/*								 SCPI_ParamToString() in scpi.h to match definition in scpi.c	(was	*/
/*								 using out-of-date type for parameter pLen).												*/
/**************************************************************************************/


/* Only include this header file once */
#ifndef SCPI_H
#define SCPI_H

#ifdef __cplusplus
extern "C" {
#endif

/**************************************************************************************/
/***************** JPA-SCPI Parser Version Information Definitions ********************/
/**************************************************************************************/

#define SCPI_PARSER_VER_MAJOR		(1)
#define SCPI_PARSER_VER_MINOR		(3)
#define SCPI_PARSER_VER_REV			(1)


/**************************************************************************************/
/************** Common Substitutions used throughout JPA-SCPI Parser ******************/
/**************************************************************************************/

#ifndef BOOL
#define BOOL		unsigned char
#endif
#ifndef UCHAR
#define UCHAR		unsigned char
#endif
#ifndef TRUE
#define TRUE		(1)
#endif
#ifndef FALSE
#define FALSE		(0)
#endif


/**************************************************************************************/
/************ Enumerations and Definitions used throughout JPA-SCPI Parser ************/
/**************************************************************************************/

/* Parameter Types */
enum enParamType
{
	P_NONE,																			/* No Parameter 		*/
	P_NUM,																			/* Numeric Value 		*/
	P_BOOL,																			/* Boolean 					*/
	P_CH_DAT,																		/* Character Data 	*/
	P_STR,																			/* String 					*/
	P_UNQ_STR,																	/* Unquoted String 	*/
	P_EXPR,																			/* Expression 			*/
	P_NUM_LIST,																	/* Numeric List			*/
	P_CHAN_LIST																	/* Channel List			*/
};

#define END_OF_COMMANDS				""							/* Marks end of Command Spec command keyword entries 						*/
#define END_OF_UNITS					{"", U_NONE, 0}	/* Marks end of units																						*/
#define C_NO_DEF							(255)						/* Character Data - Indicates no default Item in Sequence 			*/


/**************************************************************************************/
/********************* Structures relating to Command Specs ***************************/
/**************************************************************************************/

/* Units Spec */
struct strSpecUnits
{
	const char									*SUnit;					/* Pointer to start of Units String (e.g. "UV") 										 */
	enum enUnits								eUnits;					/* Type of base units 																							 */
	signed char									Exp;						/* Exponent of units, e.g. -6 for microvolts if volts are base units */
};

/* Parameter Spec's Numeric Value Attributes */
struct strSpecAttrNumericVal
{
	enum enUnits								eUnits;					/* Type of units allowed (use these if no units in Input Parameter) */
	const enum enUnits					*peAlternateUnits;	/* Pointer to array of alternative unit types 									*/
	signed char									Exp;						/* Default unit exponent to use if no units in Input Parameter		 	*/
};

/* Parameter Spec's Boolean Attributes */
struct strSpecAttrBoolean
{
	unsigned										bHasDef	 : 1;		/* 1=Parameter has a default value, 0=no default value	*/
	unsigned										bDefOn 	 : 1;		/* Default value (1=On, 0=Off) 													*/
};

/* Parameter Spec's Character Data Attributes */
struct strSpecAttrCharData
{
	char const 									*SSeq;					/* Character Data Sequence 												 					 */
	unsigned char								DefItemNum;			/* Number of Default Character Data Item within Sequence 		 */
	enum enParamType						eAltType;				/* Alternative parameter type (or P_NONE if no alternatives) */
	const void									*pAltAttr;			/* Pointer to Parameter Spec of alternative parameter type 	 */
};

#ifdef SUPPORT_NUM_LIST
/* Parameter Spec's Numeric List Attributes */
struct strSpecAttrNumList
{
	unsigned									  bReal			: 1;	/* 1=Real (non-integer) numbers are allowed, 0=integers only */
	unsigned									  bNeg			: 1;	/* 1=Negative numbers are allowed, 0=positive only			 		 */
	unsigned										bRangeChk : 1;	/* 1=Perform range checking, 0=do not perform range checking */
	long												lMin;						/* Minimum value allowed 																		 */
	long												lMax;						/* Maximum value allowed																		 */
};
#endif

#ifdef SUPPORT_CHAN_LIST
/* Parameter Spec's Channel List Attributes */
struct strSpecAttrChanList
{
	unsigned									  bReal			: 1;	/* 1=Real (non-integer) numbers are allowed, 0=integers only */
	unsigned									  bNeg			: 1;	/* 1=Negative numbers are allowed, 0=positive only			 		 */
	unsigned										bRangeChk : 1;	/* 1=Perform range checking, 0=do not perform range checking */
	unsigned char								DimMin;					/* Minimum number of dimensions allowed									 		 */
	unsigned char								DimMax;					/* Maximum number of dimensions allowed 								 		 */
	long												lMin;						/* Minimum value allowed 																		 */
	long												lMax;						/* Maximum value allowed																		 */
};
#endif

/* Parameter Spec */
struct strSpecParam
{
	unsigned										bOptional : 1;	/* 1=parameter is optional, 0=parameter is required	*/
	enum enParamType						eType;					/* Type of parameter 																*/
	const void									*pAttr;					/* Pointer to additional attributes of parameter 		*/
};

/* Command Spec */
struct strSpecCommand
{
	struct strSpecParam sSpecParam[MAX_PARAMS];	/* Array of Parameter Specs of Command Spec */
};


/**************************************************************************************/
/***************** Declarations of Global Constants, defined in cmds.c ****************/
/**************************************************************************************/

extern const struct strSpecCommand sSpecCommand[];	/* Array of Command Specs 								*/
extern const char *SSpecCmdKeywords[];							/* Array of Command Spec command keywords */
extern const struct strSpecUnits sSpecUnits[];			/* Array of Unit Specs										*/


/**************************************************************************************/
/************** Structures relating to returned (populated) Parameters ****************/
/**************************************************************************************/

/* Attributes of a Numeric Value type parameter */
struct strAttrNumericVal
{
	unsigned long								ulSigFigs;			/* Significant Figures component of value 				*/
	unsigned char								Exp;						/* Exponent component of value 										*/
	unsigned										bNeg		: 1;		/* 1=Value is negative, 0=value is positive 			*/
	unsigned										bNegExp : 1;		/* 1=Exponent is negative, 1=exponent is positive */
	enum enUnits								eUnits;					/* Type of units 																	*/
};

/* Numeric Value type parameter Attribute flags */
#define SCPI_NUM_ATTR_NEG				(1)
#define SCPI_NUM_ATTR_REAL			(2)

/* Attributes of a Booolean type parameter */
struct strAttrBoolean
{
	BOOL												bVal;						/* Value: 1=On, 0=Off */
};

/* Attributes of a Character Data type parameter */
struct strAttrCharData
{
	unsigned char								ItemNum;				/* Number of Character Data Item, within Sequence */
	const char 									*SSeq;					/* Character Data Sequence */
};

/* Attributes of a String, Unquoted String, Expression, Channel List or Numeric List type parameter */
struct strAttrString
{
	char												*pSString;			/* Pointer to start of string */
	SCPI_CHAR_IDX								Len;						/* Length of string 					*/
	char												Delimiter;			/* Delimiting character				*/
};

/* Union of all types of attributes */
union uniParamAttr
{
	struct strAttrNumericVal	sNumericVal;
	struct strAttrBoolean			sBoolean;
	struct strAttrCharData		sCharData;
	struct strAttrString			sString;
};

/* Parameter structure */
struct strParam
{
	enum	enParamType						eType;					/* Type of parameter 														*/
	union uniParamAttr					unAttr;					/* Attributes specific to the type of parameter */
};


/**************************************************************************************/
/************* Error Codes returned by JPA-SCPI Parser Access Functions ***************/
/**************************************************************************************/

#define SCPI_ERR_NONE							 (0)				/* No error																											*/
#define SCPI_ERR_TOO_MANY_NUM_SUF	 (5)				/* DESIGN ERROR: Too many numeric suffices in Command Spec			*/
#define SCPI_ERR_NO_COMMAND				 (10)				/* No Input Command to parse																		*/
#define SCPI_ERR_NUM_SUF_INVALID	 (14)				/* Numeric suffix is invalid value															*/
#define SCPI_ERR_INVALID_VALUE		 (16)				/* Invalid value in numeric or channel list, e.g. out of range 	*/
#define SCPI_ERR_INVALID_DIMS			 (17)				/* Invalid number of dimensions in a channel list								*/
#define SCPI_ERR_PARAM_OVERFLOW		 (20)				/* Parameter of type Numeric Value overflowed its storage				*/
#define SCPI_ERR_PARAM_UNITS			 (30)				/* Wrong units for parameter																		*/
#define SCPI_ERR_PARAM_TYPE				 (40)				/* Wrong type of parameter(s)																		*/
#define SCPI_ERR_PARAM_COUNT			 (50)				/* Wrong number of parameters																		*/
#define SCPI_ERR_UNMATCHED_QUOTE	 (60)				/* Unmatched quotation mark (single/double) in parameters				*/
#define SCPI_ERR_UNMATCHED_BRACKET (65)				/* Unmatched bracket																						*/
#define SCPI_ERR_INVALID_CMD			 (70)				/* Command keywords were not recognized													*/
#define SCPI_ERR_NO_ENTRY					 (200)			/* No entry in list to retrieve (number list or channel list)		*/
#define SCPI_ERR_TOO_MANY_DIMS		 (210)			/* Too many dimensions in entry to be returned in parameters		*/


/**************************************************************************************/
/****************** Declarations of JPA-SCPI Parser Access Functions ******************/
/**************************************************************************************/

#ifdef SUPPORT_NUM_SUFFIX
UCHAR SCPI_Parse (char **pSInput, BOOL bResetTree, UCHAR *pCmdSpecNum, struct strParam sParam[],
 UCHAR *pNumSufCnt, unsigned int uiNumSuf[]);
#else
UCHAR SCPI_Parse (char **pSInput, BOOL bResetTree, UCHAR *pCmdSpecNum, struct strParam sParam[]);
#endif
UCHAR SCPI_ParamType (struct strParam *psParam, enum enParamType *pePType, UCHAR *pNumSubtype);
UCHAR SCPI_ParamUnits (struct strParam *psParam, enum enUnits *peUnits);
UCHAR SCPI_ParamToCharDataItem (struct strParam *psParam, UCHAR *pItemNum);
UCHAR SCPI_ParamToBOOL (struct strParam *psParam, BOOL *pbVal);
UCHAR SCPI_ParamToUnsignedInt (struct strParam *psParam, unsigned int *puiVal);
UCHAR SCPI_ParamToInt (struct strParam *psParam, int *piVal);
UCHAR SCPI_ParamToUnsignedLong (struct strParam *psParam, unsigned long *pulVal);
UCHAR SCPI_ParamToLong (struct strParam *psParam, long *plVal);
UCHAR SCPI_ParamToDouble (struct strParam *psParam, double *pfdVal);
UCHAR SCPI_ParamToString (struct strParam *psParam, char **pSString, SCPI_CHAR_IDX *pLen, char *pDelimiter);
#ifdef SUPPORT_NUM_LIST
UCHAR SCPI_GetNumListEntry (struct strParam *psParam, UCHAR Index, BOOL *pbRange, struct strParam *psFirst,
 struct strParam *psLast);
#endif
#ifdef SUPPORT_CHAN_LIST
UCHAR SCPI_GetChanListEntry (struct strParam *psParam, UCHAR Index, UCHAR *pDims,
 BOOL *pbRange, struct strParam sFirst[], struct strParam sLast[]);
#endif


#ifdef __cplusplus
}
#endif

#endif
