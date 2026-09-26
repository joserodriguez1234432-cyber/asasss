// Decompiled with JetBrains decompiler
// Type: System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

#nullable enable
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
[ExcludeFromCodeCoverage]
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
