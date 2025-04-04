﻿using System.Net.Http;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using SpeedrunMod.Events;
using SpeedrunMod.Utils;

namespace SpeedrunMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("SliceCraft.MenuLib")]
// ReSharper disable once ClassNeverInstantiated.Global
internal class Plugin : BasePlugin
{
    internal new static ManualLogSource Log;
    internal static ConfigEntry<bool> configEnableDialogueSkip;

    private readonly Harmony _harmony = new(MyPluginInfo.PLUGIN_GUID);

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        
        configEnableDialogueSkip = Config.Bind("Automatic", "EnableDialogueSkip", false, "Enable the dialogue skip on game startup (NOTE: This value is automatically controlled by the mod)");
        GlobalGame.canSkipDialogue = configEnableDialogueSkip.Value;

        _harmony.PatchAll();

        SceneLoadedEvent.RegisterEvent();
        MenuInitializedEvent.RegisterEvent();

        GetVersion().Wait();

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private static async Task GetVersion()
    {
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        try
        {
            HttpClient client = new HttpClient();
            using HttpResponseMessage response = await client.GetAsync("https://tuyu.slicegames.nl/assets/speedrunmodversion");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            VersionText.NewestVersion = responseBody;
        }
        catch (HttpRequestException)
        {
            Log.LogError("Unable to request version");
        }
    }
}
