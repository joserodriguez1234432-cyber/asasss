// Decompiled with JetBrains decompiler
// Type: SmashTools.Patching.Patch_Components
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using System;
using System.Reflection;
using Verse;

#nullable disable
namespace SmashTools.Patching;

internal class Patch_Components : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Game), "AddMap", (Type[]) null, (Type[]) null), postfix: new HarmonyMethod(typeof (ComponentCache), "PreCache", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Map), "FinalizeLoading", (Type[]) null, (Type[]) null), new HarmonyMethod(typeof (ComponentCache), "PreCacheInst", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MapDeiniter), "Deinit", (Type[]) null, (Type[]) null), postfix: new HarmonyMethod(typeof (ComponentCache), "ClearMap", new Type[1]
    {
      typeof (Map)
    }));
  }
}
