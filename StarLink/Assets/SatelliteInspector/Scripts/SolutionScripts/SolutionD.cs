using System;

namespace SatelliteInspector.Scripts.SolutionScripts
{
    /// <summary>
    /// ClassSolution
    /// </summary>
    internal class SolutionD : ClassSolution
    {
        private const double const1 = 1.19459E-5;
        private const double const2 = 0.01675;
        private const double const3 = 1.5835218E-4;
        private const double const4 = 0.05490;
        private const double const5 = 4.3752691E-3;

        private double v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14;
        private double v15, v16, v17, v18, v19, v20, v21, v22, v23, v24, j_incl, mod, v27;

        private double j_time, j2201, j2211, j3210, j3222, j4410, j4422, j5220, j5232, j5421, j5433, jd1;
        private double jd2, jd3, j_argper, jd4, jd5, jd6, jd7, jd8, jd9, jd10, jd11, j_gmsTime, jd13, jd14, jd15, jd16;

        private bool isRES;
        private bool isSYN;

        /// <summary>
        /// Run new solution
        /// </summary>
        /// <param name="orbit"></param>
        public new void RunSolution(Orbit orbit)
        {
            j_gmsTime = orbit.JT_TLE.ToGmsTime();
            j_incl = orbit.Inclination;
            j_argper = orbit.ArgPerigee;

            mod = Modul(4.7199672 + 0.22997150 * orbit.JT_TLE.J1900 - (5.8351514 + 0.0019443680 * orbit.JT_TLE.J1900));
            v27 = Modul(6.2565837 + 0.017201977 * orbit.JT_TLE.J1900);

            #region local vars
            double a1, a3, a7, a8, a9, a10;
            double a2, a4, a5, a6;
            double x1, x2, x3, x4, x5, x6, x7, x8;
            double z31, z32, z33;
            double z1, z2, z3;
            double z11, z12, z13;
            double z21, z22, z23;
            double s3, s2, s4, s1, s5, s6, s7;

            double js1 = 0.0;
            double js2 = 0.0;
            double js3 = 0.0;
            double js4 = 0.0;
            double js5 = 0.0;
            #endregion

            for (int n = 1; n <= 2; n++)
            {
                double cosRaan = Math.Cos(orbit.RAAN);
                double sinRaan = Math.Sin(orbit.RAAN);
                double coef1, coef2, coef3, coef4;
                SetCoefs(out coef1, out coef2, out coef3, out coef4);
                a1 = coef1 * cosRaan - coef2 * coef3 * sinRaan;
                a3 = coef2 * cosRaan + coef1 * coef3 * sinRaan;
                a7 = -coef1 * sinRaan - coef2 * coef3 * cosRaan;
                a8 = -coef2 * coef4;
                a9 = -coef2 * sinRaan + coef1 * coef3 * cosRaan;
                a10 = coef1 * coef4;
                a2 = cos_incl * a7 + var13 * a8;
                a4 = cos_incl * a9 + var13 * a10;
                a5 = -var13 * a7 + cos_incl * a8;
                a6 = -var13 * a9 + cos_incl * a10;
                x1 = a1 * Math.Cos(orbit.ArgPerigee) + a2 * Math.Sin(orbit.ArgPerigee);
                x2 = a3 * Math.Cos(orbit.ArgPerigee) + a4 * Math.Sin(orbit.ArgPerigee);
                x3 = -a1 * Math.Sin(orbit.ArgPerigee) + a2 * Math.Cos(orbit.ArgPerigee);
                x4 = -a3 * Math.Sin(orbit.ArgPerigee) + a4 * Math.Cos(orbit.ArgPerigee);
                x5 = a5 * Math.Sin(orbit.ArgPerigee);
                x6 = a6 * Math.Sin(orbit.ArgPerigee);
                x7 = a5 * Math.Cos(orbit.ArgPerigee);
                x8 = a6 * Math.Cos(orbit.ArgPerigee);
                z31 = 12.0 * x1 * x1 - 3.0 * x3 * x3;
                z32 = 24.0 * x1 * x2 - 6.0 * x3 * x4;
                z33 = 12.0 * x2 * x2 - 3.0 * x4 * x4;
                z1 = 3.0 * (a1 * a1 + a2 * a2) + z31 * Math.Pow(orbit.Eccentricity, 2);
                z2 = 6.0 * (a1 * a3 + a2 * a4) + z32 * Math.Pow(orbit.Eccentricity, 2);
                z3 = 3.0 * (a3 * a3 + a4 * a4) + z33 * Math.Pow(orbit.Eccentricity, 2);
                z11 = -6.0 * a1 * a5 + Math.Pow(orbit.Eccentricity, 2) * (-24.0 * x1 * x7 - 6.0 * x3 * x5);
                z12 = -6.0 * (a1 * a6 + a3 * a5) + Math.Pow(orbit.Eccentricity, 2) * (-24.0 * (x2 * x7 + x1 * x8) - 6.0 * (x3 * x6 + x4 * x5));
                z13 = -6.0 * a3 * a6 + Math.Pow(orbit.Eccentricity, 2) * (-24.0 * x2 * x8 - 6.0 * x4 * x6);
                z21 = 6.0 * a2 * a5 + Math.Pow(orbit.Eccentricity, 2) * (24.0 * x1 * x5 - 6.0 * x3 * x7);
                z22 = 6.0 * (a4 * a5 + a2 * a6) + Math.Pow(orbit.Eccentricity, 2) * (24.0 * (x2 * x5 + x1 * x6) - 6.0 * (x4 * x7 + x3 * x8));
                z23 = 6.0 * a4 * a6 + Math.Pow(orbit.Eccentricity, 2) * (24.0 * x2 * x6 - 6.0 * x4 * x8);
                z1 = z1 + z1 + sat_angbeta * z31;
                z2 = z2 + z2 + sat_angbeta * z32;
                z3 = z3 + z3 + sat_angbeta * z33;
                s3 = 2.9864797e-6 * (1.0 / orbit.MeanMotion);
                s2 = -1.0 / 2.0 * s3 / sat_sqrt_angbeta;
                s4 = s3 * sat_sqrt_angbeta;
                s1 = -15.0 * orbit.Eccentricity * s4;
                s5 = x1 * x3 + x2 * x4;
                s6 = x2 * x3 + x1 * x4;
                s7 = x2 * x4 - x1 * x3;
                js1 = s1 * const1 * s5;
                js2 = s2 * const1 * (z11 + z13);
                js3 = -const1 * s3 * (z1 + z3 - 14.0 - 6.0 * Math.Pow(orbit.Eccentricity, 2));
                js4 = s4 * const1 * (z31 + z33 - 6.0);

                js5 = j_incl < 5.2359877E-2 ? 0.0 : -const1 * s2 * (z21 + z23);

                v2 = 2.0 * s1 * s6;
                v1 = 2.0 * s1 * s7;
                v20 = 2.0 * s2 * z12;
                v21 = 2.0 * s2 * (z13 - z11);
                v22 = -2.0 * s3 * z2;
                v23 = -2.0 * s3 * (z3 - z1);
                v24 = -2.0 * s3 * (-21.0 - 9.0 * Math.Pow(orbit.Eccentricity, 2)) * const2;
                v15 = 2.0 * s4 * z32;
                v16 = 2.0 * s4 * (z33 - z31);
                v17 = -18.0 * s4 * const2;
                v18 = -2.0 * s2 * z22;
                v19 = -2.0 * s2 * (z23 - z21);

                if (n == 1)
                {
                    jd4 = js1;
                    jd7 = js2;
                    jd8 = js3;
                    jd6 = js5 / var13;
                    jd5 = js4 - cos_incl * jd6;
                    v3 = v2;
                    v10 = v20;
                    v12 = v22;
                    v5 = v15;
                    v8 = v18;
                    v4 = v1;
                    v11 = v21;
                    v13 = v23;
                    v6 = v16;
                    v9 = v19;
                    v14 = v24;
                    v7 = v17;

                    cosRaan = NewRaan(orbit);
                }
            }

            jd4 += js1;
            jd7 += js2;
            jd8 += js3;
            jd5 += js4 - cos_incl / var13 * js5;
            jd6 += js5 / var13;

            isRES = false;
            isSYN = false;

            double matrix31;
            double matrix22;
            double sum = 0.0;

            bool orbitMotion = (orbit.MeanMotion > 0.0034906585) && (orbit.MeanMotion < 0.0052359877);
        
            const double d = 3.7393792E-7;
            const double d1 = 2.5;
            const double d2 = 0.8125;
            const double d3 = 6.60937;
            const double d4 = 0.75;
            const double d5 = 0.9375;
        
        
            if (orbitMotion)
            {
                isRES = true;
                isSYN = true;

                double lm22 = 1.0 + Math.Pow(orbit.Eccentricity, 2) * (-d1 + d2 * Math.Pow(orbit.Eccentricity, 2));
                matrix31 = 1.0 + 2.0 * Math.Pow(orbit.Eccentricity, 2);
                double lm30 = 1 + Math.Pow(orbit.Eccentricity, 2) * (-6.0 + d3 * Math.Pow(orbit.Eccentricity, 2));
                matrix22 = d4 * (1.0 + cos_incl) * (1.0 + cos_incl);
                double lm311 = d5 * var13 * var13 * (1.0 + 3.0 * cos_incl) - d4 * (1 + cos_incl);
                double lm33 = 1.0 + cos_incl;
                lm33 = 1.875 * Math.Pow(lm33, 3);

                jd1 = 3.0 * Math.Pow(sat_MeanMotion, 2) * Math.Pow((1.0 / orbit.SemiMajor), 2);
                jd2 = 2.0 * jd1 * matrix22 * lm22 * 1.7891679e-06;
                jd3 = 3.0 * jd1 * lm33 * lm30 * 2.2123015e-07 * (1.0 / orbit.SemiMajor);

                jd1 = jd1 * lm311 * matrix31 * 2.1460748e-06 * (1.0 / orbit.SemiMajor);
                jd14 = orbit.MeanAnomaly + orbit.RAAN + orbit.ArgPerigee - j_gmsTime;
                sum = var18 + var19 + var20 - const5 + jd8 + jd5 + jd6;
            }
            else
            {
                bool OrbitMotion2 = (orbit.MeanMotion >= 8.26E-3) && (orbit.MeanMotion <= 9.24E-3) && (orbit.Eccentricity >= 0.5);
                if (OrbitMotion2)
                {
                    isRES = true;

                    double eccen3 = Math.Pow(orbit.Eccentricity, 3);

                    double gmat211;
                    double gmat322;
                    double gmat410;
                    double gmat422;
                    double gmat520;

                    if (orbit.Eccentricity <= 0.65)
                    {
                        gmat211 = 3.616 - 13.247 * orbit.Eccentricity + 16.290 * Math.Pow(orbit.Eccentricity, 2);
                        matrix31 = -19.302 + 117.390 * orbit.Eccentricity - 228.419 * Math.Pow(orbit.Eccentricity, 2) + 156.591 * eccen3;
                        gmat322 = -18.9068 + 109.7927 * orbit.Eccentricity - 214.6334 * Math.Pow(orbit.Eccentricity, 2) + 146.5816 * eccen3;
                        gmat410 = -41.122 + 242.694 * orbit.Eccentricity - 471.094 * Math.Pow(orbit.Eccentricity, 2) + 313.953 * eccen3;
                        gmat422 = -146.407 + 841.880 * orbit.Eccentricity - 1629.014 * Math.Pow(orbit.Eccentricity, 2) + 1083.435 * eccen3;
                        gmat520 = -532.114 + 3017.977 * orbit.Eccentricity - 5740.0 * Math.Pow(orbit.Eccentricity, 2) + 3708.276 * eccen3;
                    }
                    else
                    {
                        gmat211 = -72.099 + 331.819 * orbit.Eccentricity - 508.738 * Math.Pow(orbit.Eccentricity, 2) + 266.724 * eccen3;
                        matrix31 = -346.844 + 1582.851 * orbit.Eccentricity - 2415.925 * Math.Pow(orbit.Eccentricity, 2) + 1246.113 * eccen3;
                        gmat322 = -342.585 + 1554.908 * orbit.Eccentricity - 2366.899 * Math.Pow(orbit.Eccentricity, 2) + 1215.972 * eccen3;
                        gmat410 = -1052.797f + 4758.686 * orbit.Eccentricity - 7193.992 * Math.Pow(orbit.Eccentricity, 2) + 3651.957 * eccen3;
                        gmat422 = -3581.69f + 16178.11 * orbit.Eccentricity - 24462.77 * Math.Pow(orbit.Eccentricity, 2) + 12422.52 * eccen3;

                        if (orbit.Eccentricity <= 0.715)
                        {
                            gmat520 = 1464.74f - 4664.75 * orbit.Eccentricity + 3763.64 * Math.Pow(orbit.Eccentricity, 2);
                        }
                        else
                        {
                            gmat520 = -5149.66f + 29936.92 * orbit.Eccentricity - 54087.36 * Math.Pow(orbit.Eccentricity, 2) + 31324.56 * eccen3;
                        }
                    }

                    double gmat533;
                    double gmat521;
                    double gmat532;

                    if (orbit.Eccentricity < 0.7)
                    {
                        gmat533 = -919.2277 + 4988.61 * orbit.Eccentricity - 9064.77 * Math.Pow(orbit.Eccentricity, 2) + 5542.21 * eccen3;
                        gmat521 = -822.71072 + 4568.6173 * orbit.Eccentricity - 8491.4146 * Math.Pow(orbit.Eccentricity, 2) + 5337.524 * eccen3;
                        gmat532 = -853.666f + 4690.25 * orbit.Eccentricity - 8624.77 * Math.Pow(orbit.Eccentricity, 2) + 5341.4 * eccen3;
                    }
                    else
                    {
                        gmat533 = -37995.78 + 161616.52 * orbit.Eccentricity - 229838.2 * Math.Pow(orbit.Eccentricity, 2) + 109377.94 * eccen3;
                        gmat521 = -51752.104 + 218913.95 * orbit.Eccentricity - 309468.16 * Math.Pow(orbit.Eccentricity, 2) + 146349.42 * eccen3;
                        gmat532 = -40023.88 + 170470.89 * orbit.Eccentricity - 242699.48 * Math.Pow(orbit.Eccentricity, 2) + 115605.82 * eccen3;
                    }

                    double var13s2 = Math.Pow(var13, 2);
                    double cos_incls2 = Math.Pow(cos_incl, 2);
                    matrix22 = d4 * (1.0 + 2.0 * cos_incl + cos_incls2);

                    double func221 = 1.5 * var13s2;
                    double func321 = 1.875 * var13 * (1.0 - 2.0 * cos_incl - 3.0 * cos_incls2);
                    double func322 = -1.875 * var13 * (1.0 + 2.0 * cos_incl - 3.0 * cos_incls2);
                    double func441 = 35.0 * var13s2 * matrix22;
                    double func442 = 39.3750 * var13s2 * var13s2;
                    double func522 = 9.84375 * var13 * (var13s2 * (1.0 - 2.0 * cos_incl - 5.0 * cos_incls2) + 0.33333333 * (-2.0 + 4.0 * cos_incl + 6.0 * cos_incls2));
                    double func523 = var13 * (4.92187512 * var13s2 * (-2.0 - 4.0 * cos_incl + 10.0 * cos_incls2) + 6.56250012 * (1.0 + 2.0 * cos_incl - 3.0 * cos_incls2));
                    double func542 = 29.53125 * var13 * (2.0 - 8.0 * cos_incl + cos_incls2 * (-12.0 + 8.0 * cos_incl + 10.0 * cos_incls2));
                    double func543 = 29.53125 * var13 * (-2.0 - 8.0 * cos_incl + cos_incls2 * (12.0 + 8.0 * cos_incl - 10.0 * cos_incls2));
                    double varres1 = 3 * Math.Pow(sat_MeanMotion, 2) * 1.0 / orbit.SemiMajor * (1.0 / orbit.SemiMajor);

                    double varres2 = varres1 * 1.7891679E-6;

                    j2201 = varres2 * matrix22 * (-0.306 - (orbit.Eccentricity - 0.64) * 0.44);
                    j2211 = varres2 * func221 * gmat211;
                    varres1 *= (1.0 / orbit.SemiMajor);
                    varres2 = varres1 * d;
                    j3210 = varres2 * func321 * matrix31;
                    j3222 = varres2 * func322 * gmat322;
                    varres1 *= (1.0 / orbit.SemiMajor);
                    varres2 = 2.0 * varres1 * 7.3636953E-9;
                    j4410 = varres2 * func441 * gmat410;
                    j4422 = varres2 * func442 * gmat422;
                    varres1 *= (1.0 / orbit.SemiMajor);
                    varres2 = varres1 * 1.1428639E-7;
                    j5220 = varres2 * func522 * gmat520;
                    j5232 = varres2 * func523 * gmat532;
                    varres2 = 2.0 * varres1 * 2.1765803E-9;
                    j5421 = varres2 * func542 * gmat521;
                    j5433 = varres2 * func543 * gmat533;
                    jd14 = orbit.MeanAnomaly + 2.0 * orbit.RAAN - 2.0 * j_gmsTime;
                    sum = var18 + var20 + var20 - const5 - const5;
                    sum += (jd8 + jd6 + jd6);
                }
            }

            bool Res = isRES || isSYN;
            if (Res)
            {
                jd13 = sum - sat_MeanMotion;
                jd15 = jd14;
                jd16 = sat_MeanMotion;
                jd11 = 720.0;
                jd10 = -720.0;
                jd9 = 259200.0;
            }
        }

