using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Collections;
using Hazel;


namespace TownOfPlus {
    [Harmony]
    public class CustomOverlays
    {

        public static Dictionary<int, PlayerVersion> playerVersions = new Dictionary<int, PlayerVersion>();
        private static SpriteRenderer meetingUnderlay;
        private static SpriteRenderer infoUnderlay;
        private static TMPro.TextMeshPro infoOverlayRules;
        private static TMPro.TextMeshPro infoOverlayPlayer;

        public static bool overlayShown = false;
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class AmongUsClientOnPlayerJoinedPatch
        {
            public static void Postfix()
            {
                if (PlayerControl.LocalPlayer != null)
                {
                    shareGameVersion();
                }
            }
        }

        public static bool SendVersion = false;

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static void Postfix(GameStartManager __instance)
            {
                SendVersion = false;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerPatch
        {
            public static void Postfix(GameStartManager __instance)
            {
                if (PlayerControl.LocalPlayer != null && SendVersion == false)
                {
                    shareGameVersion();
                    SendVersion = true;
                }
            }
        }

        private static void shareGameVersion()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)TOPRPC.TOPVersionShare, Hazel.SendOption.Reliable, -1);
            writer.WritePacked(main.VersionId.Major);
            writer.WritePacked(main.VersionId.Minor);
            writer.WritePacked(main.VersionId.Build);
            writer.WritePacked(AmongUsClient.Instance.ClientId);
            writer.Write((byte)(main.VersionId.Revision < 0 ? 0xFF : main.VersionId.Revision));
            writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            TOPRPCProcedure.TOPversionshare(main.VersionId.Major, main.VersionId.Minor, main.VersionId.Build, main.VersionId.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
        }
        public static void resetOverlays()
        {
            hideInfoOverlay();
            UnityEngine.Object.Destroy(meetingUnderlay);
            UnityEngine.Object.Destroy(infoUnderlay);
            UnityEngine.Object.Destroy(infoOverlayRules);
            UnityEngine.Object.Destroy(infoOverlayPlayer);
            infoOverlayRules = infoOverlayPlayer = null;
            meetingUnderlay = infoUnderlay = null;
            overlayShown = false;
        }

        public static bool initializeOverlays()
        {
            HudManager hudManager = DestroyableSingleton<HudManager>.Instance;
            if (hudManager == null) return false;

            if (meetingUnderlay == null)
            {
                meetingUnderlay = UnityEngine.Object.Instantiate(hudManager.FullScreen, hudManager.transform);
                meetingUnderlay.transform.localPosition = new Vector3(0f, 0f, 20f);
                meetingUnderlay.gameObject.SetActive(true);
                meetingUnderlay.enabled = false;
            }

            if (infoUnderlay == null)
            {
                infoUnderlay = UnityEngine.Object.Instantiate(meetingUnderlay, hudManager.transform);
                infoUnderlay.transform.localPosition = new Vector3(0f, 0f, -900f);
                infoUnderlay.gameObject.SetActive(true);
                infoUnderlay.enabled = false;
            }

            if (infoOverlayRules == null)
            {
                infoOverlayRules = UnityEngine.Object.Instantiate(hudManager.TaskText, hudManager.transform);
                infoOverlayRules.fontSize = infoOverlayRules.fontSizeMin = infoOverlayRules.fontSizeMax = 1.15f;
                infoOverlayRules.autoSizeTextContainer = false;
                infoOverlayRules.enableWordWrapping = false;
                infoOverlayRules.alignment = TMPro.TextAlignmentOptions.TopLeft;
                infoOverlayRules.transform.position = Vector3.zero;
                infoOverlayRules.transform.localPosition = new Vector3(-1.5f, 0.9f, -910f);
                infoOverlayRules.transform.localScale = Vector3.one * 1.25f;
                infoOverlayRules.color = Palette.White;
                infoOverlayRules.enabled = false;
            }

            if (infoOverlayPlayer == null)
            {
                infoOverlayPlayer = UnityEngine.Object.Instantiate(infoOverlayRules, hudManager.transform);
                infoOverlayPlayer.maxVisibleLines = 28;
                infoOverlayPlayer.fontSize = infoOverlayPlayer.fontSizeMin = infoOverlayPlayer.fontSizeMax = 1.10f;
                infoOverlayPlayer.outlineWidth += 0.02f;
                infoOverlayPlayer.autoSizeTextContainer = false;
                infoOverlayPlayer.enableWordWrapping = false;
                infoOverlayPlayer.alignment = TMPro.TextAlignmentOptions.TopLeft;
                infoOverlayPlayer.transform.position = Vector3.zero;
                infoOverlayPlayer.transform.localPosition = infoOverlayRules.transform.localPosition + new Vector3(2.75f, 0.1f, 0.0f);
                infoOverlayPlayer.transform.localScale = Vector3.one * 1.25f;
                infoOverlayPlayer.color = Palette.White;
                infoOverlayPlayer.enabled = false;
            }

            return true;
        }

