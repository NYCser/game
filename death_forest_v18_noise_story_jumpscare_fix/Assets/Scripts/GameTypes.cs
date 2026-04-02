using UnityEngine;

namespace HollowManor
{
    public enum ItemType
    {
        None = 0,
        BlueFuse = 1,
        Evidence = 2,
        RedKeycard = 3,
        Noisemaker = 4,
        WardenSeal = 5,
        ConfessionTape = 6,
        CarBattery = 10,
        FanBelt = 11,
        SparkPlugKit = 12,
        SpareWheel = 13,
        NotePage = 14
    }

    public enum ObjectiveStage
    {
        FindCarParts = 0,
        RepairCar = 1,
        Escape = 2,
        Win = 3,
        Lose = 4,

        // Legacy values kept for compatibility with older scripts / HUD assumptions.
        FindBlueFuse = 100,
        RestorePower = 101,
        GatherEvidence = 102,
        FindRedKeycard = 103,
        FindWardenSeal = 104,
        UnlockArchive = 105,
        RetrieveConfessionTape = 106
    }

    public static class GameColors
    {
        public static readonly Color Floor = new Color(0.09f, 0.12f, 0.10f);
        public static readonly Color Wall = new Color(0.14f, 0.18f, 0.16f);
        public static readonly Color Trim = new Color(0.25f, 0.19f, 0.14f);
        public static readonly Color Objective = new Color(0.32f, 0.80f, 0.60f);
        public static readonly Color Evidence = new Color(0.91f, 0.74f, 0.28f);
        public static readonly Color Power = new Color(0.44f, 0.94f, 0.68f);
        public static readonly Color Danger = new Color(0.88f, 0.20f, 0.24f);
        public static readonly Color Neutral = new Color(0.48f, 0.56f, 0.54f);
        public static readonly Color UIBackground = new Color(0.02f, 0.03f, 0.04f, 0.84f);
        public static readonly Color UIPanelSoft = new Color(0.06f, 0.08f, 0.10f, 0.76f);
        public static readonly Color UIPanelBorder = new Color(0.18f, 0.24f, 0.22f, 0.98f);
        public static readonly Color UITextPrimary = new Color(0.95f, 0.98f, 1.0f);
        public static readonly Color UITextSecondary = new Color(0.84f, 0.89f, 0.95f);
        public static readonly Color UIWarning = new Color(1.0f, 0.86f, 0.36f);
        public static readonly Color UIThreat = new Color(0.96f, 0.20f, 0.24f);
        public static readonly Color UIGood = new Color(0.50f, 0.95f, 0.65f);
    }
}
