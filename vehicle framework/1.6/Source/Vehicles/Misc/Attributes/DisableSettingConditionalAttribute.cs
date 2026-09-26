// Decompiled with JetBrains decompiler
// Type: Vehicles.DisableSettingConditionalAttribute
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
[UsedImplicitly]
public class DisableSettingConditionalAttribute : Attribute
{
  private const string Unassigned = "Unassigned";

  public System.Type MemberType { get; set; }

  public string Property { get; set; }

  public string Field { get; set; }

  public object DisableIfEqualTo { get; set; } = (object) nameof (Unassigned);

  public object DisableIfNotEqualTo { get; set; } = (object) nameof (Unassigned);

  public string DisableReason { get; set; }

  public string MayRequire { get; set; }

  public string[] MayRequireAny { get; set; }

  public string[] MayRequireAll { get; set; }

  public bool PropertyDisabled(VehicleDef vehicleDef, out string tooltip)
  {
    tooltip = string.Empty;
    if (GenText.NullOrEmpty(this.Property))
      return false;
    if (this.MemberType == (System.Type) null)
    {
      Log.Warning("DisableSettingConditional has Property name, not null MemberType. MemberType property must be included for PropertyInfo to be evaluated.");
      return false;
    }
    PropertyInfo propertyInfo = AccessTools.Property(this.MemberType, this.Property);
    if (propertyInfo == (PropertyInfo) null || propertyInfo.GetGetMethod() == (MethodInfo) null)
    {
      Log.Error($"MemberInfo for {this.MemberType}.{this.Property} not found.");
      return false;
    }
    object obj1 = DisableSettingConditionalAttribute.Parent(vehicleDef, (MemberInfo) propertyInfo);
    if (obj1 != null || propertyInfo.GetGetMethod().IsStatic)
    {
      if (!this.DisableIfEqualTo.Equals((object) "Unassigned"))
      {
        object obj2 = propertyInfo.GetValue(obj1);
        tooltip = !GenText.NullOrEmpty(this.DisableReason) ? TaggedString.op_Implicit(Translator.Translate(this.DisableReason)) : DisableSettingConditionalAttribute.BuildDisabledReport((MemberInfo) propertyInfo, "=", obj2);
        return obj2.Equals(this.DisableIfEqualTo);
      }
      if (!this.DisableIfNotEqualTo.Equals((object) "Unassigned"))
      {
        object obj3 = propertyInfo.GetValue(obj1);
        tooltip = !GenText.NullOrEmpty(this.DisableReason) ? TaggedString.op_Implicit(Translator.Translate(this.DisableReason)) : DisableSettingConditionalAttribute.BuildDisabledReport((MemberInfo) propertyInfo, "≠", obj3);
        return !obj3.Equals(this.DisableIfNotEqualTo);
      }
    }
    return false;
  }

  public bool FieldDisabled(VehicleDef vehicleDef, out string tooltip)
  {
    tooltip = string.Empty;
    if (GenText.NullOrEmpty(this.Field))
      return false;
    if (this.MemberType == (System.Type) null)
    {
      Log.Warning("DisableSettingConditional has Field name, not null MemberType. MemberType property must be included for FieldInfo to be evaluated.");
      return false;
    }
    FieldInfo field = AccessTools.Field(this.MemberType, this.Field);
    if (field == (FieldInfo) null)
    {
      Log.Error($"FieldInfo for {this.MemberType}.{this.Field} not found.");
      return false;
    }
    object obj1 = DisableSettingConditionalAttribute.Parent(vehicleDef, (MemberInfo) field);
    if (obj1 != null || field.IsStatic)
    {
      if (!this.DisableIfEqualTo.Equals((object) "Unassigned"))
      {
        object obj2;
        if (!SettingsCache.TryGetValue<object>(vehicleDef, field, out obj2))
          obj2 = field.GetValue(obj1);
        tooltip = !GenText.NullOrEmpty(this.DisableReason) ? TaggedString.op_Implicit(Translator.Translate(this.DisableReason)) : DisableSettingConditionalAttribute.BuildDisabledReport((MemberInfo) field, "=", obj2);
        return obj2.Equals(this.DisableIfEqualTo);
      }
      if (!this.DisableIfNotEqualTo.Equals((object) "Unassigned"))
      {
        object obj3;
        if (!SettingsCache.TryGetValue<object>(vehicleDef, field, out obj3))
          obj3 = field.GetValue(obj1);
        tooltip = !GenText.NullOrEmpty(this.DisableReason) ? TaggedString.op_Implicit(Translator.Translate(this.DisableReason)) : DisableSettingConditionalAttribute.BuildDisabledReport((MemberInfo) field, "≠", obj3);
        return !obj3.Equals(this.DisableIfNotEqualTo);
      }
    }
    return false;
  }

  private static string BuildDisabledReport(
    MemberInfo memberInfo,
    string comparisonLabel,
    object value)
  {
    string str = memberInfo.Name;
    PostToSettingsAttribute settingsAttribute;
    if (GenAttribute.TryGetAttribute<PostToSettingsAttribute>(memberInfo, ref settingsAttribute))
      str = settingsAttribute.ResolvedLabel();
    return $"{str} {comparisonLabel} {value}";
  }

  private static object Parent(VehicleDef vehicleDef, MemberInfo memberInfo)
  {
    if (memberInfo.DeclaringType == typeof (VehicleDef))
      return (object) vehicleDef;
    object obj = DisableSettingConditionalAttribute.IterateTypesForParent((object) vehicleDef, memberInfo);
    if (obj == null && !GenList.NullOrEmpty<CompProperties>((IList<CompProperties>) vehicleDef.comps))
    {
      foreach (object comp in vehicleDef.comps)
      {
        obj = DisableSettingConditionalAttribute.IterateTypesForParent(comp, memberInfo);
        if (obj != null)
          return obj;
      }
    }
    return obj;
  }

  private static object IterateTypesForParent(object parent, MemberInfo memberInfo)
  {
    if (parent == null)
      return (object) null;
    foreach (FieldInfo field in parent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
    {
      if ((MemberInfo) field == memberInfo)
        return parent;
      PostToSettingsAttribute settingsAttribute;
      if (GenAttribute.TryGetAttribute<PostToSettingsAttribute>((MemberInfo) field, ref settingsAttribute) && settingsAttribute.ParentHolder)
      {
        object obj = DisableSettingConditionalAttribute.IterateTypesForParent(field.GetValue(parent), (MemberInfo) field);
        if (obj != null)
          return obj;
      }
    }
    return (object) null;
  }
}
