using System;
using UnityEngine;

namespace HollowManor
{
    public sealed class AudioFeedbackController : MonoBehaviour
    {
        private AudioSource ambientSource;
        private AudioSource tensionSource;
        private AudioSource uiSfxSource;
        private AudioSource proximitySource;
        private AudioSource hunterLoopSource;
        private AudioClip ambientClip;
        private AudioClip tensionClip;
        private AudioClip whisperClip;
        private AudioClip stingerClip;
        private AudioClip screamClip;
        private AudioClip repairClip;
        private AudioClip engineClip;
        private AudioClip doorClip;
        private AudioClip rustleClip;
        private AudioClip hunterLoopClip;
        private AudioClip winClip;
        private AudioClip loseClip;
        private float nextThreatSoundTime;
        private int lastNoiseEventId = -1;
        private ObjectiveStage lastStage;
        private PatrolEnemy trackedGhost;
        private Transform threatAudioAnchor;

        private void Awake()
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
            ambientSource.spatialBlend = 0f;
            ambientSource.volume = 0.15f;

            tensionSource = gameObject.AddComponent<AudioSource>();
            tensionSource.loop = true;
            tensionSource.playOnAwake = false;
            tensionSource.spatialBlend = 0f;
            tensionSource.volume = 0f;

            uiSfxSource = gameObject.AddComponent<AudioSource>();
            uiSfxSource.loop = false;
            uiSfxSource.playOnAwake = false;
            uiSfxSource.spatialBlend = 0f;
            uiSfxSource.volume = 0.32f;

            GameObject threatObject = new GameObject("ThreatAudioAnchor");
            threatObject.transform.SetParent(transform, false);
            threatAudioAnchor = threatObject.transform;

            proximitySource = threatObject.AddComponent<AudioSource>();
            proximitySource.loop = false;
            proximitySource.playOnAwake = false;
            proximitySource.spatialBlend = 1f;
            proximitySource.volume = 0.38f;
            proximitySource.minDistance = 2f;
            proximitySource.maxDistance = 30f;

            hunterLoopSource = threatObject.AddComponent<AudioSource>();
            hunterLoopSource.loop = true;
            hunterLoopSource.playOnAwake = false;
            hunterLoopSource.spatialBlend = 1f;
            hunterLoopSource.volume = 0f;
            hunterLoopSource.minDistance = 4f;
            hunterLoopSource.maxDistance = 36f;

            ambientClip = Resources.Load<AudioClip>("ForestAmbientLoop") ?? CreateForestAmbientLoop();
            tensionClip = Resources.Load<AudioClip>("ForestTensionLoop") ?? CreateTensionLoop();
            whisperClip = Resources.Load<AudioClip>("GhostWhisperSfx") ?? CreateGhostWhisper();
            stingerClip = Resources.Load<AudioClip>("GhostStingerSfx") ?? CreateGhostStinger();
            screamClip = Resources.Load<AudioClip>("GhostScreamSfx") ?? CreateGhostScream();
            repairClip = Resources.Load<AudioClip>("CarRepairSfx") ?? CreateMetalClank();
            engineClip = Resources.Load<AudioClip>("CarStartSfx") ?? CreateEngineStart();
            doorClip = Resources.Load<AudioClip>("DoorCreakSfx") ?? CreateDoorCreak();
            rustleClip = Resources.Load<AudioClip>("GrassRustleSfx") ?? CreateGrassRustle();
            hunterLoopClip = Resources.Load<AudioClip>("HunterThreatLoop") ?? CreateThreatPulseLoop();
            winClip = Resources.Load<AudioClip>("EscapeWinSfx") ?? CreateWinClip();
            loseClip = Resources.Load<AudioClip>("DeathLoseSfx") ?? CreateLoseClip();

            ambientSource.clip = ambientClip;
            ambientSource.Play();
            tensionSource.clip = tensionClip;
            tensionSource.Play();
            hunterLoopSource.clip = hunterLoopClip;
            hunterLoopSource.Play();
        }

