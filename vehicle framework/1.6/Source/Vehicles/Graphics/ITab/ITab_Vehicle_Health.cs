// Decompiled with JetBrains decompiler
// Type: Vehicles.ITab_Vehicle_Health
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class ITab_Vehicle_Health : ITab
{
  private static readonly Vector2 PanelSize = new Vector2(100f, 100f);

  public ITab_Vehicle_Health()
  {
    ((InspectTabBase) this).size = ITab_Vehicle_Health.PanelSize;
    ((InspectTabBase) this).labelKey = "VF_TabComponents";
  }

  public VehiclePawn Vehicle => this.SelPawn as VehiclePawn;

  public virtual void OnOpen()
  {
    ((InspectTabBase) this).OnOpen();
    VehicleTabHelper_Health.Init();
    ((InspectTabBase) this).UpdateSize();
  }

  protected virtual void CloseTab()
  {
    base.CloseTab();
    this.Vehicle.HighlightedComponent = (VehicleComponent) null;
  }

  protected virtual void FillTab()
  {
    Vector2 vector2 = VehicleTabHelper_Health.Start(this.Vehicle);
    if (Vector2.op_Inequality(((InspectTabBase) this).size, vector2))
    {
      ((InspectTabBase) this).size = vector2;
      ((InspectTabBase) this).UpdateSize();
    }
    VehicleTabHelper_Health.DrawHealthPanel(this.Vehicle);
    VehicleTabHelper_Health.End();
  }

  public enum VehicleHealthTab
  {
    Overview,
    JobSettings,
  }
}
