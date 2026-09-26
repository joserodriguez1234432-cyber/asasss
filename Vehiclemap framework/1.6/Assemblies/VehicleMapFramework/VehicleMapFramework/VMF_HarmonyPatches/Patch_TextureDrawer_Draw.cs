// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_TextureDrawer_Draw
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[VFVersionalPatch]
[HarmonyPatchCategory("VMF_Patches_VehicleFramework")]
[HarmonyPatch(typeof (TextureDrawer), "Draw")]
[PatchLevel(Level.Sensitive)]
public static class Patch_TextureDrawer_Draw
{
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return new CodeMatcher(instructions, (ILGenerator) null).MatchStartForward(new CodeMatch[1]
    {
      CodeMatch.Calls(AccessTools.Method(typeof (UIElements), "DrawTextureWithMaterialOnGUI", (Type[]) null, (Type[]) null))
    }).InsertAfter(new CodeInstruction[2]
    {
      CodeInstruction.LoadLocal(3, false),
      PatchHelper.get_CallInstruction((Patch_TextureDrawer_Draw.\u003C\u003EO.\u003C0\u003E__TryRenderVehicleMap ?? (Patch_TextureDrawer_Draw.\u003C\u003EO.\u003C0\u003E__TryRenderVehicleMap = new Action<Rect>(Patch_TextureDrawer_Draw.TryRenderVehicleMap))).Method)
    }).InstructionEnumeration();
  }

  public static void TryRenderVehicleMap(Rect drawRect)
  {
    ref VehiclePawnWithMap local = ref Patch_TransferableVehicleWidget_DrawCard.vehicle;
    if (local == null)
      return;
    Vector2? drawSize = new Vector2?();
    Vector3? drawOffset = new Vector3?();
    if (!((Def) ((Thing) local).def).HasModExtension<VehicleMapProps_Gravship>())
    {
      drawSize = new Vector2?(((Thing) local).DrawSize);
      drawOffset = new Vector3?(VehicleMapUtility.OffsetFor(local, Rot8.op_Implicit(Rot4.East)));
    }
    Texture vehicleMapTexture = VehicleMapUIRenderer.GetVehicleMapTexture(local, Rot4.East, (256 /*0x0100*/, 256 /*0x0100*/), drawSize, drawOffset);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(0.0f, 0.0f, 150f, 150f);
    ((Rect) ref rect).center = ((Rect) ref drawRect).center;
    Widgets.DrawTextureFitted(rect, vehicleMapTexture, 1f, 1f);
    local = (VehiclePawnWithMap) null;
  }
}
