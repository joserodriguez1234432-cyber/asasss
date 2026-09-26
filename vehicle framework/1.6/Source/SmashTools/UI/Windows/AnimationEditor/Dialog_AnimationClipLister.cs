// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.Dialog_AnimationClipLister
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.IO;
using UnityEngine;

#nullable disable
namespace SmashTools.Animations;

public class Dialog_AnimationClipLister : Dialog_ItemDropdown<AnimationClip>
{
  private readonly IAnimator animator;
  private readonly AnimationClip animation;

  public Dialog_AnimationClipLister(
    IAnimator animator,
    Rect rect,
    AnimationClip animation,
    Dialog_ItemDropdown<AnimationClip>.CreateItemButton createItem = null,
    Action<AnimationClip> onFilePicked = null)
    : base(rect, AnimationLoader.Cache<AnimationClip>.GetAll(), onFilePicked, Dialog_AnimationClipLister.\u003C\u003EO.\u003C0\u003E__FileName ?? (Dialog_AnimationClipLister.\u003C\u003EO.\u003C0\u003E__FileName = new Func<AnimationClip, string>(Dialog_AnimationClipLister.FileName)), (Func<AnimationClip, bool>) (other => (bool) other && (bool) animation && animation.FilePath == other.FilePath), createItem: createItem)
  {
    // ISSUE: reference to a compiler-generated field (out of statement scope)
    // ISSUE: reference to a compiler-generated field (out of statement scope)
    this.animator = animator;
    this.animation = animation;
  }

  private static string FileName(AnimationClip clip)
  {
    return Path.GetFileNameWithoutExtension(clip.FilePath);
  }
}
