using UnityEngine;

namespace HollowManor
{
    public sealed class CarRepairInteractable : Interactable
    {
        public override string GetPrompt(PlayerInteractor interactor)
        {
            if (GameManager.Instance == null)
            {
                return string.Empty;
            }

            if (GameManager.Instance.CarRepaired)
            {
                return "Khoang máy đã được sửa xong";
            }

            return GameManager.Instance.CanRepairCar(out string reason)
                ? "E - Lắp linh kiện và sửa xe"
                : reason;
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            if (!GameManager.Instance.CanRepairCar(out string reason))
            {
                GameManager.Instance.ShowToast(reason);
                return;
            }

            GameManager.Instance.RepairCar();
        }
    }
}
