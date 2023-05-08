using System;
using SatelliteInspector.Scripts.PrefabScripts;
using UnityEngine;
using Unity.Mathematics;

namespace SatelliteInspector.Scripts.SolutionScripts
{
    internal abstract class ClassSolution : ScriptableObject
    {
        #region VARIABLES

        protected double sat_inclination;
        protected double sat_eccentricity;
        protected double cos_incl;
        protected double var2;
        protected double sat_angbeta;
        protected double sat_sqrt_angbeta;
        protected double sat_SemiMajor;
        protected double sat_MeanMotion;
        protected double var4;
        protected double var6;
        protected double var7;
        protected double var8;
        protected double var9;
        protected double var10;
        protected double var11;
        protected double var12;
        protected double var15;
        protected double var17;
        protected double var13;
        protected double var16;
        protected double var18;
        protected double var19;
        protected double var20;
        protected double var21;
        protected double var22;
        protected double var23;
        protected double var24;
        protected double var25;

        #endregion

        public virtual SatVector SolutionVector(Orbit orbit, double deltaTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Run precalculate solution
        /// </summary>
        /// <param name="orbit">Satellite orbit</param>
        public void RunSolution(Orbit orbit)
        {
            sat_inclination = orbit.Inclination;
            sat_eccentricity = orbit.Eccentricity;
            cos_incl = Math.Cos(sat_inclination);
            double var1 = Math.Pow(cos_incl, 2);
            var2 = 3.0 * var1 - 1.0;
            sat_angbeta = 1.0 - Math.Pow(sat_eccentricity, 2);
            sat_sqrt_angbeta = Math.Sqrt(sat_angbeta);
            sat_SemiMajor = orbit.SemiMajor;
            sat_MeanMotion = orbit.MeanMotion;

            double var5 = CalculateV5();
            var6 = 1.0 / (sat_SemiMajor - var4);
            var7 = sat_SemiMajor * sat_eccentricity * var6;
            var8 = Math.Pow(var7, 2);
            var9 = sat_eccentricity * var7;
            var10 = var5 * Math.Pow(var6, 4);
            var11 = var10 / Math.Pow(Math.Abs(1.0 - var8), 3.5);
            var12 = orbit.BStar * var11 * sat_MeanMotion * (sat_SemiMajor * (1.0 + 1.5 * var8 + var9 * (4.0 + var8)) +
                                                            0.75 * (1.0826158E-3 / 2.0) * var6 / Math.Abs(1.0 - var8) *
                                                            var2 * (8.0 + 3.0 * var8 * (8.0 + var8)));
            var13 = Math.Sin(sat_inclination);
            double var14 = -(-2.53881E-6) / (1.0826158E-3 / 2.0) * Math.Pow(1.0, 3);
            var15 = var10 * var6 * var14 * sat_MeanMotion * 1.0 * var13 / sat_eccentricity;
            var16 = 1.0 - var1;
            var17 = 2.0 * sat_MeanMotion * var11 * sat_SemiMajor * sat_angbeta * (var7 * (2.0 + var8 / 2.0) +
                sat_eccentricity * (1.0 / 2.0 + var8 / 2.0) - 2.0 * (1.0826158E-3 / 2.0) * var6 /
                (sat_SemiMajor * Math.Abs(1.0 - var8)) *
                (-3.0 * var2 * (1.0 - 2.0 * var9 + var8 * (3.0 / 2.0 - 1.0 / 2.0 * var9)) + 0.75 * var16 *
                    (2.0 * var8 - var9 * (1.0 + var8)) * Math.Cos(2.0 * orbit.ArgPerigee)));
            double angle = Math.Pow(var1, 2);
            double pq = 1.0 / (sat_SemiMajor * sat_SemiMajor * sat_angbeta * sat_angbeta);
            double t1 = 3.0 * (1.0826158E-3 / 2.0) * pq * sat_MeanMotion;
            double t2 = t1 * (1.0826158E-3 / 2.0) * pq;
            double t3 = 1.25f * (-3.0 * -1.65597E-6 / 8.0) * Math.Pow(pq, 2) * sat_MeanMotion;
            var18 = sat_MeanMotion + 0.5 * t1 * sat_sqrt_angbeta * var2 +
                    0.0625 * t2 * sat_sqrt_angbeta * (13.0 - 78.0 * var1 + 137.0 * angle);
            var19 = -0.5 * t1 * (1.0 - 5.0 * var1) + 0.0625 * t2 * (7.0 - 114.0 * var1 + 395.0 * angle) +
                    t3 * (3.0 - 36.0 * var1 + 49.0 * angle);
            var20 = -t1 * cos_incl + (0.5 * t2 * (4.0 - 19.0 * var1) + 2.0 * t3 * (3.0 - 7.0 * var1)) * cos_incl;
            var21 = 3.5 * sat_angbeta * -t1 * cos_incl * var12;
            var22 = 1.5 * var12;
            var23 = 0.125 * var14 * var13 * (3.0 + 5.0 * cos_incl) / (1.0 + cos_incl);
            var24 = 0.25 * var14 * var13;
            var25 = 7.0 * var1 - 1.0;
        }

        /// <summary>
        /// Help calculation var5
        /// </summary>
        /// <returns></returns>
        private double CalculateV5()
        {
            double sat_per = EarthScript.Radius * (sat_SemiMajor * (1.0 - sat_eccentricity) - 1.0);
            var4 = 1.0 + 78.0 / EarthScript.Radius;
            double var5 = Math.Pow(((1.0 + 120.0 / EarthScript.Radius) - (var4)), 4);
            if (sat_per < 156.0)
            {
                var4 = sat_per - 78.0;

                if (sat_per <= 98.0)
                {
                    var4 = 20.0;
                }

                var5 = Math.Pow((120.0 - var4) * 1.0 / EarthScript.Radius, 4);
                var4 = var4 / EarthScript.Radius + 1.0;
            }

            return var5;
        }

        /// <summary>
        /// Find satellite vector
        /// </summary>
        /// <param name="orbit">Satellites orbit</param>
        /// <param name="inc"></param>
        /// <param name="angle"></param>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="d3"></param>
        /// <param name="d4"></param>
        /// <param name="d5"></param>
        /// <param name="deltatime"></param>
        /// <returns></returns>
        public SatVector SolutionVector(Orbit orbit, double inc, double angle, double d1, double d2, double d3,
            double d4,
            double d5, double deltatime)
        {
            double v1 = d1 * Math.Cos(angle);
            double v2 = 1.0 / (d2 * Math.Sqrt(1.0 - Math.Pow(d1, 2) * Math.Sqrt(1.0 - Math.Pow(d1, 2))));
            double v3 = v2 * var23 * v1;
            double v4 = v2 * var24;
            double v5 = d3 + v3;
            double v6 = d1 * Math.Sin(angle) + v4;

            double c1 = Modul(v5 - d4);
            double c2 = c1;
            double c3 = 0.0;
            double c4 = 0.0;
            double c5 = 0.0;
            double c6 = 0.0;
            double c7 = 0.0;
            double c8 = 0.0;
            bool End = false;

            for (int i = 1; (i <= 10) && !End; i++)
            {
                c7 = Math.Sin(c2);
                c8 = Math.Cos(c2);
                c3 = v1 * c7;
                c4 = v6 * c8;
                c5 = v1 * c8;
                c6 = v6 * c7;

                if (Math.Abs(((c1 - c4 + c3 - c2) / (1.0 - c5 - c6)) + c2 - c2) <= 1.0e-06)
                {
                    End = true;
                }
                else
                {
                    c2 = (c1 - c4 + c3 - c2) / (1.0 - c5 - c6) + c2;
                }
            }

            double c9 = c5 + c6;
            double c10 = c3 - c4;
            v2 = 1.0 - (Math.Pow(v1, 2) + Math.Pow(v6, 2));
            double c12 = d2 * v2;
            double c13 = 1.0 / (d2 * (1.0 - c9));
            double c14 = Math.Sqrt(3600.0 * 398600.8 / Math.Pow(EarthScript.Radius, 3)) * Math.Sqrt(d2) * c10 * c13;
            double c15 = Math.Sqrt(3600.0 * 398600.8 / Math.Pow(EarthScript.Radius, 3)) * Math.Sqrt(c12) * c13;
            c2 = d2 * c13;
            c3 = 1.0 / (1.0 + Math.Sqrt(v2));
            double c16 = c2 * (c8 - v1 + v6 * c10 * c3);
            double c17 = c2 * (c7 - v6 - v1 * c10 * c3);
            double c18 = 2.0 * c17 * c16;
            double c19 = 2.0 * c16 * c16 - 1;

            v2 = 1.0 / c12;
            c13 = (1.0826158E-3 / 2.0) * v2;
            c2 = c13 * v2;

            double a1 = d2 * (1.0 - c9) * (1.0 - 1.5 * c2 * Math.Sqrt(v2) * var2) + 0.5 * c13 * var16 * c19;
            double a2 = Math.Atan2(c17, c16) - 0.25 * c2 * var25 * c18;
            double b1 = d4 + 1.5 * c2 * cos_incl * c18;
            double b2 = inc + 1.5 * c2 * cos_incl * var13 * c19;
            double b4 = c14 - d5 * c13 * var16 * c18;
            double b5 = c15 + d5 * c13 * (var16 * c19 + 1.5 * var2);

            double m1 = -Math.Sin(b1) * Math.Cos(b2) * Math.Sin(a2) + Math.Cos(b1) * Math.Cos(a2);
            double m2 = Math.Cos(b1) * Math.Cos(b2) * Math.Sin(a2) + Math.Sin(b1) * Math.Cos(a2);
            double m3 = Math.Sin(b2) * Math.Sin(a2);
            double m4 = -Math.Sin(b1) * Math.Cos(b2) * Math.Cos(a2) - Math.Cos(b1) * Math.Sin(a2);
            double m5 = Math.Cos(b1) * Math.Cos(b2) * Math.Cos(a2) - Math.Sin(b1) * Math.Sin(a2);
            double m6 = Math.Sin(b2) * Math.Cos(a2);

            double posx = a1 * m1;
            double posy = a1 * m2;
            double posz = a1 * m3;

            Vector3 vecPos = new Vector3((float)posx, (float)posy, (float)posz);
            JTime gmt = CreateInstance<JTime>();

            gmt.Date = orbit.JT_TLE.Date;
            gmt.Day = orbit.JT_TLE.Day;
            gmt.Year = orbit.JT_TLE.Year;
            gmt.AddMin(deltatime);

            double velx = b4 * m1 + b5 * m4;
            double vely = b4 * m2 + b5 * m5;
            double velz = b4 * m3 + b5 * m6;

            Vector3 vecVel = new Vector3((float)velx, (float)vely, (float)velz);
            SatVector result = CreateInstance<SatVector>();
            result.CreateResult(vecPos, vecVel);
            return result;
        }

        /// <summary>
        /// Absolute value in radians
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected static double Modul(double value)
        {
            double modul = value % (Mathf.PI * 2);
            if (modul < 0.0)
            {
                modul += 2.0 * Mathf.PI;
            }

            return modul;
        }
    }
}