using System;
using System.Collections;
using System.IO;
using SatelliteInspector.Scripts.SolutionScripts;
using UnityEngine;
using UnityEngine.Networking;

namespace SatelliteInspector.Scripts.PrefabScripts
{
    public interface ISatInsBase
    {
    }

    public class SatInsBase : MonoBehaviour, ISatInsBase
    {
        public GameObject Earth;
        public GameObject Place;
        public GameObject Systems;
        [SerializeField] public JTime JTimeIns;
        [SerializeField] public string InsTime;
        [SerializeField] public float Timefactor;
        [SerializeField] public float OrbitQual_SL;
        [SerializeField] public bool isShowInfo;
        [SerializeField] public bool isShowOrbit;
        [SerializeField] public bool isShowVisor;
        [SerializeField] public int TextSize;

        private void Awake()
        {
            JTimeIns = ScriptableObject.CreateInstance<JTime>();
            JTimeIns.InitAtTime(DateTime.Parse(InsTime));
        }

        private void OnGUI()
        {
            //Show simulation time in playmode
            DateTime simTime = DateTime.Parse(InsTime);
            GUI.Label(new Rect(10, 10, 250, 20), "Simulation time: " + simTime);
        }

        private void Update()
        {
            #region DateTime "timefactor"

            DateTime itime = DateTime.Parse(InsTime);
            if (Mathf.Approximately(Timefactor, 1.0f))
            {
                itime = itime.AddSeconds(Time.deltaTime);
                JTimeIns.InitAtTime(itime);
            }
            else
            {
                itime = itime.AddSeconds(10 * Timefactor * Time.deltaTime);
                JTimeIns.InitAtTime(itime);
            }

            InsTime = itime.ToString("o");

            #endregion
        }

        /// <summary>
        /// Update satellites data files
        /// </summary>
        /// <param name="filename">name of file without extension</param>
        public void UpdateFile(string filename) => StartCoroutine(GetText(filename));

        /// <summary>
        /// Update data from WWW
        /// </summary>
        /// <param name="filename">name of file without extension</param>
        /// <returns></returns>
        private IEnumerator GetText(string filename)
        {
            using UnityWebRequest Loader = UnityWebRequest.Get($"http://celestrak.com/NORAD/elements/{filename}.txt");
            yield return Loader.SendWebRequest();
            if (Loader.isNetworkError || Loader.isHttpError)
            {
                Debug.Log(Loader.error);
            }
            else
            {
                string Datatext = Loader.downloadHandler.text;
                File.WriteAllText(Directory.GetCurrentDirectory() + Data.DataFolder + filename + ".txt", Datatext);
                Debug.Log(filename + " is updated.");
            }
        }
    }
}