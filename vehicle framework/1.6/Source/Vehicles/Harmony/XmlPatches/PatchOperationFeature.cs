// Decompiled with JetBrains decompiler
// Type: Vehicles.PatchOperationFeature
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Xml;
using Vehicles.Config;
using Verse;

#nullable disable
namespace Vehicles;

public class PatchOperationFeature : PatchOperation
{
  public string feature;
  public PatchOperation patch;
  private readonly FeatureFlags featureFlags;

  public PatchOperationFeature() => this.featureFlags = FeatureFlags.Default;

  internal PatchOperationFeature(FeatureFlags featureFlags) => this.featureFlags = featureFlags;

  protected virtual bool ApplyWorker(XmlDocument xml)
  {
    if (!this.featureFlags.IsEnabled(this.feature))
      return true;
    return this.patch != null && this.patch.Apply(xml);
  }
}
