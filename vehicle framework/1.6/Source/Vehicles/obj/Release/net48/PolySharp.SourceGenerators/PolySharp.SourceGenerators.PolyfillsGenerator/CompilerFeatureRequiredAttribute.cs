// Decompiled with JetBrains decompiler
// Type: System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Diagnostics.CodeAnalysis;

#nullable enable
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
[ExcludeFromCodeCoverage]
internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
  public const string RefStructs = "RefStructs";
  public const string RequiredMembers = "RequiredMembers";

  public CompilerFeatureRequiredAttribute(string featureName) => this.FeatureName = featureName;

  public string FeatureName { get; }

  public bool IsOptional { get; set; }
}
