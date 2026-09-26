// Decompiled with JetBrains decompiler
// Type: Vehicles.PostToSettingsAttribute
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Vehicles.Config;
using Verse;

#nullable disable
namespace Vehicles;

[AttributeUsage(AttributeTargets.Field)]
public class PostToSettingsAttribute : Attribute
{
  public string Label { get; set; }

  public string Tooltip { get; set; }

  public bool Translate { get; set; }

  public UISettingsType UISettingsType { get; set; }

  public VehicleType VehicleType { get; set; } = VehicleType.Universal;

  public bool ParentHolder { get; set; }

  public string ResolvedLabel()
  {
    return !this.Translate ? this.Label : Translator.Translate(this.Label).ToString();
  }

  public string ResolvedTooltip()
  {
    return !this.Translate ? this.Tooltip : Translator.Translate(this.Tooltip).ToString();
  }

  public void DrawLister(Listing_Settings lister, VehicleDef vehicleDef, FieldInfo field)
  {
    string label = this.ResolvedLabel();
    string tooltip1 = this.ResolvedTooltip();
    string disabledTooltip = string.Empty;
    FeatureEnabledAttribute enabledAttribute;
    if (GenAttribute.TryGetAttribute<FeatureEnabledAttribute>((MemberInfo) field, ref enabledAttribute) && !FeatureFlags.IsFeatureEnabled(enabledAttribute.FeatureName))
      return;
    DisableSettingConditionalAttribute conditionalAttribute;
    if (GenAttribute.TryGetAttribute<DisableSettingConditionalAttribute>((MemberInfo) field, ref conditionalAttribute))
    {
      if (!GenText.NullOrEmpty(conditionalAttribute.MayRequire) && !Ext_Mods.HasActiveMod(conditionalAttribute.MayRequire))
      {
        disabledTooltip = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_DisabledSingleModDependencyTooltip", NamedArgument.op_Implicit(conditionalAttribute.MayRequire)));
      }
      else
      {
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        if (!GenList.NullOrEmpty<string>((IList<string>) conditionalAttribute.MayRequireAny) && !((IEnumerable<string>) conditionalAttribute.MayRequireAny).Any<string>(PostToSettingsAttribute.\u003C\u003EO.\u003C0\u003E__HasActiveMod ?? (PostToSettingsAttribute.\u003C\u003EO.\u003C0\u003E__HasActiveMod = new Func<string, bool>(Ext_Mods.HasActiveMod))))
        {
          disabledTooltip = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_DisabledSingleModDependencyTooltip", NamedArgument.op_Implicit(Environment.NewLine + string.Join(Environment.NewLine, conditionalAttribute.MayRequireAny))));
        }
        else
        {
          // ISSUE: reference to a compiler-generated field
          // ISSUE: reference to a compiler-generated field
          if (!GenList.NullOrEmpty<string>((IList<string>) conditionalAttribute.MayRequireAll) && !((IEnumerable<string>) conditionalAttribute.MayRequireAll).All<string>(PostToSettingsAttribute.\u003C\u003EO.\u003C0\u003E__HasActiveMod ?? (PostToSettingsAttribute.\u003C\u003EO.\u003C0\u003E__HasActiveMod = new Func<string, bool>(Ext_Mods.HasActiveMod))))
          {
            disabledTooltip = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_DisabledMultipleModsDependencyTooltip", NamedArgument.op_Implicit(Environment.NewLine + string.Join(Environment.NewLine, conditionalAttribute.MayRequireAll))));
          }
          else
          {
            string tooltip2;
            if (conditionalAttribute.FieldDisabled(vehicleDef, out tooltip2))
            {
              disabledTooltip = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_SaveableFieldDisabledConditionTooltip", NamedArgument.op_Implicit(tooltip2)));
            }
            else
            {
              string tooltip3;
              if (conditionalAttribute.PropertyDisabled(vehicleDef, out tooltip3))
                disabledTooltip = TaggedString.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_SaveableFieldDisabledConditionTooltip", NamedArgument.op_Implicit(tooltip3)));
            }
          }
        }
      }
    }
    if (this.VehicleType != VehicleType.Universal && this.VehicleType != vehicleDef.type)
      disabledTooltip = TaggedString.op_Implicit(Translator.Translate("VF_SaveableFieldDisabledTooltip"));
    bool locked = false;
    HashSet<FieldInfo> fieldInfoSet;
    if (ParsingHelper.LockedFields.TryGetValue(((Def) vehicleDef).defName, out fieldInfoSet) && fieldInfoSet.Contains(field))
    {
      locked = true;
      disabledTooltip = TaggedString.op_Implicit(Translator.Translate("VF_SaveableFieldLockedTooltip"));
    }
    if (GenAttribute.HasAttribute<DisableSettingAttribute>((MemberInfo) field))
      disabledTooltip = TaggedString.op_Implicit(Translator.Translate("VF_DebugDisabledTooltip"));
    if (field.FieldType.GetInterface("ICustomSettingsDrawer") is ICustomSettingsDrawer customSettingsDrawer)
      customSettingsDrawer.DrawSetting(lister, vehicleDef, field, label, tooltip1, disabledTooltip, locked, this.Translate);
    else
      PostToSettingsAttribute.DrawSetting(lister, vehicleDef, field, this.UISettingsType, label, tooltip1, disabledTooltip, locked, this.Translate);
  }

