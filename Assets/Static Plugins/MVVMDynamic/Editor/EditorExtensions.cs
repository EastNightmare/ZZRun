using UnityEditor;
using UnityEngine;

namespace MVVMDynamic.Editors
{
    public static class EditorExtensions
    {
        private const string thread = "http://edentec.org/unity/mvvm-dynamic/thread";
        private const string docs = "http://edentec.org//unity/mvvm-dynamic/docs";
        
        [MenuItem("Tools/MVVMDynamic/Unity forums thread")]
        private static void OpenThread()
        {
            Help.BrowseURL(thread);
        }

        [MenuItem("Tools/MVVMDynamic/Docs")]
        private static void OpenDocs()
        {
            Help.BrowseURL(docs);
        }
    }
}