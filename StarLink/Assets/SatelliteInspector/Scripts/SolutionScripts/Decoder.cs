using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SatelliteInspector.Scripts.SolutionScripts
{
    /// <summary>
    /// Decoder files from data folder
    /// </summary>
    public class Decoder : ScriptableObject
    {
        /// <summary>
        /// Orbit parameters
        /// </summary>
        public enum Params
        {
            NumSat,
            Inter,
            Number,
            Year,
            Day,
            Epoch,
            Incl,
            R_A,
            Eccentricity,
            ArgPer,
            Anomaly,
            Motion,
            FirstMotion,
            SecondMotion,
            Drag
        }

        public enum Unit
        {
            Rad,
            Deg,
            Incl
        }

        private string TLE1 { get; set; }
        private string TLE2 { get; set; }

        private Dictionary<Params, string> dictonaryParams;
        private Dictionary<int, double> dictonaryInts;

        #region GETFIELD

        private static string EtD(string str)
        {
            double tempvalue =
                double.Parse(str.Substring(0, 1) + "0." + str.Substring(1, 5) + "e" + str.Substring(6, 2).TrimStart(),
                    CultureInfo.InvariantCulture);
            int dig = str.Substring(1, 5).Length +
                      Math.Abs(int.Parse(str.Substring(6, 2).TrimStart(), CultureInfo.InvariantCulture));
            return tempvalue.ToString("F" + dig, CultureInfo.InvariantCulture);
        }

        private static int Key(Unit u, Params f) => ((int)u * 100) + (int)f;

        private static double ChangeUnits(double valNative, Params fld, Unit units)
        {
            switch (fld)
            {
                case Params.Incl:
                case Params.R_A:
                case Params.ArgPer:
                case Params.Anomaly:
                    if (units == Unit.Rad)
                    {
                        return valNative * Mathf.Deg2Rad;
                    }

                    break;
                case Params.NumSat:
                    break;
                case Params.Inter:
                    break;
                case Params.Number:
                    break;
                case Params.Year:
                    break;
                case Params.Day:
                    break;
                case Params.Epoch:
                    break;
                case Params.Eccentricity:
                    break;
                case Params.Motion:
                    break;
                case Params.FirstMotion:
                    break;
                case Params.SecondMotion:
                    break;
                case Params.Drag:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fld), fld, null);
            }

            return valNative;
        }

        public double GetField(Params fld) => GetField(fld, Unit.Incl);

        public double GetField(Params fld, Unit units)
        {
            if (dictonaryInts.ContainsKey(Key(units, fld)))
            {
                return dictonaryInts[Key(units, fld)];
            }
            else
            {
                double nValue = double.Parse(dictonaryParams[fld], CultureInfo.InvariantCulture);
                double changedVal = ChangeUnits(nValue, fld, units);
                dictonaryInts[Key(units, fld)] = changedVal;
                return changedVal;
            }
        }

        #endregion

        /// <summary>
        /// Start decode satellite data
        /// </summary>
        /// <param name="tle">three line element</param>
        public void StartDecode(string[] tle)
        {
            TLE1 = tle[1];
            TLE2 = tle[2];

            dictonaryParams = new Dictionary<Params, string>();
            dictonaryInts = new Dictionary<int, double>();

            dictonaryParams[Params.NumSat] = TLE1.Substring(2, 5);
            dictonaryParams[Params.Inter] = TLE1.Substring(9, 8);
            dictonaryParams[Params.Year] = TLE1.Substring(18, 2);
            dictonaryParams[Params.Day] = TLE1.Substring(20, 12);
            switch (TLE1[33])
            {
                case '-':
                    dictonaryParams[Params.FirstMotion] = "-0";
                    break;
                default:
                    dictonaryParams[Params.FirstMotion] = "0";
                    break;
            }

            dictonaryParams[Params.FirstMotion] += TLE1.Substring(34, 10);
            dictonaryParams[Params.SecondMotion] = EtD(TLE1.Substring(44, 8));
            dictonaryParams[Params.Drag] = EtD(TLE1.Substring(53, 8));
            dictonaryParams[Params.Number] = TLE1.Substring(64, 4).TrimStart();
            dictonaryParams[Params.Incl] = TLE2.Substring(8, 8).TrimStart();
            dictonaryParams[Params.R_A] = TLE2.Substring(17, 8).TrimStart();
            dictonaryParams[Params.Eccentricity] = "0." + TLE2.Substring(26, 7);
            dictonaryParams[Params.ArgPer] = TLE2.Substring(34, 8).TrimStart();
            dictonaryParams[Params.Anomaly] = TLE2.Substring(43, 8).TrimStart();
            dictonaryParams[Params.Motion] = TLE2.Substring(52, 11).TrimStart();
            dictonaryParams[Params.Epoch] = TLE2.Substring(63, 5).TrimStart();
        }
    }
}