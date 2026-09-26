// Decompiled with JetBrains decompiler
// Type: JetBrains.Annotations.MustDisposeResourceAttribute
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Diagnostics;

#nullable disable
namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Parameter)]
[Conditional("JETBRAINS_ANNOTATIONS")]
public sealed class MustDisposeResourceAttribute : Attribute
{
  public MustDisposeResourceAttribute() => this.Value = true;

  public MustDisposeResourceAttribute(bool value) => this.Value = value;

  public bool Value { get; }
}
