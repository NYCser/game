using UnityEngine;

namespace HollowManor
{
    [RequireComponent(typeof(Light))]
    public sealed class FlickerLight : MonoBehaviour
    {
        public float variation = 0.35f;
        public float speed = 14f;

        private Light targetLight;
        private float baseIntensity;
        private float seed;

        private void Awake()
        {
            targetLight = GetComponent<Light>();
            baseIntensity = targetLight.intensity;
            seed = Random.Range(0f, 999f);
        }

        private void Update()
        {
            float wave = Mathf.PerlinNoise(seed, Time.time * speed * 0.1f);
            float delta = Mathf.Lerp(-variation, variation, wave);
            targetLight.intensity = baseIntensity + delta;
        }
    }
}
