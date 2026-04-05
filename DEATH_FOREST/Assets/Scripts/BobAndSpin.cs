using UnityEngine;

namespace HollowManor
{
    public sealed class BobAndSpin : MonoBehaviour
    {
        public float bobAmplitude = 0.12f;
        public float bobSpeed = 2.5f;
        public float spinSpeed = 60f;

        private Vector3 startLocalPosition;

        private void Awake()
        {
            startLocalPosition = transform.localPosition;
        }

        private void Update()
        {
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.localPosition = startLocalPosition + Vector3.up * bobOffset;
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
        }
    }
}
