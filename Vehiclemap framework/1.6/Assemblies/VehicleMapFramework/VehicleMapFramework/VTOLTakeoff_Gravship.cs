// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VTOLTakeoff_Gravship
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Vehicles.World;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public class VTOLTakeoff_Gravship : VTOLTakeoff
{
  private static MaterialPropertyBlock thrusterFlameBlock;
  private static MaterialPropertyBlock engineGlowBlock;
  private static Material MatGravEngineGlow;
  private static readonly int ShaderPropertyColor2 = Shader.PropertyToID("_Color2");

  static VTOLTakeoff_Gravship()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() => VTOLTakeoff_Gravship.MatGravEngineGlow = MatLoader.LoadMat("Map/Gravship/GravEngineGlow", -1)));
  }

  public VTOLTakeoff_Gravship() => VTOLTakeoff_Gravship.Init();

  public VTOLTakeoff_Gravship(VTOLTakeoff_Gravship reference, VehiclePawn vehicle)
    : base((VTOLTakeoff) reference, vehicle)
  {
    VTOLTakeoff_Gravship.Init();
  }

  public VerticalProtocolProperties_Gravship LaunchProperties_Gravship
  {
    get => ((LaunchProtocol) this).LaunchProperties as VerticalProtocolProperties_Gravship;
  }

  public VerticalProtocolProperties_Gravship LandingProperties_Gravship
  {
    get => ((LaunchProtocol) this).LandingProperties as VerticalProtocolProperties_Gravship;
  }

  private static void Init()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      VTOLTakeoff_Gravship.thrusterFlameBlock = new MaterialPropertyBlock();
      VTOLTakeoff_Gravship.engineGlowBlock = new MaterialPropertyBlock();
    }));
  }

  protected virtual (Vector3 drawPos, float rotation, DynamicShadowData shadowData) AnimateTakeoff(
    Vector3 drawPos,
    float rotation,
    DynamicShadowData shadowData)
  {
    Vector3 vector3_1 = drawPos;
    (drawPos, rotation, shadowData) = base.AnimateTakeoff(drawPos, rotation, shadowData);
    if (((LaunchProtocol) this).TicksPassed > 0)
    {
      Vector3 vector3_2 = Vector3.zero;
      if (!Ext_LinearCurve.NullOrEmpty(this.LaunchProperties_VTOL.zPositionVerticalCurve))
        vector3_2.z += this.LaunchProperties_VTOL.zPositionVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
      if (!Ext_LinearCurve.NullOrEmpty(this.LaunchProperties_VTOL.xPositionVerticalCurve))
        vector3_2.x += this.LaunchProperties_VTOL.xPositionVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
      if (!Ext_LinearCurve.NullOrEmpty(this.LaunchProperties_VTOL.offsetVerticalCurve))
      {
        Vector2 t = this.LaunchProperties_VTOL.offsetVerticalCurve.EvaluateT(this.TimeInAnimationVTOL);
        vector3_2 = Vector3.op_Addition(vector3_2, new Vector3(t.x, 0.0f, t.y));
      }
      Vector3 vector3_3 = Vector3.op_Subtraction(drawPos, vector3_1);
      drawPos = Vector3.op_Subtraction(drawPos, vector3_3);
      Vector3 vector3_4 = drawPos;
      Vector3 vector3_5 = Vector3.op_Subtraction(vector3_3, vector3_2);
      VehiclePawn vehicle = ((LaunchProtocol) this).Vehicle;
      double num;
      if (vehicle == null)
      {
        num = 0.0;
      }
      else
      {
        Rot4 rotation1 = ((Thing) vehicle).Rotation;
        num = (double) ((Rot4) ref rotation1).AsAngle;
      }
      Vector3 vector3_6 = Vector3.op_Addition(Vector3Utility.RotatedBy(vector3_5, (float) num), vector3_2);
      drawPos = Vector3.op_Addition(vector3_4, vector3_6);
    }
    if (ModsConfig.OdysseyActive && this.LaunchProperties_Gravship != null)
    {
      VehiclePawnWithMap vehiclePawnWithMap = ((LaunchProtocol) this).Vehicle as VehiclePawnWithMap;
      if (vehiclePawnWithMap != null)
      {
        Building_GravEngine gravEngineNewTemp = GravshipUtility.GetPlayerGravEngine_NewTemp(vehiclePawnWithMap.VehicleMap);
        if (gravEngineNewTemp != null)
        {
          Color color1 = GenColor.WithAlpha(Color.white, 0.0f);
          if (!Ext_LinearCurve.NullOrEmpty(this.LaunchProperties_Gravship.thrusterFlameCurve))
            color1.a += this.LaunchProperties_Gravship.thrusterFlameCurve.Evaluate(((LaunchProtocol) this).TimeInAnimation);
          if (!Ext_LinearCurve.NullOrEmpty(this.LaunchProperties_Gravship.thrusterFlameVerticalCurve))
            color1.a += this.LaunchProperties_Gravship.thrusterFlameVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
          VTOLTakeoff_Gravship.thrusterFlameBlock.Clear();
          VTOLTakeoff_Gravship.thrusterFlameBlock.SetColor(VTOLTakeoff_Gravship.ShaderPropertyColor2, color1);
          CollectionExtensions.Do<CompGravshipThruster>(gravEngineNewTemp.GravshipComponents.OfType<CompGravshipThruster>().Where<CompGravshipThruster>((Func<CompGravshipThruster, bool>) (t => ((CompFacility) t).CanBeActive)).Where<CompGravshipThruster>((Func<CompGravshipThruster, bool>) (t =>
          {
            CompProperties_GravshipThruster.ExhaustSettings exhaustSettings = t.Props.exhaustSettings;
            return (exhaustSettings != null ? (exhaustSettings.enabled ? 1 : 0) : 0) != 0 && t.Props.exhaustSettings.ExhaustFleckDef != null;
          })), (Action<CompGravshipThruster>) (t =>
          {
            CompProperties_GravshipThruster props = t.Props;
            ThingWithComps parent = ((ThingComp) t).parent;
            float num = (float) ((Thing) parent).def.size.x * props.flameSize;
            Rot4 rot4 = ((Thing) parent).BaseRotation();
            Vector3 vector3_7 = Quaternion.op_Multiply(((Rot4) ref rot4).AsQuat, props.flameOffsetsPerDirection[((Rot4) ref rot4).AsInt]);
            Vector3 drawPos1 = ((Thing) parent).DrawPos;
            IntVec3 asIntVec3 = ((Rot4) ref rot4).AsIntVec3;
            Vector3 vector3_8 = Vector3Utility.RotatedBy(Vector3.op_Addition(Vector3.op_Multiply(Vector3.op_Multiply(((IntVec3) ref asIntVec3).ToVector3(), num), 0.5f), vector3_7), rotation);
            Vector3 vector3_9 = Vector3.op_Subtraction(drawPos1, vector3_8);
            MaterialRequest materialRequest;
            // ISSUE: explicit constructor call
            ((MaterialRequest) ref materialRequest).\u002Ector(props.FlameShaderType.Shader);
            materialRequest.renderQueue = 3201;
            Material material = MaterialPool.MatFrom(materialRequest);
            foreach (ShaderParameter flameShaderParameter in props.flameShaderParameters)
              flameShaderParameter.Apply(VTOLTakeoff_Gravship.thrusterFlameBlock);
            GenDraw.DrawQuad(material, Vector3Utility.SetToAltitude(vector3_9, (AltitudeLayer) 30).YOffsetFull(vehiclePawnWithMap), Quaternion.AngleAxis(((Rot4) ref rot4).AsAngle + rotation, Vector3.up), num, VTOLTakeoff_Gravship.thrusterFlameBlock);
          }));
          Color color2 = Color.op_Multiply(GenColor.WithAlpha(Color.white, 0.0f), Mathf.Lerp(0.75f, 1f, Mathf.PerlinNoise1D((float) (((LaunchProtocol) this).ticksPassed + this.ticksPassedVertical) / (float) ((LaunchProtocol) this).TotalTicks_Takeoff) * 100f));
          if (!Ext_LinearCurve.NullOrEmpty(this.LaunchProperties_Gravship.engineGlowVerticalCurve))
            color2.a += this.LaunchProperties_Gravship.engineGlowVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
          if (!Ext_LinearCurve.NullOrEmpty(this.LaunchProperties_Gravship.engineGlowCurve))
            color2.a += this.LaunchProperties_Gravship.engineGlowCurve.Evaluate(((LaunchProtocol) this).TimeInAnimation);
          VTOLTakeoff_Gravship.engineGlowBlock.SetColor(VTOLTakeoff_Gravship.ShaderPropertyColor2, color2);
          GenDraw.DrawQuad(VTOLTakeoff_Gravship.MatGravEngineGlow, Vector3Utility.SetToAltitude(((Thing) gravEngineNewTemp).DrawPos, (AltitudeLayer) 39), Quaternion.identity, 12.5f, VTOLTakeoff_Gravship.engineGlowBlock);
        }
      }
    }
    return (drawPos, rotation, shadowData);
  }

  protected virtual (Vector3 drawPos, float rotation, DynamicShadowData shadowData) AnimateLanding(
    Vector3 drawPos,
    float rotation,
    DynamicShadowData shadowData)
  {
    Vector3 vector3_1 = drawPos;
    (drawPos, rotation, shadowData) = base.AnimateLanding(drawPos, rotation, shadowData);
    if (((LaunchProtocol) this).TicksPassed <= 0)
    {
      Vector3 vector3_2 = Vector3.zero;
      if (!Ext_LinearCurve.NullOrEmpty(this.LandingProperties_VTOL.zPositionVerticalCurve))
        vector3_2.z += this.LandingProperties_VTOL.zPositionVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
      if (!Ext_LinearCurve.NullOrEmpty(this.LandingProperties_VTOL.xPositionVerticalCurve))
        vector3_2.x += this.LandingProperties_VTOL.xPositionVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
      if (!Ext_LinearCurve.NullOrEmpty(this.LandingProperties_VTOL.offsetVerticalCurve))
      {
        Vector2 t = this.LandingProperties_VTOL.offsetVerticalCurve.EvaluateT(this.TimeInAnimationVTOL);
        vector3_2 = Vector3.op_Addition(vector3_2, new Vector3(t.x, 0.0f, t.y));
      }
      Vector3 vector3_3 = Vector3.op_Subtraction(drawPos, vector3_1);
      drawPos = Vector3.op_Subtraction(drawPos, vector3_3);
      Vector3 vector3_4 = drawPos;
      Vector3 vector3_5 = Vector3.op_Subtraction(vector3_3, vector3_2);
      VehiclePawn vehicle = ((LaunchProtocol) this).Vehicle;
      double num;
      if (vehicle == null)
      {
        num = 0.0;
      }
      else
      {
        Rot4 rotation1 = ((Thing) vehicle).Rotation;
        num = (double) ((Rot4) ref rotation1).AsAngle;
      }
      Vector3 vector3_6 = Vector3.op_Addition(Vector3Utility.RotatedBy(vector3_5, (float) num), vector3_2);
      drawPos = Vector3.op_Addition(vector3_4, vector3_6);
    }
    if (ModsConfig.OdysseyActive && this.LandingProperties_Gravship != null)
    {
      VehiclePawnWithMap vehiclePawnWithMap = ((LaunchProtocol) this).Vehicle as VehiclePawnWithMap;
      if (vehiclePawnWithMap != null)
      {
        Building_GravEngine gravEngineNewTemp = GravshipUtility.GetPlayerGravEngine_NewTemp(vehiclePawnWithMap.VehicleMap);
        if (gravEngineNewTemp != null)
        {
          Color color1 = GenColor.WithAlpha(Color.white, 0.0f);
          if (!Ext_LinearCurve.NullOrEmpty(this.LandingProperties_Gravship.thrusterFlameCurve))
            color1.a += this.LandingProperties_Gravship.thrusterFlameCurve.Evaluate(((LaunchProtocol) this).TimeInAnimation);
          if (!Ext_LinearCurve.NullOrEmpty(this.LandingProperties_Gravship.thrusterFlameVerticalCurve))
            color1.a += this.LandingProperties_Gravship.thrusterFlameVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
          VTOLTakeoff_Gravship.thrusterFlameBlock.Clear();
          VTOLTakeoff_Gravship.thrusterFlameBlock.SetColor(VTOLTakeoff_Gravship.ShaderPropertyColor2, color1);
          CollectionExtensions.Do<CompGravshipThruster>(gravEngineNewTemp.GravshipComponents.OfType<CompGravshipThruster>().Where<CompGravshipThruster>((Func<CompGravshipThruster, bool>) (t => ((CompFacility) t).CanBeActive)).Where<CompGravshipThruster>((Func<CompGravshipThruster, bool>) (t =>
          {
            CompProperties_GravshipThruster.ExhaustSettings exhaustSettings = t.Props.exhaustSettings;
            return (exhaustSettings != null ? (exhaustSettings.enabled ? 1 : 0) : 0) != 0 && t.Props.exhaustSettings.ExhaustFleckDef != null;
          })), (Action<CompGravshipThruster>) (t =>
          {
            CompProperties_GravshipThruster props = t.Props;
            ThingWithComps parent = ((ThingComp) t).parent;
            float num = (float) ((Thing) parent).def.size.x * props.flameSize;
            Rot4 rot4 = ((Thing) parent).BaseRotation();
            Vector3 vector3_7 = Quaternion.op_Multiply(((Rot4) ref rot4).AsQuat, props.flameOffsetsPerDirection[((Rot4) ref rot4).AsInt]);
            Vector3 drawPos1 = ((Thing) parent).DrawPos;
            IntVec3 asIntVec3 = ((Rot4) ref rot4).AsIntVec3;
            Vector3 vector3_8 = Vector3Utility.RotatedBy(Vector3.op_Addition(Vector3.op_Multiply(Vector3.op_Multiply(((IntVec3) ref asIntVec3).ToVector3(), num), 0.5f), vector3_7), rotation);
            Vector3 vector3_9 = Vector3.op_Subtraction(drawPos1, vector3_8);
            MaterialRequest materialRequest;
            // ISSUE: explicit constructor call
            ((MaterialRequest) ref materialRequest).\u002Ector(props.FlameShaderType.Shader);
            materialRequest.renderQueue = 3201;
            Material material = MaterialPool.MatFrom(materialRequest);
            foreach (ShaderParameter flameShaderParameter in props.flameShaderParameters)
              flameShaderParameter.Apply(VTOLTakeoff_Gravship.thrusterFlameBlock);
            GenDraw.DrawQuad(material, Vector3Utility.SetToAltitude(vector3_9, (AltitudeLayer) 30).YOffsetFull(vehiclePawnWithMap), Quaternion.AngleAxis(((Rot4) ref rot4).AsAngle + rotation, Vector3.up), num, VTOLTakeoff_Gravship.thrusterFlameBlock);
          }));
          Color color2 = Color.op_Multiply(GenColor.WithAlpha(Color.white, 0.0f), Mathf.Lerp(0.75f, 1f, Mathf.PerlinNoise1D((float) (((LaunchProtocol) this).ticksPassed + this.ticksPassedVertical) / (float) ((LaunchProtocol) this).TotalTicks_Takeoff) * 100f));
          if (!Ext_LinearCurve.NullOrEmpty(this.LandingProperties_Gravship.engineGlowVerticalCurve))
            color2.a += this.LandingProperties_Gravship.engineGlowVerticalCurve.Evaluate(this.TimeInAnimationVTOL);
          if (!Ext_LinearCurve.NullOrEmpty(this.LandingProperties_Gravship.engineGlowCurve))
            color2.a += this.LandingProperties_Gravship.engineGlowCurve.Evaluate(((LaunchProtocol) this).TimeInAnimation);
          VTOLTakeoff_Gravship.engineGlowBlock.SetColor(VTOLTakeoff_Gravship.ShaderPropertyColor2, color2);
          GenDraw.DrawQuad(VTOLTakeoff_Gravship.MatGravEngineGlow, Vector3Utility.SetToAltitude(((Thing) gravEngineNewTemp).DrawPos, (AltitudeLayer) 39), Quaternion.identity, 12.5f, VTOLTakeoff_Gravship.engineGlowBlock);
        }
      }
    }
    return (drawPos, rotation, shadowData);
  }

  public virtual void OrderProtocol(LaunchProtocol.LaunchType launchType_)
  {
    ((LaunchProtocol) this).OrderProtocol(launchType_);
    if (!ModsConfig.OdysseyActive || !(((LaunchProtocol) this).Vehicle is VehiclePawnWithMap vehicle) || launchType_ != 1)
      return;
    Building_GravEngine gravEngineNewTemp = GravshipUtility.GetPlayerGravEngine_NewTemp(vehicle.VehicleMap);
    if (gravEngineNewTemp == null)
      return;
    Building_GravEngine buildingGravEngine = gravEngineNewTemp;
    int ticksGame = GenTicks.TicksGame;
    LaunchInfo launchInfo = gravEngineNewTemp.launchInfo;
    int num1 = (int) GravshipUtility.LaunchCooldownFromQuality(launchInfo != null ? launchInfo.quality : 1f);
    int num2 = ticksGame + num1;
    buildingGravEngine.cooldownCompleteTick = num2;
  }

  public virtual IEnumerable<ArrivalOption> GetArrivalOptions(GlobalTargetInfo target)
  {
    VTOLTakeoff_Gravship vtolTakeoffGravship = this;
    WorldObject worldObject = ((GlobalTargetInfo) ref target).WorldObject;
    if (worldObject is MapParent mapParent && worldObject.Spawned && mapParent.HasMap && !EnterCooldownCompUtility.EnterCooldownBlocksEntering(mapParent))
    {
      yield return new ArrivalOption(TranslatorFormattedStringExtensions.Translate("VF_LandVehicleTargetedLanding", NamedArgument.op_Implicit(((WorldObject) mapParent).Label)), (IArrivalAction) new ArrivalAction_LoadMap(((LaunchProtocol) vtolTakeoffGravship).vehicle, AerialVehicleArrivalModeDefOf.TargetedLanding));
    }
    else
    {
      // ISSUE: reference to a compiler-generated method
      foreach (ArrivalOption arrivalOption in vtolTakeoffGravship.\u003C\u003En__0(target))
        yield return arrivalOption;
    }
  }
}
