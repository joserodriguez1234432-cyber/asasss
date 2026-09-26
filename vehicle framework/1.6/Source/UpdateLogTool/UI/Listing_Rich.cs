// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.Listing_Rich
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

#nullable disable
namespace UpdateLogTool;

public class Listing_Rich : Listing_Standard
{
  private const string LineBreakTag = "<br/>";
  private static readonly GUIContent tmpTextGUIContent = new GUIContent();
  private static readonly Color disabledColor = new Color(0.2f, 0.2f, 0.2f);

  public UpdateLog CurrentLog { get; set; }

  public Listing_Rich()
  {
  }

  public Listing_Rich(GameFont font)
    : base(font)
  {
  }

  public void DrawTexture(Texture2D texture, float width, float height)
  {
    ((Listing) this).NewColumnIfNeeded(height);
    Rect rect1 = ((Listing) this).GetRect(height, 1f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(rect1);
    ((Rect) ref rect2).x = (float) (((double) ((Rect) ref rect1).width - (double) width) / 2.0);
    ((Rect) ref rect2).height = height;
    ((Rect) ref rect2).width = width;
    GUI.DrawTexture(rect2, (Texture) texture);
  }

  public void DrawGif(Texture2D texture, float width, float height, IntVec2 size, int fps = 60)
  {
    ((Listing) this).NewColumnIfNeeded(height);
    Rect rect1 = ((Listing) this).GetRect(height, 1f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(rect1);
    ((Rect) ref rect2).x = (float) (((double) ((Rect) ref rect1).width - (double) width) / 2.0);
    ((Rect) ref rect2).height = height;
    ((Rect) ref rect2).width = width;
    Rect rect3 = rect2;
    int num1 = Mathf.FloorToInt(Time.time * (float) fps % (float) (size.x * size.z));
    float num2 = (float) num1 % (float) size.x / (float) size.x;
    float num3 = (float) (1.0 - (double) (Mathf.FloorToInt((float) num1 / (float) size.z) + 1) / (double) size.z);
    Rect rect4;
    // ISSUE: explicit constructor call
    ((Rect) ref rect4).\u002Ector(num2, num3, 1f / (float) size.x, 1f / (float) size.z);
    GUI.DrawTextureWithTexCoords(rect3, (Texture) texture, rect4);
  }

  public void DrawLoadingPlaceholder(float width, float height, string loadingMessage)
  {
    ((Listing) this).NewColumnIfNeeded(height);
    Rect rect1 = ((Listing) this).GetRect(height, 1f);
    Rect rect2;
    // ISSUE: explicit constructor call
    ((Rect) ref rect2).\u002Ector(rect1);
    ((Rect) ref rect2).x = (float) (((double) ((Rect) ref rect1).width - (double) width) / 2.0);
    ((Rect) ref rect2).height = height;
    ((Rect) ref rect2).width = width;
    Rect rect3 = rect2;
    Widgets.DrawBoxSolid(rect3, Listing_Rich.disabledColor);
    if (GenText.NullOrEmpty(loadingMessage))
      return;
    TextAnchor anchor = Verse.Text.Anchor;
    Verse.Text.Anchor = (TextAnchor) 4;
    Widgets.Label(rect3, loadingMessage);
    Verse.Text.Anchor = anchor;
  }

  public void RichText(DescriptionData segment)
  {
    string bracketText = segment.text;
    if (!GenList.NullOrEmpty<string>((IList<string>) segment.underlineText))
    {
      bracketText = Regex.Replace(segment.text, "(<u>.*?<\\/u>)|(<link>.*?<\\/link>)|(<b>.*?<\\/b>)|(<color>.*?<\\/color>)", "", RegexOptions.Singleline, TimeSpan.FromMilliseconds(5.0));
      this.DrawUnderlines(segment, bracketText);
    }
    if (!GenList.NullOrEmpty<string>((IList<string>) segment.hyperlinks))
    {
      bracketText = Regex.Replace(segment.text, "(<u>.*?<\\/u>)|(<b>.*?<\\/b>)|(<color>.*?<\\/color>)", "", RegexOptions.Singleline, TimeSpan.FromMilliseconds(5.0));
      bool[] flagArray = this.DrawHyperlinks(segment, bracketText);
      for (int index = 0; index < segment.hyperlinks.Length; ++index)
      {
        string hyperlink = segment.hyperlinks[index];
        string str1 = Regex.Match(hyperlink, "((?<=<link>).*?(?=<\\/link>))").Value;
        string str2 = Regex.Match(hyperlink, "(\\(.*?\\))", RegexOptions.Singleline, TimeSpan.FromMilliseconds(2.0)).Value.Trim('(').Trim(')');
        string str3 = "#99D9EA";
        if (flagArray[index])
          str3 = "#4DB3E6";
        string newValue;
        if (GenText.NullOrEmpty(str2) || string.IsNullOrWhiteSpace(str2))
          newValue = $"<color={str3}>{str1}</color>";
        else
          newValue = $"<color={str3}>{str2}</color>";
        bracketText = bracketText.Replace(hyperlink, newValue);
      }
    }
    this.Label(bracketText, -1f, new TipSignal?());
  }

  private void DrawUnderlines(DescriptionData segment, string bracketText)
  {
    string input = bracketText.Replace(Environment.NewLine, "<br/>");
    foreach (string str1 in segment.underlineText)
    {
      string str2 = str1.Replace("<u>", "").Replace("</u>", "");
      int length = bracketText.IndexOf(str2);
      try
      {
        Listing_Rich.tmpTextGUIContent.text = str2;
        Vector2 vector2 = Verse.Text.CurFontStyle.CalcSize(Listing_Rich.tmpTextGUIContent);
        Listing_Rich.tmpTextGUIContent.text = " ";
        float num1 = Verse.Text.CurFontStyle.CalcHeight(Listing_Rich.tmpTextGUIContent, 9999f);
        string str3 = bracketText.Substring(0, length);
        string[] array = Regex.Matches(input, "(<br/>)|(.*?(?=\\s|<br/>|$))", RegexOptions.Singleline, TimeSpan.FromSeconds(1.0)).Cast<Match>().Select<Match, string>((Func<Match, string>) (m => m.Value)).ToArray<string>();
        float num2 = 0.0f;
        float num3 = (float) ((double) Verse.Text.CalcHeight(str3, ((Listing) this).ColumnWidth) + (double) ((Listing) this).CurHeight - (double) num1 * 0.25);
        foreach (string str4 in array)
        {
          if (str4 == "<br/>")
            num2 = 0.0f;
          else if (!GenText.NullOrEmpty(str4))
          {
            if (!(str4 == str1))
            {
              float x = Verse.Text.CalcSize(str4 + " ").x;
              num2 += x;
              if ((double) num2 >= (double) ((Listing) this).ColumnWidth)
                num2 = x;
            }
            else
              break;
          }
        }
        Widgets.DrawLineHorizontal(num2, num3, vector2.x);
      }
      catch (ArgumentOutOfRangeException ex)
      {
        Log.Error($"Failed to find {str2} in {segment.text}");
      }
    }
  }

  private bool[] DrawHyperlinks(DescriptionData segment, string bracketText)
  {
    bool[] flagArray = new bool[segment.hyperlinks.Length];
    int index1 = 0;
    foreach (string hyperlink1 in segment.hyperlinks)
    {
      string str1 = Regex.Match(hyperlink1, "((?<=<link>).*?(?=<\\/link>))").Value;
      string str2 = Regex.Match(hyperlink1, "((?<=\\().*?(?=\\)))").Value;
      if (GenText.NullOrEmpty(str2) || string.IsNullOrWhiteSpace(str2))
        str2 = str1;
      for (int index2 = 0; index2 < index1; ++index2)
      {
        string hyperlink2 = segment.hyperlinks[index2];
        string newValue = Regex.Match(hyperlink2, "((?<=\\().*?(?=\\)))").Value;
        bracketText = bracketText.Replace(hyperlink2, newValue);
      }
      string input = bracketText.Replace(Environment.NewLine, "<br/>");
      int length = bracketText.IndexOf(hyperlink1);
      try
      {
        Listing_Rich.tmpTextGUIContent.text = str2;
        Vector2 vector2 = Verse.Text.CurFontStyle.CalcSize(Listing_Rich.tmpTextGUIContent);
        Listing_Rich.tmpTextGUIContent.text = " ";
        float num1 = Verse.Text.CurFontStyle.CalcHeight(Listing_Rich.tmpTextGUIContent, 9999f);
        string str3 = bracketText.Substring(0, length);
        string[] array = Regex.Matches(input, "(<br/>)|(.*?(?=<br/>|\\s|$))", RegexOptions.Singleline, TimeSpan.FromSeconds(1.0)).Cast<Match>().Select<Match, string>((Func<Match, string>) (m => m.Value)).ToArray<string>();
        float num2 = 0.0f;
        float num3 = (float) ((double) Verse.Text.CalcHeight(str3 + Verse.Text.CalcSize(str2 + " ").x.ToString(), ((Listing) this).ColumnWidth) + (double) ((Listing) this).CurHeight - (double) num1 * 0.25);
        foreach (string str4 in array)
        {
          if (str4.Contains("<link>"))
          {
            float x = Verse.Text.CalcSize(str2 + " ").x;
            if ((double) num2 + (double) x >= (double) ((Listing) this).ColumnWidth)
            {
              num2 = 0.0f;
              break;
            }
            break;
          }
          if (str4 == "<br/>")
            num2 = 0.0f;
          else if (!GenText.NullOrEmpty(str4))
          {
            float x = Verse.Text.CalcSize(str4 + " ").x;
            num2 += x;
            if ((double) num2 >= (double) ((Listing) this).ColumnWidth)
              num2 = x;
          }
        }
        Rect rect;
        // ISSUE: explicit constructor call
        ((Rect) ref rect).\u002Ector(num2, num3 - num1, vector2.x, num1);
        TooltipHandler.TipRegion(rect, TipSignal.op_Implicit(str1));
        if (Mouse.IsOver(rect))
        {
          flagArray[index1] = true;
          Widgets.DrawLine(new Vector2(num2, num3), new Vector2(num2 + vector2.x, num3), GenUI.MouseoverColor, 1f);
          if (Widgets.ButtonInvisible(rect, true))
            Application.OpenURL(str1);
        }
        else
          flagArray[index1] = false;
      }
      catch (ArgumentOutOfRangeException ex)
      {
        Log.Error($"Failed to find {hyperlink1} in {bracketText}");
      }
      ++index1;
    }
    return flagArray;
  }

  public void HyperlinkNewline(string name, string url)
  {
    Color color = GUI.color;
    Vector2 vector2 = Verse.Text.CalcSize(name);
    Rect rect = ((Listing) this).GetRect(vector2.y, 1f);
    ((Rect) ref rect).width = vector2.x;
    if (Mouse.IsOver(rect))
    {
      GUI.color = GenUI.MouseoverColor;
      Widgets.DrawLineHorizontal(((Rect) ref rect).x, ((Rect) ref rect).y + ((Rect) ref rect).height * 0.75f, vector2.x);
    }
    else
      GUI.color = new Color(0.6f, 0.85f, 1f);
    Widgets.Label(rect, name);
    if (Widgets.ButtonInvisible(rect, true))
      Application.OpenURL(url);
    GUI.color = color;
  }
}
