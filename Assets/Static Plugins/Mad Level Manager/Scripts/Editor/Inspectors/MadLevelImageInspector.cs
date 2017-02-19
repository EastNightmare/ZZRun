/*
* Copyright (c) Mad Pixel Machine
* http://www.madpixelmachine.com/
*/

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using MadLevelManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if !UNITY_3_5
namespace MadLevelManager {
#endif

[CustomEditor(typeof(MadLevelImage))]
public class MadLevelImageInspector : Editor {

    #region Fields

    private MadLevelImage script;

    #endregion

    #region Methods

    void OnEnable() {
        script = target as MadLevelImage;
    }

    public override void OnInspectorGUI() {

        int number = 1;

        var list = new MadGUI.ArrayList<MadLevelImage.LevelTexture>(script.levelTextures, texture => {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Level " + number++ + ".");
            GUILayout.FlexibleSpace();
            texture.image = (Texture2D) EditorGUILayout.ObjectField("", texture.image, typeof (Texture2D), false);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            return texture;
        });
        if (list.Draw()) {
            EditorUtility.SetDirty(script);
        }
    }

    #endregion
}

#if !UNITY_3_5
} // namespace
#endif