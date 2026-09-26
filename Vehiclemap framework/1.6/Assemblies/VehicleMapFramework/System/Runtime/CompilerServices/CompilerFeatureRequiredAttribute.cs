// Decompiled with JetBrains decompiler
// Type: System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

#nullable enable
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
[ExcludeFromCodeCoverage]
[Embedded]
internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
  public const string RefStructs = "RefStructs";
  public const string RequiredMembers = "RequiredMembers";

  public CompilerFeatureRequiredAttribute(string featureName) => this.FeatureName = featureName;

  public string FeatureName { get; }

  public bool IsOptional { get; set; }
}
