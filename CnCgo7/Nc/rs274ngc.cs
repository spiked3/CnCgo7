using System;
using System.Collections;
using System.Text;
using System.ComponentModel;

namespace CnCgo7
{
	public partial class EmcInterpreter 
	{

		#region canon_hh

		/*
		This is the header file that all trajectory applications that use the
		canonical commands for three-axis machining should include.

		It is assumed in these activities that the spindle tip is always at
		some location called the "current location," and the controller always
		knows where that is. It is also assumed that there is always a
		"selected plane" which must be the XY-plane, the YZ-plane, or the
		ZX-plane of the  
		*/

		public enum CANON_PLANE { XY, YZ, XZ }
		public enum CANON_UNITS { INCHES, MM, CM }
		public enum CANON_MOTION_MODE { EXACT_STOP, EXACT_PATH, CONTINUOUS }
		public enum CANON_SPEED_FEED_MODE { SYNCHED, INDEPENDENT }
		public enum CANON_DIRECTION { STOPPED, CLOCKWISE, COUNTERCLOCKWISE }
		public enum CANON_FEED_REFERENCE { WORKPIECE, XYZ }
		public enum CANON_AXIS { X, Y, Z, A, B, C }
		public enum CANON_CUTTER_COMP { RIGHT, LEFT, OFF }

		public class CANON_VECTOR
		{
			public double x, y, z;
			public CANON_VECTOR() {}
			public CANON_VECTOR(double _x, double _y, double _z)
			{
				x = _x; y = _y; z = _z;
			}
		}

		public class CANON_POSITION
		{
			public double x, y, z, a, b, c;
			public CANON_POSITION() {}
			public CANON_POSITION(double _x, double _y, double _z,
				double _a, double _b, double _c)
			{
				x = _x; y = _y; z = _z;
				a = _a; b = _b; c = _c;
			}
			public CANON_POSITION(double _x, double _y, double _z)
			{
				x = _x; y = _y; z = _z;
				a = 0.0; b = 0.0; c = 0.0;
			}
		}

		// Tools are numbered 1..CANON_TOOL_MAX, with tool 0 meaning no tool.
		public const int CANON_TOOL_MAX = 32;       // number of tools handled
		//#define CANON_TOOL_ENTRY_LEN 256 // how long each file line can be

		public class CANON_TOOL_TABLE
		{
			public int id;
			public double length;
			public double diameter;
		};

		#endregion

		#region rs274ngc_hh

		/* Declarations for the rs274ngc translator. */

		/* numerical constants */
		const double TOLERANCE_INCH = 0.0002;
		const double TOLERANCE_MM = 0.002;
		/* angle threshold for concavity for cutter compensation, in radians */
		const double TOLERANCE_CONCAVE_CORNER = 0.01; 
		const double TINY = 1e-12; /* for arc_data_r */
		const double UNKNOWN = 1e-20;
		const double TWO_PI = Math.PI * 2.0;
		const double PI = Math.PI;
		const double PI2 = Math.PI / 2.0;
		/* English - Metric conversion (long number keeps error buildup down) */
		const double MM_PER_INCH = 25.4;
		const double INCH_PER_MM = 0.039370078740157477;

		const int MAX_GEES = 4;
		const int MAX_EMS = 4;
		const int MAX_ACTIVES = 100;

		/* on-off switch settings */
		public const int OFF = 0;
		public const int ON = 1;

		/* feed_mode */
		const int UNITS_PER_MINUTE = 0;
		const int INVERSE_TIME = 1;

		/* error and exit status */
		// FMP added prefix since MS VC++ conflicts
		const int RS274NGC_OK = 0;
		const int RS274NGC_ERROR = -1;
		const int RS274NGC_EXIT = 1;
		const int RS274NGC_ENDFILE = 2;
		const int RS274NGC_EXECUTE_FINISH = 3;

		/* unary operations */
		/* These are not enums because the "&" operator is used in
		reading the operation names and is illegal with an enum */

		const int ABS = 1;
		const int ACOS = 2;
		const int ASIN = 3;
		const int ATAN = 4;
		const int COS = 5;
		const int EXP = 6;
		const int FIX = 7;
		const int FUP = 8;
		const int LN = 9;
		const int ROUND = 10;
		const int SIN = 11;
		const int SQRT = 12;
		const int TAN = 13;


		/* binary operations */
		const int NO_OPERATION = 0;
		const int DIVIDED_BY = 1;
		const int MODULO = 2;
		const int POWER = 3;
		const int TIMES = 4;
		const int LOGICAL_AND = 5;
		const int EXCLUSIVE_OR = 6;
		const int MINUS = 7;
		const int NON_EXCLUSIVE_OR = 8;
		const int PLUS = 9;
		const int RIGHT_BRACKET = 10;

		/* G Codes are symbolic to be dialect-independent in source code */
		const int G_0      = 0;
		const int G_1     = 10;
		const int G_2     = 20;
		const int G_3     = 30;
		const int G_4     = 40;
		const int G_10   = 100;
		const int G_17   = 170;
		const int G_18   = 180;
		const int G_19   = 190;
		const int G_20   = 200;
		const int G_21   = 210;
		const int G_38_2 = 382;
		const int G_40   = 400;
		const int G_41   = 410;
		const int G_42   = 420;
		const int G_43   = 430;
		const int G_49   = 490;
		const int G_53   = 530;
		const int G_54   = 540;
		const int G_55   = 550;
		const int G_56   = 560;
		const int G_57   = 570;
		const int G_58   = 580;
		const int G_59   = 590;
		const int G_59_1 = 591;
		const int G_59_2 = 592;
		const int G_59_3 = 593;
		const int G_61   = 610;
		const int G_64   = 640;
		const int G_80   = 800;
		const int G_81   = 810;
		const int G_82   = 820;
		const int G_83   = 830;
		const int G_84   = 840;
		const int G_85   = 850;
		const int G_86   = 860;
		const int G_87   = 870;
		const int G_88   = 880;
		const int G_89   = 890;
		const int G_90   = 900;
		const int G_91   = 910;
		const int G_92   = 920;
		const int G_92_2 = 922;
		const int G_93   = 930;
		const int G_94   = 940;
		const int G_98   = 980;
		const int G_99   = 990;

		/*

		To be sure errors are handled consistently and to shorten the source
		code, macros are used for dealing with errors. The error macros
		print the name of the function returning the error and a message about
		the error (if one is needed) with a newline. Then ERROR is returned.
		The weird if-else syntax is used so that is appropriate to use a
		semicolon at the end of the line of source code on which the error
		macro is used.

		If error handling is changed somewhat, only the macros will need
		changing.

		There are two types of error handling macro, one for handling errors
		in the input code (named ERROR_MACRO) and one (named BUG_MACRO) for
		handling situations that should never occur regardless of the input
		code, and will only occur if there is a bug in the program.

		*/


		//	#define ERROR_MACRO(text, function_name, message) if (1) {     \
		//		fprintf(stderr, "%s\n%s: error %d:\n%s\n",                   \
		//		text,                                            \
		//		function_name,                                   \
		//		utility_error_number(message, interp_errors, 0), \
		//		message);                                        \
		//		return RS274NGC_ERROR;                                       \
		//	} 
		//	else


		//		#define ERR_MACRO(text, function_name, message) if (1) {       \
		//		if (_interpreter_fp != null)                               \
		//			{fclose(_interpreter_fp); _interpreter_fp = null;}    \
		//		fprintf(stderr, "%s\n%s: error %d:\n%s\n",                   \
		//					text,                                            \
		//					function_name,                                   \
		//					utility_error_number(message, interp_errors, 0), \
		//					message);                                        \
		//		return RS274NGC_ERROR;                                       \
		//		} else

		// #define return RS274NGC_ERROR;

		//		#define BUG_MACRO(function_name, message) if (1) {           \
		//		fprintf(stderr, "%s: bug %d:\n%s\n", function_name,        \
		//				utility_error_number(message, interp_errors, 200), \
		//				message);                                          \
		//		return RS274NGC_ERROR;                                     \
		//		} else

		/**********************/
		/*      TYPEDEFS      */
		/**********************/

		/* distance_mode */
		public enum DISTANCE_MODE { ABSOLUTE, INCREMENTAL };

		/* retract_mode for cycles */
		public enum RETRACT_MODE { R_PLANE, OLD_Z };

		/*

		The current_x, current_y, and current_z are the location of the tool
		in the current coordinate system. current_x and current_y differ from
		program_x and program_y when cutter radius compensation is on.
		current_z is the position of the tool tip in program coordinates when
		tool length compensation is on; it is the position of the spindle when
		tool length compensation is off.

		*/

		public const int RS274NGC_MAX_PARAMETERS = 5400;

