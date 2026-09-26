// Decompiled with JetBrains decompiler
// Type: SmashTools.Scribe_ObjectValue
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using System.Xml;
using Verse;

#nullable disable
namespace SmashTools;

public static class Scribe_ObjectValue
{
  public static void Look(ref object obj, string label, bool forceSave = false)
  {
    if (Scribe.mode == 1)
    {
      if (obj.GetType() == typeof (TargetInfo))
        Log.Error($"Saving a TargetInfo {label} with Scribe_Values. TargetInfos must be saved with Scribe_TargetInfo.");
      else if (obj is Thing)
        Log.Error($"Using Scribe_Values with a Thing reference {label}. Use Scribe_References or Scribe_Deep instead.");
      else if (obj is IExposable)
        Log.Error($"Using Scribe_Values with a IExposable reference {label}. Use Scribe_References or Scribe_Deep instead.");
      else if (obj is Def)
      {
        Log.Error($"Using Scribe_Values with a Def {label}. Use Scribe_Defs instead.");
      }
      else
      {
        object defaultValue = obj.GetDefaultValue<object>();
        if (!forceSave && (obj != null || defaultValue == null) && (obj == null || obj.Equals(defaultValue)))
          return;
        if (obj == null)
        {
          if (!Scribe.EnterNode(label))
            return;
          try
          {
            Scribe.saver.WriteAttribute("IsNull", "True");
          }
          finally
          {
            Scribe.ExitNode();
          }
        }
        else
        {
          if (!Scribe.EnterNode(label))
            return;
          try
          {
            List<Pair<string, string>> attributeParams = new List<Pair<string, string>>()
            {
              Scribe_ObjectValue.InnerTypeAttributePair(obj),
              Scribe_ObjectValue.SavingTypeAttributePair(obj)
            };
            Scribe.saver.WriteElementWithAttributes(obj.ToString(), attributeParams);
          }
          finally
          {
            Scribe.ExitNode();
          }
        }
      }
    }
    else
    {
      if (Scribe.mode != 2)
        return;
      obj = ObjectValueExtractor.ValueFromNode((XmlNode) Scribe.loader.curXmlParent[label]);
    }
  }

  public static Type RetrieveObjectType(object obj)
  {
    return obj is INestedType nestedType ? nestedType.InnerType : obj.GetType();
  }

  public static Pair<string, string> InnerTypeAttributePair(object obj)
  {
    return new Pair<string, string>("Type", Scribe_ObjectValue.RetrieveObjectType(obj).ToString());
  }

  public static Pair<string, string> SavingTypeAttributePair(object obj)
  {
    return new Pair<string, string>("SavedField", (obj.GetType() == typeof (SavedField<object>)).ToString());
  }
}
