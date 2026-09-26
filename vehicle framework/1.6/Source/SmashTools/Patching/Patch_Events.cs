// Decompiled with JetBrains decompiler
// Type: SmashTools.Patching.Patch_Events
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using Verse;
using Verse.Profile;

#nullable disable
namespace SmashTools.Patching;

internal class Patch_Events : IPatchCategory
{
  PatchSequence IPatchCategory.PatchAt => PatchSequence.Mod;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (DefGenerator), "GenerateImpliedDefs_PreResolve", (Type[]) null, (Type[]) null), new HarmonyMethod(typeof (GameEvent), "RaiseOnGenerateImpliedDefs", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Game), "Dispose", (Type[]) null, (Type[]) null), new HarmonyMethod(typeof (GameEvent), "RaiseOnGameDisposing", (Type[]) null), new HarmonyMethod(typeof (GameEvent), "RaiseOnGameDisposed", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (MemoryUtility), "ClearAllMapsAndWorld", (Type[]) null, (Type[]) null), new HarmonyMethod(typeof (GameEvent), "RaiseOnWorldUnloading", (Type[]) null), new HarmonyMethod(typeof (GameEvent), "RaiseOnWorldRemoved", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GameComponentUtility), "StartedNewGame", (Type[]) null, (Type[]) null), postfix: new HarmonyMethod(typeof (GameEvent), "RaiseOnNewGame", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GameComponentUtility), "LoadedGame", (Type[]) null, (Type[]) null), postfix: new HarmonyMethod(typeof (GameEvent), "RaiseOnLoadGame", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (UIRoot_Entry), "Init", (Type[]) null, (Type[]) null), postfix: new HarmonyMethod(typeof (GameEvent), "RaiseOnMainMenu", (Type[]) null));
  }
}
