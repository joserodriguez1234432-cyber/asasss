// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Graphic_DrawFromDef
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[HarmonyPatch(typeof (Graphic), "DrawFromDef")]
[PatchLevel(Level.Safe)]
public static class Patch_Graphic_DrawFromDef
{
  public static void Prefix(
    ref Vector3 loc,
    ref Rot4 rot,
    ThingDef thingDef,
    ref float extraRotation,
    Graphic __instance)
  {
    // ISSUE: variable of a compiler-generated type
    Patch_Graphic_DrawFromDef.\u003C\u003Ec__DisplayClass0_0 cDisplayClass00;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass00.__instance = __instance;
    VehiclePawnWithMap vehicle;
    if (!VehicleMapUtility.FocusedOnVehicleMap(out vehicle) || thingDef == null)
      return;
    if (!thingDef.IsBlueprint)
      thingDef1 = thingDef;
    else if (!(thingDef.entityDefToBuild is ThingDef thingDef1))
      thingDef1 = thingDef;
    ThingDef tDef = thingDef1;
    CompProperties_FireOverlay compProperties = tDef.GetCompProperties<CompProperties_FireOverlay>();
    // ISSUE: reference to a compiler-generated field
    int num1 = !(cDisplayClass00.__instance is Graphic_Flicker) ? 0 : (compProperties != null ? 1 : 0);
    if (num1 != 0)
    {
      ref Vector3 local = ref loc;
      Vector3 vector3_1 = loc;
      GraphicData graphicData = tDef.graphicData;
      Vector3 vector3_2 = Vector3.op_Addition(graphicData != null ? graphicData.DrawOffsetForRot(rot) : Vector3.zero, compProperties.DrawOffsetForRot(rot));
      Vector3 vector3_3 = Vector3.op_Subtraction(vector3_1, vector3_2);
      local = vector3_3;
    }
    float num2 = vehicle.Angle - vehicle.Transform.rotation;
    // ISSUE: variable of a compiler-generated type
    Patch_Graphic_DrawFromDef.\u003C\u003Ec__DisplayClass0_1 cDisplayClass01;
    // ISSUE: reference to a compiler-generated field
    cDisplayClass01.rot2 = rot;
    ref Patch_Graphic_DrawFromDef.\u003C\u003Ec__DisplayClass0_1 local1 = ref cDisplayClass01;
    Rot4 rot4 = vehicle.FullRotation.RotForVehicleDraw();
    int asInt = ((Rot4) ref rot4).AsInt;
    // ISSUE: reference to a compiler-generated field
    local1.baseRotInt = asInt;
    if (tDef.size.x == tDef.size.z)
    {
      // ISSUE: reference to a compiler-generated field
      GraphicData data1 = cDisplayClass00.__instance.data;
      if ((data1 != null ? (data1.drawRotated ? 1 : 0) : 0) != 0)
      {
        // ISSUE: reference to a compiler-generated field
        GraphicData data2 = cDisplayClass00.__instance.data;
        if ((data2 != null ? (!data2.Linked ? 1 : 0) : 1) != 0)
          goto label_11;
      }
      if (!tDef.rotatable)
        goto label_13;
label_11:
      if (Patch_Graphic_DrawFromDef.\u003CPrefix\u003Eg__SameMaterialByRot\u007C0_0(ref cDisplayClass00, ref cDisplayClass01))
        goto label_13;
    }
    ref Rot4 local2 = ref rot;
    // ISSUE: reference to a compiler-generated field
    ((Rot4) ref local2).AsInt = ((Rot4) ref local2).AsInt + cDisplayClass01.baseRotInt;
label_13:
    bool flag = tDef.ShouldRotatedOnVehicle();
    if (flag)
      extraRotation -= num2;
    // ISSUE: reference to a compiler-generated field
    GraphicData data = cDisplayClass00.__instance.data;
    Vector3 vector3_4 = data != null ? data.DrawOffsetForRot(rot) : Vector3.zero;
    if (num1 != 0)
    {
      Vector3 vector3_5 = compProperties.DrawOffsetForRot(rot);
      loc = Vector3.op_Addition(loc, Vector3Utility.RotatedBy(Vector3.op_Addition(vector3_4, vector3_5), flag ? -num2 : 0.0f));
    }
    else
    {
      Vector3 vector3_6 = Vector3Utility.RotatedBy(vector3_4, flag ? -num2 : 0.0f);
      loc = Vector3.op_Addition(loc, new Vector3(vector3_6.x - vector3_4.x, 0.0f, vector3_6.z - vector3_4.z));
    }
    VehicleMapProps modExtension;
    if (!thingDef.HasComp(typeof (CompVehicleEnterSpot)) || (modExtension = ((Def) vehicle.VehicleDef).GetModExtension<VehicleMapProps>()) == null)
      return;
    // ISSUE: reference to a compiler-generated field
    Rot8 rot8 = new Rot8(cDisplayClass01.rot2).Rotated(vehicle.FullRotation);
    ref Vector3 local3 = ref loc;
    Vector3 vector3_7 = loc;
    Rot8 opposite = ((Rot8) ref rot8).Opposite;
    // ISSUE: reference to a compiler-generated field
    Vector3 vector3_8 = Vector3.op_Multiply(Vector2Utility.ToVector3(((Rot8) ref opposite).AsVector2), modExtension.EdgeSpaceValue(vehicle.FullRotation, ((Rot4) ref cDisplayClass01.rot2).Opposite));
    Vector3 vector3_9 = Vector3.op_Addition(vector3_7, vector3_8);
    local3 = vector3_9;
  }
}
