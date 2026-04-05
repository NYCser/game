using System.Collections.Generic;
using UnityEngine;

namespace HollowManor
{
    public sealed class ExternalAssetPrefabAdapter : MonoBehaviour
    {
        public enum AssetRole
        {
            GeneralObstacle,
            Tree,
            Ghost,
            Cabin,
            Car
        }

        public AssetRole role = AssetRole.GeneralObstacle;
        public bool applyOnAwake = true;
        public bool keepExistingColliders = true;
        public bool normalizeGhostHeight = true;
        public float targetGhostHeight = 2.45f;
        public float doorwayWidth = 1.45f;
        public float wallThickness = 0.22f;
        public float colliderPadding = 0.06f;

        private bool applied;

        private void Awake()
        {
            if (applyOnAwake)
            {
                ApplyNow();
            }
        }

        public void ApplyNow()
        {
            if (applied)
            {
                return;
            }

            applied = true;

            if (role == AssetRole.Ghost)
            {
                AdaptGhost();
                return;
            }

            if (keepExistingColliders && HasBlockingCollider())
            {
                return;
            }

            switch (role)
            {
                case AssetRole.Tree:
                    AdaptTree();
                    break;
                case AssetRole.Cabin:
                    AdaptCabin();
                    break;
                case AssetRole.Car:
                    AdaptCar();
                    break;
                default:
                    AdaptObstacle();
                    break;
            }
        }

        private void AdaptGhost()
        {
            if (normalizeGhostHeight)
            {
                Bounds? bounds = CalculateRendererBounds();
                if (bounds.HasValue && bounds.Value.size.y > 0.01f)
                {
                    float factor = targetGhostHeight / bounds.Value.size.y;
                    if (factor > 0.2f && factor < 8f)
                    {
                        transform.localScale *= factor;
                    }
                }
            }

            foreach (Collider collider in GetComponentsInChildren<Collider>())
            {
                if (collider == null)
                {
                    continue;
                }

                Destroy(collider);
            }
        }

        private void AdaptTree()
        {
            Bounds? bounds = CalculateRendererBounds();
            if (!bounds.HasValue)
            {
                return;
            }

            Bounds b = bounds.Value;
            float radius = Mathf.Clamp(Mathf.Min(b.extents.x, b.extents.z) * 0.42f, 0.18f, 0.75f);
            float height = Mathf.Clamp(b.size.y * 0.48f, 1.2f, Mathf.Max(1.2f, b.size.y * 0.65f));

            CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
            col.direction = 1;
            col.radius = radius;
            col.height = Mathf.Max(height, radius * 2.2f);
            Vector3 center = transform.InverseTransformPoint(new Vector3(b.center.x, b.min.y + col.height * 0.5f, b.center.z));
            col.center = center;
        }

        private void AdaptCar()
        {
            Bounds? bounds = CalculateRendererBounds();
            if (!bounds.HasValue)
            {
                return;
            }

            Bounds b = bounds.Value;
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.center = transform.InverseTransformPoint(new Vector3(b.center.x, b.min.y + b.size.y * 0.35f, b.center.z));
            col.size = new Vector3(Mathf.Max(0.8f, b.size.x - colliderPadding), Mathf.Max(0.8f, b.size.y * 0.7f), Mathf.Max(1.2f, b.size.z - colliderPadding));
        }

        private void AdaptObstacle()
        {
            Bounds? bounds = CalculateRendererBounds();
            if (!bounds.HasValue)
            {
                return;
            }

            Bounds b = bounds.Value;
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.center = transform.InverseTransformPoint(b.center);
            col.size = new Vector3(Mathf.Max(0.5f, b.size.x - colliderPadding), Mathf.Max(0.5f, b.size.y - colliderPadding), Mathf.Max(0.5f, b.size.z - colliderPadding));
        }

        private void AdaptCabin()
        {
            Bounds? bounds = CalculateRendererBounds();
            if (!bounds.HasValue)
            {
                return;
            }

            Bounds b = bounds.Value;
            Transform collisionRoot = new GameObject("GeneratedCollision").transform;
            collisionRoot.SetParent(transform, false);

            float halfX = Mathf.Max(0.8f, b.extents.x - colliderPadding * 0.5f);
            float halfZ = Mathf.Max(0.8f, b.extents.z - colliderPadding * 0.5f);
            float height = Mathf.Max(2f, b.size.y * 0.78f);
            float centerY = b.min.y + height * 0.5f;
            float doorHalf = Mathf.Clamp(doorwayWidth * 0.5f, 0.45f, halfX - 0.25f);

            CreateWall(collisionRoot, new Vector3(0f, centerY, b.center.z + halfZ - wallThickness * 0.5f), new Vector3(halfX * 2f, height, wallThickness));
            CreateWall(collisionRoot, new Vector3(-halfX + wallThickness * 0.5f, centerY, b.center.z), new Vector3(wallThickness, height, halfZ * 2f));
            CreateWall(collisionRoot, new Vector3(halfX - wallThickness * 0.5f, centerY, b.center.z), new Vector3(wallThickness, height, halfZ * 2f));

            float sideWidth = Mathf.Max(0.35f, halfX - doorHalf);
            CreateWall(collisionRoot, new Vector3(-(doorHalf + sideWidth * 0.5f), centerY, b.center.z - halfZ + wallThickness * 0.5f), new Vector3(sideWidth, height, wallThickness));
            CreateWall(collisionRoot, new Vector3((doorHalf + sideWidth * 0.5f), centerY, b.center.z - halfZ + wallThickness * 0.5f), new Vector3(sideWidth, height, wallThickness));
            CreateWall(collisionRoot, new Vector3(0f, b.min.y + height - wallThickness * 0.5f, b.center.z - halfZ + wallThickness * 0.5f), new Vector3(doorHalf * 2f, wallThickness, wallThickness));
        }

        private void CreateWall(Transform parent, Vector3 worldCenter, Vector3 size)
        {
            GameObject wall = new GameObject("AutoWall");
            wall.transform.SetParent(parent, false);
            wall.transform.position = worldCenter;
            BoxCollider collider = wall.AddComponent<BoxCollider>();
            collider.size = size;
        }

        private bool HasBlockingCollider()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                if (collider != null && !collider.isTrigger)
                {
                    return true;
                }
            }

            return false;
        }

        private Bounds? CalculateRendererBounds()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            Bounds bounds = default;
            bool hasBounds = false;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds ? bounds : null;
        }
    }
}
