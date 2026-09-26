// Decompiled with JetBrains decompiler
// Type: SmashTools.RimWorldTime
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using System.Linq;
using Verse;

#nullable disable
namespace SmashTools;

public struct RimWorldTime(string time)
{
  public string time = time;
  public int ticks = RimWorldTime.ParseToTicks(time);

  public static RimWorldTime FromString(string entry) => new RimWorldTime(entry);

  public static int ParseToTicks(string time)
  {
    if (string.IsNullOrEmpty(time))
      return 0;
    int toTicks = 0;
    foreach (string str in time.Split(',', StringSplitOptions.None))
    {
      int result;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      int.TryParse(string.Concat<char>(str.Where<char>(RimWorldTime.\u003C\u003EO.\u003C0\u003E__IsDigit ?? (RimWorldTime.\u003C\u003EO.\u003C0\u003E__IsDigit = new Func<char, bool>(char.IsDigit)))), out result);
      if (!int.TryParse(str, out int _))
        result *= RimWorldTime.GetTickMultiplier(str[str.Length - 1]);
      toTicks += result;
    }
    return toTicks;
  }

  private static int GetTickMultiplier(char c)
  {
    switch (char.ToLower(c))
    {
      case 'd':
        return 60000;
      case 'h':
        return 2500;
      case 'q':
        return 900000;
      case 't':
        return 1;
      case 'w':
        return 420000;
      case 'y':
        return 3600000;
      default:
        if (!char.IsNumber(c))
          Log.Warning($"Unable to Parse {c} in RimWorldTime String.");
        return 1;
    }
  }

  public override string ToString() => this.time;

  public static string TicksToRealTime(int ticks)
  {
    return ticks <= 0 ? "00:00:00:00" : TimeSpan.FromSeconds((double) (ticks / 60)).ToString("dd\\:hh\\:mm\\:ss");
  }

  public static string TicksToGameTime(int ticks)
  {
    if (ticks <= 0)
      return "00:00:00:00";
    int result1;
    int result2;
    int result3;
    return $"{Math.DivRem(ticks, 60000, out result1):D2}:{Math.DivRem(result1, 2500, out result2):D2}:{Math.DivRem(result2, 42, out result3):D2}:{Math.DivRem(result3, 7, out int _):D2}";
  }
}
