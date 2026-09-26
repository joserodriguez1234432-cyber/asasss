// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.VMF_HarmonyPatches.PatchHelper
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Verse;

#nullable disable
namespace VehicleMapFramework.VMF_HarmonyPatches;

public static class PatchHelper
{
  private static readonly FieldInfo f_allBuildingsColonist = AccessTools.Field(typeof (ListerBuildings), "allBuildingsColonist");

  public static IEnumerable<KeyValuePair<OpCode, object>> ReadMethodBodyWrapper(MethodBase method)
  {
    try
    {
      return PatchProcessor.ReadMethodBody(method);
    }
    catch (Exception ex)
    {
      VMF_Log.Warning($"Autopatching to {GeneralExtensions.FullDescription(method)} failed. It may be referencing outdated signatures. The patch will simply be skipped.\n{ex}");
      return (IEnumerable<KeyValuePair<OpCode, object>>) Array.Empty<KeyValuePair<OpCode, object>>();
    }
  }

  public static IEnumerable<MethodBase> WhereCallsMethod(
    this IEnumerable<MethodBase> methods,
    params MethodBase[] targetMethods)
  {
    return methods.Where<MethodBase>((Func<MethodBase, bool>) (method => method.CallsMethod(targetMethods)));
  }

  public static bool CallsMethod(this MethodBase method, params MethodBase[] targetMethods)
  {
    return (object) method != null && PatchHelper.ReadMethodBodyWrapper(method).Any<KeyValuePair<OpCode, object>>((Func<KeyValuePair<OpCode, object>, bool>) (i =>
    {
      MethodBase methodBase = i.Value as MethodBase;
      return (object) methodBase != null && ((IEnumerable<MethodBase>) targetMethods).Contains<MethodBase>(methodBase);
    }));
  }

