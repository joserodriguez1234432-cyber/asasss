// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehicleStatPart_HumanPower
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehicleStatPart_HumanPower : VehicleStatPart
{
  private static float Modifier(VehiclePawn vehicle)
  {
    List<VehicleRoleHandler> handlers = vehicle.handlers;
    VehicleRoleHandler[] array = handlers != null ? handlers.Where<VehicleRoleHandler>((Func<VehicleRoleHandler, bool>) (h => GenTypes.Isnt<VehicleRoleHandlerBuildable>((object) h) && h.RequiredForMovement)).ToArray<VehicleRoleHandler>() : (VehicleRoleHandler[]) null;
    return !GenList.NullOrEmpty<VehicleRoleHandler>((IList<VehicleRoleHandler>) array) ? ((IEnumerable<VehicleRoleHandler>) array).Average<VehicleRoleHandler>((Func<VehicleRoleHandler, float>) (h =>
    {
      float num1 = 0.0f;
      foreach (Pawn pawn in h.thingOwner)
      {
        if (h.CanOperateRole(pawn))
        {
          float num2 = 1f * (StatExtension.GetStatValue((Thing) pawn, StatDefOf.MoveSpeed, true, -1) / StatDefOf.MoveSpeed.defaultBaseValue) * Mathf.LerpUnclamped(1f, 1f / StatExtension.GetStatValue((Thing) pawn, StatDefOf.IncomingDamageFactor, true, -1) / StatDefOf.IncomingDamageFactor.defaultBaseValue, 0.5f) * Mathf.LerpUnclamped(1f, StatExtension.GetStatValue((Thing) pawn, StatDefOf.WorkSpeedGlobal, true, -1) / StatDefOf.WorkSpeedGlobal.defaultBaseValue, 0.25f) * Mathf.LerpUnclamped(1f, pawn.BodySize, 1.25f) * Mathf.LerpUnclamped(1f, Mathf.Min((float) (pawn.skills.GetSkill(SkillDefOf.Melee).Level + pawn.skills.GetSkill(SkillDefOf.Mining).Level), 20f) / 10f, 0.5f);
          num1 += num2;
        }
      }
      return num1 / (float) h.role.Slots;
    })) : 0.0f;
  }

  public virtual float TransformValue(VehiclePawn vehicle, float value)
  {
    return ((Def) vehicle.VehicleDef).HasModExtension<VehicleHumanPowered>() ? value * VehicleStatPart_HumanPower.Modifier(vehicle) : value;
  }

  public virtual string ExplanationPart(VehiclePawn vehicle)
  {
    return TaggedString.op_Implicit(((Def) vehicle.VehicleDef).HasModExtension<VehicleHumanPowered>() ? TranslatorFormattedStringExtensions.Translate("VMF_StatsReport_HumanPowerAverage", NamedArgument.op_Implicit(GenText.ToStringByStyle(VehicleStatPart_HumanPower.Modifier(vehicle), (ToStringStyle) 5, (ToStringNumberSense) 2))) : TaggedString.op_Implicit((string) null));
  }
}
