// Decompiled with JetBrains decompiler
// Type: SmashTools.Targeting.TargetValidation
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using Verse;

#nullable disable
namespace SmashTools.Targeting;

public struct TargetValidation
{
  public required bool isValid;

  public TaggedString Tooltip { get; init; }

  public static TargetValidation Success
  {
    get => new TargetValidation() { isValid = true };
  }

  public static TargetValidation Failed
  {
    get => new TargetValidation() { isValid = false };
  }

  public static implicit operator bool(TargetValidation result) => result.isValid;
}
