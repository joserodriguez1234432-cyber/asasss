// Decompiled with JetBrains decompiler
// Type: Vehicles.Compatibility.Compatibility_RimHUD
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools.Patching;
using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles.Compatibility;

internal class Compatibility_RimHUD : ConditionalVehiclePatch
{
  public override string PackageId => "Jaxe.RimHUD";

  public override PatchSequence PatchAt => PatchSequence.Async;

  public override void PatchAll(ModMetaData mod)
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(AccessTools.TypeByName("RimHUD.Access.Patch.RimWorld_InspectPaneUtility_InspectPaneOnGUI"), "Prefix", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Compatibility_RimHUD), "DontRenderRimHUDForVehicles_InspectPaneUtility", (System.Type[]) null));
  }

  private static bool DontRenderRimHUDForVehicles_InspectPaneUtility(ref bool __result)
  {
    if (Find.UIRoot is UIRoot_Play uiRoot)
    {
      MapInterface mapUi = uiRoot.mapUI;
      if (mapUi != null)
      {
        Selector selector = mapUi.selector;
        if (selector != null && selector.SingleSelectedThing is VehiclePawn)
        {
          __result = true;
          return false;
        }
      }
    }
    return true;
  }

  private static bool DontRenderRimHUDForVehicles_InspectPaneFiller(
    ISelectable sel,
    ref bool __result)
  {
    if (!(sel is VehiclePawn))
      return true;
    __result = true;
    return false;
  }
}
