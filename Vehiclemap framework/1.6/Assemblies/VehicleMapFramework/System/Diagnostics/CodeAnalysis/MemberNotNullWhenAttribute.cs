// Decompiled with JetBrains decompiler
// Type: System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using Microsoft.CodeAnalysis;

#nullable enable
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
[ExcludeFromCodeCoverage]
[Embedded]
internal sealed class MemberNotNullWhenAttribute : Attribute
{
  public MemberNotNullWhenAttribute(bool returnValue, string member)
  {
    this.ReturnValue = returnValue;
    this.Members = new string[1]{ member };
  }

  public MemberNotNullWhenAttribute(bool returnValue, params string[] members)
  {
    this.ReturnValue = returnValue;
    this.Members = members;
  }

  public bool ReturnValue { get; }

  public string[] Members { get; }
}
