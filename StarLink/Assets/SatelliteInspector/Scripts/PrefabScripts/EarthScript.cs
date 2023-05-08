using UnityEngine;

namespace SatelliteInspector.Scripts.PrefabScripts
{
    public class EarthScript : MonoBehaviour
    {
        public const float Radius = 6378.135f;
        public const float SecInDay = 86400.0f;
        public const float SideralRotation = 1.00273790934f;

        public SatInspectorScript SIS;

        /// <summary>
        /// Earth rotate angle
        /// </summary>
        private float ThetaEarth => (float)SIS.JTimeIns.ToGmsTime() % (2 * Mathf.PI);

        void Update()
        {
            RotateEarth();
        }

        /// <summary>
        /// Set euler angles and transform Earth
        /// </summary>
        public void RotateEarth() => transform.eulerAngles = new Vector3(0.0f, - ThetaEarth * Mathf.Rad2Deg, 0.0f);

    }
}
