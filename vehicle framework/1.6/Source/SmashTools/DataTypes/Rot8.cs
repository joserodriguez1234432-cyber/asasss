// Decompiled with JetBrains decompiler
// Type: SmashTools.Rot8
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools;

public record struct Rot8
{
  public const byte InvalidInt = 200;
  public const byte NorthInt = 0;
  public const byte EastInt = 1;
  public const byte SouthInt = 2;
  public const byte WestInt = 3;
  public const byte NorthEastInt = 4;
  public const byte SouthEastInt = 5;
  public const byte SouthWestInt = 6;
  public const byte NorthWestInt = 7;
  private byte rotInt;

  public Rot8(byte newRot) => this.rotInt = newRot;

  public Rot8(int newRot) => this.rotInt = (byte) (newRot % 8);

  public Rot8(Rot4 rot) => this.rotInt = ((Rot4) ref rot).AsByte;

  public Rot8(Rot4 rot, float angle)
  {
    int num = ((Rot4) ref rot).AsInt;
    if (((Rot4) ref rot).IsHorizontal)
    {
      switch (num)
      {
        case 1:
          if ((double) angle == -45.0)
          {
            num = 4;
            break;
          }
          if ((double) angle == 45.0)
          {
            num = 5;
            break;
          }
          break;
        case 3:
          if ((double) angle == -45.0)
          {
            num = 6;
            break;
          }
          if ((double) angle == 45.0)
          {
            num = 7;
            break;
          }
          break;
      }
    }
    this.rotInt = (byte) (num % 8);
  }

  public readonly bool IsVertical => this.rotInt == (byte) 2 || this.rotInt == (byte) 4;

  public readonly bool IsHorizontal => this.rotInt == (byte) 1 || this.rotInt == (byte) 3;

  public readonly bool IsDiagonal
  {
    get
    {
      return this.rotInt == (byte) 4 || this.rotInt == (byte) 5 || this.rotInt == (byte) 6 || this.rotInt == (byte) 7;
    }
  }

  public static Rot8 North => new Rot8(0);

  public static Rot8 East => new Rot8(1);

  public static Rot8 South => new Rot8(2);

  public static Rot8 West => new Rot8(3);

  public static Rot8 NorthEast => new Rot8(4);

  public static Rot8 SouthEast => new Rot8(5);

  public static Rot8 SouthWest => new Rot8(6);

  public static Rot8 NorthWest => new Rot8(7);

  public static Rot8 Random => new Rot8(Rand.RangeInclusive(0, 7));

  public bool IsValid => this.AsInt >= 0 && this.AsInt <= 7;

  public static Rot8 Invalid => new Rot8() { rotInt = 200 };

  public IntVec3 FacingCell
  {
    get
    {
      IntVec3 facingCell;
      switch (this.AsInt)
      {
        case 0:
          facingCell = new IntVec3(0, 0, 1);
          break;
        case 1:
          facingCell = new IntVec3(1, 0, 0);
          break;
        case 2:
          facingCell = new IntVec3(0, 0, -1);
          break;
        case 3:
          facingCell = new IntVec3(-1, 0, 0);
          break;
        case 4:
          facingCell = new IntVec3(1, 0, 1);
          break;
        case 5:
          facingCell = new IntVec3(1, 0, -1);
          break;
        case 6:
          facingCell = new IntVec3(-1, 0, -1);
          break;
        case 7:
          facingCell = new IntVec3(-1, 0, 1);
          break;
        default:
          facingCell = new IntVec3();
          break;
      }
      return facingCell;
    }
  }

  public Rot8 Opposite
  {
    get
    {
      Rot8 opposite;
      switch (this.AsInt)
      {
        case 0:
          opposite = new Rot8(2);
          break;
        case 1:
          opposite = new Rot8(3);
          break;
        case 2:
          opposite = new Rot8(0);
          break;
        case 3:
          opposite = new Rot8(1);
          break;
        case 4:
          opposite = new Rot8(6);
          break;
        case 5:
          opposite = new Rot8(7);
          break;
        case 6:
          opposite = new Rot8(4);
          break;
        case 7:
          opposite = new Rot8(5);
          break;
        default:
          opposite = new Rot8();
          break;
      }
      return opposite;
    }
  }

