// Decompiled with JetBrains decompiler
// Type: SmashTools.Animations.AnimationProperty
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using SmashTools.Xml;
using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Animations;

public class AnimationProperty : IXmlExport, ISelectableUI
{
  private readonly ObjectPath objectPath;
  private readonly string label;
  private readonly string name;
  private AnimationProperty.PropertyType propertyType;
  private readonly string type;
  private readonly string animatorType;
  public AnimationCurve curve = new AnimationCurve();
  [Unsaved(false)]
  private Type loadedType;
  [Unsaved(false)]
  private Type loadedAnimatorType;
  [Unsaved(false)]
  private Color color;
  [Unsaved(false)]
  private AnimationProperty.SetValue evaluateValue;
  [Unsaved(false)]
  private AnimationProperty.SetValue setValue;
  [Unsaved(false)]
  private AnimationProperty.GetValue getValue;

  public AnimationProperty()
  {
  }

  private AnimationProperty(
    Type animatorType,
    string label,
    string name,
    Type type,
    ObjectPath objectPath)
  {
    this.loadedAnimatorType = animatorType;
    this.label = label;
    this.name = name;
    this.loadedType = type;
    this.objectPath = objectPath;
  }

  public string Label => this.label;

  public string Name => this.name;

  public Type Type => this.loadedType;

  public Type AnimatorType => this.loadedAnimatorType;

  public AnimationProperty.PropertyType PropType => this.propertyType;

  public bool IsValid => this.curve != null;

  public AnimationProperty.GetValue GetProperty => this.getValue;

  public AnimationProperty.SetValue SetProperty => this.setValue;

  public FieldInfo FieldInfo { get; private set; }

  public Color Color
  {
    get
    {
      if (Color.op_Equality(this.color, Color.clear))
        this.color = Random.ColorHSV(0.0f, 1f, 1f, 1f, 0.75f, 1f);
      return this.color;
    }
  }

  public void Evaluate(IAnimationObject obj, int frame) => this.evaluateValue(obj, (float) frame);

  public static AnimationProperty Create(
    Type animatorType,
    string label,
    FieldInfo fieldInfo,
    ObjectPath objectPath)
  {
    AnimationProperty animationProperty = new AnimationProperty(animatorType, label, fieldInfo.Name, fieldInfo.DeclaringType, objectPath);
    animationProperty.propertyType = AnimationProperty.PropertyTypeFrom(fieldInfo.FieldType);
    animationProperty.ResolveReferences();
    return animationProperty;
  }

  internal void ResolveReferences()
  {
    if (this.loadedType == (Type) null && !AnimationPropertyRegistry.CachedTypeByName(this.type, out this.loadedType))
      this.loadedType = ParseHelper.ParseType(this.type);
    if (this.loadedAnimatorType == (Type) null && !AnimationPropertyRegistry.CachedTypeByName(this.animatorType, out this.loadedAnimatorType))
      this.loadedAnimatorType = ParseHelper.ParseType(this.animatorType);
    try
    {
      this.FieldInfo = AccessTools.Field(this.Type, this.name);
      if (this.FieldInfo != (FieldInfo) null)
      {
        this.GenerateEvaluateCurveMethod();
        this.GenerateSetValueMethod();
        this.GenerateGetValueMethod();
      }
      else
        Log.Error($"Unable to load {this.Type.Name}.{this.name} for animation.");
    }
    catch (Exception ex)
    {
      Log.Error($"Exception caught while generating dynamic methods. Exception={ex}");
    }
  }

  private void GenerateEvaluateCurveMethod()
  {
    FieldInfo field = AccessTools.Field(typeof (AnimationProperty), "curve");
    MethodInfo meth = AccessTools.Method(typeof (AnimationCurve), "Function", (Type[]) null, (Type[]) null);
    DynamicMethod dynamicMethod = new DynamicMethod("EvaluateCurveForProperty", typeof (void), new Type[3]
    {
      typeof (AnimationProperty),
      typeof (IAnimationObject),
      typeof (float)
    }, typeof (AnimationProperty).Module, true);
    ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
    ilGenerator.Emit(OpCodes.Ldarg_1);
    ilGenerator.Emit(OpCodes.Castclass, this.AnimatorType);
    if (this.objectPath != null)
    {
      FieldInfo fieldInfo = this.objectPath.FieldInfo;
      if (fieldInfo.FieldType.IsValueType)
        ilGenerator.Emit(OpCodes.Ldflda, fieldInfo);
      else
        ilGenerator.Emit(OpCodes.Ldfld, fieldInfo);
    }
    ilGenerator.Emit(OpCodes.Ldarg_0);
    ilGenerator.Emit(OpCodes.Ldfld, field);
    ilGenerator.Emit(OpCodes.Ldarg_2);
    ilGenerator.Emit(OpCodes.Callvirt, meth);
    switch (this.propertyType)
    {
      case AnimationProperty.PropertyType.Int:
        ilGenerator.Emit(OpCodes.Conv_I4);
        break;
      case AnimationProperty.PropertyType.Bool:
        ilGenerator.Emit(OpCodes.Ldc_R4, 0.0f);
        ilGenerator.Emit(OpCodes.Ceq);
        ilGenerator.Emit(OpCodes.Ldc_I4_0);
        ilGenerator.Emit(OpCodes.Ceq);
        break;
    }
    ilGenerator.Emit(OpCodes.Stfld, this.FieldInfo);
    ilGenerator.Emit(OpCodes.Ret);
    this.evaluateValue = (AnimationProperty.SetValue) dynamicMethod.CreateDelegate(typeof (AnimationProperty.SetValue), (object) this);
  }

