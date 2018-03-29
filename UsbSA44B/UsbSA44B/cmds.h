/**************************************************************************************/
/* JPA-SCPI PARSER SOURCE CODE MODULE																									*/
/* (C) JPA Consulting Ltd., 2004	(www.jpacsoft.com)																	*/
/*																																										*/
/* View this file with tab spacings set to 2																					*/
/*																																										*/
/* cmds.h																																							*/
/* ======																																							*/
/*																																										*/
/* Module Description																																	*/
/* ------------------																																	*/
/* Contains definitions specific to your compiler.																		*/
/* Contains information specific to the SCPI command set supported by your instrument.*/
/*																																										*/
/* Where indicated "USER", you will be instructed to modify the lines of code to			*/
/* support your instrument's requirements.																						*/
/*																																										*/
/* Full instructions regarding how to modify this file to suit your requirements is		*/
/* given in the JPA-SCPI PARSER USER MANUAL - Do not attempt to make modifications		*/
/* until you have read the documentation.																							*/
/*																																										*/
/* Module Revision History																														*/
/* -----------------------																														*/
/* V1.0.0:15/04/02:Initial Release																										*/
/* V1.1.0:29/08/02:Modified for compatibility with JPA SCPI Parser V1.2.0							*/
/* V1.2.0:08/07/04:Modify for JPA SCPI Parser V1.3.0: Add variable type definitions		*/
/**************************************************************************************/


/* Only include this header file once */
#ifndef CMDS_H
#define CMDS_H

#ifdef __cplusplus
extern "C" {
#endif


/**************************************************************************************/
/* Optional Support Features																													*/
/* -------------------------																													*/
/* USER: #define the features that you require and comment out those not required			*/
#define SUPPORT_NUM_SUFFIX				/* Numeric Suffix in keywords		 										*/
#define SUPPORT_NUM_LIST					/* Numeric List parameter type											*/
#define SUPPORT_CHAN_LIST					/* Channel List parameter type 											*/
#define SUPPORT_EXPR							/* Expression parameter type												*/
/**************************************************************************************/


/**************************************************************************************/
/* Variable Types																																			*/
/* --------------																																			*/
/* USER: If you require, modify the #defines below in order to change the types of		*/
/*			 variables used in the library.																								*/
#define SCPI_CHAR_IDX		unsigned char			/* Index to char in Input Command line			*/
#define SCPI_CMD_NUM		unsigned char			/* Command number														*/
/**************************************************************************************/


/**************************************************************************************/
/* Maximum Numeric Values supported by your Compiler																	*/
/* -------------------------------------------------																	*/
/* USER: Replace these values with the limits of the compiler you are using.					*/
/*			 Alternatively, if you wish you can replace these definitions with						*/
/*			 "#include <limits.h>", if your compiler provides that file.									*/
//#define ULONG_MAX		(0xFFFFFFFF)	/* Max possible val of an unsigned long integer			*/
//#define LONG_MAX		(0x7FFFFFFF)	/* Max possible val of a signed long integer				*/
//#define UINT_MAX		(0xFFFF)			/* Max possible val of an unsigned integer					*/
//#define INT_MAX			(0x7FFF)			/* Max possible val of a signed integer							*/
//#define	UCHAR_MAX		(0xFF)				/* Max possible val of an unsigned character 				*/
/**************************************************************************************/


/**************************************************************************************/
/* Base Unit Types																																		*/
/* ---------------																																		*/
/* USER: Add Base Unit Types supported by your instrument															*/
/*       Optional: Remove Base Unit Types not supported																*/
/**************************************************************************************/
enum enUnits
{
	U_NONE,								/* USER: Do not modify this line															*/

	U_VOLT,								/* User-modifiable list of supported base unit types					*/
	U_AMP,
	U_OHM,
	U_WATT,
	U_DB_W,
	U_JOULE,
	U_FARAD,
	U_HENRY,
	U_HERTZ,
	U_SEC,
	U_KELVIN,
	U_CELSIUS,
	U_FAHREN,

	U_END									/* USER: Do not modify this line															*/
};


/**************************************************************************************/
/* Maximum Parameters																																	*/
/* ------------------																																	*/
/* USER: Modify this value to be equal to the maximum number of parameter accepted		*/
/*			 by any of the supported Command Specs																				*/
#define MAX_PARAMS								(3)				/* Most params accepted by any command		*/
/**************************************************************************************/


#ifdef SUPPORT_NUM_SUFFIX
/**************************************************************************************/
/* Numeric Suffix																																			*/
/* --------------																																			*/
/* (only used if Numeric Suffix support feature is enabled)														*/
/*																																										*/
/* USER: Modify these values as required. See User Manual for more information.				*/
#define MAX_NUM_SUFFIX				(10)					/* Maximum number of numeric suffices			*/
																						/* possible in a single command						*/
#define NUM_SUF_MIN_VAL				(1)						/* Minimum value allowed (0 or greater)		*/
#define NUM_SUF_MAX_VAL				(UINT_MAX)		/* Maximum value allowed (<=UINT_MAX)			*/
#define NUM_SUF_DEFAULT_VAL		(1)						/* Default value if no suffix present			*/
/**************************************************************************************/
#endif


#ifdef SUPPORT_CHAN_LIST
/**************************************************************************************/
/* Maximum Dimensions allowed in a Channel List Entry 																*/
/* --------------------------------------------------																	*/
/* (only used if Channel List support feature is enabled)															*/
/*																																										*/
/* USER: Modify this value to be equal to the maximum number of dimensions that are		*/
/*       allowed in any of the channel list parameters.																*/
/* See User Manual for more information.																							*/
#define MAX_DIMS						(4)							/* Maximum dimensions in a channel list		*/
/**************************************************************************************/
#endif


#ifdef __cplusplus
}
#endif

#endif
