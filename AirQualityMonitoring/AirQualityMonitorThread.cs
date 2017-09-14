using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirQualityMonitoring
{
    public class AirQualityMonitorThread
    {
        private DateTime _minuteStatsLastWrittenToDB = DateTime.Now;
        private DateTime _hourStatsLastWrittenToDB = DateTime.Now;

        private string _name;
        private string _uri;
        private bool _running = false;
        private Thread _monitorThread = null;

        private List<AirQualityStats> _statsList = new List<AirQualityStats>();

        public double AveragePM01_Minute { get; set; }
        public double AveragePM01_Hour { get; set; }
        public double AveragePM25_Minute { get; set; }
        public double AveragePM25_Hour { get; set; }
        public double AveragePM10_Minute { get; set; }
        public double AveragePM10_Hour { get; set; }


        public double AveragePM01_CF1_Minute { get; set; }
        public double AveragePM01_CF1_Hour { get; set; }
        public double AveragePM25_CF1_Minute { get; set; }
        public double AveragePM25_CF1_Hour { get; set; }
        public double AveragePM10_CF1_Minute { get; set; }
        public double AveragePM10_CF1_Hour { get; set; }


        public double AveragePoint3Micron_Minute { get; set; }
        public double AveragePoint3Micron_Hour { get; set; }

        public double AveragePoint5Micron_Minute { get; set; }
        public double AveragePoint5Micron_Hour { get; set; }

        public double AverageOneMicron_Minute { get; set; }
        public double AverageOneMicron_Hour { get; set; }

        public double AverageTwoPointFiveMicron_Minute { get; set; }

        public double AverageTwoPointFiveMicron_Hour { get; set; }

        public double AverageFiveMicron_Minute { get; set; }
        public double AverageFiveMicron_Hour { get; set; }

        public double AverageTenMicron_Minute { get; set; }

        public double AverageTenMicron_Hour { get; set; }

        public double Max_PM01_Minute { get; set; }
        public double Max_PM01_Hour { get; set; }
        public double Max_PM25_Minute { get; set; }
        public double Max_PM25_Hour { get; set; }
        public double Max_PM10_Minute { get; set; }
        public double Max_PM10_Hour { get; set; }


        public double Max_PM01_CF1_Minute { get; set; }
        public double Max_PM01_CF1_Hour { get; set; }
        public double Max_PM25_CF1_Minute { get; set; }
        public double Max_PM25_CF1_Hour { get; set; }
        public double Max_PM10_CF1_Minute { get; set; }
        public double Max_PM10_CF1_Hour { get; set; }

        public double Max_Point3Micron_Minute { get; set; }
        public double Max_Point3Micron_Hour { get; set; }

        public double Max_Point5Micron_Minute { get; set; }
        public double Max_Point5Micron_Hour { get; set; }

        public double Max_OneMicron_Minute { get; set; }
        public double Max_OneMicron_Hour { get; set; }

        public double Max_TwoPointFiveMicron_Minute { get; set; }

        public double Max_TwoPointFiveMicron_Hour { get; set; }

        public double Max_FiveMicron_Minute { get; set; }
        public double Max_FiveMicron_Hour { get; set; }

        public double Max_TenMicron_Minute { get; set; }

        public double Max_TenMicron_Hour { get; set; }


        public Thread WorkerThread
        {
            get { return _monitorThread; }
        }

        public string Name
        {
            get { return _name; }
        }

        public AirQualityMonitorThread(string name, string uri)
        {
            _name = name;
            _uri = uri;
        }

        public void Start()
        {
            _monitorThread = new Thread(new ThreadStart(Run));
            _running = true;
            _monitorThread.Start();

        }

        public void Stop()
        {
            _running = false;
        }

        private void Run()
        {
            while (_running)
            {
                AirQualityStats stats = ReadStats(_uri);
                if (null != stats)
                {
                    ProcessAirQualityStats(stats);
                    ProcessWritingStatsToDatabase();
                }
                Thread.Sleep(5000);
            }
        }

        private AirQualityStats ReadStats(string uri)
        {
            AirQualityStats stats = null;
            try
            {
                HttpWebRequest request = WebRequest.Create(_uri) as HttpWebRequest;
                request.Method = "GET";
                request.KeepAlive = false;

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AirQualityStats));

                        stats = (AirQualityStats)serializer.ReadObject(responseStream);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return stats;
        }

        private void ProcessAirQualityStats(AirQualityStats stats)
        {
            if (null != stats)
                _statsList.Add(stats);

            // prune old data
            _statsList = _statsList.Where(sl => sl.Timestamp > DateTime.Now.AddDays(-1)).ToList();

            List<AirQualityStats> minuteStats = _statsList.Where(sl => sl.Timestamp > DateTime.Now.AddMinutes(-1)).ToList();
            List<AirQualityStats> hourStats = _statsList.Where(sl => sl.Timestamp > DateTime.Now.AddHours(-1)).ToList();

            AveragePM01_CF1_Minute = (double)minuteStats.Sum(s => s.PM01ConcentrationCF1) / (double)minuteStats.Count;
            AveragePM25_CF1_Minute = (double)minuteStats.Sum(s => s.PM25ConcentrationCF1) / (double)minuteStats.Count;
            AveragePM10_CF1_Minute = (double)minuteStats.Sum(s => s.PM10ConcentrationCF1) / (double)minuteStats.Count;

            AveragePM01_Minute = (double)minuteStats.Sum(s => s.PM01ConcentrationAtm) / (double)minuteStats.Count;
            AveragePM25_Minute = (double)minuteStats.Sum(s => s.PM25ConcentrationAtm) / (double)minuteStats.Count;
            AveragePM10_Minute = (double)minuteStats.Sum(s => s.PM10ConcentrationAtm) / (double)minuteStats.Count;
            AveragePoint3Micron_Minute = (double)minuteStats.Sum(s => s.Point3MicronCount) / (double)minuteStats.Count;
            AveragePoint5Micron_Minute = (double)minuteStats.Sum(s => s.Point5MicronCount) / (double)minuteStats.Count;
            AverageOneMicron_Minute = (double)minuteStats.Sum(s => s.OneMicronCount) / (double)minuteStats.Count;
            AverageTwoPointFiveMicron_Minute = (double)minuteStats.Sum(s => s.TwoPointFiveMicronCount) / (double)minuteStats.Count;
            AverageFiveMicron_Minute = (double)minuteStats.Sum(s => s.FiveMicronCount) / (double)minuteStats.Count;
            AverageTenMicron_Minute = (double)minuteStats.Sum(s => s.TenMicronCount) / (double)minuteStats.Count;

            AveragePM01_CF1_Hour = (double)hourStats.Sum(s => s.PM01ConcentrationCF1) / (double)hourStats.Count;
            AveragePM25_CF1_Hour = (double)hourStats.Sum(s => s.PM25ConcentrationCF1) / (double)hourStats.Count;
            AveragePM10_CF1_Hour = (double)hourStats.Sum(s => s.PM10ConcentrationCF1) / (double)hourStats.Count;
            AveragePM01_Hour = (double)hourStats.Sum(s => s.PM01ConcentrationAtm) / (double)hourStats.Count;
            AveragePM25_Hour = (double)hourStats.Sum(s => s.PM25ConcentrationAtm) / (double)hourStats.Count;
            AveragePM10_Hour = (double)hourStats.Sum(s => s.PM10ConcentrationAtm) / (double)hourStats.Count;
            AveragePoint3Micron_Hour = (double)hourStats.Sum(s => s.Point3MicronCount) / (double)hourStats.Count;
            AveragePoint5Micron_Hour = (double)hourStats.Sum(s => s.Point5MicronCount) / (double)hourStats.Count;
            AverageOneMicron_Hour = (double)hourStats.Sum(s => s.OneMicronCount) / (double)hourStats.Count;
            AverageTwoPointFiveMicron_Hour = (double)hourStats.Sum(s => s.TwoPointFiveMicronCount) / (double)hourStats.Count;
            AverageFiveMicron_Hour = (double)hourStats.Sum(s => s.FiveMicronCount) / (double)hourStats.Count;
            AverageTenMicron_Hour = (double)hourStats.Sum(s => s.TenMicronCount) / (double)hourStats.Count;

            // compute maximums
            if (stats.PM01ConcentrationCF1 >= hourStats.Max(s => s.PM01ConcentrationCF1))
                Max_PM01_CF1_Hour = stats.PM01ConcentrationCF1;
            if (stats.PM25ConcentrationCF1 >= hourStats.Max(s => s.PM25ConcentrationCF1))
                Max_PM25_CF1_Hour = stats.PM25ConcentrationCF1;
            if (stats.PM10ConcentrationCF1 >= hourStats.Max(s => s.PM10ConcentrationCF1))
                Max_PM10_CF1_Hour = stats.PM10ConcentrationCF1;
            if (stats.PM01ConcentrationAtm >= hourStats.Max(s => s.PM01ConcentrationAtm))
                Max_PM01_Hour = stats.PM01ConcentrationAtm;
            if (stats.PM25ConcentrationAtm >= hourStats.Max(s => s.PM25ConcentrationAtm))
                Max_PM25_Hour = stats.PM25ConcentrationCF1;
            if (stats.PM10ConcentrationAtm >= hourStats.Max(s => s.PM10ConcentrationAtm))
                Max_PM10_Hour = stats.PM10ConcentrationAtm;
            if (stats.Point3MicronCount >= hourStats.Max(s => s.Point3MicronCount))
                Max_Point3Micron_Hour = stats.Point3MicronCount;
            if (stats.Point5MicronCount >= hourStats.Max(s => s.Point5MicronCount))
                Max_Point5Micron_Hour = stats.Point5MicronCount;
            if (stats.OneMicronCount >= hourStats.Max(s => s.OneMicronCount))
                Max_OneMicron_Hour = stats.OneMicronCount;
            if (stats.TwoPointFiveMicronCount >= hourStats.Max(s => s.TwoPointFiveMicronCount))
                Max_TwoPointFiveMicron_Hour = stats.TwoPointFiveMicronCount;
            if (stats.FiveMicronCount >= hourStats.Max(s => s.FiveMicronCount))
                Max_FiveMicron_Hour = stats.FiveMicronCount;
            if (stats.TenMicronCount >= hourStats.Max(s => s.TenMicronCount))
                Max_TenMicron_Hour = stats.TenMicronCount;

            if (stats.PM01ConcentrationCF1 >= minuteStats.Max(s => s.PM01ConcentrationCF1))
                Max_PM01_CF1_Minute = stats.PM01ConcentrationCF1;
            if (stats.PM25ConcentrationCF1 >= minuteStats.Max(s => s.PM25ConcentrationCF1))
                Max_PM25_CF1_Minute = stats.PM25ConcentrationCF1;
            if (stats.PM10ConcentrationCF1 >= minuteStats.Max(s => s.PM10ConcentrationCF1))
                Max_PM10_CF1_Minute = stats.PM10ConcentrationCF1;
            if (stats.PM01ConcentrationAtm >= minuteStats.Max(s => s.PM01ConcentrationAtm))
                Max_PM01_Minute = stats.PM01ConcentrationAtm;
            if (stats.PM25ConcentrationAtm >= minuteStats.Max(s => s.PM25ConcentrationAtm))
                Max_PM25_Minute = stats.PM25ConcentrationCF1;
            if (stats.PM10ConcentrationAtm >= minuteStats.Max(s => s.PM10ConcentrationAtm))
                Max_PM10_Minute = stats.PM10ConcentrationAtm;
            if (stats.Point3MicronCount >= minuteStats.Max(s => s.Point3MicronCount))
                Max_Point3Micron_Minute = stats.Point3MicronCount;
            if (stats.Point5MicronCount >= minuteStats.Max(s => s.Point5MicronCount))
                Max_Point5Micron_Minute = stats.Point5MicronCount;
            if (stats.OneMicronCount >= minuteStats.Max(s => s.OneMicronCount))
                Max_OneMicron_Minute = stats.OneMicronCount;
            if (stats.TwoPointFiveMicronCount >= minuteStats.Max(s => s.TwoPointFiveMicronCount))
                Max_TwoPointFiveMicron_Minute = stats.TwoPointFiveMicronCount;
            if (stats.FiveMicronCount >= minuteStats.Max(s => s.FiveMicronCount))
                Max_FiveMicron_Minute = stats.FiveMicronCount;
            if (stats.TenMicronCount >= minuteStats.Max(s => s.TenMicronCount))
                Max_TenMicron_Minute = stats.TenMicronCount;

        }

        private void ProcessWritingStatsToDatabase()
        {
            DateTime currentTime = DateTime.Now;
            // write minute stats if they haven't been written yet for this hour
            if (currentTime.Date != _minuteStatsLastWrittenToDB.Date || currentTime.Hour != _minuteStatsLastWrittenToDB.Hour || currentTime.Minute != _minuteStatsLastWrittenToDB.Minute)
            {
                _minuteStatsLastWrittenToDB = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, 0);
                WriteMinuteStatsToDB();
            }

            // write hour stats if they haven't been written yet for this hour
            if (currentTime.Date != _hourStatsLastWrittenToDB.Date || currentTime.Hour != _hourStatsLastWrittenToDB.Hour)
            {
                _hourStatsLastWrittenToDB = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 0, 0);
                WriteHourStatsToDB();
            }
        }

        private void WriteHourStatsToDB()
        {
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection("Server=192.168.222.205; database=airquality; UID=airqual; password=airqual;");
                connection.Open();

                using (MySqlCommand command = new MySqlCommand("createmonitorrecord_hour", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.Add(DatabaseUtilities.CreateStringParameter(_name, "hostname"));
                    command.Parameters.Add(DatabaseUtilities.CreateDateTimeParameter(_hourStatsLastWrittenToDB, "in_timestamp"));
                    command.Parameters.Add(DatabaseUtilities.CreateDoubleParameter(AveragePM01_CF1_Hour, "in_pm1cf1"));
                    command.Parameters.Add(DatabaseUtilities.CreateDoubleParameter(AveragePM25_CF1_Hour, "in_pm25cf1"));
                    command.Parameters.Add(DatabaseUtilities.CreateDoubleParameter(AveragePM10_CF1_Hour, "in_pm10cf1"));
                    command.Parameters.Add(DatabaseUtilities.CreateDoubleParameter(AveragePM01_Hour, "in_pm1atm"));
                    command.Parameters.Add(DatabaseUtilities.CreateDoubleParameter(AveragePM25_Hour, "in_pm25atm"));
                    command.Parameters.Add(DatabaseUtilities.CreateDoubleParameter(AveragePM10_Hour, "in_pm10atm"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)AveragePoint3Micron_Hour, "in_micron03"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)AveragePoint5Micron_Hour, "in_micron05"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)AverageOneMicron_Hour, "in_micron10"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)AverageTwoPointFiveMicron_Hour, "in_micron25"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)AverageFiveMicron_Hour, "in_micron50"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)AverageTenMicron_Hour, "in_micron100"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_PM01_Hour, "in_pm1cf1_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_PM25_Hour, "in_pm25cf1_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_PM10_Hour, "in_pm10cf1_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_PM01_CF1_Hour, "in_pm1atm_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_PM25_CF1_Hour, "in_pm25atm_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_PM10_CF1_Hour, "in_pm10atm_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_Point3Micron_Hour, "in_micron03_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_Point5Micron_Hour, "in_micron05_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_OneMicron_Hour, "in_micron10_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_TwoPointFiveMicron_Hour, "in_micron25_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_FiveMicron_Hour, "in_micron50_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_TenMicron_Hour, "in_micron100_max"));

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (null != connection)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        private void WriteMinuteStatsToDB()
        {
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection("Server=192.168.222.205; database=airquality; UID=airqual; password=airqual;");
                connection.Open();

                using (MySqlCommand command = new MySqlCommand("createmonitorrecord", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.Add(DatabaseUtilities.CreateStringParameter(_name, "hostname"));
                    command.Parameters.Add(DatabaseUtilities.CreateDateTimeParameter(_minuteStatsLastWrittenToDB, "in_timestamp"));
                    command.Parameters.Add(DatabaseUtilities.CreateDoubleParameter(AveragePM01_CF1_Minute, "in_pm1cf1"));
                    command.Parameters.Add(DatabaseUtilities.CreateDoubleParameter(AveragePM25_CF1_Minute, "in_pm25cf1"));
                    command.Parameters.Add(DatabaseUtilities.CreateDoubleParameter(AveragePM10_CF1_Minute, "in_pm10cf1"));
                    command.Parameters.Add(DatabaseUtilities.CreateDoubleParameter(AveragePM01_Minute, "in_pm1atm"));
                    command.Parameters.Add(DatabaseUtilities.CreateDoubleParameter(AveragePM25_Minute, "in_pm25atm"));
                    command.Parameters.Add(DatabaseUtilities.CreateDoubleParameter(AveragePM10_Minute, "in_pm10atm"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)AveragePoint3Micron_Minute, "in_micron03"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)AveragePoint5Micron_Minute, "in_micron05"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)AverageOneMicron_Minute, "in_micron10"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)AverageTwoPointFiveMicron_Minute, "in_micron25"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)AverageFiveMicron_Minute, "in_micron50"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)AverageTenMicron_Minute, "in_micron100"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_PM01_Minute, "in_pm1cf1_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_PM25_Minute, "in_pm25cf1_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_PM10_Minute, "in_pm10cf1_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_PM01_CF1_Minute, "in_pm1atm_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_PM25_CF1_Minute, "in_pm25atm_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_PM10_CF1_Minute, "in_pm10atm_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_Point3Micron_Minute, "in_micron03_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_Point5Micron_Minute, "in_micron05_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_OneMicron_Minute, "in_micron10_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_TwoPointFiveMicron_Minute, "in_micron25_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_FiveMicron_Minute, "in_micron50_max"));
                    command.Parameters.Add(DatabaseUtilities.CreateInt32Parameter((int)Max_TenMicron_Minute, "in_micron100_max"));

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (null != connection)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
    }
}
