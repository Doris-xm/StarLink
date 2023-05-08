using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SatelliteInspector.Scripts.PrefabScripts
{
    public class PlaceScript : MonoBehaviour
    {
        /// <summary>
        /// SatInspectorScript
        /// </summary>
        public SatInspectorScript SIS;

        /// <summary>
        /// Component for view name of place
        /// </summary>
        public TextMeshPro LabelText;

        public Transform PositionLabel;

        /// <summary>
        /// Places
        /// </summary>
        private readonly Dictionary<string, Vector3> dicPlaces = new Dictionary<string, Vector3>();

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

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        /// <summary>
        /// Place position in LLA system
        /// </summary>
        /// <param name="index">selected index</param>
        /// <returns></returns>
        public Vector3 LLA(int index) => PosPlaces[index];

        /// <summary>
        /// Current place position in ECI system
        /// </summary>
        public Vector3 CurrentECIpos => ECIposition(PosPlaces[Index]);

        /// <summary>
        /// Current place position in LLA system
        /// </summary>
        public Vector3 CurrentLLApos => LLA(Index);

        /// <summary>
        /// Get place position in Unity system
        /// </summary>
        /// <param name="ecipos"></param>
        /// <returns></returns>
        public Vector3 UnityPos(Vector3 ecipos) => new Vector3(ecipos.x, ecipos.z, ecipos.y) / 1000.0f;

        void Update()
        {
            transform.localPosition = UnityPos(CurrentECIpos);
            LabelText.transform.LookAt(-mainCamera.transform.position, mainCamera.transform.up);
            PositionLabel.localPosition = transform.position.normalized * 6.4f;
        }

        /// <summary>
        /// Show selected place name in TextMesh component
        /// </summary>
        public void ShowName()
        {
            LabelText.text = NamesOfPlaces[Index];
        }

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
            dicPlaces.Add("Berlin", new Vector3(53.52437f, 13.41053f, 0.035f));
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
        public Vector3 ECIposition(Vector3 LLA)
        {
            Vector3 LLA_rad = new Vector3
            {
                [0] = LLA.x * Mathf.Deg2Rad,
                [1] = LLA.y * Mathf.Deg2Rad,
                [2] = LLA.z
            };

            float thetaPlace = ((float) SIS.JTimeIns.ToGmsTime() + LLA_rad.y) % (Mathf.PI * 2);

            const float earthcoef = 1 / 298.26f;
            float c = 1 / Mathf.Sqrt(1 + earthcoef * (earthcoef - 2) * Mathf.Pow(Mathf.Sin(LLA_rad.x), 2));
            float s = Mathf.Pow(1 - earthcoef, 2) * c;
            float achcp = (EarthScript.Radius * c + LLA_rad.z) * Mathf.Cos(LLA_rad.x);

            float posXeci = achcp * Mathf.Cos(thetaPlace);
            float posYeci = achcp * Mathf.Sin(thetaPlace);
            float posZeci = (EarthScript.Radius * s + LLA_rad.z) * Mathf.Sin(LLA_rad.x);

            return new Vector3(posXeci, posYeci, posZeci);
        }
    }
}