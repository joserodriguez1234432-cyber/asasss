// Decompiled with JetBrains decompiler
// Type: SmashTools.Scribe_Array
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Xml;
using Verse;

#nullable disable
namespace SmashTools;

public static class Scribe_Array
{
  public static void Look<T>(
    ref T[] array,
    string label,
    bool saveDestroyedThings = false,
    LookMode lookMode = 0,
    params object[] ctorArgs)
  {
    if (lookMode == null && !Scribe_Universal.TryResolveLookMode(typeof (T), ref lookMode, false, false))
      Trace.Fail($"LookArray call with type {typeof (T)} must have lookMode set explicitly.");
    else if (Scribe.EnterNode(label))
    {
      try
      {
        if (Scribe.mode == 1)
        {
          if (array == null)
          {
            Scribe.saver.WriteAttribute("IsNull", "True");
          }
          else
          {
            Scribe.saver.WriteAttribute("Size", array.Length.ToString());
            for (int index = 0; index < array.Length; ++index)
            {
              T obj1 = array[index];
              if (lookMode == 1)
                Scribe_Values.Look<T>(ref obj1, "li", default (T), true);
              else if (lookMode == 5)
              {
                LocalTargetInfo localTargetInfo = (LocalTargetInfo) (object) obj1;
                Scribe_TargetInfo.Look(ref localTargetInfo, saveDestroyedThings, "li");
              }
              else if (lookMode == 6)
              {
                TargetInfo targetInfo = (TargetInfo) (object) obj1;
                Scribe_TargetInfo.Look(ref targetInfo, saveDestroyedThings, "li");
              }
              else if (lookMode == 7)
              {
                GlobalTargetInfo globalTargetInfo = (GlobalTargetInfo) (object) obj1;
                Scribe_TargetInfo.Look(ref globalTargetInfo, saveDestroyedThings, "li");
              }
              else if (lookMode == 4)
              {
                Def def = (Def) (object) obj1;
                Scribe_Defs.Look<Def>(ref def, "li");
              }
              else if (lookMode == 8)
              {
                BodyPartRecord bodyPartRecord = (BodyPartRecord) (object) obj1;
                Scribe_BodyParts.Look(ref bodyPartRecord, "li", (BodyPartRecord) null);
              }
              else if (lookMode == 2)
                Scribe_Deep.Look<T>(ref obj1, saveDestroyedThings, "li", ctorArgs);
              else if (lookMode == 3)
              {
                if ((object) obj1 != null && !((object) obj1 is ILoadReferenceable))
                {
                  string str;
                  if ((object) obj1 == null)
                  {
                    str = "Null";
                  }
                  else
                  {
                    ref T local1 = ref obj1;
                    if ((object) default (T) == null)
                    {
                      T obj2 = local1;
                      ref T local2 = ref obj2;
                      if ((object) obj2 == null)
                      {
                        str = (string) null;
                        goto label_29;
                      }
                      local1 = ref local2;
                    }
                    str = local1.GetType()?.Name;
                  }
label_29:
                  throw new InvalidOperationException($"Cannot save reference to {str} item if it is not ILoadReferenceable");
                }
                ILoadReferenceable iloadReferenceable = (object) obj1 as ILoadReferenceable;
                Scribe_References.Look<ILoadReferenceable>(ref iloadReferenceable, "li", saveDestroyedThings);
              }
            }
          }
        }
        else if (Scribe.mode == 2)
        {
          XmlNode curXmlParent = Scribe.loader.curXmlParent;
          XmlAttribute attribute1 = curXmlParent.Attributes["IsNull"];
          if (attribute1 != null && attribute1.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
          {
            if (lookMode == 3)
              Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml((List<string>) null, (string) null);
            array = (T[]) null;
          }
          else
          {
            XmlAttribute attribute2 = curXmlParent.Attributes["Size"];
            int capacity;
            if (attribute2 == null)
            {
              Trace.Fail("Size attribute for array not found. Defaulting to size of xml items listed.");
              capacity = curXmlParent.ChildNodes.Count;
            }
            else
              capacity = Convert.ToInt32(attribute2.Value);
            array = new T[capacity];
            if (lookMode == 3)
            {
              List<string> stringList = new List<string>(capacity);
              foreach (XmlNode childNode in curXmlParent.ChildNodes)
                stringList.Add(childNode.InnerText);
              Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(stringList, "");
            }
            else
            {
              int index = 0;
              foreach (XmlNode childNode in curXmlParent.ChildNodes)
              {
                T obj3;
                switch (lookMode - 1)
                {
                  case 0:
                    obj3 = ScribeExtractor.ValueFromNode<T>(childNode, default (T));
                    break;
                  case 1:
                    obj3 = ScribeExtractor.SaveableFromNode<T>(childNode, ctorArgs);
                    break;
                  case 3:
                    obj3 = ScribeExtractor.DefFromNodeUnsafe<T>(childNode);
                    break;
                  case 4:
                    obj3 = (T) (object) ScribeExtractor.LocalTargetInfoFromNode(childNode, index.ToString(), LocalTargetInfo.Invalid);
                    break;
                  case 5:
                    obj3 = (T) (object) ScribeExtractor.TargetInfoFromNode(childNode, index.ToString(), TargetInfo.Invalid);
                    break;
                  case 6:
                    obj3 = (T) (object) ScribeExtractor.GlobalTargetInfoFromNode(childNode, index.ToString(), GlobalTargetInfo.Invalid);
                    break;
                  case 7:
                    obj3 = (T) ScribeExtractor.BodyPartFromNode(childNode, index.ToString(), (BodyPartRecord) null);
                    break;
                  default:
                    throw new NotImplementedException();
                }
                T obj4 = obj3;
                array[index] = obj4;
                ++index;
              }
            }
          }
        }
        else
        {
          if (Scribe.mode != 3)
            return;
          if (lookMode == 3)
          {
            array = Scribe.loader.crossRefs.TakeResolvedRefList<T>("").ToArray();
          }
          else
          {
            if (lookMode != 5 && lookMode != 6 && lookMode != 7)
              return;
            for (int index1 = 0; index1 < array.Length; ++index1)
            {
              T[] objArray = array;
              int index2 = index1;
              T obj;
              switch (lookMode - 5)
              {
                case 0:
                  obj = (T) (object) ScribeExtractor.ResolveLocalTargetInfo((LocalTargetInfo) (object) array[index1], index1.ToString());
                  break;
                case 1:
                  obj = (T) (object) ScribeExtractor.ResolveTargetInfo((TargetInfo) (object) array[index1], index1.ToString());
                  break;
                case 2:
                  obj = (T) (object) ScribeExtractor.ResolveGlobalTargetInfo((GlobalTargetInfo) (object) array[index1], index1.ToString());
                  break;
                default:
                  throw new InvalidOperationException();
              }
              objArray[index2] = obj;
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
      if (lookMode == 3)
        Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml((List<string>) null, label);
      array = (T[]) null;
    }
  }
}
