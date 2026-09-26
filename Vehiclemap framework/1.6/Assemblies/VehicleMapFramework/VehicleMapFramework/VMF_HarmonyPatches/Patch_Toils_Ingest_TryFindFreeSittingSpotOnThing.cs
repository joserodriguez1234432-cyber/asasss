// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Toils_Ingest_TryFindFreeSittingSpotOnThing
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Toils_Ingest), "TryFindFreeSittingSpotOnThing")]
[PatchLevel(Level.Safe)]
public static class Patch_Toils_Ingest_TryFindFreeSittingSpotOnThing
{
  public static void Prefix(Thing t, Pawn pawn)
  {
    if (((Thing) pawn).Map == t.Map || t.Map == null)
      return;
    Job curJob = pawn.CurJob;
    if (curJob == null)
      return;
    curJob.globalTarget = GlobalTargetInfo.op_Implicit(t);
  }
}
