// Decompiled with JetBrains decompiler
// Type: SmashTools.Scribe_NestedCollections
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

#nullable disable
namespace SmashTools;

public class Scribe_NestedCollections
{
  public static void Look<K, K2, V>(
    ref Dictionary<K, Dictionary<K2, V>> dict,
    string label,
    LookMode keyLookMode,
    LookMode innerKeyLookMode,
    LookMode innerValueLookMode)
  {
    if (keyLookMode == 3 || innerKeyLookMode == 3 || innerValueLookMode == 3)
    {
      Log.Warning("Scribe_NestedCollections LookMode.Reference not yet supported.");
    }
    else
    {
      List<K2> keys = new List<K2>();
      List<V> second = new List<V>();
      if (Scribe.EnterNode(label))
      {
        try
        {
          if (Scribe.mode == 1 && dict == null)
          {
            Scribe.saver.WriteAttribute("IsNull", "True");
          }
          else
          {
            if (Scribe.mode == 2)
            {
              XmlAttribute attribute = Scribe.loader.curXmlParent.Attributes["IsNull"];
              dict = attribute == null || !(attribute.Value.ToLower() == "true") ? new Dictionary<K, Dictionary<K2, V>>() : (Dictionary<K, Dictionary<K2, V>>) null;
            }
            if (Scribe.mode == 1 && dict != null)
            {
              foreach (KeyValuePair<K, Dictionary<K2, V>> keyValuePair in dict)
              {
                if (Scribe.EnterNode("li"))
                {
                  try
                  {
                    K key = keyValuePair.Key;
                    switch (keyLookMode - 1)
                    {
                      case 0:
                        Scribe_Values.Look<K>(ref key, "key", default (K), false);
                        break;
                      case 1:
                        Scribe_Deep.Look<K>(ref key, "key", Array.Empty<object>());
                        break;
                      case 2:
                        if (key is ILoadReferenceable iloadReferenceable)
                        {
                          Scribe_References.Look<ILoadReferenceable>(ref iloadReferenceable, "referenceable", false);
                          break;
                        }
                        Log.Error("Cannot use LookMode.Reference with non ILoadReferenceable object");
                        break;
                      case 3:
                        if (key is Def def)
                        {
                          Scribe_Defs.Look<Def>(ref def, "def");
                          break;
                        }
                        break;
                    }
                    keys.Clear();
                    second.Clear();
                    keys.AddRange((IEnumerable<K2>) keyValuePair.Value.Keys);
                    second.AddRange((IEnumerable<V>) keyValuePair.Value.Values);
                    Scribe_Collections.Look<K2>(ref keys, "innerKeys", innerKeyLookMode, Array.Empty<object>());
                    if (typeof (V) == typeof (object) || typeof (V) == typeof (SavedField<object>))
                      Scribe_ObjectCollection.Look<V>(ref second, "innerValues");
                    else
                      Scribe_Collections.Look<V>(ref second, "innerValues", innerValueLookMode, Array.Empty<object>());
                  }
                  finally
                  {
                    Scribe.ExitNode();
                  }
                }
              }
              K k = default (K);
            }
            else if (Scribe.mode == 2)
            {
              XmlNode curXmlParent = Scribe.loader.curXmlParent;
              XmlAttribute attribute = curXmlParent.Attributes["IsNull"];
              if (attribute != null && attribute.Value.ToLower() == "true")
              {
                if (keyLookMode == 3)
                  Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml((List<string>) null, (string) null);
                dict = (Dictionary<K, Dictionary<K2, V>>) null;
                return;
              }
              dict = new Dictionary<K, Dictionary<K2, V>>();
              foreach (XmlNode childNode in curXmlParent.ChildNodes)
              {
                K key = default (K);
                switch (keyLookMode - 1)
                {
                  case 0:
                    key = ScribeExtractor.ValueFromNode<K>(childNode.ChildNodes[0], default (K));
                    break;
                  case 1:
                    key = ScribeExtractor.SaveableFromNode<K>(childNode.ChildNodes[0], new object[0]);
                    break;
                  case 2:
                    string innerText = childNode.ChildNodes[0].InnerText;
                    Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(new List<string>()
                    {
                      innerText
                    }, "");
                    break;
                  case 3:
                    key = ScribeExtractor.DefFromNodeUnsafe<K>(childNode.ChildNodes[0]);
                    break;
                  case 4:
                    key = (K) (object) ScribeExtractor.LocalTargetInfoFromNode(childNode.ChildNodes[0], "0", LocalTargetInfo.Invalid);
                    break;
                  case 5:
                    key = (K) (object) ScribeExtractor.TargetInfoFromNode(childNode.ChildNodes[0], "0", TargetInfo.Invalid);
                    break;
                  case 6:
                    key = (K) (object) ScribeExtractor.GlobalTargetInfoFromNode(childNode.ChildNodes[0], "0", GlobalTargetInfo.Invalid);
                    break;
                  case 7:
                    key = (K) ScribeExtractor.BodyPartFromNode(childNode.ChildNodes[0], "0", (BodyPartRecord) null);
                    break;
                }
                Scribe_NestedCollections.ExtractToLists<K2, V>(childNode, ref keys, ref second, innerKeyLookMode, innerValueLookMode);
                dict.Add(key, keys.Zip((IEnumerable<V>) second, (k, v) => new
                {
                  k = k,
                  v = v
                }).ToDictionary(d => d.k, d => d.v));
              }
            }
            if (Scribe.mode != 3 || keyLookMode == 3 || innerKeyLookMode == 3)
              ;
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
        dict = (Dictionary<K, Dictionary<K2, V>>) null;
      }
    }
  }

  public static void Look<K, V>(
    ref Dictionary<K, List<V>> dict,
    string label,
    LookMode keyLookMode,
    LookMode valueLookMode)
  {
    List<K> kList = new List<K>();
    List<V> list = new List<V>();
    if (Scribe.EnterNode(label))
    {
      try
      {
        if (Scribe.mode == 1 && dict == null)
        {
          Scribe.saver.WriteAttribute("IsNull", "True");
        }
        else
        {
          if (Scribe.mode == 2)
          {
            XmlAttribute attribute = Scribe.loader.curXmlParent.Attributes["IsNull"];
            dict = attribute == null || !(attribute.Value.ToLower() == "true") ? new Dictionary<K, List<V>>() : (Dictionary<K, List<V>>) null;
          }
          if (Scribe.mode == 1 && dict != null)
          {
            foreach (KeyValuePair<K, List<V>> keyValuePair in dict)
            {
              if (Scribe.EnterNode("li"))
              {
                try
                {
                  K key = keyValuePair.Key;
                  switch (keyLookMode - 1)
                  {
                    case 0:
                      Scribe_Values.Look<K>(ref key, "key", default (K), false);
                      break;
                    case 1:
                      Scribe_Deep.Look<K>(ref key, "key", Array.Empty<object>());
                      break;
                    case 2:
                      if (key is ILoadReferenceable iloadReferenceable)
                      {
                        Scribe_References.Look<ILoadReferenceable>(ref iloadReferenceable, "referenceable", false);
                        break;
                      }
                      Log.Error("Cannot use LookMode.Reference with non ILoadReferenceable object");
                      break;
                    case 3:
                      if (key is Def def)
                      {
                        Scribe_Defs.Look<Def>(ref def, "key");
                        break;
                      }
                      break;
                  }
                  list.Clear();
                  list.AddRange((IEnumerable<V>) keyValuePair.Value);
                  Scribe_Collections.Look<V>(ref list, "values", valueLookMode, Array.Empty<object>());
                }
                finally
                {
                  Scribe.ExitNode();
                }
              }
            }
            K k = default (K);
          }
          else if (Scribe.mode == 2)
          {
            XmlNode curXmlParent = Scribe.loader.curXmlParent;
            XmlAttribute attribute = curXmlParent.Attributes["IsNull"];
            if (attribute != null && attribute.Value.ToLower() == "true")
            {
              if (keyLookMode == 3)
                Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml((List<string>) null, (string) null);
              dict = (Dictionary<K, List<V>>) null;
              return;
            }
            dict = new Dictionary<K, List<V>>();
            foreach (XmlNode childNode in curXmlParent.ChildNodes)
            {
              K key = default (K);
              switch (keyLookMode - 1)
              {
                case 0:
                  key = ScribeExtractor.ValueFromNode<K>(childNode.ChildNodes[0], default (K));
                  break;
                case 1:
                  key = ScribeExtractor.SaveableFromNode<K>(childNode.ChildNodes[0], new object[0]);
                  break;
                case 2:
                  string innerText = childNode.ChildNodes[0].InnerText;
                  Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(new List<string>()
                  {
                    innerText
                  }, "");
                  break;
                case 3:
                  key = ScribeExtractor.DefFromNodeUnsafe<K>(childNode.ChildNodes[0]);
                  break;
                case 4:
                  key = (K) (object) ScribeExtractor.LocalTargetInfoFromNode(childNode.ChildNodes[0], "0", LocalTargetInfo.Invalid);
                  break;
                case 5:
                  key = (K) (object) ScribeExtractor.TargetInfoFromNode(childNode.ChildNodes[0], "0", TargetInfo.Invalid);
                  break;
                case 6:
                  key = (K) (object) ScribeExtractor.GlobalTargetInfoFromNode(childNode.ChildNodes[0], "0", GlobalTargetInfo.Invalid);
                  break;
                case 7:
                  key = (K) ScribeExtractor.BodyPartFromNode(childNode.ChildNodes[0], "0", (BodyPartRecord) null);
                  break;
              }
              Scribe_NestedCollections.ExtractList<V>(childNode.ChildNodes[1].ChildNodes, ref list, valueLookMode);
              Scribe_Collections.Look<V>(ref list, "values", valueLookMode, Array.Empty<object>());
              dict.Add(key, list);
            }
          }
          if (Scribe.mode != 3 || keyLookMode == 3)
            ;
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
      dict = (Dictionary<K, List<V>>) null;
    }
  }

  public static void Look<K, V>(
    ref Dictionary<K, HashSet<V>> dict,
    string label,
    LookMode keyLookMode,
    LookMode valueLookMode)
  {
    Dictionary<K, List<V>> dict1 = new Dictionary<K, List<V>>();
    if (Scribe.mode == 1)
    {
      foreach (KeyValuePair<K, HashSet<V>> keyValuePair in dict)
        dict1.Add(keyValuePair.Key, keyValuePair.Value.ToList<V>());
    }
    Scribe_NestedCollections.Look<K, V>(ref dict1, label, keyLookMode, valueLookMode);
    if (Scribe.mode != 2)
      return;
    dict = new Dictionary<K, HashSet<V>>();
    foreach (KeyValuePair<K, List<V>> keyValuePair in dict1)
      dict.Add(keyValuePair.Key, keyValuePair.Value.ToHashSet<V>());
  }

  private static void ExtractToLists<K, V>(
    XmlNode parentNode,
    ref List<K> keys,
    ref List<V> values,
    LookMode keyLookMode,
    LookMode valueLookMode)
  {
    Scribe_NestedCollections.ExtractList<K>(parentNode.ChildNodes[1].ChildNodes, ref keys, keyLookMode);
    Scribe_NestedCollections.ExtractList<V>(parentNode.ChildNodes[2].ChildNodes, ref values, valueLookMode);
  }

  private static void ExtractList<T>(XmlNodeList nodeList, ref List<T> list, LookMode lookMode)
  {
    list.Clear();
    foreach (XmlNode node in nodeList)
    {
      switch (lookMode - 1)
      {
        case 0:
          T obj1 = ScribeExtractor.ValueFromNode<T>(node, default (T));
          list.Add(obj1);
          continue;
        case 1:
          T obj2 = ScribeExtractor.SaveableFromNode<T>(node, new object[0]);
          list.Add(obj2);
          continue;
        case 3:
          T obj3 = ScribeExtractor.DefFromNodeUnsafe<T>(node);
          list.Add(obj3);
          continue;
        default:
          T obj4 = (T) ObjectValueExtractor.ValueFromNode(node);
          list.Add(obj4);
          continue;
      }
    }
  }
}
