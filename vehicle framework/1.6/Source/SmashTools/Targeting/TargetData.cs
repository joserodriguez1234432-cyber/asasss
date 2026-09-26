// Decompiled with JetBrains decompiler
// Type: SmashTools.Targeting.TargetData`1
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Collections.Generic;

#nullable disable
namespace SmashTools.Targeting;

public readonly struct TargetData<T>
{
  public readonly List<T> targets;

  public TargetData() => this.targets = new List<T>();
}
