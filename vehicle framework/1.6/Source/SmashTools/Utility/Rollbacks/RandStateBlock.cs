// Decompiled with JetBrains decompiler
// Type: SmashTools.RandStateBlock
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Runtime.InteropServices;
using Verse;

#nullable disable
namespace SmashTools;

[PublicAPI]
[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct RandStateBlock : IDisposable
{
  public RandStateBlock() => Rand.PushState();

  void IDisposable.Dispose() => Rand.PopState();
}
