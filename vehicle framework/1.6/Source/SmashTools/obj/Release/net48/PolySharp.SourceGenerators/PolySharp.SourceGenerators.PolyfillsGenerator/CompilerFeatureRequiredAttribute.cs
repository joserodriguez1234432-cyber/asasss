// Decompiled with JetBrains decompiler
// Type: System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

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
