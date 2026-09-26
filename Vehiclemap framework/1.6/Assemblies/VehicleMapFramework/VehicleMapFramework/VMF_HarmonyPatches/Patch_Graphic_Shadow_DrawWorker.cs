// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Graphic_Shadow_DrawWorker
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Graphic_Shadow), "DrawWorker")]
[PatchLevel(Level.Sensitive)]
public static class Patch_Graphic_Shadow_DrawWorker
{
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    LocalBuilder vehicle;
    Label label;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, generator).AddAltitudeFor(out vehicle, getInstance: new CodeInstruction[1]
    {
      CodeInstruction.LoadArgument(4, false)
    }).InsertAndAdvance(new CodeInstruction[2]
    {
      CodeInstruction.LoadArgument(4, false),
      PatchHelper.get_CallInstruction((Patch_Graphic_Shadow_DrawWorker.\u003C\u003EO.\u003C0\u003E__AASB2ShadowAltitude ?? (Patch_Graphic_Shadow_DrawWorker.\u003C\u003EO.\u003C0\u003E__AASB2ShadowAltitude = new Func<float, Thing, float>(Patch_Graphic_Shadow_DrawWorker.AASB2ShadowAltitude))).Method)
    }).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.g_Rot4_AsQuat)
    }).SetOperandAndAdvance((object) MethodInfoCache.CachedMethodInfo.m_Rot8_AsQuatRef).CreateLabel(ref label).Insert(new CodeInstruction[5]
    {
      new CodeInstruction(OpCodes.Ldloc_S, (object) vehicle),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Ldloc_S, (object) vehicle),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.m_FullAngleQuat),
      new CodeInstruction(OpCodes.Call, (object) MethodInfoCache.CachedMethodInfo.o_Quaternion_Multiply)
    }).InstructionEnumeration();
  }

  private static float AASB2ShadowAltitude(float value, Thing t)
  {
    if (ModCompat.CompatBase<ModCompat.AsAboveSoBelow>.Active)
    {
      Map map = t?.Map;
      VehiclePawnWithMap vehicle;
      if (map != null && map.IsNonFocusedVehicleMapOf(out vehicle))
      {
        MapComponent mapComponent = ModCompat.AsAboveSoBelow.CompOf(map);
        if (mapComponent != null && ModCompat.AsAboveSoBelow.Banded(mapComponent) && ModCompat.AsAboveSoBelow.CurrentBand(map) > ModCompat.AsAboveSoBelow.BandOf(mapComponent, t.Position))
          return Altitudes.AltitudeFor((AltitudeLayer) 2, -0.1f).YOffsetFull(vehicle);
      }
    }
    return value;
  }
}
