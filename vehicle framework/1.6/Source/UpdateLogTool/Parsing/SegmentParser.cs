// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.SegmentParser
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

#nullable disable
namespace UpdateLogTool;

public static class SegmentParser
{
  public static readonly List<TaggedSegment> tags = new List<TaggedSegment>();
  public static string regexTags;

  public static void ParseAndCreateSegments()
  {
    foreach (Type type in GenTypes.AllTypes.Where<Type>((Func<Type, bool>) (t => t.IsSubclassOf(typeof (TaggedSegment)) && !t.IsAbstract)))
    {
      TaggedSegment instance = (TaggedSegment) Activator.CreateInstance(type);
      SegmentParser.tags.Add(instance);
    }
  }

  public static void GenerateRegexText()
  {
    SegmentParser.regexTags = string.Empty;
    int index1 = 0;
    while (index1 < SegmentParser.tags.Count)
    {
      TaggedSegment tag = SegmentParser.tags[index1];
      if (!GenText.NullOrEmpty(tag.Tags.close))
      {
        SegmentParser.regexTags += "(";
        SegmentParser.regexTags += tag.Tags.open;
        SegmentParser.regexTags += ".*?";
        SegmentParser.regexTags += tag.Tags.close.Replace("/", "\\/");
        SegmentParser.regexTags += ")";
      }
      else
      {
        SegmentParser.regexTags += "(";
        SegmentParser.regexTags += tag.Tags.open.Replace("/", "\\/");
        SegmentParser.regexTags += ")";
      }
      ++index1;
      SegmentParser.regexTags += "|";
    }
    SegmentParser.regexTags += "(.+?(?=";
    int index2 = 0;
    while (index2 < SegmentParser.tags.Count)
    {
      TaggedSegment tag = SegmentParser.tags[index2];
      SegmentParser.regexTags = !GenText.NullOrEmpty(tag.Tags.close) ? SegmentParser.regexTags + tag.Tags.open.Replace("/", "\\/") : SegmentParser.regexTags + tag.Tags.open;
      ++index2;
      SegmentParser.regexTags += "|";
    }
    SegmentParser.regexTags += "$))";
  }

  public static string ColoredRegexForOutput(string regex)
  {
    string coloredRegex = regex;
    int indexOffset = 0;
    string str1 = SegmentParser.ColorFor(SegmentParser.RColor.Teal);
    coloredRegex = coloredRegex.Insert(0, str1);
    indexOffset += str1.Length;
    Color color = SegmentParser.RColor.Teal;
    try
    {
      for (int index = 0; index < regex.Length; ++index)
      {
        if (regex[index] == '*' || regex[index] == '?' || regex[index] == '+')
        {
          if (Color.op_Inequality(color, SegmentParser.RColor.HotPink))
          {
            CloseBrackets(index);
            string str2 = SegmentParser.ColorFor(SegmentParser.RColor.HotPink);
            coloredRegex = coloredRegex.Insert(index + indexOffset, str2);
            indexOffset += str2.Length;
            color = SegmentParser.RColor.HotPink;
          }
        }
        else if ((regex[index] == '.' || regex[index] == '(' || regex[index] == ')') && (index == 0 || regex[index - 1] != '\\'))
        {
          if (Color.op_Inequality(color, SegmentParser.RColor.Teal))
          {
            CloseBrackets(index);
            string str3 = SegmentParser.ColorFor(SegmentParser.RColor.Teal);
            coloredRegex = coloredRegex.Insert(index + indexOffset, str3);
            indexOffset += str3.Length;
            color = SegmentParser.RColor.Teal;
          }
        }
        else if (Color.op_Inequality(color, SegmentParser.RColor.LightOrange))
        {
          CloseBrackets(index);
          string str4 = SegmentParser.ColorFor(SegmentParser.RColor.LightOrange);
          coloredRegex = coloredRegex.Insert(index + indexOffset, str4);
          indexOffset += str4.Length;
          color = SegmentParser.RColor.LightOrange;
        }
      }
    }
    catch (ArgumentOutOfRangeException ex)
    {
      Log.Error($"Unable to colorify regex. Offset: {indexOffset} Length: {coloredRegex.Length} Regex=\"{regex}\"");
    }
    return coloredRegex;

    void CloseBrackets(int index)
    {
      coloredRegex = coloredRegex.Insert(index + indexOffset, "</color>");
      indexOffset += "</color>".Length;
    }
  }

  public static string ColorFor(Color color) => $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";

  [StructLayout(LayoutKind.Sequential, Size = 1)]
  public struct RColor
  {
    public static Color Teal => new Color(0.177f, 0.71f, 0.451f);

    public static Color LightOrange => new Color(0.9f, 0.59f, 0.275f);

    public static Color HotPink => new Color(0.843f, 0.275f, 0.785f);
  }
}
