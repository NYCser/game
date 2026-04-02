using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HollowManor
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action StateChanged;

        public ObjectiveStage Stage { get; private set; } = ObjectiveStage.FindCarParts;
        public bool HasCarBattery { get; private set; }
        public bool HasFanBelt { get; private set; }
        public bool HasSparkPlugKit { get; private set; }
        public bool HasSpareWheel { get; private set; }
        public int NotesCollected { get; private set; }
        public int NotesFoundTarget => 4;
        public bool CarRepaired { get; private set; }
        public bool IsEnded => Stage == ObjectiveStage.Win || Stage == ObjectiveStage.Lose;
        public bool IntroActive { get; private set; } = true;
        public bool EscapeSequenceActive { get; private set; }

        public int CarPartsCollected
        {
            get
            {
                int total = 0;
                if (HasCarBattery) total++;
                if (HasFanBelt) total++;
                if (HasSparkPlugKit) total++;
                if (HasSpareWheel) total++;
                return total;
            }
        }

        public int CarPartsRequired => 4;
        public bool HasAllCarParts => CarPartsCollected >= CarPartsRequired;

        // Legacy compatibility fields kept so old scripts still compile.
        public bool HasBlueFuse => false;
        public bool PowerRestored => CarRepaired;
        public int EvidenceCollected => NotesCollected;
        public int EvidenceRequired => NotesFoundTarget;
        public bool HasRedKeycard => false;
        public bool HasWardenSeal => false;
        public bool HasConfessionTape => false;
        public bool ArchiveUnlocked => CarRepaired;

        public PlayerMotor Player { get; private set; }
        public HUDController Hud { get; private set; }
        public float ThreatLevel { get; private set; }
        public string ThreatText { get; private set; } = "YEN LANG";
        public float IntroTimer { get; private set; } = 999f;
        public string CurrentAreaName { get; private set; } = "XE BI NAN";
        public string LoseReason { get; private set; } = string.Empty;

        public bool HasActiveNoiseEvent => activeNoiseEventTimer > 0f;
        public int ActiveNoiseEventId { get; private set; }
        public Vector3 ActiveNoiseEventPosition { get; private set; }
        public float ActiveNoiseEventRadius { get; private set; }
        public string ActiveNoiseEventLabel { get; private set; } = string.Empty;

        private string toastMessage = string.Empty;
        private float toastTimer;
        private float activeNoiseEventTimer;
        private float reportedThreat;
        private float doorNoisePressure;
        private string reportedThreatText = string.Empty;
        private ObjectiveStage announcedStage = (ObjectiveStage)(-1);
        private string lastAreaName = string.Empty;
        private readonly HashSet<string> visitedAreas = new HashSet<string>();
        private readonly List<string> collectedLore = new List<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (IntroActive)
            {
                IntroTimer = 999f;
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    BeginRun();
                }
            }
            else if (IntroTimer > 0f)
            {
                IntroTimer = Mathf.Max(0f, IntroTimer - Time.deltaTime);
            }

            if (toastTimer > 0f)
            {
                toastTimer -= Time.deltaTime;
                if (toastTimer <= 0f)
                {
                    toastMessage = string.Empty;
                    NotifyStateChanged();
                }
            }

            if (activeNoiseEventTimer > 0f)
            {
                activeNoiseEventTimer = Mathf.Max(0f, activeNoiseEventTimer - Time.deltaTime);
                if (activeNoiseEventTimer <= 0f)
                {
                    ActiveNoiseEventRadius = 0f;
                    ActiveNoiseEventLabel = string.Empty;
                }
            }

            doorNoisePressure = Mathf.MoveTowards(doorNoisePressure, 0f, Time.deltaTime * 0.28f);

            if (!IntroActive)
            {
                UpdateAreaTracking();
                AnnounceStageIfNeeded();
            }
        }

        private void LateUpdate()
        {
            if (IsEnded || IntroActive || EscapeSequenceActive)
            {
                ThreatLevel = Mathf.MoveTowards(ThreatLevel, 0f, Time.deltaTime * 2f);
                reportedThreat = 0f;
                reportedThreatText = string.Empty;
                return;
            }

            ThreatLevel = Mathf.MoveTowards(ThreatLevel, reportedThreat, Time.deltaTime * 3.4f);
            float passiveDecay = ThreatLevel > 0.55f ? 0.05f : 0.09f;
            ThreatLevel = Mathf.Max(ThreatLevel - Time.deltaTime * passiveDecay, reportedThreat);
            ThreatText = string.IsNullOrEmpty(reportedThreatText) ? "YEN LANG" : reportedThreatText;
            reportedThreat = 0f;
            reportedThreatText = string.Empty;
        }

        public void BindPlayer(PlayerMotor player)
        {
            Player = player;
            NotifyStateChanged();
        }

        public void BindHud(HUDController hud)
        {
            Hud = hud;
            NotifyStateChanged();
        }

        public void BeginRun()
        {
            if (!IntroActive)
            {
                return;
            }

            IntroActive = false;
            IntroTimer = 0.85f;
            ShowToast("Tim 4 linh kien, giu im lang neu muon song. Canh bao chu yeu den bang am thanh.", 3.4f);
            NotifyStateChanged();
        }

        public void RegisterPickup(ItemType itemType, string displayName)
        {
            switch (itemType)
            {
                case ItemType.CarBattery:
                    if (!HasCarBattery)
                    {
                        HasCarBattery = true;
                        collectedLore.Add("Ac quy van con am. Co nguoi da thao no ra roi keo vao khu rung.");
                        ShowToast("Da nhat: ac quy", 2.2f);
                    }
                    break;
                case ItemType.FanBelt:
                    if (!HasFanBelt)
                    {
                        HasFanBelt = true;
                        collectedLore.Add("Day curoa dính bun nuoc. Dau vet keo lê dan ve mot mieng tho khong co tren ban do.");
                        ShowToast("Da nhat: day curoa", 2.2f);
                    }
                    break;
                case ItemType.SparkPlugKit:
                    if (!HasSparkPlugKit)
                    {
                        HasSparkPlugKit = true;
                        collectedLore.Add("Bo bugi duoc dat gon trong choi cu, nhu the co ai do muon ban phai quay lai day them mot lan nua.");
                        ShowToast("Da nhat: bo bugi", 2.2f);
                    }
                    break;
                case ItemType.SpareWheel:
                    if (!HasSpareWheel)
                    {
                        HasSpareWheel = true;
                        collectedLore.Add("Banh du phong keo theo dau tay mau cu. Chu nhat ky cu cung mat o day.");
                        ShowToast("Da nhat: banh du phong", 2.2f);
                    }
                    break;
                case ItemType.NotePage:
                case ItemType.Evidence:
                    NotesCollected = Mathf.Clamp(NotesCollected + 1, 0, NotesFoundTarget);
                    collectedLore.Add(GetLoreForNote(NotesCollected));
                    ShowToast("Da nhat: trang nhat ky " + NotesCollected + "/" + NotesFoundTarget, 2.4f);
                    break;
                default:
                    ShowToast("Da lay " + displayName + ".", 2.0f);
                    break;
            }

            EvaluateStage();
        }

        public void RepairCar()
        {
            if (CarRepaired)
            {
                ShowToast("Xe da duoc rap lai.");
                return;
            }

            if (!HasAllCarParts)
            {
                ShowToast("Ban chua du linh kien de sua xe.");
                return;
            }

            CarRepaired = true;
            ShowToast("Tieng kim loai vang len giua rung. Thu do chac chan da nghe thay.", 4.0f);
            EmitNoise(new Vector3(-20f, 0f, -41f), 18f, 1.2f, "sua xe");
            EvaluateStage();
        }

        public bool CanRepairCar(out string reason)
        {
            if (CarRepaired)
            {
                reason = "Xe da san sang.";
                return true;
            }

            List<string> missing = new List<string>();
            if (!HasCarBattery) missing.Add("ac quy");
            if (!HasFanBelt) missing.Add("day curoa");
            if (!HasSparkPlugKit) missing.Add("bo bugi");
            if (!HasSpareWheel) missing.Add("banh du phong");

            if (missing.Count > 0)
            {
                reason = "Thieu: " + string.Join(", ", missing) + ".";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public void RestorePower() => RepairCar();
        public bool CanUnlockArchive(out string reason) => CanRepairCar(out reason);
        public void UnlockArchive() => RepairCar();

        public void EmitNoise(Vector3 position, float radius, float duration, string label = "")
        {
            if (IsEnded || IntroActive || radius <= 0f || duration <= 0f)
            {
                return;
            }

            string normalizedLabel = (label ?? string.Empty).ToLowerInvariant();
            if (normalizedLabel.Contains("cua"))
            {
                doorNoisePressure = Mathf.Clamp01(doorNoisePressure + 0.34f);
                float pressureScale = Mathf.Lerp(1.12f, 1.70f, doorNoisePressure);
                radius *= pressureScale;
                duration *= Mathf.Lerp(1.05f, 1.42f, doorNoisePressure);
            }
            else if (normalizedLabel.Contains("sua xe") || normalizedLabel.Contains("dong co"))
            {
                radius *= 1.08f;
                duration *= 1.10f;
            }

            ActiveNoiseEventId++;
            ActiveNoiseEventPosition = position;
            ActiveNoiseEventRadius = radius;
            ActiveNoiseEventLabel = label ?? string.Empty;
            activeNoiseEventTimer = duration;
        }

        public void PlayerCaught(string captorName = "Hon ma")
        {
            if (IsEnded || EscapeSequenceActive)
            {
                return;
            }

            if (Player != null)
            {
                LevelFactory.RecordRecentDangerPosition(Player.transform.position);
            }

            Stage = ObjectiveStage.Lose;
            ThreatLevel = 1f;
            ThreatText = "BI NUOT MAT";
            LoseReason = string.IsNullOrWhiteSpace(captorName)
                ? "Ban da bi mot linh hon trong rung bat kip."
                : captorName + " da bat kip ban giua rung.";
            ShowToast(LoseReason + "\nGame se tu respawn ngau nhien sau vai giay. Ban van co the nhan R de choi lai ngay.", 999f);
            if (Player != null)
            {
                Player.OnCaught();
            }

            NotifyStateChanged();
        }

        public void PlayerEscaped()
        {
            if (IsEnded)
            {
                return;
            }

            EscapeSequenceActive = false;
            Stage = ObjectiveStage.Win;
            ThreatLevel = 0f;
            ThreatText = "THOAT KHOI RUNG";
            ShowToast("Ban da lao len xe, no may va thoat khoi khu rung. Nhan R de choi lai.", 999f);
            if (Player != null)
            {
                Player.SetInputBlocked(true);
            }

            NotifyStateChanged();
        }

        public void BeginCarEscapeSequence(Vector3 doorPosition, Vector3 seatPosition, Quaternion seatRotation)
        {
            if (IsEnded || EscapeSequenceActive)
            {
                return;
            }

            StartCoroutine(CarEscapeSequenceRoutine(doorPosition, seatPosition, seatRotation));
        }

        private IEnumerator CarEscapeSequenceRoutine(Vector3 doorPosition, Vector3 seatPosition, Quaternion seatRotation)
        {
            EscapeSequenceActive = true;
            ThreatLevel = 0f;
            ThreatText = "NO MAY";
            ShowToast("Ban lao ve phia cua xe...", 1.1f);
            NotifyStateChanged();

            if (Player != null)
            {
                yield return StartCoroutine(Player.PlayCarEscapeOutro(doorPosition, seatPosition, seatRotation));
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }

            EmitNoise(seatPosition, 20f, 1.8f, "dong co xe");
            ShowToast("Dong co gao len. Ban bam chat cua xe va lao khoi khu rung...", 1.8f);
            NotifyStateChanged();
            yield return new WaitForSeconds(1.35f);

            PlayerEscaped();
        }

        public void ShowToast(string message, float duration = 2.5f)
        {
            toastMessage = message;
            toastTimer = duration;
            NotifyStateChanged();
        }

        public void ReportThreat(float level, string label)
        {
            if (IsEnded || IntroActive || EscapeSequenceActive)
            {
                return;
            }

            if (level > reportedThreat)
            {
                reportedThreat = Mathf.Clamp01(level);
                reportedThreatText = label;
            }
        }

        public string GetToastMessage() => toastMessage;

        public string GetObjectiveText()
        {
            switch (Stage)
            {
                case ObjectiveStage.FindCarParts:
                    return "Tim du 4 linh kien roi quay lai xe.";
                case ObjectiveStage.RepairCar:
                    return "Lap linh kien vao xe.";
                case ObjectiveStage.Escape:
                    return "Len xe va no may.";
                case ObjectiveStage.Win:
                    return "Ban da thoat.";
                case ObjectiveStage.Lose:
                    return "Ban da bi bat. Nhan R de choi lai.";
                default:
                    return string.Empty;
            }
        }

        public string GetInventoryText()
        {
            return "VAT PHAM DA NHAT\n" +
                   "- Ac quy: " + (HasCarBattery ? "Co" : "Chua") + "\n" +
                   "- Day curoa: " + (HasFanBelt ? "Co" : "Chua") + "\n" +
                   "- Bo bugi: " + (HasSparkPlugKit ? "Co" : "Chua") + "\n" +
                   "- Banh du phong: " + (HasSpareWheel ? "Co" : "Chua") + "\n" +
                   "- Nhat ky: " + NotesCollected + "/" + NotesFoundTarget;
        }

        public string GetMenuIntroText()
        {
            return "PLAY\n\n" +
                   "Ban vua tinh lai sau mot vu lao xe xuong doi bun. Chiec xe khong no may, 4 linh kien quan trong bi vang khoi than xe va roi rai rac quanh nhung diem nguoi tung dung chan trong rung. Thu dang di theo ban khong can thay mat - no lan den theo tung tieng dong lon nhat.\n\n" +
                   "- PLAY: bat dau dem tron khoi khu rung\n" +
                   "- HOW TO: xem dieu khien, muc tieng on va meo nap\n" +
                   "- STORY: doc doan mo dau va ly do khu rung nay khong binh thuong\n" +
                   "- QUIT: thoat game hoac dung Play Mode trong Editor\n\n" +
                   "Muc tieu: nhat du 4 linh kien, quay lai xe, lap lai va no may truoc khi no toi noi co tieng dong cua ban.";
        }

        public string GetHowToText()
        {
            return "HOW TO PLAY\n\n" +
                   "WASD - di chuyen\n" +
                   "Shift - chay nhanh, nhanh hon nhung de lai dau vet am thanh lon\n" +
                   "Ctrl/C - di khom, cham hon nhung kho bi nghe thay hon\n" +
                   "F - bat/tat den pin\n" +
                   "E - mo cua, nhat do, tron, sua xe, chui vao than go / tu go / tup leu do\n" +
                   "Esc - tha chuot khi dang choi\n\n" +
                   "MEO SONG SOT\n" +
                   "- Mo cua la muc tieng on de bi lan theo nhat. Mo/dong lien tuc se lam do on cua tang dan. Tiep theo la chay va di tren co ram, san go; xuong nuoc la nho nhat\n" +
                   "- Chay lien tuc giup ban keo gian khoang cach, nhung cung khien no de bat huong hon. Dung len toc khi can cat duoi, roi bo goc hoac nap som\n" +
                   "- Neu dang bi di ma ban chi doi sang di khom, cam giac nguy hiem van con trong vai giay; dung coi la da thoat ngay\n" +
                   "- Khi nap, hay im trong 5-10 giay: co 80 phan tram kha nang no bo qua. Neu da bo qua thi no se rut lui va khong bat xuyen tu/cho nap\n" +
                   "- Game se tu respawn ngau nhien sau khi bi bat, nen vi tri ban va ma se khong giong nhau moi lan\n" +
                   "- Than go rong, tu go va tup leu do cho phep nhin xoay xung quanh de do thinh huong.";
        }

        public string GetLoreText()
        {
            return "STORY\n\n" +
                   "Ban di duong tat qua con duong kiem lam cu de kip ve truoc mua, nhung mot than cay do ngang duong da ep xe lao lech xuong doi bun. Chiec xe khong vo nat, nhung ac quy roi ra, day curoa dut, bo bugi vang khoi hop do va banh sau bi bat mat. Dau vet va vat dung quanh cac diem dung chan cho thay linh kien da bi va dap va keo di quanh rung.\n\n" +
                   "Noi nay tung la loat diem nghi cua kiem lam va nguoi di rung: choi gac, tup leu do, than go rong va canh suoi. Nhung sau nhieu vu mat tich, ai cung chi nhac den Death Forest bang mot quy tac: dung gay tieng dong lon neu van muon thay duong tro ra.\n\n" +
                   "Thu dang san ban khong can phai thay ban lien tuc. No chi can am thanh: cua bat mo, buoc chan tren go, tieng do sua xe, va cuoi cung la tieng dong co. Nhung trang nhat ky bo lai se noi ro hon vi sao khu rung nay chi im lang truoc khi mot ai do bien mat.";
        }

        public string GetStoryText()
        {
            return GetHowToText() + "\n\n" + GetLoreText();
        }

        public string GetEndingTitle() => Stage == ObjectiveStage.Win ? "THOAT KHOI RUNG" : "BI BAT";

        public string GetEndingBody()
        {
            if (Stage == ObjectiveStage.Win)
            {
                string loreLine = collectedLore.Count > 0 ? "\n\nManh moi cuoi: " + collectedLore[collectedLore.Count - 1] : string.Empty;
                return "Ban da lap du 4 linh kien, no may va re khoi con duong tat truoc khi hon ma kip keo toi." + loreLine + "\n\nNhan R de choi lai.";
            }

            if (Stage == ObjectiveStage.Lose)
            {
                return "Tieng thet cuoi cung den sat ben tai truoc khi ban kip ve toi xe.\nLan sau: di khom nhieu hon, dong cua nhanh va nap som khi nghe tieng rit lon dan.\n\nNhan R de choi lai.";
            }

            return string.Empty;
        }

        private void EvaluateStage()
        {
            if (IsEnded)
            {
                return;
            }

            if (!HasAllCarParts)
            {
                Stage = ObjectiveStage.FindCarParts;
            }
            else if (!CarRepaired)
            {
                Stage = ObjectiveStage.RepairCar;
            }
            else
            {
                Stage = ObjectiveStage.Escape;
            }

            if (Player != null)
            {
                Player.SetInputBlocked(IsEnded || IntroActive || EscapeSequenceActive);
            }

            NotifyStateChanged();
        }

        private void UpdateAreaTracking()
        {
            if (Player == null)
            {
                return;
            }

            string area = ResolveAreaName(Player.transform.position);
            CurrentAreaName = area;
            if (area != lastAreaName)
            {
                lastAreaName = area;
                if (visitedAreas.Add(area) && !IsEnded)
                {
                    // Polished v5: keep traversal guidance diegetic through audio instead of frequent on-screen alerts.
                }
                NotifyStateChanged();
            }
        }

        private string ResolveAreaName(Vector3 position)
        {
            if (position.z <= -26f)
            {
                return "XE BI NAN";
            }

            if (position.x <= -18f && position.z >= 14f)
            {
                return "MIEU CU";
            }

            if (position.x >= 20f && position.z >= 12f)
            {
                return "KHE SUOI";
            }

            if (position.x <= -20f)
            {
                return "CHOI KIEM LAM";
            }

            if (position.x >= 12f)
            {
                return "TRAI BO HOANG";
            }

            return "LOI MON TOI";
        }

        private static string ResolveAreaHint(string area)
        {
            return string.Empty;
        }

        private void AnnounceStageIfNeeded()
        {
            if (announcedStage == Stage || IsEnded || IntroActive || EscapeSequenceActive)
            {
                return;
            }

            announcedStage = Stage;
            switch (Stage)
            {
                case ObjectiveStage.FindCarParts:
                    ShowToast("Tim 4 linh kien xe bi that lac trong rung.", 3.8f);
                    break;
                case ObjectiveStage.RepairCar:
                    ShowToast("Da du linh kien. Quay lai xe va sua nhanh.", 3.4f);
                    break;
                case ObjectiveStage.Escape:
                    ShowToast("Len xe va no may de thoat.", 3.0f);
                    break;
            }
        }

        private static string GetLoreForNote(int index)
        {
            switch (index)
            {
                case 1:
                    return "Trang 1: 'Neu nghe tieng tre con cuoi trong rung, dung quay lai.'";
                case 2:
                    return "Trang 2: 'Toi da tron trong tu go va nghe no dung truoc mat canh cua rat lau.'";
                case 3:
                    return "Trang 3: 'No khong mot minh. Khu rung se dua hinh cua no den sat mat ban de lam ban bo chay.'";
                case 4:
                    return "Trang 4: 'Neu mo cua ma thay bong no dung ben ngoai, dung lao ra ngay. Doi cho no tan roi moi di.'";
                default:
                    return "Manh giay am uot, chu viet do dang.";
            }
        }

        private void NotifyStateChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
