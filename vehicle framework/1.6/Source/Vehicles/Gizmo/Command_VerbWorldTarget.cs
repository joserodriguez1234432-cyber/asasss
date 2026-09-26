// Decompiled with JetBrains decompiler
// Type: Vehicles.Command_VerbWorldTarget
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

#nullable disable
namespace Vehicles;

public class Command_VerbWorldTarget : Command
{
  public Verb verb;
  protected List<Verb> groupedVerbs;
  public bool drawRadius = true;

  public virtual Color IconDrawColor
  {
    get
    {
      return this.verb.EquipmentSource != null ? ((Thing) this.verb.EquipmentSource).DrawColor : base.IconDrawColor;
    }
  }

  public virtual void GizmoUpdateOnMouseover()
  {
    if (!this.drawRadius)
      return;
    this.verb.verbProps.DrawRadiusRing(this.verb.caster.Position, (Verb) null);
    if (GenList.NullOrEmpty<Verb>((IList<Verb>) this.groupedVerbs))
      return;
    foreach (Verb groupedVerb in this.groupedVerbs)
      groupedVerb.verbProps.DrawRadiusRing(groupedVerb.caster.Position, (Verb) null);
  }

  public virtual void MergeWith(Gizmo other)
  {
    ((Gizmo) this).MergeWith(other);
    if (!(other is Command_VerbWorldTarget commandVerbWorldTarget))
    {
      Log.ErrorOnce("Tried to merge Command_VerbTarget with unexpected type", 73406263);
    }
    else
    {
      if (this.groupedVerbs == null)
        this.groupedVerbs = new List<Verb>();
      this.groupedVerbs.Add(commandVerbWorldTarget.verb);
      if (commandVerbWorldTarget.groupedVerbs == null)
        return;
      this.groupedVerbs.AddRange((IEnumerable<Verb>) commandVerbWorldTarget.groupedVerbs);
    }
  }

  public virtual void ProcessInput(Event ev)
  {
    base.ProcessInput(ev);
    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Tick_Tiny, (Map) null);
  }
}
