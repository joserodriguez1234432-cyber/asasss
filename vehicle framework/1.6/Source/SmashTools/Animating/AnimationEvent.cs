// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationEvent
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using SmashTools.Xml;
using System;

#nullable disable
namespace SmashTools.Animations;

public class AnimationEvent : IXmlExport, ISelectableUI, IComparable<AnimationEvent>
{
  public int frame;
  public DynamicDelegate method;

  int IComparable<AnimationEvent>.CompareTo(AnimationEvent other)
  {
    if (this.frame < other.frame)
      return -1;
    return this.frame > other.frame ? 1 : 0;
  }

  void IXmlExport.Export()
  {
    XmlExporter.WriteObject<int>("frame", this.frame);
    XmlExporter.WriteObject<DynamicDelegate>("method", this.method);
  }
}
