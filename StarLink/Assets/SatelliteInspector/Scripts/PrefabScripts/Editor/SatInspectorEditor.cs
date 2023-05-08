using System;
using UnityEditor;
using UnityEngine;

namespace SatelliteInspector.Scripts.PrefabScripts.Editor
{
    [CustomEditor(typeof(SatInspectorScript))]
    public class SatInspectorEditor : SatInspectorEditorBase
    {
        private EarthScript EarthSC;
        private SystemsScript SystemsSC;
        private PlaceScript PlaceSC;
        private SatelliteScript[] AllSats;

        public override void OnEnable()
        {
            //SatInspectorScript being inspected
            ins = target as SatInspectorScript;
            base.OnEnable();

            //Init EarthScript for gameobject "Earth" and set rotation
            EarthSC = ins.Earth.GetComponent<EarthScript>();
            EarthSC.RotateEarth();

            //Init PlaceScript for gameobject "Place" 
            PlaceSC = ins.Place.GetComponent<PlaceScript>();
            PlaceSC.LoadDicPlaces();
            PlaceSC.ShowName();

            //Init SystemsScript for gameobject "Systems"
            SystemsSC = ins.Systems.GetComponent<SystemsScript>();
            SystemsSC.GetNamesOfSystems();

            //Find all created SatellitesScripts (need onle for Editor)
            AllSats = ins.GetComponentsInChildren<SatelliteScript>();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            #region Time control buttons
            FadeTime = EditorGUILayout.Foldout(FadeTime, "Time options");
            if (FadeTime)
            {
                if (GUILayout.Button("Set current time"))
                {
                    InsTime.stringValue = DateTime.UtcNow.ToString("o");
                    ins.JTimeIns.InitAtTime(DateTime.Parse(InsTime.stringValue));
                    EarthSC.RotateEarth();
                    UpdateAllSats();
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+ Day"))
                {
                    InsTime.stringValue = DateTime.Parse(InsTime.stringValue).AddDays(1.0).ToString("o");
                    ins.JTimeIns.InitAtTime(DateTime.Parse(InsTime.stringValue));
                    EarthSC.RotateEarth();
                    UpdateAllSats();
                }
                if (GUILayout.Button("+ Hour"))
                {
                    InsTime.stringValue = DateTime.Parse(InsTime.stringValue).AddHours(1.0).ToString("o");
                    ins.JTimeIns.InitAtTime(DateTime.Parse(InsTime.stringValue));
                    EarthSC.RotateEarth();
                    UpdateAllSats();
                }
                if (GUILayout.Button("+ Min"))
                {
                    InsTime.stringValue = DateTime.Parse(InsTime.stringValue).AddMinutes(1.0).ToString("o");
                    ins.JTimeIns.InitAtTime(DateTime.Parse(InsTime.stringValue));
                    EarthSC.RotateEarth();
                    UpdateAllSats();
                }
                if (GUILayout.Button("+ Sec"))
                {
                    InsTime.stringValue = DateTime.Parse(InsTime.stringValue).AddSeconds(1.0).ToString("o");
                    ins.JTimeIns.InitAtTime(DateTime.Parse(InsTime.stringValue));
                    EarthSC.RotateEarth();
                    UpdateAllSats();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("- Day"))
                {
                    InsTime.stringValue = DateTime.Parse(InsTime.stringValue).AddDays(-1.0).ToString("o");
                    ins.JTimeIns.InitAtTime(DateTime.Parse(InsTime.stringValue));
                    EarthSC.RotateEarth();
                    UpdateAllSats();
                }
                if (GUILayout.Button("- Hour"))
                {
                    InsTime.stringValue = DateTime.Parse(InsTime.stringValue).AddHours(-1.0).ToString("o");
                    ins.JTimeIns.InitAtTime(DateTime.Parse(InsTime.stringValue));
                    EarthSC.RotateEarth();
                    UpdateAllSats();
                }
                if (GUILayout.Button("- Min"))
                {
                    InsTime.stringValue = DateTime.Parse(InsTime.stringValue).AddMinutes(-1.0).ToString("o");
                    ins.JTimeIns.InitAtTime(DateTime.Parse(InsTime.stringValue));
                    EarthSC.RotateEarth();
                    UpdateAllSats();
                }
                if (GUILayout.Button("- Sec"))
                {
                    InsTime.stringValue = DateTime.Parse(InsTime.stringValue).AddSeconds(-1.0).ToString("o");
                    ins.JTimeIns.InitAtTime(DateTime.Parse(InsTime.stringValue));
                    EarthSC.RotateEarth();
                    UpdateAllSats();
                }
                EditorGUILayout.EndHorizontal();
            }
            if (Application.isPlaying)
            {
                TimeFactor.floatValue = EditorGUILayout.Slider(TimeFactor.floatValue, 1.0f, 60.0f);
            }
            #endregion

            #region Set place
            GUILayout.Box("Place settings", EditorStyles.helpBox);
            PlaceSC.Index = EditorGUILayout.Popup("Select Place", PlaceSC.Index, PlaceSC.NamesOfPlaces);
            PlaceSC.ShowName();
            EditorGUILayout.Vector3Field("LLA system position", PlaceSC.LLA(PlaceSC.Index));
            //EditorGUILayout.Vector3Field("ECI system position", PlaceSC.ECIposition(PlaceSC.LLA(PlaceSC.Index)));
            Vector3 PlaceUnityPos = PlaceSC.UnityPos(PlaceSC.ECIposition(PlaceSC.LLA(PlaceSC.Index)));
            PlaceSC.transform.localPosition = PlaceUnityPos;
            #endregion

            #region Create and delete systems
            GUILayout.Box("Add or remove satellites systems", EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            SystemsSC.PopupSelectedIndex = EditorGUILayout.Popup(SystemsSC.PopupSelectedIndex, SystemsSC.NamesAllSystems);
            if (GUILayout.Button("Add"))
            {
                int indexOverwriteSystem = -1;
                foreach (GameObject createdsys in SystemsSC.ListCreatedSystems)
                {
                    if (SystemsSC.NamesAllSystems[SystemsSC.PopupSelectedIndex] == createdsys.name)
                    {
                        if (EditorUtility.DisplayDialog(
                                title: "Satellite system <" + createdsys.name + ">",
                                message: "This SatSystem already exists. Do you want to overwrite it?",
                                ok: "Overwrite",
                                cancel: "Create New"))
                            //OnOverWrite
                        {
                            indexOverwriteSystem = SystemsSC.ListCreatedSystems.IndexOf(createdsys);
                        }
                    }
                }

                if (indexOverwriteSystem != -1)
                {
                    SystemsSC.ListCreatedSystems.RemoveAt(indexOverwriteSystem);
                    DestroyImmediate(SystemsSC.ListCreatedSystems[indexOverwriteSystem]);
                }

                GameObject system = Instantiate(SystemsSC.EmtySystem, SystemsSC.transform);
                system.name = SystemsSC.NamesAllSystems[SystemsSC.PopupSelectedIndex];
                SystemsSC.ListCreatedSystems.Add(system);
                SystemsSC.indexFromList = 0;
            }

            if (GUILayout.Button("Remove"))
            {
                SystemsSC.indexFromList = 0;
                for (int k = 0; k < CountSelectedSystems; k++)
                {
                    for (int i = 0; i < SystemsSC.ListCreatedSystems.Count; i++)
                    {
                        OneSystemScript onsys = SystemsSC.ListCreatedSystems[i].GetComponent<OneSystemScript>();
                        if (onsys.isChecked)
                        {
                            SystemsSC.ListCreatedSystems.RemoveAt(i);
                            DestroyImmediate(SystemsSC.ListCreatedSystems[i]);
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            using (var h = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos, GUILayout.Width(0), GUILayout.Height(0)))
                {
                    scrollPos = scrollView.scrollPosition;
                    CountSelectedSystems = 0;
                    for (int i = 0; i < SystemsSC.ListCreatedSystems.Count; i++)
                    {
                        OneSystemScript onsys = SystemsSC.ListCreatedSystems[i].GetComponent<OneSystemScript>();
                        onsys.isChecked = EditorGUILayout.ToggleLeft(onsys.name, onsys.isChecked);
                        if (onsys.isChecked)
                        {
                            CountSelectedSystems++;
                        }
                    }
                }
            }
            #endregion

            #region Create Satellites
            GUILayout.Box("Add or remove satellites", EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            SystemsSC.indexFromList = EditorGUILayout.Popup(SystemsSC.indexFromList, SystemsSC.NamesSystem);
            if (GUILayout.Button("Add"))
            {
                OneSystemScriptBase subSystem = SystemsSC.SelectedSystem;
                if (subSystem != null)
                {
                    subSystem.LoadMatrix();
                    if (subSystem.SatellitesList.Count == 0)
                    {
                        for (int i = 0; i < subSystem.CountSats; i++)
                        {
                            GameObject sat = Instantiate(subSystem.Satellite, subSystem.transform);

                            SatelliteScript satSC = sat.GetComponent<SatelliteScript>();
                            satSC.Init(subSystem, i);

                            sat.SetActive(false);
                            subSystem.SatellitesList.Add(sat);
                        }
                    }
                }
            }
            if (GUILayout.Button("Remove"))
            {
                OneSystemScriptBase subSystem = SystemsSC.SelectedSystem;
                subSystem.SatellitesList.Clear();
                if (subSystem != null)
                {
                    SatelliteScript[] satScripts = subSystem.GetComponentsInChildren<SatelliteScript>(true);
                    foreach (SatelliteScript satscript in satScripts)
                    {
                        DestroyImmediate(satscript.gameObject);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            OneSystemScriptBase SingleSystem = SystemsSC.SelectedSystem;
            if (SingleSystem != null)
            {
                EditorGUILayout.IntField("Count satellites", SingleSystem.CountSats, EditorStyles.label);

                using (var h = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos2, GUILayout.Width(0), GUILayout.Height(0)))
                    {
                        scrollPos2 = scrollView.scrollPosition;

                        for (int i = 0; i < SingleSystem.CountSats; i++)
                        {
                            if (SingleSystem.SatellitesList.Count != 0)
                            {
                                SatelliteScript SatScript = SingleSystem.SatellitesList[i].GetComponent<SatelliteScript>();
                                SatScript.isChecked = EditorGUILayout.ToggleLeft(SatScript.name, SatScript.isChecked);
                                if (SatScript.isChecked)
                                {
                                    SatScript.gameObject.SetActive(true);
                                }
                                else SatScript.gameObject.SetActive(false);
                            }
                        }
                    }
                }

                if (SingleSystem.SatellitesList.Count != 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Show All"))
                    {
                        for (int i = 0; i < SingleSystem.SatellitesList.Count; i++)
                        {
                            SingleSystem.SatellitesList[i].GetComponent<SatelliteScript>().isChecked = true;
                        }
                    }
                    if (GUILayout.Button("Hide All"))
                    {
                        for (int i = 0; i < SingleSystem.SatellitesList.Count; i++)
                        {
                            SingleSystem.SatellitesList[i].GetComponent<SatelliteScript>().isChecked = false;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            #endregion

            #region Satellites Settings
            //Satellite parameters settings
            EditorGUILayout.LabelField("Satellites parameters settings(PlayMode)", EditorStyles.helpBox);
            OrbitQual_SL.floatValue = EditorGUILayout.Slider("Vizual orbit quality", OrbitQual_SL.floatValue, 1.0f, 10.0f);
            TextSize.intValue = EditorGUILayout.IntSlider("Font Size", TextSize.intValue, 8, 28);
            isShowInfo.boolValue = EditorGUILayout.Toggle("Show satellite info", isShowInfo.boolValue);
            isShowOrbit.boolValue = EditorGUILayout.Toggle("Show satellite orbit", isShowOrbit.boolValue);
            isShowVisor.boolValue = EditorGUILayout.Toggle("Show visor to place", isShowVisor.boolValue);
            #endregion

            #region Update satellites data
            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("Update satellites data", EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                UpdateIndex = EditorGUILayout.Popup(UpdateIndex, SystemsSC.NamesAllSystems);
                if (GUILayout.Button("Update File"))
                {
                    string updateFileName = SystemsSC.NamesAllSystems[UpdateIndex];
                    ins.UpdateFile(updateFileName);
                }
                if (GUILayout.Button("Update All"))
                {
                    foreach (string filename in SystemsSC.NamesAllSystems)
                    {
                        ins.UpdateFile(filename);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Update all satellite positions
        /// </summary>
        private void UpdateAllSats()
        {
            AllSats = ins.GetComponentsInChildren<SatelliteScript>();

            foreach (SatelliteScript sat in AllSats)
            {
                sat.FindNewPosition(DateTime.Parse(InsTime.stringValue));
            }
        }
    }
}