  public static CodeMatcher AddAltitudeFor(
    this CodeMatcher codeMatcher,
    out LocalBuilder vehicle,
    float offset = 0.0f,
    CodeMatch[] matches = null,
    CodeInstruction[] getInstance = null)
  {
    if (matches == null)
      matches = new CodeMatch[1]
      {
        CodeMatch.Calls(MethodInfoCache.CachedMethodInfo.m_Altitudes_AltitudeFor)
      };
    if (getInstance == null)
      getInstance = new CodeInstruction[1]
      {
        CodeInstruction.LoadArgument(0, false)
      };
    Label label;
    codeMatcher.MatchStartForward(matches).Advance(1).CreateLabel(ref label).DeclareLocal(typeof (VehiclePawnWithMap), ref vehicle).InsertAndAdvance(getInstance).InsertAndAdvance(new CodeInstruction[5]
    {
      new CodeInstruction(OpCodes.Ldloca_S, (object) vehicle),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_IsOnNonFocusedVehicleMapOf),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Ldloc_S, (object) vehicle),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_YOffsetFull)
    });
    if ((double) offset != 0.0)
      codeMatcher.InsertAndAdvance(new CodeInstruction[2]
      {
        new CodeInstruction(OpCodes.Ldc_R4, (object) offset),
        new CodeInstruction(OpCodes.Add, (object) null)
      });
    return codeMatcher;
  }

  public static CodeMatcher AddExtraAngle(
    this CodeMatcher codeMatcher,
    out LocalBuilder vehicle,
    CodeInstruction[] getInstance = null)
  {
    if (getInstance == null)
      getInstance = new CodeInstruction[1]
      {
        CodeInstruction.LoadArgument(0, false)
      };
    return codeMatcher.DeclareLocal(typeof (VehiclePawnWithMap), ref vehicle).InsertAndAdvance(getInstance).InsertAndAdvance(new CodeInstruction[3]
    {
      new CodeInstruction(OpCodes.Ldloca_S, (object) vehicle),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_IsOnNonFocusedVehicleMapOf),
      new CodeInstruction(OpCodes.Pop, (object) null)
    }).AddExtraAngle(vehicle);
  }

  public static CodeMatcher AddExtraAngle(this CodeMatcher codeMatcher, LocalBuilder vehicle)
  {
    Label label;
    return codeMatcher.CreateLabel(ref label).InsertAndAdvance(new CodeInstruction[7]
    {
      new CodeInstruction(OpCodes.Ldloc_S, (object) vehicle),
      new CodeInstruction(OpCodes.Brfalse_S, (object) label),
      new CodeInstruction(OpCodes.Ldloc_S, (object) vehicle),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_ExtraAngle),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.g_Vector3_up),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_Quaternion_AngleAxis),
      PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.o_Quaternion_Multiply)
    });
  }

  public static CodeInstruction get_CallInstruction(MethodInfo methodInfo)
  {
    return new CodeInstruction(OpCodes.Call, (object) methodInfo);
  }

  public static CodeInstruction get_CallvirtInstruction(MethodInfo methodInfo)
  {
    return new CodeInstruction(OpCodes.Callvirt, (object) methodInfo);
  }

  public static List<CodeInstruction> MethodReplacer(
    this IEnumerable<CodeInstruction> instructions,
    MethodInfo from,
    MethodInfo to)
  {
    return instructions.MethodReplacer(PatchHelper.Params<ValueTuple<(MethodBase, MethodBase)>>.Get((((MethodBase) from, (MethodBase) to))));
  }

  public static List<CodeInstruction> MethodReplacer(
    this IEnumerable<CodeInstruction> instructions,
    (MethodInfo, MethodInfo) pair1,
    (MethodInfo, MethodInfo) pair2)
  {
    IEnumerable<CodeInstruction> instructions1 = instructions;
    (MethodInfo, MethodInfo) tuple1 = pair1;
    (MethodBase, MethodBase) valueTuple1 = ((MethodBase) tuple1.Item1, (MethodBase) tuple1.Item2);
    (MethodInfo, MethodInfo) tuple2 = pair2;
    (MethodBase, MethodBase) valueTuple2 = ((MethodBase) tuple2.Item1, (MethodBase) tuple2.Item2);
    (MethodBase, MethodBase)[] valueTupleArray = PatchHelper.Params<((MethodBase, MethodBase), (MethodBase, MethodBase))>.Get((valueTuple1, valueTuple2));
    return instructions1.MethodReplacer(valueTupleArray);
  }

  public static List<CodeInstruction> MethodReplacer(
    this IEnumerable<CodeInstruction> instructions,
    (MethodInfo, MethodInfo) pair1,
    (MethodInfo, MethodInfo) pair2,
    (MethodInfo, MethodInfo) pair3)
  {
    IEnumerable<CodeInstruction> instructions1 = instructions;
    (MethodInfo, MethodInfo) tuple1 = pair1;
    (MethodBase, MethodBase) valueTuple1 = ((MethodBase) tuple1.Item1, (MethodBase) tuple1.Item2);
    (MethodInfo, MethodInfo) tuple2 = pair2;
    (MethodBase, MethodBase) valueTuple2 = ((MethodBase) tuple2.Item1, (MethodBase) tuple2.Item2);
    (MethodInfo, MethodInfo) tuple3 = pair3;
    (MethodBase, MethodBase) valueTuple3 = ((MethodBase) tuple3.Item1, (MethodBase) tuple3.Item2);
    (MethodBase, MethodBase)[] valueTupleArray = PatchHelper.Params<((MethodBase, MethodBase), (MethodBase, MethodBase), (MethodBase, MethodBase))>.Get((valueTuple1, valueTuple2, valueTuple3));
    return instructions1.MethodReplacer(valueTupleArray);
  }

  public static List<CodeInstruction> MethodReplacer(
    this IEnumerable<CodeInstruction> instructions,
    params (MethodBase from, MethodBase to)[] pairs)
  {
    if (!(instructions is List<CodeInstruction> codeInstructionList))
      codeInstructionList = instructions.ToList<CodeInstruction>();
    List<CodeInstruction> source = codeInstructionList;
    int pairCount = pairs.Length;
    int count = source.Count;
    if (count < 500)
    {
      for (int index = 0; index < count; ++index)
        ProcessInstruction(source[index]);
    }
    else
      Parallel.ForEach<CodeInstruction>((IEnumerable<CodeInstruction>) source, new Action<CodeInstruction>(ProcessInstruction));
    return source;

    void ProcessInstruction(CodeInstruction instruction)
    {
      MethodBase operand = instruction.operand as MethodBase;
      if ((object) operand == null)
        return;
      for (int index = 0; index < pairCount; ++index)
      {
        (MethodBase from, MethodBase to) pair = pairs[index];
        if (operand == pair.from)
        {
          instruction.opcode = pair.to.IsConstructor ? OpCodes.Newobj : OpCodes.Call;
          instruction.operand = (object) pair.to;
          break;
        }
      }
    }
  }

  public static IEnumerable<CodeInstruction> AddAllBuildingsColonistForThingInstance(
    this IEnumerable<CodeInstruction> instructions,
    int argumentIndex = 0)
  {
    foreach (CodeInstruction instruction in instructions)
    {
      yield return instruction;
      if (CodeInstructionExtensions.LoadsField(instruction, PatchHelper.f_allBuildingsColonist, false))
      {
        yield return CodeInstruction.LoadArgument(argumentIndex, false);
        yield return PatchHelper.get_CallInstruction(MethodInfoCache.CachedMethodInfo.m_AddColonistBuildingList);
      }
    }
  }

  private static class Params<T> where T : struct, ITuple
  {
    [ThreadStatic]
    private static (MethodBase, MethodBase)[] @params;

    public static (MethodBase, MethodBase)[] Get(T tuple)
    {
      if (PatchHelper.Params<T>.@params == null)
        PatchHelper.Params<T>.@params = new (MethodBase, MethodBase)[tuple.Length];
      for (int index = 0; index < tuple.Length; ++index)
        PatchHelper.Params<T>.@params[index] = ((MethodBase, MethodBase)) tuple[index];
      return PatchHelper.Params<T>.@params;
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00246C7DE3593F499BD1D1FE1E0A0EDB9649
  {
    [ExtensionMarker("<M>$008F3266EB26DABE839F3860179338CE")]
    public CodeMatcher AddAltitudeFor(
      out LocalBuilder vehicle,
      float offset = 0.0f,
      CodeMatch[] matches = null,
      CodeInstruction[] getInstance = null)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$008F3266EB26DABE839F3860179338CE")]
    public CodeMatcher AddExtraAngle(out LocalBuilder vehicle, CodeInstruction[] getInstance = null)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$008F3266EB26DABE839F3860179338CE")]
    public CodeMatcher AddExtraAngle(LocalBuilder vehicle) => throw new NotSupportedException();

    [SpecialName]
    public static class \u003CM\u003E\u0024008F3266EB26DABE839F3860179338CE
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(CodeMatcher codeMatcher)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024C219D7CAAD849B16AAC7CB332D19BED9
  {
    [ExtensionMarker("<M>$C2627B5511556B25762ADEB75E3F6B82")]
    public CodeInstruction CallInstruction
    {
      [ExtensionMarker("<M>$C2627B5511556B25762ADEB75E3F6B82")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$C2627B5511556B25762ADEB75E3F6B82")]
    public CodeInstruction CallvirtInstruction
    {
      [ExtensionMarker("<M>$C2627B5511556B25762ADEB75E3F6B82")] get
      {
        throw new NotSupportedException();
      }
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024C2627B5511556B25762ADEB75E3F6B82
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(MethodInfo methodInfo)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024AC1253223453BF8EA6DA986A4FCA37F9
  {
    [ExtensionMarker("<M>$30822E9DB3DCB0916C3E2C68A3C07E77")]
    public List<CodeInstruction> MethodReplacer(MethodInfo from, MethodInfo to)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$30822E9DB3DCB0916C3E2C68A3C07E77")]
    public List<CodeInstruction> MethodReplacer(
      (MethodInfo, MethodInfo) pair1,
      (MethodInfo, MethodInfo) pair2)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$30822E9DB3DCB0916C3E2C68A3C07E77")]
    public List<CodeInstruction> MethodReplacer(
      (MethodInfo, MethodInfo) pair1,
      (MethodInfo, MethodInfo) pair2,
      (MethodInfo, MethodInfo) pair3)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$30822E9DB3DCB0916C3E2C68A3C07E77")]
    public List<CodeInstruction> MethodReplacer(params (MethodBase from, MethodBase to)[] pairs)
    {
      throw new NotSupportedException();
    }

    [ExtensionMarker("<M>$30822E9DB3DCB0916C3E2C68A3C07E77")]
    public IEnumerable<CodeInstruction> AddAllBuildingsColonistForThingInstance(int argumentIndex = 0)
    {
      throw new NotSupportedException();
    }

    [SpecialName]
    public static class \u003CM\u003E\u002430822E9DB3DCB0916C3E2C68A3C07E77
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(IEnumerable<CodeInstruction> instructions)
      {
      }
    }
  }
}
