// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.MethodInfoCache
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VehicleMapFramework.VMF_HarmonyPatches;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class MethodInfoCache
{
  private static readonly WeakReference<MethodInfoCache> cacheInt = new WeakReference<MethodInfoCache>((MethodInfoCache) null);
  public readonly MethodInfo g_FocusedVehicle = AccessTools.PropertyGetter(typeof (Command_FocusVehicleMap), "FocusedVehicle");
  public readonly MethodInfo m_FocusedOnVehicleMap = (MethodInfoCache.\u003C\u003EO.\u003C0\u003E__FocusedOnVehicleMap ?? (MethodInfoCache.\u003C\u003EO.\u003C0\u003E__FocusedOnVehicleMap = new \u003C\u003EF\u007B00000002\u007D<VehiclePawnWithMap, bool>(VehicleMapUtility.FocusedOnVehicleMap))).Method;
  public readonly MethodInfo g_Find_CurrentMap = AccessTools.PropertyGetter(typeof (Find), "CurrentMap");
  public readonly MethodInfo g_VehicleMapUtility_CurrentMap = AccessTools.PropertyGetter(typeof (VehicleMapUtility), "CurrentMap");
  public readonly MethodInfo m_IsVehicleMapOf = (MethodInfoCache.\u003C\u003EO.\u003C1\u003E__IsVehicleMapOf ?? (MethodInfoCache.\u003C\u003EO.\u003C1\u003E__IsVehicleMapOf = new \u003C\u003EF\u007B00000010\u007D<Map, VehiclePawnWithMap, bool>(VehicleMapUtility.IsVehicleMapOf))).Method;
  public readonly MethodInfo m_IsNonFocusedVehicleMapOf = (MethodInfoCache.\u003C\u003EO.\u003C2\u003E__IsNonFocusedVehicleMapOf ?? (MethodInfoCache.\u003C\u003EO.\u003C2\u003E__IsNonFocusedVehicleMapOf = new \u003C\u003EF\u007B00000010\u007D<Map, VehiclePawnWithMap, bool>(VehicleMapUtility.IsNonFocusedVehicleMapOf))).Method;
  public readonly MethodInfo m_IsOnVehicleMapOf = (MethodInfoCache.\u003C\u003EO.\u003C3\u003E__IsOnVehicleMapOf ?? (MethodInfoCache.\u003C\u003EO.\u003C3\u003E__IsOnVehicleMapOf = new \u003C\u003EF\u007B00000010\u007D<Thing, VehiclePawnWithMap, bool>(VehicleMapUtility.IsOnVehicleMapOf))).Method;
  public readonly MethodInfo m_IsOnNonFocusedVehicleMapOf = (MethodInfoCache.\u003C\u003EO.\u003C4\u003E__IsOnNonFocusedVehicleMapOf ?? (MethodInfoCache.\u003C\u003EO.\u003C4\u003E__IsOnNonFocusedVehicleMapOf = new \u003C\u003EF\u007B00000010\u007D<Thing, VehiclePawnWithMap, bool>(VehicleMapUtility.IsOnNonFocusedVehicleMapOf))).Method;
  public readonly MethodInfo m_YOffsetFull = (MethodInfoCache.\u003C\u003EO.\u003C5\u003E__YOffsetFull ?? (MethodInfoCache.\u003C\u003EO.\u003C5\u003E__YOffsetFull = new Func<float, VehiclePawnWithMap, float>(VehicleMapUtility.YOffsetFull))).Method;
  public readonly MethodInfo m_ToBaseMapCoord1 = (MethodInfoCache.\u003C\u003EO.\u003C6\u003E__ToBaseMapCoord ?? (MethodInfoCache.\u003C\u003EO.\u003C6\u003E__ToBaseMapCoord = new Func<Vector3, Vector3>(VehicleMapUtility.ToBaseMapCoord))).Method;
  public readonly MethodInfo m_ToBaseMapCoord2 = (MethodInfoCache.\u003C\u003EO.\u003C7\u003E__ToBaseMapCoord ?? (MethodInfoCache.\u003C\u003EO.\u003C7\u003E__ToBaseMapCoord = new Func<Vector3, VehiclePawnWithMap, Vector3>(VehicleMapUtility.ToBaseMapCoord))).Method;
  public readonly MethodInfo m_ToBaseMapCoord3 = (MethodInfoCache.\u003C\u003EO.\u003C8\u003E__ToBaseMapCoord ?? (MethodInfoCache.\u003C\u003EO.\u003C8\u003E__ToBaseMapCoord = new Func<Vector3, Map, Vector3>(VehicleMapUtility.ToBaseMapCoord))).Method;
  public readonly MethodInfo m_ToBaseMapCoordCell = (MethodInfoCache.\u003C\u003EO.\u003C9\u003E__ToBaseMapCoord ?? (MethodInfoCache.\u003C\u003EO.\u003C9\u003E__ToBaseMapCoord = new Func<IntVec3, VehiclePawnWithMap, IntVec3>(VehicleMapUtility.ToBaseMapCoord))).Method;
  public readonly MethodInfo m_ToThingMapCoord = (MethodInfoCache.\u003C\u003EO.\u003C10\u003E__ToThingMapCoord ?? (MethodInfoCache.\u003C\u003EO.\u003C10\u003E__ToThingMapCoord = new Func<IntVec3, Thing, IntVec3>(VehicleMapUtility.ToThingMapCoord))).Method;
  public readonly MethodInfo m_ToNonFocusedThingMapCoord = (MethodInfoCache.\u003C\u003EO.\u003C11\u003E__ToNonFocusedThingMapCoord ?? (MethodInfoCache.\u003C\u003EO.\u003C11\u003E__ToNonFocusedThingMapCoord = new Func<Vector3, Thing, Vector3>(VehicleMapUtility.ToNonFocusedThingMapCoord))).Method;
  public readonly MethodInfo m_ToThingBaseMapCoord = (MethodInfoCache.\u003C\u003EO.\u003C12\u003E__ToThingBaseMapCoord ?? (MethodInfoCache.\u003C\u003EO.\u003C12\u003E__ToThingBaseMapCoord = new Func<Vector3, Thing, Vector3>(VehicleMapUtility.ToThingBaseMapCoord))).Method;
  public readonly MethodInfo m_ToVehicleMapCoord = (MethodInfoCache.\u003C\u003EO.\u003C13\u003E__ToVehicleMapCoord ?? (MethodInfoCache.\u003C\u003EO.\u003C13\u003E__ToVehicleMapCoord = new Func<Vector3, Vector3>(VehicleMapUtility.ToVehicleMapCoord))).Method;
  public readonly MethodInfo g_Thing_Map = AccessTools.PropertyGetter(typeof (Thing), "Map");
  public readonly MethodInfo g_TargetInfo_Map = AccessTools.PropertyGetter(typeof (TargetInfo), "Map");
  public readonly MethodInfo g_GlobalTargetInfo_Map = AccessTools.PropertyGetter(typeof (GlobalTargetInfo), "Map");
  public readonly MethodInfo m_BaseMap_Map = (MethodInfoCache.\u003C\u003EO.\u003C14\u003E__BaseMap ?? (MethodInfoCache.\u003C\u003EO.\u003C14\u003E__BaseMap = new Func<Map, Map>(VehicleMapUtility.BaseMap))).Method;
  public readonly MethodInfo m_BaseMapOrCaravan_Map = (MethodInfoCache.\u003C\u003EO.\u003C15\u003E__get_BaseMapOrCaravan ?? (MethodInfoCache.\u003C\u003EO.\u003C15\u003E__get_BaseMapOrCaravan = new Func<Map, object>(VehicleMapUtility.get_BaseMapOrCaravan))).Method;
  public readonly MethodInfo m_BaseMap_Thing = (MethodInfoCache.\u003C\u003EO.\u003C16\u003E__BaseMap ?? (MethodInfoCache.\u003C\u003EO.\u003C16\u003E__BaseMap = new Func<Thing, Map>(VehicleMapUtility.BaseMap))).Method;
  public readonly MethodInfo m_BaseMapOrCaravan_Thing = (MethodInfoCache.\u003C\u003EO.\u003C17\u003E__get_BaseMapOrCaravan ?? (MethodInfoCache.\u003C\u003EO.\u003C17\u003E__get_BaseMapOrCaravan = new Func<Thing, object>(VehicleMapUtility.get_BaseMapOrCaravan))).Method;
  public readonly MethodInfo m_BaseMap_TargetInfo = AccessTools.Method(typeof (VehicleMapUtility), "BaseMap", new Type[1]
  {
    typeof (TargetInfo).MakeByRefType()
  }, (Type[]) null);
  public readonly MethodInfo m_BaseMap_GlobalTargetInfo = AccessTools.Method(typeof (VehicleMapUtility), "BaseMap", new Type[1]
  {
    typeof (GlobalTargetInfo).MakeByRefType()
  }, (Type[]) null);
  public readonly MethodInfo m_TargetMapOrMap = (MethodInfoCache.\u003C\u003EO.\u003C18\u003E__TargetMapOrMap ?? (MethodInfoCache.\u003C\u003EO.\u003C18\u003E__TargetMapOrMap = new Func<Map, Thing, Map>(TargetMapUtility.TargetMapOrMap))).Method;
  public readonly MethodInfo m_TargetMapOrThingMap = (MethodInfoCache.\u003C\u003EO.\u003C19\u003E__get_TargetMapOrThingMap ?? (MethodInfoCache.\u003C\u003EO.\u003C19\u003E__get_TargetMapOrThingMap = new Func<Thing, Map>(TargetMapUtility.get_TargetMapOrThingMap))).Method;
  public readonly MethodInfo m_TargetMapOrPawnMap = (MethodInfoCache.\u003C\u003EO.\u003C20\u003E__get_TargetMapOrPawnMap ?? (MethodInfoCache.\u003C\u003EO.\u003C20\u003E__get_TargetMapOrPawnMap = new Func<Pawn, Map>(TargetMapUtility.get_TargetMapOrPawnMap))).Method;
  public readonly MethodInfo m_LordMapOrMapHeld = (MethodInfoCache.\u003C\u003EO.\u003C21\u003E__get_LordMapOrMapHeld ?? (MethodInfoCache.\u003C\u003EO.\u003C21\u003E__get_LordMapOrMapHeld = new Func<Pawn, Map>(VehicleMapUtility.get_LordMapOrMapHeld))).Method;
  public readonly MethodInfo g_Zone_Map = AccessTools.PropertyGetter(typeof (Zone), "Map");
  public readonly MethodInfo g_Thing_MapHeld = AccessTools.PropertyGetter(typeof (Thing), "MapHeld");
  public readonly MethodInfo m_MapHeldBaseMap = (MethodInfoCache.\u003C\u003EO.\u003C22\u003E__MapHeldBaseMap ?? (MethodInfoCache.\u003C\u003EO.\u003C22\u003E__MapHeldBaseMap = new Func<Thing, Map>(VehicleMapUtility.MapHeldBaseMap))).Method;
  public readonly MethodInfo m_MapHeldBaseMapOrCaravan = (MethodInfoCache.\u003C\u003EO.\u003C23\u003E__get_MapHeldBaseMapOrCaravan ?? (MethodInfoCache.\u003C\u003EO.\u003C23\u003E__get_MapHeldBaseMapOrCaravan = new Func<Thing, object>(VehicleMapUtility.get_MapHeldBaseMapOrCaravan))).Method;
  public readonly MethodInfo m_DepartMapOrPawnMap = (MethodInfoCache.\u003C\u003EO.\u003C24\u003E__get_DepartMapOrPawnMap ?? (MethodInfoCache.\u003C\u003EO.\u003C24\u003E__get_DepartMapOrPawnMap = new Func<Pawn, Map>(CrossMapReachabilityUtility.get_DepartMapOrPawnMap))).Method;
  public readonly MethodInfo m_DepartMapOrPawnMapHeld = (MethodInfoCache.\u003C\u003EO.\u003C25\u003E__get_DepartMapOrPawnMapHeld ?? (MethodInfoCache.\u003C\u003EO.\u003C25\u003E__get_DepartMapOrPawnMapHeld = new Func<Pawn, Map>(CrossMapReachabilityUtility.get_DepartMapOrPawnMapHeld))).Method;
  public readonly MethodInfo g_Thing_Position = AccessTools.PropertyGetter(typeof (Thing), "Position");
  public readonly MethodInfo m_PositionOnBaseMap = (MethodInfoCache.\u003C\u003EO.\u003C26\u003E__get_PositionOnBaseMap ?? (MethodInfoCache.\u003C\u003EO.\u003C26\u003E__get_PositionOnBaseMap = new Func<Thing, IntVec3>(VehicleMapUtility.get_PositionOnBaseMap))).Method;
  public readonly MethodInfo m_PositionOnBaseMapSpawned = (MethodInfoCache.\u003C\u003EO.\u003C27\u003E__get_PositionOnBaseMapSpawned ?? (MethodInfoCache.\u003C\u003EO.\u003C27\u003E__get_PositionOnBaseMapSpawned = new Func<Thing, IntVec3>(VehicleMapUtility.get_PositionOnBaseMapSpawned))).Method;
  public readonly MethodInfo g_Thing_PositionHeld = AccessTools.PropertyGetter(typeof (Thing), "PositionHeld");
  public readonly MethodInfo m_PositionHeldOnBaseMap = (MethodInfoCache.\u003C\u003EO.\u003C28\u003E__get_PositionHeldOnBaseMap ?? (MethodInfoCache.\u003C\u003EO.\u003C28\u003E__get_PositionHeldOnBaseMap = new Func<Thing, IntVec3>(VehicleMapUtility.get_PositionHeldOnBaseMap))).Method;
  public readonly MethodInfo m_PositionHeldOnBaseMapSpawned = (MethodInfoCache.\u003C\u003EO.\u003C29\u003E__get_PositionHeldOnBaseMapSpawned ?? (MethodInfoCache.\u003C\u003EO.\u003C29\u003E__get_PositionHeldOnBaseMapSpawned = new Func<Thing, IntVec3>(VehicleMapUtility.get_PositionHeldOnBaseMapSpawned))).Method;
  public readonly MethodInfo m_PositionOnAnotherThingMap = (MethodInfoCache.\u003C\u003EO.\u003C30\u003E__PositionOnAnotherThingMap ?? (MethodInfoCache.\u003C\u003EO.\u003C30\u003E__PositionOnAnotherThingMap = new Func<Thing, Thing, IntVec3>(VehicleMapUtility.PositionOnAnotherThingMap))).Method;
  public readonly MethodInfo g_LocalTargetInfo_Cell = AccessTools.PropertyGetter(typeof (LocalTargetInfo), "Cell");
  public readonly MethodInfo g_TargetInfo_Cell = AccessTools.PropertyGetter(typeof (TargetInfo), "Cell");
  public readonly MethodInfo g_GlobalTargetInfo_Cell = AccessTools.PropertyGetter(typeof (GlobalTargetInfo), "Cell");
  public readonly MethodInfo m_CellOnBaseMap = AccessTools.Method(typeof (VehicleMapUtility), "CellOnBaseMap", new Type[1]
  {
    typeof (LocalTargetInfo).MakeByRefType()
  }, (Type[]) null);
  public readonly MethodInfo m_CellOnBaseMapSpawned = AccessTools.Method(typeof (VehicleMapUtility), "CellOnBaseMapSpawned", new Type[1]
  {
    typeof (LocalTargetInfo).MakeByRefType()
  }, (Type[]) null);
  public readonly MethodInfo m_CellOnBaseMap_TargetInfo = AccessTools.Method(typeof (VehicleMapUtility), "CellOnBaseMap", new Type[1]
  {
    typeof (TargetInfo).MakeByRefType()
  }, (Type[]) null);
  public readonly MethodInfo m_CellOnBaseMapSpawned_TargetInfo = AccessTools.Method(typeof (VehicleMapUtility), "CellOnBaseMapSpawned", new Type[1]
  {
    typeof (TargetInfo).MakeByRefType()
  }, (Type[]) null);
  public readonly MethodInfo m_CellOnBaseMap_GlobalTargetInfo = AccessTools.Method(typeof (VehicleMapUtility), "CellOnBaseMap", new Type[1]
  {
    typeof (GlobalTargetInfo).MakeByRefType()
  }, (Type[]) null);
  public readonly MethodInfo m_CellOnBaseMapSpawned_GlobalTargetInfo = AccessTools.Method(typeof (VehicleMapUtility), "CellOnBaseMapSpawned", new Type[1]
  {
    typeof (GlobalTargetInfo).MakeByRefType()
  }, (Type[]) null);
  public readonly MethodInfo m_OccupiedRect = (MethodInfoCache.\u003C\u003EO.\u003C31\u003E__OccupiedRect ?? (MethodInfoCache.\u003C\u003EO.\u003C31\u003E__OccupiedRect = new Func<Thing, CellRect>(GenAdj.OccupiedRect))).Method;
  public readonly MethodInfo m_MovedOccupiedRect = (MethodInfoCache.\u003C\u003EO.\u003C32\u003E__MovedOccupiedRect ?? (MethodInfoCache.\u003C\u003EO.\u003C32\u003E__MovedOccupiedRect = new Func<Thing, CellRect>(VehicleMapUtility.MovedOccupiedRect))).Method;
  public readonly MethodInfo m_ToTargetInfo = AccessTools.Method(typeof (LocalTargetInfo), "ToTargetInfo", (Type[]) null, (Type[]) null);
  public readonly MethodInfo m_ToBaseMapTargetInfo = (MethodInfoCache.\u003C\u003EO.\u003C33\u003E__ToBaseMapTargetInfo ?? (MethodInfoCache.\u003C\u003EO.\u003C33\u003E__ToBaseMapTargetInfo = new \u003C\u003EF\u007B00000001\u007D<LocalTargetInfo, Map, TargetInfo>(VehicleMapUtility.ToBaseMapTargetInfo))).Method;
  public readonly MethodInfo m_BaseRotation = (MethodInfoCache.\u003C\u003EO.\u003C34\u003E__BaseRotation ?? (MethodInfoCache.\u003C\u003EO.\u003C34\u003E__BaseRotation = new Func<Thing, Rot4>(VehicleMapUtility.BaseRotation))).Method;
  public readonly MethodInfo m_BaseRotationSpawned = (MethodInfoCache.\u003C\u003EO.\u003C35\u003E__BaseRotationSpawned ?? (MethodInfoCache.\u003C\u003EO.\u003C35\u003E__BaseRotationSpawned = new Func<Thing, Rot4>(VehicleMapUtility.BaseRotationSpawned))).Method;
  public readonly MethodInfo m_BaseRotationVehicleDraw = (MethodInfoCache.\u003C\u003EO.\u003C36\u003E__BaseRotationVehicleDraw ?? (MethodInfoCache.\u003C\u003EO.\u003C36\u003E__BaseRotationVehicleDraw = new Func<Thing, Rot4>(VehicleMapUtility.BaseRotationVehicleDraw))).Method;
  public readonly MethodInfo m_BaseFullRotation_Thing = (MethodInfoCache.\u003C\u003EO.\u003C37\u003E__BaseFullRotation ?? (MethodInfoCache.\u003C\u003EO.\u003C37\u003E__BaseFullRotation = new Func<Thing, Rot8>(VehicleMapUtility.BaseFullRotation))).Method;
  public readonly MethodInfo m_BaseFullRotationSpawned_Thing = (MethodInfoCache.\u003C\u003EO.\u003C38\u003E__BaseFullRotationSpawned ?? (MethodInfoCache.\u003C\u003EO.\u003C38\u003E__BaseFullRotationSpawned = new Func<Thing, Rot8>(VehicleMapUtility.BaseFullRotationSpawned))).Method;
  public readonly MethodInfo m_BaseFullRotationAsRot4 = (MethodInfoCache.\u003C\u003EO.\u003C39\u003E__BaseFullRotationAsRot4 ?? (MethodInfoCache.\u003C\u003EO.\u003C39\u003E__BaseFullRotationAsRot4 = new Func<Thing, Rot4>(VehicleMapUtility.BaseFullRotationAsRot4))).Method;
  public readonly MethodInfo g_Angle = AccessTools.PropertyGetter(typeof (VehiclePawn), "Angle");
  public readonly MethodInfo g_Rot4_AsAngle = AccessTools.PropertyGetter(typeof (Rot4), "AsAngle");
  public readonly MethodInfo g_Rot8_AsAngle = AccessTools.PropertyGetter(typeof (Rot8), "AsAngle");
  public readonly MethodInfo m_FullAngle = (MethodInfoCache.\u003C\u003EO.\u003C40\u003E__get_FullAngle ?? (MethodInfoCache.\u003C\u003EO.\u003C40\u003E__get_FullAngle = new Func<VehiclePawn, float>(VehicleMapUtility.get_FullAngle))).Method;
  public readonly MethodInfo m_FullAngleQuat = (MethodInfoCache.\u003C\u003EO.\u003C41\u003E__get_FullAngleQuat ?? (MethodInfoCache.\u003C\u003EO.\u003C41\u003E__get_FullAngleQuat = new Func<VehiclePawn, Quaternion>(VehicleMapUtility.get_FullAngleQuat))).Method;
  public readonly MethodInfo m_ExtraAngle = (MethodInfoCache.\u003C\u003EO.\u003C42\u003E__get_ExtraAngle ?? (MethodInfoCache.\u003C\u003EO.\u003C42\u003E__get_ExtraAngle = new Func<VehiclePawn, float>(VehicleMapUtility.get_ExtraAngle))).Method;
  public readonly MethodInfo m_FlipAngle = (MethodInfoCache.\u003C\u003EO.\u003C43\u003E__FlipAngle ?? (MethodInfoCache.\u003C\u003EO.\u003C43\u003E__FlipAngle = new Func<float, VehiclePawn, float>(VehicleMapUtility.FlipAngle))).Method;
  public readonly MethodInfo m_RotatePoint = (MethodInfoCache.\u003C\u003EO.\u003C44\u003E__RotatePoint ?? (MethodInfoCache.\u003C\u003EO.\u003C44\u003E__RotatePoint = new Func<Vector3, Vector3, float, Vector3>(Ext_Math.RotatePoint))).Method;
  public readonly MethodInfo g_Rot4_AsQuat = AccessTools.PropertyGetter(typeof (Rot4), "AsQuat");
  public readonly MethodInfo m_Rot8_AsQuatRef = AccessTools.Method(typeof (Rot8Utility), "AsQuat", new Type[1]
  {
    typeof (Rot8).MakeByRefType()
  }, (Type[]) null);
  public readonly MethodInfo m_Rot4_Rotate = AccessTools.Method(typeof (Rot4), "Rotate", (Type[]) null, (Type[]) null);
  public readonly MethodInfo m_Rot8_Rotate = (MethodInfoCache.\u003C\u003EO.\u003C45\u003E__Rotate ?? (MethodInfoCache.\u003C\u003EO.\u003C45\u003E__Rotate = new \u003C\u003EA\u007B00000001\u007D<Rot4, RotationDirection>(Rot8Utility.Rotate))).Method;
  public readonly MethodInfo g_Quaternion_identity = AccessTools.PropertyGetter(typeof (Quaternion), "identity");
  public readonly MethodInfo o_Quaternion_Multiply = AccessTools.Method(typeof (Quaternion), "op_Multiply", new Type[2]
  {
    typeof (Quaternion),
    typeof (Quaternion)
  }, (Type[]) null);
  public readonly MethodInfo m_GenDraw_DrawFieldEdges1 = (MethodInfoCache.\u003C\u003EO.\u003C46\u003E__DrawFieldEdges ?? (MethodInfoCache.\u003C\u003EO.\u003C46\u003E__DrawFieldEdges = new Action<List<IntVec3>, int>(GenDraw.DrawFieldEdges))).Method;
  public readonly MethodInfo m_GenDraw_DrawFieldEdges2 = (MethodInfoCache.\u003C\u003EO.\u003C47\u003E__DrawFieldEdges ?? (MethodInfoCache.\u003C\u003EO.\u003C47\u003E__DrawFieldEdges = new Action<List<IntVec3>, Color, float?, HashSet<IntVec3>, int>(GenDraw.DrawFieldEdges))).Method;
  public readonly MethodInfo m_GenDrawOnVehicle_DrawFieldEdges1 = (MethodInfoCache.\u003C\u003EO.\u003C48\u003E__DrawFieldEdges ?? (MethodInfoCache.\u003C\u003EO.\u003C48\u003E__DrawFieldEdges = new Action<List<IntVec3>, int, Map>(GenDrawOnVehicle.DrawFieldEdges))).Method;
  public readonly MethodInfo m_GenDrawOnVehicle_DrawFieldEdges2 = (MethodInfoCache.\u003C\u003EO.\u003C49\u003E__DrawFieldEdges ?? (MethodInfoCache.\u003C\u003EO.\u003C49\u003E__DrawFieldEdges = new Action<List<IntVec3>, Color, float?, HashSet<IntVec3>, int, Map>(GenDrawOnVehicle.DrawFieldEdges))).Method;
  public readonly MethodInfo g_Designator_Map = AccessTools.PropertyGetter(typeof (Designator), "Map");
  public readonly MethodInfo g_Thing_Rotation = AccessTools.PropertyGetter(typeof (Thing), "Rotation");
  public readonly MethodInfo m_RotationForPrint = (MethodInfoCache.\u003C\u003EO.\u003C50\u003E__RotationForPrint ?? (MethodInfoCache.\u003C\u003EO.\u003C50\u003E__RotationForPrint = new Func<Thing, Rot4>(VehicleMapUtility.RotationForPrint))).Method;
  public readonly MethodInfo g_Thing_DrawPos = AccessTools.PropertyGetter(typeof (Thing), "DrawPos");
  public readonly MethodInfo m_GenThing_TrueCenter1 = (MethodInfoCache.\u003C\u003EO.\u003C51\u003E__TrueCenter ?? (MethodInfoCache.\u003C\u003EO.\u003C51\u003E__TrueCenter = new Func<Thing, Vector3>(GenThing.TrueCenter))).Method;
  public readonly MethodInfo m_GenThing_TrueCenter2 = (MethodInfoCache.\u003C\u003EO.\u003C52\u003E__TrueCenter ?? (MethodInfoCache.\u003C\u003EO.\u003C52\u003E__TrueCenter = new Func<IntVec3, Rot4, IntVec2, float, Vector3>(GenThing.TrueCenter))).Method;
  public readonly MethodInfo m_RotateForPrintNegate = (MethodInfoCache.\u003C\u003EO.\u003C53\u003E__RotateForPrintNegate ?? (MethodInfoCache.\u003C\u003EO.\u003C53\u003E__RotateForPrintNegate = new Func<Vector3, Vector3>(VehicleMapUtility.RotateForPrintNegate))).Method;
  public readonly MethodInfo m_ShouldLinkWith = AccessTools.Method(typeof (Graphic_Linked), "ShouldLinkWith", (Type[]) null, (Type[]) null);
  public readonly MethodInfo m_ShouldLinkWithOrig = (MethodInfoCache.\u003C\u003EO.\u003C54\u003E__ShouldLinkWith ?? (MethodInfoCache.\u003C\u003EO.\u003C54\u003E__ShouldLinkWith = new Func<Graphic_Linked, IntVec3, Thing, bool>(Patch_Graphic_Linked_ShouldLinkWith.ShouldLinkWith))).Method;
  public readonly MethodInfo m_GenSight_LineOfSightToThing = GenSight.LineOfSightToThing.Method;
  public readonly MethodInfo m_GenSightOnVehicle_LineOfSightToThing = (MethodInfoCache.\u003C\u003EO.\u003C56\u003E__LineOfSightToThing ?? (MethodInfoCache.\u003C\u003EO.\u003C56\u003E__LineOfSightToThing = new Func<IntVec3, Thing, Map, bool, Func<IntVec3, bool>, bool>(GenSightOnVehicle.LineOfSightToThing))).Method;
  public readonly MethodInfo m_GenSight_LineOfSight1 = (MethodInfoCache.\u003C\u003EO.\u003C57\u003E__LineOfSight ?? (MethodInfoCache.\u003C\u003EO.\u003C57\u003E__LineOfSight = new Func<IntVec3, IntVec3, Map, bool>(GenSight.LineOfSight))).Method;
  public readonly MethodInfo m_GenSight_LineOfSight2 = (MethodInfoCache.\u003C\u003EO.\u003C58\u003E__LineOfSight ?? (MethodInfoCache.\u003C\u003EO.\u003C58\u003E__LineOfSight = new Func<IntVec3, IntVec3, Map, bool, Func<IntVec3, bool>, int, int, bool>(GenSight.LineOfSight))).Method;
  public readonly MethodInfo m_GenSightOnVehicle_LineOfSight1 = (MethodInfoCache.\u003C\u003EO.\u003C59\u003E__LineOfSight ?? (MethodInfoCache.\u003C\u003EO.\u003C59\u003E__LineOfSight = new Func<IntVec3, IntVec3, Map, bool>(GenSightOnVehicle.LineOfSight))).Method;
  public readonly MethodInfo m_GenSightOnVehicle_LineOfSight2 = (MethodInfoCache.\u003C\u003EO.\u003C60\u003E__LineOfSight ?? (MethodInfoCache.\u003C\u003EO.\u003C60\u003E__LineOfSight = new Func<IntVec3, IntVec3, Map, bool, Func<IntVec3, bool>, int, int, bool>(GenSightOnVehicle.LineOfSight))).Method;
  public readonly MethodInfo m_GenSight_LineOfSightToEdges = GenSight.LineOfSightToEdges.Method;
  public readonly MethodInfo m_GenSightOnVehicle_LineOfSightToEdges = (MethodInfoCache.\u003C\u003EO.\u003C62\u003E__LineOfSightToEdges ?? (MethodInfoCache.\u003C\u003EO.\u003C62\u003E__LineOfSightToEdges = new Func<IntVec3, IntVec3, Map, bool, Func<IntVec3, bool>, bool>(GenSightOnVehicle.LineOfSightToEdges))).Method;
  public readonly MethodInfo m_GenUI_TargetsAtMouse = GenUI.TargetsAtMouse.Method;
  public readonly MethodInfo m_GenUIOnVehicle_TargetsAtMouse = GenUIOnVehicle.TargetsAtMouse.Method;
  public readonly MethodInfo m_Matrix4x4_SetTRS = AccessTools.Method(typeof (Matrix4x4), "SetTRS", (Type[]) null, (Type[]) null);
  public readonly MethodInfo m_SetTRSOnVehicle = (MethodInfoCache.\u003C\u003EO.\u003C65\u003E__SetTRSOnVehicle ?? (MethodInfoCache.\u003C\u003EO.\u003C65\u003E__SetTRSOnVehicle = new \u003C\u003EA\u007B00000001\u007D<Matrix4x4, Vector3, Quaternion, Vector3, Thing>(VehicleMapUtility.SetTRSOnVehicle))).Method;
  public readonly MethodInfo m_CanBeSeenOverFast = (MethodInfoCache.\u003C\u003EO.\u003C66\u003E__CanBeSeenOverFast ?? (MethodInfoCache.\u003C\u003EO.\u003C66\u003E__CanBeSeenOverFast = new Func<IntVec3, Map, bool>(GenGrid.CanBeSeenOverFast))).Method;
  public readonly MethodInfo m_CanBeSeenOverOnVehicleFast = (MethodInfoCache.\u003C\u003EO.\u003C67\u003E__CanBeSeenOverOnVehicleFast ?? (MethodInfoCache.\u003C\u003EO.\u003C67\u003E__CanBeSeenOverOnVehicleFast = new Func<IntVec3, Map, bool>(GenSightOnVehicle.CanBeSeenOverOnVehicleFast))).Method;
  public readonly MethodInfo g_Rot4_FacingCell = AccessTools.PropertyGetter(typeof (Rot4), "FacingCell");
  public readonly MethodInfo g_Rot8_FacingCell = AccessTools.PropertyGetter(typeof (Rot8), "FacingCell");
  public readonly MethodInfo g_Rot4_RighthandCell = AccessTools.PropertyGetter(typeof (Rot4), "RighthandCell");
  public readonly MethodInfo m_Rot8Utility_RighthandCell = (MethodInfoCache.\u003C\u003EO.\u003C68\u003E__RighthandCell ?? (MethodInfoCache.\u003C\u003EO.\u003C68\u003E__RighthandCell = new \u003C\u003EF\u007B00000001\u007D<Rot4, IntVec3>(Rot8Utility.RighthandCell))).Method;
  public readonly MethodInfo m_ToIntVec3 = (MethodInfoCache.\u003C\u003EO.\u003C69\u003E__ToIntVec3 ?? (MethodInfoCache.\u003C\u003EO.\u003C69\u003E__ToIntVec3 = new Func<Vector3, IntVec3>(IntVec3Utility.ToIntVec3))).Method;
  public readonly MethodInfo m_IntVec3_ToVector3 = AccessTools.Method(typeof (IntVec3), "ToVector3", (Type[]) null, (Type[]) null);
  public readonly MethodInfo m_IntVec3_ToVector3Shifted = AccessTools.Method(typeof (IntVec3), "ToVector3Shifted", (Type[]) null, (Type[]) null);
  public readonly MethodInfo m_IntVec3_ToVector3ShiftedWithAltitude = AccessTools.Method(typeof (IntVec3), "ToVector3ShiftedWithAltitude", new Type[1]
  {
    typeof (float)
  }, (Type[]) null);
  public readonly MethodInfo m_Altitudes_AltitudeFor = (MethodInfoCache.\u003C\u003EO.\u003C70\u003E__AltitudeFor ?? (MethodInfoCache.\u003C\u003EO.\u003C70\u003E__AltitudeFor = new Func<AltitudeLayer, float>(Altitudes.AltitudeFor))).Method;
  public readonly MethodInfo m_Rot8Utility_ToFundVector3 = (MethodInfoCache.\u003C\u003EO.\u003C71\u003E__ToFundVector3 ?? (MethodInfoCache.\u003C\u003EO.\u003C71\u003E__ToFundVector3 = new \u003C\u003EF\u007B00000001\u007D<IntVec3, Vector3>(Rot8Utility.ToFundVector3))).Method;
  public readonly MethodInfo m_CellRect_ClipInsideMap = AccessTools.Method(typeof (CellRect), "ClipInsideMap", (Type[]) null, (Type[]) null);
  public readonly MethodInfo m_ClipInsideVehicleMap = (MethodInfoCache.\u003C\u003EO.\u003C72\u003E__ClipInsideVehicleMap ?? (MethodInfoCache.\u003C\u003EO.\u003C72\u003E__ClipInsideVehicleMap = new \u003C\u003EF\u007B00000001\u007D<CellRect, Map, CellRect>(VehicleMapUtility.ClipInsideVehicleMap))).Method;
  public readonly MethodInfo m_FocusedDrawPosOffset = (MethodInfoCache.\u003C\u003EO.\u003C73\u003E__FocusedDrawPosOffset ?? (MethodInfoCache.\u003C\u003EO.\u003C73\u003E__FocusedDrawPosOffset = new Func<Vector3, Vector3>(VehicleMapUtility.FocusedDrawPosOffset))).Method;
  public readonly MethodInfo m_SelectedDrawPosOffset = (MethodInfoCache.\u003C\u003EO.\u003C74\u003E__SelectedDrawPosOffset ?? (MethodInfoCache.\u003C\u003EO.\u003C74\u003E__SelectedDrawPosOffset = new Func<Vector3, IntVec3, Vector3>(VehicleMapUtility.SelectedDrawPosOffset))).Method;
  public readonly MethodInfo m_FocusedOrSelectedDrawPosOffset = (MethodInfoCache.\u003C\u003EO.\u003C75\u003E__FocusedOrSelectedDrawPosOffset ?? (MethodInfoCache.\u003C\u003EO.\u003C75\u003E__FocusedOrSelectedDrawPosOffset = new Func<Vector3, IntVec3, Vector3>(VehicleMapUtility.FocusedOrSelectedDrawPosOffset))).Method;
  public readonly MethodInfo g_Rot4_AsVector2 = AccessTools.PropertyGetter(typeof (Rot4), "AsVector2");
  public readonly MethodInfo m_AsFundVector2 = (MethodInfoCache.\u003C\u003EO.\u003C76\u003E__AsFundVector2 ?? (MethodInfoCache.\u003C\u003EO.\u003C76\u003E__AsFundVector2 = new \u003C\u003EF\u007B00000001\u007D<Rot8, Vector3>(Rot8Utility.AsFundVector2))).Method;
  public readonly MethodInfo m_Roofed = AccessTools.Method(typeof (RoofGrid), "Roofed", new Type[1]
  {
    typeof (IntVec3)
  }, (Type[]) null);
  public readonly MethodInfo m_Roofed2 = (MethodInfoCache.\u003C\u003EO.\u003C77\u003E__Roofed ?? (MethodInfoCache.\u003C\u003EO.\u003C77\u003E__Roofed = new Func<IntVec3, Map, bool>(GridsUtility.Roofed))).Method;
  public readonly MethodInfo m_RoofedAcrossMaps = (MethodInfoCache.\u003C\u003EO.\u003C78\u003E__RoofedAcrossMaps ?? (MethodInfoCache.\u003C\u003EO.\u003C78\u003E__RoofedAcrossMaps = new Func<RoofGrid, IntVec3, bool>(VehicleMapUtility.RoofedAcrossMaps))).Method;
  public readonly MethodInfo m_RoofedAcrossMaps2 = (MethodInfoCache.\u003C\u003EO.\u003C79\u003E__RoofedAcrossMaps ?? (MethodInfoCache.\u003C\u003EO.\u003C79\u003E__RoofedAcrossMaps = new Func<IntVec3, Map, bool>(VehicleMapUtility.RoofedAcrossMaps))).Method;
  public readonly MethodInfo m_GetThingList = (MethodInfoCache.\u003C\u003EO.\u003C80\u003E__GetThingList ?? (MethodInfoCache.\u003C\u003EO.\u003C80\u003E__GetThingList = new Func<IntVec3, Map, List<Thing>>(GridsUtility.GetThingList))).Method;
  public readonly MethodInfo m_GetThingListAcrossMaps = (MethodInfoCache.\u003C\u003EO.\u003C81\u003E__GetThingListAcrossMaps ?? (MethodInfoCache.\u003C\u003EO.\u003C81\u003E__GetThingListAcrossMaps = new Func<IntVec3, Map, List<Thing>>(VehicleMapUtility.GetThingListAcrossMaps))).Method;
  public readonly MethodInfo m_AddColonistBuildingList = (MethodInfoCache.\u003C\u003EO.\u003C82\u003E__AddColonistBuildingList ?? (MethodInfoCache.\u003C\u003EO.\u003C82\u003E__AddColonistBuildingList = new Func<List<Building>, Thing, List<Building>>(VehicleMapUtility.AddColonistBuildingList))).Method;
  public readonly MethodInfo m_PrintExtraRotation = (MethodInfoCache.\u003C\u003EO.\u003C83\u003E__PrintExtraRotation ?? (MethodInfoCache.\u003C\u003EO.\u003C83\u003E__PrintExtraRotation = new Func<Thing, float>(VehicleMapUtility.PrintExtraRotation))).Method;
  public readonly MethodInfo m_Vector3Utility_WithY = (MethodInfoCache.\u003C\u003EO.\u003C84\u003E__WithY ?? (MethodInfoCache.\u003C\u003EO.\u003C84\u003E__WithY = new Func<Vector3, float, Vector3>(Vector3Utility.WithY))).Method;
  public readonly MethodInfo m_TargetCellOnBaseMap = (MethodInfoCache.\u003C\u003EO.\u003C85\u003E__TargetCellOnBaseMap ?? (MethodInfoCache.\u003C\u003EO.\u003C85\u003E__TargetCellOnBaseMap = new \u003C\u003EF\u007B00000001\u007D<LocalTargetInfo, Thing, IntVec3>(TargetMapUtility.TargetCellOnBaseMap))).Method;
  public readonly MethodInfo m_PositionOnTargetMap = (MethodInfoCache.\u003C\u003EO.\u003C86\u003E__get_PositionOnTargetMap ?? (MethodInfoCache.\u003C\u003EO.\u003C86\u003E__get_PositionOnTargetMap = new Func<Thing, IntVec3>(TargetMapUtility.get_PositionOnTargetMap))).Method;
  public readonly MethodInfo m_BreadthFirstTraverse = (MethodInfoCache.\u003C\u003EO.\u003C87\u003E__BreadthFirstTraverse ?? (MethodInfoCache.\u003C\u003EO.\u003C87\u003E__BreadthFirstTraverse = new Action<Region, RegionEntryPredicate, RegionProcessor, int, RegionType>(RegionTraverser.BreadthFirstTraverse))).Method;
  public readonly MethodInfo m_BreadthFirstTraverseAcrossMaps = (MethodInfoCache.\u003C\u003EO.\u003C88\u003E__BreadthFirstTraverse ?? (MethodInfoCache.\u003C\u003EO.\u003C88\u003E__BreadthFirstTraverse = new Action<Region, RegionEntryPredicate, RegionProcessor, int, RegionType>(RegionTraverserAcrossMaps.BreadthFirstTraverse))).Method;
  public readonly MethodInfo m_IsForbidden = (MethodInfoCache.\u003C\u003EO.\u003C89\u003E__IsForbidden ?? (MethodInfoCache.\u003C\u003EO.\u003C89\u003E__IsForbidden = new Func<IntVec3, Pawn, bool>(ForbidUtility.IsForbidden))).Method;
  public readonly MethodInfo m_CrossMapIsForbidden1 = (MethodInfoCache.\u003C\u003EO.\u003C90\u003E__IsForbidden ?? (MethodInfoCache.\u003C\u003EO.\u003C90\u003E__IsForbidden = new Func<IntVec3, Pawn, Thing, bool>(CrossMapForbidUtility.IsForbidden))).Method;
  public readonly MethodInfo m_CrossMapIsForbidden2 = (MethodInfoCache.\u003C\u003EO.\u003C91\u003E__IsForbidden ?? (MethodInfoCache.\u003C\u003EO.\u003C91\u003E__IsForbidden = new Func<IntVec3, Pawn, Map, bool>(CrossMapForbidUtility.IsForbidden))).Method;
  public readonly MethodInfo m_AllInventoryItems = (MethodInfoCache.\u003C\u003EO.\u003C92\u003E__AllInventoryItems ?? (MethodInfoCache.\u003C\u003EO.\u003C92\u003E__AllInventoryItems = new Func<Caravan, List<Thing>>(CaravanInventoryUtility.AllInventoryItems))).Method;
  public readonly MethodInfo m_AllInventoryItems_Original = (MethodInfoCache.\u003C\u003EO.\u003C93\u003E__AllInventoryItems ?? (MethodInfoCache.\u003C\u003EO.\u003C93\u003E__AllInventoryItems = new Func<Caravan, List<Thing>>(Patch_CaravanInventoryUtility_AllInventoryItems.AllInventoryItems))).Method;
  public readonly MethodInfo m_RotatedBy = (MethodInfoCache.\u003C\u003EO.\u003C94\u003E__RotatedBy ?? (MethodInfoCache.\u003C\u003EO.\u003C94\u003E__RotatedBy = new Func<Vector3, float, Vector3>(Vector3Utility.RotatedBy))).Method;
  public readonly MethodInfo g_AllPawnsSpawned = AccessTools.PropertyGetter(typeof (MapPawns), "AllPawnsSpawned");
  public readonly MethodInfo m_AllPawnsSpawned_Reverse = (MethodInfoCache.\u003C\u003EO.\u003C95\u003E__AllPawnsSpawned ?? (MethodInfoCache.\u003C\u003EO.\u003C95\u003E__AllPawnsSpawned = new Func<MapPawns, List<Pawn>>(Patch_MapPawns_AllPawnsSpawned.AllPawnsSpawned))).Method;
  public readonly MethodInfo g_AllPawns = AccessTools.PropertyGetter(typeof (MapPawns), "AllPawns");
  public readonly MethodInfo m_AllPawns_Reverse = (MethodInfoCache.\u003C\u003EO.\u003C96\u003E__AllPawns ?? (MethodInfoCache.\u003C\u003EO.\u003C96\u003E__AllPawns = new Func<MapPawns, List<Pawn>>(Patch_MapPawns_AllPawns.AllPawns))).Method;
  public readonly MethodInfo g_Vector3_up = AccessTools.PropertyGetter(typeof (Vector3), "up");
  public readonly MethodInfo m_Quaternion_AngleAxis = (MethodInfoCache.\u003C\u003EO.\u003C97\u003E__AngleAxis ?? (MethodInfoCache.\u003C\u003EO.\u003C97\u003E__AngleAxis = new Func<float, Vector3, Quaternion>(Quaternion.AngleAxis))).Method;

  public static MethodInfoCache CachedMethodInfo
  {
    get
    {
      MethodInfoCache target;
      if (!MethodInfoCache.cacheInt.TryGetTarget(out target))
      {
        MethodInfoCache.cacheInt.SetTarget(target = new MethodInfoCache());
        bool flag = false;
        foreach (FieldInfo declaredField in AccessTools.GetDeclaredFields(typeof (MethodInfoCache)))
        {
          if (!declaredField.IsStatic && declaredField.GetValue((object) target) == null)
          {
            flag = true;
            break;
          }
        }
        if (flag)
          VMF_Log.Error("MethodInfoCache failed to cache all MethodInfos. This may cause errors in some Harmony patches.");
      }
      return target;
    }
  }
}
