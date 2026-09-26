// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ColonistBar_CheckRecacheEntries
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (ColonistBar), "CheckRecacheEntries")]
[PatchLevel(Level.Sensitive)]
public static class Patch_ColonistBar_CheckRecacheEntries
{
  private static readonly AccessTools.FieldRef<MapPawns, Map> map = AccessTools.FieldRefAccess<MapPawns, Map>(nameof (map));
  private static readonly List<Pawn> tmpList = new List<Pawn>();

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(AccessTools.PropertyGetter(typeof (Find), "Maps"))
    }).InsertAfterAndAdvance(new CodeInstruction[1]
    {
      PatchHelper.get_CallInstruction((Patch_ColonistBar_CheckRecacheEntries.\u003C\u003EO.\u003C0\u003E__ExcludeVehicleMaps ?? (Patch_ColonistBar_CheckRecacheEntries.\u003C\u003EO.\u003C0\u003E__ExcludeVehicleMaps = new Func<IEnumerable<Map>, IEnumerable<Map>>(Patch_ColonistBar_CheckRecacheEntries.ExcludeVehicleMaps))).Method)
    }).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(AccessTools.PropertyGetter(typeof (MapPawns), "FreeColonists"))
    }).Set(OpCodes.Call, (object) (Patch_ColonistBar_CheckRecacheEntries.\u003C\u003EO.\u003C1\u003E__FreeColonists ?? (Patch_ColonistBar_CheckRecacheEntries.\u003C\u003EO.\u003C1\u003E__FreeColonists = new Func<MapPawns, List<Pawn>>(Patch_ColonistBar_CheckRecacheEntries.FreeColonists))).Method).InstructionEnumeration();
  }

  private static IEnumerable<Map> ExcludeVehicleMaps(this IEnumerable<Map> maps)
  {
    VehiclePawnWithMap vehicle;
    return maps == null ? (IEnumerable<Map>) null : maps.Where<Map>((Func<Map, bool>) (m => !m.IsVehicleMapOf(out vehicle) || !((Thing) vehicle).Spawned || m != vehicle.VehicleMap));
  }

  private static List<Pawn> FreeColonists(MapPawns instance)
  {
    List<Pawn> freeColonists = instance.FreeColonists;
    Map groundMap = VehicleMapUtility.get_GroundMap(Patch_ColonistBar_CheckRecacheEntries.map.Invoke(instance));
    for (int index = freeColonists.Count - 1; index >= 0; --index)
    {
      if (((Thing) freeColonists[index]).MapHeldBaseMap() != groundMap)
        freeColonists.RemoveAt(index);
    }
    return freeColonists;
  }
}
