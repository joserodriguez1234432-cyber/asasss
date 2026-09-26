// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ForbidUtility_InAllowedArea
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (ForbidUtility), "InAllowedArea")]
[PatchLevel(Level.Safe)]
public static class Patch_ForbidUtility_InAllowedArea
{
  public static bool Prefix(IntVec3 c, Pawn forPawn)
  {
    return GenGrid.InBounds(c, ((Thing) forPawn).MapHeld);
  }

  public static void Postfix(IntVec3 c, Pawn forPawn, ref bool __result)
  {
    VehiclePawnWithMap vehicle;
    if (((!((Thing) forPawn).IsOnVehicleMapOf(out vehicle) ? 0 : (((Thing) vehicle).Spawned ? 1 : 0)) & (__result ? 1 : 0)) == 0)
      return;
    IntVec3 baseMapCoord = c.ToBaseMapCoord(vehicle);
    if (!GenGrid.InBounds(baseMapCoord, ((Thing) vehicle).Map))
      return;
    using (new VirtualTeleporter((Thing) forPawn, ((Thing) vehicle).Map))
      __result = InAllowedArea(baseMapCoord);

    bool InAllowedArea(IntVec3 c2)
    {
      Area inPawnCurrentMap = forPawn.playerSettings?.EffectiveAreaRestrictionInPawnCurrentMap;
      return inPawnCurrentMap == null || inPawnCurrentMap.TrueCount <= 0 || inPawnCurrentMap[c2];
    }
  }
}
