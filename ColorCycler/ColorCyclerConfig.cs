using BepInEx.Configuration;
using System;

namespace ColorCycler
{
    internal class ColorCyclerConfig
    {
        private ConfigEntry<bool> _shouldCreatePollutionEntry;
        private ConfigEntry<bool> _infinitePaint;

        public bool ShouldCreatePollution => _shouldCreatePollutionEntry.Value;
        public bool InfinitePaint => _infinitePaint.Value;

        public ColorCyclerConfig(ConfigFile config)
        {
            _shouldCreatePollutionEntry = config.Bind(
                "ColorCycler",
                nameof(ShouldCreatePollution),
                false,
                "Should spray cans create pollution?"
            );
            _infinitePaint = config.Bind(
                "ColorCycler",
                nameof(InfinitePaint),
                true,
                "Should spray cans have infinite uses?"
            );

            config.ConfigReloaded += OnConfigReloaded;
        }

        private void OnConfigReloaded(object sender, EventArgs e)
        {
            ColorCyclerBep.Logger.LogWarning("Config Reloaded");
            ColorCyclerBep.Logger.LogWarning($"{nameof(ShouldCreatePollution)}: {ShouldCreatePollution}");
            ColorCyclerBep.Logger.LogWarning($"{nameof(InfinitePaint)}: {InfinitePaint}");
        }
    }
}
