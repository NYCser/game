using UnityEngine;

namespace HollowManor
{
    public sealed class HingedDoorInteractable : Interactable
    {
        public Vector3 closedEulerAngles;
        public Vector3 openEulerAngles = new Vector3(0f, 105f, 0f);
        public float rotateSpeed = 5.5f;
        public bool startsOpen;
        public bool emitLoudNoise = true;

        private bool isOpen;
        private Quaternion closedRotation;
        private Quaternion openRotation;

        private void Awake()
        {
            closedRotation = Quaternion.Euler(closedEulerAngles == Vector3.zero ? transform.localEulerAngles : closedEulerAngles);
            openRotation = Quaternion.Euler(openEulerAngles);
            isOpen = startsOpen;
            transform.localRotation = isOpen ? openRotation : closedRotation;
        }

        private void Update()
        {
            Quaternion target = isOpen ? openRotation : closedRotation;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * rotateSpeed);
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            return isOpen ? "E - Dong cua" : "E - Mo cua";
        }

        public override void Interact(PlayerInteractor interactor)
        {
            isOpen = !isOpen;
            if (GameManager.Instance != null && emitLoudNoise)
            {
                GameManager.Instance.EmitNoise(transform.position, isOpen ? 14.4f : 10.8f, isOpen ? 0.48f : 0.36f, isOpen ? "mo cua go" : "dong cua go");

                if (isOpen && !GameManager.Instance.IntroActive && !GameManager.Instance.IsEnded)
                {
                    float scareChance = Mathf.Lerp(0.22f, 0.46f, GameManager.Instance.ThreatLevel);
                    if (Random.value <= scareChance)
                    {
                        JumpScareDirector director = Object.FindFirstObjectByType<JumpScareDirector>();
                        if (director != null)
                        {
                            Vector3 facing = transform.parent != null ? transform.parent.forward : transform.forward;
                            director.TriggerDoorJumpScare(transform.position + Vector3.up * 0.15f, facing);
                        }
                    }
                }
            }
        }
    }
}
