// Decompiled with JetBrains decompiler
// Type: System.Runtime.CompilerServices.InterpolatedStringHandlerArgumentAttribute
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

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
