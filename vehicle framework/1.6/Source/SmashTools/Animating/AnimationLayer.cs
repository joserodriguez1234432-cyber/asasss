// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationLayer
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

public class AnimationLayer : IXmlExport
{
  public string name;
  public List<AnimationState> states = new List<AnimationState>();

  public AnimationController Controller { get; internal set; }

  public AnimationState AddState(string name, IntVec2 position, AnimationState.StateType type = AnimationState.StateType.None)
  {
    if (type != AnimationState.StateType.None && GenCollection.Any<AnimationState>(this.states, (Predicate<AnimationState>) (state => state.Type == type)))
    {
      Log.Error($"Attempting to load duplicate special state type to AnimationLayer {name}.");
      return (AnimationState) null;
    }
    if (type == AnimationState.StateType.None && !GenCollection.Any<AnimationState>(this.states, (Predicate<AnimationState>) (state => state.Type == AnimationState.StateType.Default)))
      type = AnimationState.StateType.Default;
    AnimationState to = new AnimationState(name, type);
    to.position = position;
    if (type == AnimationState.StateType.Default)
      this.states.First<AnimationState>((Func<AnimationState, bool>) (state => state.Type == AnimationState.StateType.Entry)).AddTransition(to);
    to.Layer = this;
    this.states.Add(to);
    return to;
  }

  public void RemoveState(string name)
  {
    for (int index = 0; index < this.states.Count; ++index)
    {
      if (this.states[index].name == name)
      {
        this.states.RemoveAt(index);
        break;
      }
    }
  }

  private void ResolveConnections()
  {
    if (this.states.NullOrEmpty<AnimationState>())
      return;
    Dictionary<Guid, AnimationState> dictionary = this.states.ToDictionary<AnimationState, Guid>((Func<AnimationState, Guid>) (state => state.guid));
    foreach (AnimationState state in this.states)
    {
      foreach (AnimationTransition transition in state.transitions)
      {
        transition.FromState = state;
        AnimationState animationState;
        if (dictionary.TryGetValue(transition.toStateGuid, out animationState))
          transition.ToState = animationState;
      }
      int num = state.transitions.RemoveAll((Predicate<AnimationTransition>) (transition => transition.ToState == null));
      if (num > 0)
        Log.Error($"{num} invalid transitions purged! This may break some animation states.");
    }
  }

  internal void ResolveReferences()
  {
    this.ResolveConnections();
    foreach (AnimationState state in this.states)
    {
      state.Layer = this;
      state.ResolveReferences();
    }
  }

  void IXmlExport.Export()
  {
    XmlExporter.WriteObject<string>("name", this.name);
    XmlExporter.WriteCollection<AnimationState>("states", (IEnumerable<AnimationState>) this.states);
  }

  public static AnimationLayer CreateLayer(string name)
  {
    AnimationLayer layer = new AnimationLayer();
    layer.name = name;
    int num1 = 32 /*0x20*/;
    int num2 = num1 + 16 /*0x10*/;
    layer.AddState("Entry", new IntVec2(-num2, 0), AnimationState.StateType.Entry);
    layer.AddState("Exit", new IntVec2(num1, 0), AnimationState.StateType.Exit);
    return layer;
  }
}
