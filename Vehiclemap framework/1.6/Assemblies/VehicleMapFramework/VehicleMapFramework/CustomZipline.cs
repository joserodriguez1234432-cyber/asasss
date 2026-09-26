// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CustomZipline
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[UsedImplicitly]
[StaticConstructorOnStartup]
public class CustomZipline : DefModExtension
{
  public string texPath;
  public float? ziplineWidth;
  public float? ziplineEndOffset;
  public float? launcherOffset;
  public ThingDef ziplineEndDef;
  public ThingDef ziplineReturnDef;
  [Unsaved(false)]
  public CustomZipline.ZipLineData zipLineData;
  private static Material DefaultZiplineMat;

  static CustomZipline()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() => CustomZipline.DefaultZiplineMat = MaterialPool.MatFrom("VehicleMapFramework/Things/ZiplineTurret/Zipline")));
  }

  public virtual void ResolveReferences(Def parentDef)
  {
    base.ResolveReferences(parentDef);
    LongEventHandler.ExecuteWhenFinished((Action) (() => this.zipLineData = new CustomZipline.ZipLineData(this.texPath, this.ziplineWidth, this.ziplineEndOffset, this.launcherOffset, this.ziplineEndDef, this.ziplineReturnDef)));
  }

  public virtual IEnumerable<string> ConfigErrors()
  {
    ThingDef ziplineEndDef = this.ziplineEndDef;
    if ((ziplineEndDef != null ? (!GenTypes.SameOrSubclassOf<ZiplineEnd>(ziplineEndDef.thingClass) ? 1 : 0) : 0) != 0)
      yield return "ZiplineEndDef must be a subclass of ZiplineEnd";
    ThingDef ziplineReturnDef = this.ziplineReturnDef;
    if ((ziplineReturnDef != null ? (!GenTypes.SameOrSubclassOf<Bullet_ZiplineEndReturn>(ziplineReturnDef.thingClass) ? 1 : 0) : 0) != 0)
      yield return "ZiplineReturnDef must be a subclass of Bullet_ZiplineEndReturn";
  }

  public readonly struct ZipLineData
  {
    private readonly float? ziplineWidth;
    private readonly float? ziplineEndOffset;
    private readonly float? launcherOffset;
    private readonly Material ziplineMat;
    private const float DefaultZiplineWidth = 0.12f;
    private const float DefaultZiplineEndOffset = 0.42f;
    private const float DefaultLauncherOffset = 0.85f;

    public ZipLineData(
      string texPath,
      float? ziplineWidth,
      float? ziplineEndOffset,
      float? launcherOffset,
      ThingDef ziplineEndDef,
      ThingDef ziplineReturnDef)
    {
      this.ziplineMat = (Material) null;
      this.ziplineWidth = ziplineWidth;
      this.ziplineEndOffset = ziplineEndOffset;
      this.launcherOffset = launcherOffset;
      // ISSUE: reference to a compiler-generated field
      this.\u003CZiplineEndDef\u003Ek__BackingField = ziplineEndDef;
      // ISSUE: reference to a compiler-generated field
      this.\u003CZiplineReturnDef\u003Ek__BackingField = ziplineReturnDef;
      if (texPath == null)
        return;
      this.ziplineMat = MaterialPool.MatFrom(texPath, ShaderDatabase.Transparent);
      this.ziplineMat.mainTexture.wrapMode = (TextureWrapMode) 0;
      this.ziplineMat.enableInstancing = true;
    }

    public Material ZiplineMat => this.ziplineMat ?? CustomZipline.DefaultZiplineMat;

    public float ZiplineWidth => this.ziplineWidth ?? 0.12f;

    public float ZiplineEndOffset => this.ziplineEndOffset ?? 0.42f;

    public float LauncherOffset => this.launcherOffset ?? 0.85f;

    public ThingDef ZiplineEndDef
    {
      get => this.\u003CZiplineEndDef\u003Ek__BackingField ?? VMF_DefOf.VMF_ZiplineEnd;
    }

    public ThingDef ZiplineReturnDef
    {
      get
      {
        return this.\u003CZiplineReturnDef\u003Ek__BackingField ?? VMF_DefOf.VMF_Bullet_ZiplineTurretReturn;
      }
    }
  }
}
