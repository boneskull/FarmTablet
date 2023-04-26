
using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Tools;
using System;

namespace FarmTablet.Framework {
  internal abstract class Patch {
    internal static IMonitor Monitor;
    internal static IModHelper Helper;

    internal const string TABLET_NAME = "farm-tablet.name";

    internal const string TABLET_DESC = "farm-tablet.description";

    internal abstract Type PatchType {
      get;
    }

    internal Patch(IMonitor modMonitor, IModHelper modHelper) {
      Monitor = modMonitor;
      Helper = modHelper;
    }
    internal abstract void Apply(Harmony harmony);
    internal static GenericTool GetTabletTool() {
      var tablet = new GenericTool(
        Helper.Translation.Get(TABLET_NAME),
        Helper.Translation.Get(TABLET_DESC),
        -1, 6, 6
        );
      tablet.modData[FarmTablet.TABLET_FLAG] = null;

      return tablet;
    }


  }
}
