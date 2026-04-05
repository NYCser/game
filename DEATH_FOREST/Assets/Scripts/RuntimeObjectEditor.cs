using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HollowManor
{
    public sealed class RuntimeObjectEditor : MonoBehaviour
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

        private sealed class CatalogEntry
        {
            public string id;
            public string label;
            public bool isExternal;

            public CatalogEntry(string id, string label, bool isExternal)
            {
                this.id = id;
                this.label = label;
                this.isExternal = isExternal;
            }
        }

        private const string LayoutFileName = "death_forest_runtime_object_layout.json";

        private readonly List<CatalogEntry> catalog = new List<CatalogEntry>();
        private Rect windowRect = new Rect(18f, 18f, 430f, 690f);
        private Vector2 assetScroll;
        private Transform placedRoot;
        private RuntimeEditorPlacedObject selectedObject;
        private string selectedAssetId = string.Empty;
        private bool editorVisible;
        private float positionStep = 0.5f;
        private float rotationStep = 15f;
        private float scaleStep = 0.1f;
        private string statusMessage = "Nhan F2 de mo Asset Editor.";
        private float statusTimer;
        private string layoutPath = string.Empty;

        private void Start()
        {
            layoutPath = Path.Combine(Application.persistentDataPath, LayoutFileName);
            EnsurePlacedRoot();
            BuildCatalog();
            TryLoadLayout(true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                SetEditorVisible(!editorVisible);
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveLayout();
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                ReloadLayout();
            }

            if (statusTimer > 0f)
            {
                statusTimer = Mathf.Max(0f, statusTimer - Time.unscaledDeltaTime);
            }

            if (!editorVisible)
            {
                return;
            }

            HandleEditorShortcuts();
        }

        private void OnGUI()
        {
            GUI.depth = -100;

            if (editorVisible)
            {
                windowRect = GUI.Window(927451, windowRect, DrawWindow, "ASSET / OBJECT EDITOR");
            }
            else
            {
                DrawCollapsedHint();
            }
        }

        private void DrawCollapsedHint()
        {
            GUILayout.BeginArea(new Rect(14f, 14f, 320f, 56f), GUI.skin.box);
            GUILayout.Label("F2: mo Asset Editor | F5: save layout | F9: reload layout");
            GUILayout.EndArea();
        }

        private void DrawWindow(int windowId)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("- Click trai: dat object tai diem dang tro");
            GUILayout.Label("- Click phai: chon object da them");
            GUILayout.Label("- Mui ten/PageUp/PageDown: di chuyen | Q/E: xoay | Z/X: phong-thu | Delete: xoa");
            GUILayout.Space(6f);
            GUILayout.Label("File luu layout:");
            GUILayout.TextField(layoutPath);

            if (!string.IsNullOrWhiteSpace(statusMessage))
            {
                GUILayout.Space(4f);
                GUILayout.Label(statusMessage);
            }

            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Layout", GUILayout.Height(28f)))
            {
                SaveLayout();
            }
            if (GUILayout.Button("Reload Layout", GUILayout.Height(28f)))
            {
                ReloadLayout();
            }
            if (GUILayout.Button("Refresh Catalog", GUILayout.Height(28f)))
            {
                BuildCatalog();
                SetStatus("Da refresh catalog asset ngoai.", 1.5f);
            }
            if (GUILayout.Button("Clear All", GUILayout.Height(28f)))
            {
                ClearPlacedObjects();
                SetStatus("Da xoa tat ca object editor. Bam Save de ghi de layout.", 3.2f);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);
            GUILayout.Label("Buoc di chuyen: " + positionStep.ToString("0.00") + " | Xoay: " + rotationStep.ToString("0") + " | Scale: " + scaleStep.ToString("0.00"));
            positionStep = GUILayout.HorizontalSlider(positionStep, 0.1f, 2f);
            rotationStep = GUILayout.HorizontalSlider(rotationStep, 5f, 90f);
            scaleStep = GUILayout.HorizontalSlider(scaleStep, 0.05f, 0.5f);

            GUILayout.Space(8f);
            GUILayout.Label("Danh sach asset de them:");
            assetScroll = GUILayout.BeginScrollView(assetScroll, GUI.skin.box, GUILayout.Height(285f));
            for (int i = 0; i < catalog.Count; i++)
            {
                CatalogEntry entry = catalog[i];
                string buttonLabel = (selectedAssetId == entry.id ? "> " : string.Empty) + (entry.isExternal ? "[EXT] " : "[BUILT-IN] ") + entry.label;
                if (GUILayout.Button(buttonLabel, GUILayout.Height(28f)))
                {
                    selectedAssetId = entry.id;
                    SetStatus("Dang chon asset: " + entry.label, 1.8f);
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Space(8f);
            GUILayout.Label("Asset dang chon: " + GetLabelForAsset(selectedAssetId));

            if (selectedObject != null)
            {
                Transform t = selectedObject.transform;
                GUILayout.Space(8f);
                GUILayout.Label("Object dang chon:");
                GUILayout.TextArea(
                    "Ten: " + t.name + "\n" +
                    "Asset: " + GetLabelForAsset(selectedObject.assetId) + "\n" +
                    "Pos: " + FormatVector(t.position) + "\n" +
                    "Rot: " + FormatVector(t.eulerAngles) + "\n" +
                    "Scale: " + FormatVector(t.localScale),
                    GUILayout.Height(96f));

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Duplicate", GUILayout.Height(28f)))
                {
                    DuplicateSelected();
                }
                if (GUILayout.Button("Snap Ground", GUILayout.Height(28f)))
                {
                    SnapSelectedToGround();
                }
                if (GUILayout.Button("Delete", GUILayout.Height(28f)))
                {
                    DeleteSelected();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Space(8f);
                GUILayout.Label("Chua chon object nao. Click phai vao object da dat de chon.");
            }

            GUILayout.Space(8f);
            if (GUILayout.Button("Dong Editor (F2)", GUILayout.Height(28f)))
            {
                SetEditorVisible(false);
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0f, 0f, 9999f, 24f));
        }

        private void HandleEditorShortcuts()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetEditorVisible(false);
                return;
            }

            if (IsPointerOverEditorWindow())
            {
                return;
            }

            if (Input.GetMouseButtonDown(1))
            {
                TrySelectObjectFromCursor();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                TryPlaceSelectedAsset();
            }

            if (selectedObject == null)
            {
                return;
            }

            Transform target = selectedObject.transform;
            bool changed = false;
            Vector3 move = Vector3.zero;

            if (Input.GetKeyDown(KeyCode.UpArrow)) move += new Vector3(0f, 0f, positionStep);
            if (Input.GetKeyDown(KeyCode.DownArrow)) move += new Vector3(0f, 0f, -positionStep);
            if (Input.GetKeyDown(KeyCode.LeftArrow)) move += new Vector3(-positionStep, 0f, 0f);
            if (Input.GetKeyDown(KeyCode.RightArrow)) move += new Vector3(positionStep, 0f, 0f);
            if (Input.GetKeyDown(KeyCode.PageUp)) move += new Vector3(0f, positionStep, 0f);
            if (Input.GetKeyDown(KeyCode.PageDown)) move += new Vector3(0f, -positionStep, 0f);

            if (move != Vector3.zero)
            {
                target.position += move;
                changed = true;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                target.Rotate(Vector3.up, -rotationStep, Space.World);
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                target.Rotate(Vector3.up, rotationStep, Space.World);
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                target.localScale = ClampScale(target.localScale - Vector3.one * scaleStep);
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                target.localScale = ClampScale(target.localScale + Vector3.one * scaleStep);
                changed = true;
            }
            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                DeleteSelected();
                return;
            }
            if (Input.GetKeyDown(KeyCode.D) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                DuplicateSelected();
                return;
            }

            if (changed)
            {
                SetStatus("Da cap nhat transform cho object dang chon.", 0.75f);
            }
        }

        private void BuildCatalog()
        {
            catalog.Clear();
            ExternalAssetCatalog.Refresh();

            var entries = ExternalAssetCatalog.GetEntries();
            for (int i = 0; i < entries.Count; i++)
            {
                catalog.Add(new CatalogEntry(entries[i].Id, entries[i].Label, true));
            }

            if (catalog.Count == 0)
            {
                SetStatus("Catalog rong. Import prefab vao Assets/Resources/ExternalAssets va chay menu Rebuild External Asset Catalog.", 3.5f);
            }

            bool selectionStillExists = false;
            for (int i = 0; i < catalog.Count; i++)
            {
                if (string.Equals(catalog[i].id, selectedAssetId, StringComparison.OrdinalIgnoreCase))
                {
                    selectionStillExists = true;
                    break;
                }
            }

            if (!selectionStillExists && catalog.Count > 0)
            {
                selectedAssetId = catalog[0].id;
            }
        }

        private void EnsurePlacedRoot()
        {
            if (placedRoot != null)
            {
                return;
            }

            Transform existing = transform.Find("RuntimeEditorPlacedObjects");
            if (existing != null)
            {
                placedRoot = existing;
                return;
            }

            GameObject root = new GameObject("RuntimeEditorPlacedObjects");
            root.transform.SetParent(transform, false);
            placedRoot = root.transform;
        }

        private void SetEditorVisible(bool visible)
        {
            editorVisible = visible;

            PlayerMotor player = GameManager.Instance != null ? GameManager.Instance.Player : null;
            if (player != null)
            {
                player.SetEditorMode(editorVisible);
            }

            Cursor.lockState = editorVisible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = editorVisible;
            SetStatus(editorVisible
                ? "Asset Editor dang mo. Click trai de dat object, click phai de chon object."
                : "Da dong Asset Editor.", 2.0f);
        }

        private void TryPlaceSelectedAsset()
        {
            Camera cam = GetEditingCamera();
            if (cam == null)
            {
                SetStatus("Khong tim thay camera de dat object.", 2.2f);
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 500f, ~0, QueryTriggerInteraction.Ignore))
            {
                SetStatus("Khong bat duoc diem dat object. Thu chi vao mat dat/prop co collider.", 1.6f);
                return;
            }

            GameObject created = CreateAssetInstance(selectedAssetId);
            if (created == null)
            {
                SetStatus("Khong tao duoc asset da chon.", 2.0f);
                return;
            }

            created.transform.SetParent(placedRoot, true);
            created.transform.position = SnapPosition(hit.point);
            AlignObjectToGround(created.transform, hit.point.y);

            RuntimeEditorPlacedObject marker = created.GetComponent<RuntimeEditorPlacedObject>();
            if (marker == null)
            {
                marker = created.AddComponent<RuntimeEditorPlacedObject>();
            }
            marker.assetId = selectedAssetId;

            selectedObject = marker;
            SetStatus("Da them: " + GetLabelForAsset(selectedAssetId), 1.6f);
        }

        private void TrySelectObjectFromCursor()
        {
            Camera cam = GetEditingCamera();
            if (cam == null)
            {
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 500f, ~0, QueryTriggerInteraction.Ignore))
            {
                selectedObject = null;
                SetStatus("Khong co object editor nao duoc chon.", 1.2f);
                return;
            }

            RuntimeEditorPlacedObject marker = hit.transform.GetComponentInParent<RuntimeEditorPlacedObject>();
            if (marker != null && marker.transform.IsChildOf(placedRoot))
            {
                selectedObject = marker;
                SetStatus("Dang chon object: " + marker.name, 1.2f);
            }
            else
            {
                selectedObject = null;
                SetStatus("Object nay khong thuoc nhom duoc editor tao ra.", 1.6f);
            }
        }

        private void DeleteSelected()
        {
            if (selectedObject == null)
            {
                return;
            }

            GameObject target = selectedObject.gameObject;
            selectedObject = null;
            Destroy(target);
            SetStatus("Da xoa object dang chon.", 1.4f);
        }

        private void DuplicateSelected()
        {
            if (selectedObject == null)
            {
                return;
            }

            string assetId = selectedObject.assetId;
            GameObject duplicate = CreateAssetInstance(assetId);
            if (duplicate == null)
            {
                SetStatus("Khong duplicate duoc object nay.", 1.6f);
                return;
            }

            duplicate.transform.SetParent(placedRoot, true);
            duplicate.transform.position = selectedObject.transform.position + new Vector3(positionStep, 0f, positionStep);
            duplicate.transform.rotation = selectedObject.transform.rotation;
            duplicate.transform.localScale = selectedObject.transform.localScale;

            RuntimeEditorPlacedObject marker = duplicate.GetComponent<RuntimeEditorPlacedObject>();
            if (marker == null)
            {
                marker = duplicate.AddComponent<RuntimeEditorPlacedObject>();
            }
            marker.assetId = assetId;
            selectedObject = marker;
            SetStatus("Da duplicate object.", 1.2f);
        }

        private void SnapSelectedToGround()
        {
            if (selectedObject == null)
            {
                return;
            }

            Vector3 castOrigin = selectedObject.transform.position + Vector3.up * 50f;
            if (Physics.Raycast(castOrigin, Vector3.down, out RaycastHit hit, 200f, ~0, QueryTriggerInteraction.Ignore))
            {
                AlignObjectToGround(selectedObject.transform, hit.point.y);
                SetStatus("Da snap object xuong mat dat.", 1.4f);
            }
        }

        private void SaveLayout()
        {
            EnsurePlacedRoot();

            LayoutData data = new LayoutData();
            RuntimeEditorPlacedObject[] markers = placedRoot.GetComponentsInChildren<RuntimeEditorPlacedObject>();
            foreach (RuntimeEditorPlacedObject marker in markers)
            {
                if (marker == null || marker.transform.parent != placedRoot)
                {
                    continue;
                }

                data.objects.Add(new PlacedObjectData
                {
                    assetId = marker.assetId,
                    position = marker.transform.position,
                    eulerAngles = marker.transform.eulerAngles,
                    localScale = marker.transform.localScale
                });
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(layoutPath, json);
            SetStatus("Da save layout: " + data.objects.Count + " object.", 2.2f);
        }

        private void ReloadLayout()
        {
            ClearPlacedObjects();
            TryLoadLayout(false);
        }

        private void TryLoadLayout(bool silentWhenMissing)
        {
            EnsurePlacedRoot();

            if (!File.Exists(layoutPath))
            {
                if (!silentWhenMissing)
                {
                    SetStatus("Chua co file layout. Hay dat object roi bam F5 de save.", 2.5f);
                }
                return;
            }

            LayoutData data = JsonUtility.FromJson<LayoutData>(File.ReadAllText(layoutPath));
            if (data == null || data.objects == null)
            {
                SetStatus("File layout khong hop le.", 2.2f);
                return;
            }

            int loaded = 0;
            foreach (PlacedObjectData item in data.objects)
            {
                GameObject instance = CreateAssetInstance(item.assetId);
                if (instance == null)
                {
                    continue;
                }

                instance.transform.SetParent(placedRoot, true);
                instance.transform.position = item.position;
                instance.transform.rotation = Quaternion.Euler(item.eulerAngles);
                instance.transform.localScale = item.localScale;

                RuntimeEditorPlacedObject marker = instance.GetComponent<RuntimeEditorPlacedObject>();
                if (marker == null)
                {
                    marker = instance.AddComponent<RuntimeEditorPlacedObject>();
                }
                marker.assetId = item.assetId;
                loaded++;
            }

            SetStatus("Da load layout: " + loaded + " object.", 2.0f);
        }

        private void ClearPlacedObjects()
        {
            selectedObject = null;

            if (placedRoot != null)
            {
                DestroyImmediate(placedRoot.gameObject);
            }

            placedRoot = null;
            EnsurePlacedRoot();
        }

        private Camera GetEditingCamera()
        {
            if (GameManager.Instance != null && GameManager.Instance.Player != null && GameManager.Instance.Player.ViewCamera != null)
            {
                return GameManager.Instance.Player.ViewCamera;
            }

            return Camera.main;
        }

        private bool IsPointerOverEditorWindow()
        {
            Vector2 mouseGui = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            return windowRect.Contains(mouseGui);
        }

        private GameObject CreateAssetInstance(string assetId)
        {
            if (string.IsNullOrEmpty(assetId))
            {
                return null;
            }

            GameObject created = ExternalAssetCatalog.CreateInstance(assetId);
            if (created == null)
            {
                return null;
            }

            RuntimeEditorPlacedObject marker = created.GetComponent<RuntimeEditorPlacedObject>();
            if (marker == null)
            {
                marker = created.AddComponent<RuntimeEditorPlacedObject>();
            }
            marker.assetId = assetId;

            EnsureHasSelectableCollider(created);
            return created;
        }

        private static void EnsureHasSelectableCollider(GameObject root)
        {
            if (root.GetComponentInChildren<Collider>() != null)
            {
                return;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                BoxCollider fallback = root.AddComponent<BoxCollider>();
                fallback.size = Vector3.one;
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = root.transform.InverseTransformPoint(bounds.center);
            collider.size = bounds.size;
        }

        private static void AlignObjectToGround(Transform target, float groundY)
        {
            Bounds bounds;
            if (!TryCalculateBounds(target.gameObject, out bounds))
            {
                Vector3 pos = target.position;
                pos.y = groundY;
                target.position = pos;
                return;
            }

            float delta = groundY - bounds.min.y;
            target.position += Vector3.up * delta;
        }

        private static bool TryCalculateBounds(GameObject root, out Bounds bounds)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                return true;
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>();
            if (colliders.Length > 0)
            {
                bounds = colliders[0].bounds;
                for (int i = 1; i < colliders.Length; i++)
                {
                    bounds.Encapsulate(colliders[i].bounds);
                }
                return true;
            }

            bounds = new Bounds(root.transform.position, Vector3.one);
            return false;
        }

        private Vector3 SnapPosition(Vector3 position)
        {
            float step = Mathf.Max(0.01f, positionStep);
            position.x = Mathf.Round(position.x / step) * step;
            position.y = Mathf.Round(position.y / step) * step;
            position.z = Mathf.Round(position.z / step) * step;
            return position;
        }

        private static Vector3 ClampScale(Vector3 scale)
        {
            return new Vector3(
                Mathf.Clamp(scale.x, 0.1f, 20f),
                Mathf.Clamp(scale.y, 0.1f, 20f),
                Mathf.Clamp(scale.z, 0.1f, 20f));
        }

        private string GetLabelForAsset(string assetId)
        {
            for (int i = 0; i < catalog.Count; i++)
            {
                if (catalog[i].id == assetId)
                {
                    return catalog[i].label;
                }
            }

            return assetId;
        }

        private static string FormatVector(Vector3 value)
        {
            return value.x.ToString("0.00") + ", " + value.y.ToString("0.00") + ", " + value.z.ToString("0.00");
        }

        private void SetStatus(string message, float duration)
        {
            statusMessage = message;
            statusTimer = duration;
        }
    }
}
