// Decompiled with JetBrains decompiler
// Type: SmashTools.ObjectPath
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using SmashTools.Xml;
using System;
using System.Reflection;
using Verse;

#nullable disable
namespace SmashTools;

public class ObjectPath : IXmlExport
{
  private readonly Type type;
  private readonly string name;
  private readonly int index;

  public ObjectPath()
  {
  }

  public ObjectPath(FieldInfo fieldInfo, int index = -1)
  {
    this.index = index;
    this.type = fieldInfo.DeclaringType;
    this.name = fieldInfo.Name;
  }

  public int Index => this.index;

  public bool IsIndexer => this.index >= 0;

  public FieldInfo FieldInfo => AccessTools.Field(this.type, this.name);

  public object GetValue(object obj)
  {
    object obj1 = this.FieldInfo.GetValue(obj);
    if (!this.IsIndexer)
      return obj1;
    return this.FieldInfo.FieldType.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(obj1, new object[1]
    {
      (object) this.index
    });
  }

  void IXmlExport.Export()
  {
    XmlExporter.WriteElement("type", GenTypes.GetTypeNameWithoutIgnoredNamespaces(this.type));
    XmlExporter.WriteElement("name", this.name);
    XmlExporter.WriteObject<int>("index", this.index);
  }

  public static implicit operator ObjectPath(FieldInfo fieldInfo) => new ObjectPath(fieldInfo);
}
