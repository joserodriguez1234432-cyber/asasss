// Decompiled with JetBrains decompiler
// Type: System.Runtime.CompilerServices.OverloadResolutionPriorityAttribute
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Diagnostics.CodeAnalysis;

#nullable disable
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
[ExcludeFromCodeCoverage]
internal sealed class OverloadResolutionPriorityAttribute : Attribute
{
  public OverloadResolutionPriorityAttribute(int priority) => this.Priority = priority;

  public int Priority { get; }
}
