// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationManager
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

#nullable disable
namespace SmashTools.Animations;

public class AnimationManager : IExposable
{
  public readonly IAnimator animator;
  public readonly AnimationController controller;
  private AnimationManager.LayerData[] layerDatas;
  private Dictionary<ushort, float> parameters = new Dictionary<ushort, float>();

  public AnimationManager(IAnimator animator, AnimationController controller)
  {
    this.animator = animator;
    this.controller = controller;
    this.Init(animator, controller);
  }

  private void Init(IAnimator animator, AnimationController controller)
  {
    this.layerDatas = new AnimationManager.LayerData[controller.layers.Count];
    for (int index = 0; index < controller.layers.Count; ++index)
      this.layerDatas[index] = new AnimationManager.LayerData(animator, controller.layers[index]);
    foreach (Def def in DefDatabase<AnimationParameterDef>.AllDefsListForReading)
      this.parameters[def.shortHash] = 0.0f;
    if (controller.parameters.NullOrEmpty<AnimationParameter>())
      return;
    foreach (AnimationParameter parameter in controller.parameters)
      this.parameters[parameter.Id] = parameter.Value;
  }

  public void PostLoad()
  {
    foreach (AnimationManager.LayerData layerData in this.layerDatas)
      layerData.PostLoad();
  }

  public void AnimationTick()
  {
    for (int index = 0; index < this.controller.layers.Count; ++index)
    {
      AnimationManager.LayerData layerData = this.layerDatas[index];
      if (!layerData.IsValid)
        this.StartNextState(layerData);
      else if (layerData.frame >= layerData.state.clip.frameCount)
        this.StartNextState(layerData);
      else
        layerData.Update();
    }
  }

  void IExposable.ExposeData()
  {
    Scribe_Collections.Look<ushort, float>(ref this.parameters, "parameters", (LookMode) 1, (LookMode) 1);
    Scribe_Array.Look<AnimationManager.LayerData>(ref this.layerDatas, "layerDatas", lookMode: (LookMode) 2);
  }

  private void Transition(AnimationManager.LayerData layerData)
  {
    if (!layerData.Transitioning)
      layerData.EvaluateTransition();
    if (layerData.TransitionTick < layerData.transition.exitTicks)
      return;
    this.StartNextState(layerData);
  }

  private void StartNextState(AnimationManager.LayerData layerData)
  {
    foreach (AnimationTransition transition in layerData.state.transitions)
    {
      if (transition.conditions.NullOrEmpty<AnimationCondition>())
      {
        layerData.SetState(transition.ToState);
        return;
      }
      foreach (AnimationCondition condition in transition.conditions)
      {
        float parameter = this.parameters[condition.Def.shortHash];
        if (condition.ConditionMet(parameter))
        {
          layerData.SetState(transition.ToState);
          return;
        }
      }
    }
    if (!layerData.IsValid || !layerData.state.loop)
      return;
    layerData.frame = 0;
  }

  internal (AnimationState state, int frame) CurrentFrame(AnimationLayer layer)
  {
    foreach (AnimationManager.LayerData layerData in this.layerDatas)
    {
      if (layerData.layer == layer)
        return (layerData.state, layerData.frame);
    }
    return ((AnimationState) null, 0);
  }

  internal void SetFrame(AnimationClip clip, int frame)
  {
    foreach (AnimationManager.LayerData layerData in this.layerDatas)
      layerData.SetFrame(clip, frame);
  }

  public void SetFloat(string name, float value)
  {
    this.SetFloat(DefDatabase<AnimationParameterDef>.GetNamed(name, true), value);
  }

  public void SetFloat(AnimationParameterDef paramDef, float value)
  {
    this.SetFloat(paramDef.shortHash, value);
  }

  public void SetFloat(ushort id, float value) => this.parameters[id] = value;

  public void SetInt(AnimationParameterDef paramDef, int value)
  {
    this.SetInt(paramDef.shortHash, value);
  }

  public void SetInt(ushort id, int value) => this.SetFloat(id, (float) value);

  public void SetBool(AnimationParameterDef paramDef, bool value)
  {
    this.SetBool(paramDef.shortHash, value);
  }

  public void SetBool(ushort id, bool value) => this.SetFloat(id, (float) (value ? 1 : 0));

  public void SetTrigger(AnimationParameterDef paramDef, bool value)
  {
    this.SetTrigger(paramDef.shortHash, value);
  }

  public void SetTrigger(ushort id, bool value) => this.SetFloat(id, (float) (value ? 1 : 0));

  public float GetFloat(AnimationParameterDef paramDef) => this.GetFloat(paramDef.shortHash);

  public float GetFloat(ushort id) => this.parameters[id];

  public int GetInt(AnimationParameterDef paramDef) => this.GetInt(paramDef.shortHash);

  public int GetInt(ushort id) => (int) this.parameters[id];

  public bool GetBool(AnimationParameterDef paramDef) => this.GetBool(paramDef.shortHash);

