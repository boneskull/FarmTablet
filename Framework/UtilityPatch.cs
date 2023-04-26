using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FarmTablet.Framework {
  internal class UtilityPatch : Patch {
    internal override Type PatchType => typeof(Utility);

    internal UtilityPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper) {

    }

    internal override void Apply(Harmony harmony) {
      harmony.Patch(AccessTools.Method(PatchType, nameof(Utility.getCarpenterStock), null), postfix: new HarmonyMethod(GetType(), nameof(GetCarpenterStockPostFix)));
    }

    internal static bool IsUnlocked() {
      return Helper.ReadConfig<ModConfig>().RequireFarmComputerRecipe || Utility.GetAllPlayerUnlockedCraftingRecipes().Contains("Farm Computer");
    }

    /// <summary>
    /// If unlocked, adds the tablet to Robin's items for sale
    /// </summary>
    internal static void GetCarpenterStockPostFix(Utility __instance, ref Dictionary<ISalable, int[]> __result) {
      if (IsUnlocked())
      {
        // TODO: make price configurable
        __result.Add(GetTabletTool(), new int[2] { 20000, 1 });
      }
    }
  }
}
