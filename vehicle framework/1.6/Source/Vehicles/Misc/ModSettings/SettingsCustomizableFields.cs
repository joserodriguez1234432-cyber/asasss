// Decompiled with JetBrains decompiler
// Type: Vehicles.SettingsCustomizableFields
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public static class SettingsCustomizableFields
{
  static SettingsCustomizableFields()
  {
    if (!VehicleMod.ModifiableSettings)
      return;
    List<bool> source = new List<bool>();
    VehicleMod.PopulateCachedFields();
    foreach (VehicleDef def in DefDatabase<VehicleDef>.AllDefsListForReading)
    {
      bool flag1 = SettingsCustomizableFields.PopulateSaveableFields(def);
      bool flag2 = true;
      source.Add(flag1);
      source.Add(flag2);
      if (!flag1 || !flag2)
        VehicleMod.SettingsDisabledFor.Add(((Def) def).defName);
    }
    if (GenList.NullOrEmpty<bool>((IList<bool>) source) || !source.All<bool>((Func<bool, bool>) (b => !b)))
      return;
    Log.Error("SaveableFields have failed for every VehicleDef. Consider turning off the ModifiableSettings option in the ModSettings to bypass customizable field generation. This will require a restart.");
  }

  public static bool PopulateSaveableFields(VehicleDef def, bool hardReset = false)
  {
    try
    {
      if (hardReset)
        VehicleMod.settings.vehicles.fieldSettings.Remove(((Def) def).defName);
      Dictionary<SaveableField, SavedField<object>> fieldSetting;
      if (!VehicleMod.settings.vehicles.fieldSettings.TryGetValue(((Def) def).defName, out fieldSetting))
      {
        VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName] = new Dictionary<SaveableField, SavedField<object>>();
        fieldSetting = VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName];
      }
      VehicleMod.settings.vehicles.defaultValues[((Def) def).defName] = new Dictionary<SaveableField, object>();
      SettingsCustomizableFields.IterateTypeFields(def, ((object) def).GetType(), (object) def, ref fieldSetting);
      foreach (CompProperties comp in def.comps)
        SettingsCustomizableFields.IterateTypeFields(def, comp.GetType(), (object) comp, ref fieldSetting);
      VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName] = fieldSetting;
    }
    catch (Exception ex)
    {
      Log.Error($"Failed to populate field settings for <text>{((Def) def).defName}</text>.\nException=\"{ex}\"\nInnerException=\"{ex.InnerException}\"");
      return false;
    }
    return true;
  }

  public static bool PopulateSaveableUpgrades(VehicleDef def, bool hardReset = false)
  {
    try
    {
      if (hardReset)
        VehicleMod.settings.upgrades.upgradeSettings.Remove(((Def) def).defName);
      if (def.HasComp(typeof (CompUpgradeTree)))
      {
        if (!VehicleMod.settings.upgrades.upgradeSettings.TryGetValue(((Def) def).defName, out Dictionary<SaveableField, SavedField<object>> _))
        {
          VehicleMod.settings.upgrades.upgradeSettings.Add(((Def) def).defName, new Dictionary<SaveableField, SavedField<object>>());
          Dictionary<SaveableField, SavedField<object>> upgradeSetting = VehicleMod.settings.upgrades.upgradeSettings[((Def) def).defName];
          foreach (UpgradeNode node in def.GetSortedCompProperties<CompProperties_UpgradeTree>().def.nodes)
            SettingsCustomizableFields.IterateUpgradeNode(def, node, ref upgradeSetting);
          VehicleMod.settings.upgrades.upgradeSettings[((Def) def).defName] = upgradeSetting;
        }
      }
    }
    catch (Exception ex)
    {
      Log.Error($"Failed to populate upgrade settings for {((Def) def).defName}. Exception=\"{ex}\"\nInnerException=\"{ex.InnerException}\"");
      return false;
    }
    return true;
  }

  public static void IterateTypeFields(
    VehicleDef def,
    System.Type type,
    object obj,
    ref Dictionary<SaveableField, SavedField<object>> currentDict)
  {
    List<FieldInfo> fieldInfoList;
    if (!VehicleMod.CachedFields.TryGetValue(type, out fieldInfoList))
      return;
    Dictionary<SaveableField, SavedField<object>> fieldSetting = VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName];
    Dictionary<SaveableField, object> defaultValue = VehicleMod.settings.vehicles.defaultValues[((Def) def).defName];
    foreach (FieldInfo field in fieldInfoList)
    {
      PostToSettingsAttribute settingsAttribute;
      if (GenAttribute.TryGetAttribute<PostToSettingsAttribute>((MemberInfo) field, ref settingsAttribute) && settingsAttribute.ParentHolder)
      {
        object obj1 = field.GetValue(obj);
        if (field.FieldType.IsGenericType)
        {
          MethodInfo method = field.DeclaringType.GetMethod("ResolvePostToSettings", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
          if (method != (MethodInfo) null)
          {
            object[] parameters = new object[2]
            {
              (object) def,
              (object) currentDict
            };
            method.Invoke(obj, parameters);
            currentDict = (Dictionary<SaveableField, SavedField<object>>) parameters[1];
          }
          else
            SmashLog.Error($"Unable to generate customizable setting <field>{field.Name}</field> for <text>{((Def) def).defName}</text>. Fields of type <type>Dictionary<T></type> must implement ResolvePostToSettings method to be manually resolved.");
        }
        else
          SettingsCustomizableFields.IterateTypeFields(def, field.FieldType, obj1, ref currentDict);
      }
      else
      {
        SaveableField key = new SaveableField((Def) def, field);
        defaultValue[key] = field.GetValue(obj);
      }
    }
    VehicleMod.settings.vehicles.fieldSettings[((Def) def).defName] = fieldSetting;
  }

  public static void IterateUpgradeNode(
    VehicleDef def,
    UpgradeNode node,
    ref Dictionary<SaveableField, SavedField<object>> currentDict)
  {
    List<FieldInfo> fieldInfoList;
    if (!VehicleMod.CachedFields.TryGetValue(node.GetType(), out fieldInfoList))
      return;
    Dictionary<SaveableField, SavedField<object>> upgradeSetting = VehicleMod.settings.upgrades.upgradeSettings[((Def) def).defName];
    foreach (FieldInfo field in fieldInfoList)
    {
      PostToSettingsAttribute settingsAttribute;
      if (GenAttribute.TryGetAttribute<PostToSettingsAttribute>((MemberInfo) field, ref settingsAttribute) && settingsAttribute.ParentHolder)
      {
        object obj = field.GetValue((object) node);
        if (field.FieldType.IsGenericType)
        {
          MethodInfo method = field.DeclaringType.GetMethod("ResolvePostToSettings", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
          if (method != (MethodInfo) null)
          {
            object[] parameters = new object[2]
            {
              (object) def,
              (object) currentDict
            };
            method.Invoke((object) node, parameters);
            currentDict = (Dictionary<SaveableField, SavedField<object>>) parameters[1];
          }
          else
            Log.Error($"Unable to generate customizable setting {field.Name} for {((Def) def).defName}. Fields of type Dictionary<> must implement ResolvePostToSettings method to be manually resolved.");
        }
        else
          SettingsCustomizableFields.IterateTypeFields(def, field.FieldType, obj, ref currentDict);
      }
      else
      {
        SaveableField key = new SaveableField((Def) def, field);
        if (!upgradeSetting.TryGetValue(key, out SavedField<object> _))
          upgradeSetting.Add(key, new SavedField<object>(field.GetValue((object) node)));
      }
    }
    VehicleMod.settings.upgrades.upgradeSettings[((Def) def).defName] = upgradeSetting;
  }
}
