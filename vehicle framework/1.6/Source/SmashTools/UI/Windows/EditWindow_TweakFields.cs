// Decompiled with JetBrains decompiler
// Type: SmashTools.EditWindow_TweakFields
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using LudeonTK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public class EditWindow_TweakFields : EditWindow
{
  private const float VectorLabelProportion = 0.5f;
  private const float VectorSubLabelProportion = 0.15f;
  private const float FloatRangeLabelProportion = 0.5f;
  private const float FloatRangeSubLabelProportion = 0.25f;
  private const float RowHeight = 24f;
  private const int ColumnCount = 2;
  private readonly Thing thing;
  private static List<EditWindow_TweakFields.TweakInfo> instanceTweaks;
  private Vector2 scrollPosition;
  private readonly Listing_SplitColumns listing = new Listing_SplitColumns();
  private static readonly Dictionary<FieldInfo, EditWindow_TweakFields.UiSettings> registeredFields = new Dictionary<FieldInfo, EditWindow_TweakFields.UiSettings>();

  public virtual bool IsDebug => true;

  public EditWindow_TweakFields(Thing thing)
  {
    this.thing = thing;
    ((Window) this).optionalTitle = "TweakValues";
    EditWindow_TweakFields.instanceTweaks = EditWindow_TweakFields.FindAllTweakablesRecursive(thing).OrderBy<EditWindow_TweakFields.TweakInfo, string>((Func<EditWindow_TweakFields.TweakInfo, string>) (info => info.ui.category)).ThenBy<EditWindow_TweakFields.TweakInfo, string>((Func<EditWindow_TweakFields.TweakInfo, string>) (info => info.ui.subCategory)).ThenBy<EditWindow_TweakFields.TweakInfo, string>((Func<EditWindow_TweakFields.TweakInfo, string>) (info => info.fieldInfo.DeclaringType?.Name)).ThenBy<EditWindow_TweakFields.TweakInfo, string>((Func<EditWindow_TweakFields.TweakInfo, string>) (info => info.Name)).ToList<EditWindow_TweakFields.TweakInfo>();
  }

  public virtual Vector2 InitialSize
  {
    get => new Vector2((float) UI.screenWidth / 2f, (float) UI.screenHeight * 0.9f);
  }

  private float CachedHeight { get; set; } = -1f;

  public static void RegisterField(
    FieldInfo fieldInfo,
    string category,
    string subCategory,
    UISettingsType settingsType)
  {
    if (EditWindow_TweakFields.registeredFields.ContainsKey(fieldInfo))
      return;
    EditWindow_TweakFields.registeredFields.Add(fieldInfo, new EditWindow_TweakFields.UiSettings(category, subCategory, settingsType));
  }

  private static IEnumerable<EditWindow_TweakFields.TweakInfo> FindAllTweakablesRecursive(
    Thing thing)
  {
    IEnumerator<EditWindow_TweakFields.TweakInfo> enumerator1 = EditWindow_TweakFields.FindAllTweakablesRecursive(thing.GetType(), (object) thing, ((Entity) thing).Label, string.Empty).GetEnumerator();
    while (enumerator1.MoveNext())
      yield return enumerator1.Current;
    enumerator1 = (IEnumerator<EditWindow_TweakFields.TweakInfo>) null;
    enumerator1 = EditWindow_TweakFields.FindAllTweakablesRecursive(thing.def.GetType(), (object) thing.def, ((Def) thing.def).defName, string.Empty).GetEnumerator();
    while (enumerator1.MoveNext())
      yield return enumerator1.Current;
    enumerator1 = (IEnumerator<EditWindow_TweakFields.TweakInfo>) null;
    if (!thing.def.comps.NullOrEmpty<CompProperties>())
    {
      List<CompProperties>.Enumerator enumerator2 = thing.def.comps.GetEnumerator();
      while (enumerator2.MoveNext())
      {
        CompProperties current = enumerator2.Current;
        enumerator1 = EditWindow_TweakFields.FindAllTweakablesRecursive(current.GetType(), (object) current, current.GetType().Name, string.Empty).GetEnumerator();
        while (enumerator1.MoveNext())
          yield return enumerator1.Current;
        enumerator1 = (IEnumerator<EditWindow_TweakFields.TweakInfo>) null;
      }
      enumerator2 = new List<CompProperties>.Enumerator();
    }
    if (thing is ThingWithComps thingWithComps)
    {
      foreach (ThingComp allComp in thingWithComps.AllComps)
      {
        enumerator1 = EditWindow_TweakFields.FindAllTweakablesRecursive(allComp.GetType(), (object) allComp, allComp.GetType().Name, string.Empty).GetEnumerator();
        while (enumerator1.MoveNext())
          yield return enumerator1.Current;
        enumerator1 = (IEnumerator<EditWindow_TweakFields.TweakInfo>) null;
      }
    }
  }

  private static IEnumerable<EditWindow_TweakFields.TweakInfo> FindAllTweakablesRecursive(
    Type type,
    object parent,
    string category,
    string subCategory)
  {
    FieldInfo[] fieldInfoArray = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
    for (int index1 = 0; index1 < fieldInfoArray.Length; ++index1)
    {
      FieldInfo fieldInfo = fieldInfoArray[index1];
      if (GenAttribute.TryGetAttribute<TweakFieldAttribute>((MemberInfo) fieldInfo) != null || EditWindow_TweakFields.registeredFields.ContainsKey(fieldInfo))
      {
        if (fieldInfo.IsStatic)
        {
          Log.Error($"Cannot use TweakFieldAttribute on static fields. Use vanilla's TweakValues for static fields instead. Field={fieldInfo.DeclaringType.Name}.{fieldInfo.Name}");
          continue;
        }
        if (fieldInfo.IsLiteral)
        {
          Log.Error($"Cannot use TweakFieldAttribute on constants. Field={fieldInfo.DeclaringType.Name}.{fieldInfo.Name}");
          continue;
        }
        if (fieldInfo.FieldType.IsClass)
        {
          if (fieldInfo.FieldType.IsIList())
          {
            if (!fieldInfo.FieldType.GetGenericArguments()[0].IsClass)
            {
              Log.Error($"Cannot use TweakFieldAttribute on list of non-reference types. Field={fieldInfo.DeclaringType.Name}.{fieldInfo.Name}");
              continue;
            }
            IList list = (IList) fieldInfo.GetValue(parent);
            if (list != null)
            {
              int index2 = 0;
              foreach (object obj in (IEnumerable) list)
              {
                (string category1, string subCategory1) = EditWindow_TweakFields.GetCategory(fieldInfo, obj, category, subCategory);
                List<EditWindow_TweakFields.TweakInfo>.Enumerator enumerator = EditWindow_TweakFields.FindAllTweakablesRecursive(obj.GetType(), obj, category1, subCategory1).ToList<EditWindow_TweakFields.TweakInfo>().GetEnumerator();
                while (enumerator.MoveNext())
                {
                  EditWindow_TweakFields.TweakInfo current = enumerator.Current;
                  if (list.Count > 1)
                    current.IndexInList = index2;
                  yield return current;
                }
                enumerator = new List<EditWindow_TweakFields.TweakInfo>.Enumerator();
                ++index2;
              }
            }
            list = (IList) null;
          }
          else
          {
            object obj = fieldInfo.GetValue(parent);
            if (obj != null)
            {
              (string category2, string subCategory2) = EditWindow_TweakFields.GetCategory(fieldInfo, obj, category, subCategory);
              foreach (EditWindow_TweakFields.TweakInfo tweakInfo in EditWindow_TweakFields.FindAllTweakablesRecursive(fieldInfo.FieldType, obj, category2, subCategory2))
                yield return tweakInfo;
            }
          }
        }
        else
        {
          UISettingsType settingsType = UISettingsType.None;
          TweakFieldAttribute attribute = GenAttribute.TryGetAttribute<TweakFieldAttribute>((MemberInfo) fieldInfo);
          if (attribute != null)
          {
            settingsType = attribute.SettingsType;
          }
          else
          {
            EditWindow_TweakFields.UiSettings uiSettings;
            if (EditWindow_TweakFields.registeredFields.TryGetValue(fieldInfo, out uiSettings))
              settingsType = uiSettings.settingsType;
          }
          yield return EditWindow_TweakFields.CreateInfo(fieldInfo, parent, category, subCategory, settingsType);
        }
      }
      fieldInfo = (FieldInfo) null;
    }
    fieldInfoArray = (FieldInfo[]) null;
  }

  private static (string category, string subCategory) GetCategory(
    FieldInfo fieldInfo,
    object instance,
    string category,
    string subCategory)
  {
    if (instance is ITweakFields tweakFields)
    {
      if (!tweakFields.Category.NullOrEmpty<char>())
        category = tweakFields.Category;
      if (!tweakFields.Label.NullOrEmpty<char>())
        subCategory = tweakFields.Label;
    }
    else
    {
      TweakFieldAttribute attribute = GenAttribute.TryGetAttribute<TweakFieldAttribute>((MemberInfo) fieldInfo);
      if (attribute != null)
      {
        if (!attribute.Category.NullOrEmpty<char>())
          category = attribute.Category;
        if (!attribute.SubCategory.NullOrEmpty<char>())
          subCategory = attribute.SubCategory;
      }
      else
      {
        EditWindow_TweakFields.UiSettings uiSettings;
        if (EditWindow_TweakFields.registeredFields.TryGetValue(fieldInfo, out uiSettings))
        {
          if (!uiSettings.category.NullOrEmpty<char>())
            category = uiSettings.category;
          if (!uiSettings.subCategory.NullOrEmpty<char>())
            subCategory = uiSettings.subCategory;
        }
      }
    }
    return (category, subCategory);
  }

  private static EditWindow_TweakFields.TweakInfo CreateInfo(
    FieldInfo fieldInfo,
    object instance,
    string category,
    string subCategory,
    UISettingsType settingsType)
  {
    return new EditWindow_TweakFields.TweakInfo()
    {
      fieldInfo = fieldInfo,
      instance = instance,
      ui = new EditWindow_TweakFields.UiSettings(category, subCategory, settingsType),
      IndexInList = -1
    };
  }

  private void RecacheHeight(float viewWidth)
  {
    this.CachedHeight = 0.0f;
    string enumerable1 = string.Empty;
    string enumerable2 = string.Empty;
    int num = 0;
    foreach (EditWindow_TweakFields.TweakInfo instanceTweak in EditWindow_TweakFields.instanceTweaks)
    {
      if (instanceTweak.ui.category != enumerable1)
      {
        enumerable1 = instanceTweak.ui.category;
        if (!enumerable1.NullOrEmpty<char>())
        {
          TextBlock textBlock;
          // ISSUE: explicit constructor call
          ((TextBlock) ref textBlock).\u002Ector((GameFont) 2);
          try
          {
            this.CachedHeight += Text.CalcHeight(enumerable1, viewWidth) + 2f;
            num = 0;
          }
          finally
          {
            textBlock.Dispose();
          }
        }
      }
      if (instanceTweak.ui.subCategory != enumerable2)
      {
        enumerable2 = instanceTweak.ui.subCategory;
        if (!enumerable2.NullOrEmpty<char>())
        {
          TextBlock textBlock;
          // ISSUE: explicit constructor call
          ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
          try
          {
            this.CachedHeight += Text.CalcHeight(enumerable2, viewWidth) + 2f;
            num = 0;
          }
          finally
          {
            textBlock.Dispose();
          }
        }
      }
      TextBlock textBlock1;
      // ISSUE: explicit constructor call
      ((TextBlock) ref textBlock1).\u002Ector((GameFont) 0);
      try
      {
        if (this.DrawField(instanceTweak, out bool _))
        {
          ++num;
          if (num > 2)
            num = 1;
          if (num == 1)
            this.CachedHeight += 34f;
        }
      }
      finally
      {
        textBlock1.Dispose();
      }
    }
  }

  public virtual void DoWindowContents(Rect inRect)
  {
    TextBlock textBlock1;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock1).\u002Ector((GameFont) 1);
    try
    {
      Rect rect1;
      Rect rect2 = rect1 = GenUI.ContractedBy(inRect, 4f);
      ref Rect local = ref rect1;
      ((Rect) ref local).xMax = ((Rect) ref local).xMax - 33f;
      float viewWidth = ((Rect) ref rect1).width - 16f;
      if ((double) this.CachedHeight < 0.0)
        this.RecacheHeight(viewWidth);
      Rect rect3 = GenUI.ContractedBy(new Rect(0.0f, 0.0f, viewWidth, this.CachedHeight), 4f);
      Widgets.BeginScrollView(rect2, ref this.scrollPosition, rect3, true);
      this.listing.Begin(rect3, 2);
      string str1 = string.Empty;
      string str2 = string.Empty;
      foreach (EditWindow_TweakFields.TweakInfo instanceTweak in EditWindow_TweakFields.instanceTweaks)
      {
        if (instanceTweak.ui.category != str1)
        {
          str1 = instanceTweak.ui.category;
          if (!str1.NullOrEmpty<char>())
            this.listing.Header(str1, ListingExtension.BannerColor, (GameFont) 2, (TextAnchor) 4, 24f);
        }
        if (instanceTweak.ui.subCategory != str2)
        {
          str2 = instanceTweak.ui.subCategory;
          if (!str2.NullOrEmpty<char>())
            this.listing.Header(str2, ListingExtension.BannerColor, (GameFont) 1, (TextAnchor) 4, 24f);
        }
        TextBlock textBlock2;
        // ISSUE: explicit constructor call
        ((TextBlock) ref textBlock2).\u002Ector((GameFont) 0);
        try
        {
          bool fieldChanged;
          if (this.DrawField(instanceTweak, out fieldChanged) & fieldChanged)
            EditWindow_TweakFields.FieldChanged();
        }
        finally
        {
          textBlock2.Dispose();
        }
      }
      ((Listing) this.listing).End();
      Widgets.EndScrollView();
      if (!Mouse.IsOver(inRect) || Event.current.type != 6)
        return;
      Event.current.Use();
    }
    finally
    {
      textBlock1.Dispose();
    }
  }

  private bool DrawField(EditWindow_TweakFields.TweakInfo info, out bool fieldChanged)
  {
    fieldChanged = false;
    UISettingsType settingsType = info.ui.settingsType;
    switch (info.ui.settingsType)
    {
      case UISettingsType.None:
        return false;
      case UISettingsType.Checkbox:
        bool flag;
        if (!info.TryGetValue<bool>(out flag))
          return false;
        bool checkState = flag;
        this.listing.CheckboxLabeled(info.Name, ref checkState, string.Empty, string.Empty, false, new float?(24f));
        if (flag != checkState)
        {
          info.SetValue<bool>(checkState);
          fieldChanged = true;
        }
        return true;
      case UISettingsType.SliderInt:
        int num1;
        if (!info.TryGetValue<int>(out num1))
          return false;
        int num2 = num1;
        SliderValuesAttribute sliderValuesAttribute1;
        if (GenAttribute.TryGetAttribute<SliderValuesAttribute>((MemberInfo) info.fieldInfo, ref sliderValuesAttribute1))
        {
          this.listing.SliderLabeled(info.Name, ref num2, string.Empty, string.Empty, sliderValuesAttribute1.EndSymbol, Mathf.RoundToInt(sliderValuesAttribute1.MinValue), Mathf.RoundToInt(sliderValuesAttribute1.MaxValue), (int) sliderValuesAttribute1.EndValue, sliderValuesAttribute1.MaxValueDisplay, sliderValuesAttribute1.MinValueDisplay);
        }
        else
        {
          Log.WarningOnce($"Slider declared {info.fieldInfo.DeclaringType}.{info.fieldInfo.Name} with no " + "SliderValues attribute. Slider will use default values instead.", info.fieldInfo.GetHashCode());
          this.listing.SliderLabeled(info.Name, ref num2, string.Empty, string.Empty, string.Empty, 0, 100);
        }
        if (num1 != num2)
        {
          info.SetValue<int>(num2);
          fieldChanged = true;
        }
        return true;
      case UISettingsType.SliderFloat:
        float num3;
        if (!info.TryGetValue<float>(out num3))
          return false;
        float num4 = num3;
        SliderValuesAttribute sliderValuesAttribute2;
        if (GenAttribute.TryGetAttribute<SliderValuesAttribute>((MemberInfo) info.fieldInfo, ref sliderValuesAttribute2))
        {
          this.listing.SliderLabeled(info.Name, ref num4, string.Empty, string.Empty, sliderValuesAttribute2.EndSymbol, sliderValuesAttribute2.MinValue, sliderValuesAttribute2.MaxValue, sliderValuesAttribute2.RoundDecimalPlaces, sliderValuesAttribute2.EndValue, sliderValuesAttribute2.Increment);
        }
        else
        {
          Log.WarningOnce($"Slider declared {info.fieldInfo.DeclaringType}.{info.fieldInfo.Name} with no " + "SliderValues attribute. Slider will use default values instead.", info.fieldInfo.GetHashCode());
          this.listing.SliderLabeled(info.Name, ref num4, string.Empty, string.Empty, string.Empty, 0.0f, 100f);
        }
        if (!Mathf.Approximately(num3, num4))
        {
          info.SetValue<float>(num4);
          fieldChanged = true;
        }
        return true;
      case UISettingsType.SliderPercent:
        float num5;
        if (!info.TryGetValue<float>(out num5))
          return false;
        float num6 = num5;
        SliderValuesAttribute sliderValuesAttribute3;
        if (GenAttribute.TryGetAttribute<SliderValuesAttribute>((MemberInfo) info.fieldInfo, ref sliderValuesAttribute3))
        {
          this.listing.SliderPercentLabeled(info.Name, ref num6, string.Empty, string.Empty, sliderValuesAttribute3.EndSymbol, sliderValuesAttribute3.MinValue, sliderValuesAttribute3.MaxValue, sliderValuesAttribute3.RoundDecimalPlaces, sliderValuesAttribute3.EndValue);
        }
        else
        {
          Log.WarningOnce($"Slider declared {info.fieldInfo.DeclaringType}.{info.fieldInfo.Name} with no " + "SliderValues attribute. Slider will use default values instead.", info.fieldInfo.GetHashCode());
          this.listing.SliderPercentLabeled(info.Name, ref num6, string.Empty, string.Empty, "%", 0.0f, 1f, 0);
        }
        if (!Mathf.Approximately(num5, num6))
        {
          info.SetValue<float>(num6);
          fieldChanged = true;
        }
        return true;
      case UISettingsType.SliderEnum:
        int num7;
        if (!info.TryGetValue<int>(out num7))
          return false;
        int num8 = num7;
        this.listing.EnumSliderLabeled(info.Name, ref num8, string.Empty, string.Empty, info.fieldInfo.FieldType);
        if (num7 != num8)
        {
          info.SetValue<int>(num8);
          fieldChanged = true;
        }
        return true;
      case UISettingsType.IntegerBox:
        int num9;
        if (!info.TryGetValue<int>(out num9))
          return false;
        int num10 = num9;
        NumericBoxValuesAttribute boxValuesAttribute1;
        if (GenAttribute.TryGetAttribute<NumericBoxValuesAttribute>((MemberInfo) info.fieldInfo, ref boxValuesAttribute1))
          this.listing.IntegerBox(info.Name, ref num10, string.Empty, string.Empty, Mathf.RoundToInt(boxValuesAttribute1.MinValue), Mathf.RoundToInt(boxValuesAttribute1.MaxValue), new float?(24f));
        else
          this.listing.IntegerBox(info.Name, ref num10, string.Empty, string.Empty, 0, lineHeight: new float?(24f));
        if (num9 != num10)
        {
          info.SetValue<int>(num10);
          fieldChanged = true;
        }
        return true;
      case UISettingsType.FloatBox:
        Vector2 vector2_1;
        if (info.TryGetValue<Vector2>(out vector2_1))
        {
          Vector2 vector2_2 = vector2_1;
          this.listing.Vector2Box(info.Name, ref vector2_2, buffer: 5f);
          if (Vector2.op_Inequality(vector2_1, vector2_2))
          {
            info.SetValue<Vector2>(vector2_2);
            fieldChanged = true;
          }
          return true;
        }
        FloatRange floatRange1;
        if (info.TryGetValue<FloatRange>(out floatRange1))
        {
          FloatRange floatRange2 = floatRange1;
          this.listing.FloatRangeBox(info.Name, ref floatRange2, buffer: 5f);
          if (FloatRange.op_Inequality(floatRange1, floatRange2))
          {
            info.SetValue<FloatRange>(floatRange2);
            fieldChanged = true;
          }
          return true;
        }
        Vector3 vector3_1;
        if (info.TryGetValue<Vector3>(out vector3_1))
        {
          Vector3 vector3_2 = vector3_1;
          this.listing.Vector3Box(info.Name, ref vector3_2, buffer: 5f);
          if (Vector3.op_Inequality(vector3_1, vector3_2))
          {
            info.SetValue<Vector3>(vector3_2);
            fieldChanged = true;
          }
          return true;
        }
        float num11;
        if (!info.TryGetValue<float>(out num11))
          return false;
        float num12 = num11;
        NumericBoxValuesAttribute boxValuesAttribute2;
        if (GenAttribute.TryGetAttribute<NumericBoxValuesAttribute>((MemberInfo) info.fieldInfo, ref boxValuesAttribute2))
          this.listing.FloatBox(info.Name, ref num12, string.Empty, string.Empty, boxValuesAttribute2.MinValue, boxValuesAttribute2.MaxValue, new float?(24f));
        else
          this.listing.FloatBox(info.Name, ref num12, string.Empty, string.Empty, 0.0f, float.MaxValue, new float?(24f));
        if (!Mathf.Approximately(num11, num12))
        {
          info.SetValue<float>(num12);
          fieldChanged = true;
        }
        return true;
      case UISettingsType.ToggleLabel:
        Color white = Color.white;
        Color mouseOver;
        // ISSUE: explicit constructor call
        ((Color) ref mouseOver).\u002Ector(0.1f, 0.85f, 0.85f);
        Color color;
        // ISSUE: explicit constructor call
        ((Color) ref color).\u002Ector(mouseOver.r - 0.15f, mouseOver.g - 0.15f, mouseOver.b - 0.15f);
        Rot4 rot4;
        if (info.TryGetValue<Rot4>(out rot4))
        {
          if (this.listing.ClickableLabel(info.Name, ((Rot4) ref rot4).ToStringWord(), mouseOver, white, new Color?(color), new float?(24f)))
          {
            ((Rot4) ref rot4).Rotate((RotationDirection) 1);
            info.SetValue<Rot4>(rot4);
            fieldChanged = true;
          }
          return true;
        }
        Rot8 rot8;
        if (!info.TryGetValue<Rot8>(out rot8))
          return false;
        if (this.listing.ClickableLabel(info.Name, rot8.ToStringNamed(), mouseOver, white, new Color?(color), new float?(24f)))
        {
          rot8.Rotate((RotationDirection) 1);
          info.SetValue<Rot8>(rot8);
          fieldChanged = true;
        }
        return true;
      default:
        Log.ErrorOnce($"{settingsType} has not yet been implemented for PostToSettings.DrawLister. Please notify SmashPhil.", settingsType.ToString().GetHashCode());
        return false;
    }
  }

  private static void FieldChanged()
  {
    foreach (EditWindow_TweakFields.TweakInfo instanceTweak in EditWindow_TweakFields.instanceTweaks)
    {
      if (instanceTweak.instance is ITweakFields instance)
        instance.OnFieldChanged();
    }
  }

  private class UiSettings(string category, string subCategory, UISettingsType settingsType)
  {
    public readonly string category = category;
    public readonly string subCategory = subCategory;
    public readonly UISettingsType settingsType = settingsType;
  }

  private class TweakInfo
  {
    public FieldInfo fieldInfo;
    public object instance;
    public EditWindow_TweakFields.UiSettings ui;

    public string Name
    {
      get
      {
        return this.IndexInList >= 0 ? $"{this.fieldInfo.Name}_{this.IndexInList + 1}" : this.fieldInfo.Name;
      }
    }

    public int IndexInList { get; internal set; }

    public bool TryGetValue<T>(out T value) where T : struct
    {
      value = default (T);
      if (typeof (T) != this.fieldInfo.FieldType && typeof (T) != Nullable.GetUnderlyingType(this.fieldInfo.FieldType) && (!this.fieldInfo.FieldType.IsEnum || typeof (T) != typeof (int)))
        return false;
      object obj = this.fieldInfo.GetValue(this.instance);
      if (Nullable.GetUnderlyingType(this.fieldInfo.FieldType) == typeof (T))
      {
        T? nullable = (T?) obj;
        if (!nullable.HasValue)
          return false;
        obj = (object) nullable.Value;
      }
      if (obj.GetType() != typeof (T) && (!this.fieldInfo.FieldType.IsEnum || typeof (T) != typeof (int)))
      {
        Log.Error($"Invalid Cast: {obj.GetType()} to {typeof (T)}");
        return false;
      }
      value = (T) obj;
      return true;
    }

    public void SetValue<T>(T value) => this.fieldInfo.SetValue(this.instance, (object) value);
  }
}
