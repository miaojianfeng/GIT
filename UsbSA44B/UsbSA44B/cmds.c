/**************************************************************************************/
/* Digitizer SCPI Instrument Class Template 																					*/
/*																																										*/
/* Template Revision History																													*/
/* -------------------------																													*/
/* V1.0.0:16/05/02:Initial Release																										*/
/* V1.1.0:29/08/02:Modified for compatibility with JPA SCPI Parser V1.2.0;						*/
/* 								 Use numeric suffix instead of duplicate command specifications			*/
/*								 for commands that can operate on multiple channels;								*/
/*								 Make use of optional text and numeric suffix within character			*/
/*								 sequence SeqXTIMe to replace multiple options with one option			*/
/* V1.1.1:19/09/04:Fix typo in lines 409, 412 & 415 - extraneous trailing bracket			*/
/*								 in #ifdef statement that caused compilation errors now removed;		*/
/*								 Correct command definition TRIGger[:SEQuence]:SLOPe so now takes		*/
/*								 "EITHer" (not "EITher") as a possible character data parameter.	 	*/
/**************************************************************************************/


/**************************************************************************************/
/* JPA-SCPI PARSER SOURCE CODE MODULE																									*/
/* (C) JPA Consulting Ltd., 2002	(www.jpacsoft.com)																	*/
/*																																										*/
/* View this file with tab spacings set to 2																					*/
/*																																										*/
/* cmds.c																																							*/
/* ======																																							*/
/*																																										*/
/* Module Description																																	*/
/* ------------------																																	*/
/* Contains the specifications of the SCPI command set supported by your instrument.	*/
/*																																										*/
/* Where indicated "USER", you will be instructed to modify the lines of code to			*/
/* support your instrument's requirements.																						*/
/*																																										*/
/* Full instructions regarding how to modify this file to suit your requirements is		*/
/* given in the JPA-SCPI PARSER USER MANUAL - Do not attempt to make modifications		*/
/* until you have read the documentation.																							*/
/*																																										*/
/* JPA-SCPI Parser Revision History																										*/
/* --------------------------------																										*/
/* Refer to scpi.h for revision history																								*/
/**************************************************************************************/


/* USER: Include any headers required by your compiler here														*/
#include "cmds.h"
#include "scpi.h"


/**************************************************************************************/
/* Miscellaneous Definitions & Declarations used in this Module												*/
/* ------------------------------------------------------------												*/
/* USER: DO NOT MODIFY THESE LINES OF CODE																						*/
/*																																										*/
/* Boolean Param Spec Declarations																										*/
const struct strSpecAttrBoolean sBNoDef	 = {FALSE, 0};/* Boolean (no default value)		*/
const struct strSpecAttrBoolean sBDefOn	 = {TRUE, 1}; /* Boolean (default=1 [ON])			*/
const struct strSpecAttrBoolean sBDefOff = {TRUE, 0}; /* Boolean (default=0 [OFF])		*/
/*																																										*/
/* Numeric Value Param Spec Definitions																								*/
#define NAU								((enum enUnits *)0)					/* No Alternate Units						*/
#define ALT_UNITS_LIST		const enum enUnits					/* List of Alternate Units			*/
#define NUM_TYPE					const struct strSpecAttrNumericVal	/* Numeric Val Attribs	*/
/*																																										*/
/* Character Data Param Spec Definitions																							*/
#define CHDAT_SEQ					const char									/* Char Data Sequence						*/
#define CHDAT_TYPE				const struct strSpecAttrCharData	/* Char Data Attribs			*/
#define NO_DEF						(255)												/* No default item number				*/
#define ALT_NONE					P_NONE, (void *)(0)					/* No alternative type of param */
/*																																										*/
#ifdef SUPPORT_NUM_LIST
/* Numeric List Param Spec Definitions																								*/
#define NUMLIST_TYPE			const struct strSpecAttrNumList		/* Numeric List Attribs		*/
#endif
#ifdef SUPPORT_CHAN_LIST
/* Channel List Param Spec Definitions																								*/
#define CHANLIST_TYPE			const struct strSpecAttrChanList	/* Channel List Attribs		*/
#endif
/**************************************************************************************/


