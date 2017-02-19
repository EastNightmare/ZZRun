/*
* Mad Level Manager by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using MadLevelManager;

#if !UNITY_3_5
namespace MadLevelManager {
#endif

[CustomEditor(typeof(MadDragStopDraggable))]
public class MadDragStopDraggableInspector : Editor {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    SerializedProperty moveEasingType;
    SerializedProperty moveEasingDuration;

    SerializedProperty swipeVirtualDistanceModifier;
    SerializedProperty limitSwipeToSinglePage;
    SerializedProperty switchAfterDistance;
    
    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================

    // ===========================================================
    // Methods
    // ===========================================================

    void OnEnable() {
        moveEasingType = serializedObject.FindProperty("moveEasingType");
        moveEasingDuration = serializedObject.FindProperty("moveEasingDuration");

        swipeVirtualDistanceModifier = serializedObject.FindProperty("swipeVirtualDistanceModifier");
        limitSwipeToSinglePage = serializedObject.FindProperty("limitSwipeToSinglePage");
        switchAfterDistance = serializedObject.FindProperty("switchAfterDistance");
    }

    public override void OnInspectorGUI() {
        serializedObject.UpdateIfDirtyOrScript();
    
        MadGUI.PropertyField(moveEasingType, "Type");
        MadGUI.PropertyField(moveEasingDuration, "Duration");

        EditorGUILayout.Space();

        GUILayout.Label("Swipe", "HeaderLabel");
        using (MadGUI.Indent()) {
            MadGUI.PropertyField(swipeVirtualDistanceModifier, "Virtual Distance Modifier");
            MadGUI.PropertyField(limitSwipeToSinglePage, "Limit To A Single Page");
            MadGUI.PropertyField(switchAfterDistance, "Switch Page After Distance");
        }

        serializedObject.ApplyModifiedProperties();
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

#if !UNITY_3_5
} // namespace
#endif