        [TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public setup Setup { get { return _interpreter_settings; } }
        [TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public block Block { get { return _interpreter_block; } }

		public class setup 
		{
			public double axis_offset_x {get; set;}
            public double axis_offset_y { get; set; }
            public double axis_offset_z { get; set; }
            public bool block_delete { get; set; }
            public CANON_MOTION_MODE control_mode { get; set; }
            public int current_slot { get; set; }
			public double current_x {get; set;}
			public double current_y {get; set;}
			public double current_z {get; set;}
			public CANON_CUTTER_COMP cutter_radius_compensation {get; set;}
			public double cycle_i {get; set;}
			public double cycle_j {get; set;}
			public double cycle_k {get; set;}
			public int cycle_l {get; set;}
			public double cycle_p {get; set;}
			public double cycle_q {get; set;}
			public double cycle_r {get; set;}
			public double cycle_z {get; set;}
			public DISTANCE_MODE distance_mode {get; set;}
			public int feed_mode {get; set;}
			public int feed_override {get; set;}
			public double feed_rate {get; set;}
			public int flood {get; set;}
			public int length_offset_index {get; set;} /* for use with tool length offsets */
			public CANON_UNITS length_units {get; set;}
			public int mist {get; set;}
			public int motion_mode {get; set;}
			public int origin_ngc {get; set;}
			public double origin_offset_x {get; set;}
			public double origin_offset_y {get; set;}
			public double origin_offset_z {get; set;}
			public double[] parameters = new double[RS274NGC_MAX_PARAMETERS];
			public CANON_PLANE plane {get; set;}
			public int probe_flag {get; set;}
			public double program_x {get; set;}  /* used when cutter radius compensation is on */
			public double program_y {get; set;}
			public RETRACT_MODE retract_mode {get; set;}
			public int selected_tool_slot {get; set;}
			public double speed {get; set;}
			public CANON_SPEED_FEED_MODE speed_feed_mode {get; set;}
			public int speed_override {get; set;}
			public CANON_DIRECTION spindle_turning {get; set;}
			public double tool_length_offset {get; set;}
            public CANON_TOOL_TABLE[] tool_table = new CANON_TOOL_TABLE[CANON_TOOL_MAX + 1];
			public int tool_table_index {get; set;} /* for use with cutter radius compensation */
			public double traverse_rate {get; set;}
		};

		public class block 
		{
			public string   comment {get; set;}
			public int      d_number {get; set;}
			public double   f_number {get; set;}
			public int      g_count {get; set;}
			public int[]	g_modes = new int[14];
			public int      h_number {get; set;}
			public int		i_flag {get; set;}
			public double   i_number {get; set;}
			public int		j_flag {get; set;}
			public double   j_number {get; set;}
			public int		k_flag {get; set;}
			public double   k_number {get; set;}
			public int      l_number {get; set;}
			public int      line_number {get; set;}
			public int      motion_to_be {get; set;}
			public int      m_count {get; set;}
            public int[] m_modes = new int[10];
			public double   p_number {get; set;}
			public double   q_number {get; set;}
			public int		r_flag {get; set;}
			public double   r_number {get; set;}
			public double   s_number {get; set;}
			public int      t_number {get; set;}
			public int		x_flag {get; set;}
			public double   x_number {get; set;}
			public int		y_flag {get; set;}
			public double   y_number {get; set;}
			public int		z_flag {get; set;}
			public double   z_number {get; set;}
		};


		// name of parameter file for saving/restoring interpreter variables
		const string DEFAULT_RS274NGC_PARAMETER_FILE = @"rs274ngc.var";
		const string  RS274NGC_PARAMETER_FILE_BACKUP_SUFFIX = @".bak";

		// copy active G codes into array [0]..[11]
		const int RS274NGC_ACTIVE_G_CODES = 12;

		// copy active M codes into array [0]..[6]
		const int RS274NGC_ACTIVE_M_CODES = 7;

		// copy active F, S settings into array [0]..[2]
		const int RS274NGC_ACTIVE_SETTINGS = 3;

		#endregion

		#region rs274ngc_cc
		/*
		rs274ngc.cc

		Interpreter for Next Generation Controller (NGC) dialect of RS-274.

		ABOUT THE rs274ngc.cc FILE:

		The rs274ngc.cc file contains the source code for the rs274ngc
		interpreter, excluding a main routine and initialization routines.
		The file has two sections: the kernel functions (which are not
		intended to be called by programs using the interpreter), and
		the interface functions (which are intended to be called).
		The names of the interface functions start with "rs274ngc".

		Error handling throughout the system is by returning a status value of
		RS274NGC_OK or RS274NGC_ERROR from each function where there is a
		possibility of error.  An appropriate error message will be printed
		for each error.  If an error occurs, processing is always stopped, and
		control is passed back up through the function call hierarchy to the
		highest level function called which is defined in this file. Error
		reporting is handled by the CANON_ERROR function defined externally.

		Since returned values are usually used as just described to handle the
		possibility of errors, an alternative method of passing calculated
		values is required. In general, if function A needs a value for
		variable V calculated by function B, this is handled by passing a
		pointer to V from A to B, and B calculates and sets V.

		There are a lot of functions named read_XXXX. All such functions read
		characters from a string using a counter. They all reset the counter
		to point at the character in the string following the last one used by
		the function. The counter is passed around from function to function
		by using pointers to it. The first character read by each of these
		functions is expected to be a member of some set of characters (often
		a specific character), and each function checks the first character.

		This version of the interpreter not saving input lines. A list
		of all lines will be needed in future versions to implement loops,
		and probably for other purposes.

		This version is does not use any noticeable additional memory as it
		runs. Only comments in the NC code are malloc'ed, and that memory is
		freed when the next line of code is read. There should not be any
		memory leaks.

		This version does not suppress superfluous commands, such as a command
		to start the spindle when the spindle is already turning, or a command
		to turn on flood coolant, when flood coolant is already on.  When the
		interpreter is being used for direct control of the machining center,
		suppressing superfluous commands might confuse the user and could be
		dangerous, but when it is used to translate from one file to another,
		suppression can produce more concise output. Future versions might
		include an option for suppressing superfluous commands.

		*/

		/****************************************************************************/

		/* the current interpreter settings */

		setup _interpreter_settings = new setup();

		/*

		/* Interpreter global arrays for g_codes and m_codes. The nth entry
		in each array is the modal group number corresponding to the nth
		code. Entries which are -1 represent illegal codes. Remember g_codes
		in this interpreter are multiplied by 10.

		The modal g groups and group numbers defined in [NCMS, pages 71 - 73]
		are used here, except the canned cycles (g80 - g89), which comprise
		modal g group 9 in [NCMS], are treated here as being in the same modal
		group (group 1) with the straight moves and arcs (g0, g1, g2,g3).
		The straight_probe move, g38.2, is in group 1; it is not defined in
		[NCMS].

		Some g_codes (g4, g10, g53, g92, and g92.2 here - many more in [NCMS])
		are non-modal. [NCMS] puts all these in the same group 0.  Logically,
		there are two subgroups, those which require coordinate values and
		those which do not. Here we are using separate groups for these two.
		In group 0 we have put the ones that take coordinate values (g10,
		g92), and g92.2, which cancels g92.  In group 4 (which is not
		otherwise being used) we have put the ones that do not use coordinate
		values (g4, g53). We are allowing only one g_code from the combined
		groups 0 and 4 on a line. Those in group 0 may not be on the same line
		as those in group 1 (except g80) because they would be competing for
		the axis values. Those in group 4 may be used on the same line as
		those in group 1.

		There is no evident reason why only one of our modal group 4 should be
		allowed on a line, and it is not clear what the intent of [NCMS] is
		regarding how many of these may be on the same line. We are following
		the rule given above, however.

		The groups are:
		group  0 = {g10,g92,g92.2} - setup, set axis offsets (non-modal)
		group  1 = {g0,g1,g2,g3,g38.2,g80,g81,g82,g83,g84,g85,g86,g87,g88,g89} - motion
		group  2 = {g17,g18,g19}   - plane selection
		group  3 = {g90,g91}       - distance mode
		group  4 = {g4, g53}       - dwell, motion in abs coords (non-modal)
		group  5 = {g93,g94}       - spindle speed mode
		group  6 = {g20,g21}       - units
		group  7 = {g40,g41,g42}   - cutter diameter compensation
		group  8 = {g43,g49}       - tool length offset
		group 10 = {g98,g99}       - return mode in canned cycles
		group 12 = {g54,g55,g56,g57,g58,g59,g59.1,g59.2,g59.3} - coordinate system
		group 13 = {g61,g64}       - motion mode (exact stop or cutting)

		*/

		static int[] gees = {
								/*   0 */   1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/*  20 */   1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/*  40 */   4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/*  60 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/*  80 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 100 */   0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 120 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 140 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 160 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 180 */   2,-1,-1,-1,-1,-1,-1,-1,-1,-1, 2,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 200 */   6,-1,-1,-1,-1,-1,-1,-1,-1,-1, 6,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 220 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 240 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 260 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 280 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 300 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 320 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 340 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 360 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 380 */  -1,-1, 1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 400 */   7,-1,-1,-1,-1,-1,-1,-1,-1,-1, 7,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 420 */   7,-1,-1,-1,-1,-1,-1,-1,-1,-1, 8,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 440 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 460 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 480 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 8,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 500 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 520 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 4,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 540 */  12,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 560 */  12,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 580 */  12,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,12,12,12,-1,-1,-1,-1,-1,-1,
								/* 600 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,13,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 620 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 640 */  13,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 660 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 680 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 700 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 720 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 740 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 760 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 780 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 800 */   1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 820 */   1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 840 */   1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 860 */   1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 880 */   1,-1,-1,-1,-1,-1,-1,-1,-1,-1, 1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 900 */   3,-1,-1,-1,-1,-1,-1,-1,-1,-1, 3,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 920 */   0,-1, 0,-1,-1,-1,-1,-1,-1,-1, 5,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 940 */   5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 960 */  -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,
								/* 980 */  10,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1};

		/*

		Modal groups and modal group numbers for M codes are described in [NCMS,
		page 7]. We have added M60 as an extension of the language
		so the language can be used to control the K&T 800  

		The groups are:
		group 4 = {m0,m1,m2,m30,m60} - stopping
		group 6 = {m6}               - tool change
		group 7 = {m3,m4,m5}         - spindle turning
		group 8 = {m7,m8,m9}         - coolant
		group 9 = {m48,m49}          - feed and speed override switch bypass

		*/

		static int[] ems = {
							   4,  4,  4,  7,  7,  7,  6,  8,  8,  8,
							   -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
							   -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
							   4, -1, -1, -1, -1, -1, -1, -1, -1, -1,
							   -1, -1, -1, -1, -1, -1, -1, -1,  9,  9,
							   -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
							   4, -1, -1, -1, -1, -1, -1, -1, -1, -1,
							   -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
							   -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
							   -1, -1, -1, -1, -1, -1, -1, -1, -1, -1};

		/*

		This is the list of error messages that may be generated. Messages
		numbered 0 through 199 result from errors in the input to the interpreter.
		Messages numbered 200 and above result from bugs in the interpreter and
		should never be received.

		*/

		//		string[] interp_errors[] = {


		Hashtable ErrorNumToTextMap = new Hashtable();
		Hashtable ErrorTextToNumMap = new Hashtable();

		public int ErrorNumberOf(string t)
		{
			if (ErrorTextToNumMap.Count == 0)
				ErrorTextToNumMapInit();
			System.Diagnostics.Trace.WriteLine(t);
			return (int)ErrorTextToNumMap[t];
		}

		public string ErrorTextOf(int e)
		{
			if (ErrorNumToTextMap.Count == 0)
				ErrorNumToTextMapInit();
			return (string)ErrorNumToTextMap["" + e];
		}

		public void ErrorTextToNumMapInit()
		{
			if (ErrorNumToTextMap.Count == 0)
				ErrorNumToTextMapInit();
			foreach (string k in ErrorNumToTextMap.Keys)
				ErrorTextToNumMap.Add(ErrorNumToTextMap[k], int.Parse(k));
		}

		public 	void ErrorNumToTextMapInit()
		{
			ErrorNumToTextMap.Add("" + 0, "Unspecified error"); /*  see note above */
			ErrorNumToTextMap.Add("" + 1, "All axes missing with G92"); /* check_g_codes */
			ErrorNumToTextMap.Add("" + 2, "Arc radius too small to reach end point"); /* arc_data_r */
			ErrorNumToTextMap.Add("" + 3, "Argument to acos out of range"); /* execute_unary */
			ErrorNumToTextMap.Add("" + 4, "Argument to asin out of range"); /* execute_unary */
			ErrorNumToTextMap.Add("" + 5, "Attempt to divide by zero"); /* execute_binary1 */
			ErrorNumToTextMap.Add("" + 6, "Bad character used"); /* read_one_item */
			ErrorNumToTextMap.Add("" + 7, "Bad format unsigned integer"); /* read_integer_unsigned */
			ErrorNumToTextMap.Add("" + 8, "Bad number format"); /* read_real_number */
			ErrorNumToTextMap.Add("" + 9, "Bad tool radius value with cutter radius comp"); /* convert_arc_comp1, convert_arc_comp2, convert_straight_comp1, convert_straight_comp2 */
			ErrorNumToTextMap.Add("" + 10, "Cannot change axis offsets with cutter radius comp"); /* convert_axis_offsets */
			ErrorNumToTextMap.Add("" + 11, "Cannot change units with cutter radius comp"); /* convert_length_units */
			ErrorNumToTextMap.Add("" + 12, "Cannot create backup file"); /* rs274ngc_save_parameters */
			ErrorNumToTextMap.Add("" + 13, "Cannot do G1 with zero feed rate"); /* convert_straight */
			ErrorNumToTextMap.Add("" + 14, "Cannot do zero repeats of cycle"); /* convert_cycle */
			ErrorNumToTextMap.Add("" + 15, "Cannot have concave corner with cutter radius comp"); /* convert_straight_comp2 */
			ErrorNumToTextMap.Add("" + 16, "Cannot make arc with zero feed rate"); /* convert_arc */
			ErrorNumToTextMap.Add("" + 17, "Cannot open backup file"); /* rs274ngc_save_parameters */
			ErrorNumToTextMap.Add("" + 18, "Cannot open file"); /* rs274ngc_restore_parameters */
			ErrorNumToTextMap.Add("" + 19, "Cannot open variable file"); /* rs274ngc_save_parameters */
			ErrorNumToTextMap.Add("" + 20, "Cannot probe in inverse time feed mode"); /* convert_motion */
			ErrorNumToTextMap.Add("" + 21, "Cannot probe with cutter radius compensation on"); /* convert_motion */
			ErrorNumToTextMap.Add("" + 22, "Cannot probe with zero feed rate"); /* convert_motion */
			ErrorNumToTextMap.Add("" + 23, "Cannot put an M code for stopping with G38.2"); /* check_items */
			ErrorNumToTextMap.Add("" + 24, "Cannot raise negative number to non-integer power"); /* execute_binary1 */
			ErrorNumToTextMap.Add("" + 25, "Cannot turn cutter radius comp on out of XY-plane"); /* convert_cutter_compensation_on */
			ErrorNumToTextMap.Add("" + 26, "Cannot turn cutter radius comp on when already on"); /* convert_cutter_compensation_on */
			ErrorNumToTextMap.Add("" + 27, "Cannot use G0 with cutter radius comp"); /* convert_straight */
			ErrorNumToTextMap.Add("" + 28, "Cannot use G53 in incremental distance mode"); /* check_g_codes */
			ErrorNumToTextMap.Add("" + 29, "Cannot use G53 with cutter radius comp"); /* convert_straight */
			ErrorNumToTextMap.Add("" + 30, "Cannot use XZ plane with cutter radius comp"); /* convert_set_plane */
			ErrorNumToTextMap.Add("" + 31, "Cannot use YZ plane with cutter radius comp"); /* convert_set_plane */
			ErrorNumToTextMap.Add("" + 32, "Cannot use a G code for motion with G10"); /* check_g_codes */
			ErrorNumToTextMap.Add("" + 33, "Cannot use a G motion with G92"); /* check_g_codes */
			ErrorNumToTextMap.Add("" + 34, "Cannot use axis commands with G4"); /* check_g_codes */
			ErrorNumToTextMap.Add("" + 35, "Cannot use axis commands with G80"); /* check_g_codes */
			ErrorNumToTextMap.Add("" + 36, "Cannot use inverse time feed with cutter radius comp"); /* convert_straight */
			ErrorNumToTextMap.Add("" + 37, "Cannot use two G codes from group 0"); /* check_g_codes */
			ErrorNumToTextMap.Add("" + 38, "Command too long"); /* close_and_downcase, rs274ngc_execute */
			ErrorNumToTextMap.Add("" + 39, "Concave corner with cutter radius comp"); /* convert_arc_comp2 */
			ErrorNumToTextMap.Add("" + 40, "Coordinate setting given with G80"); /* convert_motion */
			ErrorNumToTextMap.Add("" + 41, "Current point same as end point of arc"); /* arc_data_comp_r, arc_data_r */
			ErrorNumToTextMap.Add("" + 42, "Cutter gouging with cutter radius comp"); /* convert_straight_comp1 */
			ErrorNumToTextMap.Add("" + 43, "D word missing with cutter radius comp on"); /* convert_cutter_compensation_on */
			ErrorNumToTextMap.Add("" + 44, "D word on line with no cutter comp on (G41 or G42) command"); /* check_other_codes */
			ErrorNumToTextMap.Add("" + 45, "Dwell time missing with G4"); /* check_g_codes */
			ErrorNumToTextMap.Add("" + 46, "Equal sign missing in parameter setting"); /* read_parameter_setting */
			ErrorNumToTextMap.Add("" + 47, "F word missing with inverse time G1 move"); /* convert_straight */
			ErrorNumToTextMap.Add("" + 48, "F word missing with inverse time arc move"); /* convert_motion */
			ErrorNumToTextMap.Add("" + 49, "File ended with no stopping command given"); /* rs274ngc_read */
			ErrorNumToTextMap.Add("" + 50, "G code out of range"); /* read_g */
			ErrorNumToTextMap.Add("" + 51, "H word on line with no tool length comp (G43) command"); /* check_other_codes */
			ErrorNumToTextMap.Add("" + 52, "I word given for arc in YZ-plane"); /* convert_arc */
			ErrorNumToTextMap.Add("" + 53, "I word missing with G87"); /* convert_cycle */
			ErrorNumToTextMap.Add("" + 54, "I word on line with no G code (G2, G3, G87) that uses it"); /* check_other_codes */
			ErrorNumToTextMap.Add("" + 55, "J word given for arc in XZ-plane"); /* convert_arc */
			ErrorNumToTextMap.Add("" + 56, "J word missing with G87"); /* convert_cycle */
			ErrorNumToTextMap.Add("" + 57, "J word on line with no G code (G2, G3, G87) that uses it"); /* check_other_codes */
			ErrorNumToTextMap.Add("" + 58, "K word given for arc in XY-plane"); /* convert_arc */
			ErrorNumToTextMap.Add("" + 59, "K word missing with G87"); /* convert_cycle */
			ErrorNumToTextMap.Add("" + 60, "K word on line with no G code (G2, G3, G87) that uses it"); /* check_other_codes */
			ErrorNumToTextMap.Add("" + 61, "L word on line with no canned cycle or G10 to use it"); /* check_other_codes */
			ErrorNumToTextMap.Add("" + 62, "Left bracket missing after slash with atan operator"); /* read_atan */
			ErrorNumToTextMap.Add("" + 63, "Left bracket missing after unary operation name"); /* read_unary */
			ErrorNumToTextMap.Add("" + 64, "Line number greater than 99999"); /* read_line_number */
			ErrorNumToTextMap.Add("" + 65, "Line with G10 does not have L2"); /* check_g_codes */
			ErrorNumToTextMap.Add("" + 66, "M code greater than 99"); /* read_m */
			ErrorNumToTextMap.Add("" + 67, "Mixed radius-ijk format for arc"); /* convert_arc */
			ErrorNumToTextMap.Add("" + 68, "Multiple D words on one line"); /* read_d */
			ErrorNumToTextMap.Add("" + 69, "Multiple F words on one line"); /* read_f */
			ErrorNumToTextMap.Add("" + 70, "Multiple I words on one line"); /* read_i */
			ErrorNumToTextMap.Add("" + 71, "Multiple J words on one line"); /* read_j */
			ErrorNumToTextMap.Add("" + 72, "Multiple K words on one line"); /* read_k */
			ErrorNumToTextMap.Add("" + 73, "Multiple L words on one line"); /* read_l */
			ErrorNumToTextMap.Add("" + 74, "Multiple P words on one line"); /* read_p */
			ErrorNumToTextMap.Add("" + 75, "Multiple Q words on one line"); /* read_q */
			ErrorNumToTextMap.Add("" + 76, "Multiple R words on one line"); /* read_r */
			ErrorNumToTextMap.Add("" + 77, "Multiple S word spindle speed settings on one line"); /* read_spindle_speed */
			ErrorNumToTextMap.Add("" + 78, "Multiple T words (tool ids) on one line"); /* read_tool */
			ErrorNumToTextMap.Add("" + 79, "Multiple X words on one line"); /* read_x */
			ErrorNumToTextMap.Add("" + 80, "Multiple Y words on one line"); /* read_y */
			ErrorNumToTextMap.Add("" + 81, "Multiple Z words on one line"); /* read_z */
			ErrorNumToTextMap.Add("" + 82, "Multiple tool length offsets on one line"); /* read_tool_length_offset */
			ErrorNumToTextMap.Add("" + 83, "Must use G0 or G1 with G53"); /* check_g_codes */
			ErrorNumToTextMap.Add("" + 84, "Negative D code used"); /* read_d */
			ErrorNumToTextMap.Add("" + 85, "Negative F word found"); /* read_f */
			ErrorNumToTextMap.Add("" + 86, "Negative G code used"); /* read_g */
			ErrorNumToTextMap.Add("" + 87, "Negative L word used"); /* read_l */
			ErrorNumToTextMap.Add("" + 88, "Negative M code used"); /* read_m */
			ErrorNumToTextMap.Add("" + 89, "Negative P value used"); /* read_p */
			ErrorNumToTextMap.Add("" + 90, "Negative Q value used"); /* read_q */
			ErrorNumToTextMap.Add("" + 91, "Negative argument to sqrt"); /* execute_unary */
			ErrorNumToTextMap.Add("" + 92, "Negative spindle speed found"); /* read_spindle_speed */
			ErrorNumToTextMap.Add("" + 93, "Negative tool id used"); /* read_tool */
			ErrorNumToTextMap.Add("" + 94, "Negative tool length offset used"); /* read_tool_length_offset */
			ErrorNumToTextMap.Add("" + 95, "Nested comment found"); /* close_and_downcase */
			ErrorNumToTextMap.Add("" + 96, "No characters found in reading real value"); /* read_real_value */
			ErrorNumToTextMap.Add("" + 97, "No digits found where real number should be"); /* read_real_number */
			ErrorNumToTextMap.Add("" + 98, "Non-integer value for integer"); /* read_integer_value */
			ErrorNumToTextMap.Add("" + 99, "Null missing after newline"); /* close_and_downcase */
			ErrorNumToTextMap.Add("" + 100, "Offset index missing"); /* convert_tool_length_offset */
			ErrorNumToTextMap.Add("" + 101, "P value not an integer with G10 L2"); /* check_g_codes */
			ErrorNumToTextMap.Add("" + 102, "P value out of range with G10 L2"); /* check_g_codes */
			ErrorNumToTextMap.Add("" + 103, "P word (dwell time) missing with G82"); /* convert_cycle */
			ErrorNumToTextMap.Add("" + 104, "P word (dwell time) missing with G86"); /* convert_cycle */
			ErrorNumToTextMap.Add("" + 105, "P word (dwell time) missing with G88"); /* convert_cycle */
			ErrorNumToTextMap.Add("" + 106, "P word (dwell time) missing with G89"); /* convert_cycle */
			ErrorNumToTextMap.Add("" + 107, "P word on line with no G code (G4 G10 G82 G86 G88 G89) that uses it"); /* check_other_codes */
			ErrorNumToTextMap.Add("" + 108, "Parameter number out of range"); /* read_parameter, read_parameter_setting, rs274ngc_restore_parameters, rs274ngc_save_parameters */
			ErrorNumToTextMap.Add("" + 109, "Q word (depth increment) missing with G83"); /* convert_cycle */
			ErrorNumToTextMap.Add("" + 110, "Q word on line with no G83 cycle that uses it"); /* check_other_codes */
			ErrorNumToTextMap.Add("" + 111, "Queue is not empty after probing"); /* rs274ngc_execute, rs274ngc_read */
			ErrorNumToTextMap.Add("" + 112, "R clearance plane unspecified in canned cycle"); /* convert_cycle */
			ErrorNumToTextMap.Add("" + 113, "R value less than Z value in canned cycle"); /* convert_cycle */
			ErrorNumToTextMap.Add("" + 114, "R word on line with no G code (arc or cycle) that uses it"); /* check_other_codes */
			ErrorNumToTextMap.Add("" + 115, "R, I, J, and K words all missing for arc"); /* convert_arc */
			ErrorNumToTextMap.Add("" + 116, "Radius to end of arc differs from radius to start of arc"); /* arc_data_comp_ijk, arc_data_ijk */
			ErrorNumToTextMap.Add("" + 117, "Radius too small to reach end point"); /* arc_data_comp_r */
			ErrorNumToTextMap.Add("" + 118, "Selected tool slot number too large"); /* convert_tool_select */
			ErrorNumToTextMap.Add("" + 119, "Slash missing"); /* read_atan */
			ErrorNumToTextMap.Add("" + 120, "Spindle not turning clockwise in G84 canned cycle"); /* convert_cycle_g84 */
			ErrorNumToTextMap.Add("" + 121, "Spindle not turning in G86 canned cycle"); /* convert_cycle_g86 */
			ErrorNumToTextMap.Add("" + 122, "Spindle not turning in G87 canned cycle"); /* convert_cycle_g87 */
			ErrorNumToTextMap.Add("" + 123, "Spindle not turning in G88 canned cycle"); /* convert_cycle_g88 */
			ErrorNumToTextMap.Add("" + 124, "Too many G codes on line"); /* check_g_codes */
			ErrorNumToTextMap.Add("" + 125, "Too many M codes on line"); /* check_m_codes */
			ErrorNumToTextMap.Add("" + 126, "Tool index out of bounds"); /* convert_tool_length_offset */
			ErrorNumToTextMap.Add("" + 127, "Tool radius not less than arc radius with cutter radius comp"); /* arc_data_comp_r, convert_arc_comp2 */
			ErrorNumToTextMap.Add("" + 128, "Two G codes used from same modal group"); /* read_g */
			ErrorNumToTextMap.Add("" + 129, "Two M codes used from same modal group"); /* read_m */
			ErrorNumToTextMap.Add("" + 130, "Unable to open file"); /* rs274ngc_open */
			ErrorNumToTextMap.Add("" + 131, "Unclosed comment found"); /* close_and_downcase */
			ErrorNumToTextMap.Add("" + 132, "Unclosed expression"); /* read_operation */
			ErrorNumToTextMap.Add("" + 133, "Unknown G code used"); /* read_g */
			ErrorNumToTextMap.Add("" + 134, "Unknown M code used"); /* read_m */
			ErrorNumToTextMap.Add("" + 135, "Unknown operation name starting with A"); /* read_operation */
			ErrorNumToTextMap.Add("" + 136, "Unknown operation name starting with M"); /* read_operation */
			ErrorNumToTextMap.Add("" + 137, "Unknown operation name starting with O"); /* read_operation */
			ErrorNumToTextMap.Add("" + 138, "Unknown operation name starting with X"); /* read_operation */
			ErrorNumToTextMap.Add("" + 139, "Unknown operation"); /* read_operation */
			ErrorNumToTextMap.Add("" + 140, "Unknown word starting with A"); /* read_operation_unary */
			ErrorNumToTextMap.Add("" + 141, "Unknown word starting with C"); /* read_operation_unary */
			ErrorNumToTextMap.Add("" + 142, "Unknown word starting with E"); /* read_operation_unary */
			ErrorNumToTextMap.Add("" + 143, "Unknown word starting with F"); /* read_operation_unary */
			ErrorNumToTextMap.Add("" + 144, "Unknown word starting with L"); /* read_operation_unary */
			ErrorNumToTextMap.Add("" + 145, "Unknown word starting with R"); /* read_operation_unary */
			ErrorNumToTextMap.Add("" + 146, "Unknown word starting with S"); /* read_operation_unary */
			ErrorNumToTextMap.Add("" + 147, "Unknown word starting with T"); /* read_operation_unary */
			ErrorNumToTextMap.Add("" + 148, "Unknown word where unary operation could be"); /* read_operation_unary */
			ErrorNumToTextMap.Add("" + 149, "X and Y words missing for arc in XY-plane"); /* convert_arc */
			ErrorNumToTextMap.Add("" + 150, "X and Z words missing for arc in XZ-plane"); /* convert_arc */
			ErrorNumToTextMap.Add("" + 151, "X, Y, and Z words all missing with G0 or G1"); /* convert_straight */
			ErrorNumToTextMap.Add("" + 152, "X, Y, and Z words all missing with G38.2"); /* convert_probe */
			ErrorNumToTextMap.Add("" + 153, "Y and Z words missing for arc in YZ-plane"); /* convert_arc */
			ErrorNumToTextMap.Add("" + 154, "Z value unspecified in canned cycle"); /* convert_cycle */
			ErrorNumToTextMap.Add("" + 155, "Zero or negative argument to ln"); /* execute_unary */
			ErrorNumToTextMap.Add("" + 200, "Bad setting of g_mode in check_g_codes");
			ErrorNumToTextMap.Add("" + 201, "Code is not G0 or G1 in convert_straight");
			ErrorNumToTextMap.Add("" + 202, "Code is not G0 to G3 or G80 to G89 in convert_motion");
			ErrorNumToTextMap.Add("" + 203, "Code is not G10, G92, or G92.2 in convert_modal_0");
			ErrorNumToTextMap.Add("" + 204, "Code is not G17, G18, or G19 in convert_set_plane");
			ErrorNumToTextMap.Add("" + 205, "Code is not G2 or G3 in arc_data_comp");
			ErrorNumToTextMap.Add("" + 206, "Code is not G2 or G3 in arc_data_ijk");
			ErrorNumToTextMap.Add("" + 207, "Code is not G20 or G21 in convert_length_units");
			ErrorNumToTextMap.Add("" + 208, "Code is not G40, G41, or G42 in convert_cutter_compensation");
			ErrorNumToTextMap.Add("" + 209, "Code is not G43 or G49 in convert_tool_length_offset");
			ErrorNumToTextMap.Add("" + 210, "Code is not G54 to G59.3 in convert_coordinate_system");
			ErrorNumToTextMap.Add("" + 211, "Code is not G61 or G64 in convert_control_mode");
			ErrorNumToTextMap.Add("" + 212, "Code is not G90 or G91 in convert_distance_mode");
			ErrorNumToTextMap.Add("" + 213, "Code is not G92 or G92.2 in convert_axis_offsets");
			ErrorNumToTextMap.Add("" + 214, "Code is not G93 or G94 in convert_feed_mode");
			ErrorNumToTextMap.Add("" + 215, "Code is not G98 or G99 in convert_retract_mode");
			ErrorNumToTextMap.Add("" + 216, "Code is not M0, M1, M2, M30 or M60 in convert_stop");
			ErrorNumToTextMap.Add("" + 217, "Convert_cycle should not have been called");
			ErrorNumToTextMap.Add("" + 218, "Distance mode is neither absolute nor incremental in convert_cycle");
			ErrorNumToTextMap.Add("" + 219, "Plane is not XY, YZ, or XZ in convert_arc");
			ErrorNumToTextMap.Add("" + 220, "Read_comment should not have been called");
			ErrorNumToTextMap.Add("" + 221, "Read_d should not have been called");
			ErrorNumToTextMap.Add("" + 222, "Read_f should not have been called");
			ErrorNumToTextMap.Add("" + 223, "Read_g should not have been called");
			ErrorNumToTextMap.Add("" + 224, "Read_i should not have been called");
			ErrorNumToTextMap.Add("" + 225, "Read_j should not have been called");
			ErrorNumToTextMap.Add("" + 226, "Read_k should not have been called");
			ErrorNumToTextMap.Add("" + 227, "Read_l should not have been called");
			ErrorNumToTextMap.Add("" + 228, "Read_line_number should not have been called");
			ErrorNumToTextMap.Add("" + 229, "Read_m should not have been called");
			ErrorNumToTextMap.Add("" + 230, "Read_p should not have been called");
			ErrorNumToTextMap.Add("" + 231, "Read_parameter should not have been called");
			ErrorNumToTextMap.Add("" + 232, "Read_parameter_setting should not have been called");
			ErrorNumToTextMap.Add("" + 233, "Read_q should not have been called");
			ErrorNumToTextMap.Add("" + 234, "Read_r should not have been called");
			ErrorNumToTextMap.Add("" + 235, "Read_real_expression should not have been called");
			ErrorNumToTextMap.Add("" + 236, "Read_spindle_speed should not have been called");
			ErrorNumToTextMap.Add("" + 237, "Read_tool should not have been called");
			ErrorNumToTextMap.Add("" + 238, "Read_tool_length_offset should not have been called");
			ErrorNumToTextMap.Add("" + 239, "Read_x should not have been called");
			ErrorNumToTextMap.Add("" + 240, "Read_y should not have been called");
			ErrorNumToTextMap.Add("" + 241, "Read_z should not have been called");
			ErrorNumToTextMap.Add("" + 242, "Side fails to be right or left in convert_straight_comp1");
			ErrorNumToTextMap.Add("" + 243, "Side fails to be right or left in convert_straight_comp2");
			ErrorNumToTextMap.Add("" + 244, "Sscanf failure in read_integer_unsigned");
			ErrorNumToTextMap.Add("" + 245, "Sscanf failure in read_real_number");
			ErrorNumToTextMap.Add("" + 246, "Unknown operation in execute_binary1");
			ErrorNumToTextMap.Add("" + 247, "Unknown operation in execute_binary2");
			ErrorNumToTextMap.Add("" + 248, "Unknown operation in execute_unary");
			ErrorNumToTextMap.Add("" + 249, "");		
		}

		int _textline = 0;					/* set to 1 in rs274ngc_open */

		string _interpreter_filename;		/* file name */

		string _interpreter_linetext;		/* raw text */
		string _interpreter_blocktext;		/* parsed text */
		block _interpreter_block = new block();    /* parsed next block */

		//		FILE *_interpreter_fp = null;           /* file pointer */

		int _interpreter_status = 0;

		static int _interpreter_length = 0;			/* length of next line */
		static int[] _interpreter_active_g_codes = new int[RS274NGC_ACTIVE_G_CODES];
		static int[] _interpreter_active_m_codes = new int[RS274NGC_ACTIVE_M_CODES];
		static double[] _interpreter_active_settings = new double[RS274NGC_ACTIVE_SETTINGS];

		// the file to use for storing params
		//		char RS274NGC_PARAMETER_FILE[INTERP_TEXT_SIZE] =
		//								DEFAULT_RS274NGC_PARAMETER_FILE;

		/****************************************************************************/

		/* utility_error_number

		Returned Value: int (index number of error message in error_array)

		Side effects: (none)

		Called by: ERROR_MACRO, BUG_MACRO

		This reads through the error message array until it finds one that
		matches the given error message; then it returns the index of that
		message in the array.  If the error message is not found, this returns
		zero, the index of a string that says "Unspecified error". If zero
		is ever returned, that indicates a bug in the error message array,
		because all error messages in the system are supposed to be in that
		array.

		The initial value of index is always either 0 or 200 when this is
		called, since the input error messages start at index 0 and the
		internal error messages start at index 200 in the array.

		This is a slow way to report errors, but all of these errors stop
		the show, anyway, so an extra millisecond or two is not significant.

		*/

		//		int utility_error_number(/* ARGUMENT VALUES           */
		//		char * message,         /* string: the message text  */
		//		char * error_array[],   /* array of error messages   */
		//		int index)              /* index to start at         */
		//		{
		//		for (;error_array[index][0] != 0; index++)
		//			{
		//			if (strcmp(message, error_array[index]) == 0)
		//				return index;
		//			}
		//		return 0;
		//		}

		/****************************************************************************/

		double Hypot(double a, double b)
		{
			return Math.Sqrt(Math.Pow(a,2.0) + Math.Pow(b,2.0));
		}

		/* execute_binary1

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. the operation is unknown.
		2. An attempt is made to divide by zero.
		3. An attempt is made to raise a negative number to a non-integer power.

		Side effects:
		The result from performing the operation is put into what left points at.

		Called by: read_real_expression.

		This executes the operations: DIVIDED_BY, MODULO, POWER, TIMES.

		*/

		int execute_binary1( /* ARGUMENT VALUES                 */
			ref double left,      /* pointer to the left operand     */
			int operation,      /* integer code for the operation  */
			double right)     /* pointer to the right operand    */
		{
			switch (operation)
			{
				case DIVIDED_BY:
					if (right == 0.0)
						return ErrorNumberOf("Attempt to divide by zero");
					else
					{
						left = (left / right);
						return RS274NGC_OK;
					}
				case MODULO: /* always calculates a positive answer */
					left = left % right;
					if (left < 0.0)
					{
						left = (left + Math.Abs(right));
					}
					return RS274NGC_OK;
				case POWER:
					if ((left < 0.0) && (Math.Floor(right) != right))
						return ErrorNumberOf("Cannot raise negative number to non-integer power");
					left = Math.Pow(left, right);
					return RS274NGC_OK;
				case TIMES:
					left = (left * right);
					return RS274NGC_OK;
				default:
					throw new Exception("Unknown operation in execute_binary1");
			}
		}

		/****************************************************************************/

		/* execute_binary2

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If the operation is unknown, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.

		Side effects:
		The result from performing the operation is put into what left points at.

		Called by: read_real_expression.

		This executes the operations: LOGICAL_AND, EXCLUSIVE_OR, MINUS,
		NON_EXCLUSIVE_OR, PLUS. The manual [NCMS] does not say what
		the calculated value of the three logical operations should be. This
		function calculates either 1.0 (meaning true) or 0.0 (meaning false).
		Any non-zero input value is taken as meaning true, and only 0.0 means
		false.


		*/

		int execute_binary2( /* ARGUMENT VALUES                 */
			ref double left,      /* pointer to the left operand     */
			int operation,      /* integer code for the operation  */
			double right)     /* pointer to the right operand    */
		{
			switch (operation)
			{
				case LOGICAL_AND:
					left = ((left == 0.0) || (right == 0.0)) ? 0.0 : 1.0;
					return RS274NGC_OK;
				case EXCLUSIVE_OR:
					left = (((left == 0.0) && (right != 0.0)) ||
						((left != 0.0) && (right == 0.0))) ? 1.0 : 0.0;
					return RS274NGC_OK;
				case MINUS:
					left = (left - right);
					return RS274NGC_OK;
				case NON_EXCLUSIVE_OR:
					left = ((left != 0.0) || (right != 0.0)) ? 1.0 : 0.0;
					return RS274NGC_OK;
				case PLUS:
					left = (left + right);
					return RS274NGC_OK;
				default:
					throw new Exception("Unknown operation in execute_binary2");
			}
		}

		/****************************************************************************/

		/* execute_unary

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. the operation is unknown
		2. the argument to acos or Math.Asin( is not between minus and plus one
		3. the argument to the natural logarithm is not positive
		4. the argument to square root is negative

		Side effects:
		The result from performing the operation on the value in double_ptr
		is put into what double_ptr points at.

		Called by: read_unary.

		This executes the operations: ABS, ACOS, ASIN, COS, EXP, FIX, FUP, LN
		ROUND, SIN, SQRT, TAN

		All angle measures in the input or output are in degrees.

		*/

		int execute_unary (   /* ARGUMENT VALUES                 */
			ref double double_ptr, /* pointer to the operand          */
			int operation)       /* integer code for the operation  */
		{
			switch (operation)
			{
				case ABS:
					if (double_ptr < 0.0)
						double_ptr = (-1.0 * double_ptr);
					return RS274NGC_OK;
				case ACOS:
					if ((double_ptr < -1.0) || (double_ptr > 1.0))
						return ErrorNumberOf("Argument to acos out of range");
					else
					{
						double_ptr = Math.Acos(double_ptr);
						double_ptr = ((double_ptr * 180.0)/ PI);
						return RS274NGC_OK;
					}
				case ASIN:
					if ((double_ptr < -1.0) || (double_ptr > 1.0))
						return ErrorNumberOf("Argument to asin out of range");
					else
					{
						double_ptr = Math.Asin(double_ptr);
						double_ptr = ((double_ptr * 180.0) / PI);
						return RS274NGC_OK;
					}
				case COS:
					double_ptr = Math.Cos((double_ptr * PI) / 180.0);
					return RS274NGC_OK;
				case EXP:
					double_ptr = Math.Exp(double_ptr);
					return RS274NGC_OK;
				case FIX:
					double_ptr = Math.Floor(double_ptr);
					return RS274NGC_OK;
				case FUP:
					double_ptr = Math.Ceiling(double_ptr);
					return RS274NGC_OK;
				case LN:
					if (double_ptr <= 0.0)
						return ErrorNumberOf("Zero or negative argument to ln");
					else
					{
						double_ptr = Math.Log(double_ptr);
						return RS274NGC_OK;
					}
				case ROUND:
					double_ptr = (double)
						((int) (double_ptr + ((double_ptr < 0.0) ? -0.5 : 0.5)));
					return RS274NGC_OK;
				case SIN:
					double_ptr = Math.Sin((double_ptr * PI)/180.0);
					return RS274NGC_OK;
				case SQRT:
					if (double_ptr < 0.0)
						return ErrorNumberOf("Negative argument to sqrt");
					else
					{
						double_ptr = Math.Sqrt(double_ptr);
						return RS274NGC_OK;
					}
				case TAN:
					double_ptr = Math.Tan((double_ptr * PI)/180.0);
					return RS274NGC_OK;
				default:
					throw new Exception("Unknown operation in execute_unary");
			}
		}

		/****************************************************************************/
		/* read_atan

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. the first character to read is not a slash
		2. the second character to read is not a left bracket
		3. read_real_expression returns RS274NGC_ERROR while reading
			the second expression

		Side effects:
		The computed value is put into what double_ptr points at.
		The counter is reset to point to the first character after the
		characters which make up the value.

		Called by:
		read_unary

		When this function is called, the characters "atan" and the first
		argument have already been read, and the value of the first argument
		is stored in double_ptr.  This function attempts to read a slash and
		the second argument to the atan function, starting at the index given
		by the counter and then to compute the value of the atan operation
		applied to the two arguments.  The computed value is inserted into
		what double_ptr points at.

		The computed value is in the range from -180 degrees to +180 degrees.
		The range is not specified in the manual [NCMS, page 51],
		although using degrees (not radians) is specified.

		*/

		int read_atan(        /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274/NGC code being processed */
			ref int counter,       /* pointer to a counter for position on line      */
			ref double double_ptr, /* pointer to double to be read                   */
			ref double[] parameters) /* array of system parameters                     */
		{
			double argument2;

			if (line[counter] != '/')
				return ErrorNumberOf("Slash missing");

			counter = (counter + 1);

			if(line[counter] != '[')
				return ErrorNumberOf("Left bracket missing after slash with atan operator");

			if (read_real_expression (line, ref counter, out argument2, ref parameters) == RS274NGC_ERROR)
				return RS274NGC_ERROR;

			double_ptr = Math.Atan2(double_ptr, argument2); /* value in radians */
			double_ptr = ((double_ptr * 180.0)/PI);    /* convert to degrees */
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* read_integer_value

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If read_real_value returns RS274NGC_ERROR or if the returned value is not
		close to an integer, this returns RS274NGC_ERROR. Otherwise, it
		returns RS274NGC_OK.

		Side effects:
		The number read from the line is put into what integer_ptr points at.

		Called by:
		read_d
		read_l
		read_m
		read_parameter
		read_parameter_setting
		read_tool
		read_tool_length_offset

		This reads an integer (positive, negative or zero) from a string,
		starting from the position given by *counter. The value being
		read may be written with a decimal point or it may be an expression
		involving non-integers, as long as the result comes out within 0.0001
		of an integer.

		This proceeds by calling read_real_value and checking that it is
		close to an integer, then returning the integer it is close to.

		*/

		int read_integer_value(   /* ARGUMENT VALUES                                */
			string line,             /* string: line of RS274/NGC code being processed */
			ref int counter,           /* pointer to a counter for position on the line  */
			out int integer_ptr,       /* pointer to the value being read                */
			ref double[] parameters)     /* array of system parameters                     */
		{
			double float_value;
			integer_ptr = 0;

			if (read_real_value(line, ref counter, out float_value, ref parameters) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			integer_ptr = (int)Math.Floor(float_value);
			if ((float_value - integer_ptr) > 0.9999)
			{
				integer_ptr = (int)Math.Ceiling(float_value);
			}
			else if ((float_value - integer_ptr) > 0.0001)
				return ErrorNumberOf("Non-integer value for integer");
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* read_operation

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If the operation is unknown or if the line ends without closing the
		expression, this returns RS274NGC_ERROR. Otherwise, it returns RS274NGC_OK.

		Side effects:
		An integer representing the operation is put into what operation points at.
		The counter is reset to point to the first character after the operation.

		Called by: read_real_expression

		*/

		int read_operation(   /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274/NGC code being processed */
			ref int counter,       /* pointer to a counter for position on the line  */
			out int operation)     /* pointer to operation to be read                */
		{			
			operation = -1;
			char c = line[counter];
			counter = (counter + 1);
			switch(c)
			{
				case '+':
					operation = PLUS;
					return RS274NGC_OK;
				case '-':
					operation = MINUS;
					return RS274NGC_OK;
				case '/':
					operation = DIVIDED_BY;
					return RS274NGC_OK;
				case '*':
					if(line.Substring(counter).StartsWith("*"))
					{
						operation = POWER;
						counter = (counter + 1);
						return RS274NGC_OK;
					}
					else
					{
						operation = TIMES;
						return RS274NGC_OK;
					}
				case ']':
					operation = RIGHT_BRACKET;
					return RS274NGC_OK;
				case 'a':
					if(line.Substring(counter).StartsWith("nd"))
					{
						operation = LOGICAL_AND;
						counter = (counter + 2);
						return RS274NGC_OK;
					}
					else
						return ErrorNumberOf("Unknown operation name starting with A");
				case 'm':
					if(line.Substring(counter).StartsWith("od"))
					{
						operation = MODULO;
						counter = (counter + 2);
						return RS274NGC_OK;
					}
					else
						return ErrorNumberOf("Unknown operation name starting with M");
				case 'o':
					if(line.Substring(counter).StartsWith("r"))
					{
						operation = NON_EXCLUSIVE_OR;
						counter = (counter + 1);
						return RS274NGC_OK;
					}
					else
						return ErrorNumberOf("Unknown operation name starting with O");
				case 'x':
					if(line.Substring(counter).StartsWith("or"))
					{
						operation = EXCLUSIVE_OR;
						counter = (counter + 2);
						return RS274NGC_OK;
					}
					else
						return ErrorNumberOf("Unknown operation name starting with X");
				default:
					return ErrorNumberOf("Unknown operation");
			}
		}

		/****************************************************************************/

		/* read_operation_unary

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If the operation is not a known operation, this returns RS274NGC_ERROR.
		Otherwise, this returns RS274NGC_OK.

		Side effects:
		An integer code for the name of the operation read from the
		line is put into what operation points at.
		The counter is reset to point to the first character after the
		characters which make up the operation name.

		Called by:
		read_unary

		This attempts to read the name of a unary operation out of the line,
		starting at the index given by the counter.

		*/

		int read_operation_unary( /* ARGUMENT VALUES                                */
			string line,             /* string: line of RS274/NGC code being processed */
			ref int counter,           /* pointer to a counter for position on the line  */
			out int operation)         /* pointer to operation to be read                */
		{
			operation = 1;
			char c = line[counter];
			counter++;
			switch (c)
			{
				case 'a':
					if(line.Substring(counter).StartsWith("bs"))
					{
						operation = ABS;
						counter = (counter + 2);
						return RS274NGC_OK;
					}
					else if(line.Substring(counter).StartsWith("cos"))
					{
						operation = ACOS;
						counter = (counter + 3);
						return RS274NGC_OK;
					}
					else if(line.Substring(counter).StartsWith("sin"))
					{
						operation = ASIN;
						counter = (counter + 3);
						return RS274NGC_OK;
					}
					else if(line.Substring(counter).StartsWith("tan"))
					{
						operation = ATAN;
						counter = (counter + 3);
						return RS274NGC_OK;
					}
					else
						return ErrorNumberOf("Unknown word starting with A");
				case 'c':
					if(line.Substring(counter).StartsWith("os"))
					{
						operation = COS;
						counter = (counter + 2);
						return RS274NGC_OK;
					}
					else
						return ErrorNumberOf("Unknown word starting with C");
				case 'e':
					if(line.Substring(counter).StartsWith("xp"))
					{
						operation = EXP;
						counter = (counter + 2);
						return RS274NGC_OK;
					}
					else
						return ErrorNumberOf("Unknown word starting with E");
				case 'f':
					if(line.Substring(counter).StartsWith("ix"))
					{
						operation = FIX;
						counter = (counter + 2);
						return RS274NGC_OK;
					}
					else if(line.Substring(counter).StartsWith("up"))
					{
						operation = FUP;
						counter = (counter + 2);
						return RS274NGC_OK;
					}
					else
						return ErrorNumberOf("Unknown word starting with F");
				case 'l':
					if(line.Substring(counter).StartsWith("n"))
					{
						operation = LN;
						counter = (counter + 1);
						return RS274NGC_OK;
					}
					else
						return ErrorNumberOf("Unknown word starting with L");
				case 'r':
					if(line.Substring(counter).StartsWith("ound"))
					{
						operation = ROUND;
						counter = (counter + 4);
						return RS274NGC_OK;
					}
					else
						return ErrorNumberOf("Unknown word starting with R");
				case 's':
					if(line.Substring(counter).StartsWith("in"))
					{
						operation = SIN;
						counter = (counter + 2);
						return RS274NGC_OK;
					}
					else if(line.Substring(counter).StartsWith("qrt"))
					{
						operation = SQRT;
						counter = (counter + 3);
						return RS274NGC_OK;
					}
					else
						return ErrorNumberOf("Unknown word starting with S");
				case 't':
					if(line.Substring(counter).StartsWith("an"))
					{
						operation = TAN;
						counter = (counter + 2);
						return RS274NGC_OK;
					}
					else
						return ErrorNumberOf("Unknown word starting with T");
				default:
					return ErrorNumberOf("Unknown word where unary operation could be");
			}
		}

		/****************************************************************************/

		/* utility_find_turn

		Returned Value: double (angle between two radii of a circle)

		Side effects: none

		Called by:
		utility_find_arc_length
		convert_arc_comp1
		convert_arc_comp2
		convert_arc_xy
		convert_arc_yz
		convert_arc_zx

		*/

		double utility_find_turn( /* ARGUMENT VALUES            */
			double x1,       /* X-coordinate of start point        */
			double y1,       /* Y-coordinate of start point        */
			double center_x, /* X-coordinate of arc center         */
			double center_y, /* Y-coordinate of arc center         */
			int turn,        /* no. of full or partial circles CCW */
			double x2,       /* X-coordinate of end point          */
			double y2)       /* Y-coordinate of end point          */
		{
			double alpha;
			double beta;
			double theta;  /* amount of turn of arc CCW - negative if CW */

			if (turn == 0)
				return 0.0;
			alpha = Math.Atan2((y1 - center_y), (x1 - center_x));
			beta = Math.Atan2((y2 - center_y), (x2 - center_x));
			if (turn > 0)
			{
				if (beta <= alpha)
					beta = (beta + TWO_PI);
				theta = ((beta - alpha) + ((turn - 1) * TWO_PI));
			}
			else /* turn < 0 */
			{
				if (alpha <= beta)
					alpha = (alpha + TWO_PI);
				theta = ((beta - alpha) + ((turn + 1) * TWO_PI));
			}
			return (theta);
		}

		/****************************************************************************/

		/* arc_data_comp_ijk

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. The two calculable values of the radius differ by more than
			tolerance.
		2. BUG - move is not G_2 or G_3.

		Side effects:
		This finds and sets the values of center_x, center_y, and turn.

		Called by: convert_arc_comp1

		This finds the center coordinates and number of full or partial turns
		counterclockwise of a helical or circular arc (call it arc1) in
		ijk-format in the XY plane.  Arc2 is constructed so that it is tangent
		to a circle whose radius is tool_radius and whose center is at the
		point (current_x, current_y) and passes through the point (end_x,
		end_y). Arc1 has the same center as arc2. The radius of arc1 is one
		tool radius larger or smaller than the radius of arc2.

		*/

		int arc_data_comp_ijk(  /* ARGUMENT VALUES                               */
			int move,           /* either G_2 (cw arc) or G_3 (ccw arc)             */
			CANON_CUTTER_COMP side,           /* either CANON_CUTTER_COMP.RIGHT or CANON_CUTTER_COMP.LEFT                             */
			double tool_radius, /* radius of the tool                               */
			double current_x,   /* first coordinate of current point                */
			double current_y,   /* second coordinate of current point               */
			double end_x,       /* first coordinate of arc end point                */
			double end_y,       /* second coordinate of arc end point               */
			double i_number,    /* first coordinate offset of center from current   */
			double j_number,    /* second coordinate offset of center from current  */
			out double center_x,  /* pointer to first coordinate of center of arc     */
			out double center_y,  /* pointer to second coordinate of center of arc    */
			out int turn,         /* pointer to number of full or partial circles CCW */
			double tolerance)   /* tolerance of differing radii                     */
		{
			double arc_radius;
			double radius2;
			center_x = (current_x + i_number);
			center_y = (current_y + j_number);
			turn = 1;
			arc_radius = Hypot((center_x - current_x), (center_y - current_y));
			radius2 = Hypot((center_x - end_x), (center_y - end_y));
			radius2 =
				(((side == CANON_CUTTER_COMP.LEFT ) && (move == 30)) ||
				((side == CANON_CUTTER_COMP.RIGHT) && (move == 20))) ?
				(radius2 - tool_radius): (radius2 + tool_radius);
			if (Math.Abs(arc_radius - radius2) > tolerance)
				return ErrorNumberOf("Radius to end of arc differs from radius to start of arc");
			/* This catches an arc too small for the tool, also */
			if (move == G_2)
				turn = -1;
			else if (move == G_3)
				turn = 1;
			else
				throw new Exception("Code is not G2 or G3 in arc_data_comp");
			return RS274NGC_OK;
		}

		/****************************************************************************/
		/* arc_data_comp_r

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. The arc radius is too small to reach the end point.
		2. The current point is the same as the end point of the arc (so that
			it is not possible to locate the center of the circle).
		3. The arc radius is not greater than the tool_radius.

		Side effects:
		This finds and sets the values of center_x, center_y, and turn.

		Called by: convert_arc_comp1

		This finds the center coordinates and number of full or partial turns
		counterclockwise of a helical or circular arc (call it arc1) in
		r-format in the XY plane.  Arc2 is constructed so that it is tangent
		to a circle whose radius is tool_radius and whose center is at the
		point (current_x, current_y) and passes through the point (end_x,
		end_y). Arc1 has the same center as arc2. The radius of arc1 is one
		tool radius larger or smaller than the radius of arc2.

		If the value of the big_radius argument is negative, that means [NCMS,
		page 21] that an arc larger than a semicircle is to be made.
		Otherwise, an arc of a semicircle or less is made.

		The algorithm implemented here is to construct a line L from the
		current point to the end point, and a perpendicular to it from the
		center of the arc which intersects L at point P. Since the distance
		from the end point to the center and the distance from the current
		point to the center are known, two equations for the length of the
		perpendicular can be written. The right sides of the equations can be
		set equal to one another and the resulting equation solved for the
		length of the line from the current point to P. Then the location of
		P, the length of the perpendicular, the angle of the perpendicular,
		and the location of the center, can be found in turn.

		*/

		int arc_data_comp_r( /* ARGUMENT VALUES                                  */
			int move,           /* either G_2 (cw arc) or G_3 (ccw arc)             */
			CANON_CUTTER_COMP side,           /* either CANON_CUTTER_COMP.RIGHT or CANON_CUTTER_COMP.LEFT                             */
			double tool_radius, /* radius of the tool                               */
			double current_x,   /* first coordinate of current point                */
			double current_y,   /* second coordinate of current point               */
			double end_x,       /* first coordinate of arc end point                */
			double end_y,       /* second coordinate of arc end point               */
			double big_radius,  /* radius of arc                                    */
			out double center_x,  /* pointer to first coordinate of center of arc     */
			out double center_y,  /* pointer to second coordinate of center of arc    */
			out int turn)         /* pointer to number of full or partial circles CCW */
		{
			double abs_radius; /* absolute value of big_radius */
			double radius2;    /* distance from center to current point */
			double distance;   /* length of line L from current to end */
			double mid_length; /* length from current point to point P */
			double offset;     /* length of line from P to center */
			double alpha;      /* direction of line from current to end */
			double theta;      /* direction of line from P to center */
			double mid_x;      /* x-value of point P */
			double mid_y;      /* y-value of point P */

			center_x = center_y = turn = 0;

			if ((end_x == current_x) && (end_y == current_y))
				return ErrorNumberOf("Current point same as end point of arc");
			abs_radius = Math.Abs(big_radius);
			if ((abs_radius < tool_radius) && (((side == CANON_CUTTER_COMP.LEFT ) && (move == G_3)) ||
				((side == CANON_CUTTER_COMP.RIGHT) && (move == G_2))))
				return ErrorNumberOf("Tool radius not less than arc radius with cutter radius comp");

			distance = Hypot((end_x - current_x), (end_y - current_y));
			alpha = Math.Atan2 ((end_y - current_y), (end_x - current_x));
			theta = (((move == G_3) && (big_radius > 0)) ||
				((move == G_2) && (big_radius < 0))) ?
				(alpha + PI2) : (alpha - PI2);
			radius2 = (((side == CANON_CUTTER_COMP.LEFT ) && (move == G_3)) ||
				((side == CANON_CUTTER_COMP.RIGHT) && (move == G_2))) ?
				(abs_radius - tool_radius) : (abs_radius + tool_radius);
			if (distance > (radius2 + abs_radius))
				return ErrorNumberOf("Radius too small to reach end point");
			mid_length = (((radius2 * radius2) + (distance * distance) -
				(abs_radius * abs_radius)) / (2.0 * distance));
			mid_x = (current_x + (mid_length * Math.Cos(alpha)));
			mid_y = (current_y + (mid_length * Math.Sin(alpha)));
			offset = Math.Sqrt((radius2 * radius2) - (mid_length * mid_length));
			center_x = mid_x + (offset * Math.Cos(theta));
			center_y = mid_y + (offset * Math.Sin(theta));
			turn = (move == G_2) ? -1 : 1;

			return RS274NGC_OK;
		}

		/****************************************************************************/
		/* arc_data_ijk

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. The two calculable values of the radius differ by more than
			tolerance.
		2. BUG - The move code is not G_2 or G_3.

		Side effects:
		This finds and sets the values of center_x, center_y, and turn.

		Called by:
		convert_arc_xy
		convert_arc_xz
		convert_arc_yz
		convert_arc_comp2

		This finds the center coordinates and number of full or partial turns
		counterclockwise of a helical or circular arc in ijk-format. This
		function is used by the arc functions for all three planes, so "x" and
		"y" really mean "first_coordinate" and "second_coordinate" wherever
		they are used here as suffixes of variable names. The i and j prefixes
		are handled similarly.

		*/

		int arc_data_ijk(   /* ARGUMENT VALUES                                 */
			int move,          /* either G_2 (cw arc) or G_3 (ccw arc)            */
			double current_x,  /* first coordinate of current point               */
			double current_y,  /* second coordinate of current point              */
			double end_x,      /* first coordinate of arc end point               */
			double end_y,      /* second coordinate of arc end point              */
			double i_number,   /* first coordinate offset of center from current  */
			double j_number,   /* second coordinate offset of center from current */
			out double center_x, /* pointer to first coordinate of center of arc    */
			out double center_y, /* pointer to second coordinate of center of arc   */
			out int turn,        /* pointer to no. of full or partial circles CCW   */
			double tolerance)  /* tolerance of differing radii                    */
		{
			double radius;    /* radius to current point */
			double radius2;   /* radius to end point     */
			center_x = (current_x + i_number);
			center_y = (current_y + j_number);
			turn = 1;
			radius = Hypot((center_x - current_x), (center_y - current_y));
			radius2 = Hypot((center_x - end_x), (center_y - end_y));
			if (Math.Abs(radius - radius2) > tolerance)
				return ErrorNumberOf("Radius to end of arc differs from radius to start of arc");
			if (move == G_2)
				turn = -1;
			else if (move == G_3)
				turn = 1;
			else
				throw new Exception("Code is not G2 or G3 in arc_data_ijk");
			return RS274NGC_OK;
		}

		/****************************************************************************/
		/* arc_data_r

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. The radius is too small to reach the end point.
		2. The current point is the same as the end point of the arc
			(so that it is not possible to locate the center of the circle).

		Side effects:
		This finds and sets the values of center_x, center_y, and turn.

		Called by:
		convert_arc_xy
		convert_arc_xz
		convert_arc_yz
		convert_arc_comp2

		This finds the center coordinates and number of full or partial turns
		counterclockwise of a helical or circular arc in the r format. This
		function is used by the arc functions for all three planes, so "x" and
		"y" really mean "first_coordinate" and "second_coordinate" wherever
		they are used here as suffixes of variable names.

		If the value of the radius argument is negative, that means [NCMS,
		page 21] that an arc larger than a semicircle is to be made.
		Otherwise, an arc of a semicircle or less is made.

		The algorithm used here is based on finding the midpoint M of the line
		L between the current point and the end point of the arc. The center
		of the arc lies on a line through M perpendicular to L.

		*/

		int arc_data_r(     /* ARGUMENT VALUES                               */
			int move,          /* either G_2 (cw arc) or G_3 (ccw arc)          */
			double current_x,  /* first coordinate of current point             */
			double current_y,  /* second coordinate of current point            */
			double end_x,      /* first coordinate of arc end point             */
			double end_y,      /* second coordinate of arc end point            */
			double radius,     /* radius of arc                                 */
			out double center_x, /* pointer to first coordinate of center of arc  */
			out double center_y, /* pointer to second coordinate of center of arc */
			out int turn)        /* pointer to no. of full or partial circles CCW */
		{
			double abs_radius; /* absolute value of given radius */
			double half_length;
			double turn2;  /* absolute value of half of turn */
			double offset; /* distance from M to center */
			double theta;  /* angle of line from M to center */
			double mid_x;  /* first coordinate of M */
			double mid_y;  /* second coordinate of M */

			center_x = center_y = turn = 0;

			if ((end_x == current_x) && (end_y == current_y))
				return ErrorNumberOf("Current point same as end point of arc");
			abs_radius = Math.Abs(radius);
			mid_x = (end_x + current_x)/2.0;
			mid_y = (end_y + current_y)/2.0;
			half_length = Hypot((mid_x - end_x), (mid_y - end_y));
			if ((half_length/abs_radius) > (1+TINY))
				return ErrorNumberOf("Arc radius too small to reach end point");
			else if ((half_length/abs_radius) > (1-TINY))
				half_length = abs_radius; /* allow a small error for semicircle */
			/* check needed before calling Math.Asin(   */
			if (((move == G_2) && (radius > 0)) ||
				((move == G_3) && (radius < 0)))
				theta = Math.Atan2((end_y - current_y), (end_x - current_x)) - PI2;
			else
				theta = Math.Atan2((end_y - current_y), (end_x - current_x)) + PI2;

			turn2 = Math.Asin(half_length/abs_radius);
			offset = abs_radius * Math.Cos(turn2);
			center_x = mid_x + (offset * Math.Cos(theta));
			center_y = mid_y + (offset * Math.Sin(theta));
			turn = (move == G_2) ? -1 : 1;

			return RS274NGC_OK;
		}

		/****************************************************************************/
		/* read_parameter

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, this returns RS274NGC_OK.
		1. BUG - The first character read is not # .
		2. The characters following the # do not form an integer.
		3. The parameter number is out of bounds

		Side effects:
		The value of the given parameter is put into what double_ptr points at.
		The counter is reset to point to the first character after the
		characters which make up the value.

		Called by:
		read_real_value

		This attempts to read the value of a parameter out of the line,
		starting at the index given by the counter.

		According to the manual [NCMS, p. 62], the characters following
		'#' may be any "parameter expression". Thus, the following are legal
		and mean the same thing (the value of the parameter whose number is
		stored in parameter 2):
		'##2'
		'#[#2]'

		*/

		int read_parameter(   /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274/NGC code being processed */
			ref int counter,       /* pointer to a counter for position on the line  */
			out double double_ptr, /* pointer to double to be read                   */
			ref double[] parameters) /* array of system parameters                     */
		{
			int index;

			double_ptr = 0.0;

			if (line[counter] != '#')
				throw new Exception("Read_parameter should not have been called");
			counter = (counter + 1);
			if (read_integer_value(line, ref counter, out index, ref parameters) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else if ((index < 1) || (index >= RS274NGC_MAX_PARAMETERS))
				return ErrorNumberOf("Parameter number out of range");
			else
			{
				double_ptr = parameters[index];
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_real_expression

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character is not [ .
		2. The call back to read_real_value does not return RS274NGC_OK.
		3. read_operation does not return RS274NGC_OK.
		4. execute_binary1 or execute_binary2 does not return RS274NGC_OK.

		Side effects:
		The number read from the line is put into what value_ptr points at.
		The counter is reset to point to the first character after the real
		expression.

		Called by: read_real_value.

		Example 1: [2 - 3 * 4 / 5] means [2 - [[3 * 4] / 5]] and equals -0.4.

		Segmenting Expressions -

		The manual [NCMS, section 3.5.1.1, page 50] provides for
		using square brackets to segment expressions.

		Binary Operations -

		The manual [NCMS section 3.5.1.1] discusses expression evaluation.
		The manual provides for eight binary operations: the four basic
		mathematical operations (addition, subtraction, multiplication,
		division), three logical operations (non-exclusive ||, exclusive ||,
		and LOGICAL_AND) and the modulus operation. The manual does not explicitly call
		these "binary" operations, but implicitly recognizes that they are
		binary. We have added the "power" operation of raising the number
		on the left of the operation to the power on the right; this is
		needed for many basic machining calculations.

		There are two groups of binary operations given in the manual. If
		operations are strung together as shown in Example 1, operations in
		the first group are to be performed before operations in the second
		group. If an expression contains more than one operation from the same
		group (such as * and / in Example 1), the operation on the left is
		performed first. The first group is: multiplication (*), division (/),
		modulus (MOD), and power (**). We added power. The second group is:
		addition(+), subtraction (-), logical non-exclusive || (OR), logical
		exclusive || (XOR), and logical && (AND2).

		The logical operations and modulus are apparently to be performed on
		any real numbers, not just on integers or on some other data type.

		Unary Operations -

		The manual [NCMS, section 3.5.1.2] provides for fifteen unary
		mathematical operations. Two of these, BIN and BCD, are apparently for
		converting between decimal and hexadecimal number representation,
		although the text is not clear. These have not been implemented, since
		we are not using any hexadecimal numbers. The other thirteen unary
		operations have been implemented: absolute_value, arc_cosine, arc_sine,
		arc_tangent, cosine, e_raised_to, fix_down, fix_up, natural_log_of,
		round, sine, square_root, tangent .

		The manual [NCMS, section 3.5.1.2, page 51] requires the argument to
		all unary operations (except atan) to be in square brackets.  Thus,
		for example "sin[90]" is allowed in the interpreter, but "sin 90" is
		not. The atan operation must be in the format "atan[..]/[..]".

		Production Rule Definitions in Terms of Tokens -

		The following is a production rule definition of what this RS274NGC
		interpreter recognizes as valid combinations of symbols which form a
		recognized real_value (the top of this production hierarchy).

		The notion of "integer_value" is used in the interpreter. Below it is
		defined as a synonym for real_value, but in fact a constraint is added
		which cannot be readily written in a production language.  An
		integer_value is a real_value which is very close to an integer.
		Integer_values are needed for array and table indices and (when
		divided by 10) for the values of M codes and G codes. All numbers
		(including integers) are read as real numbers and stored as doubles.
		If an integer_value is required in some situation, a test for being
		close to an integer is applied to the number after it is read.


		arc_tangent_combo = arc_tangent expression divided_by expression .

		binary_operation1 = divided_by | modulo | power | times .

		binary_operation2 = and | exclusive_or | minus |  non_exclusive_or | plus .

		combo1 = real_value { binary_operation1 real_value } .

		digit = zero | one | two | three | four | five | six | seven |eight | nine .

		expression =
		left_bracket
		(unary_combo | (combo1 { binary_operation2 combo1 }))
		right_bracket .

		integer_value = real_value .

		ordinary_unary_combo =  ordinary_unary_operation expression .

		ordinary_unary_operation =
		absolute_value | arc_cosine | arc_sine | cosine | e_raised_to |
		fix_down | fix_up | natural_log_of | round | sine | square_root | tangent .

		parameter_index = integer_value .

		parameter_value = parameter_sign  parameter_index .

		real_number =
		[ plus | minus ]
		(( digit { digit } decimal_point {digit}) | ( decimal_point digit {digit})).

		real_value =
		real_number | expression | parameter_value | unary_combo.

		unary_combo = ordinary_unary_combo | arc_tangent_combo .


		Production Tokens in Terms of Characters -

		absolute_value   = 'abs'
		and              = 'and'
		arc_cosine       = 'acos'
		arc_sine         = 'asin('
		arc_tangent      = 'atan'
		cosine           = 'cos'
		decimal_point    = '.'
		divided_by       = '/'
		eight            = '8'
		exclusive_or     = 'xor'
		e_raised_to      = 'exp'
		five             = '5'
		fix_down         = 'fix'
		fix_up           = 'fup'
		four             = '4'
		left_bracket     = '['
		minus            = '-'
		modulo           = 'mod'
		natural_log_of   = 'ln'
		nine             = '9'
		non_exclusive_or = 'or'
		one              = '1'
		parameter_sign   = '#'
		plus             = '+'
		power            = '**'
		right_bracket    = ']'
		round            = 'round'
		seven            = '7'
		sine             = 'sin'
		six              = '6'
		square_root      = 'sqrt'
		tangent          = 'tan'
		three            = '3'
		times            = '*'
		two              = '2'
		zero             = '0'

		How the Function Works -

		When this function is called, the counter should be set at a left
		bracket. The function reads up to and including the right bracket
		which closes the expression.

		In this explanation we will use "bop1" to mean "binary_operation1"
		as defined above, and "bop2" to mean "binary_operation2". "bop"
		will mean an operation that is either a bop1 or a bop2.

		The basic form of an expression is: [v1 bop v2 bop ... vn], where
		the vi are real_values. Because bop1's are to be evaluated before
		bop2's, it is useful to rewrite the general form collecting any
		subsequences of bop1's. For example, suppose the expression is:
		[9+8*7/6+5-4*3*2+1]. It may be rewritten as: [9+(8*7/6)+5-(4*3*2)+1].

		The manual provides that operations of the same type should be processed
		left to right. The algorithm implemented here works through the
		expression left to right performing bop2's one at time and collecting
		the results in a buffer named hold2. When a subsequence of the
		expression containing bop1's is encountered, the bop1's in the
		subsequence are performed one at a time and the results are collected
		in a buffer named hold1, until the subsequence ends, at which point
		the hold1 buffer is combined with the hold2 buffer using the bop1
		that immediately preceeded the subsequence.

		An expression may be processed left to right by using a loop that
		reads one more real_value and one more bop each time around the loop.
		It is necessary for the reading to be ahead of the execution of bop's
		by one step in order to tell when subsequences of bop1's start and end.
		This algorithm implements such a loop. It keeps track of the following
		information.

		1. the value accumulated so far in hold2 (initialized to 0).
		2. the bop2 waiting to be executed (called waiting_operation
		and initialized to PLUS).
		3. the value accumulated so far in hold1.
		4. the bop which was read the last time around the loop
		(called last_operation and initialized to PLUS).
		5. the value read this time around the loop (called value).
		6. the bop read this time around the loop (called next_operation).

		The function terminates when the next_operation is found to be a right
		bracket and not a bop. When the function is ready to return, hold2
		has been set to the value of the expression, and RS274NGC_OK is returned.

		Without the initializations, special steps would have to be taken
		the first time around the loop, and special steps would have to
		be taken if the expression were simply a number in brackets (e.g. [5]).

		The algorithm (ignoring all the error detection) is:

		A. Initialize.
		B. Loop
		B1. Read the next operation into next_operation and the next
			real_value into value.
		B2. Check for one of three possibilities regarding next_operation:
			B2a. If next_operation is a right bracket:
				B2a(i). If last_operation was a bop1, a subsequence
						of bop1's has just ended, so set hold1
						to (hold1 last_operation value) and then
						set hold2 to (hold2 waiting_operation hold1).
				B2a(ii). Otherwise, last_operation must have been a bop2,
						so set hold2 to (hold2 waiting_operation value).
				B2a(iii). Return RS274NGC_OK.
			B2b. Otherwise, if next_operation is a bop1:
				B2b(i). If last_operation was a bop2, a subsequence of
						bop1's is just starting, so reinitialize hold1
						by setting it to value.
				B2b(ii). Otherwise, last operation must have been a bop1,
						implying a subsequence of bop1's has already been
						started, so set hold1 to (hold1 last_operation value).
			B2c. Otherwise, next_operation must have been a bop2, so:
				B2c(i). If last_operation was a bop1, a subsequence of
						bop1's has just ended, so set hold1 to
						(hold1 last_operation value), set hold2 to
						(hold2 waiting_operation hold1), and set
						waiting_operation to next_operation.
				B2c(ii). Otherwise, last_operation was also a bop2, so
						set hold2 to (hold2 last_operation value), and
						set waiting_operation to next_operation.
		B3. Set last_operation to next_operation, and go back to the top of
			the loop.

		The expression [2 - 3 * 4 / 5] given in Example 1 is processed as
		follows by this procedure:

		- initialize hold2 to 0.0, last_operation to PLUS, and
		waiting_operation to PLUS.

		LOOP1
		- read 2 into value and MINUS into next_operation.
		- since next_operation is a bop2, and last_operation is a bop2,
		set hold2 to the result of applying the waiting_operation
		to hold2 and value (i.e. set hold2 to 0+2 = 2)
		- reset last_operation to MINUS and waiting_operation to MINUS.

		LOOP2
		- read 3 into value and TIMES into next_operation.
		- since next_operation is a bop1 and last_operation is a bop2,
		set hold1 to 3.
		- set last_operation to TIMES (but do not change waiting_operation).

		LOOP3
		- read 4 into value and DIVIDED_BY into next_operation.
		- since next_operation is a bop1 and last operation is a bop1,
		set hold1 to the result of applying last operation to
		hold1 and value (i.e. set hold1 to 3*4 = 12)
		- reset last operation to DIVIDED_BY

		LOOP4
		- read 5 into value and RIGHT_BRACKET into next_operation
		- since next_operation is RIGHT_BRACKET and last operation was a bop1,
		o set hold1 to the result of applying the last_operation
			to hold1 and value (i.e. set hold1 to 12/5 = 2.4)
		o set hold2 to the result of applying the waiting_operation
			to hold2 and hold1 (i.e. set hold2 to 2-2.4 = -0.4)
		o return RS274NGC_OK

		The following table shows the settings of the variables at the end
		of initialization and the end of each loop.

			hold2  hold1  value  waiting_     last_     next_      unread part
									operation  operation  operation  of expression
			_____________________________________________________________________

		INIT    0       ?      ?     PLUS       PLUS         ?       2 - 3 * 4 / 5]

		LOOP1   2.0     ?      2.0   MINUS      MINUS      MINUS         3 * 4 / 5]

		LOOP2   2.0     3.0    3.0   MINUS      TIMES      TIMES             4 / 5]

		LOOP3   2.0    12.0    4.0   MINUS    DIVIDED_BY  DIVIDED_BY             5]

		LOOP4  -0.4     2.4    5.0   MINUS    DIVIDED_BY  RIGHT_BRACKET


		A reasonable alternative to the algorithm here would be to have
		read_real_expression deal with only bop2's and have it call a separate
		function to handle any subsequences of bop1's that might be found.
		(or the two functions could be written to call each other
		recursively). In either case, the read-ahead would still be necessary,
		so the functions would have either to pass the read-ahead information
		back and forth (OK) or to read things twice (bad).

		*/

		int read_real_expression( /* ARGUMENT VALUES                                */
			string line,             /* string: line of RS274/NGC code being processed */
			ref int counter,           /* pointer to a counter for position on the line  */
			out double hold2,          /* pointer to double to be read                   */
			ref double[] parameters)     /* array of system parameters                     */
		{
			double value;
			double hold1 = 0.0;
			int waiting_operation;  /* always a bop2 */
			int last_operation;
			int next_operation;

			if (line[counter] != '[')
				throw new Exception("Read_real_expression should not have been called");
			counter = (counter + 1);
			waiting_operation = PLUS;
			hold2 = 0.0;
			last_operation = PLUS;

			for (;;)
			{
				if (read_real_value(line, ref counter, out value, ref parameters) != RS274NGC_OK)
					return RS274NGC_ERROR;
				if (read_operation(line, ref counter, out next_operation) != RS274NGC_OK)
					return RS274NGC_ERROR;
				if (next_operation == RIGHT_BRACKET)
				{
					if (last_operation < LOGICAL_AND)
					{
						if (execute_binary1(ref hold1, last_operation, value)
							!= RS274NGC_OK)
							return RS274NGC_ERROR;
						if (execute_binary2(ref hold2, waiting_operation, hold1)
							!= RS274NGC_OK)
							return RS274NGC_ERROR;
					}
					else if (execute_binary2(ref hold2, waiting_operation, value)
						!= RS274NGC_OK)
						return RS274NGC_ERROR;
					else {}
					return RS274NGC_OK;
				}
				else if (next_operation < LOGICAL_AND)  /* next operation is a bop1 */
				{
					if (last_operation >= LOGICAL_AND)
					{
						hold1 = value;
					}
					else if (execute_binary1(ref hold1, last_operation, value)
						!= RS274NGC_OK)
						return RS274NGC_ERROR;
				}
				else                            /* next operation is a bop2 */
				{
					if (last_operation < LOGICAL_AND)
					{
						if (execute_binary1(ref hold1, last_operation, value)
							!= RS274NGC_OK)
							return RS274NGC_ERROR;
						if (execute_binary2(ref hold2, waiting_operation, hold1)
							!= RS274NGC_OK)
							return RS274NGC_ERROR;
					}
					else
					{
						if (execute_binary2(ref hold2, waiting_operation, value)
							!= RS274NGC_OK)
							return RS274NGC_ERROR;
					}
					waiting_operation = next_operation;
				}
				last_operation = next_operation;
			}
		}

		/****************************************************************************/

		/* read_real_number

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. The first character is not "+", "-", "." or a digit.
		2. No digits are found after the first character and before the
			end of the line or the next character that cannot be part of a real.
		3. BUG - sscanf fails.

		Side effects:
		The number read from the line is put into what double_ptr points at.
		The counter is reset to point to the first character after the real.

		Called by:
		read_real_value

		This attempts to read a number out of the line, starting at the index
		given by the counter. It stops when the first character that cannot
		be part of the number is found.

		The first character may be a digit, "+", "-", or "."
		Every following character must be a digit or "." up to anything
		that is not a digit or "." (a second "." terminates reading).

		This function is not called if the first character is null, so it is
		not necessary to check that.

		The temporary insertion of a null character on the line is to avoid
		making a format string like "%3lf" which the LynxOS compiler cannot
		handle.

		*/

		int read_real_number( /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274/NGC code being processed */
			ref int counter,       /* pointer to a counter for position on the line  */
			out double double_ptr) /* pointer to double to be read                   */
		{
			int n;
			char c;
			int flag_point;  /* set to ON if decimal point found */
			int flag_digit; /* set to ON if digit found */

			double_ptr = 0.0;

			n = counter;
			flag_point = OFF;
			flag_digit = OFF;

			/* check first character */
			c = line[n];
			if (c == '+')
			{
				counter = (counter + 1); /* skip plus sign */
				n++;
			}
			else if (c == '-')
			{
				n++;
			}
			else if ((c != '.') && ((c < 48) || (c > 57)))
				return ErrorNumberOf("Bad number format");

			/* check out rest of characters (must be digit or decimal point) */
			for (; n < line.Length; n++)
			{
				c = line[n];
				if (( 47 < c) && ( c < 58))
				{
					flag_digit = ON;
				}
				else if (c == '.')
				{
					if (flag_point == OFF)
					{
						flag_point = ON;
					}
					else
						break;
				}
				else
					break;
			}

			if (flag_digit == OFF)
				return ErrorNumberOf("No digits found where real number should be");

			double_ptr = Double.Parse(line.Substring(counter, n - counter));
			counter = n;
			return RS274NGC_OK;

			//			if (sscanf(line + *counter, "%lf", double_ptr) == 0)
			//			{
			//				line[n] = c;
			//				throw new Exception("Sscanf failure in read_real_number");
			//			}
			//			else
			//			{
			//				line[n] = c;
			//				*counter = n;
			//				return RS274NGC_OK;
			//			}
		}

		/****************************************************************************/

		/* read_unary

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. read_operation_unary returns RS274NGC_ERROR,
		2. the name of the unary operation is not followed by a left bracket,
		3. read_real_expression returns RS274NGC_ERROR,
		4. read_atan or execute_unary returns RS274NGC_ERROR,

		Side effects:
		The value read from the line is put into what double_ptr points at.
		The counter is reset to point to the first character after the
		characters which make up the value.

		Called by:
		read_real_value

		This attempts to read the value of a unary operation out of the line,
		starting at the index given by the counter. The atan operation is
		handled specially because it is followed by two arguments.

		*/

		int read_unary(       /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274/NGC code being processed */
			ref int counter,       /* pointer to a counter for position on the line  */
			out double double_ptr, /* pointer to double to be read                   */
			ref double[] parameters) /* array of system parameters                     */
		{
			double_ptr = 0.0;
			int operation;

			if (read_operation_unary (line, ref counter, out operation) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			if (line[counter] != '[')
				return ErrorNumberOf("Left bracket missing after unary operation name");
			if (read_real_expression (line, ref counter, out double_ptr, ref parameters) == RS274NGC_ERROR)
				return RS274NGC_ERROR;

			if (operation == ATAN)
				return read_atan(line, ref counter, ref double_ptr, ref parameters);
			else
				return execute_unary(ref double_ptr, operation);
		}

		/****************************************************************************/

		/* utility_find_arc_length

		Returned Value: double (length of path between start and end points)

		Side effects: none

		Called by:
		convert_arc_xy
		convert_arc_yz
		convert_arc_zx

		This calculates the length of the path that will be made relative to
		the XYZ axes for a motion in which the X,Y,Z, motion is a circular or
		helical arc with its axis parallel to the Z-axis. If tool length
		compensation is on, this is the path of the tool tip; if off, the
		length of the path of the spindle tip.

		If the arc is helical, it is coincident with the hypotenuse of a right
		triangle wrapped around a cylinder. If the triangle is unwrapped, its
		base is [the radius of the cylinder times the number of radians in the
		helix] and its height is [z2 - z1], and the path length can be found
		by the Pythagorean theorem.

		This is written as though it is only for arcs whose axis is parallel to
		the Z-axis, but it will serve also for arcs whose axis is parallel
		to the X-axis or Y-axis, with suitable permutation of the arguments.

		*/

		double utility_find_arc_length( /* ARGUMENT VALUES      */
			double x1,       /* X-coordinate of start point        */
			double y1,       /* Y-coordinate of start point        */
			double z1,       /* Z-coordinate of start point        */
			double center_x, /* X-coordinate of arc center         */
			double center_y, /* Y-coordinate of arc center         */
			int turn,        /* no. of full or partial circles CCW */
			double x2,       /* X-coordinate of end point          */
			double y2,       /* Y-coordinate of end point          */
			double z2)       /* Z-coordinate of end point          */
		{
			double radius;
			double theta;  /* amount of turn of arc */

			if (turn == 0)
				return 0.0;
			radius = Hypot((center_x - x1), (center_y - y1));
			theta = utility_find_turn(x1, y1, center_x, center_y, turn, x2, y2);
			if (z2 == z1)
				return (radius * Math.Abs(theta));
			else
				return Hypot((radius * theta), (z2 - z1));
		}

		/****************************************************************************/

		/* utility_find_straight_length

		Returned Value: double (length of path between start and end points)

		Side effects: none

		Called by:
		convert_straight

		This calculates a number to use in feed rate calculations when inverse
		time feed mode is used, for a motion in which X,Y, and Z, each change
		linearly or not at all from their initial value to their end value.

		This is used when the feed_reference mode is CANON_XYZ, which is
		always in rs274kt.

		This is the length of the path relative to the XYZ axes from the first
		point to the second. The length is the simple Euclidean distance.

		*/

		double utility_find_straight_length( /* ARGUMENT VALUES  */
			double x2,    /* X-coordinate of end point              */
			double y2,    /* Y-coordinate of end point              */
			double z2,    /* Z-coordinate of end point              */
			double x1,    /* X-coordinate of start point            */
			double y1,    /* Y-coordinate of start point            */
			double z1)    /* Z-coordinate of start point            */
		{
			return Math.Sqrt(Math.Pow((x2 - x1),2) + Math.Pow((y2 - y1),2) + Math.Pow((z2 - z1),2));
		}

		/****************************************************************************/

		/* convert_arc_comp1

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. arc_data_comp_ijk or arc_data_comp_r returns RS274NGC_ERROR.
		2. The tool radius is not positive.

		Side effects:
		This executes an arc command at
		feed rate. It also updates the setting of the position of
		the tool point to the end point of the move.

		Called by: convert_arc.

		This function converts a helical or circular arc, generating only one
		arc. The axis must be parallel to the z-axis. This is called when
		cutter radius compensation is on and this is the first cut after the
		turning on.

		The arc which is generated is derived from second arc which passes
		through the programmed end point and is tangent to the cutter at its
		current location. The generated arc moves the tool so that it stays
		tangent to the second arc throughout the move.

		*/

		int convert_arc_comp1(   /* ARGUMENT VALUES                              */
			int move,               /* either G_2 (cw arc) or G_3 (ccw arc)         */
			ref block block,    /* pointer to a block of RS274/NGC instructions */
			ref setup settings, /* pointer to machine settings                  */
			double end_x,           /* x-value at end of arc                        */
			double end_y,           /* y-value at end of arc                        */
			double end_z)           /* z-value at end of arc                        */
		{
			double center_x;
			double center_y;
			int turn;     /* 1 for counterclockwise, -1 for clockwise */
			double gamma; /* direction of perpendicular to arc at end */
			double tolerance;
			double tool_radius;
			CANON_CUTTER_COMP side;
			int status;

			side = settings.cutter_radius_compensation;
			tool_radius =
				(settings.tool_table[settings.tool_table_index].diameter)/2.0;
			if (tool_radius <= 0.0)
				return ErrorNumberOf("Bad tool radius value with cutter radius comp");
			tolerance = (settings.length_units == CANON_UNITS.INCHES) ?
			TOLERANCE_INCH : TOLERANCE_MM;

			if (block.r_flag == ON)
			{
				status =
					arc_data_comp_r(move, side, tool_radius, settings.current_x,
					settings.current_y, end_x, end_y, block.r_number,
					out center_x, out center_y, out turn);
			}
			else
			{
				status =
					arc_data_comp_ijk(move, side, tool_radius, settings.current_x,
					settings.current_y, end_x, end_y,
					block.i_number, block.j_number,
					out center_x, out center_y, out turn, tolerance);
			}

			if (status == RS274NGC_ERROR)
				return RS274NGC_ERROR;

			gamma =
				(((side == CANON_CUTTER_COMP.LEFT) && (move == G_3)) ||
				((side == CANON_CUTTER_COMP.RIGHT) && (move == G_2))) ?
				Math.Atan2 ((center_y - end_y), (center_x - end_x)) :
				Math.Atan2 ((end_y - center_y), (end_x - center_x));

			settings.program_x = end_x;
			settings.program_y = end_y;
			end_x = (end_x + (tool_radius * Math.Cos(gamma)));
			end_y = (end_y + (tool_radius * Math.Sin(gamma)));
			
             ARC_FEED(end_x, end_y, center_x, center_y, turn, end_z);
			
            settings.current_x = end_x;
			settings.current_y = end_y;
			settings.current_z = end_z;

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_arc_comp2

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occurs, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. arc_data_ijk or arc_data_r returns RS274NGC_ERROR.
		2. A concave corner is found.
		3. The tool will not fit inside an arc.
		4. The tool radius is zero or negative

		Side effects:
		This executes an arc command feed rate. If needed, at also generates
		an arc to go around a convex corner. It also updates the setting of
		the position of the tool point to the end point of the move.

		Called by: convert_arc.

		This function converts a helical or circular arc. The axis must be
		parallel to the z-axis. This is called when cutter radius compensation
		is on and this is not the first cut after the turning on.

		If the Z-axis is moved in this block and an extra arc is required to
		go around a sharp corner, all the Z-axis motion occurs on the main arc
		and none on the extra arc.  An alternative might be to distribute the
		Z-axis motion over the extra arc and the main arc in proportion to
		their lengths.

		*/

		int convert_arc_comp2(   /* ARGUMENT VALUES                                */
			int move,               /* either G_2 (cw arc) or G_3 (ccw arc)           */
			ref block block,    /* pointer to a block of RS274/NGC instructions   */
			ref setup settings, /* pointer to machine settings                    */
			double end_x,           /* x-value at end of programmed (then actual) arc */
			double end_y,           /* y-value at end of programmed (then actual) arc */
			double end_z)           /* z-value at end of arc                          */
		{
			double center_x; /* center of arc */
			double center_y;
			double start_x;
			double start_y;
			int turn;     /* number of full or partial circles CCW */
			double theta; /* direction of tangent to last cut */
			double delta; /* direction of radius from start of arc to center of arc */
			double alpha; /* direction of tangent to start of arc */
			double beta;  /* angle between two tangents above */
			double gamma; /* direction of perpendicular to arc at end */
			double arc_radius;
			double tool_radius;
			/* small angle in radians for testing corners */
			double small = TOLERANCE_CONCAVE_CORNER;
			CANON_CUTTER_COMP side;
			int status;
			double tolerance;

			/* find basic arc data: center_x, center_y, and turn */

			start_x = settings.program_x;
			start_y = settings.program_y;
			tolerance = (settings.length_units == CANON_UNITS.INCHES) ?
			TOLERANCE_INCH : TOLERANCE_MM;

			if (block.r_flag == ON)
			{
				status = arc_data_r(move, start_x, start_y, end_x, end_y,
					block.r_number, out center_x, out center_y, out turn);
			}
			else
			{
				status =
					arc_data_ijk(move, start_x, start_y, end_x, end_y,
					block.i_number, block.j_number,
					out center_x, out center_y, out turn, tolerance);
			}

			if (status == RS274NGC_ERROR)
				return RS274NGC_ERROR;

			/* compute other data */
			side = settings.cutter_radius_compensation;
			tool_radius =
				(settings.tool_table[settings.tool_table_index].diameter)/2.0;
			if (tool_radius <= 0.0)
				return ErrorNumberOf("Bad tool radius value with cutter radius comp");
			arc_radius = Hypot((center_x - end_x), (center_y - end_y));
			theta =
				Math.Atan2(settings.current_y - start_y, settings.current_x - start_x);
			theta = (side == CANON_CUTTER_COMP.LEFT) ? (theta - PI2) : (theta + PI2);
			delta = Math.Atan2(center_y - start_y, center_x - start_x);
			alpha = (move == G_3) ? (delta - PI2) : (delta + PI2);
			beta = (side == CANON_CUTTER_COMP.LEFT) ? (theta - alpha) : (alpha - theta);
			beta = (beta > (1.5 * PI))  ? (beta - TWO_PI) :
				(beta < -PI2) ? (beta + TWO_PI) : beta;

			if (((side == CANON_CUTTER_COMP.LEFT)  && (move == G_3)) ||
				((side == CANON_CUTTER_COMP.RIGHT) && (move == G_2)))
			{
				gamma = Math.Atan2 ((center_y - end_y), (center_x - end_x));
				if (arc_radius <= tool_radius)
					return ErrorNumberOf("Tool radius not less than arc radius with cutter radius comp");
			}
			else
			{
				gamma = Math.Atan2 ((end_y - center_y), (end_x - center_x));
				delta = (delta + PI);
			}

			settings.program_x = end_x;
			settings.program_y = end_y;

			/* check if extra arc needed and insert if so */

			if ((beta < -small) || (beta > (PI + small)))
				return ErrorNumberOf("Concave corner with cutter radius comp");
			else if (beta > small) /* TWO ARCS NEEDED */
				 ARC_FEED((start_x + (tool_radius * Math.Cos(delta))),
					(start_y + (tool_radius * Math.Sin(delta))),
					start_x, start_y, (side == CANON_CUTTER_COMP.LEFT) ? -1 : 1,
					settings.current_z);

			end_x = (end_x + (tool_radius * Math.Cos(gamma))); /* end_x reset actual */
			end_y = (end_y + (tool_radius * Math.Sin(gamma))); /* end_y reset actual */

			/* insert main arc */
			 ARC_FEED(end_x, end_y, center_x, center_y, turn, end_z);

			settings.current_x = end_x;
			settings.current_y = end_y;
			settings.current_z = end_z;

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_arc_xy

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If arc_data_ijk or arc_data_r returns RS274NGC_ERROR, this
		returns RS274NGC_ERROR. Otherwise, it returns RS274NGC_OK.

		Side effects:
		This executes an arc command at feed rate. It also updates the
		setting of the position of the tool point to the end point of the move.
		If inverse time feed rate is in effect, it also resets the feed rate.

		Called by: convert_arc.

		This converts a helical or circular arc. The axis must be parallel to
		the z-axis.

		*/

		int convert_arc_xy(      /* ARGUMENT VALUES                          */
			int move,               /* either G_2 (cw arc) or G_3 (ccw arc)     */
			ref block block,    /* pointer to a block of RS274 instructions */
			ref setup settings, /* pointer to machine settings              */
			double end_x,           /* x-value at end of arc                    */
			double end_y,           /* y-value at end of arc                    */
			double end_z)           /* z-value at end of arc                    */
		{
			double center_x;
			double center_y;
			double length;
			double rate;
			int turn;
			int status;
			double tolerance;

			tolerance = (settings.length_units == CANON_UNITS.INCHES) ?
			TOLERANCE_INCH : TOLERANCE_MM;

			if (block.r_flag == ON)
			{
				status =
					arc_data_r(move, settings.current_x, settings.current_y, end_x,
					end_y, block.r_number, out center_x, out center_y, out turn);
			}
			else
			{
				status =
					arc_data_ijk(move, settings.current_x, settings.current_y,
					end_x, end_y, block.i_number, block.j_number,
					out center_x, out center_y, out turn, tolerance);
			}

			if (status == RS274NGC_ERROR)
				return RS274NGC_ERROR;

			if (settings.feed_mode == INVERSE_TIME)
			{
				length = utility_find_arc_length
					(settings.current_x, settings.current_y, settings.current_z,
					center_x, center_y, turn, end_x, end_y, end_z);
				rate = Math.Max(0.1, (length * block.f_number));
				 SET_FEED_RATE (rate);
				settings.feed_rate = rate;
			}

			 ARC_FEED(end_x, end_y, center_x, center_y, turn, end_z);
			settings.current_x = end_x;
			settings.current_y = end_y;
			settings.current_z = end_z;
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_arc_yz

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If arc_data_ijk or arc_data_r returns RS274NGC_ERROR, this
		returns RS274NGC_ERROR. Otherwise, it returns RS274NGC_OK.

		Side effects:
		This executes an arc command at feed rate. It also updates the
		setting of the position of the tool point to the end point of the move.
		If inverse time feed rate is in effect, it also resets the feed rate.

		Called by: convert_arc.

		This converts a helical or circular arc. The axis must be parallel to
		the x-axis.

		*/

		int convert_arc_yz(      /* ARGUMENT VALUES                          */
			int move,               /* either G_2 (cw arc) or G_3 (ccw arc)     */
			ref block block,    /* pointer to a block of RS274 instructions */
			ref setup settings, /* pointer to machine settings              */
			double end_y,           /* y-value at end of arc                    */
			double end_z,           /* z-value at end of arc                    */
			double end_x)           /* x-value at end of arc                    */
		{
			double center_y;
			double center_z;
			double length;
			double rate;
			int turn;
			int status;
			double tolerance;

			tolerance = (settings.length_units == CANON_UNITS.INCHES) ?
			TOLERANCE_INCH : TOLERANCE_MM;

			if (block.r_flag == ON)
			{
				status =
					arc_data_r(move, settings.current_y, settings.current_z, end_y,
					end_z, block.r_number, out center_y, out center_z, out turn);
			}
			else
			{
				status =
					arc_data_ijk(move, settings.current_y, settings.current_z,
					end_y, end_z, block.j_number, block.k_number,
					out center_y, out center_z, out turn, tolerance);
			}

			if (status == RS274NGC_ERROR)
				return RS274NGC_ERROR;

			if (settings.feed_mode == INVERSE_TIME)
			{
				length = utility_find_arc_length
					(settings.current_y, settings.current_z, settings.current_x,
					center_y, center_z, turn, end_y, end_z, end_x);
				rate = Math.Max(0.1, (length * block.f_number));
				 SET_FEED_RATE (rate);
				settings.feed_rate = rate;
			}

			 ARC_FEED(end_y, end_z, center_y, center_z, turn, end_x);
			settings.current_y = end_y;
			settings.current_z = end_z;
			settings.current_x = end_x;

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_arc_zx

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If arc_data_ijk or arc_data_r returns RS274NGC_ERROR, this
		returns RS274NGC_ERROR. Otherwise, it returns RS274NGC_OK.

		Side effects:
		This executes an arc command at feed rate. It also updates the
		setting of the position of the tool point to the end point of the move.
		If inverse time feed rate is in effect, it also resets the feed rate.

		Called by: convert_arc.

		This converts a helical or circular arc. The axis must be parallel to
		the y-axis.

		*/

		int convert_arc_zx(      /* ARGUMENT VALUES                          */
			int move,               /* either G_2 (cw arc) or G_3 (ccw arc)     */
			ref block block,    /* pointer to a block of RS274 instructions */
			ref setup settings, /* pointer to machine settings              */
			double end_z,           /* z-value at end of arc                    */
			double end_x,           /* x-value at end of arc                    */
			double end_y)           /* y-value at end of arc                    */
		{
			double center_z;
			double center_x;
			double length;
			double rate;
			int turn;
			int status;
			double tolerance;

			tolerance = (settings.length_units == CANON_UNITS.INCHES) ?
			TOLERANCE_INCH : TOLERANCE_MM;

			if (block.r_flag == ON)
			{
				status =
					arc_data_r(move, settings.current_z, settings.current_x, 
					end_z, end_x, block.r_number, 
					out center_z, out center_x, out turn);
			}
			else
			{
				status =
					arc_data_ijk(move, settings.current_z, settings.current_x,
					end_z, end_x, block.k_number, block.i_number,
					out center_z, out center_x, out turn, tolerance);
			}

			if (status == RS274NGC_ERROR)
				return RS274NGC_ERROR;

			if (settings.feed_mode == INVERSE_TIME)
			{
				length = utility_find_arc_length
					(settings.current_z, settings.current_x, settings.current_y,
					center_z, center_x, turn, end_z, end_x, end_y);
				rate = Math.Max(0.1, (length * block.f_number));
				 SET_FEED_RATE (rate);
				settings.feed_rate = rate;
			}

			 ARC_FEED(end_z, end_x, center_z, center_x, turn, end_y);
			settings.current_z = end_z;
			settings.current_x = end_x;
			settings.current_y = end_y;

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_cycle_g81

		Returned Value: int (RS274NGC_OK)

		Side effects:
		A number of moves are made as described below.

		Called by: convert_cycle

		This implements the following cycle, which is usually drilling:
		1. Move the z-axis only at the current feed rate to the specified bottom_z.
		2. Retract the z-axis at traverse rate to clear_z.

		See [NCMS, page 99].

		convert_cycle has positioned the tool at (x, y, r) when this starts.

		*/

		int convert_cycle_g81( /* ARGUMENT VALUES                  */
			double x,             /* x-value where cycle is executed  */
			double y,             /* y-value where cycle is executed  */
			double clear_z,       /* z-value of clearance plane       */
			double bottom_z)      /* value of z at bottom of cycle    */
		{

			 STRAIGHT_FEED(x, y, bottom_z);
			 STRAIGHT_TRAVERSE(x, y, clear_z);

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_cycle_g82

		Returned Value: int (RS274NGC_OK)

		Side effects:
		A number of moves are made as described below.

		Called by: convert_cycle

		This implements the following cycle, which is usually drilling:
		1. Move the z_axis only at the current feed rate to the specified z-value.
		2. Dwell for the given number of seconds.
		3. Retract the z-axis at traverse rate to the clear_z.

		convert_cycle has positioned the tool at (x, y, r) when this starts.

		*/

		int convert_cycle_g82( /* ARGUMENT VALUES                  */
			double x,             /* x-value where cycle is executed  */
			double y,             /* y-value where cycle is executed  */
			double clear_z,       /* z-value of clearance plane       */
			double bottom_z,      /* value of z at bottom of cycle    */
			double dwell)         /* dwell time                       */
		{

			 STRAIGHT_FEED(x, y, bottom_z);
			 DWELL(dwell);
			 STRAIGHT_TRAVERSE(x, y, clear_z);

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_cycle_g83

		Returned Value: int (RS274NGC_OK)

		Side effects:
		A number of moves are made as described below.

		Called by: convert_cycle

		This implements the following cycle, which is usually
		deep drilling or milling with chipbreaking:
		1. Move the z-axis only at the current feed rate downward by delta or
		to the specified bottom_z, whichever is less deep.
		2. Dwell for 0.25 second.
		3. Rapid back out to the clear_z.
		4. Rapid back down to the current hole bottom, backed off a bit.
		5. Repeat steps 1 and 2 until the specified bottom_z is reached.
		6. Retract the z-axis at traverse rate to clear_z.

		convert_cycle has positioned the tool at (x, y, r) when this starts.

		The dwell causes any long stringers (which are common when drilling
		in aluminum) to be cut off, and pulls out the chips.

		*/

		const double G83_RAPID_DELTA = 0.010;   /* how far above hole bottom for rapid
										return, in inches */
		int convert_cycle_g83( /* ARGUMENT VALUES                  */
			double x,             /* x-value where cycle is executed  */
			double y,             /* y-value where cycle is executed  */
			double r,             /* initial z-value                  */
			double clear_z,       /* z-value of clearance plane       */
			double bottom_z,      /* value of z at bottom of cycle    */
			double delta)         /* size of z-axis feed increment    */
		{
			double current_depth;
			double rapid_delta;

			rapid_delta = G83_RAPID_DELTA;
			if (_interpreter_settings.length_units == CANON_UNITS.MM)
				rapid_delta *= 25.4;

			for (current_depth = (r - delta);
				current_depth > bottom_z;
				current_depth = (current_depth - delta))
			{
				 STRAIGHT_FEED(x, y, current_depth);
				 DWELL(0.25);
				 STRAIGHT_TRAVERSE(x, y, clear_z);
				 STRAIGHT_TRAVERSE(x, y, current_depth + rapid_delta);
			}
			 STRAIGHT_FEED(x, y, bottom_z);
			 DWELL(0.25);
			 STRAIGHT_TRAVERSE(x, y, clear_z);

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_cycle_g84

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If the spindle is not turning clockwise, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.

		Side effects:
		A number of moves are made as described below. This is for right-hand
		tapping.

		Called by: convert_cycle

		This implements the following cycle, which is right-hand tapping:
		1. Start speed-feed synchronization.
		2. Move the z-axis only at the current feed rate to the specified bottom_z.
		3. Stop the spindle.
		4. Start the spindle counterclockwise.
		5. Retract the z-axis at current feed rate to clear_z.
		6. If speed-feed synch was not on before the cycle started, stop it.
		7. Stop the spindle.
		8. Start the spindle clockwise.

		convert_cycle has positioned the tool at (x, y, r) when this starts.
		The direction argument must be clockwise.

		*/

		int convert_cycle_g84(       /* ARGUMENT VALUES                     */
			double x,                   /* x-value where cycle is executed     */
			double y,                   /* y-value where cycle is executed     */
			double clear_z,             /* z-value of clearance plane          */
			double bottom_z,            /* value of z at bottom of cycle       */
			CANON_DIRECTION direction,  /* direction spindle turning at outset */
			CANON_SPEED_FEED_MODE mode) /* the speed-feed mode at outset       */
		{

			if (direction != CANON_DIRECTION.CLOCKWISE)
				return ErrorNumberOf("Spindle not turning clockwise in G84 canned cycle");
			 START_SPEED_FEED_SYNCH();
			 STRAIGHT_FEED(x, y, bottom_z);
			 STOP_SPINDLE_TURNING();
			 START_SPINDLE_COUNTERCLOCKWISE();
			 STRAIGHT_FEED(x, y, clear_z);
			if (mode != CANON_SPEED_FEED_MODE.SYNCHED)
				 STOP_SPEED_FEED_SYNCH();
			 STOP_SPINDLE_TURNING();
			 START_SPINDLE_CLOCKWISE();

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_cycle_g85

		Returned Value: int (RS274NGC_OK)

		Side effects:
		A number of moves are made as described below.

		Called by: convert_cycle

		This implements the following cycle, which is usually boring or reaming:
		1. Move the z-axis only at the current feed rate to the specified z-value.
		2. Retract the z-axis at the current feed rate to clear_z.

		convert_cycle has positioned the tool at (x, y, r) when this starts.

		*/

		int convert_cycle_g85( /* ARGUMENT VALUES                  */
			double x,             /* x-value where cycle is executed  */
			double y,             /* y-value where cycle is executed  */
			double clear_z,       /* z-value of clearance plane       */
			double bottom_z)      /* value of z at bottom of cycle    */
		{

			 STRAIGHT_FEED(x, y, bottom_z);
			 STRAIGHT_FEED(x, y, clear_z);

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_cycle_g86

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If the spindle is not turning clockwise or counterclockwise,
		this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.

		Side effects:
		A number of moves are made as described below.

		Called by: convert_cycle

		This implements the following cycle, which is usually boring:
		1. Move the z-axis only at the current feed rate to bottom_z.
		2. Dwell for the given number of seconds.
		3. Stop the spindle turning.
		4. Retract the z-axis at traverse rate to clear_z.
		5. Restart the spindle in the direction it was going.

		convert_cycle has positioned the tool at (x, y, r) when this starts.

		*/

		int convert_cycle_g86(      /* ARGUMENT VALUES                     */
			double x,                  /* x-value where cycle is executed     */
			double y,                  /* y-value where cycle is executed     */
			double clear_z,            /* z-value of clearance plane          */
			double bottom_z,           /* value of z at bottom of cycle       */
			double dwell,              /* dwell time                          */
			CANON_DIRECTION direction) /* direction spindle turning at outset */
		{

			if ((direction != CANON_DIRECTION.CLOCKWISE) &&
				(direction != CANON_DIRECTION.COUNTERCLOCKWISE))
				return ErrorNumberOf("Spindle not turning in G86 canned cycle");

			 STRAIGHT_FEED(x, y, bottom_z);
			 DWELL(dwell);
			 STOP_SPINDLE_TURNING();
			 STRAIGHT_TRAVERSE(x, y, clear_z);
			if (direction == CANON_DIRECTION.CLOCKWISE)
				 START_SPINDLE_CLOCKWISE();
			else
				 START_SPINDLE_COUNTERCLOCKWISE();

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_cycle_g87

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If the spindle is not turning clockwise or counterclockwise,
		this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.

		Side effects:
		A number of moves are made as described below. This cycle is a
		modified version of [Monarch, page 5-24] since [NCMS, pages 98 - 100]
		gives no clue as to what the cycle is supposed to do.

		Called by: convert_cycle

		This implements the following cycle, which is usually back
		boring.  The situation is that you have a through hole and you want to
		counterbore the bottom of hole. To to this you put an L-shaped tool in
		the spindle with a cutting surface on the UPPER side of its base. You
		stick it carefully through the hole when it is not spinning and is
		oriented so it fits through the hole, then you move it so the stem of
		the L is on the axis of the hole, start the spindle, and feed the tool
		upward to make the counterbore. Then you get the tool out of the hole.

		1. Move at traverse rate parallel to the XY-plane to the point
		with x-value offset_x and y-value offset_y.
		2. Stop the spindle in a specific orientation.
		3. Move the z-axis only at traverse rate downward to the bottom_z.
		4. Move at traverse rate parallel to the XY-plane to the x,y location.
		5. Start the spindle in the direction it was going before.
		6. Move the z-axis only at the given feed rate upward to the middle_z.
		7. Move the z-axis only at the given feed rate back down to bottom_z.
		8. Stop the spindle in the same orientation as before.
		9. Move at traverse rate parallel to the XY-plane to the point
		with x-value offset_x and y-value offset_y.
		10. Move the z-axis only at traverse rate to the clear z value.
		11. Move at traverse rate parallel to the XY-plane to the specified x,y
			location.
		12. Restart the spindle in the direction it was going before.

		convert_cycle has positioned the tool at (x, y, r) before this starts.

		It might be useful to add a check that clear_z > middle_z > bottom_z.
		Without the check, however, this can be used to counterbore a hole in
		material that can only be accessed through a hole in material above it.

		*/

		int convert_cycle_g87(       /* ARGUMENT VALUES                     */
			double x,                   /* x-value where cycle is executed     */
			double offset_x,            /* x-axis offset position              */
			double y,                   /* y-value where cycle is executed     */
			double offset_y,            /* y-axis offset position              */
			double r,                   /* z_value of r_plane                  */
			double clear_z,             /* z-value of clearance plane          */
			double middle_z,            /* z-value of top of back bore         */
			double bottom_z,            /* value of z at bottom of cycle       */
			CANON_DIRECTION direction)  /* direction spindle turning at outset */
		{

			if ((direction != CANON_DIRECTION.CLOCKWISE) &&
				(direction != CANON_DIRECTION.COUNTERCLOCKWISE))
				return ErrorNumberOf("Spindle not turning in G87 canned cycle");

			 STRAIGHT_TRAVERSE(offset_x, offset_y, r);
			 STOP_SPINDLE_TURNING();
			 ORIENT_SPINDLE(0.0, direction);
			 STRAIGHT_TRAVERSE(offset_x, offset_y, bottom_z);
			 STRAIGHT_TRAVERSE(x, y, bottom_z);
			if (direction == CANON_DIRECTION.CLOCKWISE)
				 START_SPINDLE_CLOCKWISE();
			else
				 START_SPINDLE_COUNTERCLOCKWISE();
			 STRAIGHT_FEED(x, y, middle_z);
			 STRAIGHT_FEED(x, y, bottom_z);
			 STOP_SPINDLE_TURNING();
			 ORIENT_SPINDLE(0.0, direction);
			 STRAIGHT_TRAVERSE(offset_x, offset_y, bottom_z);
			 STRAIGHT_TRAVERSE(offset_x, offset_y, clear_z);
			 STRAIGHT_TRAVERSE(x, y, clear_z);
			if (direction == CANON_DIRECTION.CLOCKWISE)
				 START_SPINDLE_CLOCKWISE();
			else
				 START_SPINDLE_COUNTERCLOCKWISE();

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_cycle_g88

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If the spindle is not turning clockwise or counterclockwise, this
		returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.

		Side effects:
		A number of moves are made as described below.

		Called by: convert_cycle

		This implements the following cycle, which is usually boring:
		1. Move the z-axis only at the current feed rate to the specified z-value.
		2. Dwell for the given number of seconds.
		3. Stop the spindle turning.
		4. Stop the program so the operator can retract the spindle manually.
		5. Restart the spindle.

		convert_cycle has positioned the tool at (x, y, r) when this starts.

		*/

		int convert_cycle_g88(      /* ARGUMENT VALUES                     */
			double x,                  /* x-value where cycle is executed     */
			double y,                  /* y-value where cycle is executed     */
			double bottom_z,           /* value of z at bottom of cycle       */
			double dwell,              /* dwell time                          */
			CANON_DIRECTION direction) /* direction spindle turning at outset */
		{
			if ((direction != CANON_DIRECTION.CLOCKWISE) &&
				(direction != CANON_DIRECTION.COUNTERCLOCKWISE))
				return ErrorNumberOf("Spindle not turning in G88 canned cycle");

			 STRAIGHT_FEED(x, y, bottom_z);
			 DWELL(dwell);
			 STOP_SPINDLE_TURNING();
			 PROGRAM_STOP(); /* operator retracts the spindle here */
			if (direction == CANON_DIRECTION.CLOCKWISE)
				 START_SPINDLE_CLOCKWISE();
			else
				 START_SPINDLE_COUNTERCLOCKWISE();

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_cycle_g89

		Returned Value: int (RS274NGC_OK)

		Side effects:
		A number of moves are made as described below.

		Called by: convert_cycle

		This implements the following cycle, which is intended for boring:
		1. Move the z-axis only at the current feed rate to the specified z-value.
		2. Dwell for the given number of seconds.
		3. Retract the z-axis at the current feed rate to clear_z.

		*/

		int convert_cycle_g89(      /* ARGUMENT VALUES                     */
			double x,                  /* x-value where cycle is executed     */
			double y,                  /* y-value where cycle is executed     */
			double clear_z,            /* z-value of clearance plane          */
			double bottom_z,           /* value of z at bottom of cycle       */
			double dwell)              /* dwell time                          */
		{
			 STRAIGHT_FEED(x, y, bottom_z);
			 DWELL(dwell);
			 STRAIGHT_FEED(x, y, clear_z);

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_straight_comp1

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The side is not CANON_CUTTER_COMP.RIGHT or CANON_CUTTER_COMP.LEFT.
		2. The destination tangent point is not more than a tool radius
			away (indicating gouging).
		3. The tool radius is less than or equal to zero.

		Side effects:
		This executes a straight move command at cutting feed rate.
		It also updates the setting of the position of the tool point
		to the end point of the move and updates the programmed point.

		Called by: convert_straight.

		This is called if cutter radius compensation is on and
		settings.program_x is UNKNOWN, indicating that this is the first move
		after cutter radius compensation is turned on.

		The algorithm used here for determining the path is to draw a straight
		line from the destination point which is tangent to a circle whose
		center is at the current point and whose radius is the radius of the
		cutter. The destination point of the cutter tip is then found as the
		center of a circle of the same radius tangent to the tangent line at
		the destination point.

		*/

		int convert_straight_comp1( /* ARGUMENT VALUES                         */
			ref setup settings,    /* pointer to machine settings             */
			double px,                 /* X coordinate of end point               */
			double py,                 /* Y coordinate of end point               */
			double end_z)              /* Z coordinate of end point               */
		{
			double radius;
			double cx, cy;
			double distance;
			double theta;
			double alpha;
			CANON_CUTTER_COMP side;

			side = settings.cutter_radius_compensation;
			cx = settings.current_x;
			cy = settings.current_y;

			radius =
				(settings.tool_table[settings.tool_table_index].diameter)/2.0;
			if (radius <= 0.0)
				return ErrorNumberOf("Bad tool radius value with cutter radius comp");
			distance = Hypot((px - cx), (py -cy));

			if ((side != CANON_CUTTER_COMP.LEFT) && (side != CANON_CUTTER_COMP.RIGHT))
				throw new Exception("Side fails to be right or left in convert_straight_comp1");
			else if (distance <= radius)
				return ErrorNumberOf("Cutter gouging with cutter radius comp");

			theta = Math.Acos(radius/distance);
			alpha = (side == CANON_CUTTER_COMP.LEFT) ? (Math.Atan2((cy - py), (cx - px)) - theta) :
				(Math.Atan2((cy - py), (cx - px)) + theta);
			cx = (px + (radius * Math.Cos(alpha))); /* reset to end location */
			cy = (py + (radius * Math.Sin(alpha)));
			 STRAIGHT_FEED(cx, cy, end_z);
			settings.current_x = cx;
			settings.current_y = cy;
			settings.program_x = px;
			settings.program_y = py;
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_straight_comp2

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The compensation side is not CANON_CUTTER_COMP.RIGHT or CANON_CUTTER_COMP.LEFT.
		2. A concave corner is found.
		3. The tool radius is less than or equal to zero.

		Side effects:
		This executes a straight move command at cutting feed rate.
		It also generates an arc cut to go around a corner, if necessary.
		It also updates the setting of the position of the tool point to
		the end point of the move and updates the programmed point.

		Called by: convert_straight.

		This is called if cutter radius compensation is on and
		settings.program_x is not UNKNOWN, indicating that this is not the
		first move after cutter radius compensation is turned on.

		The algorithm used here is:
		1. Determine the direction of the last motion. This is done by finding
		the direction of the line from the last programmed point to the
		current tool tip location. This line is a radius of the tool and is
		perpendicular to the direction of motion since the cutter is tangent
		to that direction.
		2. Determine the direction of the programmed motion.
		3. If there is a convex corner, insert an arc to go around the corner.
		4. Find the destination point for the tool tip. The tool will be
		tangent to the line from the last programmed point to the present
		programmed point at the present programmed point.
		5. Go in a straight line from the current tool tip location to the
		destination tool tip location.

		This uses an angle tolerance of 0.001 radian to determine if:
		1) an illegal concave corner exists (tool will not fit into corner),
		2) no arc is required to go around the corner (i.e. the current line
		is in the same direction as the end of the previous move), or
		3) an arc is required to go around a convex corner and start off in
		a new direction.

		If the B-axis is moved in this block and an extra arc is required to go
		around a sharp corner, all the B-axis motion occurs on the arc.
		An alternative might be to distribute the B-axis motion over the arc
		and the straight move in proportion to their lengths.

		*/

		int convert_straight_comp2( /* ARGUMENT VALUES                         */
			ref setup settings,    /* pointer to machine settings             */
			double program_end_x,      /* X coordinate of programmed end point    */
			double program_end_y,      /* Y coordinate of programmed end point    */
			double end_z)              /* Z coordinate of end point               */
		{
			double radius;
			double cx, cy; /* actual end point */
			double dx, dy; /* end of added arc, if needed */
			double start_x, start_y; /* programmed beginning point */
			double theta;
			double alpha;
			double beta;
			double gamma;
			/* small angle in radians for testing corners */
			double small = TOLERANCE_CONCAVE_CORNER;
			CANON_CUTTER_COMP side;

			side = settings.cutter_radius_compensation;
			start_x = settings.program_x;
			start_y = settings.program_y;
			if ((start_x == program_end_x) && (start_y == program_end_y))
			{
				 STRAIGHT_FEED(settings.current_x, settings.current_y, end_z);
				return RS274NGC_OK;
			}
			radius =
				(settings.tool_table[settings.tool_table_index].diameter)/2.0;
			if (radius <= 0.0)
				return ErrorNumberOf("Bad tool radius value with cutter radius comp");
			theta = Math.Atan2(settings.current_y - start_y,
				settings.current_x - start_x);
			alpha =
				Math.Atan2(program_end_y - start_y, program_end_x - start_x);

			if (side == CANON_CUTTER_COMP.LEFT)
			{
				if (theta < alpha)
					theta = (theta + TWO_PI);
				beta = ((theta - alpha) - PI2);
				gamma = PI2;
			}
			else if (side == CANON_CUTTER_COMP.RIGHT)
			{
				if (alpha < theta)
					alpha = (alpha + TWO_PI);
				beta = ((alpha - theta) - PI2);
				gamma = -PI2;
			}
			else
				throw new Exception("Side fails to be right or left in convert_straight_comp2");
			cx = (program_end_x + (radius * Math.Cos(alpha + gamma)));
			cy = (program_end_y + (radius * Math.Sin(alpha + gamma)));
			dx = (start_x + (radius * Math.Cos(alpha + gamma)));
			dy = (start_y + (radius * Math.Sin(alpha + gamma)));

			if ((beta < -small) || (beta > (PI + small)))
				return ErrorNumberOf("Cannot have concave corner with cutter radius comp");
			else if (beta > small) /* ARC NEEDED */
			{
				 ARC_FEED(dx, dy, start_x, start_y, (side == CANON_CUTTER_COMP.LEFT) ? -1 : 1, end_z);
				 STRAIGHT_FEED(cx, cy, end_z);
			}
			else  STRAIGHT_FEED(cx, cy, end_z);

			settings.current_x = cx;
			settings.current_y = cy;
			settings.program_x = program_end_x;
			settings.program_y = program_end_y;
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* read_integer_unsigned

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, RS274NGC_ERROR is returned.
		Otherwise, RS274NGC_OK is returned.
		1. The first character is not a digit.
		2. BUG - sscanf fails.

		Side effects:
		The number read from the line is put into what integer_ptr points at.

		Called by:
		read_line_number

		This reads an explicit unsigned (positive) integer from a string,
		starting from the position given by *counter. It expects to find one
		or more digits. Any character other than a digit terminates reading
		the integer. Note that if the first character is a sign (+ or -),
		an error will be reported (since a sign is not a digit).

		*/

		int read_integer_unsigned(   /* ARGUMENT VALUES                        */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			out int integer_ptr)   /* pointer to the value being read               */
		{
			int n;
			char c;

			integer_ptr = 0;

			for (n = counter; ; n++)
			{
				c = line[n];
				if ((c < 48) || (c > 57))
					break;
			}
			if (n == counter)
				return ErrorNumberOf("Bad format unsigned integer");

			integer_ptr = int.Parse(line.Substring(counter, n-counter));
			counter = n;
			return RS274NGC_OK;

			//			if (sscanf(line + *counter, "%d", integer_ptr) == 0)
			//				throw new Exception("Sscanf failure in read_integer_unsigned");
			//			else
			//			{
			//				*counter = n;
			//				return RS274NGC_OK;
			//			}
		}

		/****************************************************************************/

		/* read_real_value

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If no characters are found before the end of the line or
		if a call to read_real_expression, read_parameter, read_unary,
		or read_real_number returns RS274NGC_ERROR, this returns RS274NGC_ERROR.
		Otherwise, this returns RS274NGC_OK.

		Side effects:
		The value read from the line is put into what double_ptr points at.
		The counter is reset to point to the first character after the
		characters which make up the value.

		Called by:
		read_f
		read_g
		read_i
		read_integer_value
		read_j
		read_k
		read_parameter
		read_parameter_setting
		read_q
		read_r
		read_real_expression
		read_spindle_speed
		read_x
		read_y
		read_z

		This attempts to read a real value out of the line, starting at the
		index given by the counter. The value may be a number, a parameter
		value, a unary function, or an expression. It calls one of four
		other readers, depending upon the first character.


		*/

		int read_real_value(  /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274/NGC code being processed */
			ref int counter,       /* pointer to a counter for position on the line  */
			out double double_ptr, /* pointer to double to be read                   */
			ref double[] parameters) /* array of system parameters                     */
		{
			char c;
			double_ptr = 0.0;

			c = line[counter];
			if (c == 0)
				return ErrorNumberOf("No characters found in reading real value");
			else if (c == '[')
				return read_real_expression (line, ref counter, out double_ptr, ref parameters);
			else if (c == '#')
				return read_parameter(line, ref counter, out double_ptr, ref parameters);
			else if ((c >= 'a') && (c <= 'z'))
				return read_unary(line, ref counter, out double_ptr, ref parameters);
			else
				return read_real_number(line, ref counter, out double_ptr);
		}

		/****************************************************************************/

		/* utility_find_ends

		Returned Value: int (RS274NGC_OK)

		Side effects:
		The values of px, py, and pz are set.

		Called by:
		convert_arc
		convert_straight

		This finds the end point of a straight line or arc.

		*/

		int utility_find_ends(   /* ARGUMENT VALUES                              */
			ref block block,    /* pointer to a block of RS274/NGC instructions */
			ref setup settings, /* pointer to machine settings                  */
			out double px,            /* pointer to end_x                             */
			out double py,            /* pointer to end_y                             */
			out double pz)            /* pointer to end_z                             */
		{
			DISTANCE_MODE mode;
			bool middle;
			bool comp;

			mode = settings.distance_mode;
			middle = (settings.program_x != UNKNOWN);
			comp = (settings.cutter_radius_compensation !=  CANON_CUTTER_COMP.OFF);

			if (block.g_modes[4] == G_53) /* mode is absolute in this case */
			{
				px = (block.x_flag == ON) ? (block.x_number -
					(settings.origin_offset_x + settings.axis_offset_x)) :
					settings.current_x;
				py = (block.y_flag == ON) ? (block.y_number -
					(settings.origin_offset_y + settings.axis_offset_y)) :
					settings.current_y;
				pz = (block.z_flag == ON) ? (block.z_number -
					(settings.tool_length_offset + settings.origin_offset_z
					+ settings.axis_offset_z)) : settings.current_z;
			}
			else if (mode == DISTANCE_MODE.ABSOLUTE)
			{
				px = (block.x_flag == ON) ? block.x_number     :
					(comp && middle)     ? settings.program_x :
					settings.current_x ;

				py = (block.y_flag == ON) ? block.y_number     :
					(comp && middle)     ? settings.program_y :
					settings.current_y ;

				pz = (block.z_flag == ON) ? block.z_number     :
					settings.current_z ;
			}
			else
			{
				px = (block.x_flag == ON)
					? ((comp && middle) ? (block.x_number + settings.program_x)
					: (block.x_number + settings.current_x))
					: ((comp && middle) ? settings.program_x
					: settings.current_x);

				py = (block.y_flag == ON)
					? ((comp && middle) ? (block.y_number + settings.program_y)
					: (block.y_number + settings.current_y))
					: ((comp && middle) ? settings.program_y
					: settings.current_y);

				pz = (block.z_flag == ON)
					? (settings.current_z + block.z_number)
					: settings.current_z;
			}

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_arc

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		This returns RS274NGC_OK unless one of the following errors occurs,
		in which case it returns RS274NGC_ERROR.
		1. The block has neither an r value nor any i,j,k values.
		2. The block has both an r value and one or more i,j,k values.
		3. In the ijk format the XY-plane is selected and either:
			a. the block has a k value, or
			b. the block has no i value and no j value.
		4. In the ijk format the YZ-plane is selected and either:
			a. the block has an i value, or
			b. the block has no j value and no k value.
		5. In the ijk format the XZ-plane is selected and either:
			a. the block has a j value, or
			b. the block has no i value and no k value.
		6. In either format any of the following occurs.
			a. The XY-plane is selected and the block has no x or y value.
			b. The XY-plane is selected, cutter radius compensation is on,
				and inverse time feed is used.
			c. The YZ-plane is selected and the block has no y or z value.
			d. The ZX-plane is selected and the block has no z or x value.
		7. BUG - The selected plane is an unknown plane.
		8. One of the subordinate arc making functions is called and returns
			RS274NGC_ERROR.
		9. The feed rate is zero.

		Side effects:
		This generates and executes an arc command at feed rate
		(and, possibly a second arc command). It also updates the setting
		of the position of the tool point to the end point of the move.

		Called by: convert_motion.

		This converts a helical or circular arc.  The function calls one of
		five convert_arc_XXX functions (three for for the xy plane and one
		each for the yz and zx plane) according to the currently selected
		plane and whether cutter radius compensation is in effect.

		If the ijk format is used, at least one of the offsets in the current
		plane must be given in the block; it is common but not required to
		give both offsets. The offsets are always incremental [NCMS, page 21].

		*/

		int convert_arc(         /* ARGUMENT VALUES                              */
			int move,               /* either G_2 (cw arc) or G_3 (ccw arc)         */
			ref block block,    /* pointer to a block of RS274/NGC instructions */
			ref setup settings) /* pointer to machine settings                  */
		{
			int status;
			DISTANCE_MODE mode;
			bool first;
			bool ijk_flag;
			double end_x;
			double end_y;
			double end_z;

			mode = settings.distance_mode;
			ijk_flag = ((block.i_flag==ON || block.j_flag==ON) || block.k_flag==ON);
			first = (settings.program_x == UNKNOWN);

			if ((block.r_flag != ON) && (!ijk_flag))
				return ErrorNumberOf("R, I, J, and K words all missing for arc");
			else if ((block.r_flag == ON) && (ijk_flag))
				return ErrorNumberOf("Mixed radius-ijk format for arc");
			else if (settings.feed_rate == 0.0)
				return ErrorNumberOf("Cannot make arc with zero feed rate");
			else if (ijk_flag)
			{
				if (settings.plane == CANON_PLANE.XY)
				{
					if (block.k_flag == ON)
						return ErrorNumberOf("K word given for arc in XY-plane");
					else if (block.i_flag == OFF) /* i or j flag on to get here */
						block.i_number = 0.0;
					else if (block.j_flag == OFF)
						block.j_number = 0.0;
				}
				else if (settings.plane == CANON_PLANE.YZ)
				{
					if (block.i_flag == ON)
						return ErrorNumberOf("I word given for arc in YZ-plane");
					else if (block.j_flag == OFF) /* j or k flag on to get here */
						block.j_number = 0.0;
					else if (block.k_flag == OFF)
						block.k_number = 0.0;
				}
				else if (settings.plane == CANON_PLANE.XZ)
				{
					if (block.j_flag == ON)
						return ErrorNumberOf("J word given for arc in XZ-plane");
					else if (block.i_flag == OFF) /* i or k flag on to get here */
						block.i_number = 0.0;
					else if (block.k_flag == OFF)
						block.k_number = 0.0;
				}
				else
					throw new Exception("Plane is not XY, YZ, or XZ in convert_arc");
			}
			else; /* r format arc; no other checks needed specific to this format */

			if (settings.plane == CANON_PLANE.XY) /* checks for both formats */
			{
				if ((block.x_flag == OFF) && (block.y_flag == OFF))
					return ErrorNumberOf("X and Y words missing for arc in XY-plane");
			}
			else if (settings.plane == CANON_PLANE.YZ)
			{
				if ((block.y_flag == OFF) && (block.z_flag == OFF))
					return ErrorNumberOf("Y and Z words missing for arc in YZ-plane");
			}
			else if (settings.plane == CANON_PLANE.XZ)
			{
				if ((block.x_flag == OFF) && (block.z_flag == OFF))
					return ErrorNumberOf("X and Z words missing for arc in XZ-plane");
			}

			utility_find_ends(ref block, ref settings, out end_x, out end_y, out end_z);

			settings.motion_mode = move;

			if (settings.plane == CANON_PLANE.XY)
			{
				if (settings.cutter_radius_compensation == CANON_CUTTER_COMP.OFF)
					status = convert_arc_xy(move, ref block, ref settings, end_x, end_y,
						end_z);
				else if (settings.feed_mode == INVERSE_TIME)
					return ErrorNumberOf("Cannot use inverse time feed with cutter radius comp");
				else if (first)
					status = convert_arc_comp1(move, ref block, ref settings, end_x, end_y, end_z);
				else
					status = convert_arc_comp2
						(move, ref block, ref settings, end_x, end_y, end_z);
			}
			else if (settings.plane == CANON_PLANE.XZ)
				status = convert_arc_zx (move, ref block, ref settings, end_z, end_x, end_y);
			else if (settings.plane == CANON_PLANE.YZ)
				status = convert_arc_yz (move, ref block, ref settings, end_y, end_z, end_x);
			else
				throw new Exception("Plane is not XY, YZ, or XZ in convert_arc");

			if (status != RS274NGC_OK)
				return RS274NGC_ERROR;
			else
				return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_axis_offsets

		Returned Value: int
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. The function is called when cutter radius compensation is on.
		2. BUG - g_code is not G_92 or G_92_2.

		Side effects:
		SET_PROGRAM_ORIGIN is called, and the coordinate
		values for the axis offsets are reset. The coordinates of the
		current point are reset.

		Called by: convert_modal_0.

		The action of G92 is described in [NCMS, pages 10 - 11] and {Fanuc,
		pages 61 - 63]. [NCMS] is ambiguous about the intent, but [Fanuc]
		is clear. When G92 is executed, an offset of the origin is calculated
		so that the coordinates of the current point with respect to the moved
		origin are as specified on the line containing the G92. If an axis
		is not mentioned on the line, the coordinates of the current point
		are not changed. The execution of G92 results in an axis offset being
		calculated and saved for each of the five axes, and the axis offsets
		are always used when motion is specified with respect to absolute
		distance mode using any of the nine coordinate systems (those designated
		by G54 - G59.3). Thus all nine coordinate systems are affected by G92.

		Being in incremental distance mode has no effect on the action of G92
		in this implementation. [NCMS] is not explicit about this, but it is
		implicit in the second sentence of [Fanuc, page 61].

		The offset is the amount the origin must be moved so that the
		coordinate of the controlled point has the specified value. For
		example if the current point is at X=4 in the currently specified
		coordinate system, then "G92 x7" causes the X-axis offset to be -3.

		Since a non-zero offset may be already be in effect when the G92 is
		called, that must be taken into account.

		The action of G92.2 is described in [NCMS, page 12]. G92.2 resets axis
		offsets to zero.

		These offset values are saved in parameters 5211-5213, which were not
		assigned in [NCMS] but look like reasonable places to put them.
		*/

		int convert_axis_offsets( /* ARGUMENT VALUES                                */
			int g_code,              /* g_code being executed (must be G_92 or G_92_2) */
			ref block block,     /* pointer to a block of RS274/NGC instructions   */
			ref setup settings)  /* pointer to machine settings                    */
		{
			if (settings.cutter_radius_compensation != CANON_CUTTER_COMP.OFF) /* not "== ON" */
				return ErrorNumberOf("Cannot change axis offsets with cutter radius comp");
			else if (g_code == G_92)
			{
				if (block.x_flag == ON)
				{
					settings.axis_offset_x =
						(settings.current_x + settings.axis_offset_x - block.x_number);
					settings.current_x = block.x_number;
				}

				if (block.y_flag == ON)
				{
					settings.axis_offset_y =
						(settings.current_y + settings.axis_offset_y - block.y_number);
					settings.current_y = block.y_number;
				}

				if (block.z_flag == ON)
				{
					settings.axis_offset_z =
						(settings.current_z + settings.axis_offset_z - block.z_number);
					settings.current_z = block.z_number;
				}

				 SET_ORIGIN_OFFSETS(settings.origin_offset_x + settings.axis_offset_x,
					settings.origin_offset_y + settings.axis_offset_y,
					settings.origin_offset_z + settings.axis_offset_z);

				settings.parameters[5211] = settings.axis_offset_x;
				settings.parameters[5212] = settings.axis_offset_y;
				settings.parameters[5213] = settings.axis_offset_z;
			}
			else if (g_code == G_92_2)
			{
				settings.current_x =
					settings.current_x + settings.axis_offset_x;
				settings.current_y =
					settings.current_y + settings.axis_offset_y;
				settings.current_z =
					settings.current_z + settings.axis_offset_z;
				 SET_ORIGIN_OFFSETS(settings.origin_offset_x,
					settings.origin_offset_y,
					settings.origin_offset_z);
				settings.axis_offset_x = 0.0;
				settings.axis_offset_y = 0.0;
				settings.axis_offset_z = 0.0;

				settings.parameters[5211] = 0.0;
				settings.parameters[5212] = 0.0;
				settings.parameters[5213] = 0.0;
			}
			else
				throw new Exception("Code is not G92 or G92.2 in convert_axis_offsets");

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_cutter_compensation_off

		Returned Value: int (RS274NGC_OK)

		Side effects:
		A comment is made that cutter radius compensation is turned off.
		The machine model of the cutter radius compensation mode is set to OFF.
		The value of program_x in the machine model is set to UNKNOWN.
			This serves as a flag when cutter radius compensation is
			turned on again.

		Called by: convert_cutter_compensation

		*/

		int convert_cutter_compensation_off( /* ARGUMENT VALUES             */
			ref setup settings)             /* pointer to machine settings */
		{
			settings.cutter_radius_compensation = CANON_CUTTER_COMP.OFF;
			settings.program_x = UNKNOWN;
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_cutter_compensation_on

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. The selected plane is not the XY plane.
		2. There is no D word in the block.
		3. Cutter radius compensation is already on.

		Side effects:
		A COMMENT function call is made (conditionally) saying that the
		interpreter is switching mode so that cutter radius compensation is on.
		The value of cutter_radius_compensation in the machine model mode is
		set to CANON_CUTTER_COMP.RIGHT or CANON_CUTTER_COMP.LEFT. The currently active tool table index in
		the machine model is updated.

		Called by: convert_cutter_compensation

		check_other_codes checks that a d word occurs only in a block with g41 or g42.

		Cutter radius compensation is carried out in the interpreter, so no
		call is made to a canonical function (although there is a canonical
		function, START_CUTTER_RADIUS_COMPENSATION, that could be called if
		the primitive level could execute it).

		*/

		int convert_cutter_compensation_on( /* ARGUMENT VALUES                */
			CANON_CUTTER_COMP side,               /* side of path cutter is on (CANON_CUTTER_COMP.LEFT or CANON_CUTTER_COMP.RIGHT) */
			ref block block,    /* pointer to a block of RS274 instructions  */
			ref setup settings) /* pointer to machine settings               */
		{
			if (settings.plane != CANON_PLANE.XY)
				return ErrorNumberOf("Cannot turn cutter radius comp on out of XY-plane");
			if (block.d_number == -1)
				return ErrorNumberOf("D word missing with cutter radius comp on");
			if (settings.cutter_radius_compensation != CANON_CUTTER_COMP.OFF)
				return ErrorNumberOf("Cannot turn cutter radius comp on when already on");

			/* radius is (settings.tool_table[block.d_number].diameter)/2.0) */
			settings.tool_table_index = block.d_number;
			settings.cutter_radius_compensation = side;
			return RS274NGC_OK;
		}

		/****************************************************************************/
		/* convert_cycle

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. The r-value is not given the first time this code is called after
			some other motion mode has been in effect.
		2. The z-value is not given the first time this code is called after
			some other motion mode has been in effect.
		3. The r clearance plane is below the bottom_z.
		4. BUG - the distance mode is neither absolute or incremental.
		5. The l number is zero.
		6. G82, G86, G88, or G89 is called when it is not already in effect,
			and no p number is in the block.
		7. G83 is called when it is not already in effect,
			and no q number is in the block.
		8. G87 is called when it is not already in effect,
			and any of the i number, j number, or k number is missing.
		9. The distance mode setting is neither incremental nor absolute.
		10. BUG - the G code is not between G_81 and G_89.
		11. One of the specific cycle functions called returns RS274NGC_ERROR.

		Side effects:
		A number of moves are made to execute the g-code

		Called by: convert_motion

		The function does not require that any of x,y,z, or r be specified in
		the block, except that if the last motion mode command executed was
		not the same as this one, the r-value and z-value must be specified.

		This function is handling the repeat feature, wherein
		the L word represents the number of repeats [NCMS, page 99]. We are
		not allowing L=0, contrary to the manual. We are allowing L > 1
		in absolute distance mode to mean "do the same thing in the same
		place several times", as provided in the manual, although this seems
		abnormal.

		In incremental distance mode, x, y, and r values are treated as
		increments to the current position and z as an increment from r.  In
		absolute distance mode, x, y, r, and z are absolute. In g87, i and j
		will always be increments, regardless of the distance mode setting, as
		implied in [NCMS, page 98], but k (z-value of top of counterbore) will
		be an absolute z-value in absolute distance mode, and an increment
		(from bottom z) in incremental distance mode.

		If the r position of a cycle is above the current_z position, this
		retracts the z-axis to the r position before moving parallel to the
		XY plane.

		In the code for this function, there is a nearly identical "for" loop
		in every case of the switch. The loop is the done with a compiler
		macro, "CYCLE_MACRO" so that the code is easy to read, automatically
		kept identical from case to case and, and much shorter than it would
		be without the macro. The loop could be put outside the switch, but
		then the switch would run every time around the loop, not just once,
		as it does here. The loop could also be placed in the called
		functions, but then it would not be clear that all the loops are the
		same, and it would be hard to keep them the same when the code is
		modified.  The macro would be very awkward as a regular function
		because it would have to be passed all of the arguments used by any of
		the specific cycles, and, if another switch in the function is to be
		avoided, it would have to passed a function pointer, but the different
		cycle functions have different arguments so the type of the pointer
		could not be declared unless the cycle functions were re-written to
		take the same arguments (in which case most of them would have several
		unused arguments).

		The motions within the CYCLE_MACRO (but outside a specific cycle) are
		a straight traverse parallel to the XY-plane to the given xy-position
		and a straight traverse of the z-axis only (if needed) to the r
		position.

		The height of the retract move at the end of each repeat of a cycle is
		determined by the setting of the retract_mode: either to the r
		position (if the retract_mode is R_PLANE) or to the original
		z-position (if that is above the r position and the retract_mode is
		not R_PLANE). This is a slight departure from [NCMS, page 98], which
		does not require checking that the original z-position is above r.

		*/

		//#define CYCLE_MACRO(call)                       \
		//		for (repeat = block.l_number;             \
		//			repeat > 0;                                \
		//			repeat--)                                  \
		//		{                                               \
		//		x = (x + x_increment);                   \
		//		y = (y + y_increment);                   \
		//		STRAIGHT_TRAVERSE(x, y, old_z);               \
		//		if (old_z != r)                             \
		//			STRAIGHT_TRAVERSE(x, y, r);                 \
		//		if (call == RS274NGC_ERROR)                   \
		//			return RS274NGC_ERROR;
		//		old_z = clear_z;                         \
		//		}
		//
		int convert_cycle(       /* ARGUMENT VALUES                                */
			int motion,             /* a g-code between G_81 and G_89, a canned cycle */
			ref block block,    /* pointer to a block of RS274/NGC instructions   */
			ref setup settings) /* pointer to machine settings                    */
		{
			double x_increment;
			double y_increment;
			double i;
			double j;
			double k;
			double x;
			double y;
			double clear_z;
			double old_z;
			double z;
			double r;
			int repeat;
			CANON_MOTION_MODE save_mode;

			if (settings.motion_mode != motion)
			{
				if (block.r_flag == OFF)
					return ErrorNumberOf("R clearance plane unspecified in canned cycle");
				if (block.z_flag == OFF)
					return ErrorNumberOf("Z value unspecified in canned cycle");
			}

			block.r_number =
				block.r_flag == ON ? block.r_number : settings.cycle_r;
			block.z_number =
				block.z_flag == ON ? block.z_number : settings.cycle_z;
			old_z = settings.current_z;

			if (settings.distance_mode == DISTANCE_MODE.ABSOLUTE)
			{
				x_increment = 0.0;
				y_increment = 0.0;
				r = block.r_number;
				z = block.z_number;
				x = block.x_flag == ON ? block.x_number : settings.current_x;
				y = block.y_flag == ON ? block.y_number : settings.current_y;
			}
			else if (settings.distance_mode == DISTANCE_MODE.INCREMENTAL)
			{
				x_increment = block.x_number;
				y_increment = block.y_number;
				r = (block.r_number + old_z);
				z = (r + block.z_number); /* [NCMS, page 98] */
				x = settings.current_x;
				y = settings.current_y;
			}
			else
				throw new Exception(
					"Distance mode is neither absolute nor incremental in convert_cycle");

			if (r < z)
				return ErrorNumberOf("R value less than Z value in canned cycle");

			if (block.l_number == -1)
				block.l_number = 1;
			else if (block.l_number == 0)
				return ErrorNumberOf("Cannot do zero repeats of cycle");

			if (old_z < r)
			{
				 STRAIGHT_TRAVERSE(settings.current_x, settings.current_y, r);
				old_z = r;
			}
			clear_z = (settings.retract_mode == RETRACT_MODE.R_PLANE) ? r : old_z;

			save_mode =  GET_MOTION_CONTROL_MODE();
			if (save_mode != CANON_MOTION_MODE.EXACT_PATH)
				 SET_MOTION_CONTROL_MODE(CANON_MOTION_MODE.EXACT_PATH);

			switch(motion)
			{
				case G_81:
					for (repeat = block.l_number; repeat > 0; repeat--)
					{
						x = (x + x_increment);
						y = (y + y_increment);
						 STRAIGHT_TRAVERSE(x, y, old_z);
						if (old_z != r)
							 STRAIGHT_TRAVERSE(x, y, r);
						if (convert_cycle_g81(x, y, clear_z, z) == RS274NGC_ERROR)
							return RS274NGC_ERROR;
						old_z = clear_z;
					}					
					break;
				case G_82:
					if ((settings.motion_mode != G_82) && (block.p_number == -1.0))
						return ErrorNumberOf("P word (dwell time) missing with G82");
					block.p_number =
						block.p_number == -1.0 ? settings.cycle_p : block.p_number;
					for (repeat = block.l_number; repeat > 0; repeat--)
					{
						x = (x + x_increment);
						y = (y + y_increment);
						 STRAIGHT_TRAVERSE(x, y, old_z);
						if (old_z != r)
							 STRAIGHT_TRAVERSE(x, y, r);
						if (convert_cycle_g82 (x, y, clear_z, z, block.p_number) == RS274NGC_ERROR)
							return RS274NGC_ERROR;
						old_z = clear_z;
					}					
					settings.cycle_p = block.p_number;
					break;
				case G_83:
					if ((settings.motion_mode != G_83) && (block.q_number == -1.0))
						return ErrorNumberOf("Q word (depth increment) missing with G83");
					block.q_number =
						block.q_number == -1.0 ? settings.cycle_q : block.q_number;
					for (repeat = block.l_number; repeat > 0; repeat--)
					{
						x = (x + x_increment);
						y = (y + y_increment);
						 STRAIGHT_TRAVERSE(x, y, old_z);
						if (old_z != r)
							 STRAIGHT_TRAVERSE(x, y, r);
						if (convert_cycle_g83 (x, y, r, clear_z, z, block.q_number) == RS274NGC_ERROR)
							return RS274NGC_ERROR;
						old_z = clear_z;
					}			
					settings.cycle_q = block.q_number;
					break;
				case G_84:
					for (repeat = block.l_number; repeat > 0; repeat--)
					{
						x = (x + x_increment);
						y = (y + y_increment);
						 STRAIGHT_TRAVERSE(x, y, old_z);
						if (old_z != r)
							 STRAIGHT_TRAVERSE(x, y, r);
						if (convert_cycle_g84 (x, y, clear_z, z, settings.spindle_turning, settings.speed_feed_mode) == RS274NGC_ERROR)
							return RS274NGC_ERROR;
						old_z = clear_z;
					}					
					break;
				case G_85:
					for (repeat = block.l_number; repeat > 0; repeat--)
					{
						x = (x + x_increment);
						y = (y + y_increment);
						 STRAIGHT_TRAVERSE(x, y, old_z);
						if (old_z != r)
							 STRAIGHT_TRAVERSE(x, y, r);
						if (convert_cycle_g85 (x, y, clear_z, z) == RS274NGC_ERROR)
							return RS274NGC_ERROR;
						old_z = clear_z;
					}					
					break;
				case G_86:
					if ((settings.motion_mode != G_86) && (block.p_number == -1.0))
						return ErrorNumberOf("P word (dwell time) missing with G86");
					block.p_number =
						block.p_number == -1.0 ? settings.cycle_p : block.p_number;
					for (repeat = block.l_number; repeat > 0; repeat--)
					{
						x = (x + x_increment);
						y = (y + y_increment);
						 STRAIGHT_TRAVERSE(x, y, old_z);
						if (old_z != r)
							 STRAIGHT_TRAVERSE(x, y, r);
						if (convert_cycle_g86 (x, y, clear_z, z, block.p_number, settings.spindle_turning) == RS274NGC_ERROR)
							return RS274NGC_ERROR;
						old_z = clear_z;
					}					
					settings.cycle_p = block.p_number;
					break;
				case G_87:
					if (settings.motion_mode != G_87)
					{
						if (block.i_flag == OFF)
							return ErrorNumberOf("I word missing with G87");
						if (block.j_flag == OFF)
							return ErrorNumberOf("J word missing with G87");
						if (block.k_flag == OFF)
							return ErrorNumberOf("K word missing with G87");
					}
					i = block.i_flag == ON ? block.i_number : settings.cycle_i;
					j = block.j_flag == ON ? block.j_number : settings.cycle_j;
					k = block.k_flag == ON ? block.k_number : settings.cycle_k;
					settings.cycle_i = i;
					settings.cycle_j = j;
					settings.cycle_k = k;
					if (settings.distance_mode == DISTANCE_MODE.INCREMENTAL)
					{
						k = (z + k); /* k always absolute in function call below */
					}
					for (repeat = block.l_number; repeat > 0; repeat--)
					{
						x = (x + x_increment);
						y = (y + y_increment);
						 STRAIGHT_TRAVERSE(x, y, old_z);
						if (old_z != r)
							 STRAIGHT_TRAVERSE(x, y, r);
						if (convert_cycle_g87 (x, (x + i), y, (y + j), r, clear_z, k, z, settings.spindle_turning) == RS274NGC_ERROR)
							return RS274NGC_ERROR;
						old_z = clear_z;
					}					
					break;
				case G_88:
					if ((settings.motion_mode != G_88) && (block.p_number == -1.0))
						return ErrorNumberOf("P word (dwell time) missing with G88");
					block.p_number =
						block.p_number == -1.0 ? settings.cycle_p : block.p_number;
					for (repeat = block.l_number; repeat > 0; repeat--)
					{
						x = (x + x_increment);
						y = (y + y_increment);
						 STRAIGHT_TRAVERSE(x, y, old_z);
						if (old_z != r)
							 STRAIGHT_TRAVERSE(x, y, r);
						if (convert_cycle_g88 (x, y, z, block.p_number, settings.spindle_turning) == RS274NGC_ERROR)
							return RS274NGC_ERROR;
						old_z = clear_z;
					}					
					settings.cycle_p = block.p_number;
					break;
				case G_89:
					if ((settings.motion_mode != G_89) && (block.p_number == -1.0))
						return ErrorNumberOf("P word (dwell time) missing with G89");
					block.p_number =
						block.p_number == -1.0 ? settings.cycle_p : block.p_number;
					for (repeat = block.l_number; repeat > 0; repeat--)
					{
						x = (x + x_increment);
						y = (y + y_increment);
						 STRAIGHT_TRAVERSE(x, y, old_z);
						if (old_z != r)
							 STRAIGHT_TRAVERSE(x, y, r);
						if (convert_cycle_g89 (x, y, clear_z, z, block.p_number) == RS274NGC_ERROR)
							return RS274NGC_ERROR;
						old_z = clear_z;
					}					
					settings.cycle_p = block.p_number;
					break;
				default:
					throw new Exception("Convert_cycle should not have been called");
			}
			settings.current_x = x;
			settings.current_y = y;
			settings.current_z = clear_z;
			settings.cycle_l = block.l_number;
			settings.cycle_r = block.r_number;
			settings.cycle_z = block.z_number;
			settings.motion_mode = motion;

			if (save_mode != CANON_MOTION_MODE.EXACT_PATH)
				 SET_MOTION_CONTROL_MODE(save_mode);

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_probe

		Returned Value: int
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. No value is given in the block for any of X, Y, or Z.

		Side effects:
		This executes a straight_probe command.
		The probe_flag in the settings is set to ON.
		The motion mode in the settings is set to G_38_2.

		Called by: convert_motion.

		The approach to operating in incremental distance mode (g91) is to
		put the the absolute position values into the block before using the
		block to generate a move.

		After probing is performed, the location of the probe cannot be
		predicted.  This differs from every other command, all of which have
		predictable results. The next call to the interpreter (with either
		rs274ngc_read or rs274ngc_execute) will result in updating the
		current position by a call to get_position. When running stand-alone
		the get_position function just returns the current position. To
		provide a reasonable value for get_position to return, in stand-alone
		mode (only) this function sets the current position nine_tenths of the
		way from the old current position to the programmed x, y, z point.

		*/

		int convert_probe(       /* ARGUMENT VALUES                          */
			ref block block,    /* pointer to a block of RS274 instructions */
			ref setup settings) /* pointer to machine settings              */
		{
			double end_x;
			double end_y;
			double end_z;

			if (((block.x_flag == OFF) && (block.y_flag == OFF)) &&
				(block.z_flag == OFF))
				return ErrorNumberOf("X, Y, and Z words all missing with G38.2");
			utility_find_ends(ref block, ref settings, out end_x, out end_y, out end_z);
			 TURN_PROBE_ON();
			 STRAIGHT_PROBE (end_x, end_y, end_z);
			 TURN_PROBE_OFF();
			settings.motion_mode = G_38_2;
			settings.probe_flag = ON;

			settings.current_x = ((0.9 * end_x) + (0.1 * settings.current_x));
			settings.current_y = ((0.9 * end_y) + (0.1 * settings.current_y));
			settings.current_z = ((0.9 * end_z) + (0.1 * settings.current_z));

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_setup

		Returned Value: int (RS274NGC_OK)

		Side effects:
		SET_ORIGIN_OFFSETS is called, and the coordinate values for the origin
		offsets in the settings are reset.
		If the coordinate system being changed is currently in use, the values
		of the the coordinates of the current point are updated.

		Called by: convert_g.

		This is called only if g10 is called. g10 may be used to alter the
		second program coordinate system as described in [NCMS, pages 9 - 10].
		All nine coordinate systems are implemented here.

		Being in incremental distance mode has no effect on the action of G10
		in this implementation. The manual is not explicit about what is
		intended.

		See documentation of convert_coordinate_systems for more information.

		*/

		int convert_setup(        /* ARGUMENT VALUES                              */
			ref block block,     /* pointer to a block of RS274/NGC instructions */
			ref setup settings)  /* pointer to machine settings                  */
		{
			double x;
			double y;
			double z;
			double[] parameters;
			int p_int;

			parameters = settings.parameters;
			p_int = (int)(block.p_number + 0.0001); /* p_number is a double */

			if (block.x_flag == ON)
			{
				x = block.x_number;
				parameters[5201 + (p_int * 20)] = x;
			}
			else
				x = parameters[5201 + (p_int * 20)];

			if (block.y_flag == ON)
			{
				y = block.y_number;
				parameters[5202 + (p_int * 20)] = y;
			}
			else
				y = parameters[5202 + (p_int * 20)];

			if (block.z_flag == ON)
			{
				z = block.z_number;
				parameters[5203 + (p_int * 20)] = z;
			}
			else
				z = parameters[5203 + (p_int * 20)];

			/* axis offsets could be included in the following calculations but
			do not need to be because the results do not change */
			if (p_int == settings.origin_ngc) /* system is currently used */
			{
				settings.current_x =
					(settings.current_x + settings.origin_offset_x);
				settings.current_y =
					(settings.current_y + settings.origin_offset_y);
				settings.current_z =
					(settings.current_z + settings.origin_offset_z);

				settings.origin_offset_x = x;
				settings.origin_offset_y = y;
				settings.origin_offset_z = z;

				 SET_ORIGIN_OFFSETS(x + settings.axis_offset_x,
					y + settings.axis_offset_y,
					z + settings.axis_offset_z);
				settings.current_x = (settings.current_x - x);
				settings.current_y = (settings.current_y - y);
				settings.current_z = (settings.current_z - z);
			}
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_straight

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. x, y, and z are all missing from the block.
		2. convert_straight_comp1 or convert_straight_comp2 is called
			and returns RS274NGC_ERROR.
		3. A traverse (g0) move is called with cutter radius compensation on.
		4. BUG - The value of move is not G_0 or G_1.
		5. A straight feed (g1) move is called with feed rate set to 0.
		6. A straight feed (g1) move is called with cutter radius compensation
			on while inverse time feed is in effect.
		7. A straight feed (g1) move is called with inverse time feed in effect
			but no f word (feed time) is provided.
		8. A G1 move is called with G53 and cutter radius compensation on.

		Side effects:
		This executes a straight move command at either traverse feed rate
		(if move is 0) or cutting feed rate (if move is 1). It also updates
		the setting of the position of the tool point to the end point of
		the move. If cutter radius compensation is on, it may also generate
		an arc before the straight move.

		Called by: convert_motion.

		The approach to operating in incremental distance mode (g91) is to
		put the the absolute position values into the block before using the
		block to generate a move.

		*/

		int convert_straight(    /* ARGUMENT VALUES                          */
			int move,               /* either G_0 (for g0) or G_1 (for g1)      */
			ref block block,    /* pointer to a block of RS274 instructions */
			ref setup settings) /* pointer to machine settings              */
		{
			double end_x = 0.0;
			double end_y = 0.0;
			double end_z = 0.0;
			double length;
			double rate;

			if (((block.x_flag == OFF) && (block.y_flag == OFF)) &&
				(block.z_flag == OFF))
				return ErrorNumberOf("X, Y, and Z words all missing with G0 or G1");

			settings.motion_mode = move;
			utility_find_ends(ref block, ref settings, out end_x, out end_y, out end_z);

			if (move == G_0)
			{
				if (settings.cutter_radius_compensation != CANON_CUTTER_COMP.OFF)
					return ErrorNumberOf("Cannot use G0 with cutter radius comp");
				 STRAIGHT_TRAVERSE(end_x, end_y, end_z);
				settings.current_x = end_x;
				settings.current_y = end_y;
			}
			else if (move == G_1)
			{
				if (settings.cutter_radius_compensation != CANON_CUTTER_COMP.OFF) /* NOT "== ON"! */
				{
					if (settings.feed_mode == INVERSE_TIME)
						return ErrorNumberOf("Cannot use inverse time feed with cutter radius comp");
					else if (block.g_modes[4] == G_53)
						return ErrorNumberOf("Cannot use G53 with cutter radius comp");
					else if (settings.feed_rate == 0.0)
						return ErrorNumberOf("Cannot do G1 with zero feed rate");
					else if (settings.program_x == UNKNOWN)
					{
						if (convert_straight_comp1(ref settings, end_x, end_y, end_z)
							== RS274NGC_ERROR)
							return RS274NGC_ERROR;
					}
					else
					{
						if (convert_straight_comp2(ref settings, end_x, end_y, end_z)
							== RS274NGC_ERROR)
							return RS274NGC_ERROR;
					}
				}
				else
				{
					if (settings.feed_mode == INVERSE_TIME)
					{
						if (block.f_number == -1.0)
							return ErrorNumberOf("F word missing with inverse time G1 move");
						else
						{
							length = utility_find_straight_length
								(end_x, end_y, end_z,
								settings.current_x, settings.current_y,
								settings.current_z);
							rate = Math.Max(0.1, (length * block.f_number));
							 SET_FEED_RATE (rate);
							settings.feed_rate = rate;
						}
					}
					else if (settings.feed_rate == 0.0)
						return ErrorNumberOf("Cannot do G1 with zero feed rate");
					 STRAIGHT_FEED(end_x, end_y, end_z);
					settings.current_x = end_x;
					settings.current_y = end_y;
				}
			}
			else
				throw new Exception("Code is not G0 or G1 in convert_straight");

			settings.current_z = end_z;
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* read_comment

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		BUG - If the first character read is not '(' , this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.

		Side effects:
		The counter is reset to point to the character following the comment.
		The comment string, without parentheses, is copied into the comment
		area of the block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character '(', indicating a comment is
		beginning. The function reads characters of the comment, up to and
		including the comment closer ')'.

		It is expected that the format of a comment will have been checked (by
		read_text or read_keyboard_line) and bad format comments will
		have prevented the system from getting this far, so that this function
		can assume a close parenthesis will be found when an open parenthesis
		has been found, and that comments are not nested.

		The "parameters" argument is not used in this function. That argument is
		present only so that this will have the same argument list as the other
		"read_XXX" functions called using a function pointer by read_one_item.

		*/

		int read_comment(     /* ARGUMENT VALUES                               */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                    */
		{
			int n;
			StringBuilder b = new StringBuilder(line.Length);

			if (line[counter] != '(')
				throw new Exception("Read_comment should not have been called");

			counter++;
			for (n = 0; line[counter] != ')' ; counter++, n++)
			{
				b.Append(line[counter]);
			}	
			block.comment = b.ToString();
			counter++;
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* read_d

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not d.
		2. A d_number has already been inserted in the block.
		3. The value cannot be read.
		4. The d_number is negative.

		Side effects:
		counter is reset to the character following the tool number.
		A d_number is inserted in the block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'd', indicating an index into a
		table of tool diameters.  The function reads characters which give the
		(positive integer) value of the index.

		read_integer_value allows a minus sign, so a check for a negative value
		is needed here, and the parameters argument is also needed.

		*/

		int read_d(           /* ARGUMENT VALUES                               */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                    */
		{
			int value;

			if (line[counter] != 'd')
				throw new Exception("Read_d should not have been called");

			counter++;

			if (block.d_number > -1)
				return ErrorNumberOf("Multiple D words on one line");
			else if (read_integer_value(line,ref  counter, out value, ref parameters) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else if (value < 0)
				return ErrorNumberOf("Negative D code used");
			else
			{
				block.d_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_f

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not f.
		2. An f_number has already been inserted in the block.
		3. The f_number cannot be read.
		4. The f_number is negative.

		Side effects:
		counter is reset to point to the first character following the f_number.
		The f_number is inserted in block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'f'. The function reads characters
		which tell how to set the f_number, up to the start of the next item
		or the end of the line. This information is inserted in the block.

		The value may be a real number or something that evaluates to a
		real number, so read_real_value is used to read it. Parameters
		may be involved, so an extra argument is required. The value is always
		a feed rate.

		*/

		int read_f(           /* ARGUMENT VALUES                               */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                    */
		{
			double value;

			if (line[counter] != 'f')
				throw new Exception("Read_f should not have been called");

			counter++;

			if (block.f_number > -1.0)
				return ErrorNumberOf("Multiple F words on one line");
			else if (read_real_value(line, ref counter, out value, ref parameters)
				== RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else if (value < 0.0)
				return ErrorNumberOf("Negative F word found");
			else
			{
				block.f_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_g

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not g.
		2. The value cannot be read.
		3. The value is negative.
		4. The value differs from a number ending in an even tenth by more
			than 0.0001.
		5. Another g code from the same modal group has already been
			inserted in the block.

		Side effects:
		counter is reset to the character following the end of the g_code.
		A g code is inserted as the value of the appropriate mode in the
		g_modes array in the block.
		The g code counter in the block is increased by 1.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'g', indicating a g_code.  The
		function reads characters which tell how to set the g_code.

		The manual [NCMS, page 51] allows g_codes to be represented
		by expressions and provide [NCMS, 71 - 73] that a g_code must evaluate
		to to a number of the form XX.X (59.1, for example). The manual does not
		say how close an expression must come to one of the allowed values for
		it to be legitimate. Is 59.099999 allowed to mean 59.1, for example?
		In the interpreter, we adopt the convention that the evaluated number
		for the g_code must be within 0.0001 of a value of the form XX.X

		To simplify the handling of g_codes, we convert them to integers by
		multiplying by 10 and rounding down or up if within 0.001 of an
		integer. Other functions that deal with g_codes handle them
		symbolically, however. The symbols are defined in rs274ngc.hh
		where G_1 is 10, G_83 is 830, etc.

		This allows any number g_codes on one line, provided that no two are
		in the same modal group. The check_g_codes function checks that no
		more than some maximum number of g_codes (currently 4) are on the same
		line.

		*/

		int read_g(           /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274/NGC code being processed */
			ref int counter,       /* pointer to a counter for position on the line  */
			ref block block, /* pointer to a block being filled from the line  */
			ref double[] parameters) /* array of system parameters                     */
		{
			double value_read;
			int value;
			int mode;

			if (line[counter] != 'g')
				throw new Exception("Read_g should not have been called");

			counter++;

			if (read_real_value(line, ref counter, out value_read, ref parameters)
				== RS274NGC_ERROR)
				return RS274NGC_ERROR;
			value_read = (10.0 * value_read);
			value = (int)Math.Floor(value_read);

			if ((value_read - value) > 0.999)
				value = (int)Math.Ceiling(value_read);
			else if ((value_read - value) > 0.001)
				return ErrorNumberOf("G code out of range");

			if (value > 999)
				return ErrorNumberOf("G code out of range");
			else if (value < 0)
				return ErrorNumberOf("Negative G code used");

			mode = gees[value];

			if (mode == -1)
				return ErrorNumberOf("Unknown G code used");
			else if (block.g_modes[mode] != -1)
				return ErrorNumberOf("Two G codes used from same modal group");
			else
			{
				block.g_modes[mode] = value;
				block.g_count++;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_i

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not i.
		2. A i_coordinate has already been inserted in the block.
		3. The value cannot be read.

		Side effects:
		counter is reset.
		The i_flag in the block is turned on.
		A i_coordinate setting is inserted in the block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'i', indicating a i_coordinate
		setting. The function reads characters which tell how to set the
		coordinate, up to the start of the next item or the end of the line.
		This information is inserted in the block. The counter is then set to
		point to the character following.

		The value may be a real number or something that evaluates to a
		real number, so read_real_value is used to read it. Parameters
		may be involved.

		*/

		int read_i(           /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                     */
		{
			double value;

			if (line[counter] != 'i')
				throw new Exception("Read_i should not have been called");

			counter++;

			if (block.i_flag != OFF)
				return ErrorNumberOf("Multiple I words on one line");
			else if (read_real_value(line, ref counter, out value, ref parameters)
				== RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else
			{
				block.i_flag = ON;
				block.i_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_j

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not j.
		2. A j_coordinate has already been inserted in the block.
		3. The value cannot be read.

		Side effects:
		counter is reset.
		The j_flag in the block is turned on.
		A j_coordinate setting is inserted in the block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'j', indicating a j_coordinate
		setting. The function reads characters which tell how to set the
		coordinate, up to the start of the next item or the end of the line.
		This information is inserted in the block. The counter is then set to
		point to the character following.

		The value may be a real number or something that evaluates to a
		real number, so read_real_value is used to read it. Parameters
		may be involved.

		*/

		int read_j(           /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                     */
		{
			double value;

			if (line[counter] != 'j')
				throw new Exception("Read_j should not have been called");

			counter++;

			if (block.j_flag != OFF)
				return ErrorNumberOf("Multiple J words on one line");
			else if (read_real_value(line, ref counter, out value, ref parameters)
				== RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else
			{
				block.j_flag = ON;
				block.j_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_k

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not k.
		2. A k_coordinate has already been inserted in the block.
		3. The value cannot be read.

		Side effects:
		counter is reset.
		The k_flag in the block is turned on.
		A k_coordinate setting is inserted in the block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'k', indicating a k_coordinate
		setting. The function reads characters which tell how to set the
		coordinate, up to the start of the next item or the end of the line.
		This information is inserted in the block. The counter is then set to
		point to the character following.

		The value may be a real number or something that evaluates to a
		real number, so read_real_value is used to read it. Parameters
		may be involved.

		*/

		int read_k(           /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                     */
		{
			double value;

			if (line[counter] != 'k')
				throw new Exception("Read_k should not have been called");

			counter++;

			if (block.k_flag != OFF)
				return ErrorNumberOf("Multiple K words on one line");
			else if (read_real_value(line, ref counter, out value, ref parameters)
				== RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else
			{
				block.k_flag = ON;
				block.k_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_l

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - the first character read is not l
		2. the value cannot be read
		3. the value is negative
		4. there is already an l code in the block

		Side effects:
		counter is reset to the character following the l number.
		An l code is inserted in the block as the value of l.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'l', indicating an L code.
		The function reads characters which give the (integer) value of the
		L code.

		L codes are used for:
		1. the number of times a canned cycle should be repeated.
		2. a key with G10.

		*/

		int read_l(           /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                     */
		{
			int value;

			if (line[counter] != 'l')
				throw new Exception("Read_l should not have been called");

			counter++;

			if (block.l_number > -1)
				return ErrorNumberOf("Multiple L words on one line");
			else if (read_integer_value(line, ref counter, out value, ref parameters)
				== RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else if (value < 0)
				return ErrorNumberOf("Negative L word used");
			else
			{
				block.l_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_m

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not m.
		2. The value cannot be read.
		3. The value is negative.
		4. The value is greater than 99.
		5. The m code is not known to the system.
		6. Another m code in the same modal group has already been read.

		Side effects:
		counter is reset to the character following the m number.
		An m code is inserted as the value of the appropriate mode in the
		m_modes array in the block.
		The m code counter in the block is increased by 1.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'm', indicating an m code.
		The function reads characters which give the (integer) value of the
		m code.

		read_integer_value allows a minus sign, so a check for a negative value
		is needed here, and the parameters argument is also needed.

		*/

		int read_m(           /* ARGUMENT VALUES                               */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                    */
		{
			int value;
			int mode;

			if (line[counter] != 'm')
				throw new Exception("Read_m should not have been called");

			counter++;

			if (read_integer_value(line, ref counter, out value, ref parameters) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else if (value < 0)
				return ErrorNumberOf("Negative M code used");
			else if (value > 99)
				return ErrorNumberOf("M code greater than 99");

			mode = ems[value];

			if (mode == -1)
				return ErrorNumberOf("Unknown M code used");
			else if (block.m_modes[mode] != -1)
				return ErrorNumberOf("Two M codes used from same modal group");
			else
			{
				block.m_modes[mode] = value;
				block.m_count++;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_p

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not p.
		2. A p value has already been inserted in the block.
		3. The p value cannot be read.
		4. The p value is negative.

		Side effects:
		counter is reset to point to the first character following the p value.
		The p value setting is inserted in block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'p', indicating a p value
		setting. The function reads characters which tell how to set the p
		value, up to the start of the next item or the end of the line. This
		information is inserted in the block.

		P codes are used for:
		1. Dwell time with G4 dwell [NCMS, page 23].
		2. Dwell time in canned cycles g82, G86, G88, G89 [NCMS pages 98 - 100].
		3. A key with G10 [NCMS, pages 9, 10].

		*/

		int read_p(           /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                     */
		{
			double value;

			if (line[counter] != 'p')
				throw new Exception("Read_p should not have been called");

			counter++;
			if (block.p_number > -1.0)
				return ErrorNumberOf("Multiple P words on one line");
			else if (read_real_value(line, ref counter, out value, ref parameters)
				== RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else if (value < 0.0)
				return ErrorNumberOf("Negative P value used");
			else
			{
				block.p_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_parameter_setting

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not # .
		2. The characters immediately following do not evaluate to an integer.
		3. The parameter index is out of range.
		4. An equal sign does not follow the parameter expression.
		5. The value on the right side of the equal sign cannot be read.

		Side effects:
		counter is reset to the character following the end of the parameter
		setting. The parameter whose index follows "#" is set to the
		real value following "=".

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character '#', indicating a parameter
		setting.  The function reads characters which tell how to set the
		parameter.

		Any number of parameters may be set on a line, and parameters set
		early on the line may be used in expressions later on the line.

		Parameter setting is not clearly described in [NCMS, pp. 51 - 62]: it is
		not clear if more than one parameter setting per line is allowed (any
		number is OK in this implementation). The characters immediately following
		the "#" must constitute a "parameter expression", but it is not clear
		what that is. Here we allow any expression as long as it evaluates to
		an integer.

		Parameters are handled in the interpreter by having a parameter table as
		part of the machine settings. The parameter table is passed to the
		reading functions which need it. Reading functions may set parameter
		values or use them.

		The syntax recognized by this this function is # followed by an
		integer expression (explicit integer or expression evaluating to an
		integer) followed by = followed by a real value (number or
		expression).

		Note that # also starts a bunch of characters which represent a parameter
		to be evaluated. That situation is handled by read_parameter.

		*/

		int read_parameter_setting(  /* ARGUMENT VALUES                         */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                     */
		{
			int index;
			double value;

			if (line[counter] != '#')
				throw new Exception("Read_parameter_setting should not have been called");
			counter++;
			if (read_integer_value(line, ref counter, out index, ref parameters) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else if ((index < 1) || (index >= RS274NGC_MAX_PARAMETERS))
				return ErrorNumberOf("Parameter number out of range");
			else if (line[counter] != '=')
				return ErrorNumberOf("Equal sign missing in parameter setting");
			counter++;
			if (read_real_value(line, ref counter, out value, ref parameters) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else
			{
				parameters[index] = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_q

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not q.
		2. A q value has already been inserted in the block.
		3. The q value cannot be read.
		4. The q value is negative.

		Side effects:
		counter is reset to point to the first character following the q value.
		The q value setting is inserted in block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'q', indicating a q value
		setting. The function reads characters which tell how to set the q
		value, up to the start of the next item or the end of the line. This
		information is inserted in the block.

		Q is used in the G87 canned cycle [NCMS, page 98].

		*/

		int read_q(           /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                     */
		{
			double value;

			if (line[counter] != 'q')
				throw new Exception("Read_q should not have been called");

			counter++;
			if (block.q_number > -1.0)
				return ErrorNumberOf("Multiple Q words on one line");
			else if (read_real_value(line, ref counter, out value, ref parameters)
				== RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else if (value < 0.0)
				return ErrorNumberOf("Negative Q value used");
			else
			{
				block.q_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_r

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not r.
		2. An r_number has already been inserted in the block.
		3. The value cannot be read.

		Side effects:
		counter is reset.
		The r_flag in the block is turned on.
		The r_number is inserted in the block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'r'. The function reads characters
		which tell how to set the coordinate, up to the start of the next item
		or the end of the line. This information is inserted in the block. The
		counter is then set to point to the character following.

		An r number indicates the clearance plane in canned cycles.

		An r number may also be the radius of an arc. The parameters argument
		is needed.

		*/

		int read_r(           /* ARGUMENT VALUES                               */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                    */
		{
			double value;

			if (line[counter] != 'r')
				throw new Exception("Read_r should not have been called");

			counter++;

			if (block.r_flag != OFF)
				return ErrorNumberOf("Multiple R words on one line");
			else if (read_real_value(line, ref counter, out value, ref parameters)
				== RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else
			{
				block.r_flag = ON;
				block.r_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_spindle_speed

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not s.
		2. A spindle speed has already been inserted in the block.
		3. The spindle speed cannot be read.
		4. The spindle speed is negative.

		Side effects:
		counter is reset to the character following the spindle speed.
		A spindle speed setting is inserted in the block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 's', indicating a spindle speed
		setting. The function reads characters which tell how to set the spindle
		speed.

		The value may be a real number or something that evaluates to a
		real number, so read_real_value is used to read it. Parameters
		may be involved.

		*/

		int read_spindle_speed( /* ARGUMENT VALUES                               */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters)   /* array of system parameters                    */
		{
			double speed;

			if (line[counter] != 's')
				throw new Exception("Read_spindle_speed should not have been called");

			counter++;

			if (block.s_number > -1.0)
				return ErrorNumberOf("Multiple S word spindle speed settings on one line");
			else if (read_real_value(line, ref counter, out speed, ref parameters)			// +++ threw except on invalid char!
				== RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else if (speed < 0.0)
				return ErrorNumberOf("Negative spindle speed found");
			else
			{
				block.s_number = speed;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_tool

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not t.
		2. A t_number has already been inserted in the block.
		3. The value cannot be read.
		4. The tool number is negative.

		Side effects:
		counter is reset to the character following the tool number.
		A t_number is inserted in the block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 't', indicating a tool.
		The function reads characters which give the (integer) value of the
		tool code.

		*/

		int read_tool(        /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                     */
		{
			int value;

			if (line[counter] != 't')
				throw new Exception("Read_tool should not have been called");

			counter = (counter + 1);
			if (block.t_number > -1)
				return ErrorNumberOf("Multiple T words (tool ids) on one line");
			if (read_integer_value(line, ref counter, out value, ref parameters) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else if (value < 0)
				return ErrorNumberOf("Negative tool id used");
			else
			{
				block.t_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_tool_length_offset

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not h.
		2. An h_number has already been inserted in the block.
		3. The value cannot be read.
		4. The value is negative.

		Side effects:
		counter is reset to the character following the tool length offset number.
		An h_number is inserted in the block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'h', indicating a tool length
		offset.  The function reads characters which give the (integer) value
		of the tool length offset code. This code is a reference to the
		location of an offset in a table, not the actual distance of the
		offset.

		*/

		int read_tool_length_offset(  /* ARGUMENT VALUES                        */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                     */
		{
			int value;

			if (line[counter] != 'h')
				throw new Exception("Read_tool_length_offset should not have been called");

			counter++;
			if (block.h_number > -1)
				return ErrorNumberOf("Multiple tool length offsets on one line");
			if (read_integer_value(line, ref counter, out value, ref parameters) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else if (value < 0)
				return ErrorNumberOf("Negative tool length offset used");
			else
			{
				block.h_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_x

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not x.
		2. A x_coordinate has already been inserted in the block.
		3. The value cannot be read.

		Side effects:
		counter is reset.
		The x_flag in the block is turned on.
		A x_coordinate setting is inserted in the block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'x', indicating a x_coordinate
		setting. The function reads characters which tell how to set the
		coordinate, up to the start of the next item or the end of the line.
		This information is inserted in the block. The counter is then set to
		point to the character following.

		The value may be a real number or something that evaluates to a
		real number, so read_real_value is used to read it. Parameters
		may be involved.

		*/

		int read_x(           /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                     */
		{
			double value;

			if (line[counter] != 'x')
				throw new Exception("Read_x should not have been called");

			counter++;

			if (block.x_flag != OFF)
				return ErrorNumberOf("Multiple X words on one line");
			else if (read_real_value(line, ref counter, out value, ref parameters)
				== RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else
			{
				block.x_flag = ON;
				block.x_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_y

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not y.
		2. A y_coordinate has already been inserted in the block.
		3. The value cannot be read.

		Side effects:
		counter is reset.
		The y_flag in the block is turned on.
		A y_coordinate setting is inserted in the block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'y', indicating a y_coordinate
		setting. The function reads characters which tell how to set the
		coordinate, up to the start of the next item or the end of the line.
		This information is inserted in the block. The counter is then set to
		point to the character following.

		The value may be a real number or something that evaluates to a
		real number, so read_real_value is used to read it. Parameters
		may be involved.

		*/

		int read_y(           /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                     */
		{
			double value;

			if (line[counter] != 'y')
				throw new Exception("Read_y should not have been called");

			counter++;

			if (block.y_flag != OFF)
				return ErrorNumberOf("Multiple Y words on one line");
			else if (read_real_value(line, ref counter, out value, ref parameters)
				== RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else
			{
				block.y_flag = ON;
				block.y_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_z

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - The first character read is not z.
		2. A z_coordinate has already been inserted in the block.
		3. The value cannot be read.

		Side effects:
		counter is reset.
		The z_flag in the block is turned on.
		A z_coordinate setting is inserted in the block.

		Called by: read_one_item

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'z', indicating a z_coordinate
		setting. The function reads characters which tell how to set the
		coordinate, up to the start of the next item or the end of the line.
		This information is inserted in the block. The counter is then set to
		point to the character following.

		The value may be a real number or something that evaluates to a
		real number, so read_real_value is used to read it. Parameters
		may be involved.

		*/

		int read_z(           /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274 code being processed    */
			ref int counter,       /* pointer to a counter for position on the line */
			ref block block, /* pointer to a block being filled from the line */
			ref double[] parameters) /* array of system parameters                     */
		{
			double value;

			if (line[counter] != 'z')
				throw new Exception("Read_z should not have been called");

			counter++;

			if (block.z_flag != OFF)
				return ErrorNumberOf("Multiple Z words on one line");
			else if (read_real_value(line, ref counter, out value, ref parameters)
				== RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else
			{
				block.z_flag = ON;
				block.z_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* check_g_codes

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. A g4 (dwell) is used with an axis word.
		2. A g4 is used without a P (dwell time) word.
		3. A g4 is used and a motion g_code (other than g80) is on same line.
		4. There are too many g codes in the block.
		5. g10 (which uses axis values) is used and a motion g_code (other
			than g80) is on the same line.
		6. A g10 is used and L2 is not in the block.
		7. A g10 is used and P is not close to an integer.
		8. A g10 is used and P is less than 1 or more than 9.
		9. A g53 is used with motion other than g0 or g1.
		10. A g53 is used in incremental distance mode.
		11. A g80 is used while g10 isn't used, and an axis value is given.
		12. All axis values are missing with g92.
		13. g92 (which uses axis values) is used and a motion g_code (other
			than g80) is on the same line.
		14. BUG - g_modes[0] is set to an unknown value.

		Side effects: none

		Called by: check_items

		This runs checks on g_codes from a block of RS274/NGC instructions.

		The read_g function checks for errors which would foul up the
		reading. This function checks for additional logical errors in g_codes.

		[NCMS] does not give any maximum for how many g_codes may be put on
		the same line. The value of MAX_GEES (set in rs274ngc.hh) is currently
		4.

		We are suspending any implicit motion g_code when g4 or g10 is used.
		The implicit motion g_code takes effect again automatically after the
		line on which the g4 or g10 occurs.  It is not clear what the intent
		of [NCMS] is in this regard. The alternative is to require that any
		implicit motion be explicitly cancelled.

		Not all checks on g_codes are included here. Those checks that are
		sensitive to whether other g_codes on the same line have been executed
		yet are made by the functions called by convert_g.

		*/

		int check_g_codes(       /* ARGUMENT VALUES                  */
			ref block block,    /* pointer to a block to be checked */
			ref setup settings) /* pointer to machine settings      */
		{
			int axis_flag;
			int explicit_motion;
			int mode0;
			int p_int;

			axis_flag =
				((block.x_flag == ON) || (block.y_flag == ON) || (block.z_flag== ON)) ? ON : OFF;
			explicit_motion = block.g_modes[1];
			mode0 = block.g_modes[0];

			if(block.g_count > MAX_GEES)
				return ErrorNumberOf("Too many G codes on line");

			if (mode0 == -1)
			{
				if (block.g_modes[4] == -1)
				{
					if ((block.motion_to_be == G_80) && (axis_flag == ON))
						return ErrorNumberOf("Cannot use axis commands with G80");
				}
				else if (block.g_modes[4] == G_4)
				{
					if (((block.motion_to_be == -1) ||
						(block.motion_to_be == G_80)) &&
						(axis_flag == ON))
						return ErrorNumberOf("Cannot use axis commands with G4");
					if(block.p_number == -1.0)
						return ErrorNumberOf("Dwell time missing with G4");
				}
				else if (block.g_modes[4] == G_53)
				{
					if ((block.motion_to_be != G_0) &&
						(block.motion_to_be != G_1))
						return ErrorNumberOf("Must use G0 or G1 with G53");
					if((block.g_modes[3] == G_91) ||
						((block.g_modes[3] != G_90) &&
						(settings.distance_mode == DISTANCE_MODE.INCREMENTAL)))
						return ErrorNumberOf("Cannot use G53 in incremental distance mode");
				}
				else
					throw new Exception("Bad setting of g_mode in check_g_codes");
			}
			else if (block.g_modes[4] != -1)
				return ErrorNumberOf("Cannot use two G codes from group 0");
			else if (mode0 == G_10)
			{
				p_int = (int)(block.p_number + 0.0001);
				if ((explicit_motion != -1) && (explicit_motion != G_80))
					return ErrorNumberOf("Cannot use a G code for motion with G10");
				if (block.l_number != 2)
					return ErrorNumberOf("Line with G10 does not have L2");
				if (((block.p_number + 0.0001) - p_int) > 0.0002)
					return ErrorNumberOf("P value not an integer with G10 L2");
				if ((p_int < 1) || (p_int > 9))
					return ErrorNumberOf("P value out of range with G10 L2");
			}
			else if (mode0 == G_92)
			{
				if ((explicit_motion != -1) && (explicit_motion != G_80))
					return ErrorNumberOf("Cannot use a G motion with G92");
				if (axis_flag == OFF)
					return ErrorNumberOf("All axes missing with G92");
			}
			else if (mode0 == G_92_2)
			{}
			else
				throw new Exception("Bad setting of g_mode in check_g_codes");

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* check_m_codes

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. There are too many m codes in the block.

		Side effects: none

		Called by: check_items

		This runs checks on m_codes from a block of RS274/NGC instructions.

		The read_m function checks for errors which would foul up the
		reading. This function checks for additional errors in m_codes.

		*/

		int check_m_codes(       /* ARGUMENT VALUES                  */
			ref block block,    /* pointer to a block to be checked */
			ref setup settings) /* pointer to machine settings      */
		{
			if(block.m_count > MAX_EMS)
				return ErrorNumberOf("Too many M codes on line");

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* check_other_codes

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. A d word is in a block with no cutter_radius_compensation_on command.
		2. An h_number is in a block with no tool length offset setting.
		3. An i_number is in a block with no G code that uses it.
		4. A j_number is in a block with no G code that uses it.
		5. A k_number is in a block with no G code that uses it.
		6. A l_number is in a block with no G code that uses it.
		7. A p_number is in a block with no G code that uses it.
		8. A q_number is in a block with no G code that uses it.
		9. An r_number is in a block with no G code that uses it.

		Side effects: none

		Called by: check_items

		This runs checks on codes from a block of RS274/NGC code which are
		not m or g codes.

		The functions named read_XXXX check for errors which would foul up the
		reading. This function checks for additional logical errors in codes.

		*/

		int check_other_codes(   /* ARGUMENT VALUES                              */
			ref block block,    /* pointer to a block of RS274/NGC instructions */
			ref setup settings) /* pointer to machine settings                  */
		{
			int motion;

			motion = block.motion_to_be;
			if (block.d_number != -1)
			{
				if ((block.g_modes[7] != G_41) && (block.g_modes[7] != G_42))
					return ErrorNumberOf("D word on line with no cutter comp on (G41 or G42) command");
			}
			if (block.h_number != -1)
			{
				if (block.g_modes[8] != G_43)
					return ErrorNumberOf("H word on line with no tool length comp (G43) command");
			}

			if (block.i_flag == ON) /* could still be useless if yz_plane arc */
			{
				if (((motion != G_2) && (motion != G_3)) && (motion != G_87))
					return ErrorNumberOf("I word on line with no G code (G2, G3, G87) that uses it");
			}


			if (block.j_flag == ON) /* could still be useless if xz_plane arc */
			{
				if (((motion != G_2) && (motion != G_3)) && (motion != G_87))
					return ErrorNumberOf("J word on line with no G code (G2, G3, G87) that uses it");
			}

			if (block.k_flag == ON) /* could still be useless if xy_plane arc */
			{
				if (((motion != G_2) && (motion != G_3)) && (motion != G_87))
					return ErrorNumberOf("K word on line with no G code (G2, G3, G87) that uses it");
			}

			if (block.l_number != -1)
			{
				if (((motion < G_81) || (motion > G_89)) &&
					(block.g_modes[0] != G_10))
					return ErrorNumberOf("L word on line with no canned cycle or G10 to use it");
			}

			if (block.p_number != -1.0)
			{
				if (((block.g_modes[0] != G_10) &&
					(block.g_modes[4] != G_4)) &&
					(((motion != G_82) && (motion != G_86)) &&
					((motion != G_88) && (motion != G_89))))
					return ErrorNumberOf("P word on line with no G code (G4 G10 G82 G86 G88 G89) that uses it");
			}

			if (block.q_number != -1.0)
			{
				if (motion != G_83)
					return ErrorNumberOf("Q word on line with no G83 cycle that uses it");
			}

			if (block.r_flag == ON)
			{
				if (((motion != G_2) && (motion != G_3)) &&
					((motion < G_81) || (motion > G_89)))
					return ErrorNumberOf("R word on line with no G code (arc or cycle) that uses it");
			}

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_control_mode

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		BUG - If g_code isn't G_61 or G_64, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.

		Side effects:
		The interpreter switches the machine settings to indicate the current
		control mode (CANON_MOTION_MODE.EXACT_PATH or CANON_CONTINUOUS).

		The call  SET_MOTION_CONTROL_MODE(CANON_CONTINUOUS) is made if
		the G code is G_64. The call  SET_MOTION_CONTROL_MODE(CANON_MOTION_MODE.EXACT_PATH)
		is made if the G code is G_61.

		We could, alternatively, set the control mode to CANON_EXACT_STOP
		on G_61, and that would correspond more closely to the meaning as
		given in [NCMS, page 40], but CANON_MOTION_MODE.EXACT_PATH has the advantage
		that the tool does not stop if it does not have to, and no evident
		disadvantage compared to CANON_EXACT_STOP, so it is being used.

		Called by: convert_g.

		It is OK to call G_61 when G_61 is already in force, and similarly for
		G_64.

		*/

		int convert_control_mode( /* ARGUMENT VALUES                               */
			int g_code,               /* g_code being executed (must be G_61 or G_64) */
			ref block block,      /* pointer to a block of RS274 instructions     */
			ref setup settings)   /* pointer to machine settings                  */
		{
			if (g_code == G_61)
			{
				 SET_MOTION_CONTROL_MODE(CANON_MOTION_MODE.EXACT_PATH);
				settings.control_mode = CANON_MOTION_MODE.EXACT_PATH;
			}
			else if (g_code == G_64)
			{
				 SET_MOTION_CONTROL_MODE(CANON_MOTION_MODE.CONTINUOUS);
				settings.control_mode = CANON_MOTION_MODE.CONTINUOUS;
			}
			else
				throw new Exception("Code is not G61 or G64 in convert_control_mode");
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_coordinate_system

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occurs, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. The value of the g_code argument is not 540, 550, 560, 570, 580,
			590, 591, 592, or 593

		Side effects:
		If the NGC coordinate system selected by the g_code is not already in
		use, the canonical program coordinate system axis offset values are
		reset and the coordinate values of the current point are reset, and
		a call is made to SET_ORIGIN_OFFSETS.

		Called by: convert_g.

		COORDINATE SYSTEMS (involves g10, g54, g54 - g59.3, g92)

		The canonical machining functions view of coordinate systems is:
		1. There are two coordinate systems: absolute and program.
		2. All coordinate values are given in terms of the program coordinate system.
		3. The offsets of the program coordinate system may be reset.

		The RS274/NGC view of coordinate systems, as given in section 3.2
		of the manual [NCMS] is:
		1. there are ten coordinate systems: absolute and 9 program. The
		program coordinate systems are numbered 1 to 9.
		2. you can switch among the 9 but not to the absolute one. G54
		selects coordinate system 1, G55 selects 2, and so on through
		G56, G57, G58, G59, G59.1, G59.2, and G59.3.
		3. you can set the offsets of the 9 program coordinate systems
		using G10 L2 Pn (n is the number of the coordinate system) with
		values for the axes in terms of the absolute coordinate system.
		4. the first one of the 9 program coordinate systems is the default.
		5. data for coordinate systems is stored in parameters [NCMS, pages 59 - 60].
		6. g53 means to interpret coordinate values in terms of the absolute
		coordinate system for the one block in which g53 appears.
		7. You can offset the current coordinate system using g92. This offset
		will then apply to all nine program coordinate systems.

		The approach used in the interpreter mates the two views
		of coordinate systems as follows:

		During initialization, data from the parameters for the first NGC
		coordinate system is used in a SET_ORIGIN_OFFSETS function call and
		origin_ngc in the machine model is set to 1.

		If a g_code in the range g54 - g59.3 is encountered in an NC program,
		the data from the appropriate NGC coordinate system is copied into the
		origin offsets used by the interpreter, a SET_ORIGIN_OFFSETS function
		call is made, and the current position is reset.

		If a g10 is encountered, the convert_setup function is called to reset
		the offsets of the program coordinate system indicated by the P number
		given in the same block.

		If a g53 is encountered, the axis values given in that block are used
		to calculate what the coordinates are of that point in the current
		coordinate system, and a STRAIGHT_TRAVERSE or STRAIGHT_FEEDfunction
		call to that point using the calculated values is made. No offset
		values are changed.

		If a g92 is encountered, that is handled by the convert_axis_offsets
		function. A g92 results in an axis offset for each axis being calculated
		and stored in the machine model. The axis offsets are applied to all
		nine coordinate systems. Axis offsets are initialized to zero.

		*/

		int convert_coordinate_system( /* ARGUMENT VALUES                         */
			int g_code,              /* g_code called (must be one listed above)     */
			ref block block,     /* pointer to a block of RS274/NGC instructions */
			ref setup settings)  /* pointer to machine settings                  */
		{
			int origin;
			double x;
			double y;
			double z;
			double[] parameters;

			parameters = settings.parameters;
			switch(g_code)
			{
				case 540:
					origin = 1;
					break;
				case 550:
					origin = 2;
					break;
				case 560:
					origin = 3;
					break;
				case 570:
					origin = 4;
					break;
				case 580:
					origin = 5;
					break;
				case 590:
					origin = 6;
					break;
				case 591:
					origin = 7;
					break;
				case 592:
					origin = 8;
					break;
				case 593:
					origin = 9;
					break;
				default:
					throw new Exception("Code is not G54 to G59.3 in convert_coordinate_system");
			}

			if (origin == settings.origin_ngc) /* already using this origin */
			{
				return RS274NGC_OK;
			}

			/* axis offsets could be included in the following calculcations
			but do not need to be because they would not change the result. */
			settings.current_x =
				(settings.current_x + settings.origin_offset_x);
			settings.current_y =
				(settings.current_y + settings.origin_offset_y);
			settings.current_z =
				(settings.current_z + settings.origin_offset_z);

			x = parameters[5201 + (origin * 20)];
			y = parameters[5202 + (origin * 20)];
			z = parameters[5203 + (origin * 20)];

			settings.origin_offset_x = x;
			settings.origin_offset_y = y;
			settings.origin_offset_z = z;

			settings.current_x = (settings.current_x - x);
			settings.current_y = (settings.current_y - y);
			settings.current_z = (settings.current_z - z);
			settings.origin_ngc = origin;

			 SET_ORIGIN_OFFSETS(x + settings.axis_offset_x,
				y + settings.axis_offset_y,
				z + settings.axis_offset_z);

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_cutter_compensation

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. convert_cutter_compensation_on or convert_cutter_compensation_off
			is called and returns RS274NGC_ERROR.
		2. BUG - g_code is not G_40, G_41, or G_42.

		Side effects:
		The value of cutter_radius_compensation in the machine model mode is
		set to CANON_CUTTER_COMP.RIGHT, CANON_CUTTER_COMP.LEFT, or OFF. The currently active tool table index in
		the machine model is updated.

		Since cutter radius compensation is performed in the interpreter, no
		call is made to any canonical function regarding cutter radius compensation.

		Called by: convert_g

		*/

		int convert_cutter_compensation(  /* ARGUMENT VALUES                  */
			int g_code,              /* must be G_40, G_41, or G_42              */
			ref block block,     /* pointer to a block of RS274 instructions */
			ref setup settings)  /* pointer to machine settings              */
		{
			if (g_code == G_40)
			{
				if (convert_cutter_compensation_off(ref settings) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			else if (g_code == G_41)
			{
				if (convert_cutter_compensation_on(CANON_CUTTER_COMP.LEFT, ref block, ref settings)
					== RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			else if (g_code == G_42)
			{
				if (convert_cutter_compensation_on(CANON_CUTTER_COMP.RIGHT, ref block, ref settings)
					== RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			else
				throw new Exception("Code is not G40, G41, or G42 in convert_cutter_compensation");

			return RS274NGC_OK;
		}
		/****************************************************************************/
		/* convert_distance_mode

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		BUG - If g_code isn't G_90 or G_91, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.

		Side effects:
		The interpreter switches the machine settings to indicate the current
		distance mode (absolute or incremental).

		The canonical machine to which commands are being sent does not have
		an incremental mode, so no command setting the distance mode is
		generated in this function. A comment function call explaining the
		change of mode is made (conditionally), however, if there is a change.

		Called by: convert_g.

		*/

		int convert_distance_mode( /* ARGUMENT VALUES                              */
			int g_code,               /* g_code being executed (must be G_90 or G_91) */
			ref block block,      /* pointer to a block of RS274 instructions     */
			ref setup settings)   /* pointer to machine settings                  */
		{
			if (g_code == G_90)
			{
				if (settings.distance_mode !=DISTANCE_MODE.ABSOLUTE)
				{
					settings.distance_mode = DISTANCE_MODE.ABSOLUTE;
				}
			}
			else if (g_code == G_91)
			{
				if (settings.distance_mode != DISTANCE_MODE.INCREMENTAL)
				{
					settings.distance_mode = DISTANCE_MODE.INCREMENTAL;
				}
			}
			else
				throw new Exception("Code is not G90 or G91 in convert_distance_mode");
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_dwell

		Returned Value: int (RS274NGC_OK)

		Side effects:
		A dwell command is executed.

		Called by: convert_g.

		*/

		int convert_dwell( /* ARGUMENT VALUES           */
			double time)      /* time in seconds to dwell  */
		{
			// DWELL(time);
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_length_units

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)

		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. Bug - The g_code argument isnt G_20 or G_21.
		2. Cutter radius compensation is on.

		Side effects:
		A command setting the length units is executed. The machine
		settings are reset regarding length units and current position.

		Called by: convert_g.

		We are not changing tool length offset values or tool diameter values.
		Those values must be given in the table in the correct units. Thus it
		will generally not be feasible to switch units in the middle of a
		program.

		We are not changing the parameters that represent the positions
		of the nine work coordinate systems.

		We are also not changing feed rate values when length units are
		changed, so the actual behavior may change.

		Several other distance items in the settings (such as the various
		parameters for cycles) are also not reset.

		We are changing origin offset and axis offset values, which are
		critical. If this were not done, when length units are set and the new
		length units are not the same as the default length units
		(millimeters), and any XYZ origin or axis offset is not zero, then any
		subsequent change in XYZ origin or axis offset values will be
		incorrect.  Also, g53 (motion in absolute coordinates) will not work
		correctly.

		*/

		int convert_length_units( /* ARGUMENT VALUES                              */
			int g_code,              /* g_code being executed (must be G_20 or G_21) */
			ref setup settings)  /* pointer to machine settings                  */
		{
			if (settings.cutter_radius_compensation != CANON_CUTTER_COMP.OFF)
				return ErrorNumberOf("Cannot change units with cutter radius comp");
			else if (g_code == G_20)
			{
				 USE_LENGTH_UNITS(CANON_UNITS.INCHES);
				if (settings.length_units != CANON_UNITS.INCHES)
				{
					settings.length_units = CANON_UNITS.INCHES;
					settings.current_x = (settings.current_x * INCH_PER_MM);
					settings.current_y = (settings.current_y * INCH_PER_MM);
					settings.current_z = (settings.current_z * INCH_PER_MM);
					settings.axis_offset_x =
						(settings.axis_offset_x * INCH_PER_MM);
					settings.axis_offset_y =
						(settings.axis_offset_y * INCH_PER_MM);
					settings.axis_offset_z =
						(settings.axis_offset_z * INCH_PER_MM);
					settings.origin_offset_x =
						(settings.origin_offset_x * INCH_PER_MM);
					settings.origin_offset_y =
						(settings.origin_offset_y * INCH_PER_MM);
					settings.origin_offset_z =
						(settings.origin_offset_z * INCH_PER_MM);
				}
			}
			else if (g_code == G_21)
			{
				 USE_LENGTH_UNITS(CANON_UNITS.MM);
				if (settings.length_units != CANON_UNITS.MM)
				{
					settings.length_units = CANON_UNITS.MM;
					settings.current_x = (settings.current_x * MM_PER_INCH);
					settings.current_y = (settings.current_y * MM_PER_INCH);
					settings.current_z = (settings.current_z * MM_PER_INCH);
					settings.axis_offset_x =
						(settings.axis_offset_x * MM_PER_INCH);
					settings.axis_offset_y =
						(settings.axis_offset_y * MM_PER_INCH);
					settings.axis_offset_z =
						(settings.axis_offset_z * MM_PER_INCH);
					settings.origin_offset_x =
						(settings.origin_offset_x * MM_PER_INCH);
					settings.origin_offset_y =
						(settings.origin_offset_y * MM_PER_INCH);
					settings.origin_offset_z =
						(settings.origin_offset_z * MM_PER_INCH);
				}
			}
			else
				throw new Exception("Code is not G20 or G21 in convert_length_units");
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_modal_0

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. code is not G_10, G_92, or G92.2
		2. One of the following functions is called and returns an error.
			convert_axis_offsets
			convert_setup

		Side effects:
		A g_code in g modal group 0 is executed (g10, g92, or g92.2)

		Called by: convert_g.

		*/

		int convert_modal_0(     /* ARGUMENT VALUES                              */
			int code,               /* G code, must be G_10, G_92, or G_92_2        */
			ref block block,    /* pointer to a block of RS274/NGC instructions */
			ref setup settings) /* pointer to machine settings                  */
		{
			if (code == G_10)
			{
				if (convert_setup(ref block, ref settings) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			else if ((code == G_92) || (code == G_92_2))
			{
				if (convert_axis_offsets(code, ref block, ref settings) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			else
				throw new Exception("Code is not G10, G92, or G92.2 in convert_modal_0");
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_motion

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. One of the following functions is called and returns RS274NGC_ERROR
			convert_arc
			convert_cycle
			convert_straight
		2. BUG - The motion code is not 0,1,2,3,80,81,82,83,84,85,86,87,88, or 89.
		3. An x, y, or z coordinate value is given with g80.
		4. Inverse time feed mode is on and no feed setting is in the block.
		5. A probe move (G38.2) is called in inverse time feed mode.
		6. A probe move is called with cutter radius compensation on.
		7. A probe move is called with zero feed rate.

		Side effects:
		A g_code from the group causing motion (mode 1) is executed.

		Called by: convert_g.

		Error number 3 above does not prevent the use of coordinate offsets in
		a block where the implicit motion mode is G80 because convert motion
		will not be called in this case (because the motion_mode will be -1).

		*/

		int convert_motion(      /* ARGUMENT VALUES                           */
			int motion,             /* g_code for a line, arc, canned cycle      */
			ref block block,    /* pointer to a block of RS274 instructions  */
			ref setup settings) /* pointer to machine settings               */
		{
			if ((motion == G_0) || (motion == G_1))
			{
				if (convert_straight (motion, ref block, ref settings) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			else if ((motion == G_3) || (motion == G_2))
			{
				if ((settings.feed_mode == INVERSE_TIME) &&
					(block.f_number == -1.0))
					return ErrorNumberOf("F word missing with inverse time arc move");
				if (convert_arc (motion, ref block, ref settings) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			else if (motion == G_38_2)
			{
				if (settings.feed_mode == INVERSE_TIME)
					return ErrorNumberOf("Cannot probe in inverse time feed mode");
				if (settings.cutter_radius_compensation != CANON_CUTTER_COMP.OFF) /* NOT "== ON"! */
					return ErrorNumberOf("Cannot probe with cutter radius compensation on");
				if (settings.feed_rate == 0.0)
					return ErrorNumberOf("Cannot probe with zero feed rate");
				if (convert_probe (ref block, ref settings) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			else if (motion == G_80)
			{
				if((block.x_flag == ON) || (block.y_flag == ON) || (block.z_flag == ON))
					return ErrorNumberOf("Coordinate setting given with G80");
				else
				{
					settings.motion_mode = G_80;
				}
			}
			else if ((motion > G_80) && (motion < G_90))
			{
				if (convert_cycle(motion, ref block, ref settings) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			else
				throw new Exception("Code is not G0 to G3 or G80 to G89 in convert_motion");

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_retract_mode

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. BUG - g_code isn't G_98 or G_99.

		Side effects:
		The interpreter switches the machine settings to indicate the current
		retract mode for canned cycles (OLD_Z or R_PLANE).

		Called by: convert_g.

		The canonical machine to which commands are being sent does not have a
		retract mode, so no command setting the retract mode is generated in
		this function.

		*/

		int convert_retract_mode( /* ARGUMENT VALUES                              */
			int g_code,              /* g_code being executed (must be G_98 or G_99) */
			ref block block,     /* pointer to a block of RS274/NGC instructions */
			ref setup settings)  /* pointer to machine settings                  */
		{
			if (g_code == G_98)
			{
				settings.retract_mode = RETRACT_MODE.OLD_Z;
			}
			else if (g_code == G_99)
			{
				settings.retract_mode = RETRACT_MODE.R_PLANE;
			}
			else
				throw new Exception("Code is not G98 or G99 in convert_retract_mode");
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_set_plane

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. G_18 or G_19 is called when cutter radius compensation is on.
		2. BUG - The g_code is not G_17, G_18, or G_19,

		Side effects:
		A command setting the current plane is executed.

		Called by: convert_g.

		*/

		int convert_set_plane(    /* ARGUMENT VALUES                          */
			int g_code,              /* must be G_17, G_18, or G_19              */
			ref block block,     /* pointer to a block of RS274 instructions */
			ref setup settings)  /* pointer to machine settings              */
		{
			if (g_code == G_17)
			{
				SELECT_PLANE(CANON_PLANE.XY);
				settings.plane = CANON_PLANE.XY;
			}
			else if (g_code == G_18)
			{
				if (settings.cutter_radius_compensation != CANON_CUTTER_COMP.OFF)
					return ErrorNumberOf("Cannot use XZ plane with cutter radius comp");
				SELECT_PLANE(CANON_PLANE.XZ);
				settings.plane = CANON_PLANE.XZ;
			}
			else if (g_code == G_19)
			{
				if (settings.cutter_radius_compensation != CANON_CUTTER_COMP.OFF)
					return ErrorNumberOf("Cannot use YZ plane with cutter radius comp");
				SELECT_PLANE(CANON_PLANE.YZ);
				settings.plane = CANON_PLANE.YZ;
			}
			else
				throw new Exception("Code is not G17, G18, or G19 in convert_set_plane");
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_tool_change

		Returned Value: int (RS274NGC_OK)

		Side effects:
		This makes function calls to primitive machining functions, and sets
		the machine model as described below.

		Called by: convert_m

		This function carries out an m6 command, which changes the tool in the
		spindle. The only function call this makes is to the CHANGE_TOOL
		function. The semantics of this function call is that when it is
		completely carried out, the tool that was selected is in the spindle,
		the tool that was in the spindle (if any) is returned to its changer
		slot, the spindle will be stopped (but the spindle speed setting will
		not have changed) and the x, y, z, and b positions will be the same
		as they were before (although they may have moved around during the
		change).

		It would be nice to add more flexibility to this function by allowing
		more changes to occur (position changes, for example) as a result of
		the tool change. There are at least two ways of doing this:

		1. Require that certain machine settings always have a given fixed
		value after a tool change (which may be different from what the value
		was before the change), and record the fixed values somewhere (in the
		world model that is read at initialization, perhaps) so that this
		function can retrieve them and reset any settings that have changed.
		Fixed values could even be hard coded in this function.

		2. Allow the executor of the CHANGE_TOOL function to change the state
		of the world however it pleases, and have the interpreter read the
		executor's world model after the CHANGE_TOOL function is carried out.
		Implementing this would require a change in other parts of the EMC
		system, since calls to the interpreter would then have to be
		interleaved with execution of the function calls output by the
		interpreter.

		There may be other commands in the block that includes the tool change.
		They will be executed in the order described in execute_block.

		This implements the "Next tool in T word" approach to tool selection.
		The tool is selected when the T word is read (and the carousel may
		move at that time) but is changed when M6 is read.

		Note that if a different tool is put into the spindle, the current_z
		location setting may be incorrect for a time. It is assumed the
		program will contain an appropriate USE_TOOL_LENGTH_OFFSET command
		near the CHANGE_TOOL command, so that the incorrect setting is only
		temporary.

		In [NCMS, page 73, 74] there are three other legal approaches in addition
		to this one.

		*/

		int convert_tool_change(  /* ARGUMENT VALUES                             */
			ref block block,     /* pointer to a block of RS274NGC instructions */
			ref setup settings)  /* pointer to machine settings                 */
		{
			 CHANGE_TOOL(settings.selected_tool_slot);
			settings.current_slot = settings.selected_tool_slot;
			settings.spindle_turning = CANON_DIRECTION.STOPPED;

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_tool_length_offset

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. The block has no offset index (h number).
		2. The h number is bigger than the largest table index.
		3. BUG - The g_code argument is not G_43 or G_49.

		Side effects:
		A USE_TOOL_LENGTH_OFFSET function call is made. Current_z,
		tool_length_offset, and length_offset_index are reset.

		Called by: convert_g

		This is called to execute g43 or g49.

		The g49 RS274/NGC command translates into a USE_TOOL_LENGTH_OFFSET(0.0)
		function call.

		The g43 RS274/NGC command translates into a USE_TOOL_LENGTH_OFFSET(length)
		function call, where length is the value of the entry in the tool length
		offset table whose index is the H number in the block.

		The H number in the block (if present) was checked for being a
		positive integer when it was read, so it that check does not need to
		be repeated.

		*/

		int convert_tool_length_offset( /* ARGUMENT VALUES                       */
			int g_code,             /* g_code being executed (must be G_43 or G_49) */
			ref block block,    /* pointer to a block of RS274/NGC instructions */
			ref setup settings) /* pointer to machine settings                  */
		{
			int index;
			double offset;

			if (g_code == G_49)
			{
				 USE_TOOL_LENGTH_OFFSET(0.0);
				settings.current_z = (settings.current_z +
					settings.tool_length_offset);
				settings.tool_length_offset = 0.0;
				settings.length_offset_index = 0;
			}
			else if (g_code == G_43)
			{
				index = block.h_number;
				if (index == -1)
					return ErrorNumberOf("Offset index missing");
				else if (index >  GET_EXTERNAL_TOOL_MAX())
					return ErrorNumberOf("Tool index out of bounds");
				else
				{
					offset = settings.tool_table[index].length;
					 USE_TOOL_LENGTH_OFFSET(offset);
					settings.current_z =
						(settings.current_z + settings.tool_length_offset - offset);
					settings.tool_length_offset = offset;
					settings.length_offset_index = index;
				}
			}
			else
				throw new Exception("Code is not G43 or G49 in convert_tool_length_offset");
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* read_line_number

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. The first character read is not n (bug).
		2. read_integer_unsigned returns RS274NGC_ERROR.
		3. The line number is too large (more than 5 digits).

		Side effects:
		counter is reset to the character following the line number.
		A line number is inserted in the block.

		Called by: read_items

		When this function is called, counter is pointing at an item on the
		line that starts with the character 'n', indicating a line number.
		The function reads characters which give the (integer) value of the
		line number.

		Note that extra initial zeros in a line number will not cause the
		line number to be too large.

		*/

		int read_line_number( /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274    code being processed  */
			ref int counter,       /* pointer to a counter for position on the line  */
			ref block block) /* pointer to a block being filled from the line  */
		{
			int value;

			if (line[counter] != 'n')
				throw new Exception("Read_line_number should not have been called");

			counter++;
			if (read_integer_unsigned(line, ref counter, out value) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else if (value > 99999)
				return ErrorNumberOf("Line number greater than 99999");
			else
			{
				block.line_number = value;
				return RS274NGC_OK;
			}
		}

		/****************************************************************************/

		/* read_one_item

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If the first character read is not a known character for starting a
		word, or if the reader function which is called returns RS274NGC_ERROR, this
		returns RS274NGC_ERROR. Otherwise, it returns RS274NGC_OK.

		Side effects:
		This function reads one item from a line of RS274/NGC code and inserts
		the information in a block. System parameters may be reset.

		Called by: read_items.

		When this function is called, the counter is set so that the position
		being considered is the first position of a word. The character at
		that position must be one known to the system.  In this version those
		characters are: d,f,g,h,i,j,k,l,m,n,p,q,r,s,t,u,x,y,z,(,#.
		This function does not look for N words because read_items calls
		read_line_number directly.

		The function looks for a letter or special character and calls a
		selected function according to what the letter or character is.  The
		selected function will be responsible to consider all the characters
		that comprise the remainder of the item, and reset the pointer so that
		points to the next character after the end of the item (which may be
		the end of the line or the first character of another item).

		After an item is read, the counter is set at the index of the
		next unread character. The item data is stored in the block.

		It is expected that the format of a comment will have been checked;
		this is being done by close_and_downcase. Bad format comments will
		have prevented the system from getting this far, so that this function
		can assume a close parenthesis will be found when an open parenthesis
		has been found, and that comments are not nested.

		*/

		// typedef int (*_function_pointer) (char *, int *, ref block, double *);

		int read_one_item(    /* ARGUMENT VALUES                                */
			string line,         /* string: line of RS274/NGC code being processed */
			ref int counter,       /* pointer to a counter for position on the line  */
			ref block block, /* pointer to a block being filled from the line  */
			ref double[] parameters) /* array of system parameters                     */
		{
			int s;
			switch (line[counter])
			{
				case 'd':
					s = read_d(line, ref counter, ref block, ref parameters);
					break;
				case 'f':
					s = read_f(line, ref counter, ref block, ref parameters);
					break;
				case 'g':
					s = read_g(line, ref counter, ref block, ref parameters);
					break;
				case 'h':
					s = read_tool_length_offset(line, ref counter, ref block, ref parameters);
					break;
				case 'i':
					s = read_i(line, ref counter, ref block, ref parameters);
					break;
				case 'j':
					s = read_j(line, ref counter, ref block, ref parameters);
					break;
				case 'k':
					s = read_k(line, ref counter, ref block, ref parameters);
					break;
				case 'l':
					s = read_l(line, ref counter, ref block, ref parameters);
					break;
				case 'm':
					s = read_m(line, ref counter, ref block, ref parameters);
					break;
				case 'p':
					s = read_p(line, ref counter, ref block, ref parameters);
					break;
				case 'q':
					s = read_q(line, ref counter, ref block, ref parameters);
					break;
				case 'r':
					s = read_r(line, ref counter, ref block, ref parameters);
					break;
				case 's':
					s = read_spindle_speed(line, ref counter, ref block, ref parameters);
					break;
				case 't':
					s = read_tool(line, ref counter, ref block, ref parameters);
					break;
				case 'x':
					s = read_x(line, ref counter, ref block, ref parameters);
					break;
				case 'y':
					s = read_y(line, ref counter, ref block, ref parameters);
					break;
				case 'z':
					s = read_z(line, ref counter, ref block, ref parameters);
					break;
				case '(':
					s = read_comment(line, ref counter, ref block, ref parameters);
					break;
				case '#':
					s = read_parameter_setting(line, ref counter, ref block, ref parameters);
					break;
				default:
					return ErrorNumberOf("Bad character used");
			}

			if (s == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* check_items

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR
		Otherwise, it returns RS274NGC_OK.
		1. check_g_codes return RS274NGC_ERROR.
		2. check_m_codes returns RS274NGC_ERROR.
		3. check_other_codes returns RS274NGC_ERROR.
		4. A G38.2 (probing) is used with an M code for stopping.

		Side effects: none

		Called by: read_line

		This runs checks on a block of RS274 code.

		The functions named read_XXXX check for errors which would foul up the
		reading. This function checks for additional logical errors.

		A block has an array of g_codes, which are initialized to -1
		(meaning no code). This calls check_g_codes to check the g_codes.

		A block has an array of m_codes, which are initialized to -1
		(meaning no code). This calls check_m_codes to check the m_codes.

		Items in the block which are not m or g codes are checked by
		check_other_codes.

		*/

		int check_items(          /* ARGUMENT VALUES                  */
			ref block block,     /* pointer to a block to be checked */
			ref setup settings)  /* pointer to machine settings      */
		{
			if (check_g_codes(ref block, ref settings) != RS274NGC_OK)
				return RS274NGC_ERROR;
			if (check_m_codes(ref block, ref settings) != RS274NGC_OK)
				return RS274NGC_ERROR;
			if (check_other_codes(ref block, ref settings) != RS274NGC_OK)
				return RS274NGC_ERROR;
			if ((block.m_modes[4] != -1) && (block.g_modes[1] == G_38_2))
				return ErrorNumberOf("Cannot put an M code for stopping with G38.2");
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* close_and_downcase

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If one of the following errors occurs, this returns RS274NGC_ERROR
		Otherwise, it returns RS274NGC_OK.
		1. A left parenthesis is found inside a comment.
		2. The line ends before an open comment is closed
		3. A newline character is found that is not followed by null
		4. The input line was too long

		Side effects:
		see below.

		Called by:
		read_text

		To simplify handling upper case letters, spaces, and tabs, this
		function removes spaces and and tabs and downcases everything on a
		line which is not part of a comment.

		Comments are left unchanged in place. Comments are anything
		enclosed in parentheses. Nested comments, indicated by a left
		parenthesis inside a comment, are illegal.

		The line must have a null character at the end when it comes in.
		The line may have one newline character just before the end. If
		there is a newline, it will be removed.

		Although this software system detects and rejects all illegal characters
		and illegal syntax, this particular function does not detect problems
		with anything but comments.

		We are treating RS274 code here as case-insensitive and spaces and
		tabs as if they have no meaning. RS274D, page 6 says spaces and tabs
		are to be ignored by control.

		The manual [NCMS] says nothing about case or spaces and tabs.

		*/

		int close_and_downcase( /* ARGUMENT VALUES             */
			ref string line)           /* string: one line of NC code */
		{
			int m;
			bool comment = false;
			char item;
			StringBuilder newLine = new StringBuilder(line.Length);
			
			for (m = 0; m < line.Length; m++)
			{
				item = line[m];
				if (comment)
				{
					newLine.Append(item);
					if (item == ')')
					{
						comment = false;
					}
					else if (item == '(')
						return ErrorNumberOf("Nested comment found");
				}
				else if ((item == '\n') || (item == ' ') || (item == '\t') || (item == '\r')) 
					/* do nothing */;
				else if ((64 < item) && (item < 91)) /* downcase upper case letters */
				{
					newLine.Append(char.ToLower(item));
				}
				else if (item == '(')   /* comment is starting */
				{
					comment = true;
					newLine.Append(item);
				}
				else
				{
					newLine.Append(item); /* copy anything else */
				}
			}
			if (comment)
				return ErrorNumberOf("Unclosed comment found");
			
			line = newLine.ToString();
			
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_comment

		Returned Value: int (RS274NGC_OK)

		Side effects:
		The message function is called if the string starts with "MSG,".
		Otherwise, the comment function is called.

		Called by: execute_block

		To be a message, the first four characters of the comment after the
		opening left parenthesis must be "MSG,", ignoring the case of the
		letters and allowing spaces or tabs anywhere before the comma (to make
		the treatment of case and white space consistent with how it is
		handled elsewhere).

		Messages are not provided for in [NCMS]. They are implemented here as a
		subtype of comment.

		*/

		int convert_comment( /*ARGUMENT VALUES      */
			string comment)     /* string with comment */
		{
			int m;
			int item;

			for (m = 0; ((item = comment[m]) == ' ') || (item == '\t') ; m++);
			if ((item != 'M') && (item != 'm'))
			{
				 COMMENT(comment);
				return RS274NGC_OK;
			}
			for (m++; ((item = comment[m]) == ' ') || (item == '\t') ; m++);
			if ((item != 'S') && (item != 's'))
			{
				 COMMENT(comment);
				return RS274NGC_OK;
			}
			for (m++; ((item = comment[m]) == ' ') || (item == '\t') ; m++);
			if ((item != 'G') && (item != 'g'))
			{
				 COMMENT(comment);
				return RS274NGC_OK;
			}
			for (m++; ((item = comment[m]) == ' ') || (item == '\t') ; m++);
			if (item != ',')
			{
				 COMMENT(comment);
				return RS274NGC_OK;
			}
			 MESSAGE(comment + m + 1);
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_feed_mode

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		BUG - If g_code isn't G_93 or G_94, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.

		Side effects:
		The interpreter switches the machine settings to indicate the current
		feed mode (UNITS_PER_MINUTE or INVERSE_TIME).

		The canonical machine to which commands are being sent does not have
		a feed mode, so no command setting the distance mode is generated in
		this function. A comment function call is made (conditionally)
		explaining the change in mode, however.

		Called by: execute_block.

		*/

		int convert_feed_mode(     /* ARGUMENT VALUES                              */
			int g_code,               /* g_code being executed (must be G_93 or G_94) */
			ref block block,      /* pointer to a block of RS274 instructions     */
			ref setup settings)   /* pointer to machine settings                  */
		{
			if (g_code == G_93)
			{
				settings.feed_mode = INVERSE_TIME;
			}
			else if (g_code == G_94)
			{
				settings.feed_mode = UNITS_PER_MINUTE;
			}
			else
				throw new Exception("Code is not G93 or G94 in convert_feed_mode");
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_feed_rate

		Returned Value: int (RS274NGC_OK)

		Side effects:
		The machine feed_rate is set to the value of f_number in the
			block by function call.
		The machine model feed_rate is set to that value.

		Called by: execute_block

		*/

		int convert_feed_rate(   /* ARGUMENT VALUES                          */
			ref block block,    /* pointer to a block of RS274 instructions */
			ref setup settings) /* pointer to machine settings              */
		{
			 SET_FEED_RATE(block.f_number);
			settings.feed_rate = block.f_number;
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_g

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occurs, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.
		1. A g code in the block is not known to the system.
		2. One of the following functions is called and returns error:
			convert_control_mode
			convert_coordinate_system
			convert_cutter_compensation
			convert_distance_mode
			convert_length_units
			convert_modal_0
			convert_motion
			convert_retract_mode
			convert_set_plane
			convert_tool_length_offset

		Side effects:
		Any g_codes in the block (excluding g93 and 94) and any implicit
		motion g_code are executed.

		Called by: execute_block.

		This takes a pointer to a block of RS274/NGC instructions (already
		read in) and creates the appropriate output commands corresponding to
		any "g" codes in the block.

		Codes g93 and g94, which set the feed mode, are executed earlier by
		execute_block before reading the feed rate.

		G codes are are executed in the following order.
		1. mode 4, (G4) - dwell. G53 is also in mode 4 but is not executed
		here.
		2. mode 2, one of (G17, G18, G19) - plane selection.
		3. mode 6, one of (G20, G21) - length units.
		4. mode 7, one of (G40, G41, G42) - cutter radius compensation.
		5. mode 8, one of (G43, G49) - tool length offset
		6. mode 12, one of (G54, G55, G56, G57, G58, G59, G59.1, G59.2, G59.3)
		- coordinate system selection.
		7. mode 13, one of (G61, G64) - control mode (exact path or continuous)
		8. mode 3, one of (G90, G91) - distance mode.
		9. mode 10, one of (G98, G99) - retract mode.
		10. mode 0 (G10, G92, G92.2) - setting coordinate system locations, setting
			or cancelling axis offsets.
		11. mode 1, one of (G0, G1, G2, G3, G38.2, G80, G81 to G89) - motion or cancel.

		The mode 0 and mode 1 G codes must be executed after the length units
		are set, since they use coordinate values. Mode 1 codes also must wait
		until most of the other modes are set.

		The mode 0 and mode 1 G codes are not quite mutually exclusive because
		G80 is in the mode 1 group. check_g_codes prevents competition between
		the mode 0 and mode 1 G codes for the use of axis values. If a mode 0
		G code is used, only G80 from the mode 1 group is possible.

		Mode 4 codes (G4 and G53) can coexist with mode 1 codes.

		*/

		int convert_g(           /* ARGUMENT VALUES                              */
			ref block block,    /* pointer to a block of RS274/NGC instructions */
			ref setup settings) /* pointer to machine settings                  */
		{
			if (block.g_modes[4] == G_4)
			{
				if (convert_dwell(block.p_number) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}

			if (block.g_modes[2] != -1)
			{
				if (convert_set_plane(block.g_modes[2], ref block, ref settings)
					== RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}

			if (block.g_modes[6] != -1)
			{
				if (convert_length_units(block.g_modes[6], ref settings) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}

			if (block.g_modes[7] != -1)
			{
				if (convert_cutter_compensation(block.g_modes[7], ref block, ref settings)
					== RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}

			if (block.g_modes[8] != -1)
			{
				if (convert_tool_length_offset(block.g_modes[8], ref block, ref settings)
					== RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}

			if (block.g_modes[12] != -1)
			{
				if (convert_coordinate_system (block.g_modes[12], ref block, ref settings)
					== RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}

			if (block.g_modes[13] != -1)
			{
				if (convert_control_mode (block.g_modes[13], ref block, ref settings)
					== RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}

			if (block.g_modes[3] != -1)
			{
				if (convert_distance_mode(block.g_modes[3], ref block, ref settings)
					== RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}

			if (block.g_modes[10] != -1)
			{
				if (convert_retract_mode(block.g_modes[10], ref block, ref settings)
					== RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}

			if (block.g_modes[0] != -1)
			{
				if (convert_modal_0(block.g_modes[0], ref block, ref settings)
					== RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}

			if (block.motion_to_be != -1)
			{
				if (convert_motion(block.motion_to_be, ref block, ref settings)
					== RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_m

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If convert_tool_change is returns RS274NGC_ERROR, this
		returns RS274NGC_ERROR. Otherwise, it returns RS274NGC_OK.

		Side effects:
		m_codes in the block are executed. For each m_code
		this consists of making a function call to a primitive machining
		function and setting the machine model.

		Called by: execute_block.

		This handles the following types of activity in order:
		1. changing the tool (m6) - which also retracts and stops the spindle.
		2. Turning the spindle on or off (m3, m4, and m5)
		3. Turning coolant on and off (m7, m8, and m9)
		4. enabling or disabling feed and speed overrides (m49, m49).
		Within each group, only the first code encountered will be executed.

		This is called if any m_code is programmed, but does nothing with m0,
		m1, m2, m30, or m60 (which are handled in convert_stop).

		*/

		int convert_m(           /* ARGUMENT VALUES                              */
			ref block block,    /* pointer to a block of RS274/NGC instructions */
			ref setup settings) /* pointer to machine settings                  */
		{
			if (block.m_modes[6] != -1)
				if (convert_tool_change(ref block, ref settings) == RS274NGC_ERROR)
					return RS274NGC_ERROR;

			if (block.m_modes[7] == 3)
			{
				 START_SPINDLE_CLOCKWISE();
				settings.spindle_turning = CANON_DIRECTION.CLOCKWISE;
			}
			else if (block.m_modes[7] == 4)
			{
				 START_SPINDLE_COUNTERCLOCKWISE();
				settings.spindle_turning = CANON_DIRECTION.COUNTERCLOCKWISE;
			}
			else if (block.m_modes[7] == 5)
			{
				 STOP_SPINDLE_TURNING();
				settings.spindle_turning = CANON_DIRECTION.STOPPED;
			}

			if (block.m_modes[8] == 7)
			{
				 MIST_ON();
				settings.mist = ON;
			}
			else if (block.m_modes[8] == 8)
			{
				 FLOOD_ON();
				settings.flood = ON;
			}
			else if (block.m_modes[8] == 9)
			{
				 MIST_OFF();
				settings.mist = OFF;
				 FLOOD_OFF();
				settings.flood = OFF;
			}

			if (block.m_modes[9] == 48)
			{
				 ENABLE_FEED_OVERRIDE();
				 ENABLE_SPEED_OVERRIDE();
				settings.feed_override = ON;
				settings.speed_override = ON;
			}
			else if (block.m_modes[9] == 49)
			{
				 DISABLE_FEED_OVERRIDE();
				 DISABLE_SPEED_OVERRIDE();
				settings.feed_override = OFF;
				settings.speed_override = OFF;
			}
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_speed

		Returned Value: int (RS274NGC_OK)

		Side effects:
		The machine spindle speed is set to the value of s_number in the
		block by function call.
		The machine model for spindle speed is set to that value.

		Called by: execute_block.

		*/

		int convert_speed(       /* ARGUMENT VALUES                          */
			ref block block,    /* pointer to a block of RS274 instructions */
			ref setup settings) /* pointer to machine settings              */
		{
			 SET_SPINDLE_SPEED(block.s_number);
			settings.speed = block.s_number;
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_stop

		Returned Value: int (RS274NGC_OK or RS274NGC_EXIT)
		When an m2 or m30 (program_end) is encountered, this returns RS274NGC_EXIT.
		BUG - If the code is not m0, m1, m2, m30, or m60, this
		returns RS274NGC_ERROR. Otherwise, it returns RS274NGC_OK.

		Side effects:
		An m0, m1, m2, m30, or m60 in the block is executed.
		This consists of making a function call to a primitive
		machining function.

		For m0, m1, and m60, this makes a function call to a canonical
		machining function that stops the   In addition, m60 calls
		PALLET_SHUTTLE.

		For m2 and m30, this resets the machine and then calls PROGRAM_END.
		In addition, m30 calls PALLET_SHUTTLE.

		Called by: execute_block.

		This handles stopping or ending the program (m0, m1, m2, m30, m60)

		[NCMS] specifies how the following modes should be reset at m2 or
		m30. The descriptions are not collected in one place, so this list
		may be incomplete.

		G52 offsetting coordinate zero points [NCMS, page 10]
		G92 coordinate offset using tool position [NCMS, page 10]

		The following should have reset values, but no description of reset
		behavior could be found in [NCMS].
		G17, G18, G19 selected plane [NCMS, pages 14, 20]
		G90, G91 distance mode [NCMS, page 15]
		G93, G94 feed mode [NCMS, pages 35 - 37]
		M48, M49 overrides enabled, disabled [NCMS, pages 37 - 38]
		M3, M4, M5 spindle turning [NCMS, page 7]

		The following should be set to some value at machine start-up but
		not automatically reset by any of the stopping codes.
		1. G20, G21 length units [NCMS, page 15]. This is up to the installer.
		2. motion_control_mode. This is set in rs274ngc_init but not reset here.
		Might add it here.

		The following resets have been added by calling the appropriate
		canonical machining command and/or by resetting interpreter
		settings. They occur on M2 or M30.

		1. Axis offsets are set to zero (like g92.2) and      - SET_ORIGIN_OFFSETS
		origin offsets are set to the default (like G54)
		2. Selected plane is set to CANON_PLANE.XY (like G17) - SELECT_PLANE
		3. Distance mode is set to ABSOLUTE (like G90)        - no canonical call
		4. Feed mode is set to UNITS_PER_MINUTE (like G94)    - no canonical call
		5. Feed and speed overrides are set to ON (like G48)  - ENABLE_FEED_OVERRIDE
															- ENABLE_SPEED_OVERRIDE
		6. Cutter compensation is turned off (like G40)       - no canonical call
		7. The spindle is stopped (like M5)                   - STOP_SPINDLE_TURNING
		8. The motion mode is set to G_1 (like G1)            - no canonical call
		9. Coolant is turned off (like M9)                    - FLOOD_OFF & MIST_OFF

		*/

		int convert_stop(         /* ARGUMENT VALUES                              */
			ref block block,     /* pointer to a block of RS274/NGC instructions */
			ref setup settings)  /* pointer to machine settings                  */
		{
			if (block.m_modes[4] == 0)
			{
				 PROGRAM_STOP();
			}
			else if (block.m_modes[4] == 60)
			{
				 PALLET_SHUTTLE();
				 PROGRAM_STOP();
			}
			else if (block.m_modes[4] == 1)
			{
				 OPTIONAL_PROGRAM_STOP();
			}
			else if ((block.m_modes[4] == 2) || (block.m_modes[4] == 30))
			{ /* reset stuff here */
				/*1*/
				settings.current_x = settings.current_x
					+ settings.origin_offset_x;
				settings.current_y = settings.current_y
					+ settings.origin_offset_y;
				settings.current_z = settings.current_z
					+ settings.origin_offset_z;
				settings.origin_offset_x = settings.parameters[5221];
				settings.origin_offset_y = settings.parameters[5222];
				settings.origin_offset_z = settings.parameters[5223];
				settings.current_x = settings.current_x -
					settings.origin_offset_x;
				settings.current_y = settings.current_y -
					settings.origin_offset_y;
				settings.current_z = settings.current_z -
					settings.origin_offset_z;
				 SET_ORIGIN_OFFSETS(settings.origin_offset_x + settings.axis_offset_x,
					settings.origin_offset_y + settings.axis_offset_y,
					settings.origin_offset_z + settings.axis_offset_z);

				/*2*/ if (settings.plane != CANON_PLANE.XY)
					  {
						   SELECT_PLANE(CANON_PLANE.XY);
						  settings.plane = CANON_PLANE.XY;
					  }

				/*3*/ settings.distance_mode = DISTANCE_MODE.ABSOLUTE;

				/*4*/ settings.feed_mode = UNITS_PER_MINUTE;

				/*5*/ if (settings.feed_override != ON)
					  {
						   ENABLE_FEED_OVERRIDE();
						  settings.feed_override = ON;
					  }
				if (settings.speed_override != ON)
				{
					 ENABLE_SPEED_OVERRIDE();
					settings.speed_override = ON;
				}

				/*6*/ settings.cutter_radius_compensation = CANON_CUTTER_COMP.OFF;
				settings.program_x = UNKNOWN;

				/*7*/  STOP_SPINDLE_TURNING();
				settings.spindle_turning = CANON_DIRECTION.STOPPED;

				/*8*/ settings.motion_mode = G_1;

				/*9*/ if (settings.mist == ON)
					  {
						   MIST_OFF();
						  settings.mist = OFF;
					  }
				if (settings.flood == ON)
				{
					 FLOOD_OFF();
					settings.flood = OFF;
				}

				if (block.m_modes[4] == 30)
					 PALLET_SHUTTLE();
				 PROGRAM_END();
				return RS274NGC_EXIT;
			}
			else
				throw new Exception("Code is not M0, M1, M2, M30 or M60 in convert_stop");
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* convert_tool_select

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If the tool number in the block is out of bounds, this
		returns RS274NGC_ERROR. Otherwise, it returns RS274NGC_OK.

		Side effects:
		A select tool command is given, which causes the changer chain
		to move so that the selected slot is next to the tool changer,
		ready for a tool change. The settings.selected_tool_slot is set
		to the t number in the block.

		Called by: execute_block.

		*/

		int convert_tool_select( /* ARGUMENT VALUES                                 */
			ref block block,    /* pointer to a block of RS274/NGC instructions    */
			ref setup settings) /* pointer to machine settings                     */
		{
			if (block.t_number >  GET_EXTERNAL_TOOL_MAX())
				return ErrorNumberOf("Selected tool slot number too large");

			 SELECT_TOOL(block.t_number);
			settings.selected_tool_slot = block.t_number;
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* init_block

		Returned Value: int (RS274NGC_OK)

		Side effects:
		Values in the block are reset as described below.

		Called by: read_line

		This system reuses the same block over and over, rather than building
		a new one for each line of NC code. The block is re-initialized before
		each new line of NC code is read.

		The block contains many slots for values which may or may not be present
		on a line of NC code. For some of these slots, there is a flag which
		is turned on (at the time time value of the slot is read) if the item
		is present.  For slots whose values are to be read which do not have a
		flag, there is always some excluded range of values. Setting the
		initial value of these slot to some number in the excluded range
		serves to show that a value for that slot has not been read.

		The rules for the indicators for slots whose values may be read are:
		1. If the value may be an arbitrary real number (which is always stored
		internally as a double), a flag is needed to indicate if a value has
		been read. All such flags are initialized to OFF.
		Note that the value itself is not initialized; there is no point in it.
		2. If the value must be a non-negative real number (which is always stored
		internally as a double), a value of -1.0 indicates the item is not present.
		3. If the value must be an unsigned integer (which is always stored
		internally as an int), a value of -1 indicates the item is not present.
		(RS274/NGC does not use any negative integers.)
		4. If the value is a character string (only the comment slot is one), the
		first character is set to 0 (null).

		*/

		int init_block(        /* ARGUMENT VALUES                               */
			ref block block)  /* pointer to a block to be initialized or reset */
		{
			int n;
			block.comment = "";
			block.d_number = -1;
			block.f_number = -1.0;
			block.g_count = 0;
			for (n = 0; n < 14; n++)
			{
				block.g_modes[n] = -1;
			}
			block.h_number = -1;
			block.i_flag = OFF;
			block.j_flag = OFF;
			block.k_flag = OFF;
			block.l_number = -1;
			block.line_number = -1;
			block.motion_to_be = -1;
			block.m_count = 0;
			for (n = 0; n < 10; n++)
			{
				block.m_modes[n] = -1;
			}
			block.p_number = -1.0;
			block.q_number = -1.0;
			block.r_flag = OFF;
			block.s_number = -1.0;
			block.t_number = -1;
			block.x_flag = OFF;
			block.y_flag = OFF;
			block.z_flag = OFF;

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* read_items

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If read_line_number or read_one_item returns RS274NGC_ERROR,
		this returns RS274NGC_ERROR. Otherwise, it returns RS274NGC_OK.

		Side effects:
		One line of RS274 code is read and data inserted into a block.
		The counter which is passed around among the readers is initialized.
		System parameters may be reset.

		Called by: read_line

		*/

		int read_items(			/* ARGUMENT VALUES                                */
			ref block block,	/* pointer to a block being filled from the line  */
			string line,        /* string: line of RS274/NGC code being processed */
			ref double[] parameters) /* array of system parameters                     */
		{
			int counter;
			int length;

			length = line.Length;
			counter = 0;

			if (line[counter] == '/')
				counter++;
			if (line[counter] == 'n')
			{
				if (read_line_number(line, ref counter, ref block) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			for ( ; counter < length; )
			{
				if (read_one_item (line, ref counter, ref block, ref parameters) != RS274NGC_OK)
//	was	+++		if (read_one_item (line, ref counter, ref block, ref parameters) == RS274NGC_ERROR)
						return RS274NGC_ERROR;
			}
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* utility_enhance_block

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If
		this returns RS274NGC_ERROR. Otherwise, it returns RS274NGC_OK.

		Side effects:
		The value of motion_to_be in the block is set.

		Called by: read_line

		It does not seem practical to change x and y to absolute coordinates
		here because they are handled differently according to whether cutter
		radius compensation is on or not.

		*/

		int utility_enhance_block( /* ARGUMENT VALUES                   */
			ref block block,      /* pointer to a block to be checked  */
			ref setup settings)   /* pointer to machine settings       */
		{
			if (block.g_modes[1] != -1)
				block.motion_to_be = block.g_modes[1];
			else if ((((block.x_flag == ON) || (block.y_flag == ON)) ||
				(block.z_flag == ON)) &&
				(block.g_modes[0] == -1))
				block.motion_to_be = settings.motion_mode;
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* write_g_codes

		Returned Value: int (RS274NGC_OK)

		Side effects:
		The gez struct is updated.

		Called by:
		rs274ngc_execute
		rs274ngc_init

		The block may be null.

		This writes active g_codes into the gez structure by examining the
		interpreter settings. The gez structure array of actives is composed
		of ints, so (to handle codes like 59.1) all g_codes are is reported as
		ints ten times the actual value. For example, 59.1 is reported as 591.

		Note: the recent_gees_pointer is no longer an array with the history.
		It's only a single group of the most recent ones.

		*/

		int write_g_codes(        /* ARGUMENT VALUES                              */
			ref block block,     /* pointer to a block of RS274/NGC instructions */
			ref setup settings,  /* pointer to machine settings                  */
			int line_number,         /* number of line being executed                */
			ref int[] gez)               /* pointer to array of G code numbers           */
		{
			gez[0] = line_number;            /* 0 line number     */
			gez[1] = settings.motion_mode;  /* 1 motion mode     */
			gez[2] =
				(block == null)             ? -1                :
				(block.g_modes[0] != -1) ? block.g_modes[0] :  /* 2 g10,g92         */
				block.g_modes[4] ;  /* 2 g4, g53         */
			gez[3] =
				(settings.plane == CANON_PLANE.XY) ? G_17 :       /* 3 active plane    */
				(settings.plane == CANON_PLANE.XZ) ? G_18 : G_19;
			gez[4] =                         /* 4 cut radius comp */
				(settings.cutter_radius_compensation == CANON_CUTTER_COMP.RIGHT) ? G_42 :
				(settings.cutter_radius_compensation == CANON_CUTTER_COMP.LEFT) ? G_41 : G_40;
			gez[5] =                         /* 5 length units    */
				(settings.length_units == CANON_UNITS.INCHES) ? G_20 : G_21;
			gez[6] =                         /* 6 distance mode   */
				(settings.distance_mode == DISTANCE_MODE.ABSOLUTE) ? G_90 : G_91;
			gez[7] =                         /* 7 feed mode       */
				(settings.feed_mode == INVERSE_TIME) ? G_93 : G_94;
			gez[8] =                         /* 8 coord system    */
				(settings.origin_ngc < 7) ? (530 + (10 * settings.origin_ngc)) :
				(584 + settings.origin_ngc);
			gez[9] =                         /* 9 tool len offset */
				(settings.tool_length_offset == 0.0) ? G_49 : G_43;
			gez[10] =                        /* 10 retract mode   */
				(settings.retract_mode == RETRACT_MODE.OLD_Z) ? G_98 : G_99;
			gez[11] =                        /* 11 control mode   */
				(settings.control_mode == CANON_MOTION_MODE.CONTINUOUS) ? G_64 : G_61;

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* write_m_codes

		Returned Value: int (RS274NGC_OK)

		Side effects:
		The array of ints is updated.

		Called by:
		rs274ngc_execute
		rs274ngc_init

		Note that this no longer maintains a history, and just returns the current
		M code settings.

		*/

		int write_m_codes(       /* ARGUMENT VALUES                              */
			ref block block,    /* pointer to a block of RS274/NGC instructions */
			ref setup settings, /* pointer to machine settings                  */
			int line_number,        /* number of line being executed                */
			ref int[] ems)              /* pointer to array of M code numbers           */
		{
			ems[0] = line_number;                           /* 0 line number */
			ems[1] =
				(block == null) ? -1 : block.m_modes[4];          /* 1 stopping    */
			ems[2] =
				(settings.spindle_turning == CANON_DIRECTION.STOPPED) ? 5 : /* 2 spindle     */
				(settings.spindle_turning == CANON_DIRECTION.CLOCKWISE) ? 3 : 4;
			ems[3] =                                        /* 3 tool change */
				(block == null) ? -1 : block.m_modes[6];
			ems[4] =                                        /* 4 mist        */
				(settings.mist == ON) ? 7 :
				(settings.flood == ON) ? -1 : 9;
			ems[5] =                                        /* 5 flood       */
				(settings.flood == ON) ? 8 : -1;
			ems[6] =                                        /* 6 overrides   */
				(settings.feed_override == ON) ? 48 : 49;         /* both overrides*/

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* write_settings

		Returned Value: int (RS274NGC_OK)

		Side effects:
		The array of doubles is updated with the F and S code settings

		Called by:
		rs274ngc_execute
		rs274ngc_init
		*/

		int write_settings(      /* ARGUMENT VALUES                              */
			ref block block,    /* pointer to a block of RS274/NGC instructions */
			ref setup settings, /* pointer to machine settings                  */
			int line_number,        /* number of line being executed                */
			ref double[] vals)          /* pointer to array of M code numbers           */
		{
			vals[0] = line_number;   /* 0 line number */
			vals[1] = settings.feed_rate; /* 1 feed rate */
			vals[2] = settings.speed; /* 2 spindle speed */

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* execute_block

		Returned Value: int (RS274NGC_OK, RS274NGC_EXIT, RS274NGC_EXECUTE_FINISH,
							or RS274NGC_ERROR)
		If any function called returns RS274NGC_ERROR, this returns RS274NGC_ERROR.
		Else if convert_stop returns RS274NGC_EXIT, this returns RS274NGC_EXIT.
		Else if the probe_flag in the settings is ON, this returns
			RS274NGC_EXECUTE_FINISH
		Otherwise, it returns RS274NGC_OK.

		Side effects:
		One block of RS274/NGC instructions is executed.

		Called by:
		rs274ngc_execute

		This converts a block to zero to many actions. The order of execution
		of items in a block is critical to safe and effective machine
		operation, but is not specified clearly in the [NCMS] documentation.

		Actions are executed in the following order:
		1. any comment.
		2. a feed mode setting (g93, g94)
		3. a feed rate (f) setting if in units_per_minute feed mode.
		4. a spindle speed (s) setting.
		5. a tool selection (t).
		6. "m" commands as described in convert_m (includes tool change).
		7. any g_codes (except g93, g94) as described in convert_g.
		8. stopping commands (m0, m1, m2, m30, or m60).

		In inverse time feed mode, the explicit and implicit g code executions
		include feed rate setting with g1, g2, and g3. Also in inverse time
		feed mode, attempting a canned cycle cycle (g81 to g89) or setting a
		feed rate with g0 is illegal and will be detected and result in an
		error message.

		*/

		int execute_block(         /* ARGUMENT VALUES                              */
			ref block block,      /* pointer to a block of RS274/NGC instructions */
			ref setup settings,   /* pointer to machine settings                  */
			int line_number)          /* number of line being executed                */
		{
			if (block.comment.Length > 0)
			{
				if (convert_comment(block.comment) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			if (block.g_modes[5] != -1)
			{
				if (convert_feed_mode(block.g_modes[5], ref block, ref settings)
					== RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			if (block.f_number > -1.0)
			{
				if (settings.feed_mode == INVERSE_TIME)
					;/* handle elsewhere */
				else if (convert_feed_rate(ref block, ref settings) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			if (block.s_number > -1.0)
			{
				if (convert_speed(ref block, ref settings) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			if (block.t_number != -1)
			{
				if (convert_tool_select(ref block, ref settings) == RS274NGC_ERROR)
					return RS274NGC_ERROR;
			}
			if (convert_m(ref block, ref settings) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			if (convert_g(ref block, ref settings) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			if (block.m_modes[4] != -1) /* converts m0, m1, m2, m30, or m60 */
			{
				if (convert_stop(ref block, ref settings) == RS274NGC_EXIT)
					return RS274NGC_EXIT;
			}
			if (settings.probe_flag == ON)
				return RS274NGC_EXECUTE_FINISH;
			else
				return RS274NGC_OK;
		}

		/****************************************************************************/

		/* read_line

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the calls to init_block, read_items, utility_enhance_block,
		or check_items returns RS274NGC_ERROR, this returns RS274NGC_ERROR.
		Otherwise, it returns RS274NGC_OK.

		Side effects:
		One RS274 line is read into a block and the block is checked for
		errors. System parameters may be reset.

		Called by:
		rs274ngc_execute
		rs274ngc_read

		*/

		int read_line(           /* ARGUMENT VALUES                      */
			string line,            /* array holding a line of RS274 code   */
			ref block block,    /* pointer to a block to be filled      */
			ref setup settings) /* pointer to machine settings          */
		{
			if (init_block(ref block) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			if (read_items(ref block, line, ref settings.parameters) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			if (utility_enhance_block(ref block, ref settings) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			if (check_items(ref block, ref settings) == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* read_text

		Returned Value: int - (RS274NGC_OK or RS274NGC_ERROR)
		If close_and_downcase returns RS274NGC_ERROR, this returns RS274NGC_ERROR;
		if the file end is found, this returns RS274NGC_ENDFILE.
		Otherwise, it returns RS274NGC_OK.

		Side effects:
		The value of the length argument is set to the number of
		characters on the reduced line. The line is written into.

		Called by:
		rs274ngc_execute
		rs274ngc_read

		This reads a line of RS274 code from a command string or a file into
		the line array. If the string is not null, the file is ignored.

		If the end of file is reached, that means that the file has run out
		without a stopping command having been given (a stopping command
		causes other functions to close the file), so RS274NGC_ERROR is returned.

		This then calls close_and_downcase to remove tabs and spaces from
		everything on the line that is not part of a comment. Any comment is
		left as is.

		The length is set to zero if any of the following occur:
		1. The line now starts with a slash, and block_delete is on.
		2. The line now starts with a slash, but the second character is null.
		3. The first character is null.
		Otherwise, length is set to the length of the line.

		An input line is blank if the first character is null or it consists
		entirely of tabs and spaces and, possibly, a newline before the first
		null.

		If there is newline character at the end of the line, it is replaced
		in close_and_downcase with a null character.

		The rule used here (for what to do with a line starting with a slash)
		is: if block delete is on, the line is read but not executed; if block
		delete is off, ignore the slash and process the line normally. A slash
		anywhere else on a line, except inside a comment, is illegal and will
		cause an error.

		Block delete is discussed in [NCMS, page 3] but the discussion makes no
		sense.

		*/

		int read_text(         /* ARGUMENT VALUES                             */
			string command, /* a string which may have input text, or null */
			//			FILE * inport,        /* a file pointer for an input file, or null   */
			out string raw_line,      /* array to write raw input line into          */
			ref string line,          /* array for input line to be processed in     */
			out int length,         /* a pointer an integer to be set              */
			bool block_delete)  /* whether block_delete switch is on or off    */
		{
			//			char * returned_value;
			//			int len;
			raw_line = "";
			length = 0;

			if (command == null)
			{
				//				returned_value = fgets(raw_line, INTERP_TEXT_SIZE, inport);
				//				// knock off CR, LF
				//				len = strlen(raw_line) - 1; // set len to index of last char
				//				while (len >= 0)
				//				{
				//					if (isspace(raw_line[len]))
				//					{
				//						raw_line[len] = 0;
				//						len--;
				//					}
				//					else
				//					{
				//						break;
				//					}
				//				}
				//				if (returned_value == null)
				//					return RS274NGC_ENDFILE;
				//				else
				//					strcpy(line, raw_line);
			}
			else
			{
				raw_line = command;
				line = command;
			}

			if (close_and_downcase(ref line) == RS274NGC_ERROR)
				return RS274NGC_ERROR;

			if (line.Trim().Length == 0)
				length = 0;
			else if ( line[0] == '/' && block_delete )
				length = 0;
			else
				length = line.Length;

			return RS274NGC_OK;
		}

		/****************************************************************************/

		/* set_probe_data

		Returned Value: int (RS274NGC_OK)

		Side effects:
		System parameters may be reset.

		Called by:
		rs274ngc_execute
		rs274ngc_read

		*/

		int set_probe_data(        /* ARGUMENT VALUES             */
			ref setup settings)   /* pointer to machine settings */
		{
			CANON_POSITION position;
			CANON_POSITION probe_position;

			position =  GET_EXTERNAL_POSITION();
			settings.current_x = position.x;
			settings.current_y = position.y;
			settings.current_z = position.z;
			probe_position =  GET_EXTERNAL_PROBE_POSITION();
			settings.parameters[5061] = probe_position.x;
			settings.parameters[5062] = probe_position.y;
			settings.parameters[5063] = probe_position.z;
			settings.parameters[5067] =  GET_EXTERNAL_PROBE_VALUE();
			return RS274NGC_OK;
		}

		/****************************************************************************/
		/****************************************************************************/

		/*

		The functions defined below this point in this file comprise an
		interface between the rs274 interpreter kernel and the rest of the
		EMC software system.

		The interface also includes ten data items, as listed immediately below.
		These are passed by reference only to the interpreter kernel. In the
		integrated EMC system, none of these is known outside this file. In
		the stand-alone version seven of them are used in the driver source code
		file.

		*/

		/***********************************************************************/

		/* rs274ngc_reset

		Returned Value: none

		Side Effects:
		This function resets the status, so that when the interpreter
		is through with a part program (error, end of file, etc.) certain
		status data is zero.

		Called By:
		rs274ngc_close
		rs274ngc_exit
		rs274ngc_init
		rs274ngc_read

		*/

		int rs274ngc_reset()
		{
			_textline = 0;
			_interpreter_filename = "";
			_interpreter_linetext = "";
			_interpreter_blocktext = "";
			//			_interpreter_fp = null;
			_interpreter_status = 0;
			_interpreter_length = 0;

			return 0;
		}

		/***********************************************************************/

		/* rs274ngc_command

		Returned Value: char * (a string with the last line of NC code read)

		Side Effects: (none)

		Called By:
		This not called by any other function in the interpreter source file.
		It is intended to be called externally.
		It is not used in the stand-alone interpreter.

		*/

		public string rs274ngc_command()
		{
			return _interpreter_linetext;
		}

		/***********************************************************************/

		/* rs274ngc_file

		Returned Value: char * (the name of the NC program file being interpreted)

		Side Effects: (none)

		Called By:
		This not called by any other function in the interpreter source file.
		It is intended to be called externally.
		It is not used in the stand-alone interpreter.

		*/

		public string rs274ngc_file()
		{
			return _interpreter_filename;
		}

		/***********************************************************************/

		/* rs274ngc_line

		Returned Value: int

		Side Effects: (none)

		Called By:
		This not called by any other function in the interpreter source file.
		It is intended to be called externally.
		It is not used in the stand-alone interpreter.

		This returns the line number assigned by the interpreter to the current
		line of NC code. This is not the "N" number which is usually at the
		beginning of a line of code. The _textline is initialized to 1 in
		rs274ngc_open and is incremented by one each time rs274ngc_read
		is called.

		*/

		public int rs274ngc_line()
		{
			return _textline;
		}

		/***********************************************************************/

		/* rs274ngc_close

		Returned Value: int (RS274NGC_OK)

		Side Effects:
		The NC-code file is closed if open.

		Called By:
		This not called by any other function in the interpreter source file.
		It is intended to be called externally.
		It is not used in the stand-alone interpreter.

		*/

		//		int rs274ngc_close()
		//		{
		//			if (_interpreter_fp != null)
		//			{
		//				fclose(_interpreter_fp);
		//				_interpreter_fp = null;
		//			}
		//
		//			_interpreter_filename[0] = 0;
		//
		//			rs274ngc_reset();
		//
		//			return RS274NGC_OK;
		//		}

		/***********************************************************************/

		/* rs274ngc_execute

		Returned Value: int (RS274NGC_OK, RS274NGC_EXIT, RS274NGC_EXECUTE_FINISH,
							or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		1. The command argument is not null and any of the following happens
			A. The command string is too long.
			B. The command string cannot be handled correctly by read_text.
			C. The command string cannot be parsed correctly by read_line.
		2. execute_block returns RS274NGC_ERROR.
		3. The probe_flag is ON but the HME command queue is not empty.

		If execute_block returns RS274NGC_EXIT, this returns RS274NGC_EXIT.
		if execute_block returns RS274NGC_EXECUTE_FINISH this returns that.
		Otherwise, this returns RS274NGC_OK.

		Side Effects:
		Calls to canonical machining commands are made.
		The interpreter variables are changed.
		At the end of the program, the file is closed.

		Called By:
		This not called by any other function in the interpreter source file.
		It is intended to be called externally.
		It is called by interpret_from_file and interpret_from_keyboard
			in the driver.

		If the command argument is a non-null string, the string is assumed to
		be a line of NC code. It is parsed into the _interpreter_block,
		and the block is executed.

		If the command argument is null, it is assumed that the
		_interpreter_block already contains the information from a line of NC
		code and the block is executed.

		*/

		public int rs274ngc_execute(   /* ARGUMENT VALUES */
			string command)  /* string: line of NC code to execute (may be null) */
		{
			if (_interpreter_settings.probe_flag == ON)
			{
				if ( IS_EXTERNAL_QUEUE_EMPTY() == 0)
				{
					/* rs274ngc_reset(); */
					return ErrorNumberOf("Queue is not empty after probing");
				}
				set_probe_data(ref _interpreter_settings);
				_interpreter_settings.probe_flag = OFF;
			}

			if (command != null)
			{
				//				if(command.Length > (INTERP_TEXT_SIZE -1))
				//					return ErrorNumberOf("Command too long");
				/*				else */ if (read_text (command, /* null, */ out _interpreter_linetext,
											ref _interpreter_blocktext, out _interpreter_length, _interpreter_settings.block_delete) != RS274NGC_OK)
											return RS274NGC_ERROR;
										else if (_interpreter_length == 0) /* blank or block-deleted command */
											return RS274NGC_OK;
										else if (read_line(_interpreter_blocktext, ref _interpreter_block,
											ref _interpreter_settings) == RS274NGC_ERROR)
											return RS274NGC_ERROR;
				_textline++;
			}

			if (_interpreter_length == 0) /* blank or block deleted line from file */
			{
				block block = null; /* a null */
				write_g_codes(ref block, ref _interpreter_settings, _textline,
					ref _interpreter_active_g_codes);
				write_m_codes(ref block, ref _interpreter_settings, _textline,
					ref _interpreter_active_m_codes);
				write_settings(ref block, ref _interpreter_settings, _textline,
					ref _interpreter_active_settings);

				return RS274NGC_OK;
			}

			_interpreter_status =
				execute_block (ref _interpreter_block, ref _interpreter_settings, _textline);

			// write most recent G codes
			write_g_codes(ref _interpreter_block, ref _interpreter_settings, _textline,
				ref _interpreter_active_g_codes);
			write_m_codes(ref _interpreter_block, ref _interpreter_settings, _textline,
				ref _interpreter_active_m_codes);
			write_settings(ref _interpreter_block, ref _interpreter_settings, _textline,
				ref _interpreter_active_settings);

			if (_interpreter_status == RS274NGC_EXIT) /* program over */
			{
				//				if (_interpreter_fp != null)
				//				{
				//					fclose(_interpreter_fp);
				//					_interpreter_fp = null;
				//					_textline = 0;
				//					_interpreter_linetext[0] = 0;
				//				}
				return RS274NGC_EXIT;
			}
			else if (_interpreter_status == RS274NGC_ERROR)
				return RS274NGC_ERROR;
			else if (_interpreter_status == RS274NGC_EXECUTE_FINISH)
				return RS274NGC_EXECUTE_FINISH;
			else /* satisfactory execution of a line */
				return RS274NGC_OK;
		}

		/***********************************************************************/

		/* rs274ngc_exit

		Returned Value: int (RS274NGC_OK)

		Side Effects:
		Some parts of the world model are reset.

		Called By:
		This not called by any other function in the interpreter source file.
		It is intended to be called externally.
		It is not used in the stand-alone interpreter.

		*/

		//		int rs274ngc_exit()
		//		{
		//			rs274ngc_save_parameters(RS274NGC_PARAMETER_FILE,
		//				_interpreter_settings.parameters);
		//
		//			rs274ngc_reset();
		//
		//			return RS274NGC_OK;
		//		}

		/***********************************************************************/

		/* rs274ngc_synch

		Returned Value: int (RS274NGC_OK)

		Side Effects:
		sets the value of _interpreter_settings.current_x/y/z to what's
		returned by GET_EXTERNAL_POSITION().

		Called By:
		This not called by any other function in the interpreter source file.
		It is intended to be called externally.
		It is not used in the stand-alone interpreter.

		*/

		public int rs274ngc_synch()
		{
			CANON_POSITION current_point;
		
			_interpreter_settings.current_slot =  GET_EXTERNAL_TOOL();
		
			rs274ngc_load_tool_table();
		
			_interpreter_settings.control_mode =  GET_MOTION_CONTROL_MODE();
			current_point =  GET_EXTERNAL_POSITION();
			_interpreter_settings.current_x = current_point.x;
			_interpreter_settings.current_y = current_point.y;
			_interpreter_settings.current_z = current_point.z;
			_interpreter_settings.feed_rate =  GET_EXTERNAL_FEED_RATE();
			_interpreter_settings.flood = ( GET_EXTERNAL_FLOOD() != 0) ? ON : OFF;
			_interpreter_settings.mist = ( GET_EXTERNAL_MIST() != 0) ? ON : OFF;
			_interpreter_settings.selected_tool_slot =  GET_EXTERNAL_POCKET();
			_interpreter_settings.speed =  GET_EXTERNAL_SPEED();
			_interpreter_settings.spindle_turning =  GET_EXTERNAL_SPINDLE();
			_interpreter_settings.traverse_rate =  GET_EXTERNAL_TRAVERSE_RATE();
			return RS274NGC_OK;
		}

		/***********************************************************************/

		/* rs274ngc_init

		Returned Value: int (RS274NGC_OK)

		Side Effects:
		Many values in the _interpreter_settings structure are reset.
		Many interpreter variables are reset.
		A SET_FEED_REFERENCE canonical command call is made.
		An INIT_CANON call is made.
		write_g_codes, write_m_codes, and write_settings are called.
		reset_interp_wm is called.


		Called By:
		This not called by any other function in the interpreter source file.
		It is intended to be called externally.
		It is called by interpret_from_file and interpret_from_keyboard
			in the interpreter driver.

		Currently we are running only in CANON_XYZ feed_reference mode.  There
		is no command regarding feed_reference in the rs274 language (we
		should try to get one added). The initialization routine, therefore,
		always calls SET_FEED_REFERENCE(CANON_XYZ).

		Length units are set early since the default is inches, and calling
		USE_LENGTH_UNITS before extracting data from the rest of the system
		will get the rest of the system to use inches.

		The early current position settings are used by the stand-alone interpreter.

		*/

		public int rs274ngc_init()
		{
			CANON_VECTOR axis_offset = new CANON_VECTOR();
			CANON_VECTOR origin_point = new CANON_VECTOR();
			int k;

			rs274ngc_reset();

			 INIT_CANON();

			_interpreter_settings.length_units = CANON_UNITS.INCHES;
			 USE_LENGTH_UNITS(_interpreter_settings.length_units);

			for (k = 5161; k < 5387; k++)
			{
				_interpreter_settings.parameters[k] = 0.0;
			}

			rs274ngc_restore_parameters(DEFAULT_RS274NGC_PARAMETER_FILE, ref _interpreter_settings.parameters);

			// Get axis offsets from parameters
			axis_offset.x = _interpreter_settings.parameters[5211];
			axis_offset.y = _interpreter_settings.parameters[5212];
			axis_offset.z = _interpreter_settings.parameters[5213];

			// Get origin from parameters for G54 default coordinate system
			origin_point.x = _interpreter_settings.parameters[5221];
			origin_point.y = _interpreter_settings.parameters[5222];
			origin_point.z = _interpreter_settings.parameters[5223];

			// Set the origin offsets
			 SET_ORIGIN_OFFSETS(origin_point.x + axis_offset.x,
				origin_point.y + axis_offset.y,
				origin_point.z + axis_offset.z);

			 SET_FEED_REFERENCE(CANON_FEED_REFERENCE.XYZ);

			_textline = 0;
			_interpreter_settings.axis_offset_x = 0.0;
			_interpreter_settings.axis_offset_y = 0.0;
			_interpreter_settings.axis_offset_z = 0.0;
			_interpreter_settings.block_delete = false;
			/*_interpreter_settings.control_mode set in rs274ngc_synch */
			/*_interpreter_settings.current_slot set in rs274ngc_synch */
			_interpreter_settings.current_x = 0.0; /* reset by rs274ngc_synch */
			_interpreter_settings.current_y = 0.0; /* reset by rs274ngc_synch */
			_interpreter_settings.current_z = 0.0; /* reset by rs274ngc_synch */
			_interpreter_settings.cutter_radius_compensation = CANON_CUTTER_COMP.OFF;
			/* cycle values do not need initialization */
			_interpreter_settings.distance_mode = DISTANCE_MODE.ABSOLUTE;
			_interpreter_settings.feed_mode = UNITS_PER_MINUTE;
			_interpreter_settings.feed_override = ON;
			/*_interpreter_settings.feed_rate set in rs274ngc_synch */
			/*_interpreter_settings.flood set in rs274ngc_synch */
			_interpreter_settings.length_offset_index = 1;
			/*_interpreter_settings.length_units set above */
			/*_interpreter_settings.mist set in rs274ngc_synch */
			_interpreter_settings.motion_mode = G_1;
			_interpreter_settings.origin_ngc = 1;
			_interpreter_settings.axis_offset_x = axis_offset.x;
			_interpreter_settings.axis_offset_y = axis_offset.y;
			_interpreter_settings.axis_offset_z = axis_offset.z;
			_interpreter_settings.origin_offset_x = origin_point.x;
			_interpreter_settings.origin_offset_y = origin_point.y;
			_interpreter_settings.origin_offset_z = origin_point.z;
			/*_interpreter_settings.parameters set above */
			_interpreter_settings.plane = CANON_PLANE.XY;
			 SELECT_PLANE(_interpreter_settings.plane);
			_interpreter_settings.probe_flag = OFF;
			_interpreter_settings.program_x = UNKNOWN; /* for cutter comp */
			_interpreter_settings.program_y = UNKNOWN; /* for cutter comp */
			_interpreter_settings.retract_mode = RETRACT_MODE.R_PLANE;
			/*_interpreter_settings.selected_tool_slot set in rs274ngc_synch */
			/*_interpreter_settings.speed set in rs274ngc_synch */
			_interpreter_settings.speed_feed_mode = CANON_SPEED_FEED_MODE.INDEPENDENT;
			_interpreter_settings.speed_override = ON;
			_interpreter_settings.tool_length_offset = 0.0;
			/*_interpreter_settings.tool_table set in rs274ngc_synch */
			_interpreter_settings.tool_table_index = 1;
			/*_interpreter_settings.traverse_rate set in rs274ngc_synch */

			block block = null; /* a null */
			write_g_codes(ref block, ref _interpreter_settings, _textline, ref _interpreter_active_g_codes);
			write_m_codes(ref block, ref _interpreter_settings, _textline, ref _interpreter_active_m_codes);
			write_settings(ref block, ref _interpreter_settings, _textline, ref _interpreter_active_settings);

			// Synch rest of settings to external world
			rs274ngc_synch();

			return RS274NGC_OK;
		}

		/***********************************************************************/

		/* rs274ngc_open

		Returned Value: int (RS274NGC_OK or RS274NGC_ERROR)
		If any of the following errors occur, this returns RS274NGC_ERROR.
		Otherwise it returns RS274NGC_OK.
		1. The file cannot be opened.

		Side Effects:
		The file is opened for reading and _interpreter_fp is set.
		The input program line counter, _textline, is set.
		_interpreter_filename is set.

		Called By:
		This not called by any other function in the interpreter source file.
		It is intended to be called externally.
		It is called by interpret_from_file in the interpreter driver.

		The manual [NCMS, page 3] discusses the use of the "%" character at the
		beginning of a "tape". It is not clear whether it is intended that
		every NC-code file should begin with that character. Here, we are not
		requiring it. _textline is set to 0.

		*/

		//		int rs274ngc_open(     /* ARGUMENT VALUES                               */
		//			string filename) /* string: the name of the input NC-program file */
		//		{
		//			_interpreter_fp = fopen(filename, "r");
		//			if (_interpreter_fp == null)
		//				return ErrorNumberOf("Unable to open file");
		//
		//			_textline = 0;
		//
		//			strcpy(_interpreter_filename, filename);
		//
		//			return RS274NGC_OK;
		//		}

		/***********************************************************************/

		/* rs274ngc_read

		Returned Value: int (RS274NGC_OK, RS274NGC_EXIT, RS274NGC_ENDFILE,
							or RS274NGC_ERROR)
		If _interpreter_fp is null and _interpreter_status is RS274NGC_EXIT,
		this returns RS274NGC_EXIT.
		If the end of the file is found, this returns RS274NGC_ENDFILE.
		Otherwise, if any of the following errors occur, this
		returns RS274NGC_ERROR. Otherwise, it returns RS274NGC_OK.
		1. The _interpreter_fp file pointer is null.
		2. read_text (which gets a line of NC code from file)
			returns RS274NGC_ERROR.
		3. read_line (which parses the line) returns RS274NGC_ERROR.
		4. The end of the file has been reached.
		5. The probe_flag is ON but the HME command queue is not empty.

		Side Effects:
		If there is an error, the file is closed.
		_textline is incremented.
		The _interpreter_block is filled with data.

		Called By:
		This not called by any other function in the interpreter source file.
		It is intended to be called externally.
		It is called by interpret_from_file in the driver.

		This reads a line of NC-code from a file. The _interpreter_length will
		be set by read_text. This will be zero if the line is blank or block
		deleted. If the length is not zero, this parses the line into the
		_interpreter_block.

		Before reading a line of NC-code, this checks to see it the probe_flag
		in the settings in ON. If so, this means the STRAIGHT_PROBE function
		was called last, so data about the current position and the probe
		position is updated, after checking to be sure the command queue of
		the HME is empty (so that the probing is sure to have been executed).

		This is not closing the file when the end of the file has been
		reached. When this function returns RS274NGC_ENDFILE, rs274ngc_close
		should be called.

		*/

		//		int rs274ngc_read()
		//		{
		//			int status;
		//
		//			if (_interpreter_settings.probe_flag == ON)
		//			{
		//				if ( IS_EXTERNAL_QUEUE_EMPTY() == 0)
		//				{
		//					/* rs274ngc_reset(); */
		//					return ErrorNumberOf("Queue is not empty after probing");
		//				}
		//				set_probe_data(&_interpreter_settings);
		//				_interpreter_settings.probe_flag = OFF;
		//			}
		//
		//			if (_interpreter_fp == null)
		//			{
		//				if (_interpreter_status == RS274NGC_EXIT)
		//				{   /* set in call to rs274ngc_execute */
		//					/* rs274ngc_reset(); */
		//					return RS274NGC_EXIT;
		//				}
		//				else
		//				{
		//					/* rs274ngc_reset(); */
		//					ERR_MACRO_PASS(name);
		//				}
		//			}
		//
		//			/* read the line */
		//			_textline++;
		//			status = read_text (null, _interpreter_fp, _interpreter_linetext,
		//				_interpreter_blocktext, &_interpreter_length,
		//				_interpreter_settings.block_delete);
		//			if ((status == RS274NGC_ENDFILE) && (_interpreter_block.m_modes[4] == 1))
		//				return RS274NGC_ENDFILE;  /* m1 appeared on the preceding line */
		//			else if (status == RS274NGC_ENDFILE)
		//			{
		//				fclose(_interpreter_fp);
		//				_interpreter_fp = null;
		//				/* rs274ngc_reset(); */
		//				return ErrorNumberOf("File ended with no stopping command given");
		//			}
		//			else if (status == RS274NGC_ERROR)
		//			{
		//				/* rs274ngc_reset(); */
		//				ERR_MACRO_PASS(name);
		//			}
		//			if (_interpreter_length == 0)
		//				return RS274NGC_OK;
		//
		//				/* parse the block */
		//			else if (read_line(_interpreter_blocktext, &_interpreter_block,
		//				&_interpreter_settings) == RS274NGC_ERROR)
		//			{
		//				/* rs274ngc_reset(); */
		//				ERR_MACRO_PASS(name);
		//			}
		//			else
		//				return RS274NGC_OK;
		//		}

		/***********************************************************************/

		/* rs274ngc_load_tool_table()

		Returned Value: RS274NGC_OK

		Side Effects:
		_interpreter_settings.tool_table[] is modified.

		Called By:
		rs274ngc_init().
		It is also intended to be called externally, when the tool table
		is changed.

		This function calls the canonical interface function GET_EXTERNAL_TOOL_TABLE
		to load the whole tool table into the _interpreter_settings.
		*/

		public void rs274ngc_load_tool_table()
		{
			int t;

			for (t = 0; t <= CANON_TOOL_MAX; t++)
			{
				_interpreter_settings.tool_table[t] =  GET_EXTERNAL_TOOL_TABLE(t);
			}
		}

		/***********************************************************************/

		/* rs274ngc_restore_parameters()

		Returned Value: RS274NGC_OK, RS274NGC_ERROR

		Side Effects:
		parameters[] array in settings structure passed (e.g.,
		_interpreter_settings.parameters[]) is modified.

		Called By:
		rs274ngc_init().

		This function restores the parameters from a file containing lines of
		the form:

		<variable number> <value>

		e.g.,

		5161 10.456
		*/
		int rs274ngc_restore_parameters(string filename, ref double[] parameters)
		{
			//			FILE *infp = null;
			//			string line;
			//			int variable;
			//			double value;
			//
			//			//open original for reading
			//			if (null == (infp = fopen(filename, "r")))
			//			{
			//				return ErrorNumberOf("Cannot open file");
			//				return -1;
			//			}
			//
			//			while (!feof(infp))
			//			{
			//				if (null == fgets(line, 256, infp))
			//				{
			//					break;
			//				}
			//
			//				// try for a variable-value match
			//				if (2 == sscanf(line, "%d %lf", &variable, &value))
			//				{
			//					// write it into variable array
			//					if (variable <= 0 || variable >= RS274NGC_MAX_PARAMETERS)
			//					{
			//						return ErrorNumberOf("Parameter number out of range");
			//					}
			//					else
			//					{
			//						parameters[variable] = value;
			//					}
			//				}
			//			}
			//
			//			fclose(infp);

			return 0;
		}

		/***********************************************************************/

		/* rs274ngc_save_parameters()

		Returned Value: RS274NGC_OK, RS274NGC_ERROR

		Side Effects:
		A file containing variable-value assignments is updated with the
		current value of the variables.

		Called By:
		rs274ngc_exit().

		This function saves parameters to a file containing lines of
		the form:

		<variable number> <value>

		e.g.,

		5161 10.456
		*/
		//		int rs274ngc_save_parameters(string filename, double[] parameters)
		//		{
		//			//			FILE *infp = null;
		//			//			FILE *outfp = null;
		//			string line;
		//			int variable;
		//			double value;
		//
		//			// rename as .bak
		//			strcpy(line, filename);
		//			strcat(line, RS274NGC_PARAMETER_FILE_BACKUP_SUFFIX);
		//			if (0 != rename(filename, line))
		//			{
		//				return ErrorNumberOf("Cannot create backup file");
		//				return -1;
		//			}
		//
		//			// open backup for reading
		//			if (null == (infp = fopen(line, "r")))
		//			{
		//				return ErrorNumberOf("Cannot open backup file");
		//				return -1;
		//			}
		//
		//			// open original for writing
		//			if (null == (outfp = fopen(filename, "w")))
		//			{
		//				return ErrorNumberOf("Cannot open variable file");
		//				return -1;
		//			}
		//
		//			while (!feof(infp))
		//			{
		//				if (null == fgets(line, 256, infp))
		//				{
		//					break;
		//				}
		//
		//				// try for a variable-value match
		//				if (2 == sscanf(line, "%d %f", &variable, &value))
		//				{
		//					// overwrite with internal variable
		//					if (variable <= 0 || variable >= RS274NGC_MAX_PARAMETERS)
		//					{
		//						return ErrorNumberOf("Parameter number out of range");
		//					}
		//					else
		//					{
		//						sprintf(line, "%d\t%f\n", variable, parameters[variable]);
		//					}
		//				}
		//
		//				// write it out
		//				fputs(line, outfp);
		//			}
		//
		//			fclose(infp);
		//			fclose(outfp);
		//
		//			return 0;
		//		}
		//
		/***********************************************************************/

		/* rs274ngc_ini_load()

		Returned Value: RS274NGC_OK, RS274NGC_ERROR

		Side Effects:
		An INI file containing values for global variables is used to
		update the globals

		Called By:
		rs274ngc_init()

		The file looks like this:

		[RS274NGC]
		VARIABLE_FILE = rs274ngc.var

		*/

		int rs274ngc_ini_load(string filename)
		{
			return RS274NGC_OK;
		}

		/***********************************************************************/

		/* rs274ngc_active_g_codes

		Returned Value: RS274NGC_OK

		Side Effects: copies active G codes into arg array

		Called By: external programs

		*/

		public int rs274ngc_active_g_codes(ref int[] codes)
		{
			int t;

			for (t = 0; t < RS274NGC_ACTIVE_G_CODES; t++)
			{
				codes[t] = _interpreter_active_g_codes[t];
			}

			return RS274NGC_OK;
		}

		/***********************************************************************/

		/* rs274ngc_active_m_codes

		Returned Value: RS274NGC_OK

		Side Effects: copies active M codes into arg array

		Called By: external programs

		*/

		public int rs274ngc_active_m_codes(ref int[] codes)
		{
			int t;

			for (t = 0; t < RS274NGC_ACTIVE_M_CODES; t++)
			{
				codes[t] = _interpreter_active_m_codes[t];
			}

			return RS274NGC_OK;
		}

		/***********************************************************************/

		/* rs274ngc_active_settings

		Returned Value: RS274NGC_OK

		Side Effects: copies active F, S settings into array

		Called By: external programs

		*/

		public int rs274ngc_active_settings(ref double[] settings)
		{
			int t;

			for (t = 0; t < RS274NGC_ACTIVE_SETTINGS; t++)
			{
				settings[t] = _interpreter_active_settings[t];
			}

			return RS274NGC_OK;
		}

		/***********************************************************************/

		#endregion

	}
}