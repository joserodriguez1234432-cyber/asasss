// Decompiled with JetBrains decompiler
// Type: SmashTools.SmashLog
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

[StaticConstructorOnStartup]
public static class SmashLog
{
  private static string RichTextRegex = "(<i>)|(<\\/i>)|(<b>)|(<\\/b>)|(<color.*?>|<\\/color>)";
  private static string RichTextRegexStartingBrackets = "";
  private static string RichTextRegexEndingBrackets = "";
  private static readonly List<(string, Color)> bracketColor = new List<(string, Color)>();

  static SmashLog()
  {
    SmashLog.RegisterRichTextBracket("text", Color.white);
    SmashLog.RegisterRichTextBracket("field", new Color(0.5f, 0.35f, 0.95f));
    SmashLog.RegisterRichTextBracket("property", new Color(0.05f, 0.5f, 1f));
    SmashLog.RegisterRichTextBracket("method", new Color(1f, 0.65f, 0.0f));
    SmashLog.RegisterRichTextBracket("struct", new Color(0.0f, 0.75f, 0.4f));
    SmashLog.RegisterRichTextBracket("class", new Color(0.0f, 0.65f, 0.5f));
    SmashLog.RegisterRichTextBracket("type", new Color(0.0f, 0.65f, 0.5f));
    SmashLog.RegisterRichTextBracket("success", new Color(0.0f, 0.5f, 0.0f));
    SmashLog.RegisterRichTextBracket("error", ColorLibrary.LogError);
    SmashLog.RegisterRichTextBracket("warning", Color.yellow);
    SmashLog.RegisterRichTextBracket("mod", new Color(0.0f, 0.5f, 0.5f));
    SmashLog.RegisterRichTextBracket("attribute", new Color(1f, 0.4f, 0.4f));
    SmashLog.RegisterRichTextBracket("xml", new Color(0.25f, 0.75f, 0.95f));
  }

  [Obsolete("Do not use this method outside of development.")]
  public static void QuickMessage(string text)
  {
    Log.Clear();
    Log.Message(text);
  }

  public static void Message(string text) => Log.Message(text.ColorizeBrackets());

  public static void ErrorLabel(string label, string text)
  {
    SmashLog.Error($"{$"<error>{label}</error>".ColorizeBrackets()} {text.ColorizeBrackets()}");
  }

  public static void Error(string text)
  {
    try
    {
      if (DebugSettings.pauseOnError && Current.ProgramState == 2)
        Find.TickManager.Pause();
      Log.Message(text.ColorizeBrackets());
      if (PlayDataLoader.Loaded && !Prefs.DevMode)
        return;
      Log.TryOpenLogWindow();
    }
    catch (Exception ex)
    {
      Log.Error($"An error occurred while logging an error with a label: {ex}");
    }
  }

  public static void ErrorOnce(string text, int key) => Log.ErrorOnce(text.ColorizeBrackets(), key);

  public static string ColorizeBrackets(this string text)
  {
    foreach ((string str, Color color) in SmashLog.bracketColor)
    {
      text = Regex.Replace(text, $"\\<{str}.*?\\>", $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>", RegexOptions.Singleline);
      text = Regex.Replace(text, $"\\<\\/{str}\\>", "</color>", RegexOptions.Singleline);
    }
    return text;
  }

  private static void RegisterRichTextBracket(string textLabel, Color color)
  {
    SmashLog.RichTextRegex = $"{SmashLog.RichTextRegex}|(<{textLabel}.*?\\>|\\<\\/{textLabel}\\>)";
    if (!SmashLog.RichTextRegexStartingBrackets.NullOrEmpty<char>())
      SmashLog.RichTextRegexStartingBrackets += "|";
    if (!SmashLog.RichTextRegexEndingBrackets.NullOrEmpty<char>())
      SmashLog.RichTextRegexEndingBrackets += "|";
    SmashLog.RichTextRegexStartingBrackets = $"{SmashLog.RichTextRegexStartingBrackets}(<{textLabel}.*?\\>)";
    SmashLog.RichTextRegexEndingBrackets = $"{SmashLog.RichTextRegexEndingBrackets}(\\<\\/{textLabel}\\>)";
    SmashLog.bracketColor.Add((textLabel, color));
  }

  internal static void TestAllBrackets()
  {
    string text = "Testing all brackets: ";
    foreach ((string str, Color _) in SmashLog.bracketColor)
      text = $"{text}<{str}>{str}</{str}> ";
    SmashLog.Message(text);
  }

  public static IEnumerable<CodeInstruction> RemoveRichTextFromDebugLogTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo method = AccessTools.Method(typeof (Debug), "Log", new Type[1]
    {
      typeof (object)
    }, (Type[]) null);
    return SmashLog.RemoveBracketsForMethodCall(instructions, method);
  }

  public static IEnumerable<CodeInstruction> RemoveRichTextFromDebugLogWarningTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo method = AccessTools.Method(typeof (Debug), "LogWarning", new Type[1]
    {
      typeof (object)
    }, (Type[]) null);
    return SmashLog.RemoveBracketsForMethodCall(instructions, method);
  }

  public static IEnumerable<CodeInstruction> RemoveRichTextFromDebugLogErrorTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    MethodInfo method = AccessTools.Method(typeof (Debug), "LogError", new Type[1]
    {
      typeof (object)
    }, (Type[]) null);
    return SmashLog.RemoveBracketsForMethodCall(instructions, method);
  }

  public static IEnumerable<CodeInstruction> RemoveRichTextMessageDetailsTranspiler(
    IEnumerable<CodeInstruction> instructions)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (CodeInstructionExtensions.LoadsField(instruction, AccessTools.Field(typeof (LogMessage), "text"), false))
      {
        yield return instruction;
        instruction = instructionList[++i];
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (SmashLog), "LogTextWithoutRichText", (Type[]) null, (Type[]) null));
      }
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static IEnumerable<CodeInstruction> RemoveBracketsForMethodCall(
    IEnumerable<CodeInstruction> instructions,
    MethodInfo method)
  {
    List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
    for (int i = 0; i < instructionList.Count; ++i)
    {
      CodeInstruction instruction = instructionList[i];
      if (CodeInstructionExtensions.Calls(instruction, method))
        yield return new CodeInstruction(OpCodes.Call, (object) AccessTools.Method(typeof (SmashLog), "LogTextWithoutRichText", (Type[]) null, (Type[]) null));
      yield return instruction;
      instruction = (CodeInstruction) null;
    }
  }

  private static string LogTextWithoutRichText(string text)
  {
    try
    {
      return Regex.Replace(text, SmashLog.RichTextRegex, "", RegexOptions.Singleline, TimeSpan.FromMilliseconds(50.0));
    }
    catch (RegexMatchTimeoutException ex)
    {
    }
    return text;
  }
}
