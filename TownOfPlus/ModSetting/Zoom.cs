using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using System;
using System.Linq;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnhollowerBaseLib;
using Hazel;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using Il2CppSystem;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UI;

namespace TownOfPlus
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class Zoom
    {
        public static void Postfix(HudManager __instance)
        {
            if (main.Zoom.Value && ((AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started
                || AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                && (PlayerControl.LocalPlayer.CanMove)
                && !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen)
                && !(MeetingHud.Instance)))
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    {
                        if (Camera.main.orthographicSize > 1.0f)
                        {
                            Camera.main.orthographicSize /= 1.5f;
                            __instance.transform.localScale /= 1.5f;
                            __instance.UICamera.orthographicSize /= 1.5f;
                            HudManager.Instance.TaskStuff.SetActive(false);
                        }
                    }
                    else
                    {
                        if (Camera.main.orthographicSize > 3.0f)
                        {
                            Camera.main.orthographicSize /= 1.5f;
                            __instance.transform.localScale /= 1.5f;
                            __instance.UICamera.orthographicSize /= 1.5f;
                        }
                    }

                }
                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    if (PlayerControl.LocalPlayer.Data.IsDead || AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    {
                        if (Camera.main.orthographicSize < 18.0f)
                        {
                            Camera.main.orthographicSize *= 1.5f;
                            __instance.transform.localScale *= 1.5f;
                            __instance.UICamera.orthographicSize *= 1.5f;
                        }
                    }
                }
                if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                {
                    if (Camera.main.orthographicSize != 3.0f)
                    {
                        HudManager.Instance.TaskStuff.SetActive(false);
                        ModManager.Instance.ModStamp.gameObject.SetActive(false);
                        if (!PlayerControl.LocalPlayer.Data.IsDead) __instance.ShadowQuad.gameObject.SetActive(false);
                    }
                    else
                    {
                        HudManager.Instance.TaskStuff.SetActive(true);
                        ModManager.Instance.ModStamp.gameObject.SetActive(true);
                        if (!PlayerControl.LocalPlayer.Data.IsDead) __instance.ShadowQuad.gameObject.SetActive(true);
                    }
                }
                CreateFlag.NewFlag("Zoom");
            }
            else
            {
                CreateFlag.Run(() =>
                {
                    Camera.main.orthographicSize = 3.0f;
                    HudManager.Instance.UICamera.orthographicSize = 3.0f;
                    HudManager.Instance.transform.localScale = Vector3.one;
                    if (MeetingHud.Instance != null) MeetingHud.Instance.transform.localScale = Vector3.one;
                    HudManager.Instance.Chat.transform.localScale = Vector3.one;
                    if (AmongUsClient.Instance.GameMode == GameModes.FreePlay && !PlayerControl.LocalPlayer.Data.IsDead) __instance.ShadowQuad.gameObject.SetActive(true);
                }, "Zoom");
            }
        }
    }
}
