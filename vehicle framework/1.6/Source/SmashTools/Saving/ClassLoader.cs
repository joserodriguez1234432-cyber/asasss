// Decompiled with JetBrains decompiler
// Type: SmashTools.Xml.ClassLoader
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Reflection;
using System.Xml;
using Verse;

#nullable disable
namespace SmashTools.Xml;

public static class ClassLoader
{
  public static void Distribute<T>(XmlNode rootNode, T instance)
  {
    foreach (FieldInfo field in instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
    {
      string name = field.Name;
      XmlNode xmlNode = (XmlNode) rootNode[name];
      if (xmlNode != null)
      {
        object obj = GenGeneric.InvokeStaticGenericMethod(typeof (DirectXmlToObject), field.FieldType, "ObjectFromXml", new object[2]
        {
          (object) xmlNode,
          (object) true
        });
        field.SetValue((object) instance, obj);
      }
    }
  }
}
