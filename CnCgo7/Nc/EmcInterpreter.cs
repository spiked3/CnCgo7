using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.Threading;

namespace CnCgo7
{
    /// <summary>
    /// Encapsulates the GCode interpreter from EMC (GPLd code)
    /// </summary>
    public partial class EmcInterpreter 
    {
        DataModel DataModel;

        public EmcInterpreter(DataModel DataModel)
        {
            this.DataModel = DataModel;
            rs274ngc_init();
        }

        //-----------------------------------------------------------------------------------------------------//
        //-----------   machine moves -------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------------//

        public void Exec(string T)
        {
            rs274ngc_execute(T);
        }

         /**************************************************************************************** 
		 ************                           ************************************************* 
		 ************    Canon Implmentation    ************************************************* 
		 ************   for GCode Interpreter   *************************************************
		 ************                           ************************************************* 
		 ****************************************************************************************/

        CANON_VECTOR program_origin = new CANON_VECTOR();
        CANON_UNITS length_units = CANON_UNITS.MM;
        CANON_PLANE active_plane = CANON_PLANE.XY;

        /* Representation */

        public void SET_ORIGIN_OFFSETS(double x, double y, double z)
        {
            Trace.WriteLine("SET_ORIGIN_OFFSETS: " + x + ", " + y + ", " + z);
            program_origin.x = x;
            program_origin.y = y;
            program_origin.z = z;
        }

        public void USE_LENGTH_UNITS( CANON_UNITS in_unit)
        {
            Trace.WriteLine("USE_LENGTH_UNITS: " + in_unit.ToString());
        }

        /* Free Space Motion */
        public void SET_TRAVERSE_RATE(double Rate)
        {
            Trace.WriteLine("SET_TRAVERSE_RATE: " + Rate);
            DataModel.TraverseRate = (float)Rate;
        }

        public void STRAIGHT_TRAVERSE(double x, double y, double z)
        {
            Trace.WriteLine("STRAIGHT_TRAVERSE: " + x + ", " + y + ", " + z);
            DataModel.LinearMove(DataModel.TraverseRate, (float)x, (float)y, (float)z);
        }

        /* Machining Attributes */
        public void SET_FEED_RATE(double Rate)
        {
            Trace.WriteLine("SET_FEED_RATE: " + Rate);
            DataModel.FeedRate = (float)Rate;
        }

        public void SET_FEED_REFERENCE( CANON_FEED_REFERENCE reference)
        {
            Trace.WriteLine("SET_FEED_REFERENCE: " + reference.ToString());
        }

        public void SET_MOTION_CONTROL_MODE( CANON_MOTION_MODE mode)
        {
            Trace.WriteLine("SET_MOTION_CONTROL_MODE: " + mode.ToString());
        }

        public void SELECT_PLANE( CANON_PLANE in_plane)
        {
            Trace.WriteLine("SELECT_PLANE: " + in_plane.ToString());
        }

        public void SET_CUTTER_RADIUS_COMPENSATION(double radius)
        {
            Trace.WriteLine("SET_CUTTER_RADIUS_COMPENSATION: " + radius);
        }

        public void START_CUTTER_RADIUS_COMPENSATION( CANON_CUTTER_COMP side)
        {
            Trace.WriteLine("START_CUTTER_RADIUS_COMPENSATION: " + side.ToString());
        }

        public void STOP_CUTTER_RADIUS_COMPENSATION()
        {
            Trace.WriteLine("STOP_CUTTER_RADIUS_COMPENSATION");
        }

        public void START_SPEED_FEED_SYNCH()
        {
            Trace.WriteLine("START_SPEED_FEED_SYNCH");
        }

        public void STOP_SPEED_FEED_SYNCH()
        {
            Trace.WriteLine("STOP_SPEED_FEED_SYNCH");
        }

        public void SELECT_MOTION_MODE( CANON_MOTION_MODE mode)
        {
            Trace.WriteLine("SELECT_MOTION_MODE: " + mode.ToString());
        }

        /* Machining Functions */

        public void ARC_FEED(double first_end, double second_end,
            double first_axis, double second_axis, int rotation, double axis_end_point)
        {
            Trace.WriteLine("ARC_FEED: " + first_end + ", " + second_end + ", " +
                first_axis + ", " + second_axis + ", " + rotation + ", " + axis_end_point);

            DataModel.ArcMove(DataModel.FeedRate, (float)first_end, (float)second_end, (float)first_axis, (float)second_axis, rotation, (float)axis_end_point);
        }

        public void STRAIGHT_FEED(double x, double y, double z)
        {
            Trace.WriteLine("STRAIGHT_FEED: " + x + ", " + y + ", " + z);
            DataModel.LinearMove(DataModel.FeedRate, (float)x, (float)y, (float)z);
        }

