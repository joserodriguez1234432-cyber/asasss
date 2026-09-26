// Decompiled with JetBrains decompiler
// Type: System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

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