/**************************************************************************************/
/* Units Specs																																				*/
/* -----------																																				*/
/* USER: Add a new entry for each Units Spec supported that is not already listed.		*/
/*			 Optional: Remove entries that you do not support.														*/
/* Notes:																																							*/
/*		a) Do not include spaces within the keywords																		*/
/*		b) Characters are case-insensitive (recommended: enter all chars in Uppercase)	*/
/*		c) All strings must be unique																										*/
/**************************************************************************************/
const struct strSpecUnits sSpecUnits[] =
{
/*	Keyword		Base Unit			Unit Exponent
		-------		---------			-------------																							*/
/* Volts	                                                                            */
  { "NV",     U_VOLT,       -9 },   /* NanoVolt                                       */
  { "UV",     U_VOLT,       -6 },   /* MicroVolt                                      */
  { "MV",     U_VOLT,       -3 },   /* MilliVolt                                      */
  { "V",      U_VOLT,       0  },   /* Volt                                           */
  { "KV",     U_VOLT,       3  },   /* KiloVolt                                       */
  { "MAV",    U_VOLT,       6  },   /* MegaVolt                                       */
/* Amps		                                                                            */
  { "NA",     U_AMP,        -9 },   /* NanoAmp                                        */
  { "UA",     U_AMP,        -6 },   /* MicroAmp                                       */
  { "MA",     U_AMP,        -3 },   /* MilliAmp                                       */
  { "A",      U_AMP,        0  },   /* Amp                                            */
/* Ohms			                                                                         	*/
  { "UR",     U_OHM,        -6 },   /* MicroOhm                                       */
  { "UOHM",   U_OHM,        -6 },   /* MicroOhm                                       */
                                    /* (Note: no MilliOhms in SCPI - see MegaOhm)     */
  { "R",      U_OHM,        0  },   /* Ohm                                            */
  { "OHM",    U_OHM,        0  },   /* Ohm                                            */
  { "KR",     U_OHM,        3  },   /* KiloOhm                                        */
  { "KOHM",   U_OHM,        3  },   /* KiloOhm                                        */
  { "MR",     U_OHM,        6  },   /* MegaOhm (in SCPI, MR=MAR=MegaOhm)              */
  { "MAR",    U_OHM,        6  },   /* MegaOhm                                        */
  { "MOHM",   U_OHM,        6  },   /* MegaOhm                                        */
  { "MAOHM",  U_OHM,        6  },   /* MegaOhm                                        */
  { "GR",     U_OHM,        9  },   /* GigaOhm                                        */
  { "GOHM",   U_OHM,        9  },   /* GigaOhm                                        */
/* Watts				                                                                      */
  { "NW",     U_WATT,       -9 },   /* NanoWatt                                       */
  { "UW",     U_WATT,       -6 },   /* MicroWatt                                      */
  { "MW",     U_WATT,       -3 },   /* MilliWatt                                      */
  { "W",      U_WATT,       0  },   /* Watt                                           */
  { "KW",     U_WATT,       3  },   /* KiloWatt                                       */
  { "MAW",    U_WATT,       6  },   /* MegaWatt                                       */
  { "GW",     U_WATT,       9  },   /* GigaWatt                                       */
/* Decibel Watts																																			*/
	{	"DBNW",		U_DB_W,				-9 },		/* Decibel NanoWatt																*/
	{	"DBUW",		U_DB_W,				-6 },		/* Decibel MicroWatt															*/
	{	"DBM",		U_DB_W,				-3 },		/* Decibel MilliWatt															*/
	{	"DBMW",		U_DB_W,				-3 },		/* Decibel MilliWatt															*/
	{	"DBW",		U_DB_W,				0	 },		/* Decibel Watt																		*/
/* Joules                                                                             */
  { "UJ",     U_JOULE,      -6 },   /* MicroJoule                                     */
  { "MJ",     U_JOULE,      -3 },   /* MilliJoule                                     */
  { "J",      U_JOULE,      0  },   /* Joule                                          */
  { "KJ",     U_JOULE,      3  },   /* KiloJoule                                      */
/* Farads                                                                        			*/
  { "PF",     U_FARAD,      -12},   /* PicoFarad                                      */
  { "NF",     U_FARAD,      -9 },   /* NanoFarad                                      */
  { "UF",     U_FARAD,      -6 },   /* MicroFarad                                     */
  { "MF",     U_FARAD,      -3 },   /* MiliFarad                                      */
  { "F",      U_FARAD,      0  },   /* Farad                                          */
/* Henrys			                                                                        */
  { "UH",     U_HENRY,      -6 },   /* MicroHenry                                     */
  { "MH",     U_HENRY,      -3 },   /* MilliHenry                                     */
  { "H",      U_HENRY,      0  },   /* Henry                                          */
/* Hertz                                                                          		*/
  { "HZ",     U_HERTZ,      0  },   /* Hertz                                          */
  { "KHZ",    U_HERTZ,      3  },   /* KiloHertz                                      */
  { "MHZ",    U_HERTZ,      6  },   /* MegaHertz (in SCPI, MHZ=MAHZ=MegaHertz)        */
  { "MAHZ",   U_HERTZ,      6  },   /* MegaHertz                                      */
  { "GHZ",    U_HERTZ,      9  },   /* GigaHertz                                      */
/* Seconds                                                                            */
  { "PS",     U_SEC,        -12},   /* PicoSecond                                     */
  { "NS",     U_SEC,        -9 },   /* NanoSecond                                     */
  { "US",     U_SEC,        -6 },   /* MicroSecond                                    */
  { "MS",     U_SEC,        -3 },   /* MilliSecond                                    */
  { "S",      U_SEC,        0  },   /* Second                                         */
/* Temperature                                                                        */
  { "K",      U_KELVIN,     0  },   /* Degree Kelvin                                  */
  { "CEL",    U_CELSIUS,    0  },   /* Degree Celsius                                 */
  { "FAR",    U_FAHREN,     0  },   /* Degree Fahrenheit                              */

	END_OF_UNITS											/* USER: Do not modify this line									*/
};


