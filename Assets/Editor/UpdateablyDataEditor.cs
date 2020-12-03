using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(UpdateableData), true)]
public class UpdateablyDataEditor : Editor
{

    #if UNITY_EDITOR

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        UpdateableData data = (UpdateableData)target;

        if (GUILayout.Button ("Update")) {
            data.NotifyOfUpdateValues();
            EditorUtility.SetDirty (target);
        }
    }

    #endif
}
