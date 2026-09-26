// Decompiled with JetBrains decompiler
// Type: SmashTools.Scribe_ObjectCollection
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

#nullable disable
namespace SmashTools;

public static class Scribe_ObjectCollection
{
  public static void Look<T>(ref List<T> list, string label, bool forceSave = true)
  {
    List<object> list1 = list.Cast<object>().ToList<object>();
    Scribe_ObjectCollection.Look(ref list1, label, forceSave);
    list = list1.Cast<T>().ToList<T>();
  }

  public static void Look(ref List<object> list, string label, bool forceSave = true)
  {
    if (Scribe.EnterNode(label))
    {
      try
      {
        if (Scribe.mode == 1)
        {
          if (list == null)
          {
            Scribe.saver.WriteAttribute("IsNull", "True");
          }
          else
          {
            for (int index = 0; index < list.Count; ++index)
            {
              object obj = list[index];
              Scribe_ObjectValue.Look(ref obj, "li", forceSave);
            }
          }
        }
        else
        {
          if (Scribe.mode != 2)
            return;
          XmlNode curXmlParent = Scribe.loader.curXmlParent;
          XmlAttribute attribute = curXmlParent.Attributes["IsNull"];
          if (attribute != null && attribute.Value.ToLower() == "true")
          {
            list = (List<object>) null;
          }
          else
          {
            list = new List<object>(curXmlParent.ChildNodes.Count);
            foreach (XmlNode childNode in curXmlParent.ChildNodes)
            {
              object obj = ObjectValueExtractor.ValueFromNode(childNode);
              list.Add(obj);
            }
          }
        }
      }
      finally
      {
        Scribe.ExitNode();
      }
    }
    else
    {
      if (Scribe.mode != 2)
        return;
      list = (List<object>) null;
    }
  }
}
