using System;
using System.Collections.Generic;
using UnityEngine;

namespace HollowManor
{
    public static class ExternalAssetCatalog
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

        public sealed class Entry
        {
            public string Id { get; private set; }
            public string Label { get; private set; }
            public string ResourcePath { get; private set; }
            public ExternalAssetPrefabAdapter.AssetRole Role { get; private set; }
            public Vector3 DefaultScale { get; private set; }

            public Entry(string id, string label, string resourcePath, ExternalAssetPrefabAdapter.AssetRole role, Vector3 defaultScale)
            {
                Id = id;
                Label = label;
                ResourcePath = resourcePath;
                Role = role;
                DefaultScale = defaultScale == Vector3.zero ? Vector3.one : defaultScale;
            }
        }

        private const string CatalogResourcePath = "Generated/ExternalAssetCatalog";

        private static readonly List<Entry> entries = new List<Entry>();
        private static readonly Dictionary<string, Entry> byId = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        private static bool loaded;

        public static IReadOnlyList<Entry> GetEntries()
        {
            EnsureLoaded();
            return entries;
        }

        public static void Refresh()
        {
            loaded = false;
            entries.Clear();
            byId.Clear();
            EnsureLoaded();
        }

        public static bool TryGetEntry(string assetId, out Entry entry)
        {
            EnsureLoaded();
            if (string.IsNullOrWhiteSpace(assetId))
            {
                entry = null;
                return false;
            }

            return byId.TryGetValue(assetId, out entry);
        }

        public static bool TryGetFirstByRole(ExternalAssetPrefabAdapter.AssetRole role, out Entry entry)
        {
            EnsureLoaded();
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Role == role)
                {
                    entry = entries[i];
                    return true;
                }
            }

            entry = null;
            return false;
        }

        public static bool TryGetFirstByKeyword(string keyword, out Entry entry)
        {
            EnsureLoaded();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                entry = null;
                return false;
            }

            string lowered = keyword.Trim().ToLowerInvariant();
            for (int i = 0; i < entries.Count; i++)
            {
                Entry candidate = entries[i];
                if (candidate.Id.ToLowerInvariant().Contains(lowered) || candidate.Label.ToLowerInvariant().Contains(lowered) || candidate.ResourcePath.ToLowerInvariant().Contains(lowered))
                {
                    entry = candidate;
                    return true;
                }
            }

            entry = null;
            return false;
        }

        public static GameObject CreateInstance(string assetId, string objectNameOverride = null, Transform parent = null)
        {
            if (!TryGetEntry(assetId, out Entry entry))
            {
                return null;
            }

            return CreateInstance(entry, objectNameOverride, parent);
        }

        public static GameObject CreateInstance(Entry entry, string objectNameOverride = null, Transform parent = null)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.ResourcePath))
            {
                return null;
            }

            GameObject prefab = Resources.Load<GameObject>(entry.ResourcePath);
            if (prefab == null)
            {
                return null;
            }

            GameObject instance = parent != null ? UnityEngine.Object.Instantiate(prefab, parent) : UnityEngine.Object.Instantiate(prefab);
            instance.name = string.IsNullOrWhiteSpace(objectNameOverride) ? prefab.name : objectNameOverride;

            ExternalAssetPrefabAdapter adapter = instance.GetComponent<ExternalAssetPrefabAdapter>();
            if (adapter == null)
            {
                adapter = instance.AddComponent<ExternalAssetPrefabAdapter>();
            }

            adapter.role = entry.Role;
            adapter.ApplyNow();
            return instance;
        }

        private static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }

            loaded = true;
            entries.Clear();
            byId.Clear();

            TextAsset catalogText = Resources.Load<TextAsset>(CatalogResourcePath);
            if (catalogText != null && !string.IsNullOrWhiteSpace(catalogText.text))
            {
                CatalogFile data = JsonUtility.FromJson<CatalogFile>(catalogText.text);
                if (data != null && data.assets != null)
                {
                    for (int i = 0; i < data.assets.Count; i++)
                    {
                        CatalogItem item = data.assets[i];
                        if (item == null || string.IsNullOrWhiteSpace(item.id) || string.IsNullOrWhiteSpace(item.resourcePath))
                        {
                            continue;
                        }

                        AddEntry(new Entry(
                            item.id.Trim(),
                            string.IsNullOrWhiteSpace(item.label) ? item.id.Trim() : item.label.Trim(),
                            item.resourcePath.Trim(),
                            ParseRole(item.role),
                            item.defaultScale));
                    }
                }
            }

        }

        private static void AddEntry(Entry entry)
        {
            if (entry == null || byId.ContainsKey(entry.Id))
            {
                return;
            }

            entries.Add(entry);
            byId.Add(entry.Id, entry);
        }

        private static ExternalAssetPrefabAdapter.AssetRole ParseRole(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return ExternalAssetPrefabAdapter.AssetRole.GeneralObstacle;
            }

            if (Enum.TryParse(value.Trim(), true, out ExternalAssetPrefabAdapter.AssetRole parsed))
            {
                return parsed;
            }

            string lowered = value.Trim().ToLowerInvariant();
            if (lowered.Contains("tree")) return ExternalAssetPrefabAdapter.AssetRole.Tree;
            if (lowered.Contains("ghost")) return ExternalAssetPrefabAdapter.AssetRole.Ghost;
            if (lowered.Contains("cabin") || lowered.Contains("house") || lowered.Contains("hut") || lowered.Contains("shack")) return ExternalAssetPrefabAdapter.AssetRole.Cabin;
            if (lowered.Contains("car") || lowered.Contains("vehicle")) return ExternalAssetPrefabAdapter.AssetRole.Car;
            return ExternalAssetPrefabAdapter.AssetRole.GeneralObstacle;
        }

    }
}
