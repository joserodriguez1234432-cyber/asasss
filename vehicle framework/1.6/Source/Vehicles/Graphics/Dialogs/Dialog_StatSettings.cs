// Decompiled with JetBrains decompiler
// Type: Vehicles.Dialog_StatSettings
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Dialog_StatSettings : Window
{
  private VehicleDef vehicleDef;
  private Listing_SplitColumns listing;
  private static Vector2 scrollPos;

  public Dialog_StatSettings(VehicleDef vehicleDef)
    : base((IWindowDrawing) null)
  {
    this.vehicleDef = vehicleDef;
    this.doCloseX = true;
    this.resizeable = true;
    this.listing = new Listing_SplitColumns();
    Dialog_StatSettings.scrollPos = Vector2.zero;
    this.RecacheHeight();
  }

  public virtual Vector2 InitialSize => new Vector2(400f, 400f);

  public float CachedHeight { get; private set; }

  public virtual void DoWindowContents(Rect inRect)
  {
    Rect rect = GenUI.ContractedBy(inRect, 10f);
    Rect viewRect;
    // ISSUE: explicit constructor call
    ((Rect) ref viewRect).\u002Ector(((Rect) ref rect).x, ((Rect) ref rect).y, ((Rect) ref rect).width, this.CachedHeight);
    this.listing.BeginScrollView(rect, ref Dialog_StatSettings.scrollPos, ref viewRect, 1);
    if (!GenList.NullOrEmpty<VehicleStatModifier>((IList<VehicleStatModifier>) this.vehicleDef.vehicleStats))
    {
      Dictionary<string, float> dictionary = GenCollection.TryGetValue<string, Dictionary<string, float>>((IReadOnlyDictionary<string, Dictionary<string, float>>) VehicleMod.settings.vehicles.vehicleStats, ((Def) this.vehicleDef).defName, (Dictionary<string, float>) null);
      foreach (VehicleStatModifier vehicleStat in VehicleMod.selectedDef.vehicleStats)
      {
        VehicleStatModifier statModifier = vehicleStat;
        UISettingsType uiSettingsType = this.SettingsType(statModifier.statDef.toStringStyle);
        float num1 = dictionary != null ? GenCollection.TryGetValue<string, float>((IReadOnlyDictionary<string, float>) dictionary, statModifier.statDef.defName, statModifier.value) : statModifier.value;
        string label = TaggedString.op_Implicit(statModifier.statDef.LabelCap);
        // ISSUE: explicit non-virtual call
        if (dictionary != null && __nonvirtual (dictionary.ContainsKey(statModifier.statDef.defName)))
          label = $"<color={Listing_Settings.modifiedColor.ToHex()}>{label}</color>";
        switch (uiSettingsType)
        {
          case UISettingsType.SliderFloat:
            float num2 = num1;
            this.listing.SliderLabeled(label, ref num2, string.Empty, string.Empty, string.Empty, statModifier.statDef.minValue, statModifier.statDef.maxValue, this.GetRoundingPlaces(statModifier.statDef.toStringStyle));
            if ((double) num1 != (double) num2)
            {
              if (!VehicleMod.settings.vehicles.vehicleStats.ContainsKey(((Def) this.vehicleDef).defName))
                VehicleMod.settings.vehicles.vehicleStats[((Def) this.vehicleDef).defName] = new Dictionary<string, float>();
              VehicleMod.settings.vehicles.vehicleStats[((Def) this.vehicleDef).defName][statModifier.statDef.defName] = num2;
              break;
            }
            break;
          case UISettingsType.IntegerBox:
            float num3 = num1;
            this.listing.FloatBox(label, ref num3, string.Empty, string.Empty, statModifier.statDef.minValue, statModifier.statDef.maxValue, new float?(24f), 0.5f);
            if ((double) num1 != (double) num3)
            {
              if (!VehicleMod.settings.vehicles.vehicleStats.ContainsKey(((Def) this.vehicleDef).defName))
                VehicleMod.settings.vehicles.vehicleStats[((Def) this.vehicleDef).defName] = new Dictionary<string, float>();
              VehicleMod.settings.vehicles.vehicleStats[((Def) this.vehicleDef).defName][statModifier.statDef.defName] = (float) Mathf.RoundToInt(num3);
              break;
            }
            break;
          case UISettingsType.FloatBox:
            float num4 = num1;
            this.listing.FloatBox(label, ref num4, string.Empty, string.Empty, statModifier.statDef.minValue, statModifier.statDef.maxValue, new float?(24f), 0.5f);
            if ((double) num1 != (double) num4)
            {
              if (!VehicleMod.settings.vehicles.vehicleStats.ContainsKey(((Def) this.vehicleDef).defName))
                VehicleMod.settings.vehicles.vehicleStats[((Def) this.vehicleDef).defName] = new Dictionary<string, float>();
              VehicleMod.settings.vehicles.vehicleStats[((Def) this.vehicleDef).defName][statModifier.statDef.defName] = num4;
              break;
            }
            break;
        }
        Rect currentRect = this.listing.GetCurrentRect(24f);
        if (Mouse.IsOver(currentRect))
        {
          Widgets.DrawHighlight(currentRect);
          if (Event.current.type == null && Event.current.button == 1)
          {
            Event.current.Use();
            Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
            {
              new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ResetButton")), (Action) (() =>
              {
                if (!VehicleMod.settings.vehicles.vehicleStats.ContainsKey(((Def) this.vehicleDef).defName))
                  return;
                VehicleMod.settings.vehicles.vehicleStats[((Def) this.vehicleDef).defName].Remove(statModifier.statDef.defName);
              }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
            })
            {
              vanishIfMouseDistant = true
            });
          }
        }
      }
    }
    this.listing.EndScrollView(ref viewRect);
  }

  private UISettingsType SettingsType(ToStringStyle stringStyle)
  {
    UISettingsType uiSettingsType;
    switch ((int) stringStyle)
    {
      case 0:
        uiSettingsType = UISettingsType.IntegerBox;
        break;
      case 1:
        uiSettingsType = UISettingsType.FloatBox;
        break;
      case 2:
        uiSettingsType = UISettingsType.FloatBox;
        break;
      case 3:
        uiSettingsType = UISettingsType.FloatBox;
        break;
      case 4:
        uiSettingsType = UISettingsType.FloatBox;
        break;
      case 5:
        uiSettingsType = UISettingsType.FloatBox;
        break;
      case 6:
        uiSettingsType = UISettingsType.FloatBox;
        break;
      case 7:
        uiSettingsType = UISettingsType.FloatBox;
        break;
      case 8:
        uiSettingsType = UISettingsType.SliderFloat;
        break;
      case 9:
        uiSettingsType = UISettingsType.SliderFloat;
        break;
      case 10:
        uiSettingsType = UISettingsType.SliderFloat;
        break;
      case 11:
        uiSettingsType = UISettingsType.IntegerBox;
        break;
      case 12:
        uiSettingsType = UISettingsType.IntegerBox;
        break;
      case 13:
        uiSettingsType = UISettingsType.IntegerBox;
        break;
      case 14:
        uiSettingsType = UISettingsType.IntegerBox;
        break;
      default:
        uiSettingsType = UISettingsType.None;
        break;
    }
    return uiSettingsType;
  }

  private int GetRoundingPlaces(ToStringStyle stringStyle)
  {
    int roundingPlaces;
    switch ((int) stringStyle)
    {
      case 0:
        roundingPlaces = 0;
        break;
      case 1:
        roundingPlaces = 1;
        break;
      case 2:
        roundingPlaces = 2;
        break;
      case 3:
        roundingPlaces = 3;
        break;
      case 4:
        roundingPlaces = 1;
        break;
      case 5:
        roundingPlaces = 2;
        break;
      case 6:
        roundingPlaces = 3;
        break;
      case 7:
        roundingPlaces = 3;
        break;
      case 8:
        roundingPlaces = 2;
        break;
      case 9:
        roundingPlaces = 3;
        break;
      case 10:
        roundingPlaces = 4;
        break;
      case 11:
        roundingPlaces = 0;
        break;
      case 12:
        roundingPlaces = 0;
        break;
      case 13:
        roundingPlaces = 0;
        break;
      case 14:
        roundingPlaces = 0;
        break;
      default:
        roundingPlaces = 0;
        break;
    }
    return roundingPlaces;
  }

  private void RecacheHeight()
  {
    this.CachedHeight = 0.0f;
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      if (GenList.NullOrEmpty<VehicleStatModifier>((IList<VehicleStatModifier>) this.vehicleDef.vehicleStats))
        return;
      foreach (VehicleStatModifier vehicleStat in this.vehicleDef.vehicleStats)
        this.CachedHeight += Text.LineHeight;
    }
    finally
    {
      textBlock.Dispose();
    }
  }
}
