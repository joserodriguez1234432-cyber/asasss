// Decompiled with JetBrains decompiler
// Type: Vehicles.Turret_RecoilTracker
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

#nullable disable
namespace Vehicles;

public class Turret_RecoilTracker
{
  private float curRecoil;
  private float targetRecoil;
  private float recoilStep;
  private bool recoilingBack;
  private readonly RecoilProperties recoilProperties;

  public Turret_RecoilTracker(RecoilProperties recoilProperties)
  {
    this.recoilProperties = recoilProperties;
  }

  public float Angle { get; private set; }

  public float Recoil => this.curRecoil;

  public bool RecoilTick()
  {
    if ((double) this.targetRecoil <= 0.0)
      return false;
    if (this.recoilingBack)
    {
      this.curRecoil += this.recoilStep;
      if ((double) this.curRecoil >= (double) this.targetRecoil)
        this.curRecoil = this.targetRecoil;
    }
    else
    {
      this.curRecoil -= this.recoilStep * this.recoilProperties.speedMultiplierPostRecoil;
      if ((double) this.curRecoil <= 0.0)
      {
        this.ResetRecoilVars();
        return false;
      }
    }
    if ((double) this.curRecoil >= (double) this.targetRecoil)
      this.recoilingBack = false;
    return true;
  }

  public void Notify_TurretRecoil(float angle)
  {
    this.targetRecoil = this.recoilProperties.distanceTotal;
    this.recoilStep = this.recoilProperties.distancePerTick;
    this.curRecoil = 0.0f;
    this.recoilingBack = true;
    this.Angle = angle;
  }

  private void ResetRecoilVars()
  {
    this.curRecoil = 0.0f;
    this.targetRecoil = 0.0f;
    this.recoilStep = 0.0f;
    this.Angle = 0.0f;
  }
}
