using System;
using SatelliteInspector.Scripts.PrefabScripts;
using UnityEngine;

namespace SatelliteInspector.Scripts.SolutionScripts
{
    public class Orbit : ScriptableObject
    {
        /// <summary>
        /// Julian Time
        /// </summary>
        public JTime JT_TLE;
    
        /// <summary>
        /// Delta time utc (+/-)
        /// </summary>
        /// <param name="utc"></param>
        /// <returns></returns>
        private TimeSpan PlusTime(DateTime utc) => utc - JT_TLE.ToTime();

        /// <summary>
        /// Satellites position at UTC time
        /// </summary>
        /// <param name="utc">UTC datetime</param>
        /// <returns></returns>
        public SatVector PositionEci(DateTime utc)
        {
            TimeSpan deltaUTC = DateTime.Now - DateTime.UtcNow;
            return PositionEci(PlusTime(utc).TotalMinutes - deltaUTC.TotalMinutes);
        }

        #region Orbit parameters
        public double Inclination { get; private set; }
        public double Eccentricity { get; private set; }
        public double RAAN { get; private set; }
        public double ArgPerigee { get; private set; }
        public double BStar { get; private set; }
        public double MeanAnomaly { get; private set; }
        public double TleMeanMotion { get; private set; }
        public double MeanMotion { get; private set; }
        public double SemiMajor { get; private set; }
        public double Perigee { get; private set; }
        public double Apogee { get; private set; }
        #endregion

        /// <summary>
        /// Decoder for data files
        /// </summary>
        private Decoder oDec;
        /// <summary>
        /// Solutions for calculate orbit parameters
        /// </summary>
        private ClassSolution MainSolution;
        /// <summary>
        /// Solution D
        /// </summary>
        private SolutionD solD;
        /// <summary>
        /// Solution G
        /// </summary>
        private SolutionG solG;

        /// <summary>
        /// Recognize Unit
        /// </summary>
        /// <param name="fld"></param>
        /// <returns></returns>
        private double GetRad(Decoder.Params fld) => oDec.GetField(fld, Decoder.Unit.Rad);

        /// <summary>
        /// Calculate deltatime
        /// </summary>
        private TimeSpan DeltaTime
        {
            get
            {
                TimeSpan deltaTime = new TimeSpan(0, 0, 0, -1);
                if (deltaTime.TotalSeconds < 0.0)
                {
                    if (MeanMotion == 0.0)
                    {
                        deltaTime = new TimeSpan(0, 0, 0);
                    }
                    else
                    {
                        double secs = ((Math.PI * 2) / MeanMotion) * 60.0;
                        int msecs = (int)((secs - (int)secs) * 1000);
                        deltaTime = new TimeSpan(0, 0, 0, (int)secs, msecs);
                    }
                }
                return deltaTime;
            }
        }
    
        /// <summary>
        /// Create julian time for decoder
        /// </summary>
        private JTime DecoderJT
        {
            get
            {
                JTime resJtime = CreateInstance<JTime>();
                if (oDec != null)
                {
                    int epochYear = (int)oDec.GetField(Decoder.Params.Year);
                    double epochDay = oDec.GetField(Decoder.Params.Day);

                    if (epochYear < 57) { epochYear += 2000; }
                    else
                    {
                        epochYear += 1900;
                    }

                    resJtime.CreateNew(epochYear, epochDay);
                }
                return resJtime;
            }
        }
 
        /// <summary>
        /// Position in ECI system
        /// </summary>
        /// <param name="mpe">Minutes</param>
        /// <returns></returns>
        private SatVector PositionEci(double mpe)
        {
            const double bordertime = 255.0;
            const double const1 = 1440.0;
            const double const2 = 86400.0;
            
            SatVector SatVec = DeltaTime.TotalMinutes >= bordertime ? solD.SolutionVector(this, mpe) : solG.SolutionVector(this, mpe);
            SatVec.Scale(EarthScript.Radius, EarthScript.Radius * (const1 / const2)); 
            return SatVec;
        }

        /// <summary>
        /// Create Orbit from decoded TLE data
        /// </summary>
        /// <param name="dec">Satellite decoder TLE</param>
        public void CreateOrbit(Decoder dec)
        {
            oDec = dec;
            JT_TLE = DecoderJT;

            Inclination = GetRad(Decoder.Params.Incl);
            Eccentricity = oDec.GetField(Decoder.Params.Eccentricity);
            RAAN = GetRad(Decoder.Params.R_A);
            ArgPerigee = GetRad(Decoder.Params.ArgPer);
            BStar = oDec.GetField(Decoder.Params.Drag);
            oDec.GetField(Decoder.Params.FirstMotion);
            MeanAnomaly = GetRad(Decoder.Params.Anomaly);
            TleMeanMotion = oDec.GetField(Decoder.Params.Motion);
            #region CALCULATION VARS
            double var1 = Math.Pow(Math.Sqrt(3600.0 * 398600.8 / Math.Pow(EarthScript.Radius, 3)) / (TleMeanMotion * Math.PI * 2.0 / 1440.0), 2.0 / 3.0);
            double var2 = (1.5 * (1.0826158E-3 / 2.0) * (3.0 * Math.Pow(Math.Cos(Inclination), 2) - 1.0) / Math.Pow(1.0 - Eccentricity * Eccentricity, 1.5));
            double var3 = var1 * (1.0 - var2 / (var1 * var1) * ((1.0 / 3.0) + var2 / (var1 * var1) * (1.0 + 134.0 / 81.0 * (var2 / (var1 * var1)))));
            #endregion
            MeanMotion = TleMeanMotion * Math.PI * 2.0 / 1440.0 / (1.0 + var2 / (var3 * var3));
            SemiMajor = var3 / (1.0 - var2 / (var3 * var3));
            Perigee = EarthScript.Radius * (SemiMajor * (1.0 - Eccentricity) - 1.0);
            Apogee = EarthScript.Radius * (SemiMajor * (1.0 + Eccentricity) - 1.0);

            //choose solution for prediction
            const double endtime = 225.0;
            if (DeltaTime.TotalMinutes >= endtime)
            {
                MainSolution = CreateInstance<SolutionD>();
                MainSolution.RunSolution(this);
                solD = CreateInstance<SolutionD>();
                solD = (SolutionD)MainSolution;
                solD.RunSolution(this);
            }
            else
            {
                MainSolution = CreateInstance<SolutionG>();
                MainSolution.RunSolution(this);
                solG = CreateInstance<SolutionG>();
                solG = (SolutionG)MainSolution;
                solG.RunSolution(this);
            }
        }

    }
}
