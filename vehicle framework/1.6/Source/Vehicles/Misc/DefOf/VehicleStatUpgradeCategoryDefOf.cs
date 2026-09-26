// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleStatUpgradeCategoryDefOf
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;

#nullable disable
namespace Vehicles;

[DefOf]
public class VehicleStatUpgradeCategoryDefOf
{
  public static StatUpgradeCategoryDef FuelCapacity;
  public static StatUpgradeCategoryDef FuelConsumptionRate;
  public static StatUpgradeCategoryDef ChargeRate;
  public static StatUpgradeCategoryDef DischargeRate;
  public static StatUpgradeCategoryDef WorldSpeedMultiplier;
  public static StatUpgradeCategoryDef OffRoadMultiplier;
  public static StatUpgradeCategoryDef WinterCostMultiplier;
  public static StatUpgradeCategoryDef PawnCollisionMultiplier;
  public static StatUpgradeCategoryDef PawnCollisionRecoilMultiplier;

  static VehicleStatUpgradeCategoryDefOf()
  {
    DefOfHelper.EnsureInitializedInCtor(typeof (VehicleStatUpgradeCategoryDefOf));
  }
}
