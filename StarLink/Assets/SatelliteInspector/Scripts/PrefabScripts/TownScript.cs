using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SatelliteInspector.Scripts.PrefabScripts
{
    public class TownScript : MonoBehaviour
    {
        /// <summary>
        /// SatInspectorScript
        /// </summary>
        public SatInspectorScript2D SIS;

        public RectTransform CanvRectTr;

        public RectTransform RectTr;

        /// <summary>
        /// Component for view name of place
        /// </summary>
        public TextMeshProUGUI LabelText;

        /// <summary>
        /// Array of names place
        /// </summary>
        public string[] NamesOfPlaces;

        /// <summary>
        /// Array of place positions
        /// </summary>
        public Vector3[] PosPlaces;

        /// <summary>
        /// Index of selected place
        /// </summary>
        public int Index;

        /// <summary>
        /// Places, geopositions
        /// </summary>
        private readonly Dictionary<string, Vector3> dicPlaces = new Dictionary<string, Vector3>();

        /// <summary>
        /// Current LLA position
        /// </summary>
        public Vector3 CurrentLLApos => LLA(Index);

        /// <summary>
        /// Current place position in ECI system
        /// </summary>
        public Vector3 CurrentECIpos => ECIposition(PosPlaces[Index]);

        /// <summary>
        /// Current place position in LLA system
        /// </summary>
        public Vector3 MapPosition
        {
            get
            {
                Rect rect = CanvRectTr.rect;
                float longtitude = LLA(Index).y + 180;
                float latitude = LLA(Index).x;
            
                Vector3 result = new Vector3
                {
                    [0] = longtitude * (rect.width / 360.0f) - rect.width/2.0f,
                    [1] = latitude * (rect.height / 180.0f),
                    [2] = 0.0f
                };
                return result;
            }
        }

        void Update()
        {
            RectTr.position = MapPosition;
        }

        /// <summary>
        /// Place position in LLA system
        /// </summary>
        /// <param name="index">selected index</param>
        /// <returns></returns>
        public Vector3 LLA(int index) => PosPlaces[index];

        /// <summary>
        /// Show selected place name in TextMesh component
        /// </summary>
        public void ShowName() => LabelText.text = NamesOfPlaces[Index];

        /// <summary>
        /// Load all place in dictonary
        /// </summary>
        public void LoadDicPlaces()
        {
            dicPlaces.Clear();

            dicPlaces.Add("London", new Vector3(51.50853f, -0.12574f, 0.255f));
            dicPlaces.Add("Moscow", new Vector3(55.75222f, 37.61556f, 0.245f));
            dicPlaces.Add("Washington", new Vector3(38.89511f, -77.03637f, 0.015f));
            dicPlaces.Add("Los Angeles", new Vector3(34.052223f, -118.24368f, 0.015f));
            dicPlaces.Add("NewYork", new Vector3(40.7147f, -74.00597f, 0.003f));
            dicPlaces.Add("Berlin", new Vector3(53.52437f, 13.37f, 0.035f));
            dicPlaces.Add("Paris", new Vector3(48.85341f, 2.3488f, 0.05f));
            dicPlaces.Add("Pekin", new Vector3(39.9075f, 116.39723f, 0.04f));
            dicPlaces.Add("Sydney", new Vector3(-33.86785f, 151.20732f, 0.28f));

            NamesOfPlaces = new string[dicPlaces.Count];
            PosPlaces = new Vector3[dicPlaces.Count];

            int counter = 0;
            foreach (KeyValuePair<string, Vector3> kv in dicPlaces)
            {
                NamesOfPlaces[counter] = kv.Key;
                PosPlaces[counter] = kv.Value;
                counter++;
            }
        }

        /// <summary>
        /// Calculate place position in ECI system
        /// </summary>
        /// <param name="LLA">position in LLA system</param>
        /// <returns></returns>
        private Vector3 ECIposition(Vector3 LLA)
        {
            Vector3 LLA_rad = new Vector3
            {
                [0] = LLA.x * Mathf.Deg2Rad,
                [1] = LLA.y * Mathf.Deg2Rad, 
                [2] = LLA.z
            };

            float thetaPlace = ((float)SIS.JTimeIns.ToGmsTime() + LLA_rad.y) % (Mathf.PI * 2);

            float c = 1 / Mathf.Sqrt(1 + (1 / 298.26f) * ((1 / 298.26f) - 2) * Mathf.Pow(Mathf.Sin(LLA_rad.x), 2));
            float s = Mathf.Pow(1 - (1 / 298.26f), 2) * c;
            float achcp = (6378.135f * c + LLA_rad.z) * Mathf.Cos(LLA_rad.x);

            float posXeci = achcp * Mathf.Cos(thetaPlace);
            float posYeci = achcp * Mathf.Sin(thetaPlace);
            float posZeci = (6378.135f * s + LLA_rad.z) * Mathf.Sin(LLA_rad.x);
        
            return new Vector3(posXeci, posYeci, posZeci);
        }
    }
}
