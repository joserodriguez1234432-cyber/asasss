// Decompiled with JetBrains decompiler
// Type: SmashTools.ScrollViewScope
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Runtime.InteropServices;
using UnityEngine;

#nullable disable
namespace SmashTools;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct ScrollViewScope : IDisposable
{
  public ScrollViewScope(Rect outRect, ref Vector2 scrollPosition, Rect viewRect)
  {
    UIElements.BeginScrollView(outRect, ref scrollPosition, viewRect);
  }

  public ScrollViewScope(
    Rect outRect,
    ref Vector2 scrollPosition,
    Rect viewRect,
    bool showHorizontalScrollbar)
  {
    UIElements.BeginScrollView(outRect, ref scrollPosition, viewRect, showHorizontalScrollbar);
  }

  public ScrollViewScope(
    Rect outRect,
    ref Vector2 scrollPosition,
    Rect viewRect,
    bool showHorizontalScrollbar,
    bool showVerticalScrollBar)
  {
    UIElements.BeginScrollView(outRect, ref scrollPosition, viewRect, showHorizontalScrollbar, showVerticalScrollBar);
  }

  public void Dispose() => UIElements.EndScrollView();
}
