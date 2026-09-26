// Decompiled with JetBrains decompiler
// Type: SmashTools.Rendering.RenderTextureIdler
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using SmashTools.Performance;
using System;
using UnityEngine;

#nullable disable
namespace SmashTools.Rendering;

[PublicAPI]
public class RenderTextureIdler : IDisposable
{
  private readonly RenderTextureBuffer buffer;
  private readonly float expiryTime;
  private float timeSinceRead;

  private RenderTextureIdler(float expiryTime)
  {
    this.expiryTime = expiryTime;
    UnityThread.StartUpdate(new UnityThread.OnUpdate(this.Update));
  }

  public RenderTextureIdler(RenderTextureBuffer buffer, float expiryTime)
    : this(expiryTime)
  {
    this.buffer = buffer;
  }

  public RenderTextureIdler(RenderTexture rtA, RenderTexture rtB, float expiryTime)
    : this(new RenderTextureBuffer(rtA, rtB), expiryTime)
  {
  }

  internal UnityThread.OnUpdate UpdateLoop => new UnityThread.OnUpdate(this.Update);

  public bool Disposed => !(bool) this.buffer;

  public RenderTexture Read
  {
    get
    {
      this.timeSinceRead = 0.0f;
      return this.buffer.Read;
    }
  }

  public RenderTexture Write
  {
    get
    {
      this.timeSinceRead = 0.0f;
      return this.buffer.Write;
    }
  }

  public RenderTexture GetWrite()
  {
    this.timeSinceRead = 0.0f;
    return this.buffer.GetWrite();
  }

  internal void SetTimeDirect(float timeSinceRead) => this.timeSinceRead = timeSinceRead;

  private bool Update()
  {
    this.timeSinceRead += Time.deltaTime;
    if ((double) this.timeSinceRead < (double) this.expiryTime)
      return true;
    this.Dispose();
    return false;
  }

  public void Dispose()
  {
    this.buffer?.Dispose();
    GC.SuppressFinalize((object) this);
  }
}
