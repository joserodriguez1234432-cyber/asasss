// Decompiled with JetBrains decompiler
// Type: SmashTools.Patching.IConditionalPatch
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using Verse;

#nullable disable
namespace SmashTools.Patching;

[PublicAPI]
public interface IConditionalPatch
{
  string SourceId { get; }

  string PackageId { get; }

  PatchSequence PatchAt { get; }

  void PatchAll(ModMetaData mod);

  [PublicAPI]
  class Result
  {
    public string PackageId { get; set; }

    public string FriendlyName { get; set; }

    public bool Active { get; set; }

    public Exception ExceptionThrown { get; set; }
  }
}