  private void GenerateGetValueMethod()
  {
    DynamicMethod dynamicMethod = new DynamicMethod("GetValueForProperty", typeof (float), new Type[2]
    {
      typeof (AnimationProperty),
      typeof (IAnimationObject)
    }, typeof (AnimationProperty).Module, true);
    ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
    ilGenerator.Emit(OpCodes.Ldarg_1);
    ilGenerator.Emit(OpCodes.Castclass, this.AnimatorType);
    if (this.objectPath != null)
    {
      FieldInfo fieldInfo = this.objectPath.FieldInfo;
      if (fieldInfo.FieldType.IsValueType)
        ilGenerator.Emit(OpCodes.Ldflda, fieldInfo);
      else
        ilGenerator.Emit(OpCodes.Ldfld, fieldInfo);
    }
    ilGenerator.Emit(OpCodes.Ldfld, this.FieldInfo);
    switch (this.propertyType)
    {
      case AnimationProperty.PropertyType.Int:
        ilGenerator.Emit(OpCodes.Conv_R4);
        break;
      case AnimationProperty.PropertyType.Bool:
        System.Reflection.Emit.Label label1 = ilGenerator.DefineLabel();
        System.Reflection.Emit.Label label2 = ilGenerator.DefineLabel();
        ilGenerator.Emit(OpCodes.Brtrue_S, label1);
        ilGenerator.Emit(OpCodes.Ldc_I4_0);
        ilGenerator.Emit(OpCodes.Br_S, label2);
        ilGenerator.MarkLabel(label1);
        ilGenerator.Emit(OpCodes.Ldc_I4_1);
        ilGenerator.MarkLabel(label2);
        ilGenerator.Emit(OpCodes.Conv_R4);
        break;
    }
    ilGenerator.Emit(OpCodes.Ret);
    this.getValue = (AnimationProperty.GetValue) dynamicMethod.CreateDelegate(typeof (AnimationProperty.GetValue), (object) this);
  }

  private void GenerateSetValueMethod()
  {
    DynamicMethod dynamicMethod = new DynamicMethod("SetValueForProperty", typeof (void), new Type[3]
    {
      typeof (AnimationProperty),
      typeof (IAnimationObject),
      typeof (float)
    }, typeof (AnimationProperty).Module, true);
    ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
    ilGenerator.Emit(OpCodes.Ldarg_1);
    ilGenerator.Emit(OpCodes.Castclass, this.AnimatorType);
    if (this.objectPath != null)
    {
      FieldInfo fieldInfo = this.objectPath.FieldInfo;
      if (fieldInfo.FieldType.IsValueType)
        ilGenerator.Emit(OpCodes.Ldflda, fieldInfo);
      else
        ilGenerator.Emit(OpCodes.Ldfld, fieldInfo);
    }
    ilGenerator.Emit(OpCodes.Ldarg_2);
    switch (this.propertyType)
    {
      case AnimationProperty.PropertyType.Int:
        ilGenerator.Emit(OpCodes.Conv_I4);
        break;
      case AnimationProperty.PropertyType.Bool:
        ilGenerator.Emit(OpCodes.Ldc_R4, 0.0f);
        ilGenerator.Emit(OpCodes.Ceq);
        ilGenerator.Emit(OpCodes.Ldc_I4_0);
        ilGenerator.Emit(OpCodes.Ceq);
        break;
    }
    ilGenerator.Emit(OpCodes.Stfld, this.FieldInfo);
    ilGenerator.Emit(OpCodes.Ret);
    this.setValue = (AnimationProperty.SetValue) dynamicMethod.CreateDelegate(typeof (AnimationProperty.SetValue), (object) this);
  }

  public static AnimationProperty.PropertyType PropertyTypeFrom(Type type)
  {
    if (type == typeof (float))
      return AnimationProperty.PropertyType.Float;
    if (type == typeof (int))
      return AnimationProperty.PropertyType.Int;
    if (type == typeof (bool))
      return AnimationProperty.PropertyType.Bool;
    throw new NotImplementedException($"{type} is not a supported PropertyType for keyframe-level animation properties.");
  }

  void IXmlExport.Export()
  {
    XmlExporter.WriteElement("objectPath", (IXmlExport) this.objectPath);
    XmlExporter.WriteElement("label", this.label);
    XmlExporter.WriteElement("name", this.name);
    XmlExporter.WriteElement("type", GenTypes.GetTypeNameWithoutIgnoredNamespaces(this.Type));
    XmlExporter.WriteElement("animatorType", GenTypes.GetTypeNameWithoutIgnoredNamespaces(this.AnimatorType));
    XmlExporter.WriteObject<AnimationProperty.PropertyType>("propertyType", this.propertyType);
    XmlExporter.WriteElement("curve", (IXmlExport) this.curve);
  }

  public delegate float GetValue(IAnimationObject animator);

  public delegate void SetValue(IAnimationObject animator, float value);

  public enum PropertyType
  {
    Invalid,
    Float,
    Int,
    Bool,
  }
}
