// Decompiled with JetBrains decompiler
// Type: Vehicles.UpgradeTreeDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class UpgradeTreeDef : Def
{
  public List<UpgradeNode> nodes;
  private Dictionary<string, UpgradeNode> lookup = new Dictionary<string, UpgradeNode>();

  public virtual IEnumerable<string> ConfigErrors()
  {
    IEnumerator<string> enumerator1 = base.ConfigErrors().GetEnumerator();
    while (enumerator1.MoveNext())
      yield return enumerator1.Current;
    enumerator1 = (IEnumerator<string>) null;
    if (!GenList.NullOrEmpty<UpgradeNode>((IList<UpgradeNode>) this.nodes))
    {
      foreach (UpgradeNode node in this.nodes)
      {
        if (!GenList.NullOrEmpty<string>((IList<string>) node.prerequisiteNodes))
        {
          List<string>.Enumerator enumerator2 = node.prerequisiteNodes.GetEnumerator();
          while (enumerator2.MoveNext())
          {
            string current = enumerator2.Current;
            if (!this.lookup.TryGetValue(current, out UpgradeNode _))
              yield return $"Unable to find key {current} in prerequisiteNodes for {node}";
          }
          enumerator2 = new List<string>.Enumerator();
        }
        if (!GenList.NullOrEmpty<Upgrade>((IList<Upgrade>) node.upgrades))
        {
          foreach (Upgrade upgrade in node.upgrades)
          {
            enumerator1 = upgrade.ConfigErrors.GetEnumerator();
            while (enumerator1.MoveNext())
            {
              string current = enumerator1.Current;
              yield return $"(UpgradeNode={node.key} Type={upgrade.GetType()}) {current}";
            }
            enumerator1 = (IEnumerator<string>) null;
          }
        }
      }
    }
  }

  public virtual void ResolveReferences()
  {
    base.ResolveReferences();
    if (GenList.NullOrEmpty<UpgradeNode>((IList<UpgradeNode>) this.nodes))
      return;
    foreach (UpgradeNode node in this.nodes)
    {
      node.ResolveReferences();
      if (!this.lookup.ContainsKey(node.key))
        this.lookup[node.key] = node;
      else
        Log.Error($"Duplicate keys in upgrade tree {this.defName}.");
    }
  }

  public virtual void PostLoad()
  {
    ((Editable) this).PostLoad();
    if (GenList.NullOrEmpty<UpgradeNode>((IList<UpgradeNode>) this.nodes))
      return;
    foreach (UpgradeNode node in this.nodes)
      node.PostLoad();
  }

  public UpgradeNode GetNode(string key)
  {
    return GenText.NullOrEmpty(key) ? (UpgradeNode) null : GenCollection.TryGetValue<string, UpgradeNode>((IReadOnlyDictionary<string, UpgradeNode>) this.lookup, key, (UpgradeNode) null);
  }
}
