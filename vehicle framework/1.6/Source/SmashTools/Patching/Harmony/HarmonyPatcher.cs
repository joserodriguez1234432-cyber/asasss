// Decompiled with JetBrains decompiler
// Type: SmashTools.Patching.HarmonyPatcher
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Verse;

#nullable disable
namespace SmashTools.Patching;

[PublicAPI]
public static class HarmonyPatcher
{
  private static Task asyncPatchTask;
  private static Type typePatching;
  private static string methodPatching = string.Empty;
  private static readonly List<HarmonyPatcher.Unpatcher> Unpatchers = new List<HarmonyPatcher.Unpatcher>();
  private static readonly Dictionary<PatchSequence, List<IPatchCategory>> Patches = new Dictionary<PatchSequence, List<IPatchCategory>>();

  private static ModContentPack Mod { get; set; }

  internal static Harmony Harmony { get; private set; }

  private static bool RunningPatcher { get; set; }

  internal static void Init(ModContentPack mod)
  {
    HarmonyPatcher.Mod = mod;
    HarmonyPatcher.Harmony = new Harmony(mod.ModMetaData.PackageIdPlayerFacing);
    ConditionalPatches.Init(mod);
    foreach (Type classImplementation in mod.AllInterfaceClassImplementations<IPatchCategory>())
    {
      IPatchCategory instance = (IPatchCategory) Activator.CreateInstance(classImplementation, true);
      HarmonyPatcher.Patches.AddOrAppend<PatchSequence, List<IPatchCategory>, IPatchCategory>(instance.PatchAt, instance);
    }
  }

  public static void Run(PatchSequence sequence)
  {
    if (!HarmonyPatcher.Patches.ContainsKey(sequence))
      return;
    if (sequence == PatchSequence.Async)
    {
      List<IPatchCategory> enumerable;
      if (!HarmonyPatcher.Patches.TryGetValue(PatchSequence.Async, out enumerable) || enumerable.NullOrEmpty<IPatchCategory>())
        return;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      HarmonyPatcher.asyncPatchTask = Task.Run(HarmonyPatcher.\u003C\u003EO.\u003C0\u003E__RunAsyncPatches ?? (HarmonyPatcher.\u003C\u003EO.\u003C0\u003E__RunAsyncPatches = new Action(HarmonyPatcher.RunAsyncPatches)));
      LongEventHandler.ExecuteWhenFinished((Action) (() =>
      {
        if (HarmonyPatcher.asyncPatchTask.IsCompleted)
          return;
        if (Prefs.DevMode)
          Log.Warning($"[{HarmonyPatcher.Mod.Name}] Patching took longer than expected, delaying startup until it's finished.");
        HarmonyPatcher.asyncPatchTask.GetAwaiter().GetResult();
      }));
    }
    else
    {
      using (new HarmonyPatcher.PatchStatusEnabler())
      {
        DeepProfiler.Start($"ConditionalPatches_{sequence}");
        ConditionalPatches.Run(sequence);
        DeepProfiler.End();
        DeepProfiler.Start($"HarmonyPatcher_{sequence}");
        foreach (IPatchCategory patchCategory in HarmonyPatcher.Patches[sequence])
        {
          try
          {
            if (patchCategory.PatchAt == sequence)
            {
              HarmonyPatcher.typePatching = patchCategory.GetType();
              DeepProfiler.Start(HarmonyPatcher.typePatching.Name);
              patchCategory.PatchMethods();
              DeepProfiler.End();
            }
          }
          catch (Exception ex)
          {
            Log.Error($"Failed to Patch {patchCategory.GetType().FullName}. Method=\"{HarmonyPatcher.methodPatching}\"\n{ex}");
          }
        }
        DeepProfiler.End();
      }
    }
  }

