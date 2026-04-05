using UnityEngine;

namespace HollowManor
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        public float interactDistance = 3f;

        private PlayerMotor motor;

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.Hud == null || motor == null)
            {
                return;
            }

            if (GameManager.Instance.IsEnded || GameManager.Instance.EscapeSequenceActive)
            {
                GameManager.Instance.Hud.SetPrompt("Nhấn R để chạy lại.");
                return;
            }

            if (motor.IsHidden && motor.CurrentHideSpot != null)
            {
                GameManager.Instance.Hud.SetPrompt("E - Rời chỗ ẩn");
                if (Input.GetKeyDown(KeyCode.E))
                {
                    motor.ExitHide();
                }
                return;
            }

            Camera cameraToUse = motor.ViewCamera;
            if (cameraToUse == null)
            {
                GameManager.Instance.Hud.SetPrompt(string.Empty);
                return;
            }

            Ray ray = cameraToUse.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, ~0, QueryTriggerInteraction.Collide))
            {
                Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
                if (interactable != null)
                {
                    GameManager.Instance.Hud.SetPrompt(interactable.GetPrompt(this));
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        interactable.Interact(this);
                    }
                    return;
                }
            }

            GameManager.Instance.Hud.SetPrompt(string.Empty);
        }

        public PlayerMotor GetMotor()
        {
            return motor;
        }
    }
}
