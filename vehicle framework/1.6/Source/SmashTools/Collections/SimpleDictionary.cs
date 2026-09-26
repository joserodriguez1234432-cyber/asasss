// Decompiled with JetBrains decompiler
// Type: SmashTools.SimpleDictionary`2
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Verse;

#nullable disable
namespace SmashTools;

public class SimpleDictionary<K, V> : Dictionary<K, V>
{
  private static MethodInfo validateNodeMethod = AccessTools.Method(typeof (DirectXmlToObject), "ValidateListNode", (Type[]) null, (Type[]) null);

  public SimpleDictionary()
  {
    if (typeof (K).IsSubclassOf(typeof (Def)) || ParseHelper.HandlesType(typeof (K)))
      return;
    SmashLog.Error($"Attempting to use <type>SimpleDictionary</type> with Key type = {typeof (K)} which is not assignable from Def and not handled by ParseHelper. This will not be parseable on startup.");
  }

  public void LoadDataFromXmlCustom(XmlNode xmlRoot)
  {
    try
    {
      if (xmlRoot["li"] != null)
        this.ParseNormalDictionary(xmlRoot);
      else if (GenTypes.IsDef(typeof (K)))
      {
        foreach (XmlNode listEntryNode in xmlRoot)
        {
          if (SimpleDictionary<K, V>.ValidateSimpleDictNode(listEntryNode, xmlRoot))
            DirectXmlCrossRefLoader.RegisterDictionaryWantsCrossRef<K, V>((Dictionary<K, V>) this, listEntryNode, (object) xmlRoot.Name);
        }
      }
      else
      {
        if (!ParseHelper.HandlesType(typeof (K)) || !ParseHelper.HandlesType(typeof (V)))
          return;
        foreach (XmlNode xmlNode in xmlRoot)
          this.Add(ParseHelper.FromString<K>(xmlNode.Name), ParseHelper.FromString<V>(xmlNode.InnerText));
      }
    }
    catch (Exception ex)
    {
      Log.Error($"Malformed dictionary XML. Node: {xmlRoot.OuterXml}.\n\nException: {ex}");
    }
  }

  private static bool ValidateSimpleDictNode(XmlNode listEntryNode, XmlNode listRootNode)
  {
    switch (listEntryNode)
    {
      case XmlComment _:
        return false;
      case XmlText _:
        Log.Error("XML format error: Raw text found inside a list element. Did you mean to surround it with list item <li> tags? " + listRootNode.OuterXml);
        return false;
      default:
        return true;
    }
  }

  private void ParseNormalDictionary(XmlNode xmlNode)
  {
    if (!GenTypes.IsDef(typeof (K)) && !GenTypes.IsDef(typeof (V)))
    {
      foreach (XmlNode xmlNode1 in xmlNode)
      {
        if ((bool) SimpleDictionary<K, V>.validateNodeMethod.Invoke((object) null, new object[3]
        {
          (object) xmlNode1,
          (object) xmlNode,
          (object) typeof (KeyValuePair<K, V>)
        }))
          this.Add(DirectXmlToObject.ObjectFromXml<K>((XmlNode) xmlNode1["key"], true), DirectXmlToObject.ObjectFromXml<V>((XmlNode) xmlNode1["value"], true));
      }
    }
    foreach (XmlNode xmlNode2 in xmlNode)
    {
      if ((bool) SimpleDictionary<K, V>.validateNodeMethod.Invoke((object) null, new object[3]
      {
        (object) xmlNode2,
        (object) xmlNode,
        (object) typeof (KeyValuePair<K, V>)
      }))
        DirectXmlCrossRefLoader.RegisterDictionaryWantsCrossRef<K, V>((Dictionary<K, V>) this, xmlNode2, (object) xmlNode.Name);
    }
  }
}
