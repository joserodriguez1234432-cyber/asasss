// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.Dialog_MethodSelector
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Animations;

public class Dialog_MethodSelector : Dialog_ItemDropdown<MethodInfo>
{
  private readonly IAnimator animator;
  private readonly AnimationEvent animationEvent;
  private static readonly List<MethodInfo> staticMethods = new List<MethodInfo>();

  public Dialog_MethodSelector(
    IAnimator animator,
    Rect rect,
    AnimationEvent animationEvent,
    Action<MethodInfo> onMethodPicked = null)
    : base(rect, Dialog_MethodSelector.EventMethods(animator), onMethodPicked, Dialog_MethodSelector.\u003C\u003EO.\u003C0\u003E__MethodName ?? (Dialog_MethodSelector.\u003C\u003EO.\u003C0\u003E__MethodName = new Func<MethodInfo, string>(Dialog_MethodSelector.MethodName)), (Func<MethodInfo, bool>) (method => animationEvent?.method != null && animationEvent.method.method == method), Dialog_MethodSelector.\u003C\u003EO.\u003C1\u003E__FullMethodSignature ?? (Dialog_MethodSelector.\u003C\u003EO.\u003C1\u003E__FullMethodSignature = new Func<MethodInfo, string>(Dialog_MethodSelector.FullMethodSignature)))
  {
    // ISSUE: reference to a compiler-generated field (out of statement scope)
    // ISSUE: reference to a compiler-generated field (out of statement scope)
    // ISSUE: reference to a compiler-generated field (out of statement scope)
    // ISSUE: reference to a compiler-generated field (out of statement scope)
    this.animator = animator;
    this.animationEvent = animationEvent;
  }

  internal static string MethodName(MethodInfo method)
  {
    return $"{GenTypes.GetTypeNameWithoutIgnoredNamespaces(method.DeclaringType)}.{method.Name}";
  }

  internal static string FullMethodSignature(MethodInfo method)
  {
    string str = Dialog_MethodSelector.MethodName(method);
    ParameterInfo[] parameters = method.GetParameters();
    if (!((IEnumerable<ParameterInfo>) parameters).NullOrEmpty<ParameterInfo>())
      str = $"{str}( {string.Join(", ", ((IEnumerable<ParameterInfo>) parameters).Select<ParameterInfo, string>((Func<ParameterInfo, string>) (parameter => parameter.ParameterType.Name)))} )";
    return str;
  }

  private static List<MethodInfo> EventMethods(IAnimator animator)
  {
    List<MethodInfo> methods = new List<MethodInfo>();
    Dialog_MethodSelector.AddEventMethods((object) animator, methods);
    methods.AddRange((IEnumerable<MethodInfo>) Dialog_MethodSelector.staticMethods);
    return methods;
  }

  private static void AddEventMethods(object obj, List<MethodInfo> methods)
  {
    foreach (MethodInfo method in obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
    {
      if (GenAttribute.HasAttribute<AnimationEventAttribute>((MemberInfo) method))
        methods.Add(method);
    }
  }

  internal static void InitStaticEventMethods()
  {
    if (Dialog_MethodSelector.staticMethods.Count > 0)
      return;
    foreach (Type allType in GenTypes.AllTypes)
    {
      foreach (MethodInfo method in allType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
      {
        AnimationEventAttribute animationEventAttribute;
        if (GenAttribute.TryGetAttribute<AnimationEventAttribute>((MemberInfo) method, ref animationEventAttribute))
          Dialog_MethodSelector.staticMethods.Add(method);
      }
    }
  }
}