        public void STRAIGHT_PROBE(double x, double y, double z)
        {
            Trace.WriteLine("STRAIGHT_PROBE: " + x + ", " + y + ", " + z);
            throw new NotImplementedException();
        }

        public void DWELL(double seconds)
        {
            // seconds is really ms
            Trace.WriteLine("DWELL: " + +seconds);
            Thread.Sleep((int)(seconds));
        }

        /* Spindle Functions */
        public void SPINDLE_RETRACT_TRAVERSE()
        {
            Trace.WriteLine("SPINDLE_RETRACT_TRAVERSE");
        }

        public void START_SPINDLE_CLOCKWISE()
        {
            Trace.WriteLine("START_SPINDLE_CLOCKWISE");
            DataModel.SpindleRotate = SpindleRotate.Clockwise;
        }

        public void START_SPINDLE_COUNTERCLOCKWISE()
        {
            Trace.WriteLine("START_SPINDLE_COUNTERCLOCKWISE");
            DataModel.SpindleRotate = SpindleRotate.CounterClockwise;
        }

        public void SET_SPINDLE_SPEED(double r)
        {
            Trace.WriteLine("SET_SPINDLE_SPEED: " + r);
            DataModel.SpindleRPM = (int)r;
        }

        public void STOP_SPINDLE_TURNING()
        {
            Trace.WriteLine("STOP_SPINDLE_TURNING");
            DataModel.SpindleRotate = SpindleRotate.None;
        }

        public void SPINDLE_RETRACT()
        {
            Trace.WriteLine("SPINDLE_RETRACT");
        }

        public void ORIENT_SPINDLE(double orientation,  CANON_DIRECTION direction)
        {
            Trace.WriteLine("ORIENT_SPINDLE: " + orientation + ", " + direction.ToString());
        }

        public void USE_NO_SPINDLE_FORCE()
        {
            Trace.WriteLine("USE_NO_SPINDLE_FORCE");
        }

        /* Tool Functions */

        public void USE_TOOL_LENGTH_OFFSET(double length)
        {
            Trace.WriteLine("USE_TOOL_LENGTH_OFFSET: " + length);
        }

        public void CHANGE_TOOL(int slot)
        {
            Trace.WriteLine("CHANGE_TOOL: " + slot);
            DataModel.CurrentTool = slot;
            //+++if (!DataModel.ToolLib.ToolMap.ContainsKey(DataModel.CurrentTool))
            //    Trace.WriteLine("Warning: CHANGE_TOOL, Tool not found: T" + DataModel.CurrentTool);
        }

        public void SELECT_TOOL(int slot)
        {
            Trace.WriteLine("SELECT_TOOL: " + slot);
            DataModel.CurrentTool = slot;
            //+++if (!DataModel.ToolLib.ToolMap.ContainsKey(DataModel.CurrentTool))
            //    Trace.WriteLine("Warning: SELECT_TOOL, Tool not found: T" + DataModel.CurrentTool);
        }

        /* Misc Functions */

        public void CLAMP_AXIS( CANON_AXIS axis)
        {
            Trace.WriteLine("CLAMP_AXIS: " + axis.ToString());
        }

        public void COMMENT(string s)
        {
            Trace.WriteLine("COMMENT: " + s);
        }

        public void DISABLE_FEED_OVERRIDE()
        {
            Trace.WriteLine("DISABLE_FEED_OVERRIDE");
        }

        public void DISABLE_SPEED_OVERRIDE()
        {
            Trace.WriteLine("DISABLE_SPEED_OVERRIDE");
        }

        public void ENABLE_FEED_OVERRIDE()
        {
            Trace.WriteLine("ENABLE_FEED_OVERRIDE");
        }

        public void ENABLE_SPEED_OVERRIDE()
        {
            Trace.WriteLine("ENABLE_SPEED_OVERRIDE");
        }

        public void FLOOD_OFF()
        {
            Trace.WriteLine("FLOOD_OFF");
        }

        public void FLOOD_ON()
        {
            Trace.WriteLine("FLOOD_ON");
        }

        public void MESSAGE(string s)
        {
            Trace.WriteLine("MESSAGE: " + s);
        }

        public void MIST_OFF()
        {
            Trace.WriteLine("MIST_OFF");
        }

        public void MIST_ON()
        {
            Trace.WriteLine("MIST_ON");
        }

        public void PALLET_SHUTTLE()
        {
            Trace.WriteLine("PALLET_SHUTTLE");
        }

        public void TURN_PROBE_OFF()
        {
            Trace.WriteLine("TURN_PROBE_OFF");
        }

        public void TURN_PROBE_ON()
        {
            Trace.WriteLine("TURN_PROBE_ON");
        }

