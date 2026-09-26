// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ReservationUtility_ReserveSittableOrSpot
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VehicleMapFramework.EarlyPatches")]
[HarmonyPatch(typeof (ReservationUtility), "ReserveSittableOrSpot")]
[PatchLevel(Level.Mandatory)]
[HarmonyBefore(new string[] {"ProgressionEducationMod"})]
public static class Patch_ReservationUtility_ReserveSittableOrSpot
{
  public static bool Prefix(Pawn pawn, IntVec3 exactSittingPos, Job job, ref Map __state)
  {
    Map map = (job != null ? ((LocalTargetInfo) ref job.targetA).Thing?.Map : (Map) null) == null || !((LocalTargetInfo) ref job.targetA).Thing.def.hasInteractionCell || !IntVec3.op_Equality(((LocalTargetInfo) ref job.targetA).Thing.InteractionCell, exactSittingPos) ? (job != null ? ((GlobalTargetInfo) ref job.globalTarget).Map : (Map) null) ?? TargetMapUtility.get_TargetMap((Thing) pawn) ?? ((Thing) pawn).Map : ((LocalTargetInfo) ref job.targetA).Thing.Map;
    if (map == null)
      return true;
    if (((Thing) pawn).Map != map && VehicleMapUtility.get_GroundMap((Thing) pawn) == VehicleMapUtility.get_GroundMap(map))
    {
      __state = ((Thing) pawn).Map;
      ((Thing) pawn).VirtualMapTransfer(map);
    }
    return GenGrid.InBounds(exactSittingPos, map);
  }

  public static void Finalizer(
    Pawn pawn,
    IntVec3 exactSittingPos,
    Job job,
    Map __state,
    bool __result)
  {
    if (__state == null)
      return;
    Map map = ((Thing) pawn).Map;
    ((Thing) pawn).VirtualMapTransfer(__state);
    if (!__result)
      return;
    job.globalTarget = new GlobalTargetInfo(exactSittingPos, map, false);
  }
}
