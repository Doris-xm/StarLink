using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SatelliteInspector.Scripts.PrefabScripts
{
    public class OneSystemScriptBase : MonoBehaviour
    {
        /// <summary>
        /// Is system checked?
        /// </summary>
        public bool isChecked;

        /// <summary>
        /// Satellite object - prefab
        /// </summary>
        public GameObject Satellite;

        /// <summary>
        /// List of created satellites
        /// </summary>
        public List<GameObject> SatellitesList;

        /// <summary>
        /// Array of satellites data
        /// </summary>
        public string[][] MatrixTLE;

        /// <summary>
        /// Path to satellites sustem data
        /// </summary>
        private string FileName => Directory.GetCurrentDirectory() + Data.DataFolder + name + Data.TxtExt;

        /// <summary>
        /// Text from file
        /// </summary>
        private string[] Lines => File.ReadAllLines(Path.Combine(FileName));

        /// <summary>
        /// Count sats in data file
        /// </summary>
        public int CountSats => Lines.Length / 3;

        /// <summary>
        /// Load satellite data matrix
        /// </summary>
        public void LoadMatrix()
        {
            MatrixTLE = new string[CountSats][];
            for (int i = 0; i < MatrixTLE.Length; i++)
            {
                MatrixTLE[i] = new string[3];
            }

            for (int i = 0; i < MatrixTLE.Length; i++)
            {
                MatrixTLE[i][0] = string.IsNullOrWhiteSpace(Lines[i * 3]) switch
                {
                    false => Lines[i * 3],
                    _ => MatrixTLE[i][0]
                };
                MatrixTLE[i][1] = string.IsNullOrWhiteSpace(Lines[i * 3 + 1]) switch
                {
                    false => Lines[i * 3 + 1],
                    _ => MatrixTLE[i][1]
                };
                MatrixTLE[i][2] = string.IsNullOrWhiteSpace(Lines[i * 3 + 2]) switch
                {
                    false => Lines[i * 3 + 2],
                    _ => MatrixTLE[i][2]
                };
            }
        }
    }
}