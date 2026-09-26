// Decompiled with JetBrains decompiler
// Type: SmashTools.Rendering.RenderTextureBuffer
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using UnityEngine;

#nullable disable
namespace SmashTools.Rendering;

[PublicAPI]
public class RenderTextureBuffer : IDisposable
{
  private RenderTexture rtA;
  private RenderTexture rtB;

  public RenderTextureBuffer(RenderTexture rtA, RenderTexture rtB)
  {
    this.rtA = rtA;
    this.rtB = rtB;
    this.Read = rtA;
  }

  public RenderTexture Read { get; private set; }

  public RenderTexture Write
  {
    get => !Object.op_Equality((Object) this.Read, (Object) this.rtA) ? this.rtA : this.rtB;
  }

  public RenderTexture GetWrite()
  {
    this.Read = this.Write;
    return this.Read;
  }

  public void Dispose()
  {
    this.rtA.Release();
    this.rtB.Release();
    Object.Destroy((Object) this.rtA);
    Object.Destroy((Object) this.rtB);
    GC.SuppressFinalize((object) this);
  }

  public static implicit operator bool(RenderTextureBuffer buffer)
  {
    if (buffer == null)
      return false;
    return Object.op_Implicit((Object) buffer.rtA) || Object.op_Implicit((Object) buffer.rtB);
  }
}