/**************************************************************************************/
/* Alternative Units																																	*/
/* -----------------																																	*/
/* USER: Create a list for each set of Alternative Units supported (if any)						*/
/* Notes:																																							*/
/*			a) Always include U_END as last member of each list														*/
/**************************************************************************************/
ALT_UNITS_LIST  eAltDegCAndF[] = {U_CELSIUS, U_FAHREN, U_END};      /* Deg C & Deg F  */


/**************************************************************************************/
/* Numerical Value Types																															*/
/* ---------------------																															*/
/* USER: Create a structure for each type of Numerical Value supported								*/
/* Notes:																																							*/
/*		a) See JPA-SCPI Parser User Manual for details																	*/
/**************************************************************************************/
/*												Default		Alternative		Exponent of													*/
/*				Name						Units			Units					Default Units												*/
/*				-----						-------		-----------		-------------												*/
NUM_TYPE  sNoUnits 		= { U_NONE,   NAU,            0   };      /* No Units           */
NUM_TYPE  sVolts 			= { U_VOLT,   NAU,            0   };      /* Volts only         */
NUM_TYPE  sAmps 			= { U_AMP,    NAU,            0   };      /* Amps only          */
NUM_TYPE  sOhms 			= { U_OHM,    NAU,            0   };      /* Ohms only          */
NUM_TYPE  sWatts 			= { U_WATT,  	NAU,            0   };      /* Watts only         */
NUM_TYPE	sDBWatts		=	{ U_DB_W,		NAU,						0		};			/* Decibel Watts only	*/
NUM_TYPE	sJoules			=	{ U_JOULE,	NAU,						0		};			/* Joules only				*/
NUM_TYPE	sFarads			=	{ U_FARAD,	NAU,						0		};			/* Farads only				*/
NUM_TYPE	sHenrys			=	{ U_HENRY,	NAU,						0		};			/* Henrys only				*/
NUM_TYPE	sHertz 			=	{ U_HERTZ,	NAU,						0		};			/* Hertz only					*/
NUM_TYPE  sSecs 			= { U_SEC,    NAU,            0   };      /* Seconds only       */
NUM_TYPE	sKelvin			=	{ U_KELVIN,	NAU,						0		};			/* Deg Kelvin only		*/
NUM_TYPE	sCelsius 		=	{ U_CELSIUS,NAU,						0		};			/* Deg Celsius only		*/
NUM_TYPE	sFahren			=	{ U_FAHREN,	NAU,						0		};			/* Deg Fahrenheit only*/
NUM_TYPE  sTemperature= { U_KELVIN, eAltDegCAndF,   0   }; /* Kelvin; also allow C & F*/


/**************************************************************************************/
/* Character Data Sequences																														*/
/* ------------------------																														*/
/* USER: Create an entry for each Character Data Sequence supported.									*/
/* Notes:																																							*/
/*		a) Separate each Item in a Sequence with a pipe (|) char												*/
/*		b) Enter required characters in Uppercase, optional characters in Lowercase			*/
/*		c) Quotes (single and double) are allowed but must be matched										*/
/*		d) Do not include spaces within the strings																			*/
/**************************************************************************************/
/* 				Name				 						Sequence																						*/
/* 				----				 						---------------																			*/
CHDAT_SEQ	SeqMinMax[] 					=	"MINimum|MAXimum";
CHDAT_SEQ	SeqMinMaxDef[] 				=	"MINimum|MAXimum|DEFault";
CHDAT_SEQ SeqBusImmExt[]				= "BUS|IMMediate|EXTernal";
CHDAT_SEQ	SeqACDCGnd[]					= "AC|DC|GND";
CHDAT_SEQ SeqACDC[]							= "AC|DC";
CHDAT_SEQ SeqXTIMe[]						= "\"XTIMe:VOLTage#[:DC]\"";
CHDAT_SEQ SeqAscii[]						= "ASCii";
CHDAT_SEQ SeqPosNegEit[]				= "POSitive|NEGative|EITHer";
CHDAT_SEQ SeqInternal[]					= "INTernal#";
CHDAT_SEQ SeqTTMode[]           = "AIR|BOR|NRM|TWO";
CHDAT_SEQ SeqContinous[]        = "CONT|NONCONT";


