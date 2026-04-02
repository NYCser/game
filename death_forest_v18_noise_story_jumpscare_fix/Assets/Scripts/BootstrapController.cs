
using System.Collections;
using UnityEngine;

namespace HollowManor
{
    public sealed class BootstrapController : MonoBehaviour
    {
        private GameObject generatedRoot;
        private bool isRebuilding;
        private float autoRespawnTimer = -1f;

        private void Start()
        {
            BuildGame();
        }

        private void Update()
        {
            if (isRebuilding || GameManager.Instance == null)
            {
                return;
            }

            if (GameManager.Instance.IsEnded && Input.GetKeyDown(KeyCode.R))
            {
                RebuildGame();
                return;
            }

            if (GameManager.Instance.Stage == ObjectiveStage.Lose)
            {
                if (autoRespawnTimer < 0f)
                {
                    autoRespawnTimer = 3.8f;
                }
                else
                {
                    autoRespawnTimer -= Time.deltaTime;
                    if (autoRespawnTimer <= 0f)
                    {
                        RebuildGame();
                    }
                }
            }
            else
            {
                autoRespawnTimer = -1f;
            }
        }

        public void RebuildGame()
        {
            if (isRebuilding)
            {
                return;
            }

            StartCoroutine(RebuildRoutine());
        }

        private IEnumerator RebuildRoutine()
        {
            isRebuilding = true;

            if (generatedRoot != null)
            {
                Destroy(generatedRoot);
                generatedRoot = null;
            }

            yield return null;

            BuildGame();
            autoRespawnTimer = -1f;
            isRebuilding = false;
        }

        private void BuildGame()
        {
            ConfigurePerformanceDefaults();
            generatedRoot = LevelFactory.Build(transform);
            DisableSceneDefaults();
        }

        private void DisableSceneDefaults()
        {
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera camera in cameras)
            {
                if (generatedRoot != null && camera.transform.IsChildOf(generatedRoot.transform))
                {
                    continue;
                }

                camera.enabled = false;
            }

            AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            foreach (AudioListener listener in listeners)
            {
                if (generatedRoot != null && listener.transform.IsChildOf(generatedRoot.transform))
                {
                    continue;
                }

                listener.enabled = false;
            }

            foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (generatedRoot != null && light.transform.IsChildOf(generatedRoot.transform))
                {
                    continue;
                }

                if (light.type == LightType.Directional)
                {
                    light.enabled = false;
                }
            }
        }

        private static void ConfigurePerformanceDefaults()
        {
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadowDistance = 56f;
            QualitySettings.pixelLightCount = 6;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.018f;
            RenderSettings.fogColor = new Color(0.03f, 0.05f, 0.06f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.14f, 0.16f, 0.18f);
        }
    }
}
