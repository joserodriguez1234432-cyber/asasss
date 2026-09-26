// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Command_SelectVehicleMap
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using SmashTools;
using UnityEngine;
using VehicleMapFramework.VMF_HarmonyPatches;
using Vehicles;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Command_SelectVehicleMap(VehiclePawnWithMap vehicle) : Command_ToggleWithIcon
{
  public VehiclePortrait portrait;

  public static bool Available { get; } = new VFVersionalPatchAttribute("1.6.2380", (ComparisonType) 4).Available;

  public virtual void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
  {
    if (!Command_SelectVehicleMap.Available)
    {
      ((Command) this).icon = VehicleMapUIRenderer.GetVehicleMapTexture(vehicle, Rot4.East, (256 /*0x0100*/, 256 /*0x0100*/));
      ((Command) this).DrawIcon(rect, buttonMat, parms);
    }
    else
    {
      float num1 = Mathf.Min(((Rect) ref rect).width, ((Rect) ref rect).height);
      Rect rect1 = GenUI.ContractedBy(rect, (float) (((double) num1 - (double) num1 * 0.949999988079071) / 2.0));
      Widgets.BeginGroup(rect1);
      BlitRequest request = BlitRequest.For((VehiclePawn) vehicle);
      Rect parentRect = GenUI.AtZero(rect1);
      Rect renderRect = vehicle.VehicleMapBlitter.GetRenderRect(parentRect, request, true);
      float num2 = ((Rect) ref parentRect).width / ((Rect) ref renderRect).width;
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector(Vector2.op_Subtraction(((Rect) ref parentRect).position, Vector2.op_Multiply(((Rect) ref renderRect).position, num2)), Vector2.op_Multiply(((Rect) ref parentRect).size, num2));
      this.portrait.Draw(rect2, in request);
      Widgets.EndGroup();
    }
  }
}