/**************************************************************************************/
/* Character Data Types																																*/
/* --------------------																																*/
/* USER: Create a structure for each type of Character Data Sequence supported				*/
/*       Optional: Remove structures not required																			*/
/* Notes:																																							*/
/*		a) See JPA-SCPI Parser User Manual for details																	*/
/**************************************************************************************/
/*																										Default		Alternative						*/
/*					Name									Sequence						Item #		Parameter							*/
/*					----									--------						-------		-----------						*/
CHDAT_TYPE  sMinMaxNoUnits 		=	{ SeqMinMax,      		NO_DEF,   P_NUM, (void *)&sNoUnits}; 		/* MIN|MAX|<value>        */
CHDAT_TYPE  sMinMaxVolts 			=	{ SeqMinMax,      		NO_DEF,   P_NUM, (void *)&sVolts};  		/* MIN|MAX|<volts>        */
CHDAT_TYPE	sMinMaxDefVolts 	=	{ SeqMinMaxDef,				NO_DEF,		P_NUM, (void *)&sVolts};			/* MIN|MAX|DEF|<volts>		*/
CHDAT_TYPE  sMinMaxAmps 			=	{ SeqMinMax,      		NO_DEF,   P_NUM, (void *)&sAmps};  			/* MIN|MAX|<amps>        	*/
CHDAT_TYPE	sMinMaxDefAmps 		=	{ SeqMinMaxDef,				NO_DEF,		P_NUM, (void *)&sAmps};				/* MIN|MAX|DEF|<amps>			*/
CHDAT_TYPE  sMinMaxOhms 			=	{ SeqMinMax,      		NO_DEF,   P_NUM, (void *)&sOhms};  			/* MIN|MAX|<ohms>        	*/
CHDAT_TYPE	sMinMaxDefOhms 		=	{ SeqMinMaxDef,				NO_DEF,		P_NUM, (void *)&sOhms};				/* MIN|MAX|DEF|<ohms>			*/
CHDAT_TYPE  sMinMaxHertz 			=	{ SeqMinMax,      		NO_DEF,   P_NUM, (void *)&sHertz};  		/* MIN|MAX|<hertz>        */
CHDAT_TYPE	sMinMaxDefHertz 	=	{ SeqMinMaxDef,				NO_DEF,		P_NUM, (void *)&sHertz};			/* MIN|MAX|DEF|<hertz>		*/
CHDAT_TYPE  sMinMaxSecs 			=	{ SeqMinMax,      		NO_DEF,   P_NUM, (void *)&sSecs};  			/* MIN|MAX|<seconds>     	*/
CHDAT_TYPE	sMinMaxDefSecs 		=	{ SeqMinMaxDef,				NO_DEF,		P_NUM, (void *)&sSecs};				/* MIN|MAX|DEF|<seconds>	*/
CHDAT_TYPE	sBusImmExt				= { SeqBusImmExt,				NO_DEF,		ALT_NONE };										/* BUS|IMMediate|EXTernal	*/
CHDAT_TYPE	sACDCGnd					=	{ SeqACDCGnd,					NO_DEF,		ALT_NONE								};		/* AC|DC|GND							*/
CHDAT_TYPE	sACDC							=	{ SeqACDC,						NO_DEF,		ALT_NONE								};		/* AC|DC									*/
CHDAT_TYPE	sXTIMe						= { SeqXTIMe,						NO_DEF,		ALT_NONE								};		/* "XTIMe:VOLTage#[:DC]"  */
CHDAT_TYPE	sASCii						=	{ SeqAscii,						NO_DEF,		ALT_NONE								};		/* ASCii									*/
CHDAT_TYPE	sPosNegEit				= { SeqPosNegEit,				NO_DEF,		ALT_NONE								};		/* POSitive|NEGative|EITher */
CHDAT_TYPE	sInternal					=	{ SeqInternal,				NO_DEF,		ALT_NONE								};		/* INTernal#							*/
CHDAT_TYPE	sTTMode					  =	{ SeqTTMode,				  NO_DEF,		ALT_NONE								};		// NRM|AIR|TWO
CHDAT_TYPE	sCont			        =	{ SeqContinous,				NO_DEF,		ALT_NONE								};		// CONT|NONCONT

#ifdef SUPPORT_NUM_LIST
/**************************************************************************************/
/* Numeric List Types																																	*/
/* ------------------																																	*/
/* USER: Create a structure for each type of Numeric List supported										*/
/* Notes:																																							*/
/*		a) See JPA-SCPI Parser User Manual for details																	*/
/**************************************************************************************/
/*																Allow		Allow		Range			 Allowed Values						*/
/*						Name								Reals?	Neg?		Check?		Minimum		Maximum					*/
/*						----								------	----		-------		-------		-------					*/
NUMLIST_TYPE  sNLAnyNumber 		=	{ TRUE,		TRUE,		FALSE,	  0,				0				};  /* All numbers allowed		*/
NUMLIST_TYPE  sNLInts  				=	{ FALSE,	TRUE,		FALSE,	  0,				0				};  /* Only integers 					*/
NUMLIST_TYPE  sNLPosInts 			=	{ FALSE,	FALSE,	FALSE,	  0,				0				};  /* Only positive integers	*/
NUMLIST_TYPE	sNL8BitPosInts	= {	FALSE,	FALSE,	TRUE,			0,				255			};	/* 8-bit integers (0-255)	*/
NUMLIST_TYPE	sNLAuxDevInts   = { FALSE,	FALSE,	FALSE, 		1,				4			  };	/* list of aux devs	*/
#endif


