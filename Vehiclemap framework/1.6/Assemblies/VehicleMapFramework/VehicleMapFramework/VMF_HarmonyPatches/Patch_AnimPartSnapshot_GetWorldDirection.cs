// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_AnimPartSnapshot_GetWorldDirection
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatchCategory("VMF_Patches_MeleeAnimation")]
[HarmonyPatch]
[PatchLevel(Level.Safe)]
public static class Patch_AnimPartSnapshot_GetWorldDirection
{
  public static void Postfix(object ___Renderer, ref Rot4 __result)
  {
    VehiclePawnWithMap vehicle;
    if (!ModCompat.MeleeAnimation.AnimRenderer_Map.Invoke(___Renderer).IsNonFocusedVehicleMapOf(out vehicle))
      return;
    ref Rot4 local = ref __result;
    int asInt1 = ((Rot4) ref local).AsInt;
    Rot4 rotation = ((Thing) vehicle).Rotation;
    int asInt2 = ((Rot4) ref rotation).AsInt;
    ((Rot4) ref local).AsInt = asInt1 + asInt2;
  }
}