  public bool GetBool(ushort id) => (double) this.parameters[id] != 0.0;

  public bool GetTrigger(AnimationParameterDef paramDef) => this.GetBool(paramDef.shortHash);

  public bool GetTrigger(ushort id) => this.GetBool(id);

  private class LayerData : IExposable
  {
    private readonly IAnimator animator;
    public readonly AnimationLayer layer;
    public readonly AnimationState defaultState;
    public int frame;
    public AnimationState state;
    public AnimationState nextState;
    public AnimationTransition transition;
    public bool paused;

    public LayerData(IAnimator animator, AnimationLayer layer)
    {
      this.animator = animator;
      this.layer = layer;
      this.defaultState = GenCollection.FirstOrDefault<AnimationState>(layer.states, (Predicate<AnimationState>) (state => state.Type == AnimationState.StateType.Default));
      this.state = this.defaultState;
    }

    public Dictionary<FieldInfo, float> Defaults { get; private set; } = new Dictionary<FieldInfo, float>();

    private Dictionary<AnimationState, IAnimationObject[]> StateObjects { get; } = new Dictionary<AnimationState, IAnimationObject[]>();

    public bool IsValid => (bool) this.state.clip;

    public int TransitionTick => this.frame - this.state.clip.frameCount;

    public bool Transitioning => this.transition != null;

    public bool WriteDefaults
    {
      get
      {
        return this.state.writeDefaults && (bool) this.state.clip && !this.state.clip.properties.NullOrEmpty<AnimationPropertyParent>();
      }
    }

    public void PostLoad() => this.MapAnimationObjects();

    public void Update()
    {
      for (int index = 0; index < this.state.PropertyCount; ++index)
      {
        IAnimationObject animationObject = this.StateObjects[this.state][index];
        this.state.clip.properties[index].EvaluateFrame(animationObject, this.frame);
      }
      for (int index = 0; index < this.state.clip.events.Count; ++index)
      {
        AnimationEvent animationEvent = this.state.clip.events[index];
        if (animationEvent.frame == this.frame)
          animationEvent.method.Invoke((object) this.animator, (object) this.animator);
      }
      ++this.frame;
    }

    internal void SetFrame(AnimationClip clip, int frame)
    {
      for (int index = 0; index < clip.properties.Count; ++index)
      {
        IAnimationObject animationObject = clip.properties[index].ObjectFromHierarchy(this.animator);
        clip.properties[index].EvaluateFrame(animationObject, frame);
      }
    }

    public void EvaluateTransition() => throw new NotImplementedException();

    public void SetState(AnimationState state)
    {
      this.RestoreDefaults();
      this.frame = 0;
      if (state.Type == AnimationState.StateType.Exit)
        state = this.defaultState;
      this.state = state;
      if (!this.IsValid)
        return;
      this.CacheDefaults();
    }

    private void CacheDefaults()
    {
      if (!this.WriteDefaults || !this.IsValid)
        return;
      this.Defaults.Clear();
      for (int index1 = 0; index1 < this.state.clip.properties.Count; ++index1)
      {
        AnimationPropertyParent property1 = this.state.clip.properties[index1];
        for (int index2 = 0; index2 < property1.Properties.Count; ++index2)
        {
          AnimationProperty property2 = property1.Properties[index2];
          IAnimationObject animator = this.StateObjects[this.state][index2];
          float num = property2.GetProperty(animator);
          this.Defaults[property2.FieldInfo] = num;
        }
      }
    }

    private void MapAnimationObjects()
    {
      this.StateObjects.Clear();
      for (int index1 = 0; index1 < this.layer.states.Count; ++index1)
      {
        AnimationState state = this.layer.states[index1];
        if (state.clip != null)
        {
          this.StateObjects[state] = new IAnimationObject[state.PropertyCount];
          for (int index2 = 0; index2 < state.clip.properties.Count; ++index2)
          {
            AnimationPropertyParent property = state.clip.properties[index2];
            this.StateObjects[state][index2] = property.ObjectFromHierarchy(this.animator);
          }
        }
      }
    }

    private void RestoreDefaults()
    {
      if (!this.WriteDefaults)
        return;
      for (int index1 = 0; index1 < this.state.clip.properties.Count; ++index1)
      {
        AnimationPropertyParent property1 = this.state.clip.properties[index1];
        for (int index2 = 0; index2 < property1.Properties.Count; ++index2)
        {
          AnimationProperty property2 = property1.Properties[index2];
          IAnimationObject animator = this.StateObjects[this.state][index2];
          float num = this.Defaults[property2.FieldInfo];
          property2.SetProperty(animator, num);
        }
      }
    }

    public void Reset() => this.SetState(this.defaultState);

    void IExposable.ExposeData()
    {
      Scribe_Values.Look<int>(ref this.frame, "frame", 0, false);
      Scribe_Values.Look<Guid>(ref this.state.guid, "nextState", new Guid(), false);
      if (Scribe.mode != 4)
        return;
      this.MapAnimationObjects();
    }
  }
}