#ifdef SUPPORT_CHAN_LIST
/**************************************************************************************/
/* Channel List Types																																	*/
/* ------------------																																	*/
/* USER: Create a structure for each type of Channel List supported										*/
/* Notes:																																							*/
/*		a) See JPA-SCPI Parser User Manual for details																	*/
/**************************************************************************************/
/*																	Allow		Allow	 	Range	 Dimensions	Allowed Vals		*/
/*							Name								Reals?	Neg?	 	Check?	Min	Max		Min		Max				*/
/*							----								------	----	 	------	---	---		----	----			*/
CHANLIST_TYPE  	sCL1Dim	 				=	{ TRUE,		TRUE,		FALSE,	1,	1,		0,		0			};  /* 1 dimension, all numbers allowed 		*/
CHANLIST_TYPE  	sCL2Dim	 				=	{ TRUE,		TRUE,		FALSE,	2,	2,		0,		0			};  /* 2 dimensions, all numbers allowed		*/
CHANLIST_TYPE  	sCL1DimInts  		=	{ FALSE,	TRUE,		FALSE,	1,	1,		0,		0			};  /* 1 dimension, only integers 					*/
CHANLIST_TYPE  	sCL2DimPosInts	=	{ FALSE,	FALSE,	FALSE,	2,	2,		0,		0			};  /* 2 dimensions, only positive integers	*/
CHANLIST_TYPE  	sCL4DimPosInts	=	{ FALSE,	FALSE,	FALSE,	1,	4,		1,		4			};  /* 4 dimensions, only positive integers	*/
#endif


