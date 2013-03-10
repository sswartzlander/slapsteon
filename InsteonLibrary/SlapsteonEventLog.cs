using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using log4net;

namespace Insteon.Library
{
    [DataContract]
    public static class SlapsteonEventLog
    {
        // linked list of log entry
        private static SlapsteonEventLogEntry _headLogEntry = null;
        private static SlapsteonEventLogEntry _lastLogEntry = null;
        private static int _logSize = 0;
        private const int MAX_LOGSIZE = 20;
        private static ILog log = LogManager.GetLogger("Insteon");

        static SlapsteonEventLog()
        {
            
        }

        public static void AddLogEntry(SlapsteonEventLogEntry entry)
        {
            log.DebugFormat("Adding log entry for device: {0}, description: {1}", entry.DeviceName, entry.Description);
            if (null == _headLogEntry)
            {
                _headLogEntry = entry;
                _lastLogEntry = entry;
                _logSize++;
                return;
            }

            // skip duplicate log entries
            if (null != _lastLogEntry && (_lastLogEntry.DeviceName == entry.DeviceName && _lastLogEntry.Description == entry.Description))
                return;

            if (_logSize < MAX_LOGSIZE)
            {
                _lastLogEntry.NextLogEntry = entry;
                _lastLogEntry = entry;
                _logSize++;
            }
            else
            {
                _lastLogEntry.NextLogEntry = entry;
                _lastLogEntry = entry;

                // move the head forward
                SlapsteonEventLogEntry temp = _headLogEntry.NextLogEntry;
                _headLogEntry = temp;
            }
        }

        public static SlapsteonEventLogEntry[] ToArray() {
            List<SlapsteonEventLogEntry> log = new List<SlapsteonEventLogEntry>();

            for (SlapsteonEventLogEntry e = _headLogEntry; e != null; e = e.NextLogEntry)
            {
                log.Add(e);
            }

            return log.ToArray();
        }

    }
}
