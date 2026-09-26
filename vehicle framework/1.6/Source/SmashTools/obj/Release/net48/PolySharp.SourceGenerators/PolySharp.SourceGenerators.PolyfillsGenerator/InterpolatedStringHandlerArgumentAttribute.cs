// Decompiled with JetBrains decompiler
// Type: System.Runtime.CompilerServices.InterpolatedStringHandlerArgumentAttribute
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Diagnostics.CodeAnalysis;

#nullable enable
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
[ExcludeFromCodeCoverage]
internal sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
{
  public InterpolatedStringHandlerArgumentAttribute(string argument)
  {
    this.Arguments = new string[1]{ argument };
  }

  public InterpolatedStringHandlerArgumentAttribute(params string[] arguments)
  {
    this.Arguments = arguments;
  }

  public string[] Arguments { get; }
}
