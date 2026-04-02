using UnityEngine;

namespace HollowManor
{
    public sealed class FuseBoxInteractable : Interactable
    {
        private Renderer cachedRenderer;

        private void Awake()
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            RefreshVisual();
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StateChanged += RefreshVisual;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StateChanged -= RefreshVisual;
            }
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            if (GameManager.Instance == null)
            {
                return string.Empty;
            }

            if (GameManager.Instance.PowerRestored)
            {
                return "Hộp điện phụ đang chạy";
            }

            if (!GameManager.Instance.HasBlueFuse)
            {
                return "Cần cầu chì xanh";
            }

            return "E - Lắp cầu chì và khởi động";
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            bool wasPowered = GameManager.Instance.PowerRestored;
            GameManager.Instance.RestorePower();
            if (!wasPowered && GameManager.Instance.PowerRestored)
            {
                GameManager.Instance.EmitNoise(transform.position, 10.5f, 0.55f, "Máy phát");
            }
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            if (cachedRenderer == null)
            {
                return;
            }

            Color color = GameManager.Instance != null && GameManager.Instance.PowerRestored
                ? GameColors.Power
                : GameColors.Danger;

            if (cachedRenderer.material != null)
            {
                cachedRenderer.material.color = color;
            }
        }
    }
}
