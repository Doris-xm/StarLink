using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SatelliteInspector.Scripts.PrefabScripts
{
    public class SystScriptBase : MonoBehaviour
    {
        /// <summary>
        /// Count of the data files
        /// </summary>
        public int Count;

        /// <summary>
        /// Name of the all data files
        /// </summary>
        public string[] NamesAllSystems;

        /// <summary>
        /// Selected system (in inspector)
        /// </summary>
        public int PopupSelectedIndex;

        /// <summary>
        /// Empty system for create
        /// </summary>
        public GameObject EmtySystem;

        /// <summary>
        /// Index of the selected system
        /// </summary>
        public int indexFromList;

        private OneSystemScriptBase[] OneSystems => GetComponentsInChildren<OneSystemScriptBase>();
        
        /// <summary>
        /// Get List of created systems
        /// </summary>
        public List<GameObject> ListCreatedSystems
        {
            get
            {
                List <GameObject> list = new List<GameObject>();
                for (int i = 0; i < GetComponentsInChildren<OneSystemScriptBase>().Length; i++)
                {
                    list.Add(GetComponentsInChildren<OneSystemScriptBase>()[i].gameObject);
                }
                return list;
            }
        }
        
        /// <summary>
        /// Selected single system
        /// </summary>
        public OneSystemScriptBase SelectedSystem
        {
            get
            {
                return ListCreatedSystems.Count != 0 ? ListCreatedSystems[indexFromList].GetComponent<OneSystemScriptBase>() : null;
            }
        }
        
        /// <summary>
        /// Get array of the systems name
        /// </summary>
        public string[] NamesSystem
        {
            get
            {
                string[] names = new string[OneSystems.Length];
                for (int i = 0; i < OneSystems.Length; i++)
                {
                    names[i] = OneSystems[i].name;
                }
                return names;
            }
        }
        
        
        
        /// <summary>
        /// Get all names of the system from data directory
        /// </summary>
        public void GetNamesOfSystems()
        {        
            string[] dirsDataFiles = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory() + Data.DataFolder), "*.txt");
            Count = dirsDataFiles.Length;
            NamesAllSystems = new string[Count];
            for (int i = 0; i < NamesAllSystems.Length; i++)
            {
                NamesAllSystems[i] = Path.GetFileNameWithoutExtension(dirsDataFiles[i]);
            }
        }
    }
}