/*
  *************************************************************************************
   COMMAND SPECS - Part 1: Command Keywords
   ----------------------------------------
   USER: Create an entry for each sequence of Command Keywords supported.
   Notes:
  		a) Include full command tree in all entries
  		a) Enclose optional keywords in square brackets, including any optional colon
  		b) Enter required characters in Uppercase, optional characters in Lowercase
  		c) DO NOT include spaces
  		d) Duplicate entries are allowed if required in the Command Specs (see Manual)
  *************************************************************************************
*/
const char *SSpecCmdKeywords[] =
{
// Command Number
// --------------
// Commands required by all SCPI-Compliant Instruments

// Required IEEE488.2 Common Commands (see SCPI Standard V1999.0 ch4.1.1)
	"*CLS",											//		 0
	"*ESE",											//		 1
	"*ESE?",										//		 2
	"*ESR?",										//		 3
	"*IDN?",										//		 4
	"*OPC",											//		 5
	"*OPC?",										//		 6
	"*RST",											//		 7
	"*SRE",											//		 8
	"*SRE?",										//		 9
	"*STB?",										//		10
	"*TST?",										//		11
	"*WAI",											//		12

//////////////////// Required SCPI commands (see SCPI Standard V1999.0 ch 4.2.1)
	"SYSTem:ERRor[:NEXT]?",										//		13
	"SYSTem:VERSion?",												//		14
	"STATus:OPERation[:EVENt]?",							//		15
	"STATus:OPERation:CONDition?",						//		16
	"STATus:OPERation:ENABle",								//		17
	"STATus:OPERation:ENABle?",								//		18
	"STATus:QUEStionable[:EVENt]?",						//		19
	"STATus:QUEStionable:CONDition?",					//		20
	"STATus:QUEStionable:ENABle",							//		21
	"STATus:QUEStionable:ENABle?",						//		22
	"STATus:PRESet",													//		23

///////////////////////////////////////////////////////////////////////////////////
// Commands required by all 2090 Positioner Class Compliant Instruments
///////////////////////////////////////////////////////////////////////////////////

	"AIR",			              // 24: Activates or deactivates flotation on air flotation turntables. AIR <ON/OFF>
	"AUXX",                   // 25: Activates or deactivates one or more auxiliary devices. AUX <Device #> [Device #] [Device #] [Device #] <ON/OFF>
  "AUX#",                   // 26: Activates or deactivates specified auxiliary device. AUX# <ON/OFF>
  "AUX#?",                  // 27: Queries the state of the specified auxiliary device control
 	"CAL",		                // 28: Changes the encoder calibration setting of the device. CAL <integer value>
 	"CAL?",		                // 29: Query the encoder calibration setting of the device. Response: <value> Value between 1 and 9999.
  "CC",                     // 30: Instructs the turntable to move in the counterclockwise direction
  "CL",                     // 31: Changes the counterclockwise limit of the device.
  "CL?",                    // 32: Queries the turntable's counterclockwise limit.
  "CP",                     // 33: Changes the current position of the device. Value must be between -999.9 and 999.9.
  "CP?",                    // 34: Queries the current position.
  "CW",                     // 35: Instructs the turntable to move in the clockwise direction.
  "CY",                     // 36: Changes the cycle count for the device.
  "CY?",                    // 37: Queries the cycle count for the device.
  "DIR?",                   // 38: Queries the motion direction for the device.
  "DN",                     // 39: Instructs the tower boom to move in the Down direction.
  "ERE",                    // 40: Set Device Dependent Error Enable Register.
  "ERE?",                   // 41: Query Device Dependent Error Enable Register
  "ERR?",                   // 42: Query Device Dependent Error Register.
  "LH",                     // 43: Changes the lower limit of the device for horizontal polarity.
  "LH?",                    // 44: Queries the lower limit of the device for horizontal polarity.
  "LL",                     // 45: Changes the lower limit of the device. This command effects both the horizontal and vertical limits, simultaneously.
  "LL?",                    // 46: Query the lower limit of the device. LL? returns the limit associated with the current polarization mode.
  "LV",                     // 47: Changes the lower limit of the device for vertical polarity.
  "LV?",                    // 48: Queries the lower limit of the device for vertical polarity.
  "N1",                     // 49: Places the controller in the backward compatible numeric mode 1.
  "N2",                     // 50: Places the controller in the backward compatible numeric mode 2.
  "OFF",                    // 51: Sets the polarization offset of non-bore sight towers.
  "OFF?",                   // 52: Queries the polarization offset for standard (non-bore sight) towers.
  "PH",                     // 53: Queries the polarization offset for standard (non-bore sight) towers.
  "PV",                     // 54: Queries the polarization offset for standard (non-bore sight) towers.
  "P?",                     // 55: Queries the polarization offset for standard (non-bore sight) towers.
  "RTL",                    // 56: Causes the device to return to local mode.
  "S#",                     // 57: Changes the speed selection of a two speed or variable speed device.
  "S?",                     // 58: Queries the speed selection of a two speed or variable speed device.
  "SC",                     // 59: Instructs the device to begin scanning between preset lower and upper limits.
  "SC?",                    // 60: Queries the 2090 to determine if scan mode is active.
  "SEP",                    // 61: Changes the separation distance between the mast and the EUT.
  "SEP?",                   // 62: Query the separation distance between the mast and the EUT
  "SK",                     // 63: Instructs the device to begin seeking for a preset target value.
  "SKN",                    // 64: Instructs the device to begin seeking the specified target value in the negative (down/counterclockwise) direction only.
  "SKP",                    // 65: Instructs the device to begin seeking the specified target value in the position (up/clockwise) direction only.
  "SKR",                    // 66: Instructs the device to begin seeking the specified target value relative to the current position
  "SS#",                    // 67: Sets a preset speed setting for a variable speed device
  "SS#?",                   // 68: Queries a preset speed setting for a variable speed device
  "ST",                     // 69: Causes device motion to stop
  "TG",                     // 70: Changes the seek target position of the device.
  "TG?",                    // 71: Query the seek target for the device.
  "TT",                     // 72: Selects turntable mode of operation. Allows the selection of normal, two speed, and air flotation turntables.
  "TWR",                    // 73: Selects tower mode of operation
  "TYP?",                   // 74: Queries the current device type configuration
  "UH",                     // 75: Changes the upper limit of the device for horizontal polarity
  "UH?",                    // 76: Queries the upper limit of the device for horizontal polarity.
  "UL",                     // 77: Queries the tower's upper limit
  "UL?",                    // 78: Query the upper limit of the device.
  "UP",                     // 79: Instructs the tower boom to move in the Up direction
  "UV",                     // 80: Changes the upper limit of the device for vertical polarity
  "UV?",                    // 81: Queries the upper limit of the device for vertical polarity.
  "VS?",                    // 82: Queries the variable speed capability of the device
  "WL",                     // 83: Changes the clockwise limit of the device
  "WL?",                    // 84: Queries the turntable's clockwise limit.
  "ZERO",                   // 85: Initiates a zero reference scan for devices equipped with absolute zero reference pulses.
  "ZERO?",                  // 86: Queries if the device is equipped with an absolute zero reference to determine if the ZERO command can be used
  "CR?",                    // 87: Queries Continous Rotation Capablity
  "HOME?",                  // 88: Queries if Homed
  "A",                      // 89: Set Accelration
  "A?",                     // 90: Queries Accelration setting
  "AVLCMD",					// 91: Pass thru command, sends command direclty to AVL MMI

///////////////////////////////////////////////////////////////////////////////////
// End of Commands required by all 2090 Positioner Class Compliant Instruments
///////////////////////////////////////////////////////////////////////////////////


	"SYSTem:CAPability?",											//		

  END_OF_COMMANDS														// USER: Do not modify this line
};


