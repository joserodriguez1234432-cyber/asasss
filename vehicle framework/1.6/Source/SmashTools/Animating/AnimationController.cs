// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationController
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using SmashTools.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace SmashTools.Animations;

public class AnimationController : IAnimationFile, IXmlExport
{
  public const string DefaultControllerName = "New-Controller";
  public const string FileExtension = ".ctlr";
  private Guid guid;
  public List<AnimationParameter> parameters = new List<AnimationParameter>();
  public List<AnimationLayer> layers = new List<AnimationLayer>();

  public Guid Guid => this.guid;

  public string FilePath { get; set; }

  public string FileName { get; set; }

  public string FileNameWithExtension => this.FileName + ".ctlr";

  public void AddLayer(string name)
  {
    name = AnimationLoader.GetAvailableName(this.layers.Select<AnimationLayer, string>((Func<AnimationLayer, string>) (layer => layer.name)), name);
    AnimationLayer layer1 = AnimationLayer.CreateLayer(name);
    layer1.Controller = this;
    this.layers.Add(layer1);
  }

  void IXmlExport.Export()
  {
    XmlExporter.WriteObject<Guid>("guid", this.guid);
    XmlExporter.WriteCollection<AnimationParameter>("parameters", (IEnumerable<AnimationParameter>) this.parameters, (Func<AnimationParameter, (string, string)>) (parameter => ("Class", GenTypes.GetTypeNameWithoutIgnoredNamespaces(parameter.GetType()))));
    XmlExporter.WriteCollection<AnimationLayer>("layers", (IEnumerable<AnimationLayer>) this.layers);
  }

  void IAnimationFile.ResolveReferences()
  {
    foreach (AnimationLayer layer in this.layers)
    {
      layer.Controller = this;
      layer.ResolveReferences();
    }
  }

  public static implicit operator bool(AnimationController controller) => controller != null;

  public static AnimationController EmptyController()
  {
    AnimationController animationController = new AnimationController();
    animationController.FileName = "New-Controller";
    animationController.AddLayer("Base Layer");
    animationController.guid = Guid.NewGuid();
    return animationController;
  }
}
