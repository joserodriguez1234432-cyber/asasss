// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Dialog_ChooseVehicleRoles
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Dialog_ChooseVehicleRoles : Window
{
  private readonly VehiclePawn vehicle;
  private readonly RoleUpgradeBuildable roleUpgrade;
  private readonly VehicleUpgradeBuildable upgradeBuildable;
  private readonly List<string> turretIds = new List<string>();
  private Vector2 scrollPosition;

  public virtual Vector2 InitialSize => new Vector2(350f, 216f);

  public Dialog_ChooseVehicleRoles(
    VehiclePawn vehicle,
    RoleUpgradeBuildable roleUpgrade,
    VehicleUpgradeBuildable upgradeBuildable)
    : base((IWindowDrawing) null)
  {
    this.vehicle = vehicle;
    this.roleUpgrade = roleUpgrade;
    this.upgradeBuildable = upgradeBuildable;
    this.doCloseButton = true;
    this.forcePause = true;
  }

  public virtual string CloseButtonText => TaggedString.op_Implicit(Translator.Translate("OK"));

  public virtual void DoWindowContents(Rect inRect)
  {
    TextBlock textBlock;
    // ISSUE: explicit constructor call
    ((TextBlock) ref textBlock).\u002Ector((GameFont) 1);
    try
    {
      Rect rect1 = inRect;
      ref Rect local1 = ref rect1;
      ((Rect) ref local1).height = ((Rect) ref local1).height - (Window.CloseButSize.y + 10f);
      Widgets.DrawMenuSection(rect1);
      CompVehicleTurrets compVehicleTurrets = this.vehicle.CompVehicleTurrets;
      List<VehicleTurret> vehicleTurretList;
      if (compVehicleTurrets == null)
      {
        vehicleTurretList = (List<VehicleTurret>) null;
      }
      else
      {
        IReadOnlyList<VehicleTurret> turrets = compVehicleTurrets.Turrets;
        vehicleTurretList = turrets != null ? turrets.Where<VehicleTurret>((Func<VehicleTurret, bool>) (t =>
        {
          List<VehicleRoleHandler> handlers = this.vehicle.handlers;
          return handlers == null || handlers.All<VehicleRoleHandler>((Func<VehicleRoleHandler, bool>) (h =>
          {
            VehicleRole role3 = h.role;
            bool? nullable3;
            if (role3 == null)
            {
              nullable3 = new bool?();
            }
            else
            {
              List<string> turretIds = role3.TurretIds;
              // ISSUE: explicit non-virtual call
              nullable3 = turretIds != null ? new bool?(!__nonvirtual (turretIds.Contains(t.key))) : new bool?();
            }
            if (!(nullable3 ?? true))
              return false;
            VehicleRole role4 = h.role;
            bool? nullable4;
            if (role4 == null)
            {
              nullable4 = new bool?();
            }
            else
            {
              List<string> turretIds = role4.TurretIds;
              // ISSUE: explicit non-virtual call
              nullable4 = turretIds != null ? new bool?(!__nonvirtual (turretIds.Contains(t.groupKey))) : new bool?();
            }
            return nullable4 ?? true;
          }));
        })).ToList<VehicleTurret>() : (List<VehicleTurret>) null;
      }
      List<VehicleTurret> source1 = vehicleTurretList;
      if (source1 == null)
        return;
      IEnumerable<IGrouping<string, VehicleTurret>> groupings = source1.GroupBy<VehicleTurret, string>((Func<VehicleTurret, string>) (t => GenText.NullOrEmpty(t.groupKey) ? t.key : t.groupKey));
      Rect rect2;
      // ISSUE: explicit constructor call
      ((Rect) ref rect2).\u002Ector(0.0f, 0.0f, ((Rect) ref inRect).width, (float) source1.Count * Text.LineHeight);
      Rect rect3 = rect1;
      Widgets.AdjustRectsForScrollView(rect1, ref rect3, ref rect2);
      Rect rect4;
      // ISSUE: explicit constructor call
      ((Rect) ref rect4).\u002Ector(((Rect) ref rect3).x, ((Rect) ref rect3).y, ((Rect) ref rect3).width, Text.LineHeight);
      Widgets.BeginScrollView(rect3, ref this.scrollPosition, rect2, true);
      foreach (IGrouping<string, VehicleTurret> source2 in groupings)
      {
        Rect rect5 = GenUI.LeftPartPixels(rect4, Text.LineHeight);
        Rect rect6 = GenUI.RightPartPixels(rect4, (float) ((double) ((Rect) ref rect4).width - (double) Text.LineHeight - 10.0));
        Widgets.DrawTextureFitted(rect5, (Texture) source2.First<VehicleTurret>().GizmoIcon, 1f, 1f);
        string gizmoLabel = source2.First<VehicleTurret>().gizmoLabel;
        Widgets.Label(rect6, gizmoLabel);
        if (Widgets.ButtonInvisible(rect4, true))
        {
          if (!this.turretIds.Contains(source2.Key))
          {
            this.turretIds.Add(source2.Key);
            int count = this.turretIds.Count;
            int? slotsToOperate = this.roleUpgrade.slotsToOperate;
            int valueOrDefault = slotsToOperate.GetValueOrDefault();
            if (count > valueOrDefault & slotsToOperate.HasValue)
              this.turretIds.RemoveAt(0);
          }
          else
            this.turretIds.Remove(source2.Key);
        }
        if (this.turretIds.Contains(source2.Key))
          Widgets.DrawHighlightSelected(rect4);
        Widgets.DrawHighlightIfMouseover(rect4);
        ref Rect local2 = ref rect4;
        ((Rect) ref local2).y = ((Rect) ref local2).y + Text.LineHeight;
      }
      Widgets.EndScrollView();
    }
    finally
    {
      textBlock.Dispose();
    }
  }

  public virtual void PostClose()
  {
    this.upgradeBuildable.UpgradeRole(this.vehicle, this.roleUpgrade, false, false, this.turretIds);
  }
}
