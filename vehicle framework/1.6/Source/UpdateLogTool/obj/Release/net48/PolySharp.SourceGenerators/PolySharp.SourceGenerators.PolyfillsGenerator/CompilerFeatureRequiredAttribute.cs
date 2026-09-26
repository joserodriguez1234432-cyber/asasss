// Decompiled with JetBrains decompiler
// Type: System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

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
