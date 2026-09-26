// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.KeyPrefsLoader
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Performance;

internal static class KeyPrefsLoader
{
  private static string openParseNode;

  public static void Init()
  {
    KeyPrefsData data;
    KeyPrefsLoader.Load(out data);
    AccessTools.Field(typeof (KeyPrefs), "data").SetValue((object) null, (object) data);
  }

  private static void Load(out KeyPrefsData data)
  {
    data = new KeyPrefsData();
    string keyPrefsFilePath = GenFilePaths.KeyPrefsFilePath;
    bool flag = false;
    if (!File.Exists(keyPrefsFilePath))
    {
      data.ResetToDefaults();
      flag = true;
    }
    else
    {
      XmlReaderSettings settings = new XmlReaderSettings()
      {
        IgnoreWhitespace = true,
        IgnoreComments = true
      };
      using (XmlReader reader = XmlReader.Create(keyPrefsFilePath, settings))
      {
label_14:
        while (reader.Read())
        {
          if (reader.NodeType == XmlNodeType.Element && !(reader.Name != "li"))
          {
            string name = reader.Name;
            KeyBindingDef key = (KeyBindingDef) null;
            KeyBindingData keyBindingData = (KeyBindingData) null;
            while (reader.NodeType != XmlNodeType.EndElement || !(reader.Name == name))
            {
              switch (reader.Name)
              {
                case "key":
                  KeyPrefsLoader.Parse<KeyBindingDef>(ref reader, ref key, new KeyPrefsLoader.Processor<KeyBindingDef>(ParseDef));
                  break;
                case "value":
                  KeyPrefsLoader.Parse<KeyBindingData>(ref reader, ref keyBindingData, new KeyPrefsLoader.Processor<KeyBindingData>(ParseKeyBindingData));
                  break;
              }
              if (!reader.Read())
              {
                if (key != null && keyBindingData != null)
                {
                  data.keyPrefs[key] = keyBindingData;
                  goto label_14;
                }
                goto label_14;
              }
            }
            KeyPrefsLoader.openParseNode = (string) null;
            return;
          }
        }
      }
    }
    data.AddMissingDefaultBindings();
    data.ErrorCheck();
    if (flag)
      return;
    KeyPrefs.Save();

    static void ParseDef(ref KeyBindingDef keyBindingDef, string name, string value)
    {
      keyBindingDef = DefDatabase<KeyBindingDef>.GetNamed(value, false);
    }

    static void ParseKeyBindingData(ref KeyBindingData keyBindingData, string name, string value)
    {
      if (keyBindingData == null)
        keyBindingData = new KeyBindingData();
      KeyCode keyCode = Enum.Parse<KeyCode>(value);
      switch (name)
      {
        case "keyBindingA":
          keyBindingData.keyBindingA = keyCode;
          break;
        case "keyBindingB":
          keyBindingData.keyBindingB = keyCode;
          break;
      }
    }
  }

  private static void Parse<T>(
    [RequiresLocation, In] ref XmlReader reader,
    ref T obj,
    KeyPrefsLoader.Processor<T> processor)
    where T : class
  {
    KeyPrefsLoader.openParseNode = reader.Name;
    while (reader.NodeType != XmlNodeType.EndElement || !(reader.Name == KeyPrefsLoader.openParseNode))
    {
      if (reader.NodeType == XmlNodeType.Text)
        processor(ref obj, KeyPrefsLoader.openParseNode, reader.Value);
      if (!reader.Read())
        return;
    }
    KeyPrefsLoader.openParseNode = (string) null;
  }

  private delegate void Processor<T>(ref T item, string name, string value);
}
