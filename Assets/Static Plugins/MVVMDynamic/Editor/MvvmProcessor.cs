using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Security.Cryptography;
using Debug = UnityEngine.Debug;

namespace MVVMDynamic.Editors
{
    [InitializeOnLoad]
    public class MvvmProcessor
    {
        static MvvmProcessor()
        {
            EditorApplication.update += Update;
        }

        private static bool _startExecuted = false;

        private static void Update()
        {
            if (!_startExecuted)
            {
                _startExecuted = true;
                Start();
            }
        }

        private static void Start()
        {
#if UNITY_EDITOR && (UNITY_IOS || UNITY_WEBGL)
            CheckAndRegenerate();
#endif
        }

        [Conditional("UNITY_EDITOR")]
        private static void CheckAndRegenerate()
        {
            try
            {
                var scripts = MonoImporter.GetAllRuntimeMonoScripts().ToList();
                var vmScripts = scripts.FindAll(p => TypeEmitter.Instance.ViewModelInterfaces.Contains(p.GetClass()));

                MD5 md5 = MD5.Create();
                byte[] bytes = vmScripts
                    .Select(p => AssetDatabase.GetAssetPath(p))
                    .Select(p => File.GetLastWriteTimeUtc(p))
                    .Select(p => p.Ticks)
                    .SelectMany(p => BitConverter.GetBytes(p))
                    .ToArray();

                byte[] hash = md5.ComputeHash(bytes);
                var newHashString = Convert.ToBase64String(hash);
                var oldHashString = EditorPrefs.GetString("MVVMD_hash", null);
                EditorPrefs.SetString("MVVMD_hash", newHashString);

                if (oldHashString != newHashString)
                {
                    Debug.Log("MVVMDynamic: View models have changed. Regenerating sources...");
                    TypeEmitter.Instance.GenerateSources();
                    AssetDatabase.Refresh();
                    return;
                }

                bool allVmHaveSources = vmScripts
                    .Select(p => p.GetClass())
                    .Select(p => TypeEmitter.GetDynamicallyGeneratedTypeName(p))
                    .All(p => scripts.Exists(s => s.name == p));
                if (!allVmHaveSources)
                {
                    Debug.Log("MVVMDynamic: View models sources not found. Regenerating sources...");
                    TypeEmitter.Instance.GenerateSources();
                    AssetDatabase.Refresh();
                    return;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("MVVMDynamic: Source generation failed");
                Debug.LogException(exception);
            }
        }

        [MenuItem("Tools/MVVMDynamic/Generate sources")]
        public static void ForceGenerateSources()
        {
            try
            {
                TypeEmitter.Instance.GenerateSources();
                AssetDatabase.Refresh();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("MVVMDynamic: Source generation failed");
                Debug.Log(exception);
            }
        }
    }
}