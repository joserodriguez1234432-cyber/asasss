// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.GenDrawOnVehicle
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class GenDrawOnVehicle
{
  private static BoolGrid fieldGrid;
  private static readonly bool[] rotNeeded = new bool[4];
  private static readonly List<Matrix4x4> matrixList = new List<Matrix4x4>();

  public static void DrawFieldEdges(List<IntVec3> cells, int renderQueue = 2900, Map map = null)
  {
    GenDrawOnVehicle.DrawFieldEdges(cells, Color.white, renderQueue: renderQueue, map: map);
  }

  public static void DrawFieldEdges(
    List<IntVec3> cells,
    Color color,
    float? altOffset = null,
    HashSet<IntVec3> ignoreBorderCells = null,
    int renderQueue = 2900,
    Map map = null)
  {
    if (map == null)
    {
      if (Command_FocusVehicleMap.FocusedVehicle != null)
      {
        map = Command_FocusVehicleMap.FocusedVehicle.VehicleMap;
      }
      else
      {
        GenDraw.DrawFieldEdges(cells, color, altOffset, (HashSet<IntVec3>) null, 2900);
        return;
      }
    }
    VehiclePawnWithMap vehicle;
    if (!map.IsNonFocusedVehicleMapOf(out vehicle))
    {
      GenDraw.DrawFieldEdges(cells, color, altOffset, (HashSet<IntVec3>) null, 2900);
    }
    else
    {
      MaterialRequest materialRequest = new MaterialRequest();
      materialRequest.shader = ShaderDatabase.Transparent;
      materialRequest.color = color;
      ((MaterialRequest) ref materialRequest).BaseTexPath = "UI/Overlays/TargetHighlight_Side";
      materialRequest.renderQueue = renderQueue;
      Material material1 = MaterialPool.MatFrom(materialRequest);
      material1.GetTexture(AdditionalShaderPropertyIDs.MainTex).wrapMode = (TextureWrapMode) 1;
      if (GenDrawOnVehicle.fieldGrid == null)
        GenDrawOnVehicle.fieldGrid = new BoolGrid(200, 200);
      else
        GenDrawOnVehicle.fieldGrid.ClearAndResizeTo(200, 200);
      int count = cells.Count;
      float num = (float) ((double) altOffset ?? (double) Rand.ValueSeeded(ColorExtension.ToOpaque(color).GetHashCode()) * 0.038461539894342422 / 10.0);
      IntVec3 intVec3_1;
      // ISSUE: explicit constructor call
      ((IntVec3) ref intVec3_1).\u002Ector(100, 0, 100);
      for (int index = 0; index < count; ++index)
      {
        IntVec3 c = IntVec3.op_Addition(cells[index], intVec3_1);
        if (InBounds(c))
          GenDrawOnVehicle.fieldGrid[c.x, c.z] = true;
      }
      for (int index1 = 0; index1 < count; ++index1)
      {
        IntVec3 c = IntVec3.op_Addition(cells[index1], intVec3_1);
        if (InBounds(c))
        {
          // ISSUE: explicit non-virtual call
          GenDrawOnVehicle.rotNeeded[0] = c.z < 199 && !GenDrawOnVehicle.fieldGrid[c.x, c.z + 1] && (ignoreBorderCells != null ? (__nonvirtual (ignoreBorderCells.Contains(IntVec3.op_Addition(c, IntVec3.North))) ? 1 : 0) : 0) == 0;
          // ISSUE: explicit non-virtual call
          GenDrawOnVehicle.rotNeeded[1] = c.x < 199 && !GenDrawOnVehicle.fieldGrid[c.x + 1, c.z] && (ignoreBorderCells != null ? (__nonvirtual (ignoreBorderCells.Contains(IntVec3.op_Addition(c, IntVec3.East))) ? 1 : 0) : 0) == 0;
          // ISSUE: explicit non-virtual call
          GenDrawOnVehicle.rotNeeded[2] = c.z > 0 && !GenDrawOnVehicle.fieldGrid[c.x, c.z - 1] && (ignoreBorderCells != null ? (__nonvirtual (ignoreBorderCells.Contains(IntVec3.op_Addition(c, IntVec3.South))) ? 1 : 0) : 0) == 0;
          // ISSUE: explicit non-virtual call
          GenDrawOnVehicle.rotNeeded[3] = c.x > 0 && !GenDrawOnVehicle.fieldGrid[c.x - 1, c.z] && (ignoreBorderCells != null ? (__nonvirtual (ignoreBorderCells.Contains(IntVec3.op_Addition(c, IntVec3.West))) ? 1 : 0) : 0) == 0;
          for (int index2 = 0; index2 < 4; ++index2)
          {
            if (GenDrawOnVehicle.rotNeeded[index2])
            {
              Mesh plane10 = MeshPool.plane10;
              IntVec3 intVec3_2 = IntVec3.op_Subtraction(c, intVec3_1);
              Vector3 vector3 = Vector3Utility.WithYOffset(Vector3Utility.WithY(((IntVec3) ref intVec3_2).ToVector3Shifted().ToBaseMapCoord(vehicle), Altitudes.AltitudeFor((AltitudeLayer) 39)), num);
              Rot4 rot4 = new Rot4(index2);
              Quaternion quaternion = Quaternion.op_Multiply(((Rot4) ref rot4).AsQuat, VehicleMapUtility.get_FullAngleQuat((VehiclePawn) vehicle));
              Material material2 = material1;
              Graphics.DrawMesh(plane10, vector3, quaternion, material2, 0);
            }
          }
        }
      }
    }

    static bool InBounds(IntVec3 c) => (ulong) c.x < 200UL && (ulong) c.z < 200UL;
  }

  public static void DrawFieldEdgesSF(List<IntVec3> cells, Zone zone, Map map)
  {
    if (zone is Zone_Growing zoneGrowing)
    {
      MapComponent component = map.GetComponent(ModCompat.SmartFarming.MapComponent_SmartFarming);
      if (component == null)
      {
        GenDrawOnVehicle.DrawFieldEdges(cells, map: map);
        return;
      }
      IDictionary dictionary = ModCompat.SmartFarming.growZoneRegistry.Invoke(component);
      if (dictionary == null)
      {
        GenDrawOnVehicle.DrawFieldEdges(cells, map: map);
        return;
      }
      if (dictionary.Contains((object) ((Zone) zoneGrowing).ID))
      {
        List<IntVec3> intVec3List = cells;
        Color color1;
        switch (ModCompat.SmartFarming.priority.Invoke(dictionary[(object) ((Zone) zoneGrowing).ID]))
        {
          case 1:
            color1 = Color.grey;
            break;
          case 3:
            color1 = Color.green;
            break;
          case 4:
            color1 = Color.yellow;
            break;
          case 5:
            color1 = Color.red;
            break;
          default:
            color1 = Color.white;
            break;
        }
        List<IntVec3> cells1 = intVec3List;
        Color color2 = color1;
        Map map1 = map;
        float? altOffset = new float?();
        Map map2 = map1;
        GenDrawOnVehicle.DrawFieldEdges(cells1, color2, altOffset, map: map2);
      }
    }
    GenDrawOnVehicle.DrawFieldEdges(cells, map: map);
  }

  public static void DrawFieldEdgesRG(List<IntVec3> cells, int renderQueue, Zone zone, Map map)
  {
    if (zone is Zone_Growing zoneGrowing)
    {
      MapComponent component = map.GetComponent(ModCompat.SmartFarming.MapComponent_SmartFarming);
      if (component == null)
      {
        GenDrawOnVehicle.DrawFieldEdges(cells, renderQueue, map);
        return;
      }
      IDictionary dictionary = ModCompat.SmartFarming.growZoneRegistry.Invoke(component);
      if (dictionary == null)
      {
        GenDrawOnVehicle.DrawFieldEdges(cells, renderQueue, map);
        return;
      }
      if (dictionary.Contains((object) ((Zone) zoneGrowing).ID))
      {
        List<IntVec3> intVec3List = cells;
        Color color1;
        switch (ModCompat.SmartFarming.priority.Invoke(dictionary[(object) ((Zone) zoneGrowing).ID]))
        {
          case 1:
            color1 = Color.grey;
            break;
          case 3:
            color1 = Color.green;
            break;
          case 4:
            color1 = Color.yellow;
            break;
          case 5:
            color1 = Color.red;
            break;
          default:
            color1 = Color.white;
            break;
        }
        List<IntVec3> cells1 = intVec3List;
        Color color2 = color1;
        int num = renderQueue;
        Map map1 = map;
        float? altOffset = new float?();
        int renderQueue1 = num;
        Map map2 = map1;
        GenDrawOnVehicle.DrawFieldEdges(cells1, color2, altOffset, renderQueue: renderQueue1, map: map2);
      }
    }
    GenDrawOnVehicle.DrawFieldEdges(cells, renderQueue, map);
  }

  public static void DrawLineBetweenInstanced(Vector3 A, Vector3 B, Material mat, float lineWidth = 0.2f)
  {
    if ((double) Mathf.Abs(A.x - B.x) < 0.0099999997764825821 && (double) Mathf.Abs(A.z - B.z) < 0.0099999997764825821)
      return;
    if (!mat.enableInstancing)
    {
      GenDraw.DrawLineBetween(A, B, mat, lineWidth);
    }
    else
    {
      A.y = B.y;
      float num1 = GenGeo.MagnitudeHorizontal(Vector3.op_Subtraction(B, A));
      int num2 = Mathf.CeilToInt(num1 / lineWidth);
      Vector3 vector3_1;
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3_1).\u002Ector(lineWidth, 1f, num1 / (float) num2);
      Vector3 vector3_2 = Vector3.op_Division(Vector3.op_Subtraction(B, A), (float) num2);
      Vector3 vector3_3 = Vector3.op_Addition(A, Vector3.op_Multiply(vector3_2, 0.5f));
      Quaternion quaternion = Quaternion.LookRotation(Vector3.op_Subtraction(B, A));
      GenDrawOnVehicle.matrixList.Clear();
      for (int index = 0; index < num2; ++index)
        GenDrawOnVehicle.matrixList.Add(Matrix4x4.TRS(Vector3.op_Addition(vector3_3, Vector3.op_Multiply(vector3_2, (float) index)), quaternion, vector3_1));
      Graphics.DrawMeshInstanced(MeshPool.plane10, 0, mat, GenDrawOnVehicle.matrixList);
    }
  }
}
