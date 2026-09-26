// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationClip
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using SmashTools.Xml;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace SmashTools.Animations;

public sealed class AnimationClip : IAnimationFile, IXmlExport
{
  public const string DefaultAnimName = "Untitled";
  public const string FileExtension = ".rwa";
  public const int DefaultFrameCount = 60;
  public int frameCount = 60;
  public int cycleOffset;
  private Guid guid;
  public List<AnimationPropertyParent> properties = new List<AnimationPropertyParent>();
  public List<AnimationEvent> events = new List<AnimationEvent>();

  public Guid Guid => this.guid;

  public string FilePath { get; set; }

  public string FileName { get; set; }

  public string FileNameWithExtension => this.FileName + ".rwa";

  public void SetFrame()
  {
    this.frameCount = 60;
    if (this.properties.Count <= 0)
      return;
    int num1 = 0;
    foreach (AnimationPropertyParent property1 in this.properties)
    {
      int num2 = -1;
      foreach (AnimationProperty property2 in property1.Properties)
        num2 = AnimationClip.MaxFrame(property2);
      if (num2 > num1)
        num1 = num2;
    }
    if (num1 < 0)
      return;
    this.frameCount = num1;
  }

  public void RecacheFrameCount()
  {
    this.frameCount = 60;
    if (this.properties.Count <= 0)
      return;
    int num1 = 0;
    foreach (AnimationPropertyParent property1 in this.properties)
    {
      int num2 = -1;
      foreach (AnimationProperty property2 in property1.Properties)
        num2 = AnimationClip.MaxFrame(property2);
      if (num2 > num1)
        num1 = num2;
    }
    if (num1 < 0)
      return;
    this.frameCount = num1;
  }

  private static int MaxFrame(AnimationProperty property)
  {
    return property.curve.PointsCount > 0 ? property.curve.points.Max<KeyFrame>((Func<KeyFrame, int>) (keyFrame => keyFrame.frame)) : -1;
  }

  internal void ValidateEventOrder() => this.events.Sort();

  public static AnimationClip CreateEmpty()
  {
    return new AnimationClip()
    {
      FileName = AnimationLoader.GetAvailableName(AnimationLoader.Cache<AnimationClip>.GetAll().Select<AnimationClip, string>((Func<AnimationClip, string>) (clip => clip.FileName)), "Untitled"),
      guid = Guid.NewGuid()
    };
  }

  void IAnimationFile.ResolveReferences()
  {
    if (this.properties.NullOrEmpty<AnimationPropertyParent>())
      return;
    foreach (AnimationPropertyParent property in this.properties)
      property.ResolveReferences();
  }

  void IXmlExport.Export()
  {
    this.ValidateEventOrder();
    XmlExporter.WriteObject<int>("frameCount", this.frameCount);
    XmlExporter.WriteObject<int>("cycleOffset", this.cycleOffset);
    XmlExporter.WriteObject<Guid>("guid", this.guid);
    XmlExporter.WriteCollection<AnimationPropertyParent>("properties", (IEnumerable<AnimationPropertyParent>) this.properties);
    XmlExporter.WriteCollection<AnimationEvent>("events", (IEnumerable<AnimationEvent>) this.events);
  }

  public static implicit operator bool(AnimationClip clip) => clip != null;
}
