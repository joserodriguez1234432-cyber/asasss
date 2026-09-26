// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_ItemAvailability_ThingsAvailableAnywhere
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

[HarmonyPatch(typeof (ItemAvailability), "ThingsAvailableAnywhere")]
[PatchLevel(Level.Sensitive)]
public static class Patch_ItemAvailability_ThingsAvailableAnywhere
{
  private static readonly List<Thing> tmpList = new List<Thing>();

  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => c.opcode == OpCodes.Stloc_2));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[4]
    {
      CodeInstruction.LoadArgument(0, false),
      CodeInstruction.LoadField(typeof (ItemAvailability), "map", false),
      CodeInstruction.LoadArgument(1, false),
      PatchHelper.get_CallInstruction((Patch_ItemAvailability_ThingsAvailableAnywhere.\u003C\u003EO.\u003C0\u003E__AddThingList ?? (Patch_ItemAvailability_ThingsAvailableAnywhere.\u003C\u003EO.\u003C0\u003E__AddThingList = new Func<List<Thing>, Map, ThingDef, List<Thing>>(Patch_ItemAvailability_ThingsAvailableAnywhere.AddThingList))).Method)
    }));
    return (IEnumerable<CodeInstruction>) list;
  }

  public static List<Thing> AddThingList(List<Thing> list, Map map, ThingDef need)
  {
    if (!VehicleMapUtility.get_CrossMapContext(map))
      return list;
    Patch_ItemAvailability_ThingsAvailableAnywhere.tmpList.Clear();
    for (int index = 0; index < list.Count; ++index)
      Patch_ItemAvailability_ThingsAvailableAnywhere.tmpList.Add(list[index]);
    foreach (Map mapAndVehicleMap in map.BaseMapAndVehicleMaps(false))
    {
      List<Thing> thingList = mapAndVehicleMap.listerThings.ThingsOfDef(need);
      for (int index = 0; index < thingList.Count; ++index)
        Patch_ItemAvailability_ThingsAvailableAnywhere.tmpList.Add(thingList[index]);
    }
    return Patch_ItemAvailability_ThingsAvailableAnywhere.tmpList;
  }
}
