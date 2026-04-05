using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HollowManor
{
    public sealed class HUDController : MonoBehaviour
    {
        private enum IntroPage
        {
            QuickStart,
            HowTo,
            Story
        }

        private Text titleText;
        private Text introSubtitleText;
        private Text introPageTitleText;
        private Text introHintText;
        private Text introBodyText;
        private Text introControlsText;
        private Text inventoryText;
        private Text promptText;
        private Text toastText;
        private Text endingTitleText;
        private Text endingBodyText;
        private Image overlay;
        private Image introHaze;
        private Image endingBackdrop;
        private Image crosshair;
        private CanvasGroup introGroup;
        private CanvasGroup promptGroup;
        private CanvasGroup toastGroup;
        private CanvasGroup endingGroup;
        private CanvasGroup inventoryGroup;
        private Button playButton;
        private Button howToButton;
        private Button storyButton;
        private Button quitButton;
        private Button backButton;
        private RectTransform introInfoPanelRect;
        private RectTransform introTitleRect;
        private RectTransform introHintRect;
        private RectTransform introBodyRect;
        private RectTransform introControlsRect;
        private IntroPage introPage = IntroPage.QuickStart;
        private bool crashSplashActive = true;

        public static HUDController Create(Transform parent)
        {
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            GameObject canvasObject = new GameObject("HUD");
            canvasObject.transform.SetParent(parent, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            HUDController hud = canvasObject.AddComponent<HUDController>();
            hud.Build(canvasObject.transform);
            return hud;
        }

        private void Update()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            if (GameManager.Instance.IntroActive)
            {
                if (crashSplashActive)
                {
                    if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                    {
                        ShowMenuFromSplash();
                    }
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        QuitGame();
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) ShowQuickStartPage();
                    if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) ShowHowToPage();
                    if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) ShowStoryPage();
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) OnPlayPressed();
                    if ((Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape)) && introPage != IntroPage.QuickStart)
                    {
                        ShowQuickStartPage();
                    }
                    else if (Input.GetKeyDown(KeyCode.Q))
                    {
                        QuitGame();
                    }
                }
            }

            Refresh(GameManager.Instance);
        }

        public void Refresh(GameManager gameManager)
        {
            if (gameManager == null)
            {
                return;
            }

            if (inventoryText != null)
            {
                inventoryText.text = gameManager.GetInventoryText();
            }

            if (introBodyText != null)
            {
                if (crashSplashActive)
                {
                    introBodyText.text = "MUA XE TOAC DAU XE KHI BAN LAO QUA DOAN DUONG DAT TRON. KHI MO MAT, DEN XE CHAP CHON, SUONG BO KINH LOI RA, BON LINH KIEN QUAN TRONG BI THAT LAC KHAP KHU RUNG.\n\n" +
                                         "MOT GIAT TIENG CUOI XA XA VANG LEN GAN ME RUNG. CO THU GI DO DANG LAN THEO HOI MAY NONG CUA CHIEC XE.\n\n" +
                                         "NHAN PHIM BAT KY DE TIEP TUC. SAU DO CHON PLAY DE BUOC XUONG XE VA BAT DAU SONG SOT.";
                    if (introPageTitleText != null) introPageTitleText.text = "THE ACCIDENT";
                    if (introHintText != null) introHintText.text = "Xe nan nam giua rung. Moi loi tho cua ban deu co the bi nghe thay.";
                    if (introControlsText != null) introControlsText.text = "[ENTER / SPACE / CLICK] TIEP TUC";
                }
                else
                {
                    switch (introPage)
                    {
                        case IntroPage.HowTo:
                            introBodyText.text = gameManager.GetHowToText();
                            if (introPageTitleText != null) introPageTitleText.text = "SURVIVAL GUIDE";
                            if (introHintText != null) introHintText.text = "Nghe am thanh, tranh gay on va cat tam nhin cua no.";
                            if (introControlsText != null) introControlsText.text = "[B/ESC] BACK   [1] PLAY   [3] STORY   [Q] QUIT";
                            break;
                        case IntroPage.Story:
                            introBodyText.text = gameManager.GetLoreText();
                            if (introPageTitleText != null) introPageTitleText.text = "THE STORY";
                            if (introHintText != null) introHintText.text = "Chi co mot hon ma that. Phan con lai la rung dang du ban vao noi chet.";
                            if (introControlsText != null) introControlsText.text = "[B/ESC] BACK   [1] PLAY   [2] HOW TO   [Q] QUIT";
                            break;
                        default:
                            introBodyText.text = gameManager.GetMenuIntroText();
                            if (introPageTitleText != null) introPageTitleText.text = "BEGIN THE NIGHT";
                            if (introHintText != null) introHintText.text = "Nhan Play, Enter hoac Space de bat dau ngay.";
                            if (introControlsText != null) introControlsText.text = "[1] PLAY   [2] HOW TO   [3] STORY   [Q] QUIT";
                            break;
                    }
                }
            }

            if (toastText != null)
            {
                toastText.text = gameManager.GetToastMessage();
            }

            if (introGroup != null)
            {
                float target = gameManager.IntroActive ? 1f : 0f;
                introGroup.alpha = Mathf.MoveTowards(introGroup.alpha, target, Time.deltaTime * 2.5f);
                introGroup.blocksRaycasts = introGroup.alpha > 0.01f;
                introGroup.interactable = introGroup.alpha > 0.01f;
            }

            if (inventoryGroup != null)
            {
                float target = (!gameManager.IntroActive && !gameManager.IsEnded && !gameManager.EscapeSequenceActive) ? 1f : 0f;
                inventoryGroup.alpha = Mathf.MoveTowards(inventoryGroup.alpha, target, Time.deltaTime * 4f);
            }

            if (promptGroup != null)
            {
                bool hasPrompt = !string.IsNullOrWhiteSpace(promptText != null ? promptText.text : string.Empty) && !gameManager.IntroActive && !gameManager.IsEnded && !gameManager.EscapeSequenceActive;
                float target = hasPrompt ? 1f : 0f;
                promptGroup.alpha = Mathf.MoveTowards(promptGroup.alpha, target, Time.deltaTime * 8f);
            }

            if (toastGroup != null)
            {
                bool hasToast = !string.IsNullOrWhiteSpace(toastText != null ? toastText.text : string.Empty) && !gameManager.IntroActive;
                float target = hasToast ? 1f : 0f;
                toastGroup.alpha = Mathf.MoveTowards(toastGroup.alpha, target, Time.deltaTime * 6f);
            }

            if (overlay != null)
            {
                Color color = Color.black;
                color.a = Mathf.Lerp(0.05f, 0.19f, gameManager.ThreatLevel);
                if (gameManager.IntroActive)
                {
                    color = new Color(0.02f, 0.0f, 0.0f, 0.52f);
                }
                else if (gameManager.Stage == ObjectiveStage.Win)
                {
                    color = new Color(0.05f, 0.18f, 0.08f, 0.36f);
                }
                else if (gameManager.Stage == ObjectiveStage.Lose)
                {
                    color = new Color(0.05f, 0.0f, 0.0f, 0.66f);
                }
                overlay.color = color;
            }

            if (introHaze != null)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 1.35f);
                float alpha = gameManager.IntroActive ? Mathf.Lerp(0.10f, 0.18f, pulse) : 0.04f;
                introHaze.color = new Color(0.22f, 0.02f, 0.04f, alpha);
            }

            if (crosshair != null)
            {
                Color c = Color.Lerp(new Color(0.42f, 0.78f, 0.62f, 0.8f), new Color(1f, 0.25f, 0.24f, 0.95f), gameManager.ThreatLevel);
                if (gameManager.IntroActive || gameManager.IsEnded || gameManager.EscapeSequenceActive)
                {
                    c.a = 0f;
                }
                crosshair.color = c;
            }

            if (endingGroup != null)
            {
                float target = gameManager.IsEnded ? 1f : 0f;
                endingGroup.alpha = Mathf.MoveTowards(endingGroup.alpha, target, Time.deltaTime * 4f);
                endingGroup.blocksRaycasts = endingGroup.alpha > 0.01f;
                if (endingTitleText != null)
                {
                    endingTitleText.text = gameManager.GetEndingTitle();
                    endingTitleText.color = gameManager.Stage == ObjectiveStage.Win ? new Color(0.76f, 1f, 0.82f) : new Color(1f, 0.74f, 0.64f);
                }
                if (endingBodyText != null)
                {
                    endingBodyText.text = gameManager.GetEndingBody();
                }
                if (endingBackdrop != null)
                {
                    endingBackdrop.color = gameManager.Stage == ObjectiveStage.Win
                        ? new Color(0.08f, 0.20f, 0.11f, 0.94f)
                        : new Color(0.08f, 0.02f, 0.02f, 0.96f);
                }
            }

            bool modalPage = !crashSplashActive && introPage != IntroPage.QuickStart;

            if (introInfoPanelRect != null)
            {
                Vector2 targetSize = new Vector2(1280f, 760f);
                Vector2 targetPos = new Vector2(280f, -220f);
                if (crashSplashActive)
                {
                    targetSize = new Vector2(1700f, 760f);
                    targetPos = new Vector2(0f, -212f);
                }
                else if (modalPage)
                {
                    targetSize = new Vector2(1680f, 860f);
                    targetPos = new Vector2(0f, -214f);
                }

                introInfoPanelRect.sizeDelta = Vector2.Lerp(introInfoPanelRect.sizeDelta, targetSize, Time.unscaledDeltaTime * 7f);
                introInfoPanelRect.anchoredPosition = Vector2.Lerp(introInfoPanelRect.anchoredPosition, targetPos, Time.unscaledDeltaTime * 7f);
            }

            if (introPageTitleText != null) introPageTitleText.fontSize = modalPage ? 38 : 32;
            if (introHintText != null) introHintText.fontSize = modalPage ? 24 : 20;
            if (introBodyText != null)
            {
                introBodyText.fontSize = crashSplashActive ? 30 : (modalPage ? 30 : 25);
                introBodyText.resizeTextForBestFit = true;
                introBodyText.resizeTextMinSize = modalPage ? 24 : 20;
                introBodyText.resizeTextMaxSize = modalPage ? 34 : 28;
            }
            if (introControlsText != null) introControlsText.fontSize = modalPage ? 22 : 18;

            if (introTitleRect != null)
            {
                introTitleRect.anchoredPosition = modalPage ? new Vector2(0f, -20f) : new Vector2(0f, -20f);
                introTitleRect.sizeDelta = modalPage ? new Vector2(1260f, 52f) : new Vector2(1120f, 44f);
            }

            if (introHintRect != null)
            {
                introHintRect.anchoredPosition = modalPage ? new Vector2(0f, -74f) : new Vector2(0f, -66f);
                introHintRect.sizeDelta = modalPage ? new Vector2(1260f, 42f) : new Vector2(1120f, 40f);
            }

            if (introBodyRect != null)
            {
                introBodyRect.anchoredPosition = modalPage ? new Vector2(0f, -126f) : new Vector2(0f, -118f);
                introBodyRect.sizeDelta = modalPage ? new Vector2(1340f, 560f) : new Vector2(1160f, 520f);
            }

            if (introControlsRect != null)
            {
                introControlsRect.anchoredPosition = modalPage ? new Vector2(0f, -722f) : new Vector2(0f, -598f);
                introControlsRect.sizeDelta = modalPage ? new Vector2(1340f, 42f) : new Vector2(1160f, 38f);
            }

            if (playButton != null) playButton.gameObject.SetActive(!crashSplashActive && !modalPage);
            if (howToButton != null) howToButton.gameObject.SetActive(!crashSplashActive && !modalPage);
            if (storyButton != null) storyButton.gameObject.SetActive(!crashSplashActive && !modalPage);
            if (quitButton != null) quitButton.gameObject.SetActive(!crashSplashActive && !modalPage);
            if (backButton != null) backButton.gameObject.SetActive(modalPage);

            RefreshButtonState();
        }

        public void SetPrompt(string message)
        {
            if (promptText != null)
            {
                promptText.text = message;
            }
        }

        private void Build(Transform root)
        {
            Font font = LoadBuiltinFont();

            overlay = CreateFullscreenImage(root, "Overlay", new Color(0f, 0f, 0f, 0.08f));
            introHaze = CreateFullscreenImage(root, "IntroHaze", new Color(0.22f, 0.02f, 0.04f, 0.10f));
            introHaze.transform.SetAsFirstSibling();

            Transform inventoryPanel = CreatePanel(root, "InventoryPanel", new Vector2(-28f, -26f), new Vector2(330f, 220f), new Vector2(1f, 1f), new Color(0.02f, 0.02f, 0.03f, 0.76f), true);
            inventoryGroup = inventoryPanel.gameObject.AddComponent<CanvasGroup>();
            AddGraphicOutline(inventoryPanel.GetComponent<Image>(), new Color(0.46f, 0.08f, 0.10f, 0.78f), new Vector2(1f, -1f));
            AddGraphicShadow(inventoryPanel.GetComponent<Image>(), new Color(0f, 0f, 0f, 0.55f), new Vector2(0f, -4f));
            inventoryText = CreateText(inventoryPanel, "Inventory", font, 28, TextAnchor.UpperLeft, new Vector2(18f, -16f), new Vector2(294f, 186f));
            inventoryText.color = new Color(0.92f, 0.95f, 0.98f);
            AddOutline(inventoryText);

            Transform promptPanel = CreatePanel(root, "PromptPanel", new Vector2(0f, 36f), new Vector2(860f, 70f), new Vector2(0.5f, 0f), new Color(0.02f, 0.02f, 0.03f, 0.82f), true);
            promptGroup = promptPanel.gameObject.AddComponent<CanvasGroup>();
            promptGroup.alpha = 0f;
            AddGraphicOutline(promptPanel.GetComponent<Image>(), new Color(0.50f, 0.10f, 0.12f, 0.72f), new Vector2(1f, -1f));
            promptText = CreateText(promptPanel, "Prompt", font, 30, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(820f, 56f));
            promptText.color = new Color(1f, 0.96f, 0.94f);
            AddOutline(promptText);

            Transform toastPanel = CreatePanel(root, "ToastPanel", new Vector2(0f, -160f), new Vector2(980f, 80f), new Vector2(0.5f, 0.5f), new Color(0.03f, 0.03f, 0.04f, 0.84f), true);
            toastGroup = toastPanel.gameObject.AddComponent<CanvasGroup>();
            toastGroup.alpha = 0f;
            AddGraphicOutline(toastPanel.GetComponent<Image>(), new Color(0.58f, 0.12f, 0.14f, 0.74f), new Vector2(1f, -1f));
            toastText = CreateText(toastPanel, "Toast", font, 28, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(920f, 60f));
            toastText.color = new Color(1f, 0.95f, 0.86f);
            AddOutline(toastText);

            Transform introPanel = CreatePanel(root, "IntroPanel", Vector2.zero, new Vector2(1820f, 1000f), new Vector2(0.5f, 0.5f), new Color(0.02f, 0.01f, 0.02f, 0.90f), true);
            introGroup = introPanel.gameObject.AddComponent<CanvasGroup>();
            AddGraphicOutline(introPanel.GetComponent<Image>(), new Color(0.55f, 0.09f, 0.11f, 0.92f), new Vector2(2f, -2f));
            AddGraphicShadow(introPanel.GetComponent<Image>(), new Color(0f, 0f, 0f, 0.75f), new Vector2(0f, -8f));

            Transform topBlood = CreatePanel(introPanel, "TopBlood", new Vector2(0f, -22f), new Vector2(1760f, 120f), new Vector2(0.5f, 1f), new Color(0.24f, 0.02f, 0.05f, 0.98f), false);
            AddGraphicOutline(topBlood.GetComponent<Image>(), new Color(0.78f, 0.18f, 0.22f, 0.80f), new Vector2(1f, -1f));
            AddGraphicShadow(topBlood.GetComponent<Image>(), new Color(0f, 0f, 0f, 0.45f), new Vector2(0f, -4f));

            titleText = CreateText(topBlood, "Title", font, 68, TextAnchor.MiddleCenter, new Vector2(0f, -12f), new Vector2(1460f, 74f));
            titleText.text = "DEATH FOREST";
            titleText.color = new Color(0.98f, 0.92f, 0.92f);
            AddOutline(titleText, 2);

            introSubtitleText = CreateText(introPanel, "Subtitle", font, 26, TextAnchor.UpperCenter, new Vector2(0f, -146f), new Vector2(1700f, 44f));
            introSubtitleText.text = "MOT HON MA THAT - NHIEU AO ANH - CHI CO CHIEC XE LA DUONG SONG";
            introSubtitleText.color = new Color(0.82f, 0.72f, 0.72f);
            AddOutline(introSubtitleText);

            CreateMenuDivider(introPanel, new Vector2(0f, -190f), new Vector2(1760f, 3f), new Color(0.52f, 0.10f, 0.12f, 0.85f));
            CreateMenuDivider(introPanel, new Vector2(0f, -824f), new Vector2(1760f, 2f), new Color(0.40f, 0.08f, 0.10f, 0.52f));

            playButton = CreateMenuButton(introPanel, font, "Play", new Vector2(-700f, -280f), new Color(0.23f, 0.04f, 0.06f, 1f), OnPlayPressed);
            howToButton = CreateMenuButton(introPanel, font, "How To", new Vector2(-700f, -414f), new Color(0.10f, 0.10f, 0.12f, 1f), ShowHowToPage);
            storyButton = CreateMenuButton(introPanel, font, "Story", new Vector2(-700f, -548f), new Color(0.10f, 0.10f, 0.12f, 1f), ShowStoryPage);
            quitButton = CreateMenuButton(introPanel, font, "Quit", new Vector2(-700f, -682f), new Color(0.08f, 0.08f, 0.09f, 1f), QuitGame);

            Transform infoPanel = CreatePanel(introPanel, "InfoPanel", new Vector2(240f, -220f), new Vector2(1360f, 760f), new Vector2(0.5f, 1f), new Color(0.05f, 0.06f, 0.08f, 0.94f), true);
            introInfoPanelRect = infoPanel.GetComponent<RectTransform>();
            AddGraphicOutline(infoPanel.GetComponent<Image>(), new Color(0.58f, 0.12f, 0.15f, 0.84f), new Vector2(1f, -1f));
            AddGraphicShadow(infoPanel.GetComponent<Image>(), new Color(0f, 0f, 0f, 0.55f), new Vector2(0f, -6f));

            Transform infoTopStrip = CreatePanel(infoPanel, "InfoTopStrip", new Vector2(0f, -10f), new Vector2(1240f, 52f), new Vector2(0.5f, 1f), new Color(0.12f, 0.02f, 0.04f, 0.94f), false);
            AddGraphicOutline(infoTopStrip.GetComponent<Image>(), new Color(0.80f, 0.18f, 0.20f, 0.56f), new Vector2(1f, -1f));

            introPageTitleText = CreateText(infoPanel, "InfoTitle", font, 34, TextAnchor.UpperLeft, new Vector2(0f, -18f), new Vector2(900f, 44f));
            introTitleRect = introPageTitleText.rectTransform;
            introPageTitleText.color = new Color(0.98f, 0.90f, 0.90f);
            AddOutline(introPageTitleText);

            introHintText = CreateText(infoPanel, "InfoHint", font, 20, TextAnchor.UpperLeft, new Vector2(0f, -60f), new Vector2(900f, 38f));
            introHintRect = introHintText.rectTransform;
            introHintText.color = new Color(0.88f, 0.80f, 0.80f);
            AddOutline(introHintText);

            CreateMenuDivider(infoPanel, new Vector2(0f, -100f), new Vector2(1240f, 2f), new Color(0.52f, 0.10f, 0.12f, 0.60f));

            introBodyText = CreateText(infoPanel, "IntroBody", font, 24, TextAnchor.UpperLeft, new Vector2(0f, -112f), new Vector2(1040f, 460f));
            introBodyRect = introBodyText.rectTransform;
            introBodyText.color = new Color(0.96f, 0.96f, 0.98f);
            AddOutline(introBodyText);

            introControlsText = CreateText(infoPanel, "InfoControls", font, 20, TextAnchor.LowerLeft, new Vector2(0f, -520f), new Vector2(1040f, 36f));
            introControlsRect = introControlsText.rectTransform;
            introControlsText.color = new Color(0.94f, 0.72f, 0.72f);
            AddOutline(introControlsText);

            backButton = CreateCornerButton(infoPanel, font, "Back", new Vector2(22f, -16f), new Vector2(170f, 56f), ShowQuickStartPage);
            backButton.gameObject.SetActive(false);

            Transform endingPanel = CreatePanel(root, "EndingPanel", Vector2.zero, new Vector2(1180f, 760f), new Vector2(0.5f, 0.5f), new Color(0f, 0f, 0f, 0f), true);
            endingGroup = endingPanel.gameObject.AddComponent<CanvasGroup>();
            endingGroup.alpha = 0f;
            endingBackdrop = endingPanel.GetComponent<Image>();
            endingTitleText = CreateText(endingPanel, "EndingTitle", font, 120, TextAnchor.UpperCenter, new Vector2(0f, -84f), new Vector2(1080f, 130f));
            endingBodyText = CreateText(endingPanel, "EndingBody", font, 40, TextAnchor.UpperCenter, new Vector2(0f, -242f), new Vector2(1000f, 360f));
            endingBodyText.color = Color.white;
            AddOutline(endingTitleText, 4);
            AddOutline(endingBodyText, 2);

            crosshair = CreateCrosshair(root);
        }

        private void RefreshButtonState()
        {
            if (crashSplashActive)
            {
                SetButtonSelected(playButton, false);
                SetButtonSelected(howToButton, false);
                SetButtonSelected(storyButton, false);
                SetButtonSelected(quitButton, false);
                SetButtonSelected(backButton, false);
                return;
            }

            SetButtonSelected(playButton, introPage == IntroPage.QuickStart);
            SetButtonSelected(howToButton, introPage == IntroPage.HowTo);
            SetButtonSelected(storyButton, introPage == IntroPage.Story);
            SetButtonSelected(quitButton, false);
            SetButtonSelected(backButton, introPage != IntroPage.QuickStart);
        }

        private void OnPlayPressed()
        {
            if (crashSplashActive)
            {
                ShowMenuFromSplash();
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.BeginRun();
            }
        }

        private void ShowMenuFromSplash()
        {
            crashSplashActive = false;
            introPage = IntroPage.QuickStart;
        }

        private void ShowQuickStartPage()
        {
            crashSplashActive = false;
            introPage = IntroPage.QuickStart;
        }

        private void ShowHowToPage()
        {
            crashSplashActive = false;
            introPage = IntroPage.HowTo;
        }

        private void ShowStoryPage()
        {
            crashSplashActive = false;
            introPage = IntroPage.Story;
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static Transform CreatePanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Vector2 anchor, Color color, bool raycastTarget)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = panel.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return panel.transform;
        }

        private static Button CreateMenuButton(Transform parent, Font font, string label, Vector2 anchoredPosition, Color color, UnityEngine.Events.UnityAction callback)
        {
            Transform buttonRoot = CreatePanel(parent, label + "Button", anchoredPosition, new Vector2(520f, 98f), new Vector2(0.5f, 1f), color, true);
            Image image = buttonRoot.GetComponent<Image>();
            AddGraphicOutline(image, new Color(0.72f, 0.14f, 0.16f, 0.82f), new Vector2(1f, -1f));
            AddGraphicShadow(image, new Color(0f, 0f, 0f, 0.58f), new Vector2(0f, -5f));

            Button button = buttonRoot.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, new Color(0.85f, 0.14f, 0.18f, 1f), 0.65f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.25f);
            colors.selectedColor = Color.Lerp(color, new Color(0.72f, 0.12f, 0.15f, 1f), 0.60f);
            colors.disabledColor = new Color(0.24f, 0.24f, 0.24f, 0.85f);
            button.colors = colors;
            button.targetGraphic = image;
            button.onClick.AddListener(callback);

            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(buttonRoot, false);
            RectTransform glowRect = glow.AddComponent<RectTransform>();
            glowRect.anchorMin = new Vector2(0.5f, 0.5f);
            glowRect.anchorMax = new Vector2(0.5f, 0.5f);
            glowRect.pivot = new Vector2(0.5f, 0.5f);
            glowRect.anchoredPosition = new Vector2(0f, -2f);
            glowRect.sizeDelta = new Vector2(414f, 22f);
            Image glowImage = glow.AddComponent<Image>();
            glowImage.color = new Color(0.78f, 0.10f, 0.12f, 0.18f);
            glowImage.raycastTarget = false;

            Transform accent = CreatePanel(buttonRoot, "Accent", new Vector2(0f, -8f), new Vector2(404f, 6f), new Vector2(0.5f, 0.5f), new Color(0.92f, 0.22f, 0.24f, 0.50f), false);
            accent.GetComponent<Image>().raycastTarget = false;

            Text text = CreateText(buttonRoot, label + "Text", font, 36, TextAnchor.MiddleCenter, new Vector2(0f, -10f), new Vector2(404f, 56f));
            text.text = label.ToUpperInvariant();
            text.color = new Color(0.96f, 0.92f, 0.92f);
            AddOutline(text);
            return button;
        }

        private static Button CreateCornerButton(Transform parent, Font font, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction callback)
        {
            Transform buttonRoot = CreatePanel(parent, label + "CornerButton", anchoredPosition, size, new Vector2(0f, 1f), new Color(0.12f, 0.02f, 0.04f, 0.96f), true);
            Image image = buttonRoot.GetComponent<Image>();
            AddGraphicOutline(image, new Color(0.78f, 0.14f, 0.18f, 0.82f), new Vector2(1f, -1f));

            Button button = buttonRoot.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.28f, 0.05f, 0.08f, 1f);
            colors.pressedColor = new Color(0.18f, 0.04f, 0.06f, 1f);
            colors.selectedColor = new Color(0.32f, 0.06f, 0.08f, 1f);
            button.colors = colors;
            button.targetGraphic = image;
            button.onClick.AddListener(callback);

            Text text = CreateText(buttonRoot, label + "CornerText", font, 24, TextAnchor.MiddleCenter, new Vector2(0f, 0f), new Vector2(size.x - 16f, size.y - 8f));
            text.text = label.ToUpperInvariant();
            text.color = new Color(1f, 0.92f, 0.92f);
            AddOutline(text);
            return button;
        }

        private static void SetButtonSelected(Button button, bool selected)
        {
            if (button == null)
            {
                return;
            }

            Vector3 targetScale = selected ? new Vector3(1.045f, 1.045f, 1f) : Vector3.one;
            button.transform.localScale = Vector3.Lerp(button.transform.localScale, targetScale, Time.deltaTime * 10f);

            Image image = button.targetGraphic as Image;
            if (image != null)
            {
                image.color = Color.Lerp(image.color, selected ? button.colors.selectedColor : button.colors.normalColor, Time.deltaTime * 12f);
            }

            Transform glow = button.transform.Find("Glow");
            if (glow != null)
            {
                Image glowImage = glow.GetComponent<Image>();
                if (glowImage != null)
                {
                    float pulse = selected ? (0.22f + 0.14f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6.0f))) : 0.10f;
                    glowImage.color = new Color(0.80f, 0.12f, 0.14f, pulse);
                }
            }

            Transform accent = button.transform.Find("Accent");
            if (accent != null)
            {
                Image accentImage = accent.GetComponent<Image>();
                if (accentImage != null)
                {
                    accentImage.color = selected
                        ? new Color(1.0f, 0.28f, 0.30f, 0.90f)
                        : new Color(0.76f, 0.16f, 0.18f, 0.36f);
                }
            }

            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                Color targetText = selected ? new Color(1f, 0.93f, 0.93f) : new Color(0.96f, 0.92f, 0.92f);
                label.color = Color.Lerp(label.color, targetText, Time.deltaTime * 12f);
            }
        }

        private static Font LoadBuiltinFont()
        {
            try
            {
                Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (legacyFont != null)
                {
                    return legacyFont;
                }
            }
            catch
            {
            }

            try
            {
                Font arialFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (arialFont != null)
                {
                    return arialFont;
                }
            }
            catch
            {
            }

            return Font.CreateDynamicFontFromOSFont("Arial", 28);
        }

        private static void CreateMenuDivider(Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject line = new GameObject("Divider");
            line.transform.SetParent(parent, false);
            RectTransform rect = line.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Image image = line.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static Text CreateText(Transform parent, string objectName, Font font, int fontSize, TextAnchor anchor, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = Color.white;
            return text;
        }

        private static Image CreateFullscreenImage(Transform parent, string name, Color color)
        {
            GameObject imageObject = new GameObject(name);
            imageObject.transform.SetParent(parent, false);
            RectTransform rect = imageObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Image CreateCrosshair(Transform root)
        {
            GameObject cross = new GameObject("Crosshair");
            cross.transform.SetParent(root, false);
            RectTransform rect = cross.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(10f, 10f);
            Image image = cross.AddComponent<Image>();
            image.color = new Color(0.46f, 0.90f, 0.72f, 0.82f);
            return image;
        }

        private static void AddOutline(Graphic graphic, int effectDistance = 1)
        {
            AddGraphicOutline(graphic, new Color(0f, 0f, 0f, 0.82f), new Vector2(effectDistance, -effectDistance));
        }

        private static void AddGraphicOutline(Graphic graphic, Color color, Vector2 distance)
        {
            if (graphic == null)
            {
                return;
            }

            Outline outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
        }

        private static void AddGraphicShadow(Graphic graphic, Color color, Vector2 distance)
        {
            if (graphic == null)
            {
                return;
            }

            Shadow shadow = graphic.gameObject.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
        }
    }
}
