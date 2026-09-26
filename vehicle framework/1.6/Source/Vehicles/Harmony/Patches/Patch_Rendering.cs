// Decompiled with JetBrains decompiler
// Type: Vehicles.Patch_Rendering
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using RimWorld;
using SmashTools;
using SmashTools.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Vehicles.Rendering;
using Vehicles.World;
using Verse;

#nullable disable
namespace Vehicles;

internal class Patch_Rendering : IPatchCategory
{
  public const float DistFromMouse = 26f;
  public const float LabelColumnWidth = 130f;
  public const float InfoColumnWidth = 170f;
  public const float WindowPadding = 12f;
  public const float ColumnPadding = 12f;
  private const float LineHeight = 24f;
  private const float ThingIconSize = 22f;
  private const float WindowWidth = 336f;
  private static readonly List<Pawn> TmpPawns = new List<Pawn>();
  private static readonly FastInvokeHandler RenderPulsingOverlayInvoker = MethodInvoker.GetHandler(AccessTools.Method(typeof (OverlayDrawer), "RenderPulsingOverlay", new System.Type[4]
  {
    typeof (Thing),
    typeof (Material),
    typeof (int),
    typeof (bool)
  }, (System.Type[]) null), false);

  PatchSequence IPatchCategory.PatchAt => PatchSequence.Async;

