using UnityEngine;

namespace HollowManor
{
    public sealed class HideSpotInteractable : Interactable
    {
        public enum HideSpotKind
        {
            Bush,
            Log,
            Closet,
            Tent,
            CabinCorner
        }

        public string hideLabel = "cho nap";
        public HideSpotKind kind = HideSpotKind.Bush;
        [Range(0.05f, 1f)] public float visibilityFactorWhenHidden = 0.15f;
        [Range(0.10f, 1f)] public float safeEscapeChance = 0.80f;
        public float minHideCheckDelay = 5f;
        public float maxHideCheckDelay = 10f;
        public float captureCheckDistance = 2.2f;
        public Transform HideAnchor { get; private set; }
        public Transform ExitAnchor { get; private set; }

        private void Awake()
        {
            HideAnchor = transform.Find("HideAnchor");
            ExitAnchor = transform.Find("ExitAnchor");

            if (HideAnchor == null)
            {
                GameObject hideObject = new GameObject("HideAnchor");
                HideAnchor = hideObject.transform;
                HideAnchor.SetParent(transform);
                HideAnchor.localPosition = new Vector3(0f, 0f, -0.2f);
                HideAnchor.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }

            if (ExitAnchor == null)
            {
                GameObject exitObject = new GameObject("ExitAnchor");
                ExitAnchor = exitObject.transform;
                ExitAnchor.SetParent(transform);
                ExitAnchor.localPosition = new Vector3(0f, 0f, 1.1f);
                ExitAnchor.localRotation = Quaternion.identity;
            }
        }

        public float GetRandomHideCheckDelay()
        {
            return Random.Range(Mathf.Min(minHideCheckDelay, maxHideCheckDelay), Mathf.Max(minHideCheckDelay, maxHideCheckDelay));
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            PlayerMotor motor = interactor.GetMotor();
            if (motor == null)
            {
                return string.Empty;
            }

            if (motor.IsHidden && motor.CurrentHideSpot == this)
            {
                return "E - Roi " + hideLabel;
            }

            return motor.IsHidden ? string.Empty : "E - Nap vao " + hideLabel;
        }

        public override void Interact(PlayerInteractor interactor)
        {
            PlayerMotor motor = interactor.GetMotor();
            if (motor == null)
            {
                return;
            }

            if (motor.IsHidden && motor.CurrentHideSpot == this)
            {
                motor.ExitHide();
                return;
            }

            if (!motor.IsHidden)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.EmitNoise(transform.position, 3.2f, 0.18f, "nap vao cho an");
                }
                motor.EnterHide(this);
            }
        }
    }
}
