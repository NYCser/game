using System.Collections;
using UnityEngine;

namespace HollowManor
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 4.05f;
        public float sprintSpeed = 6.55f;
        public float crouchSpeed = 2.2f;
        public float gravity = -18f;
        public float standingHeight = 1.8f;
        public float crouchHeight = 1.15f;

        [Header("Look")]
        public float mouseSensitivity = 1.8f;
        public float minPitch = -75f;
        public float maxPitch = 80f;

        [Header("Footsteps")]
        public float walkNoiseRadius = 7.0f;
        public float sprintNoiseRadius = 12.0f;
        public float walkStepInterval = 0.55f;
        public float sprintStepInterval = 0.32f;
        public float footstepEventDuration = 0.24f;

        private CharacterController controller;
        private Transform cameraPivot;
        private Camera viewCamera;
        private Light flashlight;
        private AudioSource footstepSource;
        private AudioClip[] footstepClips;
        private AudioClip crouchStepClip;

        private float yaw;
        private float pitch;
        private float verticalVelocity;
        private float footstepTimer;
        private bool inputBlocked;
        private bool isHidden;
        private bool isCrouching;
        private bool flashlightEnabled = true;
        private bool editorModeActive;
        private bool wasMovingLastFrame;
        private string currentNoiseLabel = string.Empty;
        private HideSpotInteractable currentHideSpot;
        private bool spawnSafetyChecked;
        private float hiddenTimer;
        private float hideOutcomeDelay;
        private bool hideOutcomeResolved;
        private Vector3 previousFlatPosition;
        private float movementStuckTimer;
        private int movementUnstuckSide = 1;
        private float sustainedNoiseTime;
        private bool hideOutcomeSucceeded;

        public float CurrentNoiseRadius { get; private set; }
        public bool IsCrouching => isCrouching;
        public bool IsHidden => isHidden;
        public bool FlashlightEnabled => flashlightEnabled;
        public bool InputBlocked => inputBlocked;
        public HideSpotInteractable CurrentHideSpot => currentHideSpot;
        public Camera ViewCamera => viewCamera;
        public Transform EyeTransform => viewCamera != null ? viewCamera.transform : transform;
        public float HiddenVisibilityFactor => isHidden && currentHideSpot != null ? currentHideSpot.visibilityFactorWhenHidden : 1f;
        public float HiddenElapsed => hiddenTimer;
        public bool CanRollHideOutcome => isHidden && currentHideSpot != null && !hideOutcomeResolved && hiddenTimer >= hideOutcomeDelay;
        public bool HideOutcomeResolved => hideOutcomeResolved;
        public bool HideOutcomeSucceeded => hideOutcomeSucceeded;
        public string CurrentNoiseLabel => currentNoiseLabel;

        public float VisibilityFactor
        {
            get
            {
                float value = 1f;
                if (isCrouching)
                {
                    value *= 0.74f;
                }

                if (flashlightEnabled)
                {
                    value *= 1.15f;
                }

                value *= HiddenVisibilityFactor;
                return value;
            }
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            controller.height = standingHeight;
            controller.center = new Vector3(0f, standingHeight * 0.5f, 0f);
            controller.radius = 0.3f;
            controller.stepOffset = 0.28f;
            controller.skinWidth = 0.05f;
            controller.minMoveDistance = 0f;
            EnsureViewHierarchy();
            EnsureAudio();
            previousFlatPosition = Flatten(transform.position);
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            StartCoroutine(ResolveSpawnOverlap());
        }

        private void Update()
        {
            bool forceFreeCursor = editorModeActive || GameManager.Instance == null || GameManager.Instance.IntroActive || GameManager.Instance.IsEnded || GameManager.Instance.EscapeSequenceActive;

            if (forceFreeCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                CurrentNoiseRadius = 0f;
                currentNoiseLabel = string.Empty;
                wasMovingLastFrame = false;
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool unlock = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = unlock ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = unlock;
            }

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }

                CurrentNoiseRadius = 0f;
                currentNoiseLabel = string.Empty;
                wasMovingLastFrame = false;
                return;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleFlashlight();
            }

            if (isHidden)
            {
                CurrentNoiseRadius = 0f;
                wasMovingLastFrame = false;
                hiddenTimer += Time.deltaTime;
                if (currentHideSpot != null)
                {
                    transform.position = currentHideSpot.HideAnchor.position;
                }

                Look();
                return;
            }

            if (inputBlocked)
            {
                CurrentNoiseRadius = 0f;
                currentNoiseLabel = string.Empty;
                wasMovingLastFrame = false;
                return;
            }

            Look();
            Move();
        }

        public void SetInputBlocked(bool blocked)
        {
            inputBlocked = blocked;
        }

        public void SetEditorMode(bool active)
        {
            editorModeActive = active;
            if (editorModeActive)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                CurrentNoiseRadius = 0f;
                currentNoiseLabel = string.Empty;
                wasMovingLastFrame = false;
            }
        }

        public void OnCaught()
        {
            if (isHidden)
            {
                ExitHide();
            }

            inputBlocked = true;
            CurrentNoiseRadius = 0f;
            currentNoiseLabel = string.Empty;
            wasMovingLastFrame = false;
            footstepTimer = 0f;
            flashlightEnabled = false;
            if (flashlight != null)
            {
                flashlight.enabled = false;
            }
        }

        public void EnterHide(HideSpotInteractable hideSpot)
        {
            if (hideSpot == null)
            {
                return;
            }

            currentHideSpot = hideSpot;
            isHidden = true;
            hideOutcomeResolved = false;
            hideOutcomeSucceeded = false;
            hiddenTimer = 0f;
            hideOutcomeDelay = hideSpot.GetRandomHideCheckDelay();
            CurrentNoiseRadius = 0f;
            currentNoiseLabel = string.Empty;
            verticalVelocity = -1f;
            footstepTimer = 0f;
            wasMovingLastFrame = false;

            if (controller != null)
            {
                controller.enabled = false;
            }

            transform.position = hideSpot.HideAnchor.position;
            transform.rotation = hideSpot.HideAnchor.rotation;
            yaw = transform.eulerAngles.y;
            pitch = 0f;
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.identity;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowToast("Ban dang nap trong " + hideSpot.hideLabel + ". Giu im lang...", 1.6f);
            }
        }

        public void ExitHide()
        {
            if (!isHidden || currentHideSpot == null)
            {
                return;
            }

            Transform exitAnchor = currentHideSpot.ExitAnchor;
            isHidden = false;

            transform.position = exitAnchor.position;
            transform.rotation = exitAnchor.rotation;

            if (controller != null)
            {
                controller.enabled = true;
            }

            verticalVelocity = -1f;
            footstepTimer = 0f;
            wasMovingLastFrame = false;
            currentHideSpot = null;
            hiddenTimer = 0f;
            hideOutcomeDelay = 0f;
            hideOutcomeResolved = false;
            hideOutcomeSucceeded = false;
        }

        public void ResolveHideOutcome(bool escaped)
        {
            hideOutcomeResolved = true;
            hideOutcomeSucceeded = escaped;
            if (GameManager.Instance == null || currentHideSpot == null)
            {
                return;
            }

            if (escaped)
            {
                GameManager.Instance.ShowToast("Ban nup im trong " + currentHideSpot.hideLabel + " va tam thoi qua mat duoc no.", 2.6f);
            }
        }

        public void ForceRespawnAt(Vector3 position, Quaternion rotation)
        {
            if (controller != null)
            {
                controller.enabled = false;
            }

            transform.position = position;
            transform.rotation = rotation;
            yaw = rotation.eulerAngles.y;
            pitch = 0f;
            verticalVelocity = -1f;
            CurrentNoiseRadius = 0f;
            currentNoiseLabel = string.Empty;
            movementStuckTimer = 0f;
            sustainedNoiseTime = 0f;
            previousFlatPosition = Flatten(position);

            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.identity;
            }

            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        public IEnumerator PlayCarEscapeOutro(Vector3 runTarget, Vector3 seatTarget, Quaternion finalRotation)
        {
            if (isHidden)
            {
                ExitHide();
            }

            inputBlocked = true;
            CurrentNoiseRadius = 0f;
            currentNoiseLabel = string.Empty;
            wasMovingLastFrame = false;
            footstepTimer = 0f;
            movementStuckTimer = 0f;

            if (controller != null)
            {
                controller.enabled = false;
            }

            Vector3 start = transform.position;
            Quaternion startRotation = transform.rotation;
            Vector3 flatRunDirection = runTarget - start;
            flatRunDirection.y = 0f;
            Quaternion runRotation = flatRunDirection.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(flatRunDirection.normalized, Vector3.up)
                : finalRotation;

            float runDuration = Mathf.Clamp(Vector3.Distance(start, runTarget) / 8.5f, 0.16f, 0.42f);
            float timer = 0f;
            while (timer < runDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / runDuration);
                float eased = Mathf.SmoothStep(0f, 1f, t);
                transform.position = Vector3.Lerp(start, runTarget, eased);
                transform.rotation = Quaternion.Slerp(startRotation, runRotation, eased);
                yield return null;
            }

            Vector3 hopStart = transform.position;
            Quaternion hopRotation = transform.rotation;
            float hopDuration = 0.40f;
            timer = 0f;
            while (timer < hopDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / hopDuration);
                float eased = Mathf.SmoothStep(0f, 1f, t);
                Vector3 pos = Vector3.Lerp(hopStart, seatTarget, eased);
                pos += Vector3.up * Mathf.Sin(eased * Mathf.PI) * 0.40f;
                transform.position = pos;
                transform.rotation = Quaternion.Slerp(hopRotation, finalRotation, eased);
                yield return null;
            }

            transform.position = seatTarget;
            transform.rotation = finalRotation;
            yaw = finalRotation.eulerAngles.y;
            pitch = -4f;
            verticalVelocity = -1f;
            previousFlatPosition = Flatten(seatTarget);

            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private IEnumerator ResolveSpawnOverlap()
        {
            if (spawnSafetyChecked)
            {
                yield break;
            }

            // Wait one frame so the generated world and optional prefabs are fully spawned.
            yield return null;
            spawnSafetyChecked = true;

            if (controller == null)
            {
                yield break;
            }

            Vector3 start = transform.position;
            Vector3[] candidates =
            {
                start,
                start + new Vector3(0f, 0f, -3.5f),
                start + new Vector3(-2.5f, 0f, -2.8f),
                start + new Vector3(2.5f, 0f, -2.8f),
                start + new Vector3(-3.5f, 0f, 0f),
                start + new Vector3(3.5f, 0f, 0f),
                start + new Vector3(0f, 0f, 3.0f)
            };

            foreach (Vector3 candidate in candidates)
            {
                if (PositionIsClear(candidate))
                {
                    controller.enabled = false;
                    transform.position = candidate;
                    controller.enabled = true;
                    previousFlatPosition = Flatten(candidate);
                    yield break;
                }
            }
        }

        private bool PositionIsClear(Vector3 position)
        {
            if (controller == null)
            {
                return true;
            }

            Vector3 center = position + controller.center;
            float radius = Mathf.Max(0.05f, controller.radius * 0.96f);
            float halfHeight = Mathf.Max(radius, controller.height * 0.5f - radius);
            Vector3 point1 = center + Vector3.up * halfHeight;
            Vector3 point2 = center - Vector3.up * halfHeight;
            Collider[] hits = Physics.OverlapCapsule(point1, point2, radius, ~0, QueryTriggerInteraction.Ignore);
            foreach (Collider hit in hits)
            {
                if (hit == null || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private void Look()
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void Move()
        {
            isCrouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);

            float targetHeight = isCrouching ? crouchHeight : standingHeight;
            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 12f);
            controller.center = new Vector3(0f, controller.height * 0.5f, 0f);
            if (cameraPivot != null)
            {
                Vector3 pivotLocal = cameraPivot.localPosition;
                pivotLocal.y = controller.height - 0.15f;
                cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, pivotLocal, Time.deltaTime * 12f);
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 inputDirection = (transform.right * horizontal + transform.forward * vertical).normalized;
            float moveAmount = new Vector2(horizontal, vertical).magnitude;

            float speed = walkSpeed;
            bool isSprinting = false;
            if (isCrouching)
            {
                speed = crouchSpeed;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = sprintSpeed;
                isSprinting = true;
            }

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -1f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 displacement = inputDirection * speed;
            displacement.y = verticalVelocity;
            CollisionFlags flags = controller.Move(displacement * Time.deltaTime);
            TryRecoverFromCorner(inputDirection, moveAmount, speed, flags);

            HandleNoise(moveAmount, isSprinting);
        }

        private void TryRecoverFromCorner(Vector3 inputDirection, float moveAmount, float speed, CollisionFlags flags)
        {
            Vector3 flatCurrent = Flatten(transform.position);
            float movedDistance = Vector3.Distance(flatCurrent, previousFlatPosition);
            bool pushingIntoWall = moveAmount > 0.15f && (((flags & CollisionFlags.Sides) != 0) || movedDistance <= Mathf.Max(0.008f, speed * Time.deltaTime * 0.10f));

            if (pushingIntoWall)
            {
                movementStuckTimer += Time.deltaTime;
            }
            else
            {
                movementStuckTimer = 0f;
            }

            if (movementStuckTimer >= 0.16f && inputDirection.sqrMagnitude > 0.001f)
            {
                Vector3 side = Vector3.Cross(Vector3.up, inputDirection).normalized * movementUnstuckSide;
                Vector3 nudge = (side * speed * 1.05f - inputDirection * speed * 0.14f + Vector3.down) * Time.deltaTime;
                controller.Move(nudge);
            }

            if (movementStuckTimer >= 0.40f && inputDirection.sqrMagnitude > 0.001f)
            {
                movementUnstuckSide *= -1;
                movementStuckTimer = 0.14f;
            }

            if (movementStuckTimer >= 0.70f && inputDirection.sqrMagnitude > 0.001f)
            {
                Vector3 side = Vector3.Cross(Vector3.up, inputDirection).normalized * movementUnstuckSide;
                Vector3 emergencyNudge = (side * speed * 1.35f - inputDirection * speed * 0.24f + Vector3.down) * Time.deltaTime;
                controller.Move(emergencyNudge);
                movementStuckTimer = 0.18f;
            }

            previousFlatPosition = flatCurrent;
        }

        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }

        private void HandleNoise(float moveAmount, bool isSprinting)
        {
            if (moveAmount < 0.1f || !controller.isGrounded)
            {
                CurrentNoiseRadius = 0f;
                currentNoiseLabel = string.Empty;
                footstepTimer = 0f;
                wasMovingLastFrame = false;
                sustainedNoiseTime = 0f;
                return;
            }

            string surfaceLabel;
            float surfaceMultiplier;
            ResolveSurfaceNoise(isSprinting, out surfaceLabel, out surfaceMultiplier);

            sustainedNoiseTime = Mathf.Min(sustainedNoiseTime + Time.deltaTime, 7.5f);
            float sustainedBoost = isSprinting
                ? Mathf.Lerp(1f, 1.72f, Mathf.Clamp01(sustainedNoiseTime / 2.2f))
                : Mathf.Lerp(1f, 1.36f, Mathf.Clamp01(sustainedNoiseTime / 3.1f));

            if (isCrouching)
            {
                CurrentNoiseRadius = Mathf.Max(0.28f, 0.72f * surfaceMultiplier);
                currentNoiseLabel = "khom " + surfaceLabel;
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f && GameManager.Instance != null && !GameManager.Instance.IsEnded && !GameManager.Instance.EscapeSequenceActive)
                {
                    GameManager.Instance.EmitNoise(transform.position, CurrentNoiseRadius, footstepEventDuration * 0.70f, currentNoiseLabel);
                    PlayFootstep(false, true);
                    footstepTimer = walkStepInterval * 1.55f;
                }
                wasMovingLastFrame = true;
                return;
            }

            float interval = isSprinting ? sprintStepInterval : walkStepInterval;
            float baseRadius = isSprinting ? sprintNoiseRadius : walkNoiseRadius;
            float radius = baseRadius * surfaceMultiplier * sustainedBoost;
            CurrentNoiseRadius = radius;
            currentNoiseLabel = (isSprinting ? "chay " : "di ") + surfaceLabel;

            if (!wasMovingLastFrame)
            {
                footstepTimer = 0f;
            }

            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f && GameManager.Instance != null && !GameManager.Instance.IsEnded && !GameManager.Instance.EscapeSequenceActive)
            {
                GameManager.Instance.EmitNoise(transform.position, radius, footstepEventDuration, currentNoiseLabel);
                PlayFootstep(isSprinting, false);
                footstepTimer = interval;
            }

            wasMovingLastFrame = true;
        }

        private void ResolveSurfaceNoise(bool isSprinting, out string label, out float multiplier)
        {
            Vector3 pos = transform.position;

            if (pos.x >= 23f && pos.x <= 37f && pos.z >= 22f && pos.z <= 31f)
            {
                label = "xuong nuoc";
                multiplier = isSprinting ? 0.22f : 0.18f;
                return;
            }

            if ((pos.x <= -46f && pos.x >= -58f && pos.z >= 1f && pos.z <= 10f) ||
                (pos.x >= 30f && pos.x <= 38f && pos.z >= 22f && pos.z <= 29f))
            {
                label = "san go";
                multiplier = isSprinting ? 1.20f : 1.12f;
                return;
            }

            label = "co ram";
            multiplier = isSprinting ? 1.12f : 1.04f;
        }

        private void ToggleFlashlight()
        {
            flashlightEnabled = !flashlightEnabled;
            if (flashlight != null)
            {
                flashlight.enabled = flashlightEnabled;
            }
        }

        private void EnsureAudio()
        {
            footstepSource = GetComponent<AudioSource>();
            if (footstepSource == null)
            {
                footstepSource = gameObject.AddComponent<AudioSource>();
            }

            footstepSource.playOnAwake = false;
            footstepSource.loop = false;
            footstepSource.spatialBlend = 1f;
            footstepSource.volume = 0.18f;
            footstepSource.minDistance = 2f;
            footstepSource.maxDistance = 14f;

            AudioClip clipA = Resources.Load<AudioClip>("FootstepGrass1");
            AudioClip clipB = Resources.Load<AudioClip>("FootstepGrass2");
            AudioClip clipC = Resources.Load<AudioClip>("FootstepGrass3");
            crouchStepClip = Resources.Load<AudioClip>("FootstepGrassCrouch");

            if (clipA != null && clipB != null)
            {
                footstepClips = clipC != null ? new[] { clipA, clipB, clipC } : new[] { clipA, clipB };
            }
            else
            {
                footstepClips = new[]
                {
                    CreateStepClip("GrassStepA", 0.11f, 0.75f),
                    CreateStepClip("GrassStepB", 0.12f, 0.9f),
                    CreateStepClip("GrassStepC", 0.10f, 1.05f)
                };
            }

            if (crouchStepClip == null)
            {
                crouchStepClip = CreateStepClip("GrassStepCrouch", 0.08f, 0.55f);
            }
        }

        private void PlayFootstep(bool isSprinting, bool isCrouchStep)
        {
            if (footstepSource == null || !isActiveAndEnabled)
            {
                return;
            }

            AudioClip clip = isCrouchStep ? crouchStepClip : footstepClips[Random.Range(0, footstepClips.Length)];
            if (clip == null)
            {
                return;
            }

            footstepSource.pitch = isCrouchStep ? Random.Range(0.9f, 1.0f) : (isSprinting ? Random.Range(1.05f, 1.18f) : Random.Range(0.92f, 1.04f));
            footstepSource.PlayOneShot(clip, isCrouchStep ? 0.12f : (isSprinting ? 0.22f : 0.18f));
        }

        private static AudioClip CreateStepClip(string clipName, float duration, float roughness)
        {
            int sampleRate = 22050;
            int length = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Exp(-t * 18f) * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / duration));
                float noise = (Random.value * 2f - 1f) * 0.4f;
                float body = Mathf.Sin(2f * Mathf.PI * (80f + roughness * 20f) * t) * 0.12f;
                data[i] = (noise * 0.22f + body) * envelope;
            }

            AudioClip clip = AudioClip.Create(clipName, length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void EnsureViewHierarchy()
        {
            cameraPivot = transform.Find("CameraPivot");
            if (cameraPivot == null)
            {
                GameObject pivot = new GameObject("CameraPivot");
                cameraPivot = pivot.transform;
                cameraPivot.SetParent(transform);
                cameraPivot.localPosition = new Vector3(0f, standingHeight - 0.15f, 0f);
                cameraPivot.localRotation = Quaternion.identity;
            }

            viewCamera = GetComponentInChildren<Camera>();
            if (viewCamera == null)
            {
                GameObject cameraObject = new GameObject("PlayerCamera");
                cameraObject.transform.SetParent(cameraPivot);
                cameraObject.transform.localPosition = Vector3.zero;
                cameraObject.transform.localRotation = Quaternion.identity;
                viewCamera = cameraObject.AddComponent<Camera>();
                viewCamera.fieldOfView = 68f;
                viewCamera.nearClipPlane = 0.05f;
                cameraObject.AddComponent<AudioListener>();
            }

            flashlight = viewCamera.GetComponentInChildren<Light>();
            if (flashlight == null)
            {
                GameObject lightObject = new GameObject("Flashlight");
                lightObject.transform.SetParent(viewCamera.transform);
                lightObject.transform.localPosition = Vector3.zero;
                lightObject.transform.localRotation = Quaternion.identity;
                flashlight = lightObject.AddComponent<Light>();
                flashlight.type = LightType.Spot;
                flashlight.range = 18f;
                flashlight.intensity = 8f;
                flashlight.spotAngle = 68f;
                flashlight.color = new Color(0.78f, 0.84f, 1f);
                flashlight.shadows = LightShadows.Soft;
            }

            flashlight.enabled = flashlightEnabled;
        }
    }
}
