using UnityEngine;

namespace HollowManor
{
    public sealed class ExitInteractable : Interactable
    {
        public override string GetPrompt(PlayerInteractor interactor)
        {
            if (GameManager.Instance == null)
            {
                return string.Empty;
            }

            if (!GameManager.Instance.ArchiveUnlocked)
            {
                return "Cần mở kho mật trước";
            }

            return GameManager.Instance.HasConfessionTape
                ? "E - Gọi thang nâng và thoát"
                : "Cần lấy băng ghi âm cuối";
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            if (!GameManager.Instance.ArchiveUnlocked)
            {
                GameManager.Instance.ShowToast("Cửa kho mật vẫn đang khóa.");
                return;
            }

            if (!GameManager.Instance.HasConfessionTape)
            {
                GameManager.Instance.ShowToast("Bạn chưa lấy băng ghi âm cuối trong kho mật.");
                return;
            }

            GameManager.Instance.EmitNoise(transform.position, 14f, 0.90f, "Thang nâng");
            GameManager.Instance.PlayerEscaped();
        }
    }
}
