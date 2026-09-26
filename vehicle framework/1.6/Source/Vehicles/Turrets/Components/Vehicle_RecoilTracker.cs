// Decompiled with JetBrains decompiler
// Type: Vehicles.Vehicle_RecoilTracker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

#nullable disable
namespace Vehicles;

public class Vehicle_RecoilTracker
{
  public float curRecoil;
  private float targetRecoil;
  private float recoilStep;
  private bool recoilingBack;
  private float speedMultiplierPostRecoil;

  public float Angle { get; private set; }

  public float Recoil => this.curRecoil;

  public void ProcessPostTickVisuals(int ticksPassed)
  {
    if ((double) this.targetRecoil <= 0.0)
      return;
    if (this.recoilingBack)
    {
      this.curRecoil += this.recoilStep * (float) ticksPassed;
      if ((double) this.curRecoil >= (double) this.targetRecoil)
        this.curRecoil = this.targetRecoil;
    }
    else
    {
      this.curRecoil -= this.recoilStep * this.speedMultiplierPostRecoil * (float) ticksPassed;
      if ((double) this.curRecoil <= 0.0)
        this.ResetRecoilVars();
    }
    if ((double) this.curRecoil < (double) this.targetRecoil)
      return;
    this.recoilingBack = false;
  }

  public void Notify_TurretRecoil(VehicleTurret turret, float angle)
  {
    if (turret.def.vehicleRecoil == null)
      return;
    this.targetRecoil = turret.def.vehicleRecoil.distanceTotal;
    this.recoilStep = turret.def.vehicleRecoil.distancePerTick;
    this.speedMultiplierPostRecoil = turret.def.vehicleRecoil.speedMultiplierPostRecoil;
    this.curRecoil = 0.0f;
    this.recoilingBack = true;
    this.Angle = angle;
  }

  private void ResetRecoilVars()
  {
    this.targetRecoil = 0.0f;
    this.recoilStep = 0.0f;
    this.speedMultiplierPostRecoil = 0.0f;
    this.curRecoil = 0.0f;
    this.Angle = 0.0f;
  }
}
