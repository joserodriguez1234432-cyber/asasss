// Decompiled with JetBrains decompiler
// Type: Vehicles.SettingsCache
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles;

public static class SettingsCache
{
  private static readonly Dictionary<Pair<System.Type, string>, FieldInfo> cachedFieldInfos = new Dictionary<Pair<System.Type, string>, FieldInfo>();

  public static FieldInfo GetCachedField(this System.Type type, string name)
  {
    Pair<System.Type, string> key = new Pair<System.Type, string>(type, name);
    FieldInfo cachedField;
    if (!SettingsCache.cachedFieldInfos.TryGetValue(key, out cachedField))
    {
      cachedField = AccessTools.Field(type, name);
      SettingsCache.cachedFieldInfos.Add(key, cachedField);
    }
    return cachedField;
  }

  public static bool TryGetValue<T>(VehicleDef def, FieldInfo field, out T value)
  {
    value = default (T);
    Dictionary<SaveableField, SavedField<object>> dictionary;
    if (VehicleMod.settings.vehicles.fieldSettings.TryGetValue(((Def) def).defName, out dictionary))
    {
      SaveableField key = new SaveableField((Def) def, field);
      SavedField<object> savedField;
      if (dictionary.TryGetValue(key, out savedField))
      {
        value = (T) savedField.EndValue;
        return true;
      }
    }
    return false;
  }

  public static T TryGetValue<T>(
    VehicleDef def,
    System.Type containingType,
    string fieldName,
    T fallback = null)
  {
    if (!VehicleMod.ModifiableSettings)
      return fallback;
    FieldInfo cachedField = containingType.GetCachedField(fieldName);
    if ((object) cachedField == null)
    {
      Trace.Fail(fieldName + " could not be found in CachedFields. Defaulting to defined fallback value.");
      return fallback;
    }
    System.Type type = (System.Type) null;
    try
    {
      object obj;
      if (SettingsCache.TryGetValue<object>(def, cachedField, out obj))
      {
        type = obj.GetType();
        if (!(type != typeof (T)))
          return (T) obj;
        return typeof (T).IsEnum ? (T) obj : (T) Convert.ChangeType(obj, typeof (T), (IFormatProvider) CultureInfo.InstalledUICulture.NumberFormat);
      }
    }
    catch (InvalidCastException ex)
    {
      Log.Error($"Cannot cast {fieldName} from {type?.ToString() ?? "[Null]"} to {typeof (T)}.\nException=\"{ex}\"");
    }
    return fallback;
  }

  public static float TryGetValue(VehicleDef def, VehicleStatDef statDef, float fallback)
  {
    Dictionary<string, float> dictionary;
    float num;
    return VehicleMod.settings.vehicles.vehicleStats.TryGetValue(((Def) def).defName, out dictionary) && dictionary.TryGetValue(statDef.defName, out num) ? num : fallback;
  }
}