        public void UNCLAMP_AXIS( CANON_AXIS axis)
        {
            Trace.WriteLine("UNCLAMP_AXIS: " + axis.ToString());
        }

        /* Program Functions */

        public void PROGRAM_STOP()
        {
            Trace.WriteLine("PROGRAM_STOP");
        }

        public void OPTIONAL_PROGRAM_STOP()
        {
            Trace.WriteLine("OPTIONAL_PROGRAM_STOP");
        }

        public void PROGRAM_END()
        {
            Trace.WriteLine("PROGRAM_END");
        }

        /* returns the current x, y, z origin offsets */
        public  CANON_VECTOR GET_PROGRAM_ORIGIN()
        {
            return program_origin;
        }

        /* returns the current active units */
        public  CANON_UNITS GET_LENGTH_UNITS()
        {
            return length_units;
        }

        public  CANON_PLANE GET_PLANE()
        {
            return active_plane;
        }

        /*********************************************************************/

        /*

        The purpose of these GET_XXX (and other) functions is to provide for
        local emulation of the world modeling done by the EMC system.

        /*********************************************************************/

        /* GET_EXTERNAL_FEED_RATE

        called by: rs274ngc_synch

        This is a stub.

        */

        public double GET_EXTERNAL_FEED_RATE()
        {
            return 15;
        }

        /*********************************************************************/

        public int GET_EXTERNAL_FLOOD()
        {
            return  OFF;
        }

        /*********************************************************************/

        public int GET_EXTERNAL_MIST()
        {
            return  OFF;
        }

        /*********************************************************************/

        public int GET_EXTERNAL_POCKET()
        {
            return 1;
        }

        /*********************************************************************/

        public  CANON_POSITION GET_EXTERNAL_POSITION()
        {
            return new  CANON_POSITION(
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0);
        }

        /*********************************************************************/

        public  CANON_POSITION GET_EXTERNAL_PROBE_POSITION()
        {
            return new  CANON_POSITION(
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0);
        }

        /*********************************************************************/

        public double GET_EXTERNAL_PROBE_VALUE()
        {
            return 1.0;
        }

        /*********************************************************************/

        public double GET_EXTERNAL_SPEED()
        {
            // speed is in RPMs everywhere
            return Math.Abs(DataModel.SpindleRPM);
        }

        /*********************************************************************/

        public  CANON_DIRECTION GET_EXTERNAL_SPINDLE()
        {
            return  CANON_DIRECTION.STOPPED;
        }

        /*********************************************************************/

        public int GET_EXTERNAL_TOOL()
        {
            return 1;
        }

        /*********************************************************************/

        public int GET_EXTERNAL_TOOL_MAX()
        {
            return  CANON_TOOL_MAX;
        }

        /*********************************************************************/

        public  CANON_TOOL_TABLE GET_EXTERNAL_TOOL_TABLE(int pocket)
        {
             CANON_TOOL_TABLE retval = new  CANON_TOOL_TABLE();

            if (pocket == 0)
            {
                retval.id = 1;
                retval.length = 2.0;
                retval.diameter = 1.0;
            }
            else if (pocket == 1)
            {
                retval.id = 1;
                retval.length = 2.0;
                retval.diameter = 1.0;
            }
            else if (pocket == 2)
            {
                retval.id = 2;
                retval.length = 1.0;
                retval.diameter = 2.0;
            }
            else
            {
                retval.id = 0;
                retval.length = 0.0;
                retval.diameter = 0.0;
            }

            return retval;
        }


        /*********************************************************************/

        public double GET_EXTERNAL_TRAVERSE_RATE()
        {
            return 100.0;
        }

        /*********************************************************************/

        public  CANON_MOTION_MODE GET_MOTION_CONTROL_MODE()
        {
            return  CANON_MOTION_MODE.EXACT_PATH;
        }

        /*********************************************************************/

        /* INIT_CANON()

        called by: rs274ngc_init

        This is a stub.

        */

        public void INIT_CANON()
        { }

        /*********************************************************************/

        /*

        IS_EXTERNAL_QUEUE_EMPTY emulates the EMC external to the
        interpreter. It just returns 1, meaning the queue is empty (any
        non-zero means the queue is empty).

        */

        public int IS_EXTERNAL_QUEUE_EMPTY()
        {
            return 1;
        }

        /*********************************************************************/

        /*

        CANON_UPDATE_POSITION tells the canonical interface that it should
        update the end position it may have been saving, due to an abort other
        other external event that may have made the end position different.
        Here it does nothing since the standalone interpreter doesn't keep track
        of the end position.

        */

        public void CANON_UPDATE_POSITION()
        {
            return;
        }

        /*********************************************************************/
    }
}
