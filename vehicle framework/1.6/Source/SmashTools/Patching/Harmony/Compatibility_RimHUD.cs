// Decompiled with JetBrains decompiler
// Type: SmashTools.Patching.Compatibility_RimHUD
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

internal class Compatibility_RimHUD : IConditionalPatch
{
  string IConditionalPatch.PackageId => "Jaxe.RimHUD";

  string IConditionalPatch.SourceId => "SmashPhil.SmashTools";

  PatchSequence IConditionalPatch.PatchAt => PatchSequence.Async;

  void IConditionalPatch.PatchAll(ModMetaData mod)
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(AccessTools.TypeByName("RimHUD.Interface.Screen.InspectPaneButtons"), "Draw", (Type[]) null, (Type[]) null), postfix: new HarmonyMethod(typeof (Compatibility_RimHUD), "DrawButtonsOnRimHUD", (Type[]) null));
  }

  public static void DrawButtonsOnRimHUD(Rect bounds, IInspectPane pane, ref float offset)
  {
    if (!(Find.Selector.SingleSelectedThing is IInspectable singleSelectedThing))
      return;
    offset += singleSelectedThing.DoInspectPaneButtons(((Rect) ref bounds).width - offset);
  }
}
