#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HollowManor.EditorTools
{
    [InitializeOnLoad]
    public static class LandmarkPrefabBootstrap
    {
        private static bool hasQueuedEnsure;

        static LandmarkPrefabBootstrap()
        {
            QueueEnsure();
        }

        [MenuItem("Death Forest/Landmarks/Rebuild Default Authored Landmark Prefab")]
        public static void RebuildDefaultAuthoredLandmarkPrefab()
        {
            EnsureDefaultAuthoredLandmarkPrefab(forceRebuild: true, interactive: true);
        }

        private static void QueueEnsure()
        {
            if (hasQueuedEnsure)
            {
                return;
            }

            hasQueuedEnsure = true;
            EditorApplication.delayCall += () =>
            {
                hasQueuedEnsure = false;
                EnsureDefaultAuthoredLandmarkPrefab(forceRebuild: false, interactive: false);
            };
        }

        private static void EnsureDefaultAuthoredLandmarkPrefab(bool forceRebuild, bool interactive)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                QueueEnsure();
                return;
            }

            string prefabPath = LevelFactory.AuthoredLandmarkSetPrefabAssetPath;
            if (!forceRebuild && AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(prefabPath) ?? "Assets/Resources/Generated/Landmarks");

            GameObject root = LevelFactory.CreateLegacyLandmarkSetAuthoringRoot();
            if (root == null)
            {
                Debug.LogError("[Death Forest] Khong tao duoc root authored landmark mac dinh.");
                return;
            }

            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();

                if (interactive)
                {
                    EditorUtility.DisplayDialog(
                        "Death Forest",
                        "Da rebuild authored landmark prefab mac dinh:\n" + prefabPath,
                        "OK");
                }
                else
                {
                    Debug.Log("[Death Forest] Da tao authored landmark prefab mac dinh tai " + prefabPath);
                }
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
#endif
