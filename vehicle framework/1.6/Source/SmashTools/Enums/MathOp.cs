// Decompiled with JetBrains decompiler
// Type: SmashTools.MathOp
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;
using UnityEngine;

#nullable disable
namespace SmashTools;

public static class MathOp
{
  public static float Apply(this OperationType operationType, int x, int y)
  {
    float num;
    switch (operationType)
    {
      case OperationType.Addition:
        num = (float) (x + y);
        break;
      case OperationType.Subtraction:
        num = (float) (x - y);
        break;
      case OperationType.Multiplication:
        num = (float) (x * y);
        break;
      case OperationType.Division:
        num = (float) (x / y);
        break;
      case OperationType.Remainder:
        num = (float) (x % y);
        break;
      case OperationType.Root:
        num = Mathf.Pow((float) Mathf.Abs(y), (float) (1 / x));
        break;
      case OperationType.Pow:
        num = Mathf.Pow((float) x, (float) y);
        break;
      default:
        num = 0.0f;
        break;
    }
    return num;
  }

  public static float Apply(this OperationType operationType, float x, float y)
  {
    float num;
    switch (operationType)
    {
      case OperationType.Addition:
        num = x + y;
        break;
      case OperationType.Subtraction:
        num = x - y;
        break;
      case OperationType.Multiplication:
        num = x * y;
        break;
      case OperationType.Division:
        num = x / y;
        break;
      case OperationType.Remainder:
        num = x % y;
        break;
      case OperationType.Root:
        num = Mathf.Pow(Mathf.Abs(y), 1f / x);
        break;
      case OperationType.Pow:
        num = Mathf.Pow(x, y);
        break;
      default:
        num = 0.0f;
        break;
    }
    return num;
  }

  public static bool Compare(this ComparisonType comparisonType, int x, int y)
  {
    switch (comparisonType)
    {
      case ComparisonType.LessThan:
        return x < y;
      case ComparisonType.LessThanOrEqual:
        return x <= y;
      case ComparisonType.Equal:
        return x == y;
      case ComparisonType.GreaterThan:
        return x > y;
      case ComparisonType.GreaterThanOrEqual:
        return x >= y;
      case ComparisonType.NotEqual:
        return x != y;
      default:
        throw new NotImplementedException(comparisonType.ToString());
    }
  }

  public static bool Compare(this ComparisonType comparisonType, float x, float y)
  {
    switch (comparisonType)
    {
      case ComparisonType.LessThan:
        return (double) x < (double) y;
      case ComparisonType.LessThanOrEqual:
        return (double) x <= (double) y;
      case ComparisonType.Equal:
        return (double) x == (double) y;
      case ComparisonType.GreaterThan:
        return (double) x > (double) y;
      case ComparisonType.GreaterThanOrEqual:
        return (double) x >= (double) y;
      case ComparisonType.NotEqual:
        return (double) x != (double) y;
      default:
        throw new NotImplementedException(comparisonType.ToString());
    }
  }
}
