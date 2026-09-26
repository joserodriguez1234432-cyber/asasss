// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleStatWorker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[UsedImplicitly]
public class VehicleStatWorker
{
  private static readonly Gradient Gradient = new Gradient()
  {
    colorKeys = new GradientColorKey[7]
    {
      new GradientColorKey(Color.gray, 0.0f),
      new GradientColorKey(TexData.RedReadable, 0.25f),
      new GradientColorKey(TexData.SevereDamage, 0.4f),
      new GradientColorKey(TexData.ModerateDamage, 0.7f),
      new GradientColorKey(TexData.MinorDamage, 0.75f),
      new GradientColorKey(TexData.WorkingCondition, 1f),
      new GradientColorKey(TexData.Enhanced, 1.01f)
    }
  };
  public VehicleStatDef statDef;
  protected Dictionary<VehicleDef, float> baseValues;
  protected List<VehicleStatPart> statParts;

  public virtual void ClearCachedBaseValues(VehicleDef vehicleDef)
  {
    this.baseValues.Remove(vehicleDef);
  }

  public virtual void InitStatWorker(VehicleStatDef statDef)
  {
    this.statDef = statDef;
    this.baseValues = new Dictionary<VehicleDef, float>();
  }

  public virtual float GetValue(VehiclePawn vehicle)
  {
    float baseValue = this.GetBaseValue(vehicle.VehicleDef);
    return (this.TransformValue(vehicle, baseValue) * this.StatEfficiency(vehicle)).Clamp(this.statDef.minValue, this.statDef.maxValue);
  }

  public virtual float GetValueAbstract(VehicleDef vehicleDef) => this.GetBaseValue(vehicleDef);

  public virtual float StatEfficiency(VehiclePawn vehicle)
  {
    return vehicle.statHandler.StatEfficiency(this.statDef);
  }

  public float RecacheBaseValue(VehicleDef vehicleDef)
  {
    float defaultBaseValue = this.statDef.defaultBaseValue;
    foreach (VehicleStatModifier vehicleStat in vehicleDef.vehicleStats)
    {
      if (vehicleStat.statDef == this.statDef)
      {
        defaultBaseValue = vehicleStat.value;
        this.statParts = vehicleStat.parts;
        break;
      }
    }
    this.baseValues[vehicleDef] = defaultBaseValue;
    return defaultBaseValue;
  }

  public virtual string StatValueFormatted(VehiclePawn vehicle)
  {
    string str = GenText.ToStringByStyle(this.GetValue(vehicle), this.statDef.toStringStyle, (ToStringNumberSense) 1);
    if (!GenText.NullOrEmpty(this.statDef.formatString))
      str = string.Format(this.statDef.formatString, (object) str);
    return str;
  }

  public virtual float GetBaseValue(VehiclePawn vehicle) => this.GetBaseValue(vehicle.VehicleDef);

  public virtual float GetBaseValue(VehicleDef vehicleDef)
  {
    float fallback1;
    if (this.baseValues.TryGetValue(vehicleDef, out fallback1))
      return SettingsCache.TryGetValue(vehicleDef, this.statDef, fallback1);
    float fallback2 = this.RecacheBaseValue(vehicleDef);
    return SettingsCache.TryGetValue(vehicleDef, this.statDef, fallback2);
  }

  public virtual float TransformValue(VehiclePawn vehicle, float value)
  {
    if (!GenList.NullOrEmpty<VehicleStatPart>((IList<VehicleStatPart>) this.statDef.parts))
    {
      foreach (VehicleStatPart part in this.statDef.parts)
        value = part.TransformValue(vehicle, value);
    }
    if (!GenList.NullOrEmpty<VehicleStatPart>((IList<VehicleStatPart>) this.statParts))
    {
      foreach (VehicleStatPart statPart in this.statParts)
        value = statPart.TransformValue(vehicle, value);
    }
    float statOffset = vehicle.statHandler.GetStatOffset(this.statDef);
    return value + statOffset;
  }

  public virtual bool IsDisabledFor(VehiclePawn vehicle)
  {
    if (this.statDef.neverDisabled || GenList.NullOrEmpty<VehicleStatPart>((IList<VehicleStatPart>) this.statDef.parts))
      return false;
    foreach (VehicleStatPart part in this.statDef.parts)
    {
      if (part.Disabled(vehicle))
        return true;
    }
    return false;
  }

