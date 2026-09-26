// Decompiled with JetBrains decompiler
// Type: System.Diagnostics.CodeAnalysis.StringSyntaxAttribute
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using Microsoft.CodeAnalysis;

#nullable enable
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
[ExcludeFromCodeCoverage]
[Embedded]
internal sealed class StringSyntaxAttribute : Attribute
{
  public const string CompositeFormat = "CompositeFormat";
  public const string DateOnlyFormat = "DateOnlyFormat";
  public const string DateTimeFormat = "DateTimeFormat";
  public const string EnumFormat = "EnumFormat";
  public const string GuidFormat = "GuidFormat";
  public const string Json = "Json";
  public const string NumericFormat = "NumericFormat";
  public const string Regex = "Regex";
  public const string TimeOnlyFormat = "TimeOnlyFormat";
  public const string TimeSpanFormat = "TimeSpanFormat";
  public const string Uri = "Uri";
  public const string Xml = "Xml";

  public StringSyntaxAttribute(string syntax)
  {
    this.Syntax = syntax;
    this.Arguments = new object[0];
  }

  public StringSyntaxAttribute(string syntax, params object?[] arguments)
  {
    this.Syntax = syntax;
    this.Arguments = arguments;
  }

  public string Syntax { get; }

  public object?[] Arguments { get; }
}
