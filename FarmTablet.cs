using FarmTablet.Framework;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;

namespace FarmTablet {
  public interface IGenericModConfigMenuApi {
    void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

    void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);
  }

  /// <summary>The mod entry point.</summary>
  internal sealed class FarmTablet : Mod {
    /// <summary>
    /// Flag applied to `GenericTool` so it can be recognized as a farm tablet.
    /// </summary>
    internal const string TABLET_FLAG = "FarmTablet.TabletFlag";

    /// <summary>
    /// Texture for the tablet icon
    /// </summary>
    internal static Texture2D TabletTexture;

    public ModConfig Config;

    /// <inheritdoc />
    public override void Entry(IModHelper helper) {
      Config = helper.ReadConfig<ModConfig>();

      var assetFolderPath = helper.ModContent.GetInternalAssetName("assets").Name;
      TabletTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "tablet.png"));

      // To create a new tool that displays no animation, we patch `GameLocation.CheckAction`.
      // Then, to give the tool behavior and to make it display in menus, we patch various methods
      // in `Tool`.
      try
      {
        var harmony = new Harmony(ModManifest.UniqueID);
        new GameLocationPatch(Monitor, helper).Apply(harmony);
        new ToolPatch(Monitor, helper).Apply(harmony);
        new UtilityPatch(Monitor, helper).Apply(harmony);

        // Apply texture override related patches
      } catch (Exception e)
      {
        Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
        return;
      }

      ConfigureCommands();

      helper.Events.GameLoop.GameLaunched += OnGameLaunched;


    }

    private void ConfigureCommands() {
      Helper.ConsoleCommands.Add("at_tablet_shop", Helper.Translation.Get("command.at_tablet_shop.description"), DebugShowTabletShop);
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
      if (Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
      {
        ConfigureGMCM();
      }
    }

    private void ConfigureGMCM() {
      var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
      if (configMenu is null)
      {
        return;
      }
      configMenu.Register(
        mod: ModManifest,
        reset: () => Config = new ModConfig(),
        save: () => base.Helper.WriteConfig(Config)
      );
      configMenu.AddBoolOption(
        mod: ModManifest,
        name: () => Helper.Translation.Get("config.require-farm-computer-recipe.description"),
        setValue: value => Config.RequireFarmComputerRecipe = value,
        getValue: () => Config.RequireFarmComputerRecipe,
        tooltip: () => Helper.Translation.Get("config.require-farm-computer-recipe.tooltip")
      );
    }

    /// <summary>
    /// Debugging tool to show the tablet in the carpenter shop.
    /// </summary>
    private void DebugShowTabletShop(string command, string[] args) {
      var items = new Dictionary<ISalable, int[]>()
            {
                { Framework.Patch.GetTabletTool(), new int[2] { 100, 1 } },
            };
      Game1.activeClickableMenu = new ShopMenu(items);
    }

  }

}
