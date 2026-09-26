// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_JoyGiver_GetSearchSet
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (JoyGiver), "GetSearchSet")]
[PatchLevel(Level.Safe)]
public static class Patch_JoyGiver_GetSearchSet
{
  private static bool working;
  private static readonly List<Thing> tmpCandidates = new List<Thing>();
  private static readonly Action<JoyGiver, Pawn, List<Thing>> GetSearchSet = AccessTools.MethodDelegate<Action<JoyGiver, Pawn, List<Thing>>>(AccessTools.Method(typeof (JoyGiver), nameof (GetSearchSet), (Type[]) null, (Type[]) null), (object) null, true, (Type[]) null);

  private static bool Prepare()
  {
    VehicleMapSettings settings = VehicleMapFramework.VehicleMapFramework.settings;
    return settings != null && settings.joyPatches;
  }

  public static void Postfix(JoyGiver __instance, Pawn pawn, List<Thing> outCandidates)
  {
    if (Patch_JoyGiver_GetSearchSet.working)
      return;
    Patch_JoyGiver_GetSearchSet.working = true;
    try
    {
      foreach (Map mapAndVehicleMap in ((Thing) pawn).Map.BaseMapAndVehicleMaps(false))
      {
        using (new VirtualTeleporter((Thing) pawn, mapAndVehicleMap))
        {
          Patch_JoyGiver_GetSearchSet.GetSearchSet(__instance, pawn, Patch_JoyGiver_GetSearchSet.tmpCandidates);
          outCandidates.AddRange((IEnumerable<Thing>) Patch_JoyGiver_GetSearchSet.tmpCandidates);
          Patch_JoyGiver_GetSearchSet.tmpCandidates.Clear();
        }
      }
    }
    finally
    {
      Patch_JoyGiver_GetSearchSet.working = false;
    }
  }
}
