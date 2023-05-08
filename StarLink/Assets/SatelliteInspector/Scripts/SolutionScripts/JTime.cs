using System;
using SatelliteInspector.Scripts.PrefabScripts;
using UnityEngine;

namespace SatelliteInspector.Scripts.SolutionScripts
{
    /// <summary>
    /// Julian time
    /// </summary>
    public class JTime : ScriptableObject
    {
        public double Date;
        public int Year;
        public double Day;

        const double JDayConst = 2415020.0;
        /// <summary>
        /// Delta time
        /// </summary>
        public double J1900 => Date - JDayConst;

        /// <summary>
        /// Local mean sidereal time
        /// </summary>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public double ToLmsTime(double longitude) => (ToGmsTime() + longitude) % (Math.PI * 2);

        /// <summary>
        /// Returns a UTC DateTime
        /// </summary>
        /// <returns></returns>
        public DateTime ToTime() => new DateTime(Year, 1, 1).AddDays(Day - 1);

        /// <summary>
        /// Create new julian time
        /// </summary>
        /// <param name="year">year</param>
        /// <param name="doy">day of the year</param>1
        public void CreateNew(int year, double doy)
        {
            const double coef1 = 365.25;
            const double coef2 = 30.6001;
            const double coef3 = 14.0;
            const double coef4 = 1720994.5;

            Year = year;
            Day = doy;
            year--;
            Date = (int)(coef1 * year) + (int)(coef2 * coef3) + coef4 + (2.0 - (year / 100.0) + (year / 100.0 / 4.0)) +
                   doy;
        }

        /// <summary>
        /// Create julian time use DateTime
        /// </summary>
        /// <param name="time">DateTime</param>
        public void InitAtTime(DateTime time)
        {
            const double coef1 = 365.25;
            const double coef2 = 30.6001;
            const double coef3 = 14.0;
            const double coef4 = 1720994.5;

            Year = time.Year;
            Day = time.DayOfYear +
                  (time.Hour + ((time.Minute + ((time.Second + (time.Millisecond / 1000.0)) / 60.0)) / 60.0)) / 24.0;
            double year = Year;
            year--;
            Date = (int)(coef1 * year) + (int)(coef2 * coef3) + coef4 + (2.0 - (year / 100.0) + (year / 100.0 / 4.0)) +
                   Day;
        }

        /// <summary>
        /// Calculate Greenwich mean sidereal time (GMST)
        /// </summary>
        /// <returns></returns>
        public double ToGmsTime()
        {
            double ut = (Date + 0.5) % 1.0;
            double tu = ((Date - 2451545.0) - ut) / 36525.0;
            double gmst = 24110.54841 + tu * (8640184.812866 + tu * (0.093104 - tu * 6.2e-06));
            gmst = (gmst + EarthScript.SecInDay * EarthScript.SideralRotation * ut) % EarthScript.SecInDay;
            if (gmst < 0.0)
            {
                gmst += EarthScript.SecInDay;
            }

            return (2 * Math.PI * (gmst / EarthScript.SecInDay));
        }

        /// <summary>
        /// Add minutes to julian time
        /// </summary>
        /// <param name="min"></param>
        public void AddMin(double min)
        {
            Date += (min / 1440.0);
            Day += (min / 1440.0);
        }
    }
}