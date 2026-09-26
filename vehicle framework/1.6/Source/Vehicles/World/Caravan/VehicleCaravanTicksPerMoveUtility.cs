// Decompiled with JetBrains decompiler
// Type: Vehicles.World.VehicleCaravanTicksPerMoveUtility
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.World;

[PublicAPI]
public static class VehicleCaravanTicksPerMoveUtility
{
  private const int MaxPawnTicksPerMove = 150;
  private const int DownedPawnMoveTicks = 450;
  private const float CellToTilesConversionRatio = 340f;
  private const float MoveSpeedFactorAtZeroMass = 2f;
  public const int DefaultTicksPerMove = 3300;
  private static readonly List<int> MoveSpeedTicks = new List<int>();
  private static readonly StringBuilder TicksExplanation = new StringBuilder();

  [MustUseReturnValue]
  public static int GetTicksPerMove(Caravan caravan, StringBuilder explanation = null)
  {
    if (caravan != null)
      return VehicleCaravanTicksPerMoveUtility.GetTicksPerMove(new VehicleCaravanInfo(caravan), explanation);
    if (explanation != null)
      VehicleCaravanTicksPerMoveUtility.AppendUsingDefaultTicksPerMoveInfo(explanation);
    return 3300;
  }

  [MustUseReturnValue]
  public static int GetTicksPerMove(VehicleCaravanInfo caravanInfo, StringBuilder explanation = null)
  {
    return VehicleCaravanTicksPerMoveUtility.GetTicksPerMove(caravanInfo.vehiclesAndDismountedPawns, caravanInfo.massUsage, caravanInfo.massCapacity, explanation);
  }

  [MustUseReturnValue]
  public static int GetTicksPerMove(
    List<Pawn> pawns,
    float massUsage,
    float massCapacity,
    StringBuilder explanation = null)
  {
    bool flag1 = false;
    if (!GenList.NullOrEmpty<Pawn>((IList<Pawn>) pawns))
    {
      using (new ClearOnDispose<int>((ICollection<int>) VehicleCaravanTicksPerMoveUtility.MoveSpeedTicks))
      {
        using (new ClearStringOnDispose(VehicleCaravanTicksPerMoveUtility.TicksExplanation))
        {
          foreach (Pawn pawn in pawns)
          {
            bool flag2 = explanation != null;
            if (pawn is VehiclePawn vehiclePawn)
            {
              float worldSpeedMultiplier = vehiclePawn.WorldSpeedMultiplier;
              float moveSpeedNormalized = (float) ((double) vehiclePawn.GetStatValue(VehicleStatDefOf.MoveSpeed) * (double) worldSpeedMultiplier / 60.0);
              if ((double) moveSpeedNormalized > 0.0)
              {
                int num = VehicleCaravanTicksPerMoveUtility.TicksFromMoveSpeed(moveSpeedNormalized);
                VehicleCaravanTicksPerMoveUtility.MoveSpeedTicks.Add(num);
                if (flag2)
                  VehicleCaravanTicksPerMoveUtility.TicksExplanation.AppendLine($"  {((Entity) vehiclePawn).LabelCap}: {60000 / num:0.#} {Translator.Translate("TilesPerDay")}");
              }
              else
              {
                flag1 = true;
                if (flag2)
                  VehicleCaravanTicksPerMoveUtility.TicksExplanation.AppendLine($"  {((Entity) vehiclePawn).LabelCap}: 0 {Translator.Translate("TilesPerDay")}");
              }
            }
            else if (!pawn.InVehicle())
            {
              int num = VehicleCaravanTicksPerMoveUtility.TicksFromMoveSpeed(StatExtension.GetStatValueAbstract((BuildableDef) ThingDefOf.Human, StatDefOf.MoveSpeed, (ThingDef) null) / 60f);
              VehicleCaravanTicksPerMoveUtility.MoveSpeedTicks.Add(num);
              if (flag2)
                VehicleCaravanTicksPerMoveUtility.TicksExplanation.AppendLine($"  {((Entity) pawn).LabelCap}: {60000 / num:0.#} {Translator.Translate("TilesPerDay")}");
            }
          }
          float num1 = float.MaxValue;
          if (VehicleCaravanTicksPerMoveUtility.MoveSpeedTicks.Count > 0 && !flag1)
            num1 = (float) VehicleCaravanTicksPerMoveUtility.MoveSpeedTicks.Average();
          int ticksPerMove = Mathf.RoundToInt(num1);
          if (explanation != null)
          {
            explanation.AppendLine($"{Translator.Translate("CaravanMovementSpeedFull")}:");
            explanation.AppendLine(VehicleCaravanTicksPerMoveUtility.TicksExplanation.ToString());
            if ((double) massUsage > (double) massCapacity)
              explanation.AppendLine($"  {Translator.Translate("MultiplierForCarriedMass")}");
            explanation.AppendLine();
            float num2 = 60000f / (float) ticksPerMove;
            explanation.AppendLine($"  {Translator.Translate("Average")}: {num2:0.#} {Translator.Translate("TilesPerDay")}");
          }
          return ticksPerMove;
        }
      }
    }
    if (explanation != null)
      VehicleCaravanTicksPerMoveUtility.AppendUsingDefaultTicksPerMoveInfo(explanation);
    return 3300;
  }

