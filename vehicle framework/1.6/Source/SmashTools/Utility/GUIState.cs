// Decompiled with JetBrains decompiler
// Type: SmashTools.GUIState
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
namespace SmashTools;

public static class GUIState
{
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Disable()
  {
    GUI.enabled = false;
    GUI.color = UIElements.InactiveColor;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void Enable()
  {
    GUI.enabled = true;
    GUI.color = Color.white;
  }

  public readonly struct Disabler : IDisposable
  {
    private readonly bool prevState;
    private readonly Color prevColor;

    public Disabler()
    {
      this.prevState = GUI.enabled;
      this.prevColor = GUI.color;
    }

    void IDisposable.Dispose()
    {
      GUI.enabled = this.prevState;
      GUI.color = this.prevColor;
    }
  }
}
