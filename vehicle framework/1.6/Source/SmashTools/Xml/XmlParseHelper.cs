// Decompiled with JetBrains decompiler
// Type: SmashTools.Xml.XmlParseHelper
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using SmashTools.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Verse;

#nullable disable
namespace SmashTools.Xml;

[PublicAPI]
public static class XmlParseHelper
{
  private const string ValidAttributeRegex = "^([A-Za-z0-9]*$)";
  internal static readonly Dictionary<string, XmlParseHelper.CustomAttribute> RegisteredAttributes = new Dictionary<string, XmlParseHelper.CustomAttribute>();

  internal static void RegisterParseTypes()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    ParseHelper.Parsers<Rot8>.Register(XmlParseHelper.\u003C\u003EO.\u003C0\u003E__FromString ?? (XmlParseHelper.\u003C\u003EO.\u003C0\u003E__FromString = new Func<string, Rot8>(Rot8.FromString)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    ParseHelper.Parsers<Quadrant>.Register(XmlParseHelper.\u003C\u003EO.\u003C1\u003E__FromString ?? (XmlParseHelper.\u003C\u003EO.\u003C1\u003E__FromString = new Func<string, Quadrant>(Quadrant.FromString)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    ParseHelper.Parsers<RimWorldTime>.Register(XmlParseHelper.\u003C\u003EO.\u003C2\u003E__FromString ?? (XmlParseHelper.\u003C\u003EO.\u003C2\u003E__FromString = new Func<string, RimWorldTime>(RimWorldTime.FromString)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    ParseHelper.Parsers<KeyFrame>.Register(XmlParseHelper.\u003C\u003EO.\u003C3\u003E__ParseKeyFrame ?? (XmlParseHelper.\u003C\u003EO.\u003C3\u003E__ParseKeyFrame = new Func<string, KeyFrame>(XmlParseHelper.ParseKeyFrame)));
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    ParseHelper.Parsers<Guid>.Register(XmlParseHelper.\u003C\u003EO.\u003C4\u003E__ParseGuid ?? (XmlParseHelper.\u003C\u003EO.\u003C4\u003E__ParseGuid = new Func<string, Guid>(XmlParseHelper.ParseGuid)));
  }

  private static KeyFrame ParseKeyFrame(string entry) => KeyFrame.FromString(entry);

  private static Guid ParseGuid(string entry)
  {
    return entry.NullOrEmpty<char>() ? Guid.Empty : Guid.Parse(entry);
  }

  public static object WrapStringAndParse(Type type, string innerText, bool doPostLoad = true)
  {
    if (ParseHelper.HandlesType(type))
      return ParseHelper.FromString(innerText, type);
    if (type != typeof (string))
      innerText = innerText.Trim();
    XmlDocument xmlDocument = new XmlDocument();
    xmlDocument.LoadXml($"<temp>{innerText}</temp>");
    XmlNode documentElement = (XmlNode) xmlDocument.DocumentElement;
    return DirectXmlToObject.GetObjectFromXmlMethod(type)(documentElement, doPostLoad);
  }

  public static void RegisterAttribute(
    string attribute,
    XmlParseHelper.AttributeProcessor processor,
    params string[] nodeAllowed)
  {
    if (!Regex.IsMatch(attribute, "^([A-Za-z0-9]*$)", RegexOptions.CultureInvariant))
      Log.Error("Cannot register attribute due to invalid naming. Only alphanumeric characters may be used.");
    else
      XmlParseHelper.RegisteredAttributes.Add(attribute, new XmlParseHelper.CustomAttribute(attribute, nodeAllowed)
      {
        Processor = processor
      });
  }

  public static void RegisterPreProcessor(
    string attribute,
    XmlParseHelper.AttributePreProcessor preProcessor,
    params string[] nodeAllowed)
  {
    if (!Regex.IsMatch(attribute, "^([A-Za-z0-9]*$)", RegexOptions.CultureInvariant))
      Log.Error("Cannot register attribute due to invalid naming. Only alphanumeric characters may be used.");
    else
      XmlParseHelper.RegisteredAttributes.Add(attribute, new XmlParseHelper.CustomAttribute(attribute, nodeAllowed)
      {
        PreProcessor = preProcessor
      });
  }

  public delegate bool AttributePreProcessor(XmlNode node, string defName, FieldInfo fieldInfo = null);

  public delegate void AttributeProcessor(XmlNode node, string defName, FieldInfo fieldInfo = null);

  internal class CustomAttribute
  {
    private readonly string attribute;
    private readonly HashSet<string> nodeWhiteList;

    public CustomAttribute(string attribute, params string[] nodeAllowed)
    {
      this.attribute = attribute;
      if (((IEnumerable<string>) nodeAllowed).NullOrEmpty<string>())
        return;
      this.nodeWhiteList = ((IEnumerable<string>) nodeAllowed).ToHashSet<string>();
    }

    public XmlParseHelper.AttributePreProcessor PreProcessor { get; init; }

    public XmlParseHelper.AttributeProcessor Processor { get; init; }

    public bool PreProcess(XmlNode node, XmlAttribute attr, FieldInfo fieldInfo)
    {
      if (this.PreProcessor == null)
        return true;
      if (attr.Name.NullOrEmpty<char>())
      {
        Log.Error("Malformed xml attribute, missing name.");
        return true;
      }
      if (this.nodeWhiteList.NullOrEmpty<string>() || this.nodeWhiteList.Contains(node.Name))
        return this.PreProcessor(node, attr.Value, fieldInfo);
      Log.Error($"Unable to execute {this.attribute}. It is only allowed to be used on nodes=({string.Join(",", (IEnumerable<string>) this.nodeWhiteList)}) curNode={node.Name}");
      return true;
    }

    public void Process(XmlNode node, XmlAttribute attr, FieldInfo fieldInfo)
    {
      if (this.Processor == null)
        return;
      if (attr.Name.NullOrEmpty<char>())
        Log.Error("Malformed xml attribute, missing name.");
      else if (!this.nodeWhiteList.NullOrEmpty<string>() && !this.nodeWhiteList.Contains(node.Name))
        Log.Error($"Unable to execute {this.attribute}. It is only allowed to be used on nodes=({string.Join(",", (IEnumerable<string>) this.nodeWhiteList)}) curNode={node.Name}");
      else
        this.Processor(node, attr.Value, fieldInfo);
    }
  }
}
