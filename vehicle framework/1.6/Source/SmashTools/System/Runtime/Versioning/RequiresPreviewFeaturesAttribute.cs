// Decompiled with JetBrains decompiler
// Type: System.Runtime.Versioning.RequiresPreviewFeaturesAttribute
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Diagnostics.CodeAnalysis;

#nullable enable
namespace System.Runtime.Versioning;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
[ExcludeFromCodeCoverage]
internal sealed class RequiresPreviewFeaturesAttribute : Attribute
{
  public RequiresPreviewFeaturesAttribute()
  {
  }

  public RequiresPreviewFeaturesAttribute(string? message) => this.Message = message;

  public string? Message { get; }

  public string? Url { get; set; }
}
