// Decompiled with JetBrains decompiler
// Type: SmashTools.AnimationEvents
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using SmashTools.Animations;

#nullable disable
namespace SmashTools;

internal static class AnimationEvents
{
  [AnimationEvent]
  private static void SetFloat(IAnimator __animator, AnimationParameterDef paramDef, float value)
  {
    Trace.IsTrue(paramDef.type == AnimationParameter.ParamType.Trigger, $"Mismatched AnimationParameterDef type. \r\nMust call method with matching type {paramDef.type}");
    __animator.Manager.SetFloat(paramDef, value);
  }

  [AnimationEvent]
  private static void SetInt(IAnimator __animator, AnimationParameterDef paramDef, int value)
  {
    Trace.IsTrue(paramDef.type == AnimationParameter.ParamType.Trigger, $"Mismatched AnimationParameterDef type. \r\nMust call method with matching type {paramDef.type}");
    __animator.Manager.SetInt(paramDef, value);
  }

  [AnimationEvent]
  private static void SetBool(IAnimator __animator, AnimationParameterDef paramDef, bool value)
  {
    Trace.IsTrue(paramDef.type == AnimationParameter.ParamType.Trigger, $"Mismatched AnimationParameterDef type. \r\nMust call method with matching type {paramDef.type}");
    __animator.Manager.SetBool(paramDef, value);
  }

  [AnimationEvent]
  private static void SetTrigger(IAnimator __animator, AnimationParameterDef paramDef, bool value)
  {
    Trace.IsTrue(paramDef.type == AnimationParameter.ParamType.Trigger, $"Mismatched AnimationParameterDef type. \r\nMust call method with matching type {paramDef.type}");
    __animator.Manager.SetTrigger(paramDef, value);
  }
}