  public byte AsByte
  {
    get => this.rotInt;
    set => this.rotInt = (byte) ((uint) value % 8U);
  }

  public int AsInt
  {
    get => (int) this.rotInt;
    set
    {
      if (value < 0)
        value += 4000;
      this.rotInt = (byte) (value % 8);
    }
  }

  public float AsAngle
  {
    get
    {
      int asAngle;
      switch (this.AsInt)
      {
        case 0:
          asAngle = 0;
          break;
        case 1:
          asAngle = 90;
          break;
        case 2:
          asAngle = 180;
          break;
        case 3:
          asAngle = 270;
          break;
        case 4:
          asAngle = 45;
          break;
        case 5:
          asAngle = 135;
          break;
        case 6:
          asAngle = 225;
          break;
        case 7:
          asAngle = 315;
          break;
        default:
          throw new Exception($"value cannot be > 7 but it is = {this.rotInt}");
      }
      return (float) asAngle;
    }
  }

  public float AsRotationAngle
  {
    get
    {
      int asRotationAngle;
      switch (this.AsInt)
      {
        case 4:
          asRotationAngle = -45;
          break;
        case 5:
          asRotationAngle = 45;
          break;
        case 6:
          asRotationAngle = -45;
          break;
        case 7:
          asRotationAngle = 45;
          break;
        default:
          asRotationAngle = 0;
          break;
      }
      return (float) asRotationAngle;
    }
  }

  public int AsIntClockwise
  {
    get
    {
      switch (this.AsInt)
      {
        case 0:
          return 0;
        case 1:
          return 2;
        case 2:
          return 4;
        case 3:
          return 6;
        case 4:
          return 1;
        case 5:
          return 3;
        case 6:
          return 5;
        case 7:
          return 7;
        default:
          throw new Exception($"value cannot be > 7 but it is = {this.rotInt}");
      }
    }
  }

  public SpectateRectSide AsSpectateSide
  {
    get
    {
      SpectateRectSide asSpectateSide;
      switch (this.AsInt)
      {
        case 0:
          asSpectateSide = (SpectateRectSide) 1;
          break;
        case 1:
          asSpectateSide = (SpectateRectSide) 2;
          break;
        case 2:
          asSpectateSide = (SpectateRectSide) 4;
          break;
        case 3:
          asSpectateSide = (SpectateRectSide) 8;
          break;
        default:
          asSpectateSide = (SpectateRectSide) 0;
          break;
      }
      return asSpectateSide;
    }
  }

  public Quaternion AsQuat
  {
    get
    {
      switch (this.rotInt)
      {
        case 0:
          return Quaternion.identity;
        case 1:
          return Quaternion.LookRotation(Vector3.right);
        case 2:
          return Quaternion.LookRotation(Vector3.back);
        case 3:
          return Quaternion.LookRotation(Vector3.left);
        default:
          Log.Error("ToQuat with Rot = " + this.AsInt.ToString());
          return Quaternion.identity;
      }
    }
  }

  public Vector2 AsVector2
  {
    get
    {
      switch (this.rotInt)
      {
        case 0:
          return Vector2.up;
        case 1:
          return Vector2.right;
        case 2:
          return Vector2.down;
        case 3:
          return Vector2.left;
        case 4:
          return new Vector2(1f, 1f);
        case 5:
          return new Vector2(1f, -1f);
        case 6:
          return new Vector2(-1f, -1f);
        case 7:
          return new Vector2(-1f, 1f);
        default:
          throw new Exception("rotInt's value cannot be > 7 but it is:" + this.rotInt.ToString());
      }
    }
  }

