// Decompiled with JetBrains decompiler
// Type: Vehicles.Ext_Settings
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles;

internal static class Ext_Settings
{
  public static IEnumerable<FieldInfo> GetPostSettingsFields(this System.Type type)
  {
    FieldInfo[] fieldInfoArray = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    for (int index = 0; index < fieldInfoArray.Length; ++index)
    {
      FieldInfo element = fieldInfoArray[index];
      if (Attribute.IsDefined((MemberInfo) element, typeof (PostToSettingsAttribute)))
        yield return element;
    }
    fieldInfoArray = (FieldInfo[]) null;
  }

  public static int CombineVersionString(string version)
  {
    int result;
    if (int.TryParse(new string(version.Where<char>((Func<char, bool>) (c => char.IsDigit(c))).ToArray<char>()), out result))
      return result;
    Log.Error($"Unable to parse {version} as raw value following Major.Minor.Patch sequence.");
    return 0;
  }
}
