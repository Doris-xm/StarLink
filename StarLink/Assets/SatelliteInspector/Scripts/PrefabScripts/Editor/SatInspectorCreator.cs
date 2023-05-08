using UnityEditor;
using UnityEngine;

namespace SatelliteInspector.Scripts.PrefabScripts.Editor
{
    public class SatInspectorCreator : UnityEditor.Editor
    {
        [MenuItem("Tools/Satellites inspector/Satellite Inspector 3D")]
        private static void CreateSatelliteInspector3D()
        {
            GameObject satins = Instantiate((GameObject)Resources.Load("Prefabs/SatInspector"));
            satins.name = "SatellitesInspector3D";
        }

        [MenuItem("Tools/Satellites inspector/Satellite Inspector 2D")]
        private static void CreateSatelliteInspector2D()
        {
            GameObject satins = Instantiate((GameObject)Resources.Load("Prefabs/SatInspector2D"));
            satins.name = "SatellitesInspector2D";
        }
    }
}
