// Decompiled with JetBrains decompiler
// Type: SmashTools.Performance.Profiler
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using Verse;

#nullable enable
namespace SmashTools.Performance;

[PublicAPI]
public static class Profiler
{
  private const int MaxPoolSize = 100;
  private const int PreWarmSize = 50;
  private const 
  #nullable disable
  string HarmonyId = "SmashTools.Profiler";
  private static readonly MethodInfo PatchInjectionMethod = AccessTools.Method(typeof (Profiler), "InjectProfileInstructions", (Type[]) null, (Type[]) null);
  private static readonly MethodInfo ProfileStartMethod = AccessTools.Method(typeof (Profiler), "Begin", (Type[]) null, (Type[]) null);
  private static readonly MethodInfo ProfileStopMethod = AccessTools.Method(typeof (Profiler), "End", (Type[]) null, (Type[]) null);
  private static readonly Harmony Harmony;
  private static readonly ObjectPool<Profiler.Timer> TimerPool = new ObjectPool<Profiler.Timer>(100, 50);
  private static readonly ObjectPool<Profiler.Result> ResultPool = new ObjectPool<Profiler.Result>(100, 50);
  private static readonly ThreadLocal<Stack<Profiler.Timer>> Blocks = new ThreadLocal<Stack<Profiler.Timer>>((Func<Stack<Profiler.Timer>>) (() => new Stack<Profiler.Timer>()));
  private static readonly ConcurrentDictionary<string, Profiler.Summary> ResultBuffer = new ConcurrentDictionary<string, Profiler.Summary>();

  static Profiler()
  {
    Log.Error("ProfilerWatch initialized in release build! This will affect performance.");
  }

  internal static void Enable() => Profiler.ApplyPatches();

  internal static void Disable() => Profiler.Harmony.UnpatchAll("SmashTools.Profiler");

  internal static Profiler.Timer Begin(string label)
  {
    Profiler.Timer timer = Profiler.TimerPool.Get();
    Profiler.Blocks.Value.Push(timer);
    timer.Label = label;
    timer.Start();
    return timer;
  }

  internal static void End(Profiler.Timer timer)
  {
    timer.Stop();
    Profiler.Result current = Profiler.ResultPool.Get();
    current.Record(timer);
    Profiler.Summary orAdd = Profiler.ResultBuffer.GetOrAdd(timer.Label, SummaryFactory());
    Profiler.ResultPool.Return(orAdd.Push(current));
    Profiler.TimerPool.Return(timer);

    static Profiler.Summary SummaryFactory() => new Profiler.Summary(1000);
  }

  internal static IEnumerator<KeyValuePair<string, Profiler.Summary>> GetResults()
  {
    return Profiler.ResultBuffer.GetEnumerator();
  }

  private static void ApplyPatches()
  {
    TaskManager.Run(new Action(ProcessMethods), CancellationToken.None);

    static void ProcessMethods()
    {
      foreach (ModContentPack modContentPack in LoadedModManager.RunningModsListForReading)
      {
        foreach (Assembly loadedAssembly in modContentPack.assemblies.loadedAssemblies)
        {
          foreach (Type type in loadedAssembly.GetTypes())
          {
            foreach (MethodInfo method in type.GetMethods(AccessTools.allDeclared))
            {
              ProfileAttribute profileAttribute;
              if (!method.IsAbstract && GenAttribute.TryGetAttribute<ProfileAttribute>((MemberInfo) method, ref profileAttribute))
              {
                Profiler.Harmony.Patch((MethodBase) method, (HarmonyMethod) null, (HarmonyMethod) null, HarmonyMethod.op_Implicit(Profiler.PatchInjectionMethod), (HarmonyMethod) null);
                Messages.Message(method.Name + " patched for profiling.", MessageTypeDefOf.SilentInput, false);
              }
            }
          }
        }
      }
    }
  }