        /// <summary>
        /// Set coefficients
        /// </summary>
        /// <param name="coef1"></param>
        /// <param name="coef2"></param>
        /// <param name="coef3"></param>
        /// <param name="coef4"></param>
        private static void SetCoefs(out double coef1, out double coef2, out double coef3, out double coef4)
        {
            coef1 = 0.19459050;
            coef2 = 0.98088458;
            coef3 = 0.91744867;
            coef4 = 0.39785416;
        }

        /// <summary>
        /// Calculate orbit RAAN
        /// </summary>
        /// <param name="orbit"></param>
        /// <returns></returns>
        private static double NewRaan(Orbit orbit)
        {
            return Math.Sqrt(1.0 - 0.089683511 * Math.Sin(4.5236020 - 9.2422029E-4 * orbit.JT_TLE.J1900) / Math.Sqrt(1.0 - (0.91375164 - 0.03568096 *
                           Math.Cos(4.5236020 - 9.2422029E-4 * orbit.JT_TLE.J1900)) * (0.91375164 - 0.03568096 * Math.Cos(4.5236020f - 9.2422029E-4 * orbit.JT_TLE.J1900))) *
                       (0.089683511 * Math.Sin(4.5236020 - 9.2422029E-4 * orbit.JT_TLE.J1900) / Math.Sqrt(1.0 - (0.91375164 - 0.03568096 * Math.Cos(4.5236020 - 9.2422029E-4 *
                           orbit.JT_TLE.J1900)) * (0.91375164 - 0.03568096 * Math.Cos(4.5236020 - 9.2422029E-4 * orbit.JT_TLE.J1900))))) * Math.Cos(orbit.RAAN)
                   + 0.089683511 * Math.Sin(4.5236020 - 9.2422029E-4 * orbit.JT_TLE.J1900) / Math.Sqrt(1.0 - (0.91375164 - 0.03568096 * Math.Cos(4.5236020 - 9.2422029E-4 * orbit.JT_TLE.J1900)) *
                       (0.91375164 - 0.03568096 * Math.Cos(4.5236020 - 9.2422029E-4 * orbit.JT_TLE.J1900))) * Math.Sin(orbit.RAAN);
        }

