// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.EnhancedText
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;

#nullable disable
namespace UpdateLogTool;

public static class EnhancedText
{
  public const string hyperlinkTag = "<link>";
  public const string hyperLinkEndTag = "</link>";
  public const string underlineTag = "<u>";
  public const string underlineEndTag = "</u>";

  private static UpdateLogTool.TaggedSegment TaggedSegment(string segment)
  {
    return GenCollection.FirstOrDefault<UpdateLogTool.TaggedSegment>(SegmentParser.tags, (Predicate<UpdateLogTool.TaggedSegment>) (t => segment.Contains(t.Tags.open)));
  }

  public static IEnumerable<DescriptionData> ParseDescriptionData(UpdateLog log)
  {
    return EnhancedText.ParseDescriptionData(log.UpdateData.EnhancedDescription);
  }

  public static IEnumerable<DescriptionData> ParseDescriptionData(string description)
  {
    foreach (string str in Regex.Split(description, SegmentParser.regexTags, RegexOptions.Singleline, TimeSpan.FromSeconds(3.0)))
    {
      if (!GenText.NullOrEmpty(str) && !(str == Environment.NewLine))
      {
        UpdateLogTool.TaggedSegment taggedSegment = EnhancedText.TaggedSegment(str);
        if (taggedSegment != null)
        {
          string text = str.Replace(taggedSegment.Tags.open, string.Empty);
          if (!GenText.NullOrEmpty(taggedSegment.Tags.close))
            text = text.Replace(taggedSegment.Tags.close, string.Empty);
          yield return new DescriptionData(text)
          {
            tag = taggedSegment
          };
        }
        else
        {
          string[] array1 = Regex.Matches(str, "(<u>.*?<\\/u>)", RegexOptions.Singleline, TimeSpan.FromSeconds(1.0)).Cast<Match>().Select<Match, string>((Func<Match, string>) (m => m.Value)).Where<string>((Func<string, bool>) (s => !string.IsNullOrWhiteSpace(s) && !GenText.NullOrEmpty(s) && s != Environment.NewLine)).ToArray<string>();
          string[] array2 = Regex.Matches(str, "(<link>.*?<\\/link>.+?(?=\\))\\))", RegexOptions.Singleline, TimeSpan.FromSeconds(1.0)).Cast<Match>().Select<Match, string>((Func<Match, string>) (m => m.Value)).Where<string>((Func<string, bool>) (s => !string.IsNullOrWhiteSpace(s) && !GenText.NullOrEmpty(s) && s != Environment.NewLine)).ToArray<string>();
          yield return new DescriptionData(str)
          {
            underlineText = array1,
            hyperlinks = array2
          };
        }
      }
    }
  }
}
