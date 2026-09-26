// Decompiled with JetBrains decompiler
// Type: SmashTools.ProjectStartup
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using SmashTools.Patching;
using SmashTools.Xml;
using System.Collections;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

[StaticConstructorOnStartup]
public static class ProjectStartup
{
  private const float UnpatchDelay = 3f;

  static ProjectStartup()
  {
    HarmonyPatcher.Run(PatchSequence.PostDefDatabase);
    DelayedCrossRefResolver.ResolveAll();
    ConditionalPatches.DumpPatchReport();
  }

  private static IEnumerator UnpatchAfterSeconds(float seconds)
  {
    yield return (object) new WaitForSeconds(seconds);
    HarmonyPatcher.RunUnpatches();
  }

  private static void DrawDebugWindowButton(WidgetRow ___widgetRow, out float ___widgetRowFinalX)
  {
    ___widgetRow.ButtonIcon(TexButton.OpenDebugActionsMenu, "Open Startup Actions menu.\n\n This lets you initiate certain static methods on startup for quick testing.", new Color?(), new Color?(), new Color?(), true, -1f);
    ___widgetRowFinalX = ___widgetRow.FinalX;
  }
}
