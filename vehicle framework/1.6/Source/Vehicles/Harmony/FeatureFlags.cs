// Decompiled with JetBrains decompiler
// Type: Vehicles.Config.FeatureFlags
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools.Xml;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles.Config;

[UsedImplicitly]
internal class FeatureFlags
{
  public const string Raiders = "Raiders";
  public const string Paratroopers = "Paratroopers";
  public const string Fishing = "Fishing";
  public const string TradeableVehicles = "TradeableVehicles";
  public const string VehicleCaravanProps = "VehicleCaravanProps";
  public const string BetterAutoLoadConfig = "BetterAutoLoadConfig";
  [UsedImplicitly]
  public List<IFeatureFlag> features;

  public static FeatureFlags Default => VehicleMod.mod.features;

  public static bool RaidersEnabled => FeatureFlags.Default.IsEnabled("Raiders");

  public static bool ParatroopersEnabled => FeatureFlags.Default.IsEnabled("Paratroopers");

  public static bool FishingEnabled => FeatureFlags.Default.IsEnabled("Fishing");

  public static FeatureFlags InitDefault()
  {
    return new FeatureFlags()
    {
      features = new List<IFeatureFlag>(4)
      {
        (IFeatureFlag) FeatureFlags.Feature.Create("Raiders", Build.Configuration.Debug, Build.Configuration.Unstable),
        (IFeatureFlag) FeatureFlags.Feature.Create("Paratroopers", Build.Configuration.Debug, Build.Configuration.Unstable),
        (IFeatureFlag) FeatureFlags.Feature.Create("Fishing", Build.Configuration.Debug, Build.Configuration.Unstable),
        (IFeatureFlag) FeatureFlags.Feature.Create("TradeableVehicles", Build.Configuration.Debug, Build.Configuration.Unstable)
      }
    };
  }

  public bool IsEnabled(string featureName)
  {
    if (GenList.NullOrEmpty<IFeatureFlag>((IList<IFeatureFlag>) this.features))
      return false;
    foreach (IFeatureFlag feature in this.features)
    {
      if (feature.Name == featureName)
        return feature.Enabled;
    }
    return false;
  }

  public static bool IsFeatureEnabled(string featureName)
  {
    return FeatureFlags.Default.IsEnabled(featureName);
  }

  public class Feature : IFeatureFlag
  {
    private string name;
    private readonly HashSet<Build.Configuration> enabledFor = new HashSet<Build.Configuration>();

    string IFeatureFlag.Name => this.name;

    bool IFeatureFlag.Enabled => this.enabledFor.Contains(Build.Configuration.Release);

    public static FeatureFlags.Feature Create(string name, params Build.Configuration[] config)
    {
      FeatureFlags.Feature feature = new FeatureFlags.Feature()
      {
        name = name
      };
      if (!GenList.NullOrEmpty<Build.Configuration>((IList<Build.Configuration>) config))
        GenCollection.AddRange<Build.Configuration>(feature.enabledFor, (IEnumerable<Build.Configuration>) config);
      return feature;
    }

    public static string Name(FeatureFlags.Feature feature) => feature.name;

    public static void Write(FeatureFlags.Feature feature)
    {
      XmlExporter.WriteString(string.Join<Build.Configuration>('|', (IEnumerable<Build.Configuration>) feature.enabledFor));
    }
  }
}
