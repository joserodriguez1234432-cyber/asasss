// Decompiled with JetBrains decompiler
// Type: Vehicles.HealthHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Text;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public static class HealthHelper
{
  private static readonly (int, string)[] cachedVehicleHealths = new (int, string)[DefDatabase<VehicleDef>.AllDefsListForReading.Count];
  private static readonly DamageArmorCategoryDef bluntArmor = DefDatabase<DamageArmorCategoryDef>.GetNamed("Blunt", true);
  private static readonly DamageArmorCategoryDef heatArmor = DefDatabase<DamageArmorCategoryDef>.GetNamed("Heat", true);

  public static (int current, int max, string explanation) GetTotalHealth(this VehiclePawn vehicle)
  {
    int num1 = 0;
    int num2 = 0;
    StringBuilder stringBuilder = new StringBuilder();
    foreach (VehicleComponent component in vehicle.statHandler.components)
    {
      num1 += Mathf.RoundToInt(component.Health);
      num2 += component.props.health;
      stringBuilder.AppendLine($"{component.props.label}: {component.Health} / {component.props.health}");
      float upgraded;
      float num3 = component.ArmorRating(HealthHelper.bluntArmor, out upgraded);
      stringBuilder.AppendLine($"    {((Def) StatDefOf.ArmorRating_Blunt).LabelCap}: {GenText.ToStringPercent(num3)}");
      float num4 = component.ArmorRating(DamageArmorCategoryDefOf.Sharp, out upgraded);
      stringBuilder.AppendLine($"    {((Def) StatDefOf.ArmorRating_Sharp).LabelCap}: {GenText.ToStringPercent(num4)}");
      float num5 = component.ArmorRating(HealthHelper.heatArmor, out upgraded);
      stringBuilder.AppendLine($"    {((Def) StatDefOf.ArmorRating_Heat).LabelCap}: {GenText.ToStringPercent(num5)}");
      stringBuilder.AppendLine();
    }
    return (num1, num2, stringBuilder.ToString());
  }

  public static (int current, int max, string explanation) GetTotalHealth(this VehicleDef vehicleDef)
  {
    (int, string) cachedVehicleHealth = HealthHelper.cachedVehicleHealths[vehicleDef.DefIndex];
    if (cachedVehicleHealth.Item1 == 0)
    {
      cachedVehicleHealth.Item1 = 0;
      StringBuilder stringBuilder = new StringBuilder();
      float statValueAbstract1 = StatExtension.GetStatValueAbstract((BuildableDef) vehicleDef, StatDefOf.ArmorRating_Blunt, (ThingDef) null);
      float statValueAbstract2 = StatExtension.GetStatValueAbstract((BuildableDef) vehicleDef, StatDefOf.ArmorRating_Sharp, (ThingDef) null);
      float statValueAbstract3 = StatExtension.GetStatValueAbstract((BuildableDef) vehicleDef, StatDefOf.ArmorRating_Heat, (ThingDef) null);
      foreach (VehicleComponentProperties component in vehicleDef.components)
      {
        cachedVehicleHealth.Item1 += component.health;
        stringBuilder.AppendLine($"{component.label}: {component.health}");
        float statValueFromList1 = StatUtility.GetStatValueFromList(component.armor, StatDefOf.ArmorRating_Blunt, statValueAbstract1);
        stringBuilder.AppendLine($"    {((Def) StatDefOf.ArmorRating_Blunt).LabelCap}: {GenText.ToStringPercent(statValueFromList1)}");
        float statValueFromList2 = StatUtility.GetStatValueFromList(component.armor, StatDefOf.ArmorRating_Sharp, statValueAbstract2);
        stringBuilder.AppendLine($"    {((Def) StatDefOf.ArmorRating_Sharp).LabelCap}: {GenText.ToStringPercent(statValueFromList2)}");
        float statValueFromList3 = StatUtility.GetStatValueFromList(component.armor, StatDefOf.ArmorRating_Heat, statValueAbstract3);
        stringBuilder.AppendLine($"    {((Def) StatDefOf.ArmorRating_Heat).LabelCap}: {GenText.ToStringPercent(statValueFromList3)}");
        stringBuilder.AppendLine();
      }
      cachedVehicleHealth.Item2 = stringBuilder.ToString();
      HealthHelper.cachedVehicleHealths[vehicleDef.DefIndex] = cachedVehicleHealth;
    }
    return (cachedVehicleHealth.Item1, cachedVehicleHealth.Item1, cachedVehicleHealth.Item2);
  }

  public static bool AttemptToDrown(Pawn pawn)
  {
    if (pawn is VehiclePawn)
      return true;
    float movementCapacity = (float) (((double) pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving) + (double) pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation)) / 2.0);
    return (double) movementCapacity <= 1.1499999761581421 && Rand.Chance(HealthHelper.InstantDeathChance(movementCapacity));
  }

  public static float InstantDeathChance(float movementCapacity)
  {
    return Mathf.Clamp01(movementCapacity - 0.65f);
  }
}
