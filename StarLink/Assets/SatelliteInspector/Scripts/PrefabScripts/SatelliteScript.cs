using System;
using SatelliteInspector.Scripts.SolutionScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SatelliteInspector.Scripts.PrefabScripts
{
    public class SatelliteScript : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// Satellite inspector
        /// </summary>
        public SatInspectorScript SIS;

        /// <summary>
        /// LineRenderer component - line from sattellite to place
        /// </summary>
        public LineRenderer LineToPlace;

        /// <summary>
        /// LineRenderer component - orbit
        /// </summary>
        public LineRenderer LineOrbit;

        /// <summary>
        /// Is checked in inspector toggle?
        /// </summary>
        public bool isChecked;

        /// <summary>
        /// Satellite info in TextMeshPro component
        /// </summary>
        public TextMeshPro Text;

        /// <summary>
        /// Begin information, give from datafile
        /// </summary>
        public string[] SatTLE = new string[3];

        /// <summary>
        /// Name of satellite
        /// </summary>
        public string Name;

        /// <summary>
        /// Decoder, decode info from data file
        /// </summary>
        public Decoder Dec;

        /// <summary>
        /// Parameters satellite`s orbit
        /// </summary>
        public Orbit Orb;

        /// <summary>
        /// Vector position and velocity
        /// </summary>
        public SatVector Vector;

        /// <summary>
        /// Component of created place
        /// </summary>
        private PlaceScript PlaceSC => SIS.Place.GetComponent<PlaceScript>();

        /// <summary>
        /// Get position in Unity coordinate system from ECI system
        /// </summary>
        private Vector3 UnityPos => Vector != null ? new Vector3(Vector.PositionECI.x, Vector.PositionECI.z, Vector.PositionECI.y) / 1000.0f : Vector3.zero;

        /// <summary>
        /// Get velocity in Unity coordinate system from ECI system
        /// </summary>
        private Vector3 UnityVel => Vector != null ? new Vector3(Vector.VelocityECI.x, Vector.VelocityECI.z, Vector.VelocityECI.y) / 1000.0f : Vector3.zero;

        /// <summary>
        /// Array of used materials
        /// </summary>
        public Material[] Materials = new Material[2];

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


        public string getElevation()
        {
            return $"{(Elevation * Mathf.Rad2Deg):0.00}";
        }

        public string getAzimuth()
        {
            return $"{(Azimut * Mathf.Rad2Deg):0.00}";
        }

        public string getDistance()
        {
            return $"{(Range):0.00}";
        }

        public string getLatitude()
        {
            return $"{GetLLA().x:0.00}";
        }

        public string getLongitude()
        {
            return $"{GetLLA().y:0.00}";
        }

        public string getAltitude()
        {
            return $"{GetLLA().z:0.00}";
        }

        /// <summary>
        /// Satellite position in LLA system: 
        /// .x - Latitude, Deg;
        /// .y - Longitude, Deg;
        /// .z - Altitude, km;
        /// </summary>
        private Vector3 GetLLA()
        {
            float theta0 = Mathf.Atan2(Vector.PositionECI.y, Vector.PositionECI.x);
            float theta = ((float)SIS.JTimeIns.ToGmsTime() - theta0) % (2 * Mathf.PI);

            if (theta < 0.0f) {theta += (Mathf.PI * 2);}
        
            float latitude = Mathf.Atan2(Vector.PositionECI.z, Mathf.Sqrt(Mathf.Pow(Vector.PositionECI.x, 2) + Mathf.Pow(Vector.PositionECI.y, 2)));
            float phi ;
            float c;

            const float coef = 1.0f / 298.26f;
            do
            {
                phi = latitude;
                c = 1.0f / Mathf.Sqrt(1.0f - (coef) * (2.0f - (coef)) * Mathf.Pow(Mathf.Sin(phi), 2));
                latitude = Mathf.Atan2(Vector.PositionECI.z + 6378.135f * c * (coef) * (2.0f - (coef)) * Mathf.Sin(phi), Mathf.Sqrt(Mathf.Pow(Vector.PositionECI.x, 2) + Mathf.Pow(Vector.PositionECI.y, 2)));
            }
            while (Mathf.Abs(latitude - phi) > 1.0e-07f);

            float latitudeDeg = latitude * Mathf.Rad2Deg;
            float longitudeDeg = theta * Mathf.Rad2Deg;
            float altitude = (Mathf.Sqrt(Mathf.Pow(Vector.PositionECI.x, 2) + Mathf.Pow(Vector.PositionECI.y, 2)) / Mathf.Cos(latitude)) - 6378.135f * c;

            return new Vector3(latitudeDeg, longitudeDeg, altitude);
        }

        /// <summary>
        /// Get topo vector
        /// </summary>
        /// <returns></returns>
        private Vector3 GetSEZ()
        {
            Debug.Log("GetSEZ");
            Debug.Log(Vector.PositionECI);
            Vector3 deltaPos = Vector.PositionECI - PlaceSC.CurrentECIpos;
            float sin_lat = Mathf.Sin(PlaceSC.CurrentLLApos.x * Mathf.Deg2Rad);
            float cos_lat = Mathf.Cos(PlaceSC.CurrentLLApos.x * Mathf.Deg2Rad);
            float thetaplace = (float)SIS.JTimeIns.ToLmsTime(PlaceSC.CurrentLLApos.y * Mathf.Deg2Rad);
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
            LineToPlace.enabled = false;
            LineOrbit.enabled = false;
        }

        /*
        * @author: dxm
        * @brief: 用于处理鼠标点击事件
        * @param: eventData 事件数据
        */
         public void OnPointerClick(PointerEventData eventData)
        {
            // 在这里添加你要执行的逻辑
            Debug.Log("Satellite " + Name + " was clicked!");
        }

        void Update()
        {
            transform.LookAt(UnityVel + UnityPos, Vector3.forward);
            Text.fontSize = TextSize;

            if (isChecked)
            {
                FindNewPosition(DateTime.Parse(SIS.InsTime));
                transform.LookAt(Camera.main.transform.position, Camera.main.transform.up);
                Text.transform.LookAt(-Camera.main.transform.position, Camera.main.transform.up);

                if (IsSelected)
                {
                    GetComponent<MeshRenderer>().material = Materials[1];
                    Text.text = SIS.isShowInfo ? SatInfo : Name;

                    if (SIS.isShowVisor)
                    {
                        LineToPlace.enabled = true;
                        LineToPlace.SetPosition(0, transform.position);
                        LineToPlace.SetPosition(1, SIS.Place.transform.position);
                    }
                    else
                    {
                        LineToPlace.enabled = false;
                    }

                    if (SIS.isShowOrbit)
                    {
                        LineOrbit.enabled = true;
                        float velocity = Vector.VelocityECI.magnitude;
                        float period = 2.0f * Mathf.PI * (6378.135f + (float)(Orb.Perigee + Orb.Apogee) / 2.0f) / velocity;
                        LineOrbit.positionCount = (int)(period / (60 * OrbQuality));

                        DateTime PredictTime = DateTime.Parse(SIS.InsTime);
                       
                        for (int i = 0; i < LineOrbit.positionCount; i++)
                        {
                            PredictTime = PredictTime.AddMinutes(1 * OrbQuality);
                            SatVector ResVecs = Orb.PositionEci(PredictTime);
                            Vector3 PosECI = ResVecs.PositionECI;
                            Vector3 orbitPoints = new Vector3(PosECI.x, PosECI.z, PosECI.y) / 1000.0f;
                            orbitPoints += SIS.transform.position;
                            LineOrbit.SetPosition(i, orbitPoints); 
                        }
                    }
                    else
                    {
                        LineOrbit.enabled = false;
                    }
                }
                else
                {
                    GetComponent<MeshRenderer>().material = Materials[0];
                    Text.text = Name;
                    LineToPlace.enabled = false; 
                    LineOrbit.enabled = false;
                }
            }

            //if (TextElevation != null) {
            //    TextElevation.text = $"{(Elevation * Mathf.Rad2Deg):0.00}";
            //}
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
            SIS = GetComponentInParent<SatInspectorScript>();
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
            transform.localPosition = UnityPos;
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
                transform.localPosition = UnityPos;
            }
        }

        /// <summary>
        /// Change material if satellite is visible from place
        /// </summary>
        public void ShowIfVisible() => GetComponent<MeshRenderer>().material = IsVisibleFromPlace ? Materials[1] : Materials[0];

        /// <summary>
        /// Satellite information
        /// </summary>
        private string SatInfo
        {
            get
            {
                string info = $"{Name}\n";
                info += $"Elevation:  {(Elevation * Mathf.Rad2Deg):0.00}\n";
                info += $"Azimuth:    {(Azimut * Mathf.Rad2Deg):0.00}\n";
                info += $"Distance:   {Range:0.0}\n";
                info += $"Latitude:   {GetLLA().x:0.00}\n";
                info += $"Longtitude: {GetLLA().y:0.00}\n";
                info += $"Altitude:   {GetLLA().z:0.00}\n";
                info += IsVisibleFromPlace ? "Visible: true" : "Visible: false";
                return info;
            }
        }
    }
}
