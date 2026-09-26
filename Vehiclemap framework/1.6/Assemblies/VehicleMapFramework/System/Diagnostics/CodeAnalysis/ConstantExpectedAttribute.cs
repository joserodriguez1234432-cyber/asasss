// Decompiled with JetBrains decompiler
// Type: System.Diagnostics.CodeAnalysis.ConstantExpectedAttribute
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using Microsoft.CodeAnalysis;

#nullable enable
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
[ExcludeFromCodeCoverage]
[Embedded]
internal sealed class ConstantExpectedAttribute : Attribute
{
  public object? Min { get; set; }

  public object? Max { get; set; }
}
