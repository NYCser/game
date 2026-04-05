#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HollowManor.EditorTools
{
    public static class ExternalEnvironmentBaker
    {
        [Serializable]
        private sealed class LayoutData
        {
            public List<PlacedObjectData> objects = new List<PlacedObjectData>();
        }

        [Serializable]
        private sealed class PlacedObjectData
        {
            public string assetId = string.Empty;
            public Vector3 position;
            public Vector3 eulerAngles;
            public Vector3 localScale = Vector3.one;
        }

        private const string LayoutFileName = "death_forest_runtime_object_layout.json";
        private const string OutputPrefabPath = "Assets/Resources/Generated/BakedExternalEnvironment.prefab";

        [MenuItem("Death Forest/Bake Runtime Layout To Prefab")]
        public static void BakeRuntimeLayoutToPrefab()
        {
            string layoutPath = Path.Combine(Application.persistentDataPath, LayoutFileName);
            if (!File.Exists(layoutPath))
            {
                EditorUtility.DisplayDialog(
                    "Death Forest",
                    "Khong tim thay layout runtime tai:\n" + layoutPath + "\n\nHay vao Play, dat asset bang editor, Save Layout roi chay lenh nay.",
                    "OK");
                return;
            }

            ExternalAssetCatalog.Refresh();
            LayoutData data = JsonUtility.FromJson<LayoutData>(File.ReadAllText(layoutPath));
            if (data == null || data.objects == null || data.objects.Count == 0)
            {
                EditorUtility.DisplayDialog("Death Forest", "Layout runtime rong hoac khong hop le.", "OK");
                return;
            }

            GameObject root = new GameObject("BakedExternalEnvironment");
            int bakedCount = 0;

            try
            {
                for (int i = 0; i < data.objects.Count; i++)
                {
                    PlacedObjectData item = data.objects[i];
                    GameObject instance = ExternalAssetCatalog.CreateInstance(item.assetId, null, root.transform);
                    if (instance == null)
                    {
                        continue;
                    }

                    instance.transform.localPosition = item.position;
                    instance.transform.localRotation = Quaternion.Euler(item.eulerAngles);
                    instance.transform.localScale = item.localScale;
                    if (instance.GetComponent<RuntimeEditorPlacedObject>() != null)
                    {
                        UnityEngine.Object.DestroyImmediate(instance.GetComponent<RuntimeEditorPlacedObject>());
                    }
                    bakedCount++;
                }

                if (bakedCount == 0)
                {
                    EditorUtility.DisplayDialog("Death Forest", "Khong bake duoc asset nao. Kiem tra catalog va prefab ngoai.", "OK");
                    return;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(OutputPrefabPath) ?? "Assets/Resources/Generated");
                PrefabUtility.SaveAsPrefabAsset(root, OutputPrefabPath);
                AssetDatabase.ImportAsset(OutputPrefabPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "Death Forest",
                    "Da bake " + bakedCount + " asset vao prefab:\n" + OutputPrefabPath + "\n\nLuc Play, LevelFactory se tu nap prefab nay neu co.",
                    "OK");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
#endif
