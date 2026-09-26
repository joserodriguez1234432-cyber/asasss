// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationState
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

public class AnimationState : IXmlExport, IDisposable
{
  public string name;
  public Guid guid;
  public IntVec2 position;
  public AnimationClip clip;
  public float speed = 1f;
  public bool loop;
  public bool writeDefaults = true;
  private AnimationState.StateType stateType;
  public List<AnimationTransition> transitions = new List<AnimationTransition>();
  [Unsaved(false)]
  public List<AnimationTransition> transitionsIncoming = new List<AnimationTransition>();

  public AnimationState()
  {
  }

  public AnimationState(string name, AnimationState.StateType stateType)
  {
    this.name = name;
    this.stateType = stateType;
    this.guid = Guid.NewGuid();
  }

  public AnimationState.StateType Type => this.stateType;

  public bool IsPermanent
  {
    get
    {
      return this.Type == AnimationState.StateType.Entry || this.Type == AnimationState.StateType.Exit || this.Type == AnimationState.StateType.Any;
    }
  }

  public AnimationLayer Layer { get; internal set; }

  public int PropertyCount
  {
    get
    {
      return this.clip == null ? 0 : this.clip.properties.Sum<AnimationPropertyParent>((Func<AnimationPropertyParent, int>) (parent => parent.Properties.Count));
    }
  }

  public void AddTransition(AnimationState to)
  {
    AnimationTransition animationTransition = new AnimationTransition(this, to);
    this.transitions.Add(animationTransition);
    to.transitionsIncoming.Add(animationTransition);
  }

  public void Dispose()
  {
    for (int index = this.transitions.Count - 1; index >= 0; --index)
      this.transitions[index].Dispose();
    for (int index = this.transitionsIncoming.Count - 1; index >= 0; --index)
      this.transitionsIncoming[index].Dispose();
  }

  internal void ResolveReferences()
  {
    if (this.transitions.NullOrEmpty<AnimationTransition>())
      return;
    foreach (AnimationTransition transition in this.transitions)
    {
      transition.FromState = this;
      transition.ResolveReferences();
    }
  }

  void IXmlExport.Export()
  {
    XmlExporter.WriteObject<string>("name", this.name);
    XmlExporter.WriteObject<Guid>("guid", this.guid);
    XmlExporter.WriteObject<IntVec2>("position", this.position);
    XmlExporter.WriteObject<Guid?>("clip", this.clip?.Guid);
    XmlExporter.WriteObject<float>("speed", this.speed);
    XmlExporter.WriteObject<bool>("loop", this.loop);
    XmlExporter.WriteObject<bool>("writeDefaults", this.writeDefaults);
    XmlExporter.WriteObject<AnimationState.StateType>("stateType", this.stateType);
    XmlExporter.WriteCollection<AnimationTransition>("transitions", (IEnumerable<AnimationTransition>) this.transitions);
  }

  public AnimationState CreateCopy(AnimationLayer layer)
  {
    AnimationState copy = new AnimationState();
    copy.name = AnimationLoader.GetAvailableName(layer.states.Select<AnimationState, string>((Func<AnimationState, string>) (l => l.name)), this.name);
    copy.guid = Guid.NewGuid();
    copy.position = this.position;
    copy.clip = this.clip;
    copy.speed = this.speed;
    copy.writeDefaults = this.writeDefaults;
    copy.stateType = this.stateType;
    copy.transitions = this.transitions.Select<AnimationTransition, AnimationTransition>((Func<AnimationTransition, AnimationTransition>) (transition => transition.CreateCopy())).ToList<AnimationTransition>();
    if (this.stateType == AnimationState.StateType.Default)
      copy.stateType = AnimationState.StateType.None;
    return copy;
  }

  public enum StateType
  {
    None,
    Entry,
    Default,
    Exit,
    Any,
  }
}
