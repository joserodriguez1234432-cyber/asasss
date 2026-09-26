// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Rot8Utility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using HarmonyLib;
using SmashTools;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class Rot8Utility
{
  public static readonly AccessTools.StructFieldRef<Rot4, byte> rot4Int = AccessTools.StructFieldRefAccess<Rot4, byte>("rotInt");
  private const float sin45 = 0.707106769f;

  public static IntVec3 RighthandCell(ref Rot4 rot)
  {
    Rot8Utility.Rotate(ref rot, (RotationDirection) 1);
    return ((Rot4) ref rot).FacingCell;
  }

  public static Quaternion AsQuat(ref Rot8 rot) => rot.AsQuat();

  public static Quaternion AsQuat(this Rot8 rot)
  {
    switch (((Rot8) ref rot).AsInt)
    {
      case 0:
        return Quaternion.identity;
      case 1:
        return Quaternion.LookRotation(Vector3.right);
      case 2:
        return Quaternion.LookRotation(Vector3.back);
      case 3:
        return Quaternion.LookRotation(Vector3.left);
      case 4:
        return Quaternion.LookRotation(new Vector3(1f, 0.0f, 1f));
      case 5:
        return Quaternion.LookRotation(new Vector3(1f, 0.0f, -1f));
      case 6:
        return Quaternion.LookRotation(new Vector3(-1f, 0.0f, -1f));
      case 7:
        return Quaternion.LookRotation(new Vector3(-1f, 0.0f, 1f));
      default:
        Log.Error("ToQuat with Rot = " + ((Rot8) ref rot).AsInt.ToString());
        return Quaternion.identity;
    }
  }

  public static Rot4 AsRot4Force(this Rot8 rot)
  {
    Rot4 rot4 = new Rot4();
    Rot8Utility.rot4Int.Invoke(ref rot4) = ((Rot8) ref rot).AsByte;
    return rot4;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Rot8 Rotated(this Rot8 rot, Rot8 other)
  {
    return new Rot8(Rot8.FromIntClockwise((((Rot8) ref rot).AsIntClockwise + ((Rot8) ref other).AsIntClockwise) % 8));
  }

  public static void Rotate(ref Rot4 rot, RotationDirection rotDir)
  {
    int asInt = ((Rot4) ref rot).AsInt;
    if (asInt < 0 || asInt > 7)
      return;
    Rot8 rot8;
    // ISSUE: explicit constructor call
    ((Rot8) ref rot8).\u002Ector(((Rot4) ref rot).AsInt);
    int asIntClockwise = ((Rot8) ref rot8).AsIntClockwise;
    switch ((int) rotDir)
    {
      case 1:
        asIntClockwise += 2;
        break;
      case 2:
        asIntClockwise += 4;
        break;
      case 3:
        asIntClockwise -= 2;
        break;
    }
    ((Rot8) ref rot8).AsInt = Rot8.FromIntClockwise(GenMath.PositiveMod(asIntClockwise, 8));
    Rot8Utility.rot4Int.Invoke(ref rot) = ((Rot8) ref rot8).AsByte;
  }

  public static Vector3 ToFundVector3(ref IntVec3 intVec)
  {
    return ((IntVec3) ref intVec).IsCardinal ? ((IntVec3) ref intVec).ToVector3() : Vector3.op_Multiply(((IntVec3) ref intVec).ToVector3(), 0.707106769f);
  }

  public static Vector3 AsFundVector2(ref Rot8 rot)
  {
    Vector2 vector2 = ((Rot8) ref rot).AsVector2;
    if (((Rot8) ref rot).IsDiagonal)
      vector2 = Vector2.op_Multiply(vector2, 0.707106769f);
    return Vector2.op_Implicit(vector2);
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024DE9C66616AB8661ABA0CF1A601217E37
  {
    [ExtensionMarker("<M>$ACAC8CACEECE00D8841643D3E36C0E59")]
    public Quaternion AsQuat() => throw new NotSupportedException();

    [ExtensionMarker("<M>$ACAC8CACEECE00D8841643D3E36C0E59")]
    public Rot4 AsRot4Force() => throw new NotSupportedException();

    [ExtensionMarker("<M>$ACAC8CACEECE00D8841643D3E36C0E59")]
    public Rot8 Rotated(Rot8 other) => throw new NotSupportedException();

    [SpecialName]
    public static class \u003CM\u003E\u0024ACAC8CACEECE00D8841643D3E36C0E59
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(Rot8 rot)
      {
      }
    }
  }
}
