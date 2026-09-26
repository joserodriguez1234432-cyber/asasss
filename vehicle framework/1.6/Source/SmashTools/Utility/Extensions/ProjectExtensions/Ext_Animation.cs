// Decompiled with JetBrains decompiler
// Type: SmashTools.Ext_Animation
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

#nullable disable
namespace SmashTools;

public static class Ext_Animation
{
  public static bool AnimationLocked(this IAnimationTarget animationTarget)
  {
    return AnimationSimulator.AnimationTarget == animationTarget && AnimationSimulator.CurrentDriver != null;
  }
}
