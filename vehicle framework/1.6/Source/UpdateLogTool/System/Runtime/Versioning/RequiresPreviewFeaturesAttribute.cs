// Decompiled with JetBrains decompiler
// Type: System.Runtime.Versioning.RequiresPreviewFeaturesAttribute
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

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
