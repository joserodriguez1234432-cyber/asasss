// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_RitualStage_GetPawnPosition
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (RitualStage), "GetPawnPosition")]
[PatchLevel(Level.Safe)]
public static class Patch_RitualStage_GetPawnPosition
{
  public static void Prefix(Pawn pawn, LordJob_Ritual ritual, ref VirtualTeleporter? __state)
  {
    if (((Thing) pawn).Map == ((LordJob) ritual).Map)
      return;
    __state = new VirtualTeleporter?(new VirtualTeleporter((Thing) pawn, ((LordJob) ritual).Map));
  }

  public static void Finalizer(ref VirtualTeleporter? __state)
  {
    ref VirtualTeleporter? local = ref __state;
    if (!local.HasValue)
      return;
    local.GetValueOrDefault().Dispose();
  }
}
