// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationPropertyParent
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using SmashTools.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Verse;

#nullable disable
namespace SmashTools.Animations;

public class AnimationPropertyParent : 
  IXmlExport,
  ISelectableUI,
  IEnumerable<AnimationProperty>,
  IEnumerable
{
  private string identifier;
  private string label;
  private string name;
  private Type type;
  private readonly List<AnimationProperty> properties = new List<AnimationProperty>();
  private readonly List<ObjectPath> hierarchyPath = new List<ObjectPath>();

  public AnimationPropertyParent()
  {
  }

  private AnimationPropertyParent(
    string identifier,
    string label,
    string name,
    Type type,
    List<ObjectPath> hierarchyPath)
  {
    this.identifier = identifier;
    this.label = label;
    this.name = name;
    this.type = type;
    this.hierarchyPath = hierarchyPath;
    this.IsIndexer = GenCollection.Any<ObjectPath>(hierarchyPath, (Predicate<ObjectPath>) (path => path.IsIndexer));
  }

  public string Identifier => this.identifier;

  public string Label => this.label;

  public string LabelWithIdentifier
  {
    get => this.Identifier == null ? this.Label : $"{this.Label} ({this.Identifier})";
  }

  public string Name => this.name;

  public Type Type => this.type;

  public bool IsValid => !this.properties.NullOrEmpty<AnimationProperty>();

  public bool IsSingle => this.properties.Count == 1;

  public List<AnimationProperty> Properties => this.properties;

  public bool IsIndexer { get; private set; }

  internal void SetSingle(AnimationProperty property)
  {
    if (this.IsSingle)
      this.properties[0] = property;
    else
      this.Add(property);
  }

  internal void Add(AnimationProperty property) => this.properties.Add(property);

  public void EvaluateFrame(IAnimationObject obj, int frame)
  {
    for (int index = 0; index < this.properties.Count; ++index)
      this.properties[index].Evaluate(obj, frame);
  }

  public bool AllKeyFramesAt(int frame)
  {
    if (!this.IsValid)
      return false;
    foreach (AnimationProperty property in this.properties)
    {
      if (!property.curve.KeyFrameAt((float) frame))
        return false;
    }
    return true;
  }

  public bool AnyKeyFrameAt(int frame)
  {
    foreach (AnimationProperty property in this.properties)
    {
      if (property.curve.KeyFrameAt((float) frame))
        return true;
    }
    return false;
  }

  internal void ResolveReferences()
  {
    foreach (AnimationProperty property in this.properties)
      property.ResolveReferences();
  }

  public IAnimationObject ObjectFromHierarchy(IAnimator animator)
  {
    object obj = (object) animator;
    for (int index = 0; index < this.hierarchyPath.Count; ++index)
    {
      obj = this.hierarchyPath[index].GetValue(obj);
      if (obj == null)
        return (IAnimationObject) null;
    }
    return obj as IAnimationObject;
  }

  void IXmlExport.Export()
  {
    XmlExporter.WriteElement("identifier", this.identifier);
    XmlExporter.WriteElement("label", this.label);
    XmlExporter.WriteElement("name", this.name);
    XmlExporter.WriteElement("type", GenTypes.GetTypeNameWithoutIgnoredNamespaces(this.type));
    XmlExporter.WriteCollection<AnimationProperty>("properties", (IEnumerable<AnimationProperty>) this.properties);
    XmlExporter.WriteCollection<ObjectPath>("hierarchyPath", (IEnumerable<ObjectPath>) this.hierarchyPath);
  }

  public IEnumerator<AnimationProperty> GetEnumerator()
  {
    return (IEnumerator<AnimationProperty>) this.properties.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();

  public static AnimationPropertyParent Create(
    string identifier,
    string label,
    FieldInfo fieldInfo,
    List<ObjectPath> hierarchyPath)
  {
    return new AnimationPropertyParent(identifier, label, fieldInfo.Name, fieldInfo.DeclaringType, hierarchyPath);
  }
}
