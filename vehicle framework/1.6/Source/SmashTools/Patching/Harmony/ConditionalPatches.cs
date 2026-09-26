// Decompiled with JetBrains decompiler
// Type: SmashTools.Patching.ConditionalPatches
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

#nullable disable
namespace SmashTools.Patching;

[PublicAPI]
public static class ConditionalPatches
{
  private static readonly Dictionary<string, List<IConditionalPatch.Result>> PatchResults = new Dictionary<string, List<IConditionalPatch.Result>>();
  private static readonly Dictionary<PatchSequence, List<IConditionalPatch>> Patches = new Dictionary<PatchSequence, List<IConditionalPatch>>();

  internal static void Init(ModContentPack mod)
  {
    foreach (Type classImplementation in mod.AllInterfaceClassImplementations<IConditionalPatch>())
    {
      IConditionalPatch instance = (IConditionalPatch) Activator.CreateInstance(classImplementation, (object[]) null);
      ConditionalPatches.Patches.AddOrAppend<PatchSequence, List<IConditionalPatch>, IConditionalPatch>(instance.PatchAt, instance);
    }
  }

  internal static void Run(PatchSequence sequence)
  {
    List<IConditionalPatch> conditionalPatchList;
    if (!ConditionalPatches.Patches.TryGetValue(sequence, out conditionalPatchList))
      return;
    foreach (IConditionalPatch conditionalPatch in conditionalPatchList)
    {
      IConditionalPatch.Result result = new IConditionalPatch.Result()
      {
        PackageId = conditionalPatch.PackageId
      };
      ModMetaData activeMod = Ext_Mods.GetActiveMod(conditionalPatch.PackageId);
      if (activeMod != null)
      {
        ConditionalPatches.PatchResults.AddOrAppend<string, List<IConditionalPatch.Result>, IConditionalPatch.Result>(conditionalPatch.SourceId, result);
        try
        {
          result.FriendlyName = activeMod.Name;
          conditionalPatch.PatchAll(activeMod);
          result.Active = true;
        }
        catch (Exception ex)
        {
          Log.Error($"{"[SmashTools]"} Failed to apply compatibility patch {result.FriendlyName}.\n{ex}");
          result.Active = false;
          result.ExceptionThrown = ex;
        }
      }
    }
  }

  public static List<IConditionalPatch.Result> GetPatches(string sourceId)
  {
    return GenCollection.TryGetValue<string, List<IConditionalPatch.Result>>((IReadOnlyDictionary<string, List<IConditionalPatch.Result>>) ConditionalPatches.PatchResults, sourceId, (List<IConditionalPatch.Result>) null);
  }

  public static void DumpPatchReport()
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach (KeyValuePair<string, List<IConditionalPatch.Result>> patchResult in ConditionalPatches.PatchResults)
    {
      string str1;
      List<IConditionalPatch.Result> resultList;
      patchResult.Deconstruct(ref str1, ref resultList);
      string str2 = str1;
      List<IConditionalPatch.Result> enumerable = resultList;
      if (!enumerable.NullOrEmpty<IConditionalPatch.Result>())
      {
        foreach (IConditionalPatch.Result result in enumerable)
          stringBuilder.AppendLine($"[{str2}] Applying compatibility patch for {result.PackageId}. Active: {GenText.ToStringYesNo(result.Active)}");
      }
    }
    if (stringBuilder.Length <= 0)
      return;
    Log.Message(stringBuilder.ToString().TrimEnd());
  }
}
