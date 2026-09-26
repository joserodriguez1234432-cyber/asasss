// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Graphic_Draw
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Graphic), "Draw")]
[PatchLevel(Level.Safe)]
public static class Patch_Graphic_Draw
{
  public static void Prefix(
    ref Vector3 loc,
    ref Rot4 rot,
    Thing thing,
    ref float extraRotation,
    Graphic __instance)
  {
    // ISSUE: variable of a compiler-generated type
    Patch_Graphic_Draw.\u003C\u003Ec__DisplayClass0_0 cDisplayClass00;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass00.thing = thing;
    VehiclePawnWithMap vehicle;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    if (cDisplayClass00.thing.IsOnNonFocusedVehicleMapOf(out vehicle) && cDisplayClass00.thing.def.drawerType == 1 && cDisplayClass00.thing.def.category != 2)
    {
      // ISSUE: variable of a compiler-generated type
      Patch_Graphic_Draw.\u003C\u003Ec__DisplayClass0_1 cDisplayClass01;
      ref Patch_Graphic_Draw.\u003C\u003Ec__DisplayClass0_1 local1 = ref cDisplayClass01;
      // ISSUE: reference to a compiler-generated field
      if (!cDisplayClass00.thing.def.IsBlueprint)
      {
        // ISSUE: reference to a compiler-generated field
        thingDef = cDisplayClass00.thing.def;
      }
      else
      {
        // ISSUE: reference to a compiler-generated field
        if (!(cDisplayClass00.thing.def.entityDefToBuild is ThingDef thingDef))
        {
          // ISSUE: reference to a compiler-generated field
          thingDef = cDisplayClass00.thing.def;
        }
      }
      // ISSUE: reference to a compiler-generated field
      local1.def = thingDef;
      // ISSUE: reference to a compiler-generated field
      cDisplayClass01.rot2 = rot;
      ref Patch_Graphic_Draw.\u003C\u003Ec__DisplayClass0_1 local2 = ref cDisplayClass01;
      Rot4 rot4 = vehicle.FullRotation.RotForVehicleDraw();
      int asInt = ((Rot4) ref rot4).AsInt;
      // ISSUE: reference to a compiler-generated field
      local2.baseRotInt = asInt;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      if (!(cDisplayClass00.thing is Building_Bookcase) || cDisplayClass00.thing.Graphic == __instance)
      {
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        if (cDisplayClass01.def.size.x == cDisplayClass01.def.size.z && !(cDisplayClass00.thing is Building_SupportedDoor))
        {
          // ISSUE: reference to a compiler-generated field
          GraphicData graphicData1 = cDisplayClass01.def.graphicData;
          if ((graphicData1 != null ? (graphicData1.drawRotated ? 1 : 0) : 0) != 0)
          {
            // ISSUE: reference to a compiler-generated field
            GraphicData graphicData2 = cDisplayClass01.def.graphicData;
            if ((graphicData2 != null ? (!graphicData2.Linked ? 1 : 0) : 1) != 0)
              goto label_10;
          }
          // ISSUE: reference to a compiler-generated field
          if (!cDisplayClass01.def.rotatable)
            goto label_12;
label_10:
          if (Patch_Graphic_Draw.\u003CPrefix\u003Eg__SameMaterialByRot\u007C0_0(ref cDisplayClass00, ref cDisplayClass01))
            goto label_12;
        }
        ref Rot4 local3 = ref rot;
        // ISSUE: reference to a compiler-generated field
        ((Rot4) ref local3).AsInt = ((Rot4) ref local3).AsInt + cDisplayClass01.baseRotInt;
      }
label_12:
      // ISSUE: reference to a compiler-generated field
      if (!cDisplayClass01.def.ShouldRotatedOnVehicle())
        return;
      float num = vehicle.Angle - vehicle.Transform.rotation;
      extraRotation -= num;
      // ISSUE: reference to a compiler-generated field
      Vector3 vector3_1 = cDisplayClass00.thing.Graphic.DrawOffset(rot);
      CompFireOverlay compFireOverlay;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      if (__instance is Graphic_Flicker && !(cDisplayClass00.thing.Graphic is Graphic_Single) && ThingCompUtility.TryGetComp<CompFireOverlay>(cDisplayClass00.thing, ref compFireOverlay))
        vector3_1 = Vector3.op_Addition(vector3_1, compFireOverlay.Props.DrawOffsetForRot(rot));
      Vector3 vector3_2 = Vector3Utility.RotatedBy(vector3_1, -num);
      loc = Vector3.op_Addition(loc, new Vector3(vector3_2.x - vector3_1.x, 0.0f, vector3_2.z - vector3_1.z));
    }
    else
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      if (cDisplayClass00.thing == null || cDisplayClass00.thing.Spawned || !cDisplayClass00.thing.SpawnedParentOrMe.IsOnNonFocusedVehicleMapOf(out vehicle))
        return;
      extraRotation += VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle);
    }
  }
}
