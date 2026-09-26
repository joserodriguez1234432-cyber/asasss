// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.SubEffector_SpawnSinkerMote
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using UnityEngine;
using Vehicles;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class SubEffector_SpawnSinkerMote : SubEffecter
{
  private readonly SubEffecterDef subDef;
  private static readonly SimpleCurve ColorOverlayAlphaCurve;

  public SubEffector_SpawnSinkerMote(SubEffecterDef subDef, Effecter parent)
  {
    this.subDef = subDef;
    // ISSUE: explicit constructor call
    base.\u002Ector(subDef, parent);
  }

  public virtual void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
  {
    VehiclePawn vehicle;
    if (!GridsUtility.TryGetFirstThing<VehiclePawn>(((TargetInfo) ref A).Cell, ((TargetInfo) ref A).Map, ref vehicle) || ((Thing) vehicle).Map == null)
      return;
    BlitRequest blitRequest = BlitRequest.For(vehicle);
    blitRequest.rot = vehicle.FullRotation;
    Vector2 vector2_1 = Vector2.op_Division(((Thing) vehicle).DrawSize, ((ThingDef) vehicle.VehicleDef).uiIconScale);
    float num = Mathf.Max(vector2_1.x, vector2_1.y);
    Vector2 vector2_2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_2).\u002Ector(num, num);
    Rect rect;
    // ISSUE: explicit constructor call
    ((Rect) ref rect).\u002Ector(Vector2.zero, Vector2.op_Multiply(vector2_2, 128f));
    RenderTexture renderTexture = VehicleGui.CreateRenderTexture(rect, ref blitRequest, 2f);
    VehicleGui.Blit(renderTexture, rect, ref blitRequest, 1f, false);
    MoteThrownSinker moteThrownSinker = (MoteThrownSinker) ThingMaker.MakeThing(VMF_DefOf.VMF_MoteSink, (ThingDef) null);
    moteThrownSinker.SetParameters(renderTexture, Quaternion.AngleAxis(-VehicleMapUtility.get_ExtraAngle(vehicle), Vector3.up), Vector3Utility.SetToAltitude(Vector2Utility.ToVector3(vector2_2), (AltitudeLayer) 12), this.subDef.color, SubEffector_SpawnSinkerMote.ColorOverlayAlphaCurve);
    moteThrownSinker.SetVelocity(((FloatRange) ref this.subDef.angle).RandomInRange, ((FloatRange) ref this.subDef.speed).RandomInRange);
    ((Mote) moteThrownSinker).exactPosition = ((Thing) vehicle).DrawPos;
    GenSpawn.Spawn((Thing) moteThrownSinker, IntVec3Utility.ToIntVec3(((Mote) moteThrownSinker).exactPosition), ((Thing) vehicle).Map, (WipeMode) 0);
  }

  static SubEffector_SpawnSinkerMote()
  {
    SimpleCurve simpleCurve = new SimpleCurve();
    simpleCurve.Add(new CurvePoint(0.0f, 0.0f), true);
    simpleCurve.Add(new CurvePoint(0.9f, 0.8f), true);
    simpleCurve.Add(new CurvePoint(0.98f, 0.8f), true);
    simpleCurve.Add(new CurvePoint(1f, 0.0f), true);
    SubEffector_SpawnSinkerMote.ColorOverlayAlphaCurve = simpleCurve;
  }
}
