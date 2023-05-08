using System;
using System.Collections.Generic;
using SatelliteInspector.Scripts.SolutionScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SatelliteInspector.Scripts.PrefabScripts
{
    public class SatelliteScript2D : MonoBehaviour
    {
        /// <summary>
        /// Canvas
        /// </summary>
        public RectTransform CanvRect;

        /// <summary>
        /// Satellite inspector
        /// </summary>
        public SatInspectorScript2D SIS;

        /// <summary>
        /// Rect transform for satellite
        /// </summary>
        public RectTransform RectTr;

        /// <summary>
        /// LineRenderer component - line from sattellite to place
        /// </summary>
        public LineRenderer LineToPlace;

        /// <summary>
        /// LineRenderer component - orbit
        /// </summary>
        public LineRenderer LineOrbit;

        /// <summary>
        /// LineRenderer component for second trace
        /// </summary>
        public LineRenderer LineOrbit2;

        /// <summary>
        /// Is checked in inspector toggle?
        /// </summary>
        public bool isChecked;

        /// <summary>
        /// Satellite info in TextMeshPro component
        /// </summary>
        public TextMeshProUGUI Text;

        /// <summary>
        /// Begin information, give from datafile
        /// </summary>
        [HideInInspector] public string[] SatTLE = new string[3];

        /// <summary>
        /// Name of satellite
        /// </summary>
        [HideInInspector] public string Name;

        /// <summary>
        /// Decoder, decode info from data file
        /// </summary>
        [HideInInspector] public Decoder Dec;

        /// <summary>
        /// Parameters satellite`s orbit
        /// </summary>
        [HideInInspector] public Orbit Orb;

        /// <summary>
        /// Vector position and velocity
        /// </summary>
        [HideInInspector] public SatVector Vector;

        private List<Vector3> ListOrb1 = new List<Vector3>();
        private List<Vector3> ListOrb2 = new List<Vector3>();

        /// <summary>
        /// Component of created place
        /// </summary>
        private TownScript PlaceSC => SIS.Place.GetComponent<TownScript>();

        /// <summary>
        /// Satellite`s rawimage
        /// </summary>
        private RawImage img => GetComponent<RawImage>();

        /// <summary>
        /// Get position in Unity coordinate system from ECI system
        /// </summary>
        private Vector3 UnityPos
        {
            get
            {
                Vector3 geo = GetLLA();
                Rect rect = CanvRect.rect;
                Vector3 result = new Vector3
                {
                    [0] = geo.y * (rect.width / 360.0f) - rect.width / 2.0f,
                    [1] = geo.x * (rect.height / 180.0f),
                    [2] = 0.0f
                };
                return result;
            }
        }

        private Vector3 UnityPos2D(Vector3 LLA)
        {
            Vector3 geo = LLA;
            Rect rect = CanvRect.rect;
            Vector3 result = new Vector3
            {
                [0] = geo.y * (rect.width / 360.0f) - rect.width / 2.0f,
                [1] = geo.x * (rect.height / 180.0f),
                [2] = 0.0f
            };
            return result;
        }

        /// <summary>
        /// Get velocity in Unity coordinate system from ECI system
        /// </summary>
        public Vector3 UnityVel => Vector != null
            ? new Vector3(Vector.VelocityECI.x, Vector.VelocityECI.z, Vector.VelocityECI.y) / 1000.0f
            : Vector3.zero;

        /// <summary>
        /// Satellite is selected?
        /// </summary>
        public bool IsSelected;

        /// <summary>
        /// Visual orbit quality give from inspector slider
        /// </summary>
        private float OrbQuality => 11 - SIS.OrbitQual_SL;

        /// <summary>
        /// Text size settings
        /// </summary>
        private int TextSize => SIS.TextSize;

        /// <summary>
        /// Is satellite visible from place?
        /// </summary>
        private bool IsVisibleFromPlace => Elevation > 0.0f;

        /// <summary>
        /// Satellite position in LLA system: 
        /// .x - Latitude, Deg;
        /// .y - Longitude, Deg;
        /// .z - Altitude, km;
        /// </summary>
        private Vector3 GetLLA()
        {
            double theta = (Mathf.Atan2(Vector.PositionECI.y, Vector.PositionECI.x) - SIS.JTimeIns.ToGmsTime()  + Math.PI - 9.0f * Mathf.Deg2Rad) % (2 * Math.PI);

            if (theta < 0.0)
            {
                theta += (Math.PI * 2);
            }

            float lat = Mathf.Atan2(Vector.PositionECI.z, Mathf.Sqrt(Mathf.Pow(Vector.PositionECI.x, 2) + Mathf.Pow(Vector.PositionECI.y, 2)));
            float phi;
            float c;

            const float coef = 1.0f / 298.26f;
            do
            {
                phi = lat;
                c = 1.0f / Mathf.Sqrt(1.0f - (coef) * (2.0f - (coef)) * Mathf.Pow(Mathf.Sin(phi), 2));
                lat = Mathf.Atan2(Vector.PositionECI.z + EarthScript.Radius * c * (coef) * (2.0f - (coef)) * Mathf.Sin(phi),
                    Mathf.Sqrt(Mathf.Pow(Vector.PositionECI.x, 2) + Mathf.Pow(Vector.PositionECI.y, 2)));
            } while (Mathf.Abs(lat - phi) > 1.0e-07f);

            float latitudeDeg = lat * Mathf.Rad2Deg;
            float longitudeDeg = (float)(theta * Mathf.Rad2Deg);
            float altitude = (Mathf.Sqrt(Mathf.Pow(Vector.PositionECI.x, 2) + Mathf.Pow(Vector.PositionECI.y, 2)) / Mathf.Cos(lat)) -
                             EarthScript.Radius * c;

            Vector3 result = new Vector3(latitudeDeg, longitudeDeg, altitude);

            return result;
        }

        /// <summary>
        /// 2D Orbit calculation 
        /// </summary>
        /// <param name="positionECI"></param>
        /// <returns></returns>
        private Vector3 GetLLA(Vector3 positionECI)
        {
            const float coef = 1.0f / 298.26f;
        
            double theta = (Mathf.Atan2(positionECI.y, positionECI.x) - SIS.JTimeIns.ToGmsTime() + Math.PI - 9.0f * Mathf.Deg2Rad) % (2 * Math.PI);

            if (theta < 0.0f)
            {
                theta += (Mathf.PI * 2);
            }

            float lat = Mathf.Atan2(positionECI.z, Mathf.Sqrt(Mathf.Pow(positionECI.x, 2) + Mathf.Pow(positionECI.y, 2)));
            float phi;
            float c;
 
            do
            {
                phi = lat;
                c = 1.0f / Mathf.Sqrt(1.0f - (coef) * (2.0f - (coef)) * Mathf.Pow(Mathf.Sin(phi), 2));
                lat = Mathf.Atan2(positionECI.z + EarthScript.Radius * c * (coef) * (2.0f - (coef)) * Mathf.Sin(phi),
                    Mathf.Sqrt(Mathf.Pow(positionECI.x, 2) + Mathf.Pow(positionECI.y, 2)));
            } while (Mathf.Abs(lat - phi) > 1.0e-07f);

            float latitudeDeg = lat * Mathf.Rad2Deg;
            float longitudeDeg = (float)theta * Mathf.Rad2Deg;
            float altitude = (Mathf.Sqrt(Mathf.Pow(positionECI.x, 2) + Mathf.Pow(positionECI.y, 2)) / Mathf.Cos(lat)) - EarthScript.Radius * c;

            Vector3 result = new Vector3(latitudeDeg, longitudeDeg, altitude);

            return result;
        }

        /// <summary>
        /// Get topo vector
        /// </summary>
        /// <returns></returns>
        private Vector3 GetSEZ()
        {
            Vector3 deltaPos = Vector.PositionECI - PlaceSC.CurrentECIpos;
            float sin_lat = Mathf.Sin(PlaceSC.CurrentLLApos.x * Mathf.Deg2Rad);
            float cos_lat = Mathf.Cos(PlaceSC.CurrentLLApos.x * Mathf.Deg2Rad);
            float thetaplace = (float) SIS.JTimeIns.ToLmsTime((PlaceSC.CurrentLLApos.y) * Mathf.Deg2Rad);
            float sin_theta = Mathf.Sin(thetaplace);
            float cos_theta = Mathf.Cos(thetaplace);

            float top_s = sin_lat * cos_theta * deltaPos.x + sin_lat * sin_theta * deltaPos.y - cos_lat * deltaPos.z;
            float top_e = -sin_theta * deltaPos.x + cos_theta * deltaPos.y;
            float top_z = cos_lat * cos_theta * deltaPos.x + cos_lat * sin_theta * deltaPos.y + sin_lat * deltaPos.z;

            return new Vector3(top_s, top_e, top_z);
        }

        /// <summary>
        /// Magnitude to place
        /// </summary>
        private float Range => GetSEZ().magnitude;

        /// <summary>
        /// Azimuth 
        /// </summary>
        private float Azimut
        {
            get
            {
                Vector3 sez = GetSEZ();
                float az = Mathf.Atan(-sez.y / sez.x);
                if (sez.x > 0.0f) az += Mathf.PI;
                if (az < 0.0f) az += 2.0f * Mathf.PI;
                return az;
            }
        }

        /// <summary>
        /// Satellite place elevation, in deg
        /// </summary>
        private float Elevation => Mathf.Asin(GetSEZ().z / (Vector.PositionECI - PlaceSC.CurrentECIpos).magnitude);

        private void Awake()
        {
            //lines is enabled
            LineToPlace.enabled = false;
            LineOrbit.enabled = false;
            LineOrbit2.enabled = false;
        }

        void Update()
        {
            Text.fontSize = TextSize;

            switch (isChecked)
            {
                case true:
                {
                    FindNewPosition(DateTime.Parse(SIS.InsTime));

                    switch (IsSelected)
                    {
                        case true:
                        {
                            img.color = Color.red;
                            Text.text = SIS.isShowInfo ? SatInfo : Name;

                            switch (SIS.isShowVisor)
                            {
                                case true:
                                    LineToPlace.enabled = true;
                                    LineToPlace.SetPosition(0, transform.position);
                                    LineToPlace.SetPosition(1, SIS.Place.transform.position);
                                    break;
                                default:
                                    LineToPlace.enabled = false;
                                    break;
                            }

                            switch (SIS.isShowOrbit)
                            {
                                case true:
                                {
                                    LineOrbit.enabled = true;
                                    LineOrbit2.enabled = true;

                                    ListOrb1.Clear();
                                    ListOrb2.Clear();

                                    float velocity = Vector.VelocityECI.magnitude;
                                    float period = 2.0f * Mathf.PI * (EarthScript.Radius + (float)(Orb.Perigee + Orb.Apogee) / 2.0f) / velocity;

                                    int countpoints = (int) (period / (60 * OrbQuality));

                                    DateTime PredictTime = DateTime.Parse(SIS.InsTime);
                                
                                    for (int i = 0; i < countpoints - 5 ; i++)
                                    {
                                        PredictTime = PredictTime.AddMinutes(OrbQuality);
                                        SatVector ResVecs = Orb.PositionEci(PredictTime);
                                        Vector3 neworbitPoints = UnityPos2D(GetLLA(ResVecs.PositionECI));

                                        if (GetLLA(ResVecs.PositionECI).y > GetLLA().y && GetLLA(ResVecs.PositionECI).y < 360.0f)
                                        {
                                            ListOrb1.Add(neworbitPoints);
                                        }

                                        if (GetLLA(ResVecs.PositionECI).y < GetLLA().y && GetLLA(ResVecs.PositionECI).y > 0.0f)
                                        {
                                            ListOrb2.Add(neworbitPoints);
                                        }
                                    }

                                    LineOrbit.positionCount = ListOrb1.Count;
                                    LineOrbit2.positionCount = ListOrb2.Count;
                                    LineOrbit.SetPositions(ListOrb1.ToArray());
                                    LineOrbit2.SetPositions(ListOrb2.ToArray());
                                    break;
                                }
                                default:
                                    LineOrbit.enabled = false;
                                    LineOrbit2.enabled = false;
                                    break;
                            }

                            break;
                        }
                        default:
                            img.color = Color.yellow;
                            Text.text = Name;
                            LineToPlace.enabled = false;
                            LineOrbit.enabled = false;
                            LineOrbit2.enabled = false;
                            break;
                    }

                    break;
                }
            }
        }

        private void OnMouseDown()
        {
            IsSelected = !IsSelected;
        }

        /// <summary>
        /// Create satellite
        /// </summary>
        /// <param name="subSys">parent system</param>
        /// <param name="index">satellite index</param>
        public void Init(OneSystemScriptBase subSys, int index)
        {
            SIS = GetComponentInParent<SatInspectorScript2D>();
            for (int n = 0; n < SatTLE.Length; n++)
            {
                SatTLE[n] = subSys.MatrixTLE[index][n];
            }

            Name = SatTLE[0];

            Dec = ScriptableObject.CreateInstance<Decoder>();
            Dec.StartDecode(SatTLE);
            Orb = ScriptableObject.CreateInstance<Orbit>();
            Orb.CreateOrbit(Dec);
            Vector = ScriptableObject.CreateInstance<SatVector>();
            Vector = Orb.PositionEci(DateTime.Parse(SIS.InsTime));
            name = Name;
            RectTr.position = UnityPos;
            Text.text = name;
        }

        /// <summary>
        /// Calculate new position of the satellite
        /// </summary>
        /// <param name="utc"></param>
        public void FindNewPosition(DateTime utc)
        {
            if (Vector != null)
            {
                Dec.StartDecode(SatTLE);
                Orb.CreateOrbit(Dec);
                Vector = Orb.PositionEci(utc);
                RectTr.position = UnityPos;
            }
        }

        //public void ShowIfVisible() => GetComponent<MeshRenderer>().material = IsVisibleFromPlace ? Materials[1] : Materials[0];

        /// <summary>
        /// Satellite information
        /// </summary>
        private string SatInfo
        {
            get
            {
                string info = Name + "\n";
                info += $"Elevation:  {(Elevation * Mathf.Rad2Deg):0.00}\n";
                info += $"Azimuth:    {(Azimut * Mathf.Rad2Deg):0.00}\n";
                info += $"Distance:   {Range:0.0}\n";
                info += $"Latitude:   {GetLLA().x:0.00}\n";
                info += $"Longtitude: {(GetLLA().y - 180.0f):0.00}\n";
                info += $"Altitude:   {GetLLA().z:0.00}\n";
                info += IsVisibleFromPlace ? "Visible: true" : "Visible: false";

                return info;
            }
        }
    }
}