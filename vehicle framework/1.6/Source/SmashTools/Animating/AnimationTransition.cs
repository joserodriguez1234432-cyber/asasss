// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationTransition
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using SmashTools.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

#nullable disable
namespace SmashTools.Animations;

public class AnimationTransition : IXmlExport, IDisposable
{
  public int exitTicks;
  public Guid toStateGuid;
  public List<AnimationCondition> conditions = new List<AnimationCondition>();

  public AnimationTransition()
  {
  }

  public AnimationTransition(AnimationState from, AnimationState to)
  {
    this.FromState = from;
    this.ToState = to;
    this.toStateGuid = to.guid;
  }

  public AnimationState FromState { get; internal set; }

  public AnimationState ToState { get; internal set; }

  public bool DefaultTransition
  {
    get
    {
      return this.FromState != null && this.FromState.Type == AnimationState.StateType.Entry && this.ToState != null && this.ToState.Type == AnimationState.StateType.Default;
    }
  }

  public void Dispose()
  {
    Trace.IsTrue(this.FromState.transitions.Remove(this));
    Trace.IsTrue(this.ToState.transitionsIncoming.Remove(this));
    this.FromState = (AnimationState) null;
    this.ToState = (AnimationState) null;
  }

  public AnimationTransition CreateCopy()
  {
    return new AnimationTransition()
    {
      exitTicks = this.exitTicks,
      conditions = new List<AnimationCondition>((IEnumerable<AnimationCondition>) this.conditions)
    };
  }

  public void AddCondition()
  {
    AnimationCondition animationCondition = new AnimationCondition();
    animationCondition.Transition = this;
    if (animationCondition.Parameter == null)
    {
      AnimationParameterDef def = DefDatabase<AnimationParameterDef>.AllDefsListForReading.FirstOrDefault<AnimationParameterDef>();
      animationCondition.Parameter = new AnimationParameter(def);
    }
    this.conditions.Add(animationCondition);
  }

  internal void ResolveReferences()
  {
    if (this.conditions.NullOrEmpty<AnimationCondition>())
      return;
    foreach (AnimationCondition condition in this.conditions)
    {
      condition.Transition = this;
      condition.ResolveReferences();
    }
  }

  void IXmlExport.Export()
  {
    XmlExporter.WriteObject<int>("exitTicks", this.exitTicks);
    XmlExporter.WriteObject<Guid>("toStateGuid", this.toStateGuid);
    XmlExporter.WriteCollection<AnimationCondition>("conditions", (IEnumerable<AnimationCondition>) this.conditions);
  }
}
