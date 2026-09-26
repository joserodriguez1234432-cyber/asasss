// Decompiled with JetBrains decompiler
// Type: Vehicles.Listing_Settings
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Listing_Settings : Listing_SplitColumns
{
  public static readonly Color modifiedColor = new Color(0.4f, 0.4f, 1f);
  private readonly SettingsPage settings;

  public Listing_Settings(SettingsPage settings, GameFont font = 0)
    : base(font)
  {
    this.settings = settings;
  }

  public Listing_Settings()
    : this(SettingsPage.Vehicles, (GameFont) 0)
  {
  }

  private object GetSettingsValue(VehicleDef def, SaveableField field)
  {
    try
    {
      switch (this.settings)
      {
        case SettingsPage.Vehicles:
          SavedField<object> savedField;
          return VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName].TryGetValue(field, out savedField) ? savedField.EndValue : VehicleMod.settings.vehicles.defaultValues[((Def) def).defName][field];
        case SettingsPage.Upgrades:
          throw new NotImplementedException();
        default:
          throw new NotSupportedException($"Cannot use Listing_Settings with settings set to {this.settings}");
      }
    }
    catch
    {
      Log.Error($"Unable to retrieve field {field.name} for {((Def) def).defName}. Settings=\"{this.settings}\"");
      throw;
    }
  }

  private void SetSettingsValue<T>(VehicleDef def, SaveableField field, T value1, T value2)
  {
    switch (this.settings)
    {
      case SettingsPage.Vehicles:
        VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName][field] = new SavedField<object>((object) value1, (object) value2);
        break;
      case SettingsPage.Upgrades:
        VehicleMod.settings.upgrades.upgradeSettings[((Def) def).defName][field] = new SavedField<object>((object) value1, (object) value2);
        break;
      default:
        throw new NotSupportedException($"Cannot use Listing_SplitColumns with settings set to {this.settings}");
    }
    ActionOnSettingsInputAttribute.InvokeIfApplicable(field.FieldInfo);
  }

  private void SetSettingsValue<T>(VehicleDef def, SaveableField field, T value)
  {
    switch (this.settings)
    {
      case SettingsPage.Vehicles:
        VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName][field] = new SavedField<object>((object) value);
        break;
      case SettingsPage.Upgrades:
        VehicleMod.settings.upgrades.upgradeSettings[((Def) def).defName][field] = new SavedField<object>((object) value);
        break;
      default:
        throw new NotSupportedException($"Cannot use Listing_SplitColumns with settings set to {this.settings}");
    }
    ActionOnSettingsInputAttribute.InvokeIfApplicable(field.FieldInfo);
  }

  private static bool FieldModified(VehicleDef def, SaveableField field)
  {
    return VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName].ContainsKey(field);
  }

  public void CheckboxLabeled(
    VehicleDef def,
    SaveableField field,
    string label,
    string tooltip,
    string disabledTooltip,
    bool locked)
  {
    this.Shift();
    try
    {
      Rect splitRect = this.GetSplitRect(24f);
      bool disabled = !GenText.NullOrEmpty(disabledTooltip);
      bool flag = Mouse.IsOver(splitRect);
      if (disabled)
        TooltipHandler.TipRegion(splitRect, TipSignal.op_Implicit(disabledTooltip));
      else if (!GenText.NullOrEmpty(tooltip))
      {
        if (flag)
          Widgets.DrawHighlight(splitRect);
        TooltipHandler.TipRegion(splitRect, TipSignal.op_Implicit(tooltip));
      }
      if (!disabled & flag && Event.current.type == null && Event.current.button == 1)
      {
        Event.current.Use();
        Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
        {
          new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ResetButton")), (Action) (() =>
          {
            ActionOnSettingsInputAttribute.InvokeIfApplicable(field.FieldInfo);
            VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName].Remove(field);
          }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
        })
        {
          vanishIfMouseDistant = true
        });
      }
      bool checkOn = (bool) this.GetSettingsValue(def, field);
      if (locked)
        checkOn = false;
      if (Listing_Settings.FieldModified(def, field))
        label = ColoredText.Colorize(label, Listing_Settings.modifiedColor);
      if (!UIElements.CheckboxLabeled(splitRect, label, ref checkOn, disabled))
        return;
      this.SetSettingsValue<bool>(def, field, checkOn);
    }
    catch (Exception ex)
    {
      Log.Error($"Unable to convert to bool. Def=\"{((Def) def).defName}\" Field=\"{field.name}\" Exception={ex}");
    }
  }

  public void IntegerBox(
    VehicleDef def,
    SaveableField field,
    string label,
    string tooltip,
    string disabledTooltip,
    int min = -2147483648 /*0x80000000*/,
    int max = 2147483647 /*0x7FFFFFFF*/)
  {
    this.Shift();
    try
    {
      int int32 = Convert.ToInt32(this.GetSettingsValue(def, field));
      bool flag1 = !GenText.NullOrEmpty(disabledTooltip);
      Rect splitRect = this.GetSplitRect(24f);
      float num1 = ((Rect) ref splitRect).y + (float) (((double) ((Rect) ref splitRect).height - (double) Text.LineHeight) / 2.0);
      float num2 = ((Rect) ref splitRect).width * 0.75f;
      float num3 = ((Rect) ref splitRect).width * 0.25f;
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(((Rect) ref splitRect).x, num1, num2, ((Rect) ref splitRect).height);
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector(((Rect) ref splitRect).x + ((Rect) ref splitRect).width - num3, num1, num3, Text.LineHeight);
      bool flag2 = Mouse.IsOver(splitRect);
      if (flag1)
      {
        using (new GUIState.Disabler())
          TooltipHandler.TipRegion(splitRect, TipSignal.op_Implicit(disabledTooltip));
      }
      else if (!GenText.NullOrEmpty(tooltip))
      {
        if (flag2)
          Widgets.DrawHighlight(splitRect);
        TooltipHandler.TipRegion(splitRect, TipSignal.op_Implicit(tooltip));
      }
      if (!flag1 & flag2 && Event.current.type == null && Event.current.button == 1)
      {
        Event.current.Use();
        Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
        {
          new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ResetButton")), (Action) (() =>
          {
            ActionOnSettingsInputAttribute.InvokeIfApplicable(field.FieldInfo);
            VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName].Remove(field);
          }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
        })
        {
          vanishIfMouseDistant = true
        });
      }
      if (Listing_Settings.FieldModified(def, field))
        label = ColoredText.Colorize(label, Listing_Settings.modifiedColor);
      Widgets.Label(rect1, label);
      Text.Anchor = (TextAnchor) 5;
      string str = int32.ToString();
      int num4 = int32;
      Widgets.TextFieldNumeric<int>(rect2, ref int32, ref str, (float) min, (float) max);
      if (num4 == int32)
        return;
      this.SetSettingsValue<int>(def, field, int32);
    }
    catch (Exception ex)
    {
      Log.Error($"Unable to convert to integer. Def=\"{((Def) def).defName}\" Field=\"{field.name}\" Exception={ex}");
    }
  }

  public void FloatBox(
    VehicleDef def,
    SaveableField field,
    string label,
    string tooltip,
    string disabledTooltip,
    float min = -2.14748365E+09f,
    float max = 2.14748365E+09f)
  {
    this.Shift();
    try
    {
      float single = Convert.ToSingle(this.GetSettingsValue(def, field));
      bool flag1 = !GenText.NullOrEmpty(disabledTooltip);
      Rect splitRect = this.GetSplitRect(24f);
      float num1 = ((Rect) ref splitRect).y + (float) (((double) ((Rect) ref splitRect).height - (double) Text.LineHeight) / 2.0);
      float num2 = ((Rect) ref splitRect).width * 0.75f;
      float num3 = ((Rect) ref splitRect).width * 0.25f;
      Rect rect1;
      // ISSUE: explicit constructor call
      ((Rect) ref rect1).\u002Ector(((Rect) ref splitRect).x, num1, num2, ((Rect) ref splitRect).height);
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector(((Rect) ref splitRect).x + ((Rect) ref splitRect).width - num3, num1, num3, Text.LineHeight);
      bool flag2 = Mouse.IsOver(splitRect);
      if (flag1)
      {
        using (new GUIState.Disabler())
          TooltipHandler.TipRegion(splitRect, TipSignal.op_Implicit(disabledTooltip));
      }
      else if (!GenText.NullOrEmpty(tooltip))
      {
        if (flag2)
          Widgets.DrawHighlight(splitRect);
        TooltipHandler.TipRegion(splitRect, TipSignal.op_Implicit(tooltip));
      }
      if (!flag1 & flag2 && Event.current.type == null && Event.current.button == 1)
      {
        Event.current.Use();
        Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
        {
          new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ResetButton")), (Action) (() =>
          {
            VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName].Remove(field);
            ActionOnSettingsInputAttribute.InvokeIfApplicable(field.FieldInfo);
          }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
        })
        {
          vanishIfMouseDistant = true
        });
      }
      if (Listing_Settings.FieldModified(def, field))
        label = ColoredText.Colorize(label, Listing_Settings.modifiedColor);
      Widgets.Label(rect1, label);
      Text.Anchor = (TextAnchor) 5;
      string str = single.ToString();
      float num4 = single;
      Widgets.TextFieldNumeric<float>(rect2, ref single, ref str, min, max);
      if (Mathf.Approximately(num4, single))
        return;
      this.SetSettingsValue<float>(def, field, single);
    }
    catch (Exception ex)
    {
      Log.Error($"Unable to convert to float. Def=\"{((Def) def).defName}\" Field=\"{field.name}\" Exception={ex}");
    }
  }

  public void SliderPercentLabeled(
    VehicleDef def,
    SaveableField field,
    string label,
    string tooltip,
    string disabledTooltip,
    string endSymbol,
    float min,
    float max,
    int decimalPlaces = 2,
    float endValue = -1f,
    string endValueDisplay = "",
    bool translate = false)
  {
    this.Shift();
    try
    {
      float single = Convert.ToSingle(this.GetSettingsValue(def, field));
      bool flag1 = !GenText.NullOrEmpty(disabledTooltip);
      Rect splitRect = this.GetSplitRect(24f);
      Rect rect = splitRect;
      ref Rect local = ref splitRect;
      ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref splitRect).height / 2f;
      string str = $"{Math.Round((double) single * 100.0, decimalPlaces)}" + endSymbol;
      if (!GenText.NullOrEmpty(endValueDisplay) && (double) endValue > 0.0 && (double) single >= (double) endValue)
      {
        str = endValueDisplay;
        if (translate)
          str = TaggedString.op_Implicit(Translator.Translate(str));
      }
      bool flag2 = Mouse.IsOver(rect);
      if (flag1)
      {
        using (new GUIState.Disabler())
          TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(disabledTooltip));
      }
      else if (!GenText.NullOrEmpty(tooltip))
      {
        if (flag2)
          Widgets.DrawHighlight(rect);
        TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
      }
      if (!flag1 & flag2 && Event.current.type == null && Event.current.button == 1)
      {
        Event.current.Use();
        Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
        {
          new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ResetButton")), (Action) (() =>
          {
            ActionOnSettingsInputAttribute.InvokeIfApplicable(field.FieldInfo);
            VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName].Remove(field);
          }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
        })
        {
          vanishIfMouseDistant = true
        });
      }
      if (Listing_Settings.FieldModified(def, field))
        label = ColoredText.Colorize(label, Listing_Settings.modifiedColor);
      float num1 = single;
      float num2 = Widgets.HorizontalSlider(splitRect, single, min, max, false, (string) null, label, str, -1f);
      float num3 = num2;
      if ((double) endValue > 0.0 && (double) num3 >= (double) max)
        num3 = endValue;
      if (Mathf.Approximately(num1, num2))
        return;
      this.SetSettingsValue<float>(def, field, num2, num3);
    }
    catch (Exception ex)
    {
      Log.Error($"Unable to convert to float. Def=\"{((Def) def).defName}\" Field=\"{field.name}\" Exception={ex}");
    }
  }

  public void SliderLabeled(
    VehicleDef def,
    SaveableField field,
    string label,
    string tooltip,
    string disabledTooltip,
    string endSymbol,
    float min,
    float max,
    int decimalPlaces = 2,
    float endValue = -1f,
    float increment = 0.0f,
    string endValueDisplay = "",
    bool translate = false)
  {
    this.Shift();
    try
    {
      float single = Convert.ToSingle(this.GetSettingsValue(def, field));
      bool flag1 = !GenText.NullOrEmpty(disabledTooltip);
      Rect splitRect = this.GetSplitRect(24f);
      Rect rect = splitRect;
      ref Rect local = ref splitRect;
      ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref splitRect).height / 2f;
      string str = $"{Math.Round((double) single, decimalPlaces)}" + endSymbol;
      if (!GenText.NullOrEmpty(endValueDisplay) && (double) single >= (double) max)
      {
        str = endValueDisplay;
        if (translate)
          str = TaggedString.op_Implicit(Translator.Translate(str));
      }
      bool flag2 = Mouse.IsOver(rect);
      if (flag1)
      {
        using (new GUIState.Disabler())
          TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(disabledTooltip));
      }
      else if (!GenText.NullOrEmpty(tooltip))
      {
        if (flag2)
          Widgets.DrawHighlight(rect);
        TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
      }
      if (!flag1 & flag2 && Event.current.type == null && Event.current.button == 1)
      {
        Event.current.Use();
        Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
        {
          new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ResetButton")), (Action) (() =>
          {
            ActionOnSettingsInputAttribute.InvokeIfApplicable(field.FieldInfo);
            VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName].Remove(field);
          }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
        })
        {
          vanishIfMouseDistant = true
        });
      }
      if (Listing_Settings.FieldModified(def, field))
        label = ColoredText.Colorize(label, Listing_Settings.modifiedColor);
      float num1 = single;
      float num2 = Widgets.HorizontalSlider(splitRect, single, min, max, false, (string) null, label, str, -1f);
      float num3 = num2;
      if ((double) increment > 0.0)
      {
        num2 = num2.RoundTo(increment);
        num3 = num3.RoundTo(increment);
      }
      if ((double) endValue > 0.0 && (double) num3 >= (double) max)
        num3 = endValue;
      if (Mathf.Approximately(num1, num2))
        return;
      this.SetSettingsValue<float>(def, field, num2, num3);
    }
    catch (Exception ex)
    {
      Log.Error($"Unable to convert to float. Def=\"{((Def) def).defName}\" Field=\"{field.name}\" Exception={ex}");
    }
  }

  public void SliderLabeled(
    VehicleDef def,
    SaveableField field,
    string label,
    string tooltip,
    string disabledTooltip,
    string endSymbol,
    int min,
    int max,
    int endValue = -1,
    string maxValueDisplay = "",
    string minValueDisplay = "",
    bool translate = false)
  {
    this.Shift();
    try
    {
      int int32 = Convert.ToInt32(this.GetSettingsValue(def, field));
      bool flag1 = !GenText.NullOrEmpty(disabledTooltip);
      Rect splitRect = this.GetSplitRect(24f);
      Rect rect = splitRect;
      ref Rect local = ref splitRect;
      ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref splitRect).height / 2f;
      string str = string.Format("{0}" + endSymbol, (object) int32);
      if (!GenText.NullOrEmpty(maxValueDisplay) && int32 == max)
      {
        str = maxValueDisplay;
        if (translate)
          str = TaggedString.op_Implicit(Translator.Translate(str));
      }
      if (!GenText.NullOrEmpty(minValueDisplay) && int32 == min)
      {
        str = minValueDisplay;
        if (translate)
          str = TaggedString.op_Implicit(Translator.Translate(str));
      }
      bool flag2 = Mouse.IsOver(rect);
      if (flag1)
      {
        using (new GUIState.Disabler())
          TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(disabledTooltip));
      }
      else if (!GenText.NullOrEmpty(tooltip))
      {
        if (flag2)
          Widgets.DrawHighlight(rect);
        TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
      }
      if (!flag1 & flag2 && Event.current.type == null && Event.current.button == 1)
      {
        Event.current.Use();
        Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
        {
          new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ResetButton")), (Action) (() =>
          {
            ActionOnSettingsInputAttribute.InvokeIfApplicable(field.FieldInfo);
            VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName].Remove(field);
          }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
        })
        {
          vanishIfMouseDistant = true
        });
      }
      if (Listing_Settings.FieldModified(def, field))
        label = ColoredText.Colorize(label, Listing_Settings.modifiedColor);
      int num1 = int32;
      int num2 = (int) Widgets.HorizontalSlider(splitRect, (float) int32, (float) min, (float) max, false, (string) null, label, str, -1f);
      int num3 = num2;
      if (num3 >= max && endValue > 0)
        num3 = endValue;
      if (num1 == num2)
        return;
      this.SetSettingsValue<int>(def, field, num2, num3);
    }
    catch (Exception ex)
    {
      Log.Error($"Unable to convert to int. Def=\"{((Def) def).defName}\" Field=\"{field.name}\" Exception={ex}");
    }
  }

  public void EnumSliderLabeled(
    VehicleDef def,
    SaveableField field,
    string label,
    string tooltip,
    string disabledTooltip,
    System.Type enumType,
    bool translate = false)
  {
    this.Shift();
    try
    {
      int int32 = Convert.ToInt32(this.GetSettingsValue(def, field));
      bool flag1 = !GenText.NullOrEmpty(disabledTooltip);
      int[] array = Enum.GetValues(enumType).Cast<int>().ToArray<int>();
      int num1 = array[0];
      int num2 = ((IEnumerable<int>) array).Last<int>();
      Rect splitRect = this.GetSplitRect(24f);
      Rect rect = splitRect;
      ref Rect local = ref splitRect;
      ((Rect) ref local).y = ((Rect) ref local).y + ((Rect) ref splitRect).height / 2f;
      string str = Enum.GetName(enumType, (object) int32);
      if (translate)
        str = TaggedString.op_Implicit(Translator.Translate(str));
      bool flag2 = Mouse.IsOver(rect);
      if (flag1)
      {
        using (new GUIState.Disabler())
          TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(disabledTooltip));
      }
      else if (!GenText.NullOrEmpty(tooltip))
      {
        if (flag2)
          Widgets.DrawHighlight(rect);
        TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(tooltip));
      }
      if (!flag1 & flag2 && Event.current.type == null && Event.current.button == 1)
      {
        Event.current.Use();
        Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>()
        {
          new FloatMenuOption(TaggedString.op_Implicit(Translator.Translate("ResetButton")), (Action) (() =>
          {
            ActionOnSettingsInputAttribute.InvokeIfApplicable(field.FieldInfo);
            VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName].Remove(field);
          }), (MenuOptionPriority) 4, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0)
        })
        {
          vanishIfMouseDistant = true
        });
      }
      if (Listing_Settings.FieldModified(def, field))
        label = ColoredText.Colorize(label, Listing_Settings.modifiedColor);
      int num3 = int32;
      int num4 = (int) Widgets.HorizontalSlider(splitRect, (float) int32, (float) num1, (float) num2, false, (string) null, label, str, -1f);
      if (num3 == num4)
        return;
      this.SetSettingsValue<int>(def, field, num4);
    }
    catch (Exception ex)
    {
      Log.Error($"Unable to convert to int. Def=\"{((Def) def).defName}\" Field=\"{field.name}\" Exception={ex}");
    }
  }
}
