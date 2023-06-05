using HarmonyLib;
using MelonLoader;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace BoofOfMemes
{
    public class Main : MelonMod
    {
        public override void OnApplicationLateStart()
        {
            GameDataManager.powerPrefs.dontUploadToLeaderboard = true;
            PatchGame();

            Game game = Singleton<Game>.Instance;

            if (game == null)
                return;

            game.OnLevelLoadComplete += OnLevelLoadComplete;

            if (RM.drifter)
                OnLevelLoadComplete();
        }

        private void OnLevelLoadComplete()
        {
            if (SceneManager.GetActiveScene().name.Equals("Heaven_Environment"))
                return;

            GS.AddCard("RAPTURE");
        }

        private void PatchGame()
        {
            HarmonyLib.Harmony harmony = new("de.MOPSKATER.BoofOfMemes");

            MethodInfo target = typeof(LevelStats).GetMethod("UpdateTimeMicroseconds");
            HarmonyMethod patch = new(typeof(Main).GetMethod("PreventNewScore"));
            harmony.Patch(target, patch);

            target = typeof(Game).GetMethod("OnLevelWin");
            patch = new(typeof(Main).GetMethod("PreventNewGhost"));
            harmony.Patch(target, patch);

            target = typeof(LevelRush).GetMethod("IsCurrentLevelRushScoreBetter", BindingFlags.NonPublic | BindingFlags.Static);
            patch = new(typeof(Main).GetMethod("PreventNewBestLevelRush"));
            harmony.Patch(target, patch);

            target = typeof(LevelGate).GetMethod("SetUnlocked");
            patch = new(typeof(Main).GetMethod("UnlockGate"));
            harmony.Patch(target, patch);

            target = typeof(MenuScreenLevelRushComplete).GetMethod("OnSetVisible");
            patch = new(typeof(Main).GetMethod("PostOnSetVisible"));
            harmony.Patch(target, null, patch);
        }

        public static bool PreventNewScore(LevelStats __instance, ref long newTime)
        {
            if (newTime < __instance._timeBestMicroseconds)
            {
                if (__instance._timeBestMicroseconds == 999999999999L)
                    __instance._timeBestMicroseconds = 600000000;
                __instance._newBest = true;
            }
            else
                __instance._newBest = false;
            __instance._timeLastMicroseconds = newTime;
            return false;
        }

        public static bool PreventNewGhost(Game __instance)
        {
            __instance.winAction = null;
            return true;
        }

        public static bool PreventNewBestLevelRush(ref bool __result)
        {
            __result = false;
            return false;
        }

        public static bool UnlockGate(LevelGate __instance, ref bool u)
        {
            if (__instance.Unlocked)
                return false;
            u = true;
            return true;
        }

        public static void PostOnSetVisible(ref MenuScreenLevelRushComplete __instance)
        {
            string text = LevelRush.GetCurrentLevelRushType() switch
            {
                LevelRush.LevelRushType.WhiteRush => "White's",
                LevelRush.LevelRushType.MikeyRush => "Mikeys's",
                LevelRush.LevelRushType.VioletRush => "Violet's",
                LevelRush.LevelRushType.RedRush => "Red's",
                LevelRush.LevelRushType.YellowRush => "Yellow's",
                _ => "Error"
            };

            if (text == "Error") return;

            text += (LevelRush.IsHellRush() ? " Hell " : " Heaven ") + "Boof Rush";
            __instance._rushName.textMeshProUGUI.text = text;
        }
    }
}