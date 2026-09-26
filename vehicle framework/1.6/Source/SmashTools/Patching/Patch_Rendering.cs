// Decompiled with JetBrains decompiler
// Type: SmashTools.Patching.Patch_Rendering
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Patching;

internal class Patch_Rendering : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (PawnRenderer), "GetBodyPos", (Type[]) null, (Type[]) null), transpiler: new HarmonyMethod(typeof (PawnOverlayRenderer), "ShowBodyTranspiler", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (PawnRenderer), "LayingFacing", (Type[]) null, (Type[]) null), new HarmonyMethod(typeof (PawnOverlayRenderer), "LayingFacing", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WindowStack), "Add", (Type[]) null, (Type[]) null), postfix: new HarmonyMethod(typeof (WindowEvents), "WindowAddedToStack", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WindowStack), "TryRemove", new Type[2]
    {
      typeof (Window),
      typeof (bool)
    }, (Type[]) null), postfix: new HarmonyMethod(typeof (WindowEvents), "WindowRemovedFromStack", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (WindowStack), "HandleEventsHighPriority", (Type[]) null, (Type[]) null), postfix: new HarmonyMethod(typeof (WindowEvents), "HighPriorityOnGUI", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MainTabWindow_Inspect), "DoInspectPaneButtons", (Type[]) null, (Type[]) null), new HarmonyMethod(AccessTools.Method(typeof (Patch_Rendering), "InspectablePaneButtons", (Type[]) null, (Type[]) null), 800, (string[]) null, (string[]) null, new bool?()));
  }

  private static bool InspectablePaneButtons(Rect rect, ref float lineEndWidth)
  {
    if (!(Find.Selector.SingleSelectedThing is IInspectable singleSelectedThing))
      return true;
    lineEndWidth += 30f;
    Widgets.InfoCardButton(((Rect) ref rect).width - lineEndWidth, 0.0f, Find.Selector.SingleSelectedThing);
    lineEndWidth += singleSelectedThing.DoInspectPaneButtons(((Rect) ref rect).width - lineEndWidth);
    return false;
  }
}
