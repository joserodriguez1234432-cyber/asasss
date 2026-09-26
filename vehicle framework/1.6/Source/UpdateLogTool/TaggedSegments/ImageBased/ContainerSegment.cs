// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.ContainerSegment
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace UpdateLogTool;

public abstract class ContainerSegment : TaggedSegment
{
  protected virtual IEnumerable<(string name, Type type)> Attributes
  {
    get
    {
      yield return ("WIDTH", typeof (int));
      yield return ("HEIGHT", typeof (int));
    }
  }

  protected virtual string GetInnerText(string fullText)
  {
    string[] strArray = fullText.Split('>', StringSplitOptions.None);
    return strArray.Length > 1 ? ((IEnumerable<string>) strArray[1].Split('<', StringSplitOptions.None)).FirstOrDefault<string>() : fullText;
  }

  public override int HeightOccupied(UpdateLog log, string fullText)
  {
    string[] source = fullText.Split('>', StringSplitOptions.None);
    if (source.Length < 1)
    {
      Log.ErrorOnce($"Incorrect split for bracketText in {fullText}.", fullText.GetHashCode());
      return Mathf.FloorToInt(200f);
    }
    string fullText1 = ((IEnumerable<string>) source).Last<string>();
    int fallback = Mathf.FloorToInt(700f);
    int num = fallback;
    if (source.Length > 1 && !GenText.NullOrEmpty(((IEnumerable<string>) source).FirstOrDefault<string>()))
    {
      string bracketText = source[0];
      fullText1 = ((IEnumerable<string>) source).Last<string>();
      Lookup lookup = this.ContainerAttributes(bracketText);
      fallback = lookup.Get<int>("WIDTH", fallback);
      num = lookup.Get<int>("HEIGHT", fallback);
    }
    string innerText = this.GetInnerText(fullText1);
    Texture2D texture2D;
    if (log.cachedTextures.TryGetValue(innerText, out texture2D))
    {
      if (num == fallback && Object.op_Implicit((Object) texture2D))
        num = Mathf.CeilToInt((float) ((Texture) texture2D).height / (float) ((Texture) texture2D).width * (float) fallback);
    }
    else
    {
      WebTexture webTexture;
      if (log.cachedDownloadedTextures.TryGetValue(innerText, out webTexture) && num == fallback && Object.op_Implicit((Object) webTexture.texture))
        num = Mathf.CeilToInt((float) ((Texture) webTexture.texture).height / (float) ((Texture) webTexture.texture).width * (float) fallback);
    }
    return num;
  }

  protected virtual (string name, string value) ParseAttribute(string fullAttribute)
  {
    string[] strArray = fullAttribute.Split('=', StringSplitOptions.None);
    return (strArray[0], strArray[1]);
  }

  protected Lookup ContainerAttributes(string bracketText)
  {
    Lookup lookup = new Lookup();
    string str1 = "Splitting bracketText";
    try
    {
      string[] strArray = bracketText.Split('>', StringSplitOptions.None);
      if (strArray.Length != 0)
      {
        List<(string name, Type type)> list = this.Attributes.ToList<(string, Type)>();
        foreach (string fullAttribute in ((IEnumerable<string>) strArray[0].Split(' ', StringSplitOptions.None)).Where<string>((Func<string, bool>) (s => !string.IsNullOrWhiteSpace(s))))
        {
          str1 = fullAttribute;
          (string name, string value) attribute = this.ParseAttribute(fullAttribute);
          string name1 = attribute.name;
          string str2 = attribute.value.Trim('"');
          foreach ((string name2, Type type) in list)
          {
            if (name1.ToUpperInvariant() == name2.ToUpperInvariant())
            {
              object obj = ParseHelper.FromString(str2, type);
              lookup[name1.ToUpperInvariant()] = obj;
              break;
            }
          }
        }
      }
    }
    catch (Exception ex)
    {
      Log.ErrorOnce($"Exception thrown grabbing inner properties {bracketText}\nFailed: {str1}\nException={ex}", bracketText.GetHashCode());
    }
    return lookup;
  }
}
