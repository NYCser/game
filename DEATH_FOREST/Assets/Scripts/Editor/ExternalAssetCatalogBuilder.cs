#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HollowManor.EditorTools
{
    public static class ExternalAssetCatalogBuilder
    {
        [Serializable]
        private sealed class CatalogFile
        {
            public List<CatalogItem> assets = new List<CatalogItem>();
        }

        [Serializable]
        private sealed class CatalogItem
        {
            public string id = string.Empty;
            public string label = string.Empty;
            public string resourcePath = string.Empty;
            public string role = string.Empty;
            public Vector3 defaultScale = Vector3.one;
        }

        private const string SearchRoot = "Assets/Resources/ExternalAssets";
        private const string OutputPath = "Assets/Resources/Generated/ExternalAssetCatalog.json";

        [MenuItem("Death Forest/Rebuild External Asset Catalog")]
        public static void RebuildCatalog()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath) ?? "Assets/Resources/Generated");

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { SearchRoot });
            Array.Sort(guids, StringComparer.Ordinal);

            CatalogFile file = new CatalogFile();
            HashSet<string> usedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    continue;
                }

                if (!TryGetResourcePath(assetPath, out string resourcePath))
                {
                    continue;
                }

                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                string id = MakeUniqueId(Slugify(fileName), usedIds);
                file.assets.Add(new CatalogItem
                {
                    id = id,
                    label = ObjectNames.NicifyVariableName(fileName),
                    resourcePath = resourcePath,
                    role = InferRole(fileName),
                    defaultScale = Vector3.one
                });
            }

            string json = JsonUtility.ToJson(file, true);
            File.WriteAllText(OutputPath, json);
            AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Death Forest",
                "Da quet " + file.assets.Count + " prefab trong Assets/Resources/ExternalAssets va tao catalog runtime.",
                "OK");
        }

        private static bool TryGetResourcePath(string assetPath, out string resourcePath)
        {
            const string marker = "Assets/Resources/";
            int index = assetPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                resourcePath = string.Empty;
                return false;
            }

            string relative = assetPath.Substring(index + marker.Length);
            resourcePath = Path.ChangeExtension(relative, null)?.Replace('\\', '/');
            return !string.IsNullOrWhiteSpace(resourcePath);
        }

        private static string InferRole(string fileName)
        {
            string lowered = fileName.ToLowerInvariant();
            if (lowered.Contains("ghost") || lowered.Contains("spirit") || lowered.Contains("monster")) return ExternalAssetPrefabAdapter.AssetRole.Ghost.ToString();
            if (lowered.Contains("tree") || lowered.Contains("pine") || lowered.Contains("oak") || lowered.Contains("bush") || lowered.Contains("foliage")) return ExternalAssetPrefabAdapter.AssetRole.Tree.ToString();
            if (lowered.Contains("cabin") || lowered.Contains("hut") || lowered.Contains("house") || lowered.Contains("shack")) return ExternalAssetPrefabAdapter.AssetRole.Cabin.ToString();
            if (lowered.Contains("car") || lowered.Contains("truck") || lowered.Contains("vehicle")) return ExternalAssetPrefabAdapter.AssetRole.Car.ToString();
            return ExternalAssetPrefabAdapter.AssetRole.GeneralObstacle.ToString();
        }

        private static string Slugify(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "asset";
            }

            char[] chars = value.Trim().ToLowerInvariant().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if ((chars[i] < 'a' || chars[i] > 'z') && (chars[i] < '0' || chars[i] > '9'))
                {
                    chars[i] = '_';
                }
            }

            string result = new string(chars);
            while (result.Contains("__"))
            {
                result = result.Replace("__", "_");
            }

            return result.Trim('_');
        }

        private static string MakeUniqueId(string baseId, HashSet<string> usedIds)
        {
            string candidate = string.IsNullOrWhiteSpace(baseId) ? "asset" : baseId;
            int counter = 2;
            while (!usedIds.Add(candidate))
            {
                candidate = baseId + "_" + counter;
                counter++;
            }

            return candidate;
        }
    }
}
#endif
