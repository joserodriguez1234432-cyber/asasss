// Decompiled with JetBrains decompiler
// Type: Vehicles.Alert_IdleInVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using System.Collections.Generic;
using System.Text;
using Verse;

#nullable disable
namespace Vehicles;

public class Alert_IdleInVehicle : Alert
{
  private readonly List<Pawn> idlePawns = new List<Pawn>();
  private readonly StringBuilder explanation = new StringBuilder();

  public Alert_IdleInVehicle()
  {
    this.defaultLabel = TaggedString.op_Implicit(Translator.Translate("ColonistsIdle"));
    this.defaultPriority = (AlertPriority) 1;
  }

  private List<Pawn> IdlePawns
  {
    get
    {
      this.idlePawns.Clear();
      foreach (Map map in Find.Maps)
      {
        if (map.IsPlayerHome)
        {
          foreach (VehiclePawn allClaimant in map.GetDetachedMapComponent<VehiclePositionManager>().AllClaimants)
          {
            if (allClaimant.IdlePawnsInVehicle)
              this.idlePawns.AddRange((IEnumerable<Pawn>) allClaimant.AllPawnsAboard);
          }
        }
      }
      return this.idlePawns;
    }
  }

  public virtual string GetLabel()
  {
    List<Pawn> idlePawns = this.IdlePawns;
    return TaggedString.op_Implicit(idlePawns.Count == 1 ? Translator.Translate("ColonistIdle") : TranslatorFormattedStringExtensions.Translate("ColonistsIdle", NamedArgument.op_Implicit(GenString.ToStringCached(idlePawns.Count))));
  }

  public virtual TaggedString GetExplanation()
  {
    using (new ClearStringOnDispose(this.explanation))
    {
      foreach (Pawn idlePawn in this.IdlePawns)
      {
        StringBuilder explanation = this.explanation;
        TaggedString nameShortColored = idlePawn.NameShortColored;
        string str = "  - " + ((TaggedString) ref nameShortColored).Resolve();
        explanation.AppendLine(str);
      }
      return TranslatorFormattedStringExtensions.Translate("VF_IdleInVehicle", NamedArgument.op_Implicit(GenText.TrimEndNewlines(this.explanation.ToString())));
    }
  }

  public virtual AlertReport GetReport() => AlertReport.CulpritsAre(this.IdlePawns);
}