        /// <summary>
        /// New vector
        /// </summary>
        /// <param name="orbit">Satellite orbit</param>
        /// <param name="deltaTime">delta time</param>
        /// <returns></returns>
        public override SatVector SolutionVector(Orbit orbit, double deltaTime)
        {
            double par1 = orbit.MeanAnomaly + var18 * deltaTime;
            double par2 = orbit.ArgPerigee + var19 * deltaTime;
            double par3 = orbit.RAAN + var20 * deltaTime + var21 * Math.Pow(deltaTime, 2);
            double par6 = sat_MeanMotion;
            double par4 = 0.0;
            double par5 = 0.0;

            IsSec(orbit, ref par1, ref par2, ref par3, ref par4, ref par5, ref par6, ref deltaTime);

            double var1 = Math.Pow(Math.Sqrt(3600.0 * 398600.8 / Math.Pow(6378.135, 3)) / par6, 2.0 / 3.0) * Math.Pow(1.0 - var12 * deltaTime, 2);
            double parE = par4 - orbit.BStar * var17 * deltaTime;
            double par7 = par1 + sat_MeanMotion * var22 * Math.Pow(deltaTime, 2);

            IsPer(ref parE, ref par5, ref par2, ref par3, ref par7, deltaTime);

            double par8 = par7 + par2 + par3;

            par6 = Math.Sqrt(3600.0 * 398600.8 / Math.Pow(6378.135, 3)) / Math.Pow(var1, 1.5);
            return SolutionVector(orbit, par5, par2, parE, var1, par8, par3, par6, deltaTime);
        }