/**************************************************************************************/
/* Definitions for use in the Command Spec Table																			*/
/*																																										*/
/* USER: DO NOT MODIFY THESE LINES OF CODE																						*/
/*																																										*/
/* Optional / Required / No Parameter																									*/
#define OPT					1,												/* Optional parameter										*/
#define REQ					0,												/* Required parameter										*/
#define NOP					REQ	 P_NONE, (void *)0		/* No paramater													*/
/*																																										*/
/* Parameter Types																																		*/
#define CH_DAT			P_CH_DAT,		(void *)&			/* Character Data												*/
#define BOOLEAN			P_BOOL,			(void *)&			/* Boolean															*/
#define NUM					P_NUM,			(void *)&			/* Numerical Value											*/
#define STRING			P_STR,			(void *)0			/* String (quoted)											*/
#define UNQ_STR			P_UNQ_STR,	(void *)0			/* Unquoted String											*/
#ifdef SUPPORT_EXPR
#define EXPR				P_EXPR,			(void *)0			/* Expression														*/
#endif
#ifdef SUPPORT_NUM_LIST
#define NUM_L				P_NUM_LIST,	(void *)&			/* Numeric List													*/
#endif
#ifdef SUPPORT_CHAN_LIST
#define CH_L				P_CHAN_LIST,	(void *)&		/* Channel List													*/
#endif
/**************************************************************************************/


/**************************************************************************************/
/* More definitions for use in the Command Spec Table																	*/
/*																																										*/
/* USER: Modify as instructed																													*/
/*																																										*/
/* Command Without Parameters																													*/
/* USER: Modify to match your value of MAX_PARAMS, e.g. for 3, use {NOP},{NOP},{NOP}	*/
//#define NO_PARAMS		{NOP},{NOP}
#define NO_PARAMS		{NOP},{NOP},{NOP}


/*																																										*/
/* USER: DO NOT MODIFY THIS LINE:																											*/
#define END_OF_COMMAND_SPECS	{{ NO_PARAMS }}
/**************************************************************************************/


/**************************************************************************************/
/* Command Specs - Part 2: Parameters																									*/
/* ----------------------------------																									*/
/* USER: Include all the Command Specs supported																			*/
/* Notes:																																							*/
/*		a) Each line in this table corresponds to the line in the Command Spec Command  */
/*			 Keyword table with the same index. There must be the same number of entries	*/
/*			 in both tables.																															*/
/**************************************************************************************/
const struct strSpecCommand sSpecCommand[] =
{
//																																					C o m m a n d
//		Param 1														Param 2															Number	Syntax
//		=======														=======															======	======
//		Opt/Req Type		Attributes				Opt/Req Type		Attributes
//		------- ----		----------				------- ----		----------
//
// Commands required by all SCPI-Compliant Instruments
//
// Required IEEE488.2 Common Commands (see SCPI Standard V1999.0 ch4.1.1)
	{{ NO_PARAMS                                                          }},	//	 0	*CLS
	{{ { REQ	NUM	sNoUnits }, {NOP}, {NOP}                                }},	//	 1	*ESE <value>
	{{	NO_PARAMS																													}},	//	 2	*ESE?
	{{	NO_PARAMS																													}},	//	 3	*ESR?
	{{	NO_PARAMS																													}},	//	 4	*IDN?
	{{	NO_PARAMS																													}},	//	 5	*OPC
	{{	NO_PARAMS																													}},	//	 6	*OPC?
	{{	NO_PARAMS																													}},	//	 7	*RST
	{{ { REQ	NUM	sNoUnits }, {NOP}, {NOP}                                }},	//	 8	*SRE <value>
	{{	NO_PARAMS																													}},	//	 9	*SRE?
	{{	NO_PARAMS																													}},	//	10	*STB?
	{{	NO_PARAMS																													}},	//	11	*TST?
	{{	NO_PARAMS																													}},	//	12	*WAI

// Required SCPI commands (see SCPI Standard V1999.0 ch 4.2.1)
//			SYSTem
	{{	NO_PARAMS																													}},	//	13:ERRor[:NEXT]?
	{{	NO_PARAMS																													}},	//	14:VERSion?


	{{	NO_PARAMS																													}},	//	15:OPERation[:EVENt]?
	{{	NO_PARAMS																													}},	//	16:OPERation:CONDition?
	{{ { REQ	NUM	sNoUnits }, {NOP}, {NOP}	                              }},	//	17:OPERation:ENABle <value>
	{{	NO_PARAMS																													}},	//	18:OPERation:ENABle?
	{{	NO_PARAMS																													}},	//	19:QUEStionable[:EVENt]?
	{{	NO_PARAMS																													}},	//	20:QUEStionable:CONDition?
	{{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}                    	          }},	//	21:QUEStionable:ENABle <value>
	{{	NO_PARAMS																													}},	//	22:QUEStionable:ENABle?
	{{	NO_PARAMS																													}},	//	23:PRESet

