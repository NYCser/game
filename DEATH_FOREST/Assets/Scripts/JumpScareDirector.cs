using System.Collections;
using UnityEngine;

namespace HollowManor
{
    public sealed class JumpScareDirector : MonoBehaviour
    {
        public float triggerDistance = 11f;
        public float minimumThreat = 0.30f;
        public float cooldown = 9f;
        public float distantScareCooldown = 5.75f;
        public float distantScareMinDistance = 7f;
        public float distantScareMaxDistance = 15f;

        private PatrolEnemy ghost;
        private PlayerMotor player;
        private float cooldownTimer;
        private float distantCooldownTimer;
        private AudioClip distantWhisperClip;

        private void Awake()
        {
            distantWhisperClip = CreateDistantWhisperClip();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.IntroActive || GameManager.Instance.IsEnded || GameManager.Instance.EscapeSequenceActive)
            {
                return;
            }

            if (ghost == null)
            {
                ghost = FindFirstObjectByType<PatrolEnemy>();
            }

            if (player == null)
            {
                player = GameManager.Instance.Player;
            }

            if (ghost == null || player == null)
            {
                return;
            }

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            if (distantCooldownTimer > 0f)
            {
                distantCooldownTimer -= Time.deltaTime;
            }

            if (!player.IsHidden)
            {
                TryCloseJumpScare();
            }

            TryDistantScare();
        }

        public void TriggerDoorJumpScare(Vector3 doorwayPosition, Vector3 facingDirection)
        {
            if (GameManager.Instance == null || GameManager.Instance.IntroActive || GameManager.Instance.IsEnded || GameManager.Instance.EscapeSequenceActive || player == null)
            {
                return;
            }

            Vector3 flatDirection = Flatten(facingDirection);
            if (flatDirection.sqrMagnitude < 0.001f)
            {
                flatDirection = Vector3.forward;
            }

            Vector3 spawn = doorwayPosition + flatDirection.normalized * 1.4f + Vector3.up * 0.08f;
            StartCoroutine(SpawnJumpScare(null, 0f, 0.72f, false, spawn));
            PlayDistantWhisper(spawn + Vector3.up * 1.2f);
            cooldownTimer = Mathf.Max(cooldownTimer, 3.6f);
            distantCooldownTimer = Mathf.Max(distantCooldownTimer, 2.2f);
        }

        private void TryCloseJumpScare()
        {
            if (cooldownTimer > 0f)
            {
                return;
            }

            float distance = Vector3.Distance(Flatten(ghost.transform.position), Flatten(player.transform.position));
            if (distance > triggerDistance || GameManager.Instance.ThreatLevel < minimumThreat)
            {
                return;
            }

            StartCoroutine(SpawnJumpScare(player.ViewCamera.transform, 1.6f, 0.55f, true));
            cooldownTimer = cooldown;
        }

        private void TryDistantScare()
        {
            if (distantCooldownTimer > 0f || player.ViewCamera == null)
            {
                return;
            }

            float threat = GameManager.Instance.ThreatLevel;
            if (threat < 0.16f)
            {
                return;
            }

            float chance = Mathf.Lerp(0.03f, 0.14f, threat);
            if (Random.value > chance * Time.deltaTime)
            {
                return;
            }

            Transform cam = player.ViewCamera.transform;
            Vector3 flatForward = Flatten(cam.forward).normalized;
            if (flatForward.sqrMagnitude < 0.01f)
            {
                flatForward = Flatten(player.transform.forward).normalized;
            }

            Vector3 side = Random.value < 0.5f ? cam.right : -cam.right;
            float distance = Random.Range(distantScareMinDistance, distantScareMaxDistance);
            Vector3 spawn = player.transform.position + flatForward * distance + Flatten(side) * Random.Range(3f, 7f);
            spawn.y = Mathf.Max(0.1f, player.transform.position.y + Random.Range(-0.1f, 0.15f));

            if (Physics.Raycast(cam.position, (spawn - cam.position).normalized, out RaycastHit hit, Vector3.Distance(cam.position, spawn), ~0, QueryTriggerInteraction.Ignore))
            {
                spawn = hit.point - (spawn - cam.position).normalized * 0.65f;
                spawn.y = Mathf.Max(0.1f, spawn.y);
            }

            StartCoroutine(SpawnJumpScare(null, 0f, 0.85f, false, spawn));
            PlayDistantWhisper(spawn + Vector3.up * 1.15f);
            distantCooldownTimer = Random.Range(distantScareCooldown * 0.75f, distantScareCooldown * 1.25f);
        }

