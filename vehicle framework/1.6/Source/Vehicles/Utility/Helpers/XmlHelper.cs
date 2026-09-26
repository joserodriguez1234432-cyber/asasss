// Decompiled with JetBrains decompiler
// Type: Vehicles.XmlHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public static class XmlHelper
{
  public static void FillDefaults_Def<K, V>(
    string defName,
    string fieldName,
    Dictionary<K, V> dictionary)
    where K : Def
  {
    Dictionary<string, string> dictionary1;
    string str;
    if (!ParsingHelper.SetDefaultValues.TryGetValue(defName, out dictionary1) || !dictionary1.TryGetValue(fieldName, out str))
      return;
    V v = (V) Convert.ChangeType((object) str, typeof (V));
    foreach (K k in DefDatabase<K>.AllDefsListForReading)
      dictionary.TryAdd(k, v);
  }

  public static void FillDefaults_Enum<K, V>(
    string defName,
    string fieldName,
    Dictionary<K, V> dictionary)
    where K : Enum
  {
    Dictionary<string, string> dictionary1;
    string str;
    if (!ParsingHelper.SetDefaultValues.TryGetValue(defName, out dictionary1) || !dictionary1.TryGetValue(fieldName, out str))
      return;
    V v = (V) Convert.ChangeType((object) str, typeof (V));
    foreach (K k in Enum.GetValues(typeof (K)))
      dictionary.TryAdd(k, v);
  }
}
