// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.RenderTextureIdler
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public sealed class RenderTextureIdler : IDisposable
{
  private readonly RenderTexture renderTex;
  private readonly float expiryTime;
  private float timeSinceRead;

  private RenderTextureIdler(float expiryTime)
  {
    this.expiryTime = expiryTime;
    Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("CoreLib.Performance.UnityThread", "CoreLib.Performance");
    if ((object) typeInAnyAssembly == null)
      typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly("SmashTools.Performance.UnityThread", "SmashTools.Performance");
    MethodInfo methodInfo = AccessTools.Method(typeInAnyAssembly, "StartUpdate", (Type[]) null, (Type[]) null);
    methodInfo.Invoke((object) null, new object[1]
    {
      (object) Delegate.CreateDelegate(methodInfo.GetParameters()[0].ParameterType, (object) this, "Update")
    });
  }

  public RenderTextureIdler(RenderTexture renderTex, float expiryTime)
    : this(expiryTime)
  {
    this.renderTex = renderTex;
  }

  public bool Disposed => !Object.op_Implicit((Object) this.renderTex);

  public RenderTexture RenderTex
  {
    get
    {
      this.timeSinceRead = 0.0f;
      return this.renderTex;
    }
  }

  internal void SetTimeDirect(float time) => this.timeSinceRead = time;

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
    RenderTexture renderTex = this.renderTex;
    if (renderTex == null)
      return;
    renderTex.ReleaseAndDestroy();
  }
}
