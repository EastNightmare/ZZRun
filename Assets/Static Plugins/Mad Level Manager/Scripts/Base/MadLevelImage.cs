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

[RequireComponent(typeof(MadSprite))]
public class MadLevelImage : MonoBehaviour {

    #region Public Fields

    public List<LevelTexture> levelTextures = new List<LevelTexture>();

    #endregion

    #region Private Fields

    private MadLevelIcon icon;
    private MadSprite sprite;

    #endregion

    #region Slots

    void Start() {
        icon = MadTransform.FindParent<MadLevelIcon>(transform);
        sprite = GetComponent<MadSprite>();
        if (icon != null) {
            AssignTexture();
        } else {
            Debug.LogError("MadLevelImage may be set only as a MadLevelIcon child");
        }
    }

    #endregion

    #region Private Methods

    private void AssignTexture() {
        var index = icon.levelIndex;
        if (index < levelTextures.Count) {
            var texture = levelTextures[index];
            if (texture.image != null) {
                sprite.texture = texture.image;
            } else {
                Debug.LogWarning("Image for level " + (index + 1) + " not assinged");
            }
        } else {
            Debug.LogWarning("Image for level " + (index + 1) + " not assinged");
        }
    }

    #endregion

    #region Inner and Anonymous Classes

    [Serializable]
    public class LevelTexture {
        public Texture2D image;
    }

    #endregion
}

#if !UNITY_3_5
} // namespace
#endif