        private void IsSec(Orbit orbit, ref double par1, ref double par2, ref double par3, ref double par4,
            ref double par5, ref double par6, ref double deltatime)
        {
            par1 += jd8 * deltatime;
            par2 += jd5 * deltatime;
            par3 += jd6 * deltatime;
            par4 = orbit.Eccentricity + jd4 * deltatime;
            par5 = orbit.Inclination + jd7 * deltatime;

            if (par5 < 0.0)
            {
                par5 = -par5;
                par3 += Math.PI;
                par2 -= Math.PI;
            }

            double v1 = 0.0;
            double v2 = 0.0;
            double v3 = 0.0;
            double v4 = 0.0;
            double v5 = 0.0;

            bool is_Ended = false;

            if (isRES)
            {
                while (!is_Ended)
                {
                    if ((j_time == 0.0) || ((deltatime >= 0.0) && (j_time < 0.0)) || ((deltatime < 0.0) && (j_time >= 0.0)))
                    {
                        v5 = (deltatime < 0.0) ? jd10 : jd11;
                        j_time = 0.0;
                        jd16 = sat_MeanMotion;
                        jd15 = jd14;
                        is_Ended = true;
                    }
                    else
                    {
                        if (Math.Abs(deltatime) < Math.Abs(j_time))
                        {
                            v5 = jd11;
                            if (deltatime >= 0.0)
                            {
                                v5 = jd10;
                            }
                            Calc(ref v2, ref v1, ref v3, v5);
                        }
                        else
                        {
                            v5 = jd10;
                            if (deltatime > 0.0f)
                            {
                                v5 = jd11;
                            }
                            is_Ended = true;
                        }
                    }
                }

                while (Math.Abs(deltatime - j_time) >= jd11)
                {
                    Calc(ref v2, ref v1, ref v3, v5);
                }
                v4 = deltatime - j_time;
                CalcPoints(ref v2, ref v1, ref v3);

                par6 = jd16 + (v2 * v4) + (v1 * v4 * v4 * 0.5);
                par1 = jd15 + (v3 * v4) + (v2 * v4 * v4 * 0.5) - par2 - par3 + j_gmsTime + (deltatime * const5);

                if (!isSYN)
                {
                    par1 = jd15 + v3 * v4 + v2 * v4 * v4 * 0.5 + 2.0 * (-1.0 * par3 + j_gmsTime + deltatime * const5);
                }
            }
        }

