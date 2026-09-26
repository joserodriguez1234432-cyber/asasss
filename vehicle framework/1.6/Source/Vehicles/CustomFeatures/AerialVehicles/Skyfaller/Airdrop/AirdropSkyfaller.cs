// Decompiled with JetBrains decompiler
// Type: Vehicles.AirdropSkyfaller
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
[StaticConstructorOnStartup]
public class AirdropSkyfaller : Skyfaller
{
  private static readonly Material RopeMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.SolidColor, new Color(0.15f, 0.15f, 0.15f));

  public AirdropDef AirdropDef => ((Thing) this).def as AirdropDef;

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    Thing thingForGraphic = this.GetThingForGraphic();
    float extraRotation = 0.0f;
    if (((Thing) this).def.skyfaller.rotateGraphicTowardsDirection)
      extraRotation = this.angle;
    if (((Thing) this).def.skyfaller.angleCurve != null)
      this.angle = ((Thing) this).def.skyfaller.angleCurve.Evaluate(this.TimeInAnimation);
    if (((Thing) this).def.skyfaller.rotationCurve != null)
      extraRotation += ((Thing) this).def.skyfaller.rotationCurve.Evaluate(this.TimeInAnimation);
    if (((Thing) this).def.skyfaller.xPositionCurve != null)
      drawLoc.x += ((Thing) this).def.skyfaller.xPositionCurve.Evaluate(this.TimeInAnimation);
    if (((Thing) this).def.skyfaller.zPositionCurve != null)
      drawLoc.z += ((Thing) this).def.skyfaller.zPositionCurve.Evaluate(this.TimeInAnimation);
    if (thingForGraphic is VehiclePawn vehiclePawn)
      ((Thing) vehiclePawn).DynamicDrawPhaseAt((DrawPhase) 2, drawLoc, flip);
    if (thingForGraphic is Pawn pawn)
    {
      ((Thing) pawn).DrawNowAt(drawLoc, flip);
    }
    else
    {
      Graphic graphic = ((Thing) this).Graphic;
      Vector3 vector3 = drawLoc;
      Rot4 rot4;
      if (!flip)
      {
        rot4 = thingForGraphic.Rotation;
      }
      else
      {
        Rot4 rotation = thingForGraphic.Rotation;
        rot4 = ((Rot4) ref rotation).Opposite;
      }
      Thing thing = thingForGraphic;
      double num = (double) extraRotation;
      graphic.Draw(vector3, rot4, thing, (float) num);
    }
    this.DrawParachute(drawLoc, extraRotation);
    this.DrawDropSpotShadowTrailing(drawLoc);
  }

  protected virtual void DrawDropSpotShadowTrailing(Vector3 apparentLoc)
  {
    Material shadowMaterial = this.ShadowMaterial;
    if (Object.op_Equality((Object) shadowMaterial, (Object) null))
      return;
    Vector3 vector3 = GenThing.TrueCenter((Thing) this);
    vector3.x = apparentLoc.x;
    Skyfaller.DrawDropSpotShadow(vector3, ((Thing) this).Rotation, shadowMaterial, ((Thing) this).def.skyfaller.shadowSize, this.ticksToImpact);
  }

  public void DrawParachute(Vector3 drawLoc, float extraRotation)
  {
    GraphicData parachuteGraphicData = this.AirdropDef.parachuteGraphicData;
    if (parachuteGraphicData == null)
      return;
    parachuteGraphicData.Graphic.DrawWorker(Vector3.op_Addition(drawLoc, Vector3.op_Multiply(Altitudes.AltIncVect, 2f)), Rot4.North, (ThingDef) null, (Thing) null, extraRotation);
    if (GenList.NullOrEmpty<AirdropDef.AnchorPoint>((IList<AirdropDef.AnchorPoint>) this.AirdropDef.ropes))
      return;
    foreach (AirdropDef.AnchorPoint rope in this.AirdropDef.ropes)
    {
      Vector3 vector3_1 = Vector3.op_Addition(new Vector3(rope.from.x, 0.0f, rope.from.y), Vector3.op_Multiply(Altitudes.AltIncVect, (float) rope.layer));
      Vector3 vector3_2 = Vector3.op_Addition(new Vector3(rope.to.x, 0.0f, rope.to.y), Vector3.op_Multiply(Altitudes.AltIncVect, (float) rope.layer));
      GenDraw.DrawLineBetween(Vector3.op_Addition(vector3_1, drawLoc), Vector3.op_Addition(vector3_2, drawLoc), AirdropSkyfaller.RopeMat, 0.05f);
    }
  }

  private Thing GetThingForGraphic()
  {
    return ((Thing) this).def.graphicData != null || !this.innerContainer.Any ? (Thing) this : this.innerContainer[0];
  }

  protected virtual void Impact()
  {
    for (int index = 0; index < 6; ++index)
    {
      IntVec3 position = ((Thing) this).Position;
      FleckMaker.ThrowDustPuff(Vector3.op_Addition(((IntVec3) ref position).ToVector3Shifted(), Gen.RandomHorizontalVector(1f)), ((Thing) this).Map, 1.2f);
    }
    IntVec3 position1 = ((Thing) this).Position;
    FleckMaker.ThrowLightningGlow(((IntVec3) ref position1).ToVector3Shifted(), ((Thing) this).Map, 2f);
    GenClamor.DoClamor((Thing) this, 15f, ClamorDefOf.Impact);
    base.Impact();
  }

  protected virtual void SpawnThings()
  {
    for (int index = this.innerContainer.Count - 1; index >= 0; --index)
    {
      Thing thing = this.innerContainer[index];
      // ISSUE: method pointer
      if (GenPlace.TryPlaceThing(thing, ((Thing) this).Position, ((Thing) this).Map, (ThingPlaceMode) 1, new Action<Thing, int>((object) this, __methodptr(\u003CSpawnThings\u003Eg__PlaceThing\u007C8_0)), (Predicate<IntVec3>) null, new Rot4?(((BuildableDef) this.innerContainer[index].def).defaultPlacingRot), 1) && thing is Pawn pawn && pawn.IsColonist && ((Thing) pawn).Spawned && !((Thing) this).Map.IsPlayerHome)
        pawn.drafter.Drafted = true;
    }
  }

  protected virtual void HitRoof()
  {
    base.HitRoof();
    if (!((IEnumerable<IntVec3>) (object) GenAdj.OccupiedRect((Thing) this)).Any<IntVec3>((Func<IntVec3, bool>) (cell => GridsUtility.Fogged(cell, ((Thing) this).Map))))
      return;
    FloodFillerFog.FloodUnfog(((Thing) this).Position, ((Thing) this).Map);
  }

  public virtual void SpawnSetup(Map map, bool respawningAfterLoad)
  {
    base.SpawnSetup(map, respawningAfterLoad);
    foreach (Thing thing in (IEnumerable<Thing>) this.innerContainer)
    {
      if (thing is Pawn pawn)
        ((Thing) pawn).Rotation = Rot4.South;
    }
  }

  [DebugAction("Vehicle Framework", "Spawn Airdrop", false, false, false, false, false, 0, false)]
  private static List<DebugActionNode> SpawnAirdrop()
  {
    return new List<DebugActionNode>()
    {
      new DebugActionNode(((Def) SkyfallerDefOf.AirdropPackage).defName, (DebugActionType) 1, (Action) null, (Action<Pawn>) null)
      {
        action = (Action) (() =>
        {
          Map currentMap = Find.CurrentMap;
          if (currentMap == null)
          {
            Log.Error("Attempting to use DebugRegionOptions with null map.");
          }
          else
          {
            IntVec3 intVec3 = UI.MouseCell();
            List<Thing> contents = new List<Thing>(5)
            {
              MakeAirdropContent(ThingDefOf.MedicineIndustrial),
              MakeAirdropContent(ThingDefOf.MealSurvivalPack),
              MakeAirdropContent(ThingDefOf.MealSurvivalPack),
              MakeAirdropContent(ThingDefOf.MealSurvivalPack),
              MakeAirdropContent(ThingDefOf.Penoxycyline)
            };
            GenSpawn.Spawn((Thing) AirdropSkyfallerMaker.MakeAirdrop(SkyfallerDefOf.AirdropPackage, contents, AirdropProperties.Default with
            {
              packIntoContainer = true
            }), intVec3, currentMap, (WipeMode) 0);
          }
        })
      },
      new DebugActionNode(((Def) SkyfallerDefOf.AirdropParatrooper).defName, (DebugActionType) 1, (Action) null, (Action<Pawn>) null)
      {
        childGetter = (Func<List<DebugActionNode>>) (() =>
        {
          Map map = Find.CurrentMap;
          if (map == null)
          {
            Log.Error("Attempting to use DebugRegionOptions with null map.");
            return (List<DebugActionNode>) null;
          }
          List<DebugActionNode> debugActionNodeList = new List<DebugActionNode>();
          foreach (Pawn freeColonist in map.mapPawns.FreeColonists)
          {
            Pawn pawn = freeColonist;
            debugActionNodeList.Add(new DebugActionNode(((Entity) pawn).Label, (DebugActionType) 1, (Action) (() =>
            {
              IntVec3 intVec3 = UI.MouseCell();
              GenSpawn.Spawn((Thing) AirdropSkyfallerMaker.MakeAirdrop(SkyfallerDefOf.AirdropParatrooper, (Thing) pawn, AirdropProperties.Default), intVec3, map, (WipeMode) 0);
            }), (Action<Pawn>) null));
          }
          return debugActionNodeList;
        })
      }
    };

    static Thing MakeAirdropContent(ThingDef thingDef)
    {
      Thing thing = ThingMaker.MakeThing(thingDef, (ThingDef) null);
      thing.stackCount = Rand.Range(1, thingDef.stackLimit);
      return thing;
    }
  }
}
