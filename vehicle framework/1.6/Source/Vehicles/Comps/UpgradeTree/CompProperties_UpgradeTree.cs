// Decompiled with JetBrains decompiler
// Type: Vehicles.CompProperties_UpgradeTree
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class CompProperties_UpgradeTree : CompProperties
{
  public UpgradeTreeDef def;
  [Unsaved(false)]
  private Dictionary<UpgradeNode, List<GraphicOverlay>> overlays;

  public CompProperties_UpgradeTree() => this.compClass = typeof (CompUpgradeTree);

  public virtual IEnumerable<string> ConfigErrors(ThingDef parentDef)
  {
    foreach (string configError in base.ConfigErrors(parentDef))
      yield return configError;
    if (this.def == null)
      yield return "<field>def</field> is null. Consider removing CompProperties_UpgradeTree if you don't " + "plan on using upgrades.".ConvertRichText();
    else if (!GenList.NullOrEmpty<UpgradeNode>((IList<UpgradeNode>) this.def.nodes))
    {
      foreach (UpgradeNode node in this.def.nodes)
      {
        if (node.GridCoordinate.x < 0 || node.GridCoordinate.x >= ITab_Vehicle_Upgrades.MaxLinesAcross)
          yield return $"Maximum grid coordinate width={ITab_Vehicle_Upgrades.MaxLinesAcross - 1}. Larger " + $"coordinates are not supported, consider going downward. Coord=({node.GridCoordinate.x}," + $"{node.GridCoordinate.z})".ConvertRichText();
      }
    }
  }

  public virtual void ResolveReferences(ThingDef parentDef)
  {
    base.ResolveReferences(parentDef);
    VehicleDef vehicleDef = parentDef as VehicleDef;
    if (vehicleDef == null)
    {
      Log.Error($"Attaching {((object) this).GetType()} on non-vehicle def. This is not allowed.");
    }
    else
    {
      if (this.def == null || GenList.NullOrEmpty<UpgradeNode>((IList<UpgradeNode>) this.def.nodes))
        return;
      LongEventHandler.ExecuteWhenFinished((Action) (() =>
      {
        this.overlays = new Dictionary<UpgradeNode, List<GraphicOverlay>>();
        foreach (UpgradeNode node in this.def.nodes)
        {
          if (!GenList.NullOrEmpty<GraphicDataOverlay>((IList<GraphicDataOverlay>) node.graphicOverlays))
          {
            foreach (GraphicDataOverlay graphicOverlay1 in node.graphicOverlays)
            {
              GraphicOverlay graphicOverlay2 = GraphicOverlay.Create(graphicOverlay1, vehicleDef);
              graphicOverlay2.data.graphicData.RecacheLayerOffsets();
              this.overlays.AddOrAppend<UpgradeNode, List<GraphicOverlay>, GraphicOverlay>(node, graphicOverlay2);
            }
          }
        }
      }));
    }
  }

  public List<GraphicOverlay> TryGetOverlays(UpgradeNode node)
  {
    Dictionary<UpgradeNode, List<GraphicOverlay>> overlays = this.overlays;
    return overlays == null ? (List<GraphicOverlay>) null : GenCollection.TryGetValue<UpgradeNode, List<GraphicOverlay>>((IReadOnlyDictionary<UpgradeNode, List<GraphicOverlay>>) overlays, node, (List<GraphicOverlay>) null);
  }
}
