// Decompiled with JetBrains decompiler
// Type: Vehicles.DutyDefOf_Vehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse.AI;

#nullable disable
namespace Vehicles;

[DefOf]
public static class DutyDefOf_Vehicles
{
  public static DutyDef PrepareVehicleCaravan_BoardVehicle;
  public static DutyDef PrepareVehicleCaravan_GatherItems;
  public static DutyDef PrepareVehicleCaravan_WaitVehicle;
  public static DutyDef PrepareVehicleCaravan_GatherDownedPawns;
  public static DutyDef PrepareVehicleCaravan_SendSlavesToVehicle;
  public static DutyDef PrepareVehicleCaravan_RopeAnimalsToVehicle;
  public static DutyDef TravelOrWaitVehicle;
  public static DutyDef FollowVehicle;
  public static DutyDef VF_RangedAggressive;
  public static DutyDef VF_RangedSupport;
  public static DutyDef VF_ArmoredAssault;
  public static DutyDef VF_EscortVehicle;

  static DutyDefOf_Vehicles() => DefOfHelper.EnsureInitializedInCtor(typeof (DutyDefOf_Vehicles));
}