  private static IEnumerable<CodeInstruction> InjectProfileInstructions(
    IEnumerable<CodeInstruction> instructions,
    ILGenerator ilg,
    MethodBase __originalMethod)
  {
    LocalBuilder timerLocal = ilg.DeclareLocal(typeof (Profiler.Timer));
    yield return new CodeInstruction(OpCodes.Ldstr, (object) $"{__originalMethod.DeclaringType?.Name}.{__originalMethod.Name}");
    yield return new CodeInstruction(OpCodes.Call, (object) Profiler.ProfileStartMethod);
    yield return new CodeInstruction(OpCodes.Stloc_S, (object) timerLocal.LocalIndex);
    foreach (CodeInstruction instruction in instructions)
    {
      if (instruction.opcode == OpCodes.Ret)
      {
        yield return new CodeInstruction(OpCodes.Ldloc_S, (object) timerLocal.LocalIndex);
        yield return new CodeInstruction(OpCodes.Call, (object) Profiler.ProfileStopMethod);
      }
      yield return instruction;
    }
  }

  public enum Measurement
  {
    Seconds,
    Milliseconds,
    Microseconds,
    Nanoseconds,
  }

  internal sealed class Summary
  {
    private readonly RingBuffer<Profiler.Result> buffer;
    private readonly int size;
    private long total;
    private int count;
    private double average;
    private readonly object syncRoot = new object();

    public Summary(int size)
    {
      this.size = size;
      this.buffer = new RingBuffer<Profiler.Result>(size);
      for (int index = 0; index < size; ++index)
        this.buffer.Push(new Profiler.Result());
    }

    public long Total => this.total;

    public int Count => this.count;

    public double Average => this.average;

    internal Profiler.Result Push(Profiler.Result current)
    {
      Profiler.Result result;
      lock (this.syncRoot)
      {
        result = this.buffer.Push(current);
        this.Recalculate(result.Ticks, current.Ticks);
      }
      return result;
    }

    private void Recalculate(long removed, long added)
    {
      this.total -= removed;
      this.total += added;
      if (this.count < this.size)
        ++this.count;
      this.average = (double) this.total / (double) this.count;
    }
  }

  public sealed class Timer : IPoolable
  {
    private readonly Stopwatch stopwatch = new Stopwatch();

    public string Label { get; set; }

    bool IPoolable.InPool { get; set; }

    public long ElapsedTicks => this.stopwatch.ElapsedTicks;

    void IPoolable.Reset()
    {
      this.stopwatch.Reset();
      this.Label = "Invalid";
    }

    public void Start() => this.stopwatch.Restart();

    public void Stop() => this.stopwatch.Stop();
  }

  internal sealed record Result : IPoolable
  {
    private string name;
    private long ticks;

    public string Name => this.name;

    public long Ticks => this.ticks;

    bool IPoolable.InPool { get; set; }

    public void Record(Profiler.Timer timer)
    {
      this.name = timer.Label;
      this.ticks = timer.ElapsedTicks;
    }

    void IPoolable.Reset()
    {
      this.name = (string) null;
      this.ticks = 0L;
    }

    [CompilerGenerated]
    public override int GetHashCode()
    {
      // ISSUE: reference to a compiler-generated field
      return ((EqualityComparer<Type>.Default.GetHashCode(this.EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.name)) * -1521134295 + EqualityComparer<long>.Default.GetHashCode(this.ticks)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.\u003CSmashTools\u002EPerformance\u002EIPoolable\u002EInPool\u003Ek__BackingField);
    }

    [CompilerGenerated]
    public bool Equals(
    #nullable enable
    Profiler.Result? other)
    {
      if ((object) this == (object) other)
        return true;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      return (object) other != null && this.EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(this.name, other.name) && EqualityComparer<long>.Default.Equals(this.ticks, other.ticks) && EqualityComparer<bool>.Default.Equals(this.\u003CSmashTools\u002EPerformance\u002EIPoolable\u002EInPool\u003Ek__BackingField, other.\u003CSmashTools\u002EPerformance\u002EIPoolable\u002EInPool\u003Ek__BackingField);
    }
  }
}
