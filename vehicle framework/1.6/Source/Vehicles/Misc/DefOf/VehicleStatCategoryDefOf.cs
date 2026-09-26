// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleStatCategoryDefOf
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;

#nullable disable
namespace Vehicles;

[DefOf]
public class VehicleStatCategoryDefOf
{
  public static StatCategoryDef VehicleBasics;
  public static StatCategoryDef VehicleBasicsImportant;
  public static StatCategoryDef VehicleAerial;
  public static StatCategoryDef VehicleRefuelable;
  public static StatCategoryDef VehicleTurrets;

  static VehicleStatCategoryDefOf()
  {
    DefOfHelper.EnsureInitializedInCtor(typeof (StatCategoryDefOf));
  }
}
