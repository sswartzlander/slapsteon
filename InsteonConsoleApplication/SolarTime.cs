using System;
using System.Diagnostics;

namespace Insteon
{
    public class SolarTime
    {
        private double _latitude;
        private double _longitude;

        private double RadianToDegree(double angleRadian)
        {
            return (180.0 * angleRadian / Math.PI);
        }

        private double DegreeToRadian(double angleDegree)
        {
            return (Math.PI * angleDegree / 180.0);
        }

        private double CalculateGeometricMeanLongSun(double t)
        {
            double L0 = 280.46646 + t * (36000.76983 + t * (0.0003032));
            while (L0 > 360.0)
                L0 -= 360.0;
            while (L0 < 0.0)
                L0 += 360.0;

            return L0; // in degrees
        }

        private double CalculateGeometricMeanAnomalySun(double t)
        {
            double M = 357.52911 + t * (35999.05029 - 0.0001537 * t);
            return M;
        }

        private double CalculateEccentricityEarthOrbit(double t)
        {
            double e = 0.016708634 - t * (0.000042037 + 0.0000001267 * t);
            return e;
        }

        private double CalculateSunEqOfCenter(double t)
        {
            double m = CalculateGeometricMeanLongSun(t);
            double mrad = DegreeToRadian(m);
            double sinm = Math.Sin(mrad);
            double sin2m = Math.Sin(mrad + mrad);
            double sin3m = Math.Sin(mrad + mrad + mrad);
            var C = sinm * (1.914602 - t * (0.004817 + 0.000014 * t)) + sin2m * (0.019993 - 0.000101 * t) + sin3m * 0.000289;
            return C; // in degrees
        }

        private double CalculateSunTrueLong(double t)
        {
            double l0 = CalculateGeometricMeanLongSun(t);
            double c = CalculateSunEqOfCenter(t);
            double O = l0 + c;
            return O; // in degrees
        }

        private double CalculateSunTrueAnomaly(double t)
        {
            double m = CalculateGeometricMeanAnomalySun(t);
            double c = CalculateSunEqOfCenter(t);
            double v = m + c;
            return v;
        }

        private double CalculateSunRadVector(double t)
        {
            double v = CalculateSunTrueAnomaly(t);
            double e = CalculateEccentricityEarthOrbit(t);
            double R = (1.000001018 * (1 - e * e)) / (1 + e * Math.Cos(DegreeToRadian(v)));
            return R; // in AUs
        }

        private double CalculateSunApparentLongitude(double t)
        {
            double o = CalculateSunTrueLong(t);
            double omega = 125.04 - 1934.136 * t;
            double lambda = o - 0.00569 - 0.00478 * Math.Sin(DegreeToRadian(omega));
            return lambda;		// in degrees
        }

        private double CalculateMeanObliquityOfEcliptic(double t)
        {
            double seconds = 21.448 - t * (46.8150 + t * (0.00059 - t * (0.001813)));
            double e0 = 23.0 + (26.0 + (seconds / 60.0)) / 60.0;
            return e0;		// in degrees
        }

        private double CalculateObliquityCorrection(double t)
        {
            double e0 = CalculateMeanObliquityOfEcliptic(t);
            double omega = 125.04 - 1934.136 * t;
            double e = e0 + 0.00256 * Math.Cos(DegreeToRadian(omega));
            return e;		// in degrees
        }

        private double CAlculateSunRightAscension(double t)
        {
            var e = CalculateObliquityCorrection(t);
            var lambda = CalculateSunApparentLongitude(t);
            var tananum = (Math.Cos(DegreeToRadian(e)) * Math.Sin(DegreeToRadian(lambda)));
            var tanadenom = (Math.Cos(DegreeToRadian(lambda)));
            var alpha = RadianToDegree(Math.Atan2(tananum, tanadenom));
            return alpha;		// in degrees
        }

        private double CalculateSunDeclination(double t)
        {
            double e = CalculateObliquityCorrection(t);
            double lambda = CalculateSunApparentLongitude(t);

            double sint = Math.Sin(DegreeToRadian(e)) * Math.Sin(DegreeToRadian(lambda));
            double theta = RadianToDegree(Math.Asin(sint));
            return theta;		// in degrees
        }

