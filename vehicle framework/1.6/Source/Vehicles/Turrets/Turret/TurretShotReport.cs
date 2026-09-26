// Decompiled with JetBrains decompiler
// Type: Vehicles.TurretShotReport
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public struct TurretShotReport
{
  private TargetInfo target;
  private float distance;
  private List<CoverInfo> covers;
  private float coversOverallBlockChance;
  private float factorFromShooterAndDist;
  private float factorFromTurret;
  private float factorFromTargetSize;
  private float factorFromWeather;
  private float forcedMissRadius;
  private float offsetFromDarkness;
  private float factorFromCoveringGas;
  private ShootLine shootLine;

  public float AimOnTargetChance
  {
    get
    {
      float aimOnTargetChance = this.factorFromShooterAndDist * this.factorFromTurret * this.factorFromWeather * this.factorFromCoveringGas + this.offsetFromDarkness;
      if ((double) aimOnTargetChance < 0.020099999383091927)
        aimOnTargetChance = 0.0201f;
      return aimOnTargetChance;
    }
  }

  public float AimOnTargetChanceWithSize => this.AimOnTargetChance * this.factorFromTargetSize;

  public float PassCoverChance => 1f - this.coversOverallBlockChance;

  public float TotalEstimatedHitChance
  {
    get => Mathf.Clamp01(this.AimOnTargetChance * this.PassCoverChance);
  }

  public ShootLine ShootLine => this.shootLine;

  public static TurretShotReport HitReportFor(
    VehiclePawn vehicle,
    VehicleTurret turret,
    LocalTargetInfo target,
    Pawn caster = null)
  {
    Map map = ((Thing) vehicle).Map;
    IntVec3 cell = ((LocalTargetInfo) ref target).Cell;
    Vector3 vector3Shifted = ((IntVec3) ref cell).ToVector3Shifted();
    Vector3 vector3 = Vector3.op_Addition(turret.DrawPosition(vehicle.FullRotation), ((Thing) vehicle).DrawPos);
    IntVec3 intVec3 = IntVec3Utility.ToIntVec3(vector3);
    TurretShotReport turretShotReport = new TurretShotReport()
    {
      distance = Vector2.Distance(new Vector2(vector3Shifted.x, vector3Shifted.z), new Vector2(vector3.x, vector3.z)),
      target = ((LocalTargetInfo) ref target).ToTargetInfo(map)
    };
    turretShotReport.factorFromShooterAndDist = turret.CurrentFireMode.canMiss ? TurretShotReport.HitFactorFromShooter(caster, turretShotReport.distance) : 1f;
    turretShotReport.factorFromTurret = turret.CurrentFireMode.GetHitChanceFactor(turretShotReport.distance);
    turretShotReport.covers = CoverUtility.CalculateCoverGiverSet(target, intVec3, map);
    turretShotReport.coversOverallBlockChance = CoverUtility.CalculateOverallBlockChance(target, intVec3, map);
    turretShotReport.factorFromCoveringGas = 1f;
    if (turret.TryFindShootLineFromTo(intVec3, target, out turretShotReport.shootLine))
    {
      foreach (IntVec3 point in ((ShootLine) ref turretShotReport.shootLine).Points())
      {
        if (GenGrid.InBounds(point, map) && GasUtility.AnyGas(point, map, (GasType) 0))
        {
          turretShotReport.factorFromCoveringGas = 0.7f;
          break;
        }
      }
    }
    else
      turretShotReport.shootLine = new ShootLine(IntVec3.Invalid, IntVec3.Invalid);
    turretShotReport.factorFromWeather = !GridsUtility.Roofed(intVec3, map) || !GridsUtility.Roofed(((LocalTargetInfo) ref target).Cell, map) ? map.weatherManager.CurWeatherAccuracyMultiplier : 1f;
    if (((LocalTargetInfo) ref target).HasThing)
    {
      turretShotReport.factorFromTargetSize = !(((LocalTargetInfo) ref target).Thing is Pawn thing) ? (float) ((double) ((LocalTargetInfo) ref target).Thing.def.fillPercent * (double) ((LocalTargetInfo) ref target).Thing.def.size.x * (double) ((LocalTargetInfo) ref target).Thing.def.size.z * 2.5) : thing.BodySize;
      turretShotReport.factorFromTargetSize = Mathf.Clamp(turretShotReport.factorFromTargetSize, 0.5f, 2f);
    }
    else
      turretShotReport.factorFromTargetSize = 1f;
    turretShotReport.forcedMissRadius = turret.CurrentFireMode.forcedMissRadius;
    turretShotReport.offsetFromDarkness = 0.0f;
    if (ModsConfig.IdeologyActive && ((LocalTargetInfo) ref target).HasThing && caster != null)
    {
      if (DarknessCombatUtility.IsOutdoorsAndLit(((LocalTargetInfo) ref target).Thing))
        turretShotReport.offsetFromDarkness = StatExtension.GetStatValue((Thing) caster, StatDefOf.ShootingAccuracyOutdoorsLitOffset, true, -1);
      else if (DarknessCombatUtility.IsOutdoorsAndDark(((LocalTargetInfo) ref target).Thing))
        turretShotReport.offsetFromDarkness = StatExtension.GetStatValue((Thing) caster, StatDefOf.ShootingAccuracyOutdoorsDarkOffset, true, -1);
      else if (DarknessCombatUtility.IsIndoorsAndDark(((LocalTargetInfo) ref target).Thing))
        turretShotReport.offsetFromDarkness = StatExtension.GetStatValue((Thing) caster, StatDefOf.ShootingAccuracyIndoorsDarkOffset, true, -1);
      else if (DarknessCombatUtility.IsIndoorsAndLit(((LocalTargetInfo) ref target).Thing))
        turretShotReport.offsetFromDarkness = StatExtension.GetStatValue((Thing) caster, StatDefOf.ShootingAccuracyIndoorsLitOffset, true, -1);
    }
    return turretShotReport;
  }

  private static float HitFactorFromShooter(Pawn caster, float distance)
  {
    if (caster == null)
      return 1f;
    float statValue = StatExtension.GetStatValue((Thing) caster, StatDefOf.ShootingAccuracyPawn, true, -1);
    if ((double) distance < 0.0)
      throw new ArgumentOutOfRangeException(nameof (distance));
    float num = (double) distance <= 3.0 ? StatExtension.GetStatValue((Thing) caster, StatDefOf.ShootingAccuracyFactor_Touch, true, -1) : ((double) distance <= 12.0 ? Mathf.Lerp(StatExtension.GetStatValue((Thing) caster, StatDefOf.ShootingAccuracyFactor_Touch, true, -1), StatExtension.GetStatValue((Thing) caster, StatDefOf.ShootingAccuracyFactor_Short, true, -1), (float) (((double) distance - 3.0) / 9.0)) : ((double) distance <= 25.0 ? Mathf.Lerp(StatExtension.GetStatValue((Thing) caster, StatDefOf.ShootingAccuracyFactor_Short, true, -1), StatExtension.GetStatValue((Thing) caster, StatDefOf.ShootingAccuracyFactor_Medium, true, -1), (float) (((double) distance - 12.0) / 13.0)) : ((double) distance <= 40.0 ? Mathf.Lerp(StatExtension.GetStatValue((Thing) caster, StatDefOf.ShootingAccuracyFactor_Medium, true, -1), StatExtension.GetStatValue((Thing) caster, StatDefOf.ShootingAccuracyFactor_Long, true, -1), (float) (((double) distance - 25.0) / 15.0)) : StatExtension.GetStatValue((Thing) caster, StatDefOf.ShootingAccuracyFactor_Long, true, -1))));
    return TurretShotReport.HitFactorFromShooter(statValue * num, distance);
  }

  private static float HitFactorFromShooter(float accRating, float distance)
  {
    return Mathf.Max(Mathf.Pow(accRating, distance), 0.02f);
  }

  internal string GetTextReadout()
  {
    StringBuilder stringBuilder1 = new StringBuilder();
    if ((double) this.forcedMissRadius > 0.5)
    {
      stringBuilder1.AppendLine();
      stringBuilder1.AppendLine($"{Translator.Translate("ForcedMissRadius")}: {this.forcedMissRadius:F1}");
      stringBuilder1.AppendLine($"{Translator.Translate("DirectHitChance")}: {GenText.ToStringPercent(1f / (float) GenRadial.NumCellsInRadius(this.forcedMissRadius))}");
    }
    else
    {
      stringBuilder1.AppendLine(GenText.ToStringPercent(this.TotalEstimatedHitChance));
      stringBuilder1.AppendLine(TaggedString.op_Implicit(TaggedString.op_Addition(TaggedString.op_Addition(TaggedString.op_Addition("   ", Translator.Translate("ShootReportShooterAbility")), ": "), GenText.ToStringPercent(this.factorFromShooterAndDist))));
      stringBuilder1.AppendLine($"   {Translator.Translate("ShootReportWeapon")}: {GenText.ToStringPercent(this.factorFromTurret)}");
      if (((TargetInfo) ref this.target).HasThing && !Mathf.Approximately(this.factorFromTargetSize, 1f))
        stringBuilder1.AppendLine($"   {Translator.Translate("TargetSize")}: {GenText.ToStringPercent(this.factorFromTargetSize)}");
      if ((double) this.factorFromWeather < 0.99000000953674316)
        stringBuilder1.AppendLine($"   {Translator.Translate("Weather")}: {GenText.ToStringPercent(this.factorFromWeather)}");
      TaggedString taggedString;
      if ((double) this.factorFromCoveringGas < 0.99000000953674316)
      {
        StringBuilder stringBuilder2 = stringBuilder1;
        taggedString = Translator.Translate("BlindSmoke");
        string str = $"   {((TaggedString) ref taggedString).CapitalizeFirst()}: {GenText.ToStringPercent(this.factorFromCoveringGas)}";
        stringBuilder2.AppendLine(str);
      }
      if (ModsConfig.IdeologyActive && ((TargetInfo) ref this.target).HasThing && !Mathf.Approximately(this.offsetFromDarkness, 0.0f))
      {
        if (DarknessCombatUtility.IsOutdoorsAndLit(((TargetInfo) ref this.target).Thing))
          stringBuilder1.AppendLine($"   {((Def) StatDefOf.ShootingAccuracyOutdoorsLitOffset).LabelCap}: {GenText.ToStringPercent(this.offsetFromDarkness)}");
        else if (DarknessCombatUtility.IsOutdoorsAndDark(((TargetInfo) ref this.target).Thing))
          stringBuilder1.AppendLine($"   {((Def) StatDefOf.ShootingAccuracyOutdoorsDarkOffset).LabelCap}: {GenText.ToStringPercent(this.offsetFromDarkness)}");
        else if (DarknessCombatUtility.IsIndoorsAndDark(((TargetInfo) ref this.target).Thing))
          stringBuilder1.AppendLine($"   {((Def) StatDefOf.ShootingAccuracyIndoorsDarkOffset).LabelCap}: {GenText.ToStringPercent(this.offsetFromDarkness)}");
        else if (DarknessCombatUtility.IsIndoorsAndLit(((TargetInfo) ref this.target).Thing))
          stringBuilder1.AppendLine($"   {((Def) StatDefOf.ShootingAccuracyIndoorsLitOffset).LabelCap}: {GenText.ToStringPercent(this.offsetFromDarkness)}");
      }
      if ((double) this.PassCoverChance < 1.0)
      {
        stringBuilder1.AppendLine($"   {Translator.Translate("ShootingCover")}: {GenText.ToStringPercent(this.PassCoverChance)}");
        foreach (CoverInfo cover in this.covers)
        {
          if ((double) ((CoverInfo) ref cover).BlockChance > 0.0)
          {
            StringBuilder stringBuilder3 = stringBuilder1;
            taggedString = TranslatorFormattedStringExtensions.Translate("CoverThingBlocksPercentOfShots", NamedArgument.op_Implicit(((Entity) ((CoverInfo) ref cover).Thing).LabelCap), NamedArgument.op_Implicit(GenText.ToStringPercent(((CoverInfo) ref cover).BlockChance)), new NamedArgument((object) ((CoverInfo) ref cover).Thing.def, "COVER"));
            string str = $"     {((TaggedString) ref taggedString).CapitalizeFirst()}";
            stringBuilder3.AppendLine(str);
          }
        }
      }
      else
        stringBuilder1.AppendLine($"   ({Translator.Translate("NoCoverLower")})");
    }
    return stringBuilder1.ToString();
  }

  public Thing GetRandomCoverToMissInto()
  {
    CoverInfo coverInfo;
    return !GenCollection.TryRandomElementByWeight<CoverInfo>((IEnumerable<CoverInfo>) this.covers, (Func<CoverInfo, float>) (cover => ((CoverInfo) ref cover).BlockChance), ref coverInfo) ? (Thing) null : ((CoverInfo) ref coverInfo).Thing;
  }
}
