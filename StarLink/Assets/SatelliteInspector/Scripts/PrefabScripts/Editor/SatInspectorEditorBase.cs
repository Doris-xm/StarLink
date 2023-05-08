using System;
using SatelliteInspector.Scripts.SolutionScripts;
using UnityEditor;
using UnityEngine;

namespace SatelliteInspector.Scripts.PrefabScripts.Editor
{
    public abstract class SatInspectorEditorBase : UnityEditor.Editor
    {
        protected SatInsBase ins;
        protected SerializedProperty InsTime;
        protected SerializedProperty TimeFactor;
        protected SerializedProperty OrbitQual_SL;
        protected SerializedProperty isShowInfo;
        protected SerializedProperty isShowOrbit;
        protected SerializedProperty isShowVisor;
        protected SerializedProperty TextSize;
        protected Vector2 scrollPos;
        protected Vector2 scrollPos2;
        protected int CountSelectedSystems;
        protected int UpdateIndex;
        protected bool FadeTime;

        protected void SetProperies()
        {
            InsTime = serializedObject.FindProperty(nameof(ins.InsTime));
            TimeFactor = serializedObject.FindProperty(nameof(ins.Timefactor));
            OrbitQual_SL = serializedObject.FindProperty(nameof(ins.OrbitQual_SL));
            isShowInfo = serializedObject.FindProperty(nameof(ins.isShowInfo));
            isShowOrbit = serializedObject.FindProperty(nameof(ins.isShowOrbit));
            isShowVisor = serializedObject.FindProperty(nameof(ins.isShowVisor));
            TextSize = serializedObject.FindProperty(nameof(ins.TextSize));
        }

        protected void SetComponents()
        {
            if (ins is null) return;
            ins.InsTime = DateTime.UtcNow.ToString("o");
            
            ins.JTimeIns = CreateInstance<JTime>();
            ins.JTimeIns.InitAtTime(DateTime.Parse(InsTime.stringValue));
        }

        public virtual void OnEnable()
        {
            SetProperies();
            SetComponents();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            #region Set time
            GUILayout.Box("Simulation time settings", EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();
            InsTime.stringValue = EditorGUILayout.TextField("Time (full format)", InsTime.stringValue);
            EditorGUILayout.LabelField("Time (standart format)", DateTime.Parse(InsTime.stringValue).ToString());
            ins.JTimeIns.InitAtTime(DateTime.Parse(InsTime.stringValue));
            #endregion
            
            
        }
    }
}