        private double CalculateEquationOfTime(double t)
        {
            var epsilon = CalculateObliquityCorrection(t);
            var l0 = CalculateGeometricMeanLongSun(t);
            var e = CalculateEccentricityEarthOrbit(t);
            var m = CalculateGeometricMeanAnomalySun(t);

            var y = Math.Tan(DegreeToRadian(epsilon) / 2.0);
            y *= y;

            var sin2l0 = Math.Sin(2.0 * DegreeToRadian(l0));
            var sinm = Math.Sin(DegreeToRadian(m));
            var cos2l0 = Math.Cos(2.0 * DegreeToRadian(l0));
            var sin4l0 = Math.Sin(4.0 * DegreeToRadian(l0));
            var sin2m = Math.Sin(2.0 * DegreeToRadian(m));

            var Etime = y * sin2l0 - 2.0 * e * sinm + 4.0 * e * y * sinm * cos2l0 - 0.5 * y * y * sin4l0 - 1.25 * e * e * sin2m;
            return RadianToDegree(Etime) * 4.0;	// in minutes of time
        }

        private double CalculateHourAngleSunrise(double latitude, double solarDec)
        {
            var latRad = DegreeToRadian(latitude);
            var sdRad = DegreeToRadian(solarDec);
            var HAarg = (Math.Cos(DegreeToRadian(90.833)) / (Math.Cos(latRad) * Math.Cos(sdRad)) - Math.Tan(latRad) * Math.Tan(sdRad));
            var HA = Math.Acos(HAarg);
            return HA;		// in radians (for sunset, use -HA)
        }

        private int GetJulianDay()
        {
            int N1 = (int)Math.Floor(275 * DateTime.Now.Month / (double)9);
            int N2 = (int)Math.Floor((DateTime.Now.Month + 9) / (double)12);
            int N3 = (int)(1 + Math.Floor((DateTime.Now.Year - 4 * Math.Floor(DateTime.Now.Year / (double)4) + (double)2) / (double)3));
            int N = N1 - (N2 * N3) + DateTime.Now.Day - 30;

            return N;
        }

        private double CalculateTimeJulianCent(double julianDay)
        {
            double T = (julianDay - 2451545.0) / 36525.0;
            return T;
        }

        private bool IsLeapYear(int year)
        {
            return ((year % 4 == 0 && year % 100 != 0) || year % 400 == 0);
        }

        private double CalculateSunriseSetUTC(bool sunrise, double julianDay, double latitude, double longitude)
        {
            double t = CalculateTimeJulianCent(julianDay);
            double eqTime = CalculateEquationOfTime(t);
            double solarDec = CalculateSunDeclination(t);
            double hourAngle = CalculateHourAngleSunrise(latitude, solarDec);
            if (!sunrise) hourAngle = -hourAngle;
            double delta = longitude + RadianToDegree(hourAngle);
            double timeUTC = 720 - (4.0 * delta) - eqTime; // in minutes
            return timeUTC;
        }

        private double GetJD()
        {
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;
            int currentDay = DateTime.Now.Day;

            if (currentMonth <= 2)
            {
                currentYear -= 1;
                currentMonth += 12;
            }

            double A = Math.Floor(currentYear / 100.0f);
            double B = 2 - A + Math.Floor(A / 4);
            double JD = Math.Floor(365.25 * (currentYear + 4716)) + Math.Floor(30.6001 * (currentMonth + 1)) + currentDay + B - 1524.5;

            return JD;
        }


        public double CalculateSunriseOrSunset(bool sunrise, double latitude, double longitude, double timezoneOffset, bool isDST)
        {
            double JD = GetJD();

            double timeUTC = CalculateSunriseSetUTC(sunrise, JD, latitude, longitude);
            double newTimeUTC = CalculateSunriseSetUTC(sunrise, JD + timeUTC / 1440.0, latitude, longitude);
            if (newTimeUTC == Double.NaN)
                return -1;

            double timeLocal = newTimeUTC + (timezoneOffset * 60.0);
            timeLocal += isDST ? 60.0 : 0;

            return timeLocal;

        }
    }
}