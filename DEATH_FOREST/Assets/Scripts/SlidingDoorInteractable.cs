using UnityEngine;

namespace HollowManor
{
    public sealed class SlidingDoorInteractable : Interactable
    {
        public bool requiresPower;
        public bool requiresRedKeycard;
        public bool requiresAllEvidence;
        public bool unlockArchiveOnOpen;
        public bool staysOpenOnceUnlocked;
        public Vector3 openLocalOffset = new Vector3(0f, 3.15f, 0f);
        public float slideSpeed = 4f;

        private Vector3 closedLocalPosition;
        private Vector3 targetLocalPosition;
        private bool isOpen;
        private bool permanentlyUnlocked;

        private void Awake()
        {
            closedLocalPosition = transform.localPosition;
            targetLocalPosition = closedLocalPosition;
        }

        private void Update()
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, Time.deltaTime * slideSpeed);
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            if (!CanUse(out string reason))
            {
                return reason;
            }

            return isOpen ? "E - Đóng cửa" : "E - Mở cửa";
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (!CanUse(out string reason))
            {
                if (!string.IsNullOrEmpty(reason) && GameManager.Instance != null)
                {
                    GameManager.Instance.ShowToast(reason);
                }
                return;
            }

            if (staysOpenOnceUnlocked && permanentlyUnlocked)
            {
                return;
            }

            isOpen = !isOpen;
            targetLocalPosition = isOpen ? closedLocalPosition + openLocalOffset : closedLocalPosition;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.EmitNoise(transform.position, isOpen ? 14.8f : 11.2f, isOpen ? 0.52f : 0.38f, isOpen ? "mo cua an ninh" : "dong cua an ninh");
            }

            if (unlockArchiveOnOpen && isOpen)
            {
                permanentlyUnlocked = true;
                isOpen = true;
                targetLocalPosition = closedLocalPosition + openLocalOffset;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.UnlockArchive();
                }
            }
        }

        private bool CanUse(out string reason)
        {
            if (GameManager.Instance == null)
            {
                reason = string.Empty;
                return true;
            }

            if (requiresPower && !GameManager.Instance.PowerRestored)
            {
                reason = "Cửa an ninh không có điện.";
                return false;
            }

            if (requiresAllEvidence || requiresRedKeycard)
            {
                if (!GameManager.Instance.CanUnlockArchive(out reason))
                {
                    return false;
                }
            }

            reason = string.Empty;
            return true;
        }
    }
}