  public static Rot8 At(int i) => new Rot8(i);

  public static Rot8 FromDirection(IntVec2 dir)
  {
    if (IntVec2.op_Equality(dir, new IntVec2(0, 1)))
      return Rot8.North;
    if (IntVec2.op_Equality(dir, new IntVec2(1, 1)))
      return Rot8.NorthEast;
    if (IntVec2.op_Equality(dir, new IntVec2(1, 0)))
      return Rot8.East;
    if (IntVec2.op_Equality(dir, new IntVec2(1, -1)))
      return Rot8.SouthEast;
    if (IntVec2.op_Equality(dir, new IntVec2(0, -1)))
      return Rot8.South;
    if (IntVec2.op_Equality(dir, new IntVec2(-1, -1)))
      return Rot8.SouthWest;
    if (IntVec2.op_Equality(dir, new IntVec2(-1, 0)))
      return Rot8.West;
    return IntVec2.op_Equality(dir, new IntVec2(-1, 1)) ? Rot8.NorthWest : Rot8.Invalid;
  }

  public static Rot8 FromAngle(float angle)
  {
    if ((double) angle > 22.5 && (double) angle <= 67.5)
      return Rot8.NorthEast;
    if ((double) angle > 67.5 && (double) angle <= 112.5)
      return Rot8.East;
    if ((double) angle > 112.5 && (double) angle <= 157.5)
      return Rot8.SouthEast;
    if ((double) angle > 157.5 && (double) angle <= 202.5)
      return Rot8.South;
    if ((double) angle > 202.5 && (double) angle <= 247.5)
      return Rot8.SouthWest;
    if ((double) angle > 247.5 && (double) angle <= 292.5)
      return Rot8.West;
    return (double) angle > 292.5 && (double) angle <= 337.5 ? Rot8.NorthWest : Rot8.North;
  }

  public static int FromIntClockwise(int asInt)
  {
    switch (asInt)
    {
      case 0:
        return 0;
      case 1:
        return 4;
      case 2:
        return 1;
      case 3:
        return 5;
      case 4:
        return 2;
      case 5:
        return 6;
      case 6:
        return 3;
      case 7:
        return 7;
      default:
        throw new Exception($"value cannot be > 7 but it is = {asInt}");
    }
  }

  public int Difference(Rot8 rot)
  {
    if (!rot.IsValid || !this.IsValid || rot == this)
      return 0;
    int num = 0;
    Rot8 rot8_1 = this;
    Rot8 rot8_2 = this;
    for (int index = 0; index < 4; ++index)
    {
      ++num;
      rot8_1 = rot8_1.Rotated((RotationDirection) 1);
      rot8_2 = rot8_2.Rotated((RotationDirection) 3);
      if (rot8_1 == rot || rot8_2 == rot)
        return num;
    }
    Log.Error($"Could not match rot {rot} with {this}.");
    return 4;
  }

  public void Rotate(RotationDirection rotDir, bool diagonals = true)
  {
    if (this.AsInt < 0 || this.AsInt > 7)
      return;
    if (!diagonals)
    {
      Rot4 rot4_1 = (Rot4) this;
      Rot4 rot4_2 = ((Rot4) ref rot4_1).Rotated(rotDir);
      this.AsByte = ((Rot4) ref rot4_2).AsByte;
    }
    else
    {
      int asIntClockwise = this.AsIntClockwise;
      if (rotDir == 1)
        ++asIntClockwise;
      if (rotDir == 3)
        --asIntClockwise;
      if (rotDir == 2)
        asIntClockwise += 4;
      this.AsInt = Rot8.FromIntClockwise(GenMath.PositiveMod(asIntClockwise, 8));
    }
  }

  public Rot8 Rotated(RotationDirection rotDir, bool diagonals = true)
  {
    Rot8 rot8 = this;
    rot8.Rotate(rotDir, diagonals);
    return rot8;
  }