  public virtual bool ShouldShowFor(VehicleDef vehicleDef)
  {
    return !this.statDef.alwaysHide && (this.statDef.showIfUndefined || vehicleDef.vehicleStats.StatListContains(this.statDef)) && this.statDef.CanShowWithLoadedMods() && this.statDef.CanShowWithVehicle(vehicleDef);
  }

  public virtual string TipSignal(VehiclePawn vehicle) => this.statDef.description;

  public virtual string GetStatDrawEntryLabel(
    VehicleStatDef stat,
    float value,
    ToStringNumberSense numberSense,
    bool finalized = true)
  {
    return stat.ValueToString(value, finalized, numberSense);
  }

  public string GetExplanationFull(
    VehicleDef vehicleDef,
    ToStringNumberSense numberSense,
    float value,
    VehiclePawn forVehicle = null)
  {
    if (this.IsDisabledFor(forVehicle))
      return TaggedString.op_Implicit(Translator.Translate("StatsReport_PermanentlyDisabled"));
    string explanationFull = GenText.TrimEndNewlines(this.statDef.Worker.GetExplanationUnfinalized(vehicleDef, numberSense, forVehicle));
    if (!GenText.NullOrEmpty(explanationFull))
      explanationFull = explanationFull + Environment.NewLine + Environment.NewLine;
    if (forVehicle != null)
      explanationFull += this.statDef.Worker.GetExplanationFinalizePart(forVehicle, numberSense, value);
    return explanationFull;
  }

  public virtual string GetExplanationUnfinalized(
    VehicleDef vehicleDef,
    ToStringNumberSense numberSense,
    VehiclePawn forVehicle = null)
  {
    StringBuilder stringBuilder = new StringBuilder();
    float baseValue = this.GetBaseValue(vehicleDef);
    if ((double) baseValue != 0.0 || this.statDef.showZeroBaseValue)
      stringBuilder.AppendLine($"{Translator.Translate("StatsReport_BaseValue")}: {this.statDef.ValueToString(baseValue, numberSense: numberSense)}");
    if (((Thing) forVehicle)?.Stuff != null && (double) baseValue <= 0.0)
    {
      int num = this.statDef.applyFactorsIfNegative ? 1 : 0;
    }
    if (!GenList.NullOrEmpty<VehicleStatDef>((IList<VehicleStatDef>) this.statDef.statFactors))
    {
      stringBuilder.AppendLine(TaggedString.op_Implicit(Translator.Translate("StatsReport_OtherStats")));
      foreach (VehicleStatDef statFactor in this.statDef.statFactors)
      {
        string str = forVehicle != null ? GenText.ToStringPercent(statFactor.Worker.GetValue(forVehicle)) : GenText.ToStringPercent(statFactor.Worker.GetValueAbstract(vehicleDef));
        stringBuilder.AppendLine($"    {statFactor.LabelCap}: {str}");
      }
    }
    return stringBuilder.ToString();
  }

