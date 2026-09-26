// Decompiled with JetBrains decompiler
// Type: SmashTools.Xml.XmlExporter
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

#nullable disable
namespace SmashTools.Xml;

[PublicAPI]
public static class XmlExporter
{
  private static XmlWriter writer;
  private static FileStream stream;
  private static string filePath;
  private const string ListItemNodeName = "li";

  public static void StartDocument(string filePath)
  {
    XmlExporter.filePath = filePath;
    XmlExporter.stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
    XmlExporter.writer = XmlWriter.Create((Stream) XmlExporter.stream, new XmlWriterSettings()
    {
      Indent = true,
      IndentChars = "\t",
      NewLineChars = "\n",
      Encoding = Encoding.UTF8
    });
    XmlExporter.writer.WriteStartDocument();
  }

  public static void Export() => Application.OpenURL(XmlExporter.filePath);

  public static void Close()
  {
    XmlExporter.writer.WriteEndDocument();
    XmlExporter.writer.Flush();
    XmlExporter.writer.Close();
    XmlExporter.writer.Dispose();
    XmlExporter.stream.Close();
    XmlExporter.stream.Dispose();
    XmlExporter.stream = (FileStream) null;
    XmlExporter.writer = (XmlWriter) null;
    XmlExporter.filePath = string.Empty;
  }

  public static void OpenNode(string name, params (string name, string value)[] attributes)
  {
    try
    {
      XmlExporter.writer.WriteStartElement(name);
      if (((IEnumerable<(string, string)>) attributes).NullOrEmpty<(string, string)>())
        return;
      foreach ((string name, string value) attribute in attributes)
      {
        if (!attribute.name.NullOrEmpty<char>())
          XmlExporter.writer.WriteAttributeString(attribute.name, attribute.value);
      }
    }
    catch
    {
      XmlExporter.Close();
      throw;
    }
  }

  public static void CloseNode(bool emptyText = false)
  {
    try
    {
      if (emptyText)
        XmlExporter.writer.WriteFullEndElement();
      else
        XmlExporter.writer.WriteEndElement();
    }
    catch
    {
      XmlExporter.Close();
      throw;
    }
  }

  public static void WriteObject<T>(string localName, T value)
  {
    try
    {
      if ((object) value == null)
      {
        XmlExporter.OpenNode(localName, ("IsNull", "TRUE"));
        XmlExporter.CloseNode();
      }
      else
        XmlExporter.writer.WriteElementString(localName, value.ToString());
    }
    catch
    {
      XmlExporter.Close();
      throw;
    }
  }

  public static void WriteElement(string localName, string value)
  {
    try
    {
      XmlExporter.writer.WriteElementString(localName, value);
    }
    catch
    {
      XmlExporter.Close();
      throw;
    }
  }

  public static void WriteNullElement(string localName)
  {
    try
    {
      XmlExporter.OpenNode(localName, ("IsNull", "TRUE"));
      XmlExporter.CloseNode();
    }
    catch
    {
      XmlExporter.Close();
      throw;
    }
  }

  public static void WriteElement(string localName, IXmlExport value)
  {
    try
    {
      if (value == null)
      {
        XmlExporter.WriteNullElement(localName);
      }
      else
      {
        XmlExporter.OpenNode(localName);
        value.Export();
        XmlExporter.CloseNode();
      }
    }
    catch
    {
      XmlExporter.Close();
      throw;
    }
  }

  public static void WriteString(string text)
  {
    try
    {
      XmlExporter.writer.WriteString(text);
    }
    catch
    {
      XmlExporter.Close();
      throw;
    }
  }

  public static void WriteCollection<T>(
    string localName,
    IEnumerable<T> value,
    Func<T, (string name, string value)> attributeGetter = null)
    where T : IXmlExport
  {
    try
    {
      if (value == null)
      {
        XmlExporter.WriteNullElement(localName);
      }
      else
      {
        XmlExporter.OpenNode(localName);
        foreach (T obj in value)
        {
          (string, string)[] valueTupleArray;
          if (attributeGetter == null)
            valueTupleArray = ((string, string)[]) null;
          else
            valueTupleArray = new (string, string)[1]
            {
              attributeGetter(obj)
            };
          XmlExporter.OpenNode("li", valueTupleArray);
          obj.Export();
          XmlExporter.CloseNode();
        }
        XmlExporter.CloseNode();
      }
    }
    catch
    {
      XmlExporter.Close();
      throw;
    }
  }

  public static void WriteList<T>(string localName, IList<T> value, Action<T> itemWriter)
  {
    try
    {
      if (value == null)
      {
        XmlExporter.WriteNullElement(localName);
      }
      else
      {
        XmlExporter.OpenNode(localName);
        foreach (T obj in (IEnumerable<T>) value)
        {
          XmlExporter.OpenNode("li");
          itemWriter(obj);
          XmlExporter.CloseNode();
        }
        XmlExporter.CloseNode();
      }
    }
    catch
    {
      XmlExporter.Close();
      throw;
    }
  }

  public static void WriteList<T>(
    string localName,
    IList<T> value,
    Action<T> itemWriter,
    Func<T, string> listItemNameGetter)
  {
    try
    {
      if (value == null)
      {
        XmlExporter.WriteNullElement(localName);
      }
      else
      {
        XmlExporter.OpenNode(localName);
        foreach (T obj in (IEnumerable<T>) value)
        {
          XmlExporter.OpenNode(listItemNameGetter(obj));
          itemWriter(obj);
          XmlExporter.CloseNode();
        }
        XmlExporter.CloseNode();
      }
    }
    catch
    {
      XmlExporter.Close();
      throw;
    }
  }
}
