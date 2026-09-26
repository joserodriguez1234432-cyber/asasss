// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VehiclePawnWithMap_Hover
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class VehiclePawnWithMap_Hover : VehiclePawnWithMap
{
  private float drawOffset;
  private float prevOffset;
  private float? drawPosZ;
  private int? ignitionTick;
  private bool ignitionComplete;
  private bool landingComplete = true;
  private const float offsetDrafted = 0.25f;
  private const float ignitionDuration = 100f;

  public override Vector3 DrawPos
  {
    get
    {
      if (!((Thing) this).Spawned || Find.CurrentMap == this.VehicleMap || this.IsAirborne)
        return base.DrawPos;
      Vector3 drawPos = base.DrawPos;
      drawPos.z += this.drawOffset;
      return drawPos;
    }
  }

  protected override void Tick()
  {
    this.prevOffset = this.drawOffset;
    if (this.ignition.Drafted)
    {
      if (!this.ignitionComplete)
      {
        if (!this.ignitionTick.HasValue)
        {
          this.ignitionTick = new int?(Find.TickManager.TicksGame);
        }
        else
        {
          float num = Mathf.Min((float) (Find.TickManager.TicksGame - this.ignitionTick.Value) / 100f, 1f);
          if (Mathf.Approximately(num, 1f))
            this.ignitionComplete = true;
          this.drawOffset = 0.25f * num;
        }
      }
      else
      {
        this.drawOffset = 0.25f;
        this.drawOffset += Mathf.Sin((float) Find.TickManager.TicksGame * 0.075f) * 0.035f;
      }
    }
    else if (this.ignitionTick.HasValue)
    {
      this.ignitionTick = new int?();
      this.ignitionComplete = false;
      this.landingComplete = false;
    }
    if (!this.landingComplete)
    {
      this.drawOffset = Mathf.Max(0.0f, this.drawOffset - 0.004f);
      if ((double) this.drawOffset == 0.0)
        this.landingComplete = true;
    }
    base.Tick();
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<float>(ref this.drawOffset, "floatingOffset", 0.0f, false);
    Scribe_Values.Look<float>(ref this.prevOffset, "floatingOffsetPrev", 0.0f, false);
    Scribe_Values.Look<float?>(ref this.drawPosZ, "drawPosZ", new float?(), false);
    Scribe_Values.Look<int?>(ref this.ignitionTick, "ignitionTick", new int?(), false);
    Scribe_Values.Look<bool>(ref this.ignitionComplete, "ignitionComplete", false, false);
    Scribe_Values.Look<bool>(ref this.landingComplete, "landingComplete", false, false);
  }
}
