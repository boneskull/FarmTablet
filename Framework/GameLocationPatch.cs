using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System;

namespace FarmTablet.Framework {
  internal class GameLocationPatch : Patch {

    internal override Type PatchType => typeof(GameLocation);

    internal GameLocationPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper) {

    }

    internal override void Apply(Harmony harmony) {
      harmony.Patch(AccessTools.Method(PatchType, nameof(GameLocation.checkAction), new[] { typeof(xTile.Dimensions.Location), typeof(xTile.Dimensions.Rectangle), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(CheckActionPrefix)));
    }

    /// <summary>
    /// This seems to prevent the animation from occurring when the tablet is used.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="__result"></param>
    /// <param name="tileLocation"></param>
    /// <param name="viewport"></param>
    /// <param name="who"></param>
    /// <returns></returns>
    private static bool CheckActionPrefix(GameLocation __instance, ref bool __result, xTile.Dimensions.Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who) {
      if (Game1.didPlayerJustRightClick())
      {
        return true;
      }

      if (who.CurrentTool is GenericTool tool && tool.modData.ContainsKey(FarmTablet.TABLET_FLAG))
      {
        Vector2 position = (!Game1.wasMouseVisibleThisFrame) ? Game1.player.GetToolLocation() : new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y);
        tool.beginUsing(__instance, (int)position.X, (int)position.Y, who);
        __result = false;
        return false;
      }

      return true;
    }

  }
}