// ==================================================================================
////////////////////////////////////////////////////////////////////////////////
// Commands required by a 2090 SCPI Instrument Class Compliant
////////////////////////////////////////////////////////////////////////////////
	{{	{REQ BOOLEAN	sBNoDef }, {NOP}, {NOP} 	                          }},	//	24: AIR <ON|OFF>
	{{	NO_PARAMS																													}},	//	25: AUXX
	{{  {REQ BOOLEAN	sBNoDef }, {NOP}, {NOP}	                            }},	//  26: AUX# <ON|OFF>
  {{  NO_PARAMS	                                                        }},	//  27: AUX#?
	{{	{ REQ	NUM	sNoUnits },    {NOP}, {NOP} 														}},	//	28: CAL <integer value>
  {{	NO_PARAMS																												  }},	//	29: CAL?
	{{	NO_PARAMS																													}},	//	30: CC
  {{	{ REQ	NUM	sNoUnits },    {NOP}, {NOP}															}},	//  31: CL [+/-] XXX[.X]
  {{	NO_PARAMS																													}},	//  32: CL?
  {{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP} 																}},	//  33: CP [+/-] XXX[.X]
	{{	NO_PARAMS	                                                        }},	//	34: CP?
	{{	NO_PARAMS																											    }},	//	35: CW
  {{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}																}},	//  36: CY XXX.X
  {{	NO_PARAMS																													}},	//  37: CY?
  {{	NO_PARAMS																													}},	//  38: DIR?
  {{	NO_PARAMS																													}},	//  39: DN
	{{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}				 												}},	//	40: ERE XXXX
  {{	NO_PARAMS																													}},	//  41: ERE?
  {{	NO_PARAMS																													}},	//  42: ERR?
  {{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}																}},	//  43: LH [+/-] XXX[.X]
  {{	NO_PARAMS																													}},	//  44: LH?
  {{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}																}},	//  45: LL [+/-] XXX[.X]
  {{	NO_PARAMS																													}},	//  46: LL?
  {{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}																}},	//  47: LV [+/-] XXX[.X]
  {{	NO_PARAMS																													}},	//  48: LV?
  {{	NO_PARAMS																													}},	//  49: N1
	{{	NO_PARAMS																													}},	//	50: N2
  {{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}																}},	//  51: OFF [+/-] XX[.X]
  {{	NO_PARAMS																													}},	//  52: OFF?
  {{	NO_PARAMS																													}},	//  53: PH
  {{	NO_PARAMS																													}},	//  54: PV
  {{	NO_PARAMS																													}},	//  55: P?
  {{	NO_PARAMS																													}},	//  56: RTL
  {{	NO_PARAMS																													}},	//  57: S#
  {{	NO_PARAMS																													}},	//  58: S?
  {{	NO_PARAMS																													}},	//  59: SC
	{{	NO_PARAMS																													}},	//	60: SC?
  {{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}																}},	//  61: SEP XX
  {{	NO_PARAMS																													}},	//  62: SEP?
  {{	{ OPT	NUM	sNoUnits }, {NOP}, {NOP}																													}},	//  63: SK
  {{	NO_PARAMS																													}},	//  64: SKN
  {{	NO_PARAMS																													}},	//  65: SKP
  {{	NO_PARAMS																													}},	//  66: SKR
  {{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}																}},	//  67: SS# <speed>
  {{	NO_PARAMS																													}},	//  68: SS#?
  {{	NO_PARAMS																													}},	//  69: ST
	{{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}																}},	//	70: TG [+/-] XXX[.X]
  {{	NO_PARAMS																													}},	//  71: TG?
  {{	{ REQ	CH_DAT sTTMode}, { REQ	CH_DAT	sCont }, {NOP} 						  }},	//  72: TT <NRM/AIR/TWO>, <CONT/NONCONT>  TODO: make syntax 2090 compatible
  {{	{ REQ	CH_DAT sTTMode}																							}},	//  73: TWR <NRM/BOR>
  {{	NO_PARAMS																													}},	//  74: TYP?
  {{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}			 													}},	//  75: UH [+/-] XXX[.X]
  {{	NO_PARAMS																													}},	//  76: UH?
  {{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}																}},	//  77: UL [-]XXX[.X]
  {{	NO_PARAMS																													}},	//  78: UL?
  {{	NO_PARAMS																													}},	//  79: UP
	{{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}																}},	//	80: UV  [+/-] XXX[.X]
  {{	NO_PARAMS																													}},	//  81: UV?
  {{	NO_PARAMS																													}},	//  82: VS?
  {{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}																}},	//  83: WL [+/-] XXX[.X]
  {{	NO_PARAMS																													}},	//  84: WL?
  {{	NO_PARAMS																													}},	//  85: ZERO
  {{	NO_PARAMS																													}},	//  86: ZERO?
  {{	NO_PARAMS																													}},	//  87: CR?
  {{	NO_PARAMS																													}},	//  88: HOME?
  {{	{ REQ	NUM	sNoUnits }, {NOP}, {NOP}																				}},	//  89: A
  {{	NO_PARAMS																													}},	//  90: A?
  { {	{ REQ	STRING }, {NOP}, {NOP}																								}},	//  91: AVLCMD

 //==================================================================================
  {{	NO_PARAMS																													}},	//  xx: SYSTem:CAPability?


	END_OF_COMMAND_SPECS	 // USER: Do not modify this line
};
