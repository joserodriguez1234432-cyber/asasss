// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.Patch_Map_MapUpdate
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Vehicles;
using Vehicles.World;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof (Map), "MapUpdate")]
public static class Patch_Map_MapUpdate
{
  private static RenderTexture tmpRenderTex;
  public const float Altitude = 140f;
  public const int TextureSize = 2048 /*0x0800*/;
  public const float MeshSizeX = 200f;
  public static readonly Vector2 MeshSize = new Vector2(200f, 200f);
  private static Mesh mesh200;
  private static Material mat;
  private static Material skyMat;
  public static int lastRenderedTick = -1;
  private static readonly AccessTools.FieldRef<WorldCameraDriver, float> desiredAltitude = AccessTools.FieldRefAccess<WorldCameraDriver, float>(nameof (desiredAltitude));

  static Patch_Map_MapUpdate()
  {
    if (UnitTestDetector.IsTestingContext)
      return;
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      Patch_Map_MapUpdate.mesh200 = MeshPool.GridPlane(Patch_Map_MapUpdate.MeshSize);
      Patch_Map_MapUpdate.skyMat = SolidColorMaterials.NewSolidColorMaterial(Color.black, ShaderDatabase.SolidColor);
      Patch_Map_MapUpdate.skyMat.renderQueue = 3100;
    }));
  }

  public static void JumpTo(Vector3 pos, float altitude)
  {
    Find.WorldCameraDriver.JumpTo(pos);
    Find.WorldCameraDriver.altitude = altitude;
    Patch_Map_MapUpdate.desiredAltitude.Invoke(Find.WorldCameraDriver) = altitude;
    Find.WorldCameraDriver.Update();
  }

  [PatchLevel(Level.Safe)]
  public static void Postfix(Map __instance)
  {
    bool flag1 = Find.CurrentMap == __instance;
    VehiclePawnWithMap vehicle1;
    if (flag1 && __instance.IsVehicleMapOf(out vehicle1) && VehicleMapFramework.VehicleMapFramework.settings.drawPlanet && WorldRendererUtility.DrawingMap && !Find.World.renderer.RegenerateLayersIfDirtyInLongEvent())
    {
      VehicleMapSettings.ForceRotated forceRotated = VehicleMapFramework.VehicleMapFramework.settings.forceRotated;
      bool flag2 = forceRotated != VehicleMapSettings.ForceRotated.None;
      double num1;
      if (!flag2)
      {
        double rotation1 = (double) vehicle1.Transform.rotation;
        Rot4 rotation2 = ((Thing) vehicle1).Rotation;
        double asAngle = (double) ((Rot4) ref rotation2).AsAngle;
        num1 = rotation1 + asAngle;
      }
      else
      {
        Rot8 rot8 = new Rot8((byte) forceRotated);
        num1 = (double) ((Rot8) ref rot8).AsAngle;
      }
      float num2 = (float) num1;
      WorldObject orStashedVehicle = vehicle1.VehicleCaravanOrStashedVehicle;
      if ((GenTicks.TicksGame != Patch_Map_MapUpdate.lastRenderedTick || Find.TickManager.Paused) && Time.frameCount % 2 == 0 || Object.op_Implicit((Object) Patch_Map_MapUpdate.mat) && !Object.op_Implicit((Object) Patch_Map_MapUpdate.tmpRenderTex))
      {
        WorldObject worldObject = orStashedVehicle ?? GetWorldObject((IThingHolder) vehicle1);
        if (worldObject == null)
          return;
        Patch_Map_MapUpdate.lastRenderedTick = GenTicks.TicksGame;
        Find.World.renderer.wantedMode = (WorldRenderMode) 1;
        Patch_Map_MapUpdate.JumpTo(worldObject.DrawPos, 140f);
        WorldRendererUtility.UpdateGlobalShadersParams();
        ExpandableWorldObjectsUtility.ExpandableWorldObjectsUpdate();
        foreach (WorldDrawLayerBase worldDrawLayerBase in Find.World.renderer.AllVisibleDrawLayers.Where<WorldDrawLayerBase>((Func<WorldDrawLayerBase, bool>) (l => !(l is WorldDrawLayer_SingleTile) && !(l is WorldDrawLayer_Satellites))))
          worldDrawLayerBase.Render();
        Find.World.dynamicDrawManager.DrawDynamicWorldObjects();
        if (worldObject is VehicleCaravan vehicleCaravan1)
        {
          ((Caravan) vehicleCaravan1).gotoMote.RenderMote();
          vehicleCaravan1.vehiclePather?.curPath?.DrawPath((Caravan) vehicleCaravan1);
        }
        if (Object.op_Implicit((Object) Patch_Map_MapUpdate.tmpRenderTex))
          RenderTexture.ReleaseTemporary(Patch_Map_MapUpdate.tmpRenderTex);
        Patch_Map_MapUpdate.tmpRenderTex = RenderTexture.GetTemporary(2048 /*0x0800*/, 2048 /*0x0800*/);
        RenderTexture targetTexture = Find.WorldCamera.targetTexture;
        Find.WorldCamera.targetTexture = Patch_Map_MapUpdate.tmpRenderTex;
        Find.WorldCamera.orthographic = true;
        Find.WorldCamera.Render();
        Find.WorldCamera.targetTexture = targetTexture;
        Find.WorldCamera.orthographic = false;
        Find.World.renderer.wantedMode = (WorldRenderMode) 0;
        Find.CameraDriver.Update();
        if (!Object.op_Implicit((Object) Patch_Map_MapUpdate.mat))
          Patch_Map_MapUpdate.mat = MaterialPool.MatFrom(new MaterialRequest((Texture) Patch_Map_MapUpdate.tmpRenderTex));
        else
          Patch_Map_MapUpdate.mat.mainTexture = (Texture) Patch_Map_MapUpdate.tmpRenderTex;
        // ISSUE: variable of a compiler-generated type
        Patch_Map_MapUpdate.\u003C\u003Ec__DisplayClass12_0 cDisplayClass120;
        ref Patch_Map_MapUpdate.\u003C\u003Ec__DisplayClass12_0 local = ref cDisplayClass120;
        PlanetTile tile = __instance.Tile;
        PlanetLayer layer = ((PlanetTile) ref tile).Layer;
        // ISSUE: reference to a compiler-generated field
        local.planetLayer = layer;
        if (!((Thing) vehicle1).Spawned)
        {
          if (flag2)
          {
            vehicle1.FullRotation = new Rot8((byte) forceRotated);
          }
          else
          {
            float num3;
            switch (worldObject)
            {
              case VehicleCaravan vehicleCaravan2:
                WorldGrid worldGrid = Find.WorldGrid;
                PlanetTile nextTile = vehicleCaravan2.vehiclePather.NextTile;
                PlanetTile planetTile = ((PlanetTile) ref nextTile).Valid ? vehicleCaravan2.vehiclePather.NextTile : ((WorldObject) vehicleCaravan2).Tile;
                num3 = Patch_Map_MapUpdate.\u003CPostfix\u003Eg__AngleOnPlanetSurface\u007C12_1(worldGrid.GetTileCenter(planetTile), Find.WorldGrid.GetTileCenter(((WorldObject) vehicleCaravan2).Tile), ref cDisplayClass120);
                break;
              case Caravan caravan:
                num3 = Patch_Map_MapUpdate.\u003CPostfix\u003Eg__AngleOnPlanetSurface\u007C12_1(Find.WorldGrid.GetTileCenter(((PlanetTile) ref caravan.pather.nextTile).Valid ? caravan.pather.nextTile : ((WorldObject) caravan).Tile), Find.WorldGrid.GetTileCenter(((WorldObject) caravan).Tile), ref cDisplayClass120);
                break;
              case AerialVehicleInFlight aerialVehicleInFlight:
                num3 = Patch_Map_MapUpdate.\u003CPostfix\u003Eg__AngleOnPlanetSurface\u007C12_1(((WorldObject) aerialVehicleInFlight).DrawPos, aerialVehicleInFlight.position, ref cDisplayClass120);
                break;
              default:
                num3 = 0.0f;
                break;
            }
            num2 = num3;
            Rot4 rot4 = Rot4.FromAngleFlat(num2);
            if (orStashedVehicle != null)
            {
              foreach (VehiclePawn vehicle2 in VehicleCaravanHelper.get_Vehicles(orStashedVehicle))
                vehicle2.FullRotation = Rot8.op_Implicit(rot4);
            }
            else
              vehicle1.FullRotation = Rot8.op_Implicit(rot4);
          }
        }
      }
      Vector3 vector3_1;
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3_1).\u002Ector(Patch_Map_MapUpdate.MeshSize.x / 2f, 0.0f, Patch_Map_MapUpdate.MeshSize.y / 2f);
      Graphics.DrawMesh(Patch_Map_MapUpdate.mesh200, vector3_1, Quaternion.identity, Object.op_Implicit((Object) Patch_Map_MapUpdate.mat) ? Patch_Map_MapUpdate.mat : SolidColorMaterials.SimpleSolidColorMaterial(Color.black, false), 0);
      Patch_Map_MapUpdate.skyMat.color = GenColor.WithAlpha(Color.black, (float) ((1.0 - (double) vehicle1.VehicleMap.skyManager.CurSkyGlow) * 0.20000000298023224));
      Graphics.DrawMesh(Patch_Map_MapUpdate.mesh200, Vector3Utility.WithY(vector3_1, Altitudes.AltitudeFor((AltitudeLayer) 32 /*0x20*/)), Quaternion.identity, Patch_Map_MapUpdate.skyMat, 0);
      VehicleFormationComp component = orStashedVehicle?.GetComponent<VehicleFormationComp>();
      if (component != null)
      {
        Dictionary<VehiclePawn, VehicleFormationComp.DrawData> drawPositions = component.DrawPositions;
        foreach (VehiclePawn vehicle3 in VehicleCaravanHelper.get_Vehicles(orStashedVehicle))
        {
          if (!drawPositions.ContainsKey(vehicle3))
          {
            component.FindVehiclePosition(vehicle3);
            component.CenteredDrawPositions();
          }
          Vector3 vector3_2 = Vector3.op_Addition(vector3_1, Vector3Utility.RotatedBy(drawPositions[vehicle3].position, num2));
          VehiclePawn vehiclePawn = vehicle3;
          ref Vector3 local = ref vector3_2;
          Rot8 fullRotation1 = vehicle3.FullRotation;
          double num4 = (double) num2;
          Rot8 fullRotation2 = vehicle3.FullRotation;
          double asAngle = (double) ((Rot8) ref fullRotation2).AsAngle;
          double num5 = num4 - asAngle;
          vehiclePawn.DrawAt(ref local, fullRotation1, (float) num5);
        }
      }
      else
      {
        Vector3 vector3_3 = Vector3Utility.WithY(vector3_1, Altitudes.AltitudeFor((AltitudeLayer) 20));
        VehiclePawnWithMap vehiclePawnWithMap = vehicle1;
        ref Vector3 local = ref vector3_3;
        Rot8 fullRotation3 = vehicle1.FullRotation;
        double num6 = (double) num2;
        Rot8 fullRotation4 = vehicle1.FullRotation;
        double asAngle = (double) ((Rot8) ref fullRotation4).AsAngle;
        double num7 = num6 - asAngle;
        ((VehiclePawn) vehiclePawnWithMap).DrawAt(ref local, fullRotation3, (float) num7);
      }
    }
    else
    {
      if (!(Object.op_Implicit((Object) Patch_Map_MapUpdate.tmpRenderTex) & flag1))
        return;
      RenderTexture.ReleaseTemporary(Patch_Map_MapUpdate.tmpRenderTex);
      Patch_Map_MapUpdate.tmpRenderTex = (RenderTexture) null;
    }

    static WorldObject GetWorldObject(IThingHolder holder)
    {
      for (; holder != null; holder = holder.ParentHolder)
      {
        if (holder is WorldObject worldObject1)
          return worldObject1;
      }
      return (WorldObject) null;
    }
  }

  [PatchLevel(Level.Sensitive)]
  public static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator generator)
  {
    List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
    MethodInfo g_DrawingMap = AccessTools.PropertyGetter(typeof (WorldRendererUtility), "DrawingMap");
    int index = list.FindIndex((Predicate<CodeInstruction>) (c => CodeInstructionExtensions.Calls(c, g_DrawingMap))) + 1;
    Label label = generator.DefineLabel();
    LocalBuilder localBuilder = generator.DeclareLocal(typeof (VehiclePawnWithMap));
    list[index].labels.Add(label);
    // ISSUE: object of a compiler-generated type is created
    list.InsertRange(index, (IEnumerable<CodeInstruction>) new \u003C\u003Ez__ReadOnlyArray<CodeInstruction>(new CodeInstruction[11]
    {
      new CodeInstruction(OpCodes.Dup, (object) null),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      CodeInstruction.LoadField(typeof (VehicleMapFramework.VehicleMapFramework), "settings", false),
      CodeInstruction.LoadField(typeof (VehicleMapSettings), "drawPlanet", false),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      CodeInstruction.LoadArgument(0, false),
      new CodeInstruction(OpCodes.Ldloca, (object) localBuilder),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_IsVehicleMapOf),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Pop, (object) null),
      new CodeInstruction(OpCodes.Ldc_I4_0, (object) null)
    }));
    return (IEnumerable<CodeInstruction>) list;
  }
}