        private IEnumerator SpawnJumpScare(Transform attachTo, float forwardOffset, float duration, bool playLoudAudio, Vector3? worldPositionOverride = null)
        {
            if (player == null || player.ViewCamera == null)
            {
                yield break;
            }

            Transform cam = player.ViewCamera.transform;
            GameObject illusion = CreateIllusion();
            if (worldPositionOverride.HasValue)
            {
                illusion.transform.position = worldPositionOverride.Value;
                illusion.transform.rotation = Quaternion.LookRotation(Flatten(cam.position - illusion.transform.position).normalized, Vector3.up);
            }
            else
            {
                illusion.transform.position = cam.position + cam.forward * forwardOffset - cam.up * 0.65f;
                illusion.transform.rotation = Quaternion.LookRotation(-cam.forward, Vector3.up);
                illusion.transform.SetParent(attachTo, true);
            }

            AudioFeedbackController audioController = GetComponent<AudioFeedbackController>();
            if (playLoudAudio && audioController != null)
            {
                audioController.PlayJumpScare();
            }

            float t = 0f;
            Renderer[] renderers = illusion.GetComponentsInChildren<Renderer>();
            while (t < duration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / duration);
                float alpha = 1f - normalized;
                illusion.transform.position += Vector3.up * Time.deltaTime * (playLoudAudio ? 0.15f : 0.06f);
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null && renderers[i].material != null)
                    {
                        Color c = renderers[i].material.color;
                        c.a = alpha * (playLoudAudio ? 0.78f : 0.45f);
                        renderers[i].material.color = c;
                    }
                }
                yield return null;
            }

            Destroy(illusion);
        }

        private void PlayDistantWhisper(Vector3 position)
        {
            if (distantWhisperClip == null)
            {
                return;
            }

            GameObject emitter = new GameObject("DistantWhisperEmitter");
            emitter.transform.position = position;
            AudioSource source = emitter.AddComponent<AudioSource>();
            source.clip = distantWhisperClip;
            source.spatialBlend = 1f;
            source.minDistance = 2f;
            source.maxDistance = 22f;
            source.volume = 0.36f;
            source.pitch = Random.Range(0.92f, 1.06f);
            source.Play();
            Destroy(emitter, distantWhisperClip.length + 0.2f);
        }

        private static GameObject CreateIllusion()
        {
            GameObject root = new GameObject("GhostApparition");
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 1f, 1f, 0.7f);
            mat.SetFloat("_Mode", 3f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;

            if (ExternalAssetCatalog.TryGetFirstByRole(ExternalAssetPrefabAdapter.AssetRole.Ghost, out ExternalAssetCatalog.Entry ghostEntry))
            {
                GameObject instance = ExternalAssetCatalog.CreateInstance(ghostEntry, "GhostIllusionModel", root.transform);
                if (instance != null)
                {
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    foreach (Collider col in instance.GetComponentsInChildren<Collider>())
                    {
                        if (col != null)
                        {
                            Destroy(col);
                        }
                    }

                    foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>())
                    {
                        if (renderer != null)
                        {
                            renderer.material = mat;
                        }
                    }

                    return root;
                }
            }

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            body.transform.localScale = new Vector3(0.8f, 1.0f, 0.8f);
            Destroy(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().material = mat;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 2.15f, 0f);
            head.transform.localScale = Vector3.one * 0.55f;
            Destroy(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().material = mat;

            return root;
        }

        private static AudioClip CreateDistantWhisperClip()
        {
            int sampleRate = 22050;
            float duration = 1.2f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float fade = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / duration));
                float hiss = (Mathf.PerlinNoise(t * 18f, 0.17f) * 2f - 1f) * 0.11f;
                float tone = Mathf.Sin(2f * Mathf.PI * (340f + Mathf.Sin(t * 7f) * 45f) * t) * 0.05f;
                data[i] = (hiss + tone) * fade * 0.38f;
            }

            AudioClip clip = AudioClip.Create("DistantWhisper", length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }
    }
}
