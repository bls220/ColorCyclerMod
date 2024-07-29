using BepInEx.Logging;
using HarmonyLib;
using System;

namespace ColorCycler
{
    #region BepInEx
    [BepInEx.BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class ColorCyclerBep : BepInEx.BaseUnityPlugin
    {
        public const string pluginGuid = "net.elmo.stationeers.ColorCycler";
        public const string pluginName = "ColorCycler";
        public const string pluginVersion = "1.1.0";

        private Harmony _harmony;

        internal static new ManualLogSource Logger;
        internal static ColorCyclerConfig Settings;

        public ColorCyclerBep()
        {
            Settings ??= new ColorCyclerConfig(Config);
        }

        void Awake()
        {
            Logger = base.Logger;
            try
            {
                _harmony = new Harmony(pluginGuid);
                _harmony.PatchAll();
                Logger.LogInfo("Patch succeeded");
            }
            catch (Exception e)
            {
                Logger.LogFatal("Patch Failed");
                Logger.LogFatal(e);
            }
            Logger.LogMessage($"{nameof(Settings.ShouldCreatePollution)}: {Settings.ShouldCreatePollution}");
            Logger.LogMessage($"{nameof(Settings.InfinitePaint)}: {Settings.InfinitePaint}");
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }
    #endregion
}
