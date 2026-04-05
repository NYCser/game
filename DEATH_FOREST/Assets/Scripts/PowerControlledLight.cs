using UnityEngine;

namespace HollowManor
{
    [RequireComponent(typeof(Light))]
    public sealed class PowerControlledLight : MonoBehaviour
    {
        public bool activeWhenPowered = true;
        public Renderer indicatorRenderer;

        private Light targetLight;

        private void Awake()
        {
            targetLight = GetComponent<Light>();
            Refresh();
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StateChanged += Refresh;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StateChanged -= Refresh;
            }
        }

        public void Refresh()
        {
            bool powerState = GameManager.Instance != null && GameManager.Instance.PowerRestored;
            bool enabledState = activeWhenPowered ? powerState : !powerState;

            if (targetLight != null)
            {
                targetLight.enabled = enabledState;
            }

            if (indicatorRenderer != null && indicatorRenderer.material != null)
            {
                indicatorRenderer.material.color = enabledState ? GameColors.Power : GameColors.Danger;
            }
        }
    }
}