        private void Update()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager gameManager = GameManager.Instance;
            RefreshTrackedGhost();
            if (trackedGhost != null && threatAudioAnchor != null)
            {
                threatAudioAnchor.position = trackedGhost.transform.position + Vector3.up * 1.2f;
            }

            float threat = gameManager.ThreatLevel;
            if (gameManager.IntroActive || gameManager.EscapeSequenceActive)
            {
                threat = 0f;
            }

            ambientSource.volume = Mathf.Lerp(0.09f, 0.22f, threat * 0.65f + 0.12f);
            ambientSource.pitch = Mathf.Lerp(0.95f, 1.03f, threat * 0.55f);
            tensionSource.volume = Mathf.Lerp(tensionSource.volume, Mathf.Lerp(0f, 0.20f, threat), Time.deltaTime * 2.8f);
            tensionSource.pitch = Mathf.Lerp(0.84f, 1.10f, threat);
            hunterLoopSource.volume = Mathf.Lerp(hunterLoopSource.volume, Mathf.SmoothStep(0f, 0.42f, Mathf.InverseLerp(0.18f, 1f, threat)), Time.deltaTime * 4.6f);
            hunterLoopSource.pitch = Mathf.Lerp(0.76f, 1.26f, threat);

            if (gameManager.ActiveNoiseEventId != lastNoiseEventId)
            {
                lastNoiseEventId = gameManager.ActiveNoiseEventId;
                string label = gameManager.ActiveNoiseEventLabel ?? string.Empty;
                if (label.IndexOf("sua xe", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    uiSfxSource.pitch = UnityEngine.Random.Range(0.94f, 1.04f);
                    uiSfxSource.PlayOneShot(repairClip, 0.42f);
                }
                else if (label.IndexOf("dong co xe", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    uiSfxSource.pitch = 1f;
                    uiSfxSource.PlayOneShot(engineClip, 0.46f);
                }
                else if (label.IndexOf("cua", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    uiSfxSource.pitch = UnityEngine.Random.Range(0.88f, 1.04f);
                    uiSfxSource.PlayOneShot(doorClip, 0.26f);
                }
                else if (label.IndexOf("chan", StringComparison.OrdinalIgnoreCase) >= 0 || label.IndexOf("co", StringComparison.OrdinalIgnoreCase) >= 0 || label.IndexOf("nuoc", StringComparison.OrdinalIgnoreCase) >= 0 || label.IndexOf("san go", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    uiSfxSource.pitch = UnityEngine.Random.Range(0.94f, 1.08f);
                    uiSfxSource.PlayOneShot(rustleClip, 0.12f);
                }
            }

            if (!gameManager.IsEnded && !gameManager.IntroActive && !gameManager.EscapeSequenceActive && Time.time >= nextThreatSoundTime)
            {
                float intervalScale = Mathf.Lerp(2.6f, 0.9f, threat);
                if (threat >= 0.82f)
                {
                    proximitySource.pitch = UnityEngine.Random.Range(0.98f, 1.08f);
                    proximitySource.PlayOneShot(screamClip, 0.34f + threat * 0.30f);
                    nextThreatSoundTime = Time.time + UnityEngine.Random.Range(0.45f, 0.85f) * intervalScale;
                }
                else if (threat >= 0.55f)
                {
                    proximitySource.pitch = UnityEngine.Random.Range(0.92f, 1.04f);
                    proximitySource.PlayOneShot(stingerClip, 0.18f + threat * 0.22f);
                    nextThreatSoundTime = Time.time + UnityEngine.Random.Range(0.60f, 1.05f) * intervalScale;
                }
                else if (threat >= 0.20f)
                {
                    proximitySource.pitch = UnityEngine.Random.Range(0.88f, 1.00f);
                    proximitySource.PlayOneShot(whisperClip, 0.10f + threat * 0.14f);
                    nextThreatSoundTime = Time.time + UnityEngine.Random.Range(0.95f, 1.60f) * intervalScale;
                }
            }

            if (lastStage != gameManager.Stage)
            {
                if (gameManager.Stage == ObjectiveStage.Win)
                {
                    uiSfxSource.pitch = 1f;
                    uiSfxSource.PlayOneShot(winClip, 0.5f);
                }
                else if (gameManager.Stage == ObjectiveStage.Lose)
                {
                    uiSfxSource.pitch = 1f;
                    uiSfxSource.PlayOneShot(loseClip, 0.5f);
                }

                lastStage = gameManager.Stage;
            }
        }


        private void RefreshTrackedGhost()
        {
            if (trackedGhost == null)
            {
                trackedGhost = FindFirstObjectByType<PatrolEnemy>();
            }
        }

        public void PlayJumpScare()
        {
            uiSfxSource.pitch = UnityEngine.Random.Range(0.94f, 1.06f);
            uiSfxSource.PlayOneShot(screamClip, 0.48f);
        }

        private static AudioClip CreateForestAmbientLoop()
        {
            int sampleRate = 22050;
            float duration = 6.0f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float wind = Mathf.Sin(2f * Mathf.PI * 38f * t) * 0.02f + Mathf.Sin(2f * Mathf.PI * 73f * t) * 0.014f;
                float rumble = Mathf.Sin(2f * Mathf.PI * 11f * t) * 0.03f;
                float noise = (Mathf.PerlinNoise(t * 1.1f, 0.13f) * 2f - 1f) * 0.018f;
                float chirpGate = Mathf.Clamp01(Mathf.Sin(2f * Mathf.PI * 0.85f * t) * 0.5f + 0.5f);
                float chirp = Mathf.Sin(2f * Mathf.PI * (2900f + Mathf.Sin(2f * Mathf.PI * 6f * t) * 180f) * t) * 0.015f * chirpGate * chirpGate;
                data[i] = wind + rumble + noise + chirp;
            }

            AudioClip clip = AudioClip.Create("ForestAmbientLoop", length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateTensionLoop()
        {
            int sampleRate = 22050;
            float duration = 3.0f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float drone = Mathf.Sin(2f * Mathf.PI * 48f * t) * 0.06f;
                drone += Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.04f;
                float movement = 0.65f + Mathf.Sin(2f * Mathf.PI * 0.33f * t) * 0.35f;
                data[i] = drone * movement;
            }

            AudioClip clip = AudioClip.Create("ForestTensionLoop", length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateGhostWhisper()
        {
            int sampleRate = 22050;
            float duration = 0.65f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / duration));
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.16f;
                float hiss = Mathf.Sin(2f * Mathf.PI * (1400f + Mathf.Sin(2f * Mathf.PI * 5f * t) * 120f) * t) * 0.04f;
                data[i] = (noise + hiss) * envelope * 0.42f;
            }

            AudioClip clip = AudioClip.Create("GhostWhisper", length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateGhostStinger()
        {
            int sampleRate = 22050;
            float duration = 0.4f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Exp(-t * 6f);
                float tone = Mathf.Sin(2f * Mathf.PI * (320f + t * 260f) * t) * 0.22f;
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.12f;
                data[i] = (tone + noise) * envelope;
            }

            AudioClip clip = AudioClip.Create("GhostStinger", length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateGhostScream()
        {
            int sampleRate = 22050;
            float duration = 0.8f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / duration));
                float tone = Mathf.Sin(2f * Mathf.PI * (900f + 650f * t) * t) * 0.22f;
                float overtone = Mathf.Sin(2f * Mathf.PI * (1800f + 200f * t) * t) * 0.10f;
                float grit = (UnityEngine.Random.value * 2f - 1f) * 0.12f;
                data[i] = (tone + overtone + grit) * env;
            }
            AudioClip clip = AudioClip.Create("GhostScream", length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateDoorCreak()
        {
            int sampleRate = 22050;
            float duration = 0.9f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float env = Mathf.Exp(-t * 2.2f);
                float low = Mathf.Sin(2f * Mathf.PI * (95f + 12f * Mathf.Sin(t * 11f)) * t) * 0.13f;
                float squeal = Mathf.Sin(2f * Mathf.PI * (880f + 120f * Mathf.Sin(t * 6f)) * t) * 0.05f;
                data[i] = (low + squeal) * env;
            }
            AudioClip clip = AudioClip.Create("DoorCreak", length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateGrassRustle()
        {
            int sampleRate = 22050;
            float duration = 0.22f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float env = Mathf.Exp(-t * 12f);
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.18f;
                float body = Mathf.Sin(2f * Mathf.PI * 170f * t) * 0.04f;
                data[i] = (noise + body) * env;
            }
            AudioClip clip = AudioClip.Create("GrassRustle", length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateMetalClank()
        {
            int sampleRate = 22050;
            float duration = 0.7f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Exp(-t * 10f);
                float ring = Mathf.Sin(2f * Mathf.PI * 620f * t) * 0.18f;
                ring += Mathf.Sin(2f * Mathf.PI * 920f * t) * 0.12f;
                ring += Mathf.Sin(2f * Mathf.PI * 1320f * t) * 0.07f;
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.08f;
                data[i] = (ring + noise) * envelope;
            }

            AudioClip clip = AudioClip.Create("MetalClank", length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateEngineStart()
        {
            int sampleRate = 22050;
            float duration = 1.25f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float ramp = Mathf.Clamp01(t / 0.45f);
                float engine = Mathf.Sin(2f * Mathf.PI * (42f + 28f * ramp) * t) * 0.18f;
                engine += Mathf.Sin(2f * Mathf.PI * (83f + 55f * ramp) * t) * 0.08f;
                float grit = (UnityEngine.Random.value * 2f - 1f) * 0.06f * ramp;
                float cutoff = t > 0.9f ? Mathf.Lerp(1f, 0f, (t - 0.9f) / 0.35f) : 1f;
                data[i] = (engine + grit) * cutoff;
            }

            AudioClip clip = AudioClip.Create("EngineStart", length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateThreatPulseLoop()
        {
            int sampleRate = 22050;
            float duration = 1.6f;
            int length = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float pulse = Mathf.Max(0f, Mathf.Sin(2f * Mathf.PI * 1.35f * t));
                float bed = Mathf.Sin(2f * Mathf.PI * 64f * t) * 0.04f;
                float overtone = Mathf.Sin(2f * Mathf.PI * 128f * t) * 0.025f;
                float hiss = (UnityEngine.Random.value * 2f - 1f) * 0.015f;
                data[i] = (bed + overtone + hiss) * (0.45f + pulse * 0.55f);
            }

            AudioClip clip = AudioClip.Create("ThreatPulseLoop", length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateWinClip()
        {
            return CreateToneSequence("WinClip", new[] { 392f, 523f, 659f }, 0.11f, 0.03f, 0.18f);
        }

        private static AudioClip CreateLoseClip()
        {
            return CreateToneSequence("LoseClip", new[] { 330f, 220f, 146f }, 0.13f, 0.03f, 0.18f);
        }

        private static AudioClip CreateToneSequence(string name, float[] frequencies, float noteDuration, float gapDuration, float amplitude)
        {
            int sampleRate = 22050;
            int noteSamples = Mathf.RoundToInt(noteDuration * sampleRate);
            int gapSamples = Mathf.RoundToInt(gapDuration * sampleRate);
            int totalLength = frequencies.Length * noteSamples + Mathf.Max(0, frequencies.Length - 1) * gapSamples;
            float[] data = new float[totalLength];
            int cursor = 0;

            for (int noteIndex = 0; noteIndex < frequencies.Length; noteIndex++)
            {
                float frequency = frequencies[noteIndex];
                for (int i = 0; i < noteSamples; i++)
                {
                    float t = i / (float)sampleRate;
                    float env = Mathf.Sin(Mathf.PI * (i / (float)noteSamples));
                    data[cursor++] = Mathf.Sin(2f * Mathf.PI * frequency * t) * env * amplitude;
                }

                if (noteIndex < frequencies.Length - 1)
                {
                    cursor += gapSamples;
                }
            }

            AudioClip clip = AudioClip.Create(name, totalLength, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