  [MustUseReturnValue]
  public static int GetTicksPerMove(List<VehicleDef> vehicleDefs, StringBuilder explanation = null)
  {
    using (new ClearOnDispose<int>((ICollection<int>) VehicleCaravanTicksPerMoveUtility.MoveSpeedTicks))
    {
      using (new ClearStringOnDispose(VehicleCaravanTicksPerMoveUtility.TicksExplanation))
      {
        foreach (VehicleDef vehicleDef in vehicleDefs)
        {
          float worldSpeedMultiplier = vehicleDef.properties.worldSpeedMultiplier;
          float moveSpeedNormalized = (float) ((double) vehicleDef.GetStatValueAbstract(VehicleStatDefOf.MoveSpeed) * (double) worldSpeedMultiplier / 60.0);
          if ((double) moveSpeedNormalized > 0.0)
          {
            int num = VehicleCaravanTicksPerMoveUtility.TicksFromMoveSpeed(moveSpeedNormalized);
            VehicleCaravanTicksPerMoveUtility.MoveSpeedTicks.Add(num);
            VehicleCaravanTicksPerMoveUtility.TicksExplanation.AppendLine($"  {((Def) vehicleDef).LabelCap}: {60000 / num:0.#} {Translator.Translate("TilesPerDay")}");
          }
          else
            VehicleCaravanTicksPerMoveUtility.TicksExplanation.AppendLine($"  {((Def) vehicleDef).LabelCap}: 0 {Translator.Translate("TilesPerDay")}");
        }
        float num1 = float.MaxValue;
        if (VehicleCaravanTicksPerMoveUtility.MoveSpeedTicks.Count > 0)
          num1 = (float) VehicleCaravanTicksPerMoveUtility.MoveSpeedTicks.Average();
        int ticksPerMove = Mathf.RoundToInt(num1);
        explanation?.AppendLine($"{Translator.Translate("CaravanMovementSpeedFull")}:");
        explanation?.AppendLine(VehicleCaravanTicksPerMoveUtility.TicksExplanation.ToString());
        explanation?.AppendLine();
        float num2 = 60000f / (float) ticksPerMove;
        explanation?.AppendLine($"  {Translator.Translate("Average")}: {num2:0.#} {Translator.Translate("TilesPerDay")}");
        if (explanation != null)
          VehicleCaravanTicksPerMoveUtility.AppendUsingDefaultTicksPerMoveInfo(explanation);
        return ticksPerMove;
      }
    }
  }

  [MustUseReturnValue]
  public static int TicksFromMoveSpeed(float moveSpeedNormalized)
  {
    return Mathf.Max(Mathf.RoundToInt(1f / moveSpeedNormalized * 340f), 1);
  }

  private static void AppendUsingDefaultTicksPerMoveInfo(StringBuilder explanation)
  {
    explanation.Append($"{Translator.Translate("CaravanMovementSpeedFull")}:");
    explanation.AppendLine();
    explanation.Append($"  {Translator.Translate("Default")}: {(ValueType) 18.181818f:0.#} {Translator.Translate("TilesPerDay")}");
  }

  [MustUseReturnValue]
  public static float ApproxTilesPerDay(VehicleCaravan caravan, StringBuilder explanation = null)
  {
    return caravan.AerialVehicle ? 0.0f : VehicleCaravanTicksPerMoveUtility.ApproxTilesPerDay(caravan.UniqueVehicleDefsInCaravan().ToList<VehicleDef>(), caravan.TicksPerMove, ((WorldObject) caravan).Tile, caravan.vehiclePather.Moving ? caravan.vehiclePather.NextTile : PlanetTile.op_Implicit(-1), explanation, explanation != null ? caravan.TicksPerMoveExplanation : (string) null);
  }

  [MustUseReturnValue]
  public static float ApproxTilesPerDay(
    List<VehicleDef> vehicleDefs,
    int ticksPerMove,
    PlanetTile tile,
    PlanetTile nextTile,
    StringBuilder explanation = null,
    string caravanTicksPerMoveExplanation = null)
  {
    if (!((PlanetTile) ref nextTile).Valid)
      nextTile = Find.WorldGrid.FindMostReasonableAdjacentTileForDisplayedPathCost(tile);
    int num = Mathf.CeilToInt((float) VehicleCaravan_PathFollower.CostToMove(vehicleDefs, ticksPerMove, tile, nextTile, explanation: explanation, caravanTicksPerMoveExplanation: caravanTicksPerMoveExplanation));
    return num <= 0 ? 0.0f : 60000f / (float) num;
  }
}
