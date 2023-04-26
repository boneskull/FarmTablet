using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;

namespace FarmTablet.Framework {
  internal class ToolPatch : Patch {
    internal override Type PatchType => typeof(Tool);

    internal ToolPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper) {

    }

    internal override void Apply(Harmony harmony) {
      harmony.Patch(AccessTools.Method(PatchType, "get_DisplayName", null), postfix: new HarmonyMethod(GetType(), nameof(GetNamePostfix)));
      harmony.Patch(AccessTools.Method(PatchType, "get_description", null), postfix: new HarmonyMethod(GetType(), nameof(GetDescriptionPostfix)));
      harmony.Patch(AccessTools.Method(PatchType, nameof(Tool.drawInMenu), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }), prefix: new HarmonyMethod(GetType(), nameof(DrawInMenuPrefix)));
      harmony.Patch(AccessTools.Method(PatchType, nameof(Tool.beginUsing), new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(BeginUsingPrefix)));
    }

    private static void GetNamePostfix(Tool __instance, ref string __result) {
      if (__instance.modData.ContainsKey(FarmTablet.TABLET_FLAG))
      {
        __result = Helper.Translation.Get(TABLET_NAME);
      }
    }

    private static void GetDescriptionPostfix(Tool __instance, ref string __result) {
      if (__instance.modData.ContainsKey(FarmTablet.TABLET_FLAG))
      {
        __result = Helper.Translation.Get(TABLET_DESC);
      }
    }

    private static bool BeginUsingPrefix(Tool __instance, ref bool __result, GameLocation location, int x, int y, Farmer who) {
      if (who.IsLocalPlayer & __instance.modData.ContainsKey(FarmTablet.TABLET_FLAG))
      {
        __result = true;
        return UseTablet(location, who);
      }
      return true;
    }


    private static bool CancelUsing(Farmer who) {
      who.CanMove = true;
      who.UsingTool = false;
      return false;
    }

    private static bool DrawInMenuPrefix(Tool __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow) {
      if (__instance.modData.ContainsKey(FarmTablet.TABLET_FLAG))
      {
        spriteBatch.Draw(FarmTablet.TabletTexture, location + new Vector2(32f, 32f), new Rectangle(0, 0, 16, 16), color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);

        return false;
      }

      return true;
    }


    internal static bool UseTablet(GameLocation location, Farmer who) {
      if (who.IsLocalPlayer)
      {
        // this is adapted from the handler for Farm Computer

        location.playSound("DwarvishSentry");

        DelayedAction.functionAfterDelay(delegate {
          Farm farm = Game1.getFarm();
          int totalCrops = farm.getTotalCrops();
          int totalOpenHoeDirt = farm.getTotalOpenHoeDirt();
          int totalCropsReadyForHarvest = farm.getTotalCropsReadyForHarvest();
          int totalUnwateredCrops = farm.getTotalUnwateredCrops();
          int totalGreenhouseCropsReadyForHarvest = farm.getTotalGreenhouseCropsReadyForHarvest();
          int totalForageItems = farm.getTotalForageItems();
          int numberOfMachinesReadyForHarvest = farm.getNumberOfMachinesReadyForHarvest();

          bool flag = farm.doesFarmCaveNeedHarvesting();
          Game1.multipleDialogues(new string[1] {
          Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_Intro", Game1.player.farmName.Value) + "^--------------^" +
          Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_PiecesHay", farm.piecesOfHay.Value, Utility.numSilos() * 240) + "  ^" +
          Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalCrops", totalCrops) + "  ^" +
          Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsReadyForHarvest", totalCropsReadyForHarvest) + "  ^" +
          Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsUnwatered", totalUnwateredCrops) + "  ^" + ((totalGreenhouseCropsReadyForHarvest != -1) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsReadyForHarvest_Greenhouse", totalGreenhouseCropsReadyForHarvest) + "  ^") : "") +
          Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalOpenHoeDirt", totalOpenHoeDirt) + "  ^" + (farm.SpawnsForage() ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalForage", totalForageItems) + "  ^") : "") + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_MachinesReady", numberOfMachinesReadyForHarvest) + "  ^" +
          Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_FarmCave", flag ? Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes") : Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "  " });
        }, 500);
        return CancelUsing(who);
      }
      return true;
    }
  }
}
