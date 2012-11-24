using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.ServiceModel.Web;
using Insteon.Library;
using System.Configuration;
using log4net;

namespace Insteon
{
    class Program
    {
        private static bool _rising = false;

        static void Main(string[] args)
        {
            SolarTime solarTime = new SolarTime();

            double sunset = solarTime.CalculateSunriseOrSunset(false, 29.382175, -98.61328, -6, true);
            double sunrise = solarTime.CalculateSunriseOrSunset(true, 29.382175, -98.61328, -6, true);


            double zenith = 102;
            double longitude = -98.3;
            double latitude = 29.25;
            double localOffset = TimeZoneInfo.Local.BaseUtcOffset.Hours;

            // calculate the day of the year
            int N1 = (int)Math.Floor(275 * DateTime.Now.Month / (double)9);
            int N2 = (int)Math.Floor((DateTime.Now.Month + 9) / (double)12);
            int N3 = (int)(1 + Math.Floor((DateTime.Now.Year - 4 * Math.Floor(DateTime.Now.Year / (double)4) + (double)2) / (double)3));
            int N = N1 - (N2 * N3) + DateTime.Now.Day - 30;

            // convert the longitude to hour value and calculate an approximate time
            double t = 0;
            double lngHour = longitude / 15; // longitude
            if (_rising)
            {
                t = N + ((6 - lngHour) / 24);
            }
            else
            {
                t = N + ((18 - lngHour) / 24);
            }

            // calculate the sun's mean anomaly
            double M = (0.9856 * t) - 3.289;

            // calcuate the sun's true longitude
            double L = M + (1.916 + Math.Sin((180 / Math.PI) * M)) + (0.020 * Math.Sin((180 / Math.PI) * 2 * M)) + 282.634; // note: L potentially needs to be adjusted into th range [0,360) by adding/subtracting 360
            if (L > 360) L -= 360;
            else if (L < 0) L += 360;

            // calculate the sun's right ascension
            double RA = (180 / Math.PI) * Math.Atan(0.91764 * Math.Tan((180 / Math.PI) * L)); // note: L potentially needs to be adjusted into the range [0,360) by adding/subtracting 360

            // right ascension value needs to be in the same quadrant as L
            double LQuadrant = Math.Floor(L / 90) * 90;
            double RAQuadrant = Math.Floor(RA / 90) * 90;
            RA = RA + (LQuadrant - RAQuadrant);

            // right ascension values need to be converted into hours
            RA = RA / 15;

            // calculate the sun's declination
            double sinDec = 0.39782 * Math.Sin((180 / Math.PI) * L);
            double cosDec = Math.Cos(Math.Asin(sinDec));

            // calculate the sun's local hour angle

            double cosH = (Math.Cos((180 / Math.PI) * zenith) - (sinDec * Math.Sin((180 / Math.PI) * latitude))) / (cosDec * Math.Cos((180 / Math.PI) * latitude));
            if (cosH > 1)
                ; // sun never rises
            if (cosH < -1)
                ; // sun never sets


            // finish calculate H and conver into hours
            double H = 0;
            if (_rising)
                H = 360 - ((180/Math.PI) * Math.Acos(cosH));
            else
                H = Math.Acos(cosH);

            H = H / 15;

            // calculate local mean time of rising/setting
            double T = H + RA - (0.6571 * t) - 6.622;

            // adjust back to UTC
            double UT = T - lngHour; // note: UT potentially needs to be adjusted into the range [0,24) by adding/subtracting 24

            // convert UT value to local time zone of lititude/longitude
            double localT = UT + localOffset;

        }

        
    }
}
