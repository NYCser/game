using UnityEngine;

namespace HollowManor
{
    public sealed class PickupInteractable : Interactable
    {
        public ItemType itemType = ItemType.None;
        public string displayName = "Vật phẩm";

        private bool consumed;

        public override string GetPrompt(PlayerInteractor interactor)
        {
            return consumed ? string.Empty : "E - Nhặt " + displayName;
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (consumed || GameManager.Instance == null)
            {
                return;
            }

            consumed = true;
            GameManager.Instance.RegisterPickup(itemType, displayName);
            Destroy(gameObject);
        }
    }
}
