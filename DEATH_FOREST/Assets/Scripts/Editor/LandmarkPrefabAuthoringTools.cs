#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HollowManor.EditorTools
{
    public static class LandmarkPrefabAuthoringTools
    {
        private const string OutputDirectory = "Assets/Resources/Generated/Landmarks";

        [MenuItem("Death Forest/Landmarks/Generate Authored Prefabs From Legacy Layout")]
        public static void GenerateAuthoredPrefabsFromLegacyLayout()
        {
            Directory.CreateDirectory(OutputDirectory);

            IReadOnlyList<string> landmarkIds = LevelFactory.GetLegacyLandmarkIdsForAuthoring();
            int perLandmarkCount = 0;

            for (int i = 0; i < landmarkIds.Count; i++)
            {
                string landmarkId = landmarkIds[i];
                GameObject root = LevelFactory.CreateLegacyLandmarkAuthoringRoot(landmarkId);
                if (root == null)
                {
                    continue;
                }

                try
                {
                    string prefabPath = Path.Combine(OutputDirectory, landmarkId + ".prefab").Replace('\\', '/');
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                    perLandmarkCount++;
                }
                finally
                {
                    Object.DestroyImmediate(root);
                }
            }

            GameObject authoredSet = LevelFactory.CreateLegacyLandmarkSetAuthoringRoot();
            try
            {
                PrefabUtility.SaveAsPrefabAsset(authoredSet, LevelFactory.AuthoredLandmarkSetPrefabAssetPath);
            }
            finally
            {
                Object.DestroyImmediate(authoredSet);
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Death Forest",
                "Da generate " + perLandmarkCount + " landmark prefab rieng le va 1 landmark set prefab tong hop.\n\nRuntime LevelFactory se uu tien nap prefab authored tu Resources/Generated/Landmarks thay vi dung landmark procedural.",
                "OK");
        }

        [MenuItem("Death Forest/Landmarks/Spawn Authored Landmark Set In Current Scene")]
        public static void SpawnAuthoredLandmarkSetInCurrentScene()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelFactory.AuthoredLandmarkSetPrefabAssetPath);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog(
                    "Death Forest",
                    "Chua co landmark set prefab. Hay chay menu Generate Authored Prefabs From Legacy Layout truoc.",
                    "OK");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                EditorUtility.DisplayDialog("Death Forest", "Khong instantiate duoc landmark set prefab.", "OK");
                return;
            }

            instance.name = "DeathForestLandmarkSet_Authoring";
            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(instance);
        }

        [MenuItem("Death Forest/Landmarks/Save Selected Root As Authored Landmark Set")]
        public static void SaveSelectedRootAsAuthoredLandmarkSet()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Death Forest", "Hay chon root landmark set trong Hierarchy truoc khi save.", "OK");
                return;
            }

            Directory.CreateDirectory(OutputDirectory);
            PrefabUtility.SaveAsPrefabAsset(selected, LevelFactory.AuthoredLandmarkSetPrefabAssetPath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Death Forest",
                "Da save root duoc chon thanh authored landmark set:\n" + LevelFactory.AuthoredLandmarkSetPrefabAssetPath,
                "OK");
        }
    }
}
#endif
