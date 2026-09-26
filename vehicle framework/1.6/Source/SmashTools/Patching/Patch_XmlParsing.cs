// Decompiled with JetBrains decompiler
// Type: SmashTools.Patching.Patch_XmlParsing
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using SmashTools.Xml;
using System;
using System.Reflection;
using System.Xml;
using Verse;

#nullable disable
namespace SmashTools.Patching;

internal class Patch_XmlParsing : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Mod;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (DirectXmlLoader), "DefFromNode", (Type[]) null, (Type[]) null), new HarmonyMethod(typeof (Patch_XmlParsing), "PreProcessAttributesOnDef", (Type[]) null), new HarmonyMethod(typeof (Patch_XmlParsing), "ReadCustomAttributesOnDef", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (DirectXmlToObjectNew), "DefFromNodeNew", (Type[]) null, (Type[]) null), new HarmonyMethod(typeof (Patch_XmlParsing), "PreProcessAttributesOnDef", (Type[]) null), new HarmonyMethod(typeof (Patch_XmlParsing), "ReadCustomAttributesOnDef", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (XmlToObjectUtils), "DoFieldSearch", (Type[]) null, (Type[]) null), new HarmonyMethod(typeof (Patch_XmlParsing), "PreProcessAttributes", (Type[]) null), new HarmonyMethod(typeof (Patch_XmlParsing), "ReadCustomAttributes", (Type[]) null));
  }

  private static bool PreProcessAttributesOnDef(out Def __result, XmlNode node)
  {
    __result = (Def) null;
    return Patch_XmlParsing.PreProcessXmlNode(node, (FieldInfo) null);
  }

  private static void ReadCustomAttributesOnDef(XmlNode node)
  {
    Patch_XmlParsing.ProcessXmlNode(node, (FieldInfo) null);
  }

  private static bool PreProcessAttributes(XmlNode fieldNode, out FieldInfo __result)
  {
    __result = (FieldInfo) null;
    return Patch_XmlParsing.PreProcessXmlNode(fieldNode, __result);
  }

  private static void ReadCustomAttributes(XmlNode fieldNode, FieldInfo __result)
  {
    Patch_XmlParsing.ProcessXmlNode(fieldNode, __result);
  }

  private static bool PreProcessXmlNode(XmlNode node, FieldInfo fieldInfo)
  {
    if ((node != null ? (node.NodeType != XmlNodeType.Element ? 1 : 0) : 1) != 0)
      return true;
    XmlAttributeCollection attributes = node.Attributes;
    if (attributes == null)
      return true;
    try
    {
      foreach (XmlAttribute attr in (XmlNamedNodeMap) attributes)
      {
        XmlParseHelper.CustomAttribute customAttribute;
        if (XmlParseHelper.RegisteredAttributes.TryGetValue(attr.Name, out customAttribute) && !customAttribute.PreProcess(node, attr, fieldInfo))
          return false;
      }
    }
    catch (Exception ex)
    {
      Log.Error($"Exception thrown while trying to apply registered preprocessors to Def of type {node.Name}.\n{ex}");
    }
    return true;
  }

  private static void ProcessXmlNode(XmlNode node, FieldInfo fieldInfo)
  {
    if ((node != null ? (node.NodeType != XmlNodeType.Element ? 1 : 0) : 1) != 0)
      return;
    XmlAttributeCollection attributes = node.Attributes;
    if (attributes == null)
      return;
    try
    {
      foreach (XmlAttribute attr in (XmlNamedNodeMap) attributes)
      {
        XmlParseHelper.CustomAttribute customAttribute;
        if (XmlParseHelper.RegisteredAttributes.TryGetValue(attr.Name, out customAttribute))
          customAttribute.Process(node, attr, fieldInfo);
      }
    }
    catch (Exception ex)
    {
      Log.Error($"Exception thrown while trying to apply registered attributes to Def of type {node.Name}.\n{ex}");
    }
  }
}
