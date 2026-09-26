// Decompiled with JetBrains decompiler
// Type: SmashTools.AnimationTargetHandler
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Verse;

#nullable disable
namespace SmashTools;

public static class AnimationTargetHandler
{
  public static List<AnimatorObject> GetAnimators(
    this IAnimationTarget animationTarget,
    StringBuilder stringBuilder = null)
  {
    List<AnimatorObject> animators = new List<AnimatorObject>();
    foreach (ThingComp allComp in animationTarget.Thing.AllComps)
    {
      stringBuilder?.AppendLine($"Starting Traversal on {animationTarget}.{allComp.GetType()}");
      foreach (AnimatorObject animatorObject in AnimationTargetHandler.GetAnimatorRecursive((object) allComp, string.Empty, string.Empty, stringBuilder))
        animators.Add(animatorObject);
    }
    return animators;
  }

  private static IEnumerable<AnimatorObject> GetAnimatorRecursive(
    object parent,
    string category,
    string prefix,
    StringBuilder stringBuilder = null)
  {
    if (parent != null)
    {
      FieldInfo[] fieldInfoArray = parent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      for (int index = 0; index < fieldInfoArray.Length; ++index)
      {
        FieldInfo fieldInfo = fieldInfoArray[index];
        string str = category;
        GraphEditableAttribute editableAttribute;
        if (GenAttribute.TryGetAttribute<GraphEditableAttribute>((MemberInfo) fieldInfo, ref editableAttribute))
        {
          if (fieldInfo.FieldType.IsClass)
          {
            if (!editableAttribute.Category.NullOrEmpty<char>())
            {
              str = !str.NullOrEmpty<char>() ? $"{str}.{editableAttribute.Category}" : editableAttribute.Category;
              stringBuilder?.AppendLine("Starting Category: " + str);
            }
            stringBuilder?.AppendLine($"Processing <method>{fieldInfo.DeclaringType}.{fieldInfo.Name}</method> (Type=<type>{fieldInfo.FieldType}</type>)");
            object instance = fieldInfo.GetValue(parent);
            if (instance == null && GenTypes.SameOrSubclassOf(fieldInfo.FieldType, typeof (LinearCurve)))
            {
              instance = Activator.CreateInstance(fieldInfo.FieldType);
              fieldInfo.SetValue(parent, instance);
            }
            if (instance is LinearCurve)
            {
              if (str.NullOrEmpty<char>())
              {
                Log.Error($"Attempting to add {fieldInfo.Name} to GraphEditor cache with no category.  Must assign category name to either containing objects or the field itself.");
              }
              else
              {
                stringBuilder?.AppendLine($"Adding {instance} to category=\"{str}\"");
                yield return new AnimatorObject(parent, fieldInfo, str, prefix);
              }
            }
            else
            {
              foreach (AnimatorObject animatorObject in AnimationTargetHandler.GetAnimatorRecursive(instance, str, editableAttribute.Prefix, stringBuilder))
                yield return animatorObject;
            }
          }
          else
            Log.Error($"Attempting to add {fieldInfo.DeclaringType}.{fieldInfo.Name} to Graph Editor. Field must be a reference type for editing to work. Skipping...");
        }
      }
      fieldInfoArray = (FieldInfo[]) null;
    }
  }
}
