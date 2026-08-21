using BepInEx.Configuration;

namespace AmandsSense.Helpers
{
    internal static class SpoilerFreeSettings
    {
        public static ConfigEntry<bool> InspectContainerContents { get; private set; }
        public static ConfigEntry<bool> UseGenericLooseLootMarker { get; private set; }

        public static void Init(ConfigFile config)
        {
            InspectContainerContents = config.Bind(
                "AmandsSense",
                "InspectContainerContents",
                true,
                new ConfigDescription(
                    "When disabled, container Sense markers do not inspect or reveal container contents."));

            UseGenericLooseLootMarker = config.Bind(
                "AmandsSense",
                "UseGenericLooseLootMarker",
                false,
                new ConfigDescription(
                    "Use one generic icon and color for loose loot and suppress item metadata in Sense markers."));
        }
    }
}
