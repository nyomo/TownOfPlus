using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Collections;
using UnhollowerBaseLib;
using UnityEngine;
using System.Linq;
using HarmonyLib;
using Hazel;

namespace TownOfPlus {

    public static class Helpers
    {
        public static void destroyList<T>(Il2CppSystem.Collections.Generic.List<T> items) where T : UnityEngine.Object
        {
            if (items == null) return;
            foreach (T item in items)
            {
                UnityEngine.Object.Destroy(item);
            }
        }

        public static void destroyList<T>(List<T> items) where T : UnityEngine.Object
        {
            if (items == null) return;
            foreach (T item in items)
            {
                UnityEngine.Object.Destroy(item);
            }
        }
        public static Texture2D loadTextureFromResources(string path) {
            try {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteTexture = new byte[stream.Length];
                var read = stream.Read(byteTexture, 0, (int) stream.Length);
                LoadImage(texture, byteTexture, false);
                return texture;
            } catch {
            }
            return null;
        }

        public static Texture2D loadTextureFromDisk(string path) {
            try {          
                if (File.Exists(path))     {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    byte[] byteTexture = File.ReadAllBytes(path);
                    LoadImage(texture, byteTexture, false);
                    return texture;
                }
            } catch {
            }
            return null;
        }

        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage;
        private static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable) {
            if (iCall_LoadImage == null)
                iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2cppArray = (Il2CppStructArray<byte>) data;
            return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }
        public static void SyncSettings()
        {
            var optByte = PlayerControl.GameOptions.DeepCopy();
            PlayerControl.LocalPlayer.RpcSyncSettings(optByte);
        }
        public static GameOptionsData DeepCopy(this GameOptionsData opt)
        {
            var optByte = opt.ToBytes(5);
            return GameOptionsData.FromBytes(optByte);
        }

        public static PlayerControl playerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;
            return null;
        }

        public static PlayerControl GetNamePlayer(string name)
        {
            PlayerControl player = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(name));
            return player;
        }

        public static Color GetPlayerColor(PlayerControl p)
        {
            var RoleType = p.Data.Role.Role;
            if (RoleType == RoleTypes.Impostor || RoleType == RoleTypes.Shapeshifter)
            {
                return Palette.ImpostorRed;
            }
            else
            {
                if (RoleType == RoleTypes.Engineer || RoleType == RoleTypes.Scientist || RoleType == RoleTypes.GuardianAngel)
                {
                    return Palette.CrewmateBlue;
                }
                else
                {
                    return Palette.White;
                }
            }
        }
    }
    class Timer
    {
        public float timer;
        public Action action;
        public static List<Timer> Timers = new List<Timer>();
        public bool run(float deltaTime)
        {
            timer -= deltaTime;
            if (timer <= 0)
            {
                action();
                return true;
            }
            return false;
        }
        public Timer(Action action, float time)
        {
            this.action = action;
            this.timer = time;
            Timers.Add(this);
        }
        public static void Update(float deltaTime)
        {
            var TimersToRemove = new List<Timer>();
            Timers.ForEach((Timer) => {
                if (Timer.run(deltaTime))
                {
                    TimersToRemove.Add(Timer);
                }
            });
            TimersToRemove.ForEach(Timer => Timers.Remove(Timer));
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class TimerUpdate
    {
        public static void Postfix(HudManager __instance)
        {
            Timer.Update(Time.deltaTime);
        }
    }
    public class StartAction
    {
        bool flag;
        public void Reset()
        {
            flag = false;
        }

        public void Run(Action action)
        {
            if (!flag)
            {
                action();
                flag = true;
            }
        }
    }
}
