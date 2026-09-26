// Decompiled with JetBrains decompiler
// Type: SmashTools.Targeting.TargeterResult
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System.Collections.Generic;

#nullable disable
namespace SmashTools.Targeting;

[PublicAPI]
public struct TargeterResult
{
  public required TargeterAction action;
  public List<ITargetOption> options;

  public static TargeterResult None
  {
    get
    {
      return new TargeterResult()
      {
        action = TargeterAction.None
      };
    }
  }

  public static TargeterResult Reject
  {
    get
    {
      return new TargeterResult()
      {
        action = TargeterAction.Reject
      };
    }
  }

  public static TargeterResult Cancel
  {
    get
    {
      return new TargeterResult()
      {
        action = TargeterAction.Cancel
      };
    }
  }

  public static TargeterResult Submit
  {
    get
    {
      return new TargeterResult()
      {
        action = TargeterAction.Submit
      };
    }
  }

  public static TargeterResult Accept<T>(List<T> options) where T : ITargetOption
  {
    TargeterResult targeterResult = new TargeterResult();
    targeterResult.action = TargeterAction.Accept;
    List<T> objList = options;
    List<ITargetOption> targetOptionList = new List<ITargetOption>(objList.Count);
    foreach (T obj in objList)
      targetOptionList.Add((ITargetOption) obj);
    targeterResult.options = targetOptionList;
    return targeterResult;
  }
}
