﻿using UnityEditor;
using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    [CustomEditor(typeof(ProCamera2DCameraWindow))]
    public class ProCamera2DCameraWindowEditor : Editor
    {
        GUIContent _tooltip;

        MonoScript _script;

        void OnEnable()
        {
            var proCamera2DCameraWindow = (ProCamera2DCameraWindow)target;

            if(proCamera2DCameraWindow.ProCamera2D == null && Camera.main != null)
                proCamera2DCameraWindow.ProCamera2D = Camera.main.GetComponent<ProCamera2D>();

            _script = MonoScript.FromMonoBehaviour((ProCamera2DCameraWindow)target);
        }

        public override void OnInspectorGUI()
        {
            var proCamera2DCameraWindow = (ProCamera2DCameraWindow)target;
            if (proCamera2DCameraWindow.ProCamera2D == null)
            {
                EditorGUILayout.HelpBox("ProCamera2D is not set.", MessageType.Error, true);
                return;
            }

            serializedObject.Update();

            // Show script link
            _script = EditorGUILayout.ObjectField("Script", _script, typeof(MonoScript), false) as MonoScript;

            // ProCamera2D
            _tooltip = new GUIContent("Pro Camera 2D", "");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ProCamera2D"), _tooltip);


            string hAxis = "";
            string vAxis = "";
            switch (proCamera2DCameraWindow.ProCamera2D.Axis)
            {
                case MovementAxis.XY:
                    hAxis = "X";
                    vAxis = "Y";
                    break;

                case MovementAxis.XZ:
                    hAxis = "X";
                    vAxis = "Z";
                    break;

                case MovementAxis.YZ:
                    hAxis = "Y";
                    vAxis = "Z";
                    break;
            }

            _tooltip = new GUIContent("Width", "Window width");
            EditorGUILayout.Slider(serializedObject.FindProperty("CameraWindowRect.width"), 0f, 1f, _tooltip);

            _tooltip = new GUIContent("Height", "Window height");
            EditorGUILayout.Slider(serializedObject.FindProperty("CameraWindowRect.height"), 0f, 1f, _tooltip);

            _tooltip = new GUIContent(hAxis, "Window horizontal offset");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraWindowRect.x"), _tooltip);

            _tooltip = new GUIContent(vAxis, "Window vertical offset");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CameraWindowRect.y"), _tooltip);

            serializedObject.ApplyModifiedProperties();
        }
    }
}