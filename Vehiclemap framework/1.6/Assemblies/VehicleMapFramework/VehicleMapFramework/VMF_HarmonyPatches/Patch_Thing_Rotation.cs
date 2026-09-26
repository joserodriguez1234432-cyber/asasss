// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Thing_Rotation
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using Vehicles;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch]
[PatchLevel(Level.Sensitive)]
public static class Patch_Thing_Rotation
{
  public static void Prefix(Thing __instance, ref Rot4 value)
  {
    VehiclePawnWithMap vehicle;
    if (!(__instance is Pawn pawn) || pawn is VehiclePawn || !((Thing) pawn).IsOnNonFocusedVehicleMapOf(out vehicle))
      return;
    Pawn_PathFollower pather = pawn.pather;
    if (pather != null && pather.Moving)
    {
      IntVec3 nextCell = pather.nextCell;
      if (((IntVec3) ref nextCell).IsValid && IntVec3.op_Inequality(pawn.pather.nextCell, ((Thing) pawn).Position))
      {
        IntVec3 intVec3 = IntVec3.op_Subtraction(pawn.pather.nextCell, ((Thing) pawn).Position);
        float angleFlat = ((IntVec3) ref intVec3).AngleFlat;
        value = Rot8.op_Implicit(Rot8.FromAngle(Ext_Math.RotateAngle(angleFlat, VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle))));
        return;
      }
    }
    if (pawn.stances.curStance is Stance_Busy curStance)
    {
      if (((LocalTargetInfo) ref curStance.focusTarg).HasThing)
        return;
      ref Rot4 local = ref value;
      IntVec3 intVec3 = IntVec3.op_Subtraction(curStance.focusTarg.TargetCellOnBaseMap((Thing) pawn), VehicleMapUtility.get_PositionOnBaseMap((Thing) pawn));
      Rot4 rot4 = Pawn_RotationTracker.RotFromAngleBiased(((IntVec3) ref intVec3).AngleFlat);
      local = rot4;
    }
    else
    {
      if (pawn.Drafted || GenHostility.HostileTo((Thing) pawn, Faction.OfPlayer))
        return;
      ref Rot4 local = ref value;
      int asInt1 = ((Rot4) ref local).AsInt;
      Rot4 rotation = ((Thing) vehicle).Rotation;
      int asInt2 = ((Rot4) ref rotation).AsInt;
      ((Rot4) ref local).AsInt = asInt1 + asInt2;
    }
  }
}