  public Rot8 Rotated(float angle, RotationDirection rotDir)
  {
    if ((double) angle % 45.0 != 0.0)
    {
      SmashLog.Error("Cannot rotate <type>Rot8</type> with angle non-multiple of 45.");
      return this;
    }
    int num = Mathf.RoundToInt(angle / 45f);
    Rot8 rot8 = this;
    for (int index = 0; index < num; ++index)
      rot8.Rotate(rotDir);
    return rot8;
  }

  public static Rot8 DirectionFromCells(IntVec3 from, IntVec3 to)
  {
    IntVec3 intVec3 = IntVec3.op_Subtraction(to, from);
    return Rot8.FromDirection(new IntVec2(intVec3.x, intVec3.z));
  }

  public static Rot4 ToRot4(Rot8 rot)
  {
    switch (rot.AsInt)
    {
      case 0:
        return Rot4.North;
      case 1:
        return Rot4.East;
      case 2:
        return Rot4.South;
      case 3:
        return Rot4.West;
      case 4:
        return Rot4.East;
      case 5:
        return Rot4.East;
      case 6:
        return Rot4.West;
      case 7:
        return Rot4.West;
      case 200:
        return Rot4.Invalid;
      default:
        throw new Exception("Rot8 must be between (0, 7) int value");
    }
  }

  public static implicit operator Rot4(Rot8 rot) => Rot8.ToRot4(rot);

  public static implicit operator Rot8(Rot4 rot) => new Rot8(((Rot4) ref rot).AsInt);

  public override int GetHashCode() => (int) this.rotInt;

  public override string ToString() => this.rotInt.ToString();

  public string ToStringNamed()
  {
    string stringNamed;
    switch (this.rotInt)
    {
      case 0:
        stringNamed = "North";
        break;
      case 1:
        stringNamed = "East";
        break;
      case 2:
        stringNamed = "South";
        break;
      case 3:
        stringNamed = "West";
        break;
      case 4:
        stringNamed = "NorthEast";
        break;
      case 5:
        stringNamed = "SouthEast";
        break;
      case 6:
        stringNamed = "SouthWest";
        break;
      case 7:
        stringNamed = "NorthWest";
        break;
      default:
        stringNamed = "Invalid";
        break;
    }
    return stringNamed;
  }

  public static Rot8 FromString(string innerText)
  {
    int result;
    if (int.TryParse(innerText, out result))
      return new Rot8(result);
    string upperInvariant = innerText.ToUpperInvariant();
    if (upperInvariant != null)
    {
      switch (upperInvariant.Length)
      {
        case 4:
          switch (upperInvariant[0])
          {
            case 'E':
              if (upperInvariant == "EAST")
                return Rot8.East;
              break;
            case 'W':
              if (upperInvariant == "WEST")
                return Rot8.West;
              break;
          }
          break;
        case 5:
          switch (upperInvariant[0])
          {
            case 'N':
              if (upperInvariant == "NORTH")
                return Rot8.North;
              break;
            case 'S':
              if (upperInvariant == "SOUTH")
                return Rot8.South;
              break;
          }
          break;
        case 9:
          switch (upperInvariant[0])
          {
            case 'N':
              switch (upperInvariant)
              {
                case "NORTHEAST":
                  return Rot8.NorthEast;
                case "NORTHWEST":
                  return Rot8.NorthWest;
              }
              break;
            case 'S':
              switch (upperInvariant)
              {
                case "SOUTHEAST":
                  return Rot8.SouthEast;
                case "SOUTHWEST":
                  return Rot8.SouthWest;
              }
              break;
          }
          break;
      }
    }
    Log.Error("Unable to parse Rot8: " + innerText);
    return Rot8.Invalid;
  }

  [CompilerGenerated]
  public readonly bool Equals(Rot8 other)
  {
    return EqualityComparer<byte>.Default.Equals(this.rotInt, other.rotInt);
  }
}
