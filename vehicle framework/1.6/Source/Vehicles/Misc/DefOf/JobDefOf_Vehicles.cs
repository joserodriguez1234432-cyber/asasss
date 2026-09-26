// Decompiled with JetBrains decompiler
// Type: Vehicles.JobDefOf_Vehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;

#nullable disable
namespace Vehicles;

[DefOf]
public static class JobDefOf_Vehicles
{
  public static JobDef IdleVehicle;
  public static JobDef IdleVehicleDeSpawned;
  public static JobDef DeployVehicle;
  public static JobDef Board;
  public static JobDef PrepareCaravan_GatheringVehicle;
  public static JobDef RopeAnimalToVehicle;
  public static JobDef CarryPawnToVehicle;
  public static JobDef RepairVehicle;
  public static JobDef DisassembleVehicle;
  public static JobDef PaintVehicle;
  public static JobDef LoadVehicle;
  public static JobDef CarryItemToVehicle;
  public static JobDef LoadUpgradeMaterials;
  public static JobDef RemoveFuelFromVehicle;
  public static JobDef RefuelVehicle;
  public static JobDef RefuelVehicleAtomic;
  public static JobDef UpgradeVehicle;
  public static JobDef FollowVehicle;
  public static JobDef EscortVehicle;
  public static JobDef SabotageVehicle;

  static JobDefOf_Vehicles() => DefOfHelper.EnsureInitializedInCtor(typeof (JobDefOf_Vehicles));
}
