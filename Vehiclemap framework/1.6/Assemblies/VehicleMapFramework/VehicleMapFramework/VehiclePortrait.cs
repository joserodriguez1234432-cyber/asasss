// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehiclePortrait
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public sealed class VehiclePortrait : IDisposable
{
  private RenderTexture renderTex;
  private RenderTextureIdler idler;
  private VehiclePortrait.Config config;
  [CompilerGenerated]
  private bool \u003CRedrawPortrait\u003Ek__BackingField = true;

  public VehiclePortrait() => this.config = new VehiclePortrait.Config();

  public VehiclePortrait(in VehiclePortrait.Config config) => this.config = config;

  private RenderTexture RenderTexture => this.idler == null ? this.renderTex : this.idler.RenderTex;

  public bool RedrawPortrait
  {
    get
    {
      return this.\u003CRedrawPortrait\u003Ek__BackingField || Object.op_Equality((Object) this.RenderTexture, (Object) null);
    }
    private set => this.\u003CRedrawPortrait\u003Ek__BackingField = value;
  }

  public void MarkDirty() => this.RedrawPortrait = true;

  public void Dispose()
  {
    RenderTexture renderTex = this.renderTex;
    if (renderTex != null)
      renderTex.ReleaseAndDestroy();
    this.renderTex = (RenderTexture) null;
    this.idler?.Dispose();
    this.idler = (RenderTextureIdler) null;
  }

  private void CreateRenderTexture(Rect rect, [RequiresLocation, In] ref BlitRequest request)
  {
    if (Object.op_Inequality((Object) this.RenderTexture, (Object) null))
      return;
    if ((double) this.config.expiryTime > 0.0)
      this.idler = new RenderTextureIdler(VehicleGui.CreateRenderTexture(rect, ref request, 2f), this.config.expiryTime);
    else
      this.renderTex = VehicleGui.CreateRenderTexture(rect, ref request, 2f);
  }

  public void Draw(Rect rect, in BlitRequest request)
  {
    if (Event.current.type != 7)
      return;
    Widgets.BeginGroup(rect);
    Rect rect1 = GenUI.AtZero(rect);
    if (this.RedrawPortrait)
    {
      this.CreateRenderTexture(rect1, ref request);
      VehicleGui.Blit(this.RenderTexture, rect1, ref request, this.config.iconScale, this.config.forceCentering);
      this.RedrawPortrait = false;
    }
    GUI.DrawTexture(rect1, (Texture) this.RenderTexture);
    Widgets.EndGroup();
  }

  public struct Config
  {
    public float iconScale;
    public float expiryTime;
    public bool forceCentering;

    public Config()
    {
      this.iconScale = 1f;
      this.expiryTime = -1f;
      this.forceCentering = false;
    }
  }
}