  private static void RunAsyncPatches()
  {
    ConditionalPatches.Run(PatchSequence.Async);
    foreach (IPatchCategory patchCategory in HarmonyPatcher.Patches[PatchSequence.Async])
    {
      try
      {
        patchCategory.PatchMethods();
      }
      catch (Exception ex)
      {
        Log.Error($"Failed to Patch {patchCategory.GetType().FullName}.\n{ex}");
      }
    }
  }

  public static void Patch(
    MethodBase original,
    HarmonyMethod prefix = null,
    HarmonyMethod postfix = null,
    HarmonyMethod transpiler = null,
    HarmonyMethod finalizer = null)
  {
    HarmonyPatcher.methodPatching = original?.Name ?? "Null\", Previous=\"" + HarmonyPatcher.methodPatching;
    try
    {
      HarmonyPatcher.Harmony.Patch(original, prefix, postfix, transpiler, finalizer);
    }
    catch (Exception ex)
    {
      Log.Error($"Exception thrown patching {HarmonyPatcher.typePatching}::{HarmonyPatcher.methodPatching}.\n{ex}");
    }
  }

  public static void Unpatch(MethodBase original, HarmonyPatchType patchType, string harmonyId = "")
  {
    HarmonyPatcher.Unpatchers.Add(new HarmonyPatcher.Unpatcher(original, patchType, harmonyId));
  }

  public static void RunUnpatches()
  {
    foreach (HarmonyPatcher.Unpatcher unpatcher in HarmonyPatcher.Unpatchers)
      unpatcher.Execute();
    HarmonyPatcher.Unpatchers.Clear();
  }

  [Conditional("DEBUG")]
  internal static void DumpPatchReport()
  {
    if (!Prefs.DevMode)
      return;
    int num1 = 0;
    int num2 = 0;
    int num3 = 0;
    int num4 = 0;
    foreach (MethodBase methodBase in HarmonyPatcher.Harmony.GetPatchedMethods().ToList<MethodBase>())
    {
      HarmonyLib.Patches patchInfo = Harmony.GetPatchInfo(methodBase);
      num1 += CountPatches((IReadOnlyCollection<HarmonyLib.Patch>) patchInfo.Prefixes);
      num2 += CountPatches((IReadOnlyCollection<HarmonyLib.Patch>) patchInfo.Postfixes);
      num3 += CountPatches((IReadOnlyCollection<HarmonyLib.Patch>) patchInfo.Transpilers);
      num4 += CountPatches((IReadOnlyCollection<HarmonyLib.Patch>) patchInfo.Finalizers);
    }
    SmashLog.Message($"<color=orange>[{HarmonyPatcher.Mod.Name.Replace(" ", "")}]</color> <success>{num1 + num2 + num3 + num4} " + $"patches successfully applied.</success>\nPrefixes: {num1} Postfixes: {num2} Transpilers: {num3} Finalizers: {num4}");

    static int CountPatches(IReadOnlyCollection<HarmonyLib.Patch> patches)
    {
      int num = 0;
      foreach (HarmonyLib.Patch patch in (IEnumerable<HarmonyLib.Patch>) patches)
      {
        if (patch.owner == HarmonyPatcher.Harmony.Id)
          ++num;
      }
      return num;
    }
  }

  private readonly struct Unpatcher(
    MethodBase original,
    HarmonyPatchType patchType,
    string harmonyId = "")
  {
    public void Execute() => HarmonyPatcher.Harmony.Unpatch(original, patchType, harmonyId);

    public override string ToString() => $"{patchType}::{original.Name}";
  }

  [StructLayout(LayoutKind.Sequential, Size = 1)]
  private readonly struct PatchStatusEnabler : IDisposable
  {
    public PatchStatusEnabler() => HarmonyPatcher.RunningPatcher = true;

    void IDisposable.Dispose()
    {
      HarmonyPatcher.RunningPatcher = false;
      HarmonyPatcher.typePatching = (Type) null;
      HarmonyPatcher.methodPatching = (string) null;
    }
  }
}
