using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insteon.Library
{

    public class Constants
    {
        #region Standard Commands
        // standard commands
        /// <summary>
        /// Go ON to the specified level (cmd2). In a broadcast group command, the cmd2 is ignored as it relies upon its internal at-link-time setting.
        /// </summary>
        public const byte STD_COMMAND_ON = 0x11;

        /// <summary>
        /// Go ON to the specified level (cmd2). In a broadcast group command, the cmd2 is ignored as it relies upon its internal at-link-time setting. DevCat 0x01 remembers level, DevCat 0x02 turns on to full.
        /// </summary>
        public const byte STD_COMMAND_FAST_ON = 0x12;

        /// <summary>
        /// Go OFF.
        /// </summary>
        public const byte STD_COMMAND_OFF = 0x13;

        /// <summary>
        /// Go OFF, ignoring the ramp rate.
        /// </summary>
        public const byte STD_COMMAND_FAST_OFF = 0x14;

        /// <summary>
        /// Brighten one step (of 32 between on and off).
        /// </summary>
        public const byte STD_COMMAND_BRIGHT = 0x15;

        /// <summary>
        /// Dim one step (of 32 between on and off).
        /// </summary>
        public const byte STD_COMMAND_DIM = 0x16;

        /// <summary>
        /// Begin dimming or brightening until 18-Stop Manual Change received. Cmd2 is 1 for brightening, 0 for dimming.
        /// </summary>
        public const byte STD_COMMAND_START_MANUAL = 0x17;

        /// <summary>
        /// End dimming or brightening started with 17-Start Manual Change received.
        /// </summary>
        public const byte STD_COMMAND_STOP_MANUAL = 0x18;

        /// <summary>
        /// Request on-level status from a unit. Status is returned in Cmd 2. Status is On Level, Cmd 1 is a number that increments every time there is a change in the Receiving Devices Link Database (Database delta).
        /// or (depending on cmd2 = 0x00 vs 0x01)
        /// Request LED bit mask from a KPL dimmer, which will be returned as Cmd 2. Cmd 1 is the Database delta (as described for Status Request).
        /// </summary>
        public const byte STD_COMMAND_STATUS_REQUEST = 0x19;

        /// <summary>
        /// Returned ACK message will contain the requested data in Command 2.
        /// </summary>
        public const byte STD_COMMAND_GET_OP_FLAGS = 0x1F;

        /// <summary>
        /// Sets flags
        /// </summary>
        public const byte STD_COMMAND_SET_OP_FLAGS = 0x20;

        /// <summary>
        /// Set light to Level at next zero crossing.
        /// </summary>
        public const byte STD_COMMAND_LIGHT_INSTANT_CHANGE = 0x21;

        /// <summary>
        /// Indicates manual load status change. DevCat 0x01 and 0x02 only.
        /// </summary>
        public const byte STD_COMMAND_LIGHT_MANUAL_OFF = 0x22;

        /// <summary>
        /// Indicates manual load status change. DevCat 0x01 and 0x02 only.
        /// </summary>
        public const byte STD_COMMAND_LIGHT_MANUAL_ON = 0x23;

        /// <summary>
        /// Causes a device to respond as if its SET Button were tapped once or twice. Cmd2 should = 1 or 2.
        /// </summary>
        public const byte STD_COMMAND_REMOTE_SET_BUTTON_TAP = 0x25;

        /// <summary>
        /// For DevCat 0x01 only. Update SwitchLinc Companion's LEDs.
        /// </summary>
        public const byte STD_COMMAND_LIGHT_SET_STATUS = 0x27;

        /// <summary>
        /// Turns on light at specified rate. Bits 0-3 (low bits) = 2xRampRate +1. Bits 4-7 (high bits) = On-Level + 0x0F.
        /// </summary>
        public const byte STD_COMMAND_LIGHT_RAMP_ON = 0x2E;

        /// <summary>
        /// Turns off light at specified rate. Bits 0-3 (low bits) = 2xRampRate +1. Bits 4-7 (high bits) = ignored.
        /// </summary>
        public const byte STD_COMMAND_LIGHT_RAMP_OFF = 0x2F;

        /// <summary>
        /// Beep. For some devices, the duration may be set with command2.
        /// </summary>
        public const byte STD_COMMAND_BEEP = 0x30;

        public const byte STD_COMMAND_THERMOSTAT_TEMP_UP = 0x68;

        public const byte STD_COMMAND_THERMOSTAT_TEMP_DOWN = 0x69;

        public const byte STD_COMMAND_THERMOSTAT_SET_COOL_POINT = 0x6C;

        public const byte STD_COMMAND_THERMOSTAT_SET_HEAT_POINT = 0x6D;

        public const byte STD_COMMAND_THERMOSTAT_STATUS = 0x6A;

        public const byte STD_COMMAND_THERMOSTAT_CONTROL = 0x6B;

        public const byte STD_COMMAND_THERMOSTAT_STATUS_TEMP = 0x6E;
        public const byte STD_COMMAND_THERMOSTAT_STATUS_HUMIDITY = 0x6F;
        public const byte STD_COMMAND_THERMOSTAT_STATUS_MODE_FAN = 0x70;
        public const byte STD_COMMAND_THERMOSTAT_STATUS_COOL_SET = 0x71;
        public const byte STD_COMMAND_THERMOSTAT_STATUS_HEAT_SET = 0x72;


        #endregion

        #region Extended Commands
        // extended commands
        /*
         ControlLinc
         D1: 0x00-0xFF Button/Group number, D2: 0x00 for data request (D3-D14 0x00), 0x01 for response, D3-D4 X10 house/unit 1 (0x20 = none), D5-D6 X10 house/unit 2, D7-D8 X10 house/unit 3, D9-D10 X10 house/unit 4, D11-D12 X10 house/unit 5, D13-D14 unused. To set, D2 = 0x04, D3 = house (0x20 for none), D4 = unit.	
          
         RemoteLinc
         D1: 0x00-0xFF Button/Group number, D2: 0x00 for data request (D3-D14 0x00), 0x01 for response, D3: awake time upon heartbeat (sec), D4: heartbeat interval x 12.5 minutes, D5: number of 0x04 Heartbeat standard messages to send upon heartbeat, D6: button trigger all-link bitmap. To set awake time, D2: 0x02, D3: awake time (sec). To set interval, D2: 0x03, D3: heartbeat interval x12.5 minutes. To set # messages, D2: 0x04, D3: # of heartbeat messages to send. To set trigger-all-link state for button, D2: 0x05, D3: 0 send normal commands or 1 send ED 0x30 Trigger All-Link command to first device in ALDB.	
          
         KeypadLinc
         D1: 0x00-0xFF Button/Group number, D2 0x00 for data request (D3-D14 0x00) or 0x01 for response, D3 button's LED-follow mask, D4 button's LED-off mask, D5 button's X10 house, D6 button's X10 unit, D7 ramp rate, D8 on-level, D9 global LED brightness, D10 non-toggle bitmap (0 = toggle, 1 = non-toggle), D11 button-LED state bitmap (0 = off, 1 = on), D12 X10-All bitmap (0 sends X10 on/off, 1 sends X10 All-on/All-off), D13 button non-toggle on/off bitmap (0 if non-toggle sends Off, 1 if non-toggle sends On), D14 button trigger-all-link bitmap (0 send normal, 1 send ED 0x30 Trigger All-Link Command to first device in ALDB).	
                Set LED-Follow Mask for button: D2 0x02, D3 bitmap (0 = not affected, 1 = associated button's LED follows this button's LED)	 
                Set LED-Off Mask for button: D2 0x03, D3 bitmap (0 = not affected, 1 associated button's LED turns off when this button is pushed).	 
                Set X10 Address. D2 0x04, D3 house, D4 unit.	 
                Set Ramp Rate. D2 0x05, D3 ramp rate from 0x00-0x1F.	 
                Set On-Level for Button. D2 0x06, D3 on level.	 
                Set Global LED Brightness. D2 0x07, D3 0x11-0x7F brightness for all LEDs when on.	 
                Set Non-Toggle State for button. D2 0x08, D3 0x00 for toggle, 0x01 for non-toggle.	 
                Set LED State for button. D2 0x09, D3 0x00 for off, 0x01 for on.	 
                Set X10 All-On State for button. D2 0x0A, D3 0x00 send X10 on/off, 0x01 send X10 All-On/All-Off.	 
                Set Non-Toggle On/Off State for button. D2 0x0B, D3 0x00 send off, 0x01 send on.	 
                Set Trigger All-Link State for button. D2 0x0C, D3 0x00 send normal command, 0x01 send ED 0x30 Trigger All-Link Command to first device in ALDB.	 
         
         
         DevCat 0x01 (except KeypadLinc)
         D1: button/group number, D2 0x00 for data request (D3-D14 0x00) or 0x01 for response, D3-D4 unused, D5-D6: X10 house/unit, D7: ramp rate 0x00-0x1F, D8: on-level, D9: signal-to-noise threshold, D10-D14 unused.	
                Set X10 Address. D2 0x04, D3 house, D4 unit.	(also DevCat 0x02, but
                Set Ramp Rate. D2 0x05, D3 ramp rate from 0x00-0x1F.	ramp rate and on-level
                Set On-Level. D2 0x06, D3 on level.	don't apply)
         
         */
        public const byte EXT_COMMAND_EXTENDED_GET_SET = 0x2E;

        // ALDB Record Request: D1 unused, D2 0x00, D3 address high byte, D4 address low byte, D5 0x00 
        // to get all record or any other value to get one record, D6-D14 unused. 
        // First address is at 0x0000 (not necessarily the actual memory location, just start of ALDB).

        // ALDB Record Response: D1 unused, D2 0x01, D3-D4 address, D5 unused, D6-D13: 8-byte record. 
        // If returning multiple records, the address is decremented by 8 for each record.

        // ALDB Write Record: D1 unused, D2 0x02, D3-D4 address, D5 number of bytes (0x01-0x08), D6-D13 data to write.
        public const byte EXT_COMMAND_READ_WRITE_ALDB = 0x2F;

        // Tells recipient device to send a command to linked devices. D1 button/group number, D2 on-level switch (0x00 use on-level in ALDB, 0x01 use on-level in D3), 
        // D3 on-level if D2 = 0x01, D4 command1 to send, D5 command2 to send, D6 ramp rate switch (0x00 use ramp rate in ALDB, 0x01 instant).
        public const byte EXT_COMMAND_TRIGGER_ALL_LINK_CMD = 0x30;

        #endregion

        #region Group Commands
        // group commands
        public const byte GRP_COMMAND_ASSIGN_TO_GROUP = 0x01;
        public const byte GRP_COMMAND_DELETE_FROM_GROUP = 0x02;
        public const byte GRP_COMMAND_ALL_LINK_RECALL = 0x11;
        public const byte GRP_COMMAND_ALL_LINK_ALIAS_2_HIGH = 0x12;
        public const byte GRP_COMMAND_ALL_LINK_ALIAS_1_LOW = 0x13;
        public const byte GRP_COMMAND_ALL_LINK_ALIAS_2_LOw = 0x14;
        public const byte GRP_COMMAND_ALL_LINK_ALIAS_3_HIGH = 0x15;
        public const byte GRP_COMMAND_ALL_LINK_ALIAS_3_LOw = 0x16;
        public const byte GRP_COMMAND_ALL_LINK_ALIAS_4_HIGH = 0x17;
        public const byte GRP_COMMAND_ALL_LINK_ALIAS_4_LOw = 0x18;
        public const byte GRP_COMMAND_ALL_LINK_ALIAS_5 = 0x21;
        #endregion

        //100 = Broadcast Message

        //000 = Direct Message
        //001 = ACK of Direct Message
        //101 = NAK of Direct Message

        //110 = Group Broadcast Message
        //010 = Group Cleanup Direct Message
        //011 = ACK of Group Cleanup Direct Message
        //111 = NAK of Group Cleanup Direct Message
        public const byte FLAG_BROADCAST = 0x80;
        public const byte FLAG_GROUP = 0x40;
        public const byte FLAG_ACK = 0x20;
        public const byte FLAG_EXTENDED = 0x10;

        // IM COMMANDS
        public const byte IM_COMMAND_STANDARD_MESSAGE_RECEIVED = 0x50;
        public const byte IM_COMMAND_EXTENDED_MESSAGE_RECEIVED = 0x51;
        public const byte IM_COMMAND_ALL_LINKING_COMPLETED = 0x53;
        public const byte IM_COMMAND_BUTTON_EVENT_REPORT = 0x54;
        public const byte IM_COMMAND_USER_RESET_DETECTED = 0x55;
        public const byte IM_COMMAND_SEND_STANDARD_OR_EXTENDED_MSG = 0x62;
        public const byte IM_COMMAND_SET_IM_CONFIGURATION = 0x6B;
        public const byte IM_COMMAND_SEND_ALL_LINK_COMMAND = 0x61;
        public const byte IM_COMMAND_ALL_LINK_CLEANUP_FAILURE_REPORT = 0x56;
        public const byte IM_COMMAND_ALL_LINK_CLEANUP_STATUS_REOPRT = 0x58;
        public const byte IM_COMMAND_START_ALL_LINKING = 0x64;
        public const byte IM_COMMAND_CANCEL_ALL_LINKING = 0x65;
        public const byte IM_COMMAND_ALL_LINK_RECORD_RESPONSE = 0x57;
        public const byte IM_COMMAND_RESET_IM = 0x67;


        // Thermostat Control Info (Cmd2 for STD_COMMAND_THERMOSTAT_CONTROL)
        public const byte THERMOSTAT_CONTROL_SET_HEAT_MODE = 0x04;
        public const byte THERMOSTAT_CONTROL_SET_COOL_MODE = 0x05;
        public const byte THERMOSTAT_CONTROL_SET_AUTO_MODE = 0x06;
        public const byte THERMOSTAT_CONTROL_SET_FAN_ON = 0x07;
        public const byte THERMOSTAT_CONTROL_SET_FAN_OFF = 0x08;
        public const byte THERMOSTAT_CONTROL_SET_FAN_AUTO = 0x09;
        public const byte THERMOSTAT_CONTROL_SET_ALL_OFF = 0x0A;


        // Thermostat status (Cmd when receiving thermostat status 0x6A)
        public const byte THERMOSTAT_STATUS_TEMP = 0x00;
        public const byte THERMOSTAT_STATUS_SETPOINT = 0x20;
        public const byte THERMOSTAT_STATUS_HUMIDITY = 0x60;
    }
}
