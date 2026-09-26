// Decompiled with JetBrains decompiler
// Type: SmashTools.ObjectValueExtractor
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using System;
using System.Xml;
using Verse;

#nullable disable
namespace SmashTools;

public static class ObjectValueExtractor
{
  public static object ValueFromNode(XmlNode subNode)
  {
    if (subNode == null)
      return (object) null;
    XmlAttribute attribute1 = subNode.Attributes["Type"];
    XmlAttribute attribute2 = subNode.Attributes["SavedField"];
    if (attribute1 == null)
    {
      Log.Error($"Failed to retrieve Type attribute from ObjectValue saved XmlNode. Cannot parse into game. Node: {subNode}");
      return (object) null;
    }
    Type type = AccessTools.TypeByName(attribute1.Value);
    XmlAttribute attribute3 = subNode.Attributes["IsNull"];
    if (attribute3 != null && attribute3.Value.ToLower() == "true")
      return type.GetDefaultValue();
    try
    {
      try
      {
        if (attribute2 != null)
          return SavedField<object>.FromTypedString(subNode.InnerText, type);
        return AccessTools.Method(typeof (ParseHelper), "FromString", new Type[2]
        {
          typeof (string),
          typeof (Type)
        }, (Type[]) null).Invoke((object) null, new object[2]
        {
          (object) subNode.InnerText,
          (object) type
        });
      }
      catch (Exception ex)
      {
        Log.Error($"Exception parsing node {subNode.OuterXml} into a {(object) type}:\n{ex.ToString()}");
      }
      return type.GetDefaultValue();
    }
    catch (Exception ex)
    {
      Log.Error("Exception loading XML: " + ex?.ToString());
    }
    return type.GetDefaultValue();
  }
}
