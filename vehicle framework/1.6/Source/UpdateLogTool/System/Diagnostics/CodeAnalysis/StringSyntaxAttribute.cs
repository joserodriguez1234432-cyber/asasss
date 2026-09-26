// Decompiled with JetBrains decompiler
// Type: System.Diagnostics.CodeAnalysis.StringSyntaxAttribute
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

#nullable enable
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
[ExcludeFromCodeCoverage]
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
