using UnityEngine;

namespace HollowManor
{
    public abstract class Interactable : MonoBehaviour
    {
        public abstract string GetPrompt(PlayerInteractor interactor);
        public abstract void Interact(PlayerInteractor interactor);
    }
}
