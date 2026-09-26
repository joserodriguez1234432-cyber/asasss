// Decompiled with JetBrains decompiler
// Type: System.Runtime.CompilerServices.InterpolatedStringHandlerArgumentAttribute
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

#nullable enable
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
[ExcludeFromCodeCoverage]
[Embedded]
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
