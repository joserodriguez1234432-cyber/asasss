// Decompiled with JetBrains decompiler
// Type: SmashTools.CameraController
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using RimWorld;
using SmashTools.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public static class CameraController
{
  private static MethodInfo getSunShadowsViewRect_MethodInfo;
  private static int lastViewRectGetFrame = -1;
  private static CellRect lastViewRect;
  private static float rootSize;
  private static Camera camera;

  private static bool Patched { get; set; }

  public static bool InUse { get; private set; }

  private static CellRect CurrentViewRect
  {
    get
    {
      if (Time.frameCount != CameraController.lastViewRectGetFrame)
      {
        CameraController.lastViewRect = new CellRect();
        float num = (float) UI.screenWidth / (float) UI.screenHeight;
        Vector3 position = ((Component) Find.Camera).transform.position;
        CameraController.lastViewRect.minX = Mathf.FloorToInt((float) ((double) position.x - (double) CameraController.rootSize * (double) num - 1.0));
        CameraController.lastViewRect.maxX = Mathf.CeilToInt(position.x + CameraController.rootSize * num);
        CameraController.lastViewRect.minZ = Mathf.FloorToInt((float) ((double) position.z - (double) CameraController.rootSize - 1.0));
        CameraController.lastViewRect.maxZ = Mathf.CeilToInt(position.z + CameraController.rootSize);
        CameraController.lastViewRectGetFrame = Time.frameCount;
      }
      return CameraController.lastViewRect;
    }
  }

  public static void Update(Vector3 position)
  {
    ((Component) CameraController.camera).transform.position = new Vector3(position.x, ((Component) CameraController.camera).transform.position.y, position.z);
  }

  public static void Start(Camera camera)
  {
    if (CameraController.InUse)
    {
      Log.Warning("Attempting to start CameraController when it's already in use.");
    }
    else
    {
      CameraController.InUse = true;
      CameraController.camera = camera;
      if (CameraController.Patched)
        return;
      CameraController.PatchOcclusionCulling();
    }
  }

  public static void Close()
  {
    CameraController.InUse = false;
    CameraController.camera = (Camera) null;
  }

  private static void PatchOcclusionCulling()
  {
    if (CameraController.Patched)
      return;
    CameraController.Patched = true;
    try
    {
      CameraController.getSunShadowsViewRect_MethodInfo = AccessTools.Method(typeof (MapDrawer), "GetSunShadowsViewRect", (Type[]) null, (Type[]) null);
      if (CameraController.getSunShadowsViewRect_MethodInfo == (MethodInfo) null)
        throw new NullReferenceException("MethodInfo fields");
      Messages.Message("Patching CameraController to exclude from occlusion culling.", MessageTypeDefOf.NeutralEvent, true);
      HarmonyPatcher.Harmony.Patch((MethodBase) AccessTools.Method(typeof (DynamicDrawManager), "DrawDynamicThings", (Type[]) null, (Type[]) null), (HarmonyMethod) null, (HarmonyMethod) null, new HarmonyMethod(typeof (CameraController), "CameraViewRenderInRectTranspiler_Thing_Runtime", (Type[]) null), (HarmonyMethod) null);
      HarmonyPatcher.Harmony.Patch((MethodBase) AccessTools.Method(typeof (CellRenderer), "RenderSpot", new Type[3]
      {
        typeof (Vector3),
        typeof (Material),
        typeof (float)
      }, (Type[]) null), (HarmonyMethod) null, (HarmonyMethod) null, new HarmonyMethod(typeof (CameraController), "CameraViewRenderInRectTranspiler_TerrainVector3_Runtime", (Type[]) null), (HarmonyMethod) null);
      HarmonyPatcher.Harmony.Patch((MethodBase) AccessTools.Method(typeof (CellRenderer), "RenderCell", new Type[2]
      {
        typeof (IntVec3),
        typeof (Material)
      }, (Type[]) null), (HarmonyMethod) null, (HarmonyMethod) null, new HarmonyMethod(typeof (CameraController), "CameraViewRenderInRectTranspiler_TerrainIntVec3_Runtime", (Type[]) null), (HarmonyMethod) null);
      HarmonyPatcher.Harmony.Patch((MethodBase) AccessTools.Method(typeof (MapDrawer), "DrawMapMesh", (Type[]) null, (Type[]) null), (HarmonyMethod) null, new HarmonyMethod(typeof (CameraController), "CameraViewRenderInRect_MapMesh_Runtime", (Type[]) null), (HarmonyMethod) null, (HarmonyMethod) null);
    }
    catch (Exception ex)
    {
      Log.Error($"Failed to patch CameraAttacher rect for occlusion culling. Attacher will not work outside main camera rect.\nException={ex}");
    }
  }

  private static IEnumerable<CodeInstruction> CameraViewRenderInRectTranspiler_Thing_Runtime(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionsList = instructions.ToList<CodeInstruction>();
    for (int i = 0; i < instructionsList.Count; ++i)
    {
      CodeInstruction codeInstruction = instructionsList[i];
      if (CodeInstructionExtensions.Calls(codeInstruction, AccessTools.Method(typeof (CellRect), "Contains", (Type[]) null, (Type[]) null)))
      {
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return new CodeInstruction(OpCodes.Ldloc_1, (object) null);
        yield return new CodeInstruction(OpCodes.Ldloc_S, (object) 5);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (CameraController), "CameraOrCameraViewContainPosition_Thing", (Type[]) null, (Type[]) null));
        codeInstruction = instructionsList[++i];
      }
      yield return codeInstruction;
    }
  }

  private static IEnumerable<CodeInstruction> CameraViewRenderInRectTranspiler_TerrainVector3_Runtime(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionsList = instructions.ToList<CodeInstruction>();
    for (int i = 0; i < instructionsList.Count; ++i)
    {
      CodeInstruction codeInstruction = instructionsList[i];
      if (CodeInstructionExtensions.Calls(codeInstruction, AccessTools.Method(typeof (CellRect), "Contains", (Type[]) null, (Type[]) null)))
      {
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return new CodeInstruction(OpCodes.Ldsfld, (object) AccessTools.Field(typeof (CellRenderer), "viewRect"));
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (CameraController), "CameraOrCameraViewContainPosition_Vector3", (Type[]) null, (Type[]) null));
        codeInstruction = instructionsList[++i];
      }
      yield return codeInstruction;
    }
  }

  private static IEnumerable<CodeInstruction> CameraViewRenderInRectTranspiler_TerrainIntVec3_Runtime(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionsList = instructions.ToList<CodeInstruction>();
    for (int i = 0; i < instructionsList.Count; ++i)
    {
      CodeInstruction codeInstruction = instructionsList[i];
      if (CodeInstructionExtensions.Calls(codeInstruction, AccessTools.Method(typeof (CellRect), "Contains", (Type[]) null, (Type[]) null)))
      {
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return new CodeInstruction(OpCodes.Pop, (object) null);
        yield return new CodeInstruction(OpCodes.Ldsfld, (object) AccessTools.Field(typeof (CellRenderer), "viewRect"));
        yield return new CodeInstruction(OpCodes.Ldarg_0, (object) null);
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (CameraController), "CameraOrCameraViewContainPosition_IntVec3", (Type[]) null, (Type[]) null));
        codeInstruction = instructionsList[++i];
      }
      yield return codeInstruction;
    }
  }

  private static void CameraViewRenderInRect_MapMesh_Runtime(
    MapDrawer __instance,
    Map ___map,
    Section[,] ___sections)
  {
    if (!CameraController.InUse)
      return;
    foreach (IntVec3 intVec3 in CameraController.VisibleSections(___map, CameraController.CurrentViewRect).CellsNoOverlap(CameraController.VisibleSections(___map, Find.CameraDriver.CurrentViewRect)))
      ___sections[intVec3.x, intVec3.z].DrawSection();
  }

  private static bool CameraOrCameraViewContainPosition_Thing(CellRect cellRect, Thing thing)
  {
    if (!CameraController.InUse)
      return ((CellRect) ref cellRect).Contains(thing.Position);
    if (((CellRect) ref cellRect).Contains(thing.Position))
      return true;
    CellRect currentViewRect = CameraController.CurrentViewRect;
    return ((CellRect) ref currentViewRect).Contains(IntVec3Utility.ToIntVec3(thing.DrawPos));
  }

  private static bool CameraOrCameraViewContainPosition_IntVec3(CellRect cellRect, IntVec3 cell)
  {
    return CameraController.CameraOrCameraViewContainPosition_Vector3(cellRect, ((IntVec3) ref cell).ToVector3ShiftedWithAltitude((AltitudeLayer) 39));
  }

  private static bool CameraOrCameraViewContainPosition_Vector3(CellRect cellRect, Vector3 loc)
  {
    IntVec3 intVec3 = IntVec3Utility.ToIntVec3(loc);
    if (!CameraController.InUse)
      return ((CellRect) ref cellRect).Contains(intVec3);
    if (((CellRect) ref cellRect).Contains(intVec3))
      return true;
    CellRect currentViewRect = CameraController.CurrentViewRect;
    return ((CellRect) ref currentViewRect).Contains(intVec3);
  }

  private static CellRect VisibleSections(Map map, CellRect viewRect)
  {
    CellRect cellRect = (CellRect) CameraController.getSunShadowsViewRect_MethodInfo.Invoke((object) map.mapDrawer, new object[1]
    {
      (object) viewRect
    });
    ((CellRect) ref cellRect).ClipInsideMap(map);
    IntVec2 intVec2_1 = CameraController.SectionCoordsAt(((CellRect) ref cellRect).Min);
    IntVec2 intVec2_2 = CameraController.SectionCoordsAt(((CellRect) ref cellRect).Max);
    return intVec2_2.x < intVec2_1.x || intVec2_2.z < intVec2_1.z ? CellRect.Empty : CellRect.FromLimits(intVec2_1.x, intVec2_1.z, intVec2_2.x, intVec2_2.z);
  }

  private static IntVec2 SectionCoordsAt(IntVec3 loc)
  {
    return new IntVec2(Mathf.FloorToInt((float) (loc.x / 17)), Mathf.FloorToInt((float) (loc.z / 17)));
  }
}
