// Decompiled with JetBrains decompiler
// Type: UpdateLogTool.Patches
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using HarmonyLib;
using SmashTools.Patching;
using SmashTools.Performance;
using System;
using System.Reflection;
using Verse;

#nullable disable
namespace UpdateLogTool;

internal class Patches : IPatchCategory
{
  private const int DelayUpdateLog = 500;
  private const string CharacterEditor = "void.charactereditor";

  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (UIRoot_Entry), "Init", (Type[]) null, (Type[]) null), postfix: new HarmonyMethod(AccessTools.Method(typeof (Patches), "UpdateOnStartup", (Type[]) null, (Type[]) null), 0, (string[]) null, (string[]) null, new bool?()));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GameComponentUtility), "FinalizeInit", (Type[]) null, (Type[]) null), postfix: new HarmonyMethod(typeof (Patches), "UpdateOnGameInit", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GameComponentUtility), "StartedNewGame", (Type[]) null, (Type[]) null), postfix: new HarmonyMethod(typeof (Patches), "UpdateOnNewGame", (Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GameComponentUtility), "LoadedGame", (Type[]) null, (Type[]) null), postfix: new HarmonyMethod(typeof (Patches), "UpdateOnLoadedGame", (Type[]) null));
  }

  public static void UpdateOnStartup()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() => new Debouncer((Action) (() => UpdateHandler.CheckUpdates(UpdateFor.Startup)), 500).Invoke()));
  }

  public static void UpdateOnGameInit()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() => new Debouncer((Action) (() => UpdateHandler.CheckUpdates(UpdateFor.GameInit)), 500).Invoke()));
  }

  public static void UpdateOnNewGame()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() => new Debouncer((Action) (() => UpdateHandler.CheckUpdates(UpdateFor.NewGame)), 500).Invoke()));
  }

  public static void UpdateOnLoadedGame()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() => new Debouncer((Action) (() => UpdateHandler.CheckUpdates(UpdateFor.LoadedGame)), 500).Invoke()));
  }
}