  void IPatchCategory.PatchMethods()
  {
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn_RotationTracker), "UpdateRotation", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Rendering), "UpdateVehicleRotation", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (ColonistBarColonistDrawer), "DrawIcons", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Rendering), "DrawIconsVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (ColonistBar), "CheckRecacheEntries", (System.Type[]) null, (System.Type[]) null), transpiler: new HarmonyMethod(typeof (Patch_Rendering), "CheckRecacheVehicleEntriesTranspiler", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (SelectionDrawer), "DrawSelectionBracketFor", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Rendering), "DrawSelectionBracketsVehicles", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (CellInspectorDrawer), "DrawThingRow", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Rendering), "CellInspectorDrawVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Pawn), "ProcessPostTickVisuals", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Rendering), "ProcessVehiclePostTickVisuals", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GhostDrawer), "DrawGhostThing", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Rendering), "DrawGhostVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (GenThing), "TrueCenter", new System.Type[1]
    {
      typeof (Thing)
    }, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Rendering), "TrueCenterVehicle", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (PawnRenderer), "ParallelGetPreRenderResults", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Rendering), "DisableCachingPawnOverlays", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (OverlayDrawer), "RenderOutOfFuelOverlay", (System.Type[]) null, (System.Type[]) null), new HarmonyMethod(typeof (Patch_Rendering), "RenderVehicleOutOfFuelOverlay", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Targeter), "TargeterOnGUI", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Rendering), "DrawTargeters", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Targeter), "ProcessInputEvents", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Rendering), "ProcessTargeterInputEvents", (System.Type[]) null));
    HarmonyPatcher.Patch((MethodBase) AccessTools.Method(typeof (Targeter), "TargeterUpdate", (System.Type[]) null, (System.Type[]) null), postfix: new HarmonyMethod(typeof (Patch_Rendering), "TargeterUpdate", (System.Type[]) null));
  }

  public static bool UpdateVehicleRotation(Pawn ___pawn)
  {
    if (!(___pawn is VehiclePawn vehiclePawn))
      return true;
    if (((Thing) vehiclePawn).Destroyed || vehiclePawn.jobs.HandlingFacing || !vehiclePawn.vehiclePather.Moving || vehiclePawn.vehiclePather.curPath == null || vehiclePawn.vehiclePather.curPath.NodesLeft < 1)
      return false;
    vehiclePawn.UpdateRotationAndAngle();
    return false;
  }

  public static void DrawIconsVehicles(Rect rect, Pawn colonist)
  {
    Texture2D texture2D;
    if (colonist.Dead || !(((Thing) colonist).ParentHolder is VehicleRoleHandler parentHolder) || !VehicleTex.CachedTextureIcons.TryGetValue(parentHolder.vehicle.VehicleDef, out texture2D) || !Object.op_Implicit((Object) texture2D))
      return;
    float num = 20f * Find.ColonistBar.Scale;
    Vector2 vector2;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2).\u002Ector((float) ((double) ((Rect) ref rect).xMax - (double) num - 1.0), (float) ((double) ((Rect) ref rect).yMax - (double) num - 1.0));
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(vector2.x, vector2.y, num, num);
    GUI.DrawTexture(rect1, (Texture) texture2D);
    TooltipHandler.TipRegion(rect1, TipSignal.op_Implicit(TranslatorFormattedStringExtensions.Translate("VF_ActivityIconOnBoardShip", NamedArgument.op_Implicit(((Entity) parentHolder.vehicle).Label))));
    vector2.x += num;
  }

  public static IEnumerable<CodeInstruction> CheckRecacheVehicleEntriesTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    MethodInfo clearCachedEntriesMethod = AccessTools.Method(typeof (List<int>), "Clear", (System.Type[]) null, (System.Type[]) null);
    MethodInfo freeColonistsGetter = AccessTools.PropertyGetter(typeof (MapPawns), "FreeColonists");
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (CodeInstructionExtensions.Calls(instruction, freeColonistsGetter))
      {
        yield return instruction;
        instruction = instructionList[++i];
        yield return instruction;
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Ldsfld, (object) AccessTools.Field(typeof (ColonistBar), "tmpMaps"));
        yield return new CodeInstruction(OpCodes.Ldloc_1, (object) null);
        yield return new CodeInstruction(OpCodes.Callvirt, (object) AccessTools.PropertyGetter(typeof (List<Map>), "Item"));
        yield return new CodeInstruction(OpCodes.Ldsfld, (object) AccessTools.Field(typeof (ColonistBar), "tmpPawns"));
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_Rendering), "RecacheLocalVehicleEntries", (System.Type[]) null, (System.Type[]) null));
      }
      else if (CodeInstructionExtensions.Calls(instruction, clearCachedEntriesMethod))
      {
        yield return instruction;
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Ldfld, (object) AccessTools.Field(typeof (ColonistBar), "cachedEntries"));
        yield return new CodeInstruction(OpCodes.Ldloca, (object) 0);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (Patch_Rendering), "RecacheAerialVehicleEntries", (System.Type[]) null, (System.Type[]) null));
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static void RecacheLocalVehicleEntries(Map map, List<Pawn> tmpPawns)
  {
    foreach (VehiclePawn allClaimant in map.GetDetachedMapComponent<VehiclePositionManager>().AllClaimants)
    {
      if (((Thing) allClaimant).Faction == Faction.OfPlayer && allClaimant.AllPawnsAboard.Count > 0)
      {
        foreach (Pawn pawn in allClaimant.AllPawnsAboard)
        {
          if (pawn != null && pawn.IsColonist)
            tmpPawns.Add(pawn);
        }
      }
    }
  }

  private static void RecacheAerialVehicleEntries(
    List<ColonistBar.Entry> cachedEntries,
    ref int group)
  {
    foreach (AerialVehicleInFlight aerialVehicle in Find.World.GetComponent<VehicleWorldObjectsHolder>().AerialVehicles)
    {
      if (aerialVehicle.IsPlayerControlled)
      {
        using (new ClearOnDispose<Pawn>((ICollection<Pawn>) Patch_Rendering.TmpPawns))
        {
          foreach (Pawn pawn in aerialVehicle.Vehicle.AllPawnsAboard)
          {
            if (pawn != null && pawn.IsColonist)
              Patch_Rendering.TmpPawns.Add(pawn);
          }
          PlayerPawnsDisplayOrderUtility.Sort(Patch_Rendering.TmpPawns);
          foreach (Pawn tmpPawn in Patch_Rendering.TmpPawns)
            cachedEntries.Add(new ColonistBar.Entry(tmpPawn, (Map) null, group));
          ++group;
        }
      }
    }
  }

  public static bool DrawSelectionBracketsVehicles(object obj, Material overrideMat)
  {
    switch (obj)
    {
      case VehiclePawn vehiclePawn3:
label_3:
        VehiclePawn vehiclePawn1 = vehiclePawn3;
        if (vehiclePawn1 == null)
          return true;
        Vector3[] vector3Array = new Vector3[4];
        float num1 = vehiclePawn1.Angle + vehiclePawn1.Transform.rotation;
        Vector3[] bracketLocs = vector3Array;
        VehiclePawn vehiclePawn2 = vehiclePawn1;
        Vector3 drawPos = ((Thing) vehiclePawn1).DrawPos;
        IntVec2 rotatedSize = ((Thing) vehiclePawn1).RotatedSize;
        Vector2 vector2 = ((IntVec2) ref rotatedSize).ToVector2();
        Dictionary<object, float> selectTimes = SelectionDrawer.SelectTimes;
        Vector2 one = Vector2.one;
        double pawnAngle = (double) num1;
        Ext_Pawn.CalculateSelectionBracketPositionsWorldForMultiCellPawns<object>(bracketLocs, (object) vehiclePawn2, drawPos, vector2, selectTimes, one, (float) pawnAngle);
        int num2 = Mathf.CeilToInt(num1);
        for (int index = 0; index < 4; ++index)
        {
          Quaternion quaternion = Quaternion.AngleAxis((float) num2, Vector3.up);
          Material material = Object.op_Implicit((Object) overrideMat) ? overrideMat : MaterialPresets.SelectionBracketMat;
          Graphics.DrawMesh(MeshPool.plane10, vector3Array[index], quaternion, material, 0);
          num2 -= 90;
        }
        return false;
      case VehicleBuilding vehicleBuilding:
        vehiclePawn3 = vehicleBuilding.vehicle;
        goto label_3;
      default:
        vehiclePawn3 = (VehiclePawn) null;
        goto label_3;
    }
  }

  public static bool CellInspectorDrawVehicle(Thing thing, ref int ___numLines)
  {
    if (!(thing is VehiclePawn vehicle))
      return true;
    float num = (float) ___numLines * 24f;
    List<object> selectedObjects = Find.Selector.SelectedObjects;
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(12f, num + 12f, 312f, 24f);
    if (selectedObjects.Contains((object) thing))
      Widgets.DrawHighlight(rect1);
    else if (___numLines % 2 == 1)
      Widgets.DrawLightHighlight(rect1);
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(24f, (float) ((double) num + 12.0 + 1.0), 22f, 22f);
    Rect rect2 = rect1;
    Rot8? rot = new Rot8?();
    VehicleGraphics.DrawVehicle(rect2, vehicle, rot);
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(58f, num + 12f, 370f, 24f);
    Widgets.Label(rect1, ((Entity) thing).LabelMouseover);
    ++___numLines;
    return false;
  }

  public static bool ProcessVehiclePostTickVisuals(
    Pawn __instance,
    int ticksPassed,
    CellRect viewRect)
  {
    if (!(__instance is VehiclePawn vehiclePawn))
      return true;
    vehiclePawn.ProcessPostTickVisuals(ticksPassed, viewRect);
    return false;
  }

  private static void DrawGhostVehicle(
    IntVec3 center,
    Rot8 rot,
    ThingDef thingDef,
    Graphic baseGraphic,
    Color ghostCol,
    AltitudeLayer drawAltitude,
    Thing thing = null)
  {
    if (!(thingDef is VehicleBuildDef vehicleBuildDef))
      return;
    VehicleDef thingToSpawn = vehicleBuildDef.thingToSpawn;
    VehicleGhostUtility.DrawGhostOverlays(center, rot, thingToSpawn, baseGraphic, ghostCol, drawAltitude, thing);
  }

  private static bool TrueCenterVehicle(Thing t, ref Vector3 __result)
  {
    if (!(t is VehiclePawn vehiclePawn))
      return true;
    __result = vehiclePawn.TrueCenter();
    return false;
  }

  private static void DisableCachingPawnOverlays(Pawn ___pawn, ref bool disableCache)
  {
    if (!___pawn.InVehicle())
      return;
    disableCache = true;
  }

  private static bool RenderVehicleOutOfFuelOverlay(
    Thing t,
    OverlayDrawer __instance,
    Material ___OutOfFuelMat)
  {
    if (!(t is VehiclePawn vehiclePawn) || vehiclePawn.CompFueledTravel == null)
      return true;
    CompProperties_FueledTravel props = vehiclePawn.CompFueledTravel.Props;
    Material material = MaterialPool.MatFrom(Object.op_Implicit((Object) props.FuelIcon) ? props.FuelIcon : ((BuildableDef) ThingDefOf.Chemfuel).uiIcon, ShaderDatabase.MetaOverlay, Color.white);
    Patch_Rendering.RenderPulsingOverlayInvoker.Invoke((object) __instance, new object[4]
    {
      (object) t,
      (object) material,
      (object) 5,
      (object) false
    });
    Patch_Rendering.RenderPulsingOverlayInvoker.Invoke((object) __instance, new object[4]
    {
      (object) t,
      (object) ___OutOfFuelMat,
      (object) 5,
      (object) true
    });
    return false;
  }

  private static void DrawTargeters() => Targeters.OnGUITargeter();

  private static void ProcessTargeterInputEvents() => Targeters.ProcessTargeterInputEvent();

  private static void TargeterUpdate() => Targeters.UpdateTargeter();
}