  public static void DrawSetting(
    Listing_Settings lister,
    VehicleDef vehicleDef,
    FieldInfo field,
    UISettingsType settingsType,
    string label,
    string tooltip,
    string disabledTooltip,
    bool locked,
    bool translate)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      SaveableField field1 = new SaveableField((Def) vehicleDef, field);
      switch (settingsType)
      {
        case UISettingsType.None:
          break;
        case UISettingsType.Checkbox:
          lister.CheckboxLabeled(vehicleDef, field1, label, tooltip, disabledTooltip, locked);
          break;
        case UISettingsType.SliderInt:
          SliderValuesAttribute sliderValuesAttribute1;
          if (GenAttribute.TryGetAttribute<SliderValuesAttribute>((MemberInfo) field, ref sliderValuesAttribute1))
          {
            lister.SliderLabeled(vehicleDef, field1, label, tooltip, disabledTooltip, sliderValuesAttribute1.EndSymbol, (int) sliderValuesAttribute1.MinValue, (int) sliderValuesAttribute1.MaxValue, (int) sliderValuesAttribute1.EndValue, sliderValuesAttribute1.MaxValueDisplay, sliderValuesAttribute1.MinValueDisplay, translate);
            break;
          }
          Log.WarningOnce($"Slider declared for SaveableField {field.Name} in {field.DeclaringType} with no SliderValues attribute. Slider will use default values instead.", field.GetHashCode());
          lister.SliderLabeled(vehicleDef, field1, label, tooltip, disabledTooltip, string.Empty, 0, 100, maxValueDisplay: string.Empty, minValueDisplay: string.Empty, translate: translate);
          break;
        case UISettingsType.SliderFloat:
          SliderValuesAttribute sliderValuesAttribute2;
          if (GenAttribute.TryGetAttribute<SliderValuesAttribute>((MemberInfo) field, ref sliderValuesAttribute2))
          {
            lister.SliderLabeled(vehicleDef, field1, label, tooltip, disabledTooltip, sliderValuesAttribute2.EndSymbol, sliderValuesAttribute2.MinValue, sliderValuesAttribute2.MaxValue, sliderValuesAttribute2.RoundDecimalPlaces, sliderValuesAttribute2.EndValue, sliderValuesAttribute2.Increment, sliderValuesAttribute2.MaxValueDisplay, translate);
            break;
          }
          Log.WarningOnce($"Slider declared for SaveableField {field.Name} in {field.DeclaringType} with no SliderValues attribute. Slider will use default values instead.", field.GetHashCode());
          lister.SliderLabeled(vehicleDef, field1, label, tooltip, disabledTooltip, string.Empty, 0.0f, 100f, 0, increment: -1f, endValueDisplay: string.Empty, translate: translate);
          break;
        case UISettingsType.SliderPercent:
          SliderValuesAttribute sliderValuesAttribute3;
          if (GenAttribute.TryGetAttribute<SliderValuesAttribute>((MemberInfo) field, ref sliderValuesAttribute3))
          {
            lister.SliderPercentLabeled(vehicleDef, field1, label, tooltip, disabledTooltip, sliderValuesAttribute3.EndSymbol, sliderValuesAttribute3.MinValue, sliderValuesAttribute3.MaxValue, sliderValuesAttribute3.RoundDecimalPlaces, sliderValuesAttribute3.EndValue, sliderValuesAttribute3.MaxValueDisplay, translate);
            break;
          }
          Log.WarningOnce($"Slider declared for SaveableField {field.Name} in {field.DeclaringType} with no SliderValues attribute. Slider will use default values instead.", field.GetHashCode());
          lister.SliderPercentLabeled(vehicleDef, field1, label, tooltip, disabledTooltip, string.Empty, 0.0f, 100f, 0, endValueDisplay: string.Empty, translate: translate);
          break;
        case UISettingsType.SliderEnum:
          lister.EnumSliderLabeled(vehicleDef, field1, label, tooltip, disabledTooltip, field.FieldType, translate);
          break;
        case UISettingsType.IntegerBox:
          NumericBoxValuesAttribute boxValuesAttribute1;
          if (GenAttribute.TryGetAttribute<NumericBoxValuesAttribute>((MemberInfo) field, ref boxValuesAttribute1))
          {
            lister.IntegerBox(vehicleDef, field1, label, tooltip, disabledTooltip, Mathf.RoundToInt(boxValuesAttribute1.MinValue), Mathf.RoundToInt(boxValuesAttribute1.MaxValue));
            break;
          }
          lister.IntegerBox(vehicleDef, field1, label, tooltip, disabledTooltip, 0);
          break;
        case UISettingsType.FloatBox:
          NumericBoxValuesAttribute boxValuesAttribute2;
          if (GenAttribute.TryGetAttribute<NumericBoxValuesAttribute>((MemberInfo) field, ref boxValuesAttribute2))
          {
            lister.FloatBox(vehicleDef, field1, label, tooltip, disabledTooltip, boxValuesAttribute2.MinValue, boxValuesAttribute2.MaxValue);
            break;
          }
          lister.FloatBox(vehicleDef, field1, label, tooltip, disabledTooltip, 0.0f, float.MaxValue);
          break;
        case UISettingsType.ToggleLabel:
          break;
        default:
          Log.ErrorOnce($"{"[VehicleFramework]"} {settingsType} has not yet been implemented for PostToSettings.DrawLister. Please notify mod author.", settingsType.ToString().GetHashCode());
          break;
      }
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public static void DrawSetting(
    Listing_Settings lister,
    VehicleDef vehicleDef,
    FieldInfo field,
    SettingsValueInfo settingsInfo,
    string label,
    string tooltip,
    string disabledTooltip,
    bool locked,
    bool translate)
  {
    SaveableField field1 = new SaveableField((Def) vehicleDef, field);
    switch (settingsInfo.settingsType)
    {
      case UISettingsType.None:
        break;
      case UISettingsType.Checkbox:
        lister.CheckboxLabeled(vehicleDef, field1, label, tooltip, disabledTooltip, locked);
        break;
      case UISettingsType.SliderInt:
        lister.SliderLabeled(vehicleDef, field1, label, tooltip, disabledTooltip, settingsInfo.endSymbol, (int) settingsInfo.minValue, (int) settingsInfo.maxValue, (int) settingsInfo.endValue, settingsInfo.maxValueDisplay, settingsInfo.minValueDisplay, translate);
        break;
      case UISettingsType.SliderFloat:
        lister.SliderLabeled(vehicleDef, field1, label, tooltip, disabledTooltip, settingsInfo.endSymbol, settingsInfo.minValue, settingsInfo.maxValue, settingsInfo.roundDecimalPlaces, settingsInfo.endValue, settingsInfo.increment, settingsInfo.maxValueDisplay, translate);
        break;
      case UISettingsType.SliderPercent:
        lister.SliderPercentLabeled(vehicleDef, field1, label, tooltip, disabledTooltip, settingsInfo.endSymbol, settingsInfo.minValue, settingsInfo.maxValue, settingsInfo.roundDecimalPlaces, settingsInfo.endValue, settingsInfo.maxValueDisplay, translate);
        break;
      case UISettingsType.SliderEnum:
        lister.EnumSliderLabeled(vehicleDef, field1, label, tooltip, disabledTooltip, field.FieldType, translate);
        break;
      case UISettingsType.IntegerBox:
        lister.IntegerBox(vehicleDef, field1, label, tooltip, disabledTooltip, Mathf.RoundToInt(settingsInfo.minValue), Mathf.RoundToInt(settingsInfo.maxValue));
        break;
      case UISettingsType.FloatBox:
        lister.FloatBox(vehicleDef, field1, label, tooltip, disabledTooltip, settingsInfo.minValue, settingsInfo.maxValue);
        break;
      default:
        Log.ErrorOnce($"{"[VehicleFramework]"} {settingsInfo.settingsType} has not yet been implemented for PostToSettings.DrawLister. Please notify mod author.", settingsInfo.settingsType.ToString().GetHashCode());
        break;
    }
  }
}
