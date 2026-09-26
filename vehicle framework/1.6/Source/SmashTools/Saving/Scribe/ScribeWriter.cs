// Decompiled with JetBrains decompiler
// Type: SmashTools.ScribeWriter
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Xml;
using Verse;

#nullable disable
namespace SmashTools;

public static class ScribeWriter
{
  public static void WriteElementWithAttribute(
    this ScribeSaver saver,
    string value,
    string attributeName,
    string attrValue)
  {
    XmlWriter xmlWriter = (XmlWriter) AccessTools.Field(typeof (ScribeSaver), "writer").GetValue((object) saver);
    if (xmlWriter == null)
    {
      Log.Error("Called WriteElementWithAttribute(), but writer is null.");
    }
    else
    {
      try
      {
        xmlWriter.WriteAttributeString(attributeName, attrValue);
        xmlWriter.WriteString(value);
      }
      catch (Exception ex)
      {
        AccessTools.Field(typeof (ScribeSaver), "anyInternalException").SetValue((object) saver, (object) true);
        throw;
      }
    }
  }

  public static void WriteElementWithAttributes(
    this ScribeSaver saver,
    string value,
    List<Pair<string, string>> attributeParams)
  {
    XmlWriter xmlWriter = (XmlWriter) AccessTools.Field(typeof (ScribeSaver), "writer").GetValue((object) saver);
    if (xmlWriter == null)
    {
      Log.Error("Called WriteElementWithAttributes(), but writer is null.");
    }
    else
    {
      try
      {
        foreach (Pair<string, string> attributeParam in attributeParams)
          xmlWriter.WriteAttributeString(attributeParam.First, attributeParam.Second);
        xmlWriter.WriteString(value);
      }
      catch (Exception ex)
      {
        AccessTools.Field(typeof (ScribeSaver), "anyInternalException").SetValue((object) saver, (object) true);
        throw;
      }
    }
  }
}