  public virtual string GetExplanationFinalizePart(
    VehiclePawn vehicle,
    ToStringNumberSense numberSense,
    float finalValue)
  {
    StringBuilder stringBuilder = new StringBuilder();
    if (this.statDef.parts != null)
    {
      foreach (VehicleStatPart part in this.statDef.parts)
      {
        string str = part.ExplanationPart(vehicle);
        if (!GenText.NullOrEmpty(str))
          stringBuilder.AppendLine(str);
      }
    }
    List<VehicleStatModifier> vehicleStats = vehicle.VehicleDef.vehicleStats;
    VehicleStatModifier vehicleStatModifier = vehicleStats != null ? GenCollection.FirstOrDefault<VehicleStatModifier>(vehicleStats, (Predicate<VehicleStatModifier>) (statMod => statMod.statDef == this.statDef)) : (VehicleStatModifier) null;
    if (vehicleStatModifier != null && !GenList.NullOrEmpty<VehicleStatPart>((IList<VehicleStatPart>) vehicleStatModifier.parts))
    {
      foreach (VehicleStatPart part in vehicleStatModifier.parts)
      {
        string str = part.ExplanationPart(vehicle);
        if (!GenText.NullOrEmpty(str))
          stringBuilder.AppendLine(str);
      }
    }
    if (this.statDef.postProcessCurve != null)
    {
      float val1 = this.GetValue(vehicle);
      float val2 = this.statDef.postProcessCurve.Evaluate(val1);
      if (!Mathf.Approximately(val1, val2))
      {
        string str1 = this.ValueToString(val1, false, (ToStringNumberSense) 1);
        string str2 = this.statDef.ValueToString(val2, numberSense: numberSense);
        stringBuilder.AppendLine($"{Translator.Translate("StatsReport_PostProcessed")}: {str1} => {str2}");
      }
    }
    if (this.statDef.postProcessStatFactors != null)
    {
      stringBuilder.AppendLine(TaggedString.op_Implicit(Translator.Translate("StatsReport_OtherStats")));
      foreach (VehicleStatDef processStatFactor in this.statDef.postProcessStatFactors)
        stringBuilder.AppendLine($"    {processStatFactor.LabelCap}: x{GenText.ToStringPercent(processStatFactor.Worker.GetValue(vehicle))}");
    }
    stringBuilder.Append($"{Translator.Translate("StatsReport_FinalValue")}:  {this.statDef.ValueToString(finalValue, numberSense: this.statDef.toStringNumberSense)}");
    return stringBuilder.ToString();
  }

  public virtual float DrawVehicleStat(Rect leftRect, float curY, VehiclePawn vehicle)
  {
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(0.0f, curY, ((Rect) ref leftRect).width, 20f);
    if (Mouse.IsOver(rect1))
    {
      GUI.color = TexData.HighlightColor;
      GUI.DrawTexture(rect1, (Texture) TexUI.HighlightTex);
    }
    GUI.color = Color.white;
    Widgets.Label(new Rect(0.0f, curY, ((Rect) ref leftRect).width * 0.65f, 30f), this.statDef.LabelCap);
    float baseValue = this.GetBaseValue(vehicle.VehicleDef);
    if (this.statDef.operationType > EfficiencyOperationType.None)
    {
      Color color = (double) baseValue == 0.0 ? TexData.WorkingCondition : VehicleStatWorker.Gradient.Evaluate(this.GetValue(vehicle) / baseValue);
      Widgets.Label(new Rect(((Rect) ref leftRect).width * 0.65f, curY, ((Rect) ref leftRect).width * 0.35f, 30f), ColoredText.Colorize(this.StatValueFormatted(vehicle), color));
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector(0.0f, curY, ((Rect) ref leftRect).width, 20f);
      if (Mouse.IsOver(rect2))
        TooltipHandler.TipRegion(rect2, new Verse.TipSignal(this.TipSignal(vehicle), ((Thing) vehicle).thingIDNumber ^ (int) this.statDef.index));
    }
    curY += 20f;
    return curY;
  }

  public virtual string ValueToString(float val, bool finalized, ToStringNumberSense numberSense = 1)
  {
    if (!finalized)
    {
      string str = GenText.ToStringByStyle(val, this.statDef.ToStringStyleUnfinalized, numberSense);
      if (numberSense != 2 && !GenText.NullOrEmpty(this.statDef.formatStringUnfinalized))
        str = string.Format(this.statDef.formatStringUnfinalized, (object) str);
      return str;
    }
    string str1 = GenText.ToStringByStyle(val, this.statDef.toStringStyle, numberSense);
    if (numberSense != 2 && !GenText.NullOrEmpty(this.statDef.formatString))
      str1 = string.Format(this.statDef.formatString, (object) str1);
    return str1;
  }

  public virtual IEnumerable<Dialog_InfoCard.Hyperlink> GetInfoCardHyperlinks(VehiclePawn vehicle)
  {
    if (this.statDef.parts != null)
    {
      foreach (VehicleStatPart part in this.statDef.parts)
      {
        IEnumerator<Dialog_InfoCard.Hyperlink> enumerator = part.GetInfoCardHyperlinks(vehicle).GetEnumerator();
        while (enumerator.MoveNext())
          yield return enumerator.Current;
        enumerator = (IEnumerator<Dialog_InfoCard.Hyperlink>) null;
      }
    }
  }
}
