using System;
using SatelliteInspector.Scripts.PrefabScripts;

namespace SatelliteInspector.Scripts.SolutionScripts
{
    /// <summary>
    /// Class solution 
    /// </summary>
    internal class SolutionG : ClassSolution
    {
        private double sg1;
        private double sg2;
        private double sg3;
        private double sg5;
        private double sg6;

        /// <summary>
        /// Run solution
        /// </summary>
        /// <param name="orbit">Satellite orbit</param>
        public new void RunSolution(Orbit orbit)
        {
            sg1 = 2.0 * var11 * sat_SemiMajor * sat_angbeta * (1.0 + 2.75 * (var8 + var9) + var9 * var8);
            sg2 = orbit.BStar * var15 * Math.Cos(orbit.ArgPerigee);
            sg3 = -(2.0 / 3.0) * var10 * orbit.BStar * 1.0 / var9;
            sg5 = Math.Pow(1.0 + var7 * Math.Cos(orbit.MeanAnomaly), 3);
            sg6 = Math.Sin(orbit.MeanAnomaly);
        }

        /// <summary>
        /// New satellite vector
        /// </summary>
        /// <param name="orbit">orbit</param>
        /// <param name="deltaTime">delta time</param>
        /// <returns></returns>
        public override SatVector SolutionVector(Orbit orbit, double deltaTime)
        {
            bool is_sg = (sat_SemiMajor * (1.0 - sat_eccentricity) / 1.0) < ((220.0 / EarthScript.Radius) + 1.0);

            double v1 = 0.0;
            double v2 = 0.0;
            double v3 = 0.0;

            double v4 = 0.0;
            double v5 = 0.0;
            double v6 = 0.0;

            if (!is_sg)
            {
                v1 = 4.0 * sat_SemiMajor * var6 * Math.Pow(var12, 2);
                v2 = (17.0 * sat_SemiMajor + var4) * (v1 * var6 * var12 / 3.0);
                v3 = 0.5 * (v1 * var6 * var12 / 3.0) * sat_SemiMajor * var6 * (221 * sat_SemiMajor + 31.0 * var4) * var12;
                v4 = v1 + (2.0 * Math.Pow(var12, 2));
                v5 = 0.25 * (3.0 * v2 + var12 * (12.0 * v1 + 10.0 * Math.Pow(var12, 2)));
                v6 = 0.2 * (3.0 * v3 + 12.0 * var12 * v2 + 6.0 * Math.Pow(v1, 2) + 15.0 * Math.Pow(var12, 2) * (2.0 * v1 + Math.Pow(var12, 2)));
            }

            double v7 = orbit.MeanAnomaly + (var18 * deltaTime);
            double v8 = orbit.ArgPerigee + (var19 * deltaTime);
            double v9 = orbit.RAAN + (var20 * deltaTime);
            double v10 = v8;
            double v11 = v7;
            double v12 = v9 + (var21 * Math.Pow(deltaTime, 2));
            double v13 = 1 - (var12 * deltaTime);
            double v14 = orbit.BStar * var17 * deltaTime;
            double v16 = var22 * Math.Pow(deltaTime, 2);

            if (!is_sg)
            {
                v11 = v7 + sg2 * deltaTime + sg3 * (Math.Pow(1 + var7 * Math.Cos(v7), 3) - sg5);
                v10 = v8 - (sg2 * deltaTime + sg3 * (Math.Pow(1 + var7 * Math.Cos(v7), 3) - sg5));
                v13 = v13 - (v1 * Math.Pow(deltaTime, 2)) - (v2 * Math.Pow(deltaTime, 2) * deltaTime) - (v3 * deltaTime * Math.Pow(deltaTime, 2) * deltaTime);
                v14 = v14 + orbit.BStar * sg1 * (Math.Sin(v11) - sg6);
                v16 = v16 + v4 * Math.Pow(deltaTime, 2) * deltaTime + deltaTime * Math.Pow(deltaTime, 2) * deltaTime * (v5 + deltaTime * v6);
            }

            return SolutionVector(orbit, sat_inclination, v8, sat_eccentricity - v14, sat_SemiMajor * Math.Pow(v13, 2), v11 + v10 + v12 + sat_MeanMotion * v16, v12, Math.Sqrt(3600.0 * 398600.8 / Math.Pow(EarthScript.Radius, 3)) / Math.Pow(sat_SemiMajor * Math.Pow(v13, 2), 1.5), deltaTime);
        }


    }
}