        private void IsPer(ref double varE, ref double angleX, ref double angleO, ref double nX, ref double xX,
            double deltaTime)
        {
            double sinX = Math.Sin(angleX);
            double cosX = Math.Cos(angleX);

            double v2;
            double v3;
            double v4;
            double v5;
            double v6;
            double v7;

            double angle_m = v27 + const1 * deltaTime;
            double angle_f = angle_m + 2.0 * const2 * Math.Sin(angle_m);
            double sin_f = Math.Sin(angle_f);
            double func2 = 0.5 * Math.Pow(sin_f, 2) - 0.25;
            double func3 = -0.5 * sin_f * Math.Cos(angle_f);

            double v1 = this.v5 * func2 + this.v6 * func3 + this.v7 * sin_f;
            v2 = v8 * func2 + v9 * func3;
            angle_m = mod + const3 * deltaTime;
            angle_f = angle_m + 2.0 * const4 * Math.Sin(angle_m);
            sin_f = Math.Sin(angle_f);
            func2 = 0.5 * Math.Pow(sin_f, 2) - 0.25;
            func3 = -0.5 * sin_f * Math.Cos(angle_f);

            v7 = v15 * func2 + v16 * func3 + v17 * sin_f;
            v3 = v18 * func2 + v19 * func3;
            v4 = this.v3 * func2 + this.v4 * func3 + this.v2 * func2 + this.v1 * func3;
            v5 = v10 * func2 + v11 * func3 + v20 * func2 + v21 * func3;
            v6 = v12 * func2 + v13 * func3 + v14 * sin_f + v22 * func2 + v23 * func3 + v24 * sin_f;

            double sum_gh = v1 + v7;
            double sum_ph = v2 + v3;

            angleX += v5;
            varE += v4;

            if (j_incl >= 0.2)
            {
                sum_ph /= var13;
                sum_gh -= cos_incl * sum_ph;
                angleO += sum_gh;
                nX += sum_ph;
                xX += v6;
            }
            else
            {
                double sinnX = Math.Sin(nX);
                double cosnX = Math.Cos(nX);
                double alfa = sinX * sinnX;
                double beta = sinX * cosnX;

                alfa += sum_ph * cosnX + v5 * cosX * sinnX;
                beta += -sum_ph * sinnX + v5 * cosX * cosnX;

                double xxx = xX + angleO + cosX * nX;
                xxx += v6 + sum_gh - v5 * nX * sinX;

                nX = Math.Atan2(alfa, beta);
                xX += v6;
                angleO = xxx - xX - Math.Cos(angleX) * nX;
            }
        }

