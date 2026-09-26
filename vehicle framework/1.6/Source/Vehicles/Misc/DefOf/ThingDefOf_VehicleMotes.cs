// Decompiled with JetBrains decompiler
// Type: Vehicles.ThingDefOf_VehicleMotes
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using Verse;

#nullable disable
namespace Vehicles;

[DefOf]
public static class ThingDefOf_VehicleMotes
{
  public static ThingDef MoteFishingNet;
  public static ThingDef MoteLaunchedTurret;

  static ThingDefOf_VehicleMotes()
  {
    DefOfHelper.EnsureInitializedInCtor(typeof (ThingDefOf_VehicleMotes));
  }
}
