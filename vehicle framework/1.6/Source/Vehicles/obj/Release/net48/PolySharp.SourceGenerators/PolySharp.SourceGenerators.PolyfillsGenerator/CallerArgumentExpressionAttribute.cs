// Decompiled with JetBrains decompiler
// Type: System.Runtime.CompilerServices.CallerArgumentExpressionAttribute
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Diagnostics.CodeAnalysis;

#nullable enable
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
[ExcludeFromCodeCoverage]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
  public CallerArgumentExpressionAttribute(string parameterName)
  {
    this.ParameterName = parameterName;
  }

  public string ParameterName { get; }
}