        private void Calc(ref double point, ref double pointdt, ref double lpoint, double dp)
        {
            CalcPoints(ref point, ref pointdt, ref lpoint);

            jd15 += lpoint * dp + point * jd9;
            jd16 += point * dp + pointdt * jd9;
            j_time += dp;
        }

        private void CalcPoints(ref double xpoint, ref double npoint, ref double lpoint)
        {
            const double d5 = 2.0;
            if (isSYN)
            {
                xpoint = (jd1 * Math.Sin(jd15 - 0.13130908)) + (jd2 * Math.Sin(d5 * (jd15 - 2.8843198))) + (jd3 * Math.Sin(3.0 * (jd15 - 0.37448087)));
                npoint = (jd1 * Math.Cos(jd15 - 0.13130908)) + (d5 * jd2 * Math.Cos(d5 * (jd15 - 2.8843198))) + (3.0 * jd3 * Math.Cos(3.0 * (jd15 - 0.37448087)));
            }
            else
            {
                double x1 = j_argper + var19 * j_time;

                const double d = 5.7686396;
                const double d1 = 0.95240898;
                const double d2 = 1.8014998;
                const double d3 = 1.0508330;
                const double d4 = 4.4108898;
                
                xpoint = j2201 * Math.Sin(d5 * x1 + jd15 - d) +
                         j2211 * Math.Sin(jd15 - d) +
                         j3210 * Math.Sin(x1 + jd15 - d1) +
                         j3222 * Math.Sin(-x1 + jd15 - d1) +
                         j4410 * Math.Sin(d5 * x1 + d5 * jd15 - d2) +
                         j4422 * Math.Sin(d5 * jd15 - d2) +
                         j5220 * Math.Sin(x1 + jd15 - d3) +
                         j5232 * Math.Sin(-x1 + jd15 - d3) +
                         j5421 * Math.Sin(x1 + d5 * jd15 - d4) +
                         j5433 * Math.Sin(-x1 + d5 * jd15 - d4);

                npoint = j2201 * Math.Cos(d5 * x1 + jd15 - d) +
                         j2211 * Math.Cos(jd15 - d) +
                         j3210 * Math.Cos(x1 + jd15 - d1) +
                         j3222 * Math.Cos(-x1 + jd15 - d1) +
                         j5220 * Math.Cos(x1 + jd15 - d3) +
                         j5232 * Math.Cos(-x1 + jd15 - d3) +
                         d5 * (j4410 * Math.Cos(d5 * x1 + d5 * jd15 - d2) +
                               j4422 * Math.Cos(2 * jd15 - d2) +
                               j5421 * Math.Cos(x1 + d5 * jd15 - d4) +
                               j5433 * Math.Cos(-x1 + d5 * jd15 - d4));
            }

            lpoint = jd16 + jd13;
            npoint *= lpoint;
        }
    }
}
