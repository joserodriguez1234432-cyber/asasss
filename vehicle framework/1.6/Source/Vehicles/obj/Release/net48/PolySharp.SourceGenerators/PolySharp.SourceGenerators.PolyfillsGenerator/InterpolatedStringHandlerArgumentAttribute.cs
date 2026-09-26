// Decompiled with JetBrains decompiler
// Type: System.Runtime.CompilerServices.InterpolatedStringHandlerArgumentAttribute
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

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
