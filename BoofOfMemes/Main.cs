using AntiCheat;
using HarmonyLib;
using MelonLoader;
using System.Drawing;
using System.Reflection;
using UnityEngine.SceneManagement;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace BoofOfMemes
{
    public class Main : MelonMod
    {
        public override void OnLateInitializeMelon()
        {
            Anticheat.TriggerAnticheat();
            Anticheat.Register("BoofOfMemes");
            PatchGame();

            Game game = Singleton<Game>.Instance;

            if (game == null)
                return;

            game.OnLevelLoadComplete += OnLevelLoadComplete;

            Settings.Register();

            if (RM.drifter)
                OnLevelLoadComplete();
        }

        public static class Settings
        {

            public static MelonPreferences_Category Category;
            public static MelonPreferences_Entry<bool> Enabled;

            public static void Register()
            {
                Category = MelonPreferences.CreateCategory("Boof Mod");
                Enabled = Category.CreateEntry("Require All Demons", true, description: "Enabling will require all demons be killed to unlock level gate (main levels only).");
            }
        }

        private void OnLevelLoadComplete()
        {
            if (SceneManager.GetActiveScene().name.Equals("Heaven_Environment"))
                return;

            GS.AddCard("RAPTURE");
            LevelData currentLevel = Singleton<Game>.Instance.GetCurrentLevel();
            foreach (DiscardLockData discardLockData in currentLevel.discardLockData)
                for (int i = 0; i < discardLockData.cards.Count; i++)
                    if (discardLockData.cards[i].discardAbility == PlayerCardData.DiscardAbility.Telefrag)
                        discardLockData.cards.RemoveAt(i);
        }

        private void PatchGame()
        {
            HarmonyLib.Harmony harmony = new("de.MOPSKATER.BoofOfMemes");

            MethodInfo target = typeof(LevelGate).GetMethod("SetUnlocked");
            HarmonyMethod patch = new(typeof(Main).GetMethod("UnlockGate"));
            harmony.Patch(target, patch);

            target = typeof(MenuScreenLevelRushComplete).GetMethod("OnSetVisible");
            patch = new(typeof(Main).GetMethod("PostOnSetVisible"));
            harmony.Patch(target, null, patch);
        }

        public static bool UnlockGate(LevelGate __instance, ref bool u)
        {
            if (Settings.Enabled.Value)
                return true;

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