// Decompiled with JetBrains decompiler
// Type: Vehicles.AutoLoadConfig
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Verse;

#nullable enable
namespace Vehicles;

public sealed class AutoLoadConfig : IExposable
{
  private 
  #nullable disable
  VehicleTurret turret;
  private List<AutoLoadConfig.Quota> quotas = new List<AutoLoadConfig.Quota>();

  private Dictionary<ThingDef, AutoLoadConfig.Quota> QuotaLookup { get; } = new Dictionary<ThingDef, AutoLoadConfig.Quota>();

  public AutoLoadConfig(VehicleTurret turret)
  {
    this.turret = turret;
    this.RecacheSettings();
  }

  public int Get(ThingDef thingDef)
  {
    AutoLoadConfig.Quota quota = GenCollection.TryGetValue<ThingDef, AutoLoadConfig.Quota>((IReadOnlyDictionary<ThingDef, AutoLoadConfig.Quota>) this.QuotaLookup, thingDef, (AutoLoadConfig.Quota) null);
    return (object) quota == null ? 0 : quota.count;
  }

  public void Set(ThingDef thingDef, int count)
  {
    AutoLoadConfig.Quota quota;
    if (!this.QuotaLookup.TryGetValue(thingDef, out quota))
      Log.Error($"{thingDef} is not registered as a valid ammo type.");
    else
      quota.count = count;
  }

  public bool IsEnabled(ThingDef thingDef)
  {
    AutoLoadConfig.Quota quota;
    if (this.QuotaLookup.TryGetValue(thingDef, out quota))
      return quota.enabled;
    Log.Error($"{thingDef} is not registered as a valid ammo type.");
    return false;
  }

  public void SetEnabled(ThingDef thingDef, bool enabled)
  {
    AutoLoadConfig.Quota quota;
    if (!this.QuotaLookup.TryGetValue(thingDef, out quota))
      Log.Error($"{thingDef} is not registered as a valid ammo type.");
    else
      quota.enabled = enabled;
  }

  private void RecacheSettings()
  {
    if (this.turret.def.ammunition == null)
      return;
    foreach (AutoLoadConfig.Quota quota in this.quotas)
    {
      if (quota.ThingDef != null)
        this.QuotaLookup[quota.ThingDef] = quota;
    }
    foreach (ThingDef allowedThingDef in this.turret.def.ammunition.AllowedThingDefs)
    {
      AutoLoadConfig.Quota quota;
      if (!this.QuotaLookup.TryGetValue(allowedThingDef, out quota))
      {
        quota = new AutoLoadConfig.Quota(allowedThingDef);
        this.quotas.Add(quota);
        this.QuotaLookup[quota.ThingDef] = quota;
      }
    }
  }

  void IExposable.ExposeData()
  {
    Scribe_References.Look<VehicleTurret>(ref this.turret, "turret", false);
    Scribe_Values.Look<List<AutoLoadConfig.Quota>>(ref this.quotas, "quotas", (List<AutoLoadConfig.Quota>) null, false);
    if (Scribe.mode != 4)
      return;
    this.RecacheSettings();
  }

  private record Quota : IExposable
  {
    private string defName;
    public bool enabled;
    public int count;

    public Quota(ThingDef thingDef)
    {
      this.defName = ((Def) thingDef).defName;
      this.ThingDef = thingDef;
    }

    public ThingDef ThingDef { get; private set; }

    void IExposable.ExposeData()
    {
      Scribe_Values.Look<string>(ref this.defName, "defName", (string) null, false);
      Scribe_Values.Look<bool>(ref this.enabled, "enabled", false, false);
      Scribe_Values.Look<int>(ref this.count, "count", 0, false);
      if (Scribe.mode != 3)
        return;
      this.ThingDef = DefDatabase<ThingDef>.GetNamedSilentFail(this.defName);
    }

    [CompilerGenerated]
    protected virtual bool PrintMembers(
    #nullable enable
    StringBuilder builder)
    {
      RuntimeHelpers.EnsureSufficientExecutionStack();
      builder.Append("enabled = ");
      builder.Append(this.enabled.ToString());
      builder.Append(", count = ");
      builder.Append(this.count.ToString());
      builder.Append(", ThingDef = ");
      builder.Append((object) this.ThingDef);
      return true;
    }

    [CompilerGenerated]
    public override int GetHashCode()
    {
      // ISSUE: reference to a compiler-generated field
      return (((EqualityComparer<System.Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.defName)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.enabled)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.count)) * -1521134295 + EqualityComparer<ThingDef>.Default.GetHashCode(this.\u003CThingDef\u003Ek__BackingField);
    }

    [CompilerGenerated]
    public virtual bool Equals(AutoLoadConfig.Quota? other)
    {
      if ((object) this == (object) other)
        return true;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      return (object) other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(this.defName, other.defName) && EqualityComparer<bool>.Default.Equals(this.enabled, other.enabled) && EqualityComparer<int>.Default.Equals(this.count, other.count) && EqualityComparer<ThingDef>.Default.Equals(this.\u003CThingDef\u003Ek__BackingField, other.\u003CThingDef\u003Ek__BackingField);
    }

    [CompilerGenerated]
    protected Quota(AutoLoadConfig.Quota original)
    {
      this.defName = original.defName;
      this.enabled = original.enabled;
      this.count = original.count;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      this.\u003CThingDef\u003Ek__BackingField = original.\u003CThingDef\u003Ek__BackingField;
    }
  }
}
