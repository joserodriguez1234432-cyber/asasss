// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_PlaceWorker_WindTurbine_DrawGhost
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (PlaceWorker_WindTurbine), "DrawGhost")]
[PatchLevel(Level.Safe)]
public static class Patch_PlaceWorker_WindTurbine_DrawGhost
{
  public static void Prefix(ref IntVec3 center, ref Rot4 rot, Thing thing)
  {
    if (Command_FocusVehicleMap.FocusedVehicle != null)
    {
      center = center.ToBaseMapCoord(Command_FocusVehicleMap.FocusedVehicle);
      ref Rot4 local = ref rot;
      int asInt1 = ((Rot4) ref local).AsInt;
      Rot4 rotation = ((Thing) Command_FocusVehicleMap.FocusedVehicle).Rotation;
      int asInt2 = ((Rot4) ref rotation).AsInt;
      ((Rot4) ref local).AsInt = asInt1 + asInt2;
    }
    VehiclePawnWithMap vehicle;
    if (!thing.IsOnNonFocusedVehicleMapOf(out vehicle))
      return;
    center = center.ToBaseMapCoord(vehicle);
    ref Rot4 local1 = ref rot;
    int asInt3 = ((Rot4) ref local1).AsInt;
    Rot4 rotation1 = ((Thing) vehicle).Rotation;
    int asInt4 = ((Rot4) ref rotation1).AsInt;
    ((Rot4) ref local1).AsInt = asInt3 + asInt4;
  }
}