        public static void showInfoOverlay()
        {
            if (overlayShown) return;

            HudManager hudManager = DestroyableSingleton<HudManager>.Instance;
            if (PlayerControl.LocalPlayer == null || hudManager == null)
                return;

            if (!initializeOverlays()) return;

            if (MapBehaviour.Instance != null)
                MapBehaviour.Instance.Close();

            if (MeetingHud.Instance != null) hudManager.SetHudActive(false);

            overlayShown = true;

            Transform parent;
            parent = hudManager.transform;

            infoUnderlay.transform.parent = parent;
            infoOverlayRules.transform.parent = parent;
            infoOverlayPlayer.transform.parent = parent;

            infoUnderlay.color = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            infoUnderlay.transform.localScale = new Vector3(6f, 5f, 1f);
            infoUnderlay.enabled = true;
            infoOverlayRules.enabled = true;
            infoOverlayPlayer.enabled = true;

            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            HudManager.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                infoUnderlay.color = Color.Lerp(underlayTransparent, underlayOpaque, t);
                infoOverlayRules.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
                infoOverlayPlayer.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
            })));
        }

        public static void hideInfoOverlay()
        {
            if (!overlayShown) return;

            if (MeetingHud.Instance == null && ShipStatus.Instance != null) DestroyableSingleton<HudManager>.Instance.SetHudActive(true);

            overlayShown = false;
            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);

            HudManager.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                if (infoUnderlay != null)
                {
                    infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                    if (t >= 1.0f) infoUnderlay.enabled = false;
                }

                if (infoOverlayRules != null)
                {
                    infoOverlayRules.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f) infoOverlayRules.enabled = false;
                }

                if (infoOverlayPlayer != null)
                {
                    infoOverlayPlayer.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f) infoOverlayPlayer.enabled = false;
                }
            })));
        }

        public static void toggleInfoOverlay()
        {
            if (overlayShown)
                hideInfoOverlay();
            else
                showInfoOverlay();
        }

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public static class CustomOverlayKeybinds
        {
            public static void Postfix(KeyboardJoystick __instance)
            {
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    toggleInfoOverlay();
                }
            }
        }
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class CustomOverlayUpdate
        {
            public static void Postfix(HudManager __instance)
            {
                if (!initializeOverlays()) return;
                if (!overlayShown) return;
                HudManager hudManager = DestroyableSingleton<HudManager>.Instance;
                if (PlayerControl.LocalPlayer == null || hudManager == null)
                    return;

                GameOptionsData o = PlayerControl.GameOptions;
                List<string> gameOptions = o.ToString().Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();
                infoOverlayRules.text = string.Join("\n", gameOptions);
                string PlayerText = "<size=1.25>===プレイヤー一覧===</size>";
                foreach (InnerNet.ClientData Client in AmongUsClient.Instance.allClients.ToArray())
                {
                    if (Client == null) continue;
                    if (Client.Character == null) continue;
                    var player = Helpers.playerById(Client.Character.PlayerId);
                    var Platform = $"{Client.PlatformData.Platform}";
                    var TOP = "";
                    var FriendCodeText = "";
                    try
                    {
                        if (playerVersions.ContainsKey(Client.Id) || Client.Id == AmongUsClient.Instance.ClientId)
                        {
                            if (Client.Id == AmongUsClient.Instance.ClientId)
                            {
                                TOP = $"<size=0.75>(TOP v{main.Version})</size>";
                            }
                            else
                            {
                                PlayerVersion PV = playerVersions[Client.Id];
                                if (!PV.GuidMatches())
                                {
                                    TOP = $"<size=0.75><color=#FF0000>(TOP v{PV.version})</color></size>";
                                }
                                else
                                {
                                    TOP = $"<size=0.75>(TOP v{PV.version})</size>";
                                }
                            }
                        }
                    }
                    catch { }
                    var PlayerName = Client.PlayerName.DeleteHTML();
                    if (player != null && !main.HideFriendCode.Value)
                    {
                        var FriendCode = player.Data.FriendCode;
                        if (FriendCode != "") FriendCodeText = $"\n<size=0.75>FriendCode : {FriendCode}</size>";
                    }
                    var HEXcolor = Helpers.GetColorHEX(Client);
                    if (HEXcolor == "") HEXcolor = "FF000000";
                    PlayerText += $"\n<color=#{HEXcolor}>■</color>{TOP}{PlayerName} : {Platform.Replace("Standalone", "")}{FriendCodeText}";
                }

                infoOverlayPlayer.text = PlayerText;
            }
        }
        public class PlayerVersion
        {
            public readonly Version version;
            public readonly Guid guid;

            public PlayerVersion(Version version, Guid guid)
            {
                this.version = version;
                this.guid = guid;
            }

            public bool GuidMatches()
            {
                return Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.Equals(this.guid);
            }
        }
    }
}