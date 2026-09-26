// Decompiled with JetBrains decompiler
// Type: Vehicles.ParsingHelper
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using SmashTools.Xml;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Vehicles.Config;
using Verse;

#nullable disable
namespace Vehicles;

[StaticConstructorOnModInit]
public static class ParsingHelper
{
  internal static readonly Dictionary<string, HashSet<FieldInfo>> LockedFields = new Dictionary<string, HashSet<FieldInfo>>();
  internal static readonly Dictionary<string, Dictionary<string, string>> SetDefaultValues = new Dictionary<string, Dictionary<string, string>>();

  static ParsingHelper()
  {
    ParsingHelper.RegisterParsers();
    ParsingHelper.RegisterAttributes();
  }

  private static void RegisterParsers()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    ParseHelper.Parsers<VehicleJobLimitations>.Register(ParsingHelper.\u003C\u003EO.\u003C0\u003E__FromString ?? (ParsingHelper.\u003C\u003EO.\u003C0\u003E__FromString = new Func<string, VehicleJobLimitations>(VehicleJobLimitations.FromString)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    ParseHelper.Parsers<CompVehicleLauncher.DeploymentTimer>.Register(ParsingHelper.\u003C\u003EO.\u003C1\u003E__FromString ?? (ParsingHelper.\u003C\u003EO.\u003C1\u003E__FromString = new Func<string, CompVehicleLauncher.DeploymentTimer>(CompVehicleLauncher.DeploymentTimer.FromString)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    ParseHelper.Parsers<Pair<VehicleEventDef, VehicleEventDef>>.Register(ParsingHelper.\u003C\u003EO.\u003C2\u003E__VehicleEventDefPairFromString ?? (ParsingHelper.\u003C\u003EO.\u003C2\u003E__VehicleEventDefPairFromString = new Func<string, Pair<VehicleEventDef, VehicleEventDef>>(ParsingHelper.VehicleEventDefPairFromString)));
  }

  private static Pair<VehicleEventDef, VehicleEventDef> VehicleEventDefPairFromString(string entry)
  {
    entry = entry.TrimStart('(').TrimEnd(')');
    string[] strArray = entry.Split(',');
    try
    {
      return new Pair<VehicleEventDef, VehicleEventDef>(DefDatabase<VehicleEventDef>.GetNamed(strArray[0].Trim(), true), DefDatabase<VehicleEventDef>.GetNamed(strArray[1].Trim(), true));
    }
    catch (Exception ex)
    {
      Log.Error($"{entry} is not a valid Pair<VehicleEventDef, VehicleEventDef> format. Exception: {ex}");
      return new Pair<VehicleEventDef, VehicleEventDef>();
    }
  }

  private static void RegisterAttributes()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    XmlParseHelper.RegisterPreProcessor("FeatureFlag", ParsingHelper.\u003C\u003EO.\u003C3\u003E__CheckFeatureFlag ?? (ParsingHelper.\u003C\u003EO.\u003C3\u003E__CheckFeatureFlag = new XmlParseHelper.AttributePreProcessor(ParsingHelper.CheckFeatureFlag)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    XmlParseHelper.RegisterAttribute("LockSetting", ParsingHelper.\u003C\u003EO.\u003C4\u003E__CheckFieldLocked ?? (ParsingHelper.\u003C\u003EO.\u003C4\u003E__CheckFieldLocked = new XmlParseHelper.AttributeProcessor(ParsingHelper.CheckFieldLocked)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    XmlParseHelper.RegisterAttribute("AssignDefaults", ParsingHelper.\u003C\u003EO.\u003C5\u003E__AssignDefaults ?? (ParsingHelper.\u003C\u003EO.\u003C5\u003E__AssignDefaults = new XmlParseHelper.AttributeProcessor(ParsingHelper.AssignDefaults)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    XmlParseHelper.RegisterAttribute("DisableSettings", ParsingHelper.\u003C\u003EO.\u003C6\u003E__CheckDisabledSettings ?? (ParsingHelper.\u003C\u003EO.\u003C6\u003E__CheckDisabledSettings = new XmlParseHelper.AttributeProcessor(ParsingHelper.CheckDisabledSettings)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    XmlParseHelper.RegisterAttribute("AllowTerrainWithTag", ParsingHelper.\u003C\u003EO.\u003C7\u003E__AllowTerrainCosts ?? (ParsingHelper.\u003C\u003EO.\u003C7\u003E__AllowTerrainCosts = new XmlParseHelper.AttributeProcessor(ParsingHelper.AllowTerrainCosts)), "customTerrainCosts");
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    XmlParseHelper.RegisterAttribute("DisallowTerrainWithTag", ParsingHelper.\u003C\u003EO.\u003C8\u003E__DisallowTerrainCosts ?? (ParsingHelper.\u003C\u003EO.\u003C8\u003E__DisallowTerrainCosts = new XmlParseHelper.AttributeProcessor(ParsingHelper.DisallowTerrainCosts)), "customTerrainCosts");
  }

  private static bool CheckFeatureFlag(XmlNode node, string value, FieldInfo field)
  {
    return GenText.NullOrEmpty(value) || FeatureFlags.IsFeatureEnabled(value);
  }

  private static void CheckFieldLocked(XmlNode node, string value, FieldInfo field)
  {
    if (!(value.ToUpperInvariant() == "TRUE"))
      return;
    string key = ParsingHelper.BackSearchDefName(node);
    if (string.IsNullOrEmpty(key))
    {
      Log.Error($"Cannot use LockSetting on {field.Name} since it is not nested within a Def.");
    }
    else
    {
      if (!GenAttribute.HasAttribute<PostToSettingsAttribute>((MemberInfo) field))
        Log.Error($"Cannot use LockSetting on {field.Name} since related field does not have PostToSettings attribute in {field.DeclaringType}");
      if (!ParsingHelper.LockedFields.ContainsKey(key))
        ParsingHelper.LockedFields.Add(key, new HashSet<FieldInfo>());
      ParsingHelper.LockedFields[key].Add(field);
    }
  }

  private static void AssignDefaults(XmlNode node, string value, FieldInfo field)
  {
    string key = ParsingHelper.BackSearchDefName(node);
    if (string.IsNullOrEmpty(key))
    {
      Log.Error($"Cannot use AssignAllDefault on {field.Name}. This attribute cannot be used in abstract defs.");
    }
    else
    {
      if (!ParsingHelper.SetDefaultValues.ContainsKey(key))
        ParsingHelper.SetDefaultValues.Add(key, new Dictionary<string, string>());
      ParsingHelper.SetDefaultValues[key][node.Name] = value;
    }
  }

  private static void CheckDisabledSettings(XmlNode node, string value, FieldInfo field)
  {
    if (!(value.ToUpperInvariant() == "TRUE"))
      return;
    XmlNode xmlNode = node.SelectSingleNode("defName");
    if (xmlNode == null)
    {
      Log.Error("Cannot use DisableSetting on non-VehicleDef XmlNodes.");
    }
    else
    {
      string innerText = xmlNode.InnerText;
      VehicleMod.SettingsDisabledFor.Add(innerText);
    }
  }

  private static void AllowTerrainCosts(XmlNode node, string value, FieldInfo field)
  {
    string key = ParsingHelper.BackSearchDefName(node);
    if (string.IsNullOrEmpty(key))
    {
      Log.Error($"Could not find defName node for {node.Name}.");
    }
    else
    {
      int result = 1;
      XmlAttribute attribute = node.Attributes?["PathCost"];
      if (attribute != null && !int.TryParse(attribute.Value, out result))
      {
        Log.Warning("Unable to parse PathCost attribute for " + key);
        result = 1;
      }
      Dictionary<string, int> dictionary;
      if (!PathingHelper.allTerrainCostsByTag.TryGetValue(key, out dictionary))
      {
        dictionary = new Dictionary<string, int>();
        PathingHelper.allTerrainCostsByTag[key] = dictionary;
      }
      dictionary[value] = result;
    }
  }

  private static void DisallowTerrainCosts(XmlNode node, string value, FieldInfo field)
  {
    string key = ParsingHelper.BackSearchDefName(node);
    if (string.IsNullOrEmpty(key))
    {
      Log.Error($"Could not find defName node for {node.Name}.");
    }
    else
    {
      Dictionary<string, int> dictionary;
      if (!PathingHelper.allTerrainCostsByTag.TryGetValue(key, out dictionary))
      {
        dictionary = new Dictionary<string, int>();
        PathingHelper.allTerrainCostsByTag[key] = dictionary;
      }
      dictionary[value] = 10000;
    }
  }

  private static string BackSearchDefName(XmlNode curNode)
  {
    XmlNode xmlNode1 = curNode.SelectSingleNode("defName");
    XmlNode xmlNode2 = curNode;
    for (; xmlNode1 == null; xmlNode1 = xmlNode2.SelectSingleNode("defName"))
    {
      xmlNode2 = xmlNode2.ParentNode;
      if (xmlNode2 == null)
        return string.Empty;
    }
    return xmlNode1.InnerText;
  }
}
