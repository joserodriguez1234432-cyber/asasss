// Decompiled with JetBrains decompiler
// Type: Vehicles.HistoryEventDefOf_Vehicles
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;

#nullable disable
namespace Vehicles;

[DefOf]
public static class HistoryEventDefOf_Vehicles
{
  [MayRequireIdeology]
  public static HistoryEventDef VF_BoardLandVehicle;
  [MayRequireIdeology]
  public static HistoryEventDef VF_BoardAirVehicle;
  [MayRequireIdeology]
  public static HistoryEventDef VF_BoardSeaVehicle;
  [MayRequireIdeology]
  public static HistoryEventDef VF_BoardUniversalVehicle;
  [MayRequireIdeology]
  public static HistoryEventDef VF_BoardedLandVehicle;
  [MayRequireIdeology]
  public static HistoryEventDef VF_BoardedAirVehicle;
  [MayRequireIdeology]
  public static HistoryEventDef VF_BoardedSeaVehicle;
  [MayRequireIdeology]
  public static HistoryEventDef VF_BoardedUniversalVehicle;

  static HistoryEventDefOf_Vehicles()
  {
    DefOfHelper.EnsureInitializedInCtor(typeof (HistoryEventDefOf_Vehicles));
  }
}
