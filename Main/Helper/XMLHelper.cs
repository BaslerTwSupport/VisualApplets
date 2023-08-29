using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Main.Helper
{
    public static class XmlHelper
    {
        /// <summary>
        /// Convert generic type which had IFormattable to string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="format"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public static string Convert2Str<T>(T obj, string format = "", IFormatProvider formatProvider = null)
        {
            return obj is IFormattable formattable
                ? formattable.ToString(format, formatProvider)
                : Convert.ToString(obj, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Geta an attribute from an xml element, created one if it does not exist.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="el"></param>
        /// <param name="attName"></param>
        /// <returns></returns>
        public static XmlAttribute GetOrCreateAttribute(XmlDocument doc, XmlElement el, string attName)
        {
            if (el == null)
                throw new ArgumentNullException(nameof(el));

            if (doc == null)
                throw new ArgumentNullException(nameof(doc));

            if (string.IsNullOrWhiteSpace(attName))
                throw new ArgumentNullException(nameof(attName));

            if (el.Attributes[attName] != null)
            {
                return el.Attributes[attName];
            }

            XmlAttribute att = doc.CreateAttribute(attName);
            el.Attributes.Append(att);
            return att;
        }

        public static Dictionary<string, string> ReadNodes(XmlElement parentNode, IList<string> children)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string childName in children)
            {
                var ndChild = parentNode.SelectSingleNode(childName) as XmlElement;
                string content = ndChild?.InnerText;
                result.Add(childName, content);
            }

            return result;
        }

        /// <summary>
        /// Read a value from an attribute in node. Take a default value if the attribute does not contain the corresponding informations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ndParent">The Xml Node to read from</param>
        /// <param name="attName">The name of the property to read</param>
        /// <param name="defaultValue">The Default value that is used if the reading fails.</param>
        /// <returns></returns>
        public static T ReadValueFromAttribute<T>(XmlElement ndParent, string attName, T defaultValue = default(T))
        {
            if (ndParent == null)
            {
                if (defaultValue == null)
                    throw new NullReferenceException("ReadValue: Node is null!");
                return defaultValue;
            }

            if (!ndParent.HasAttribute(attName))
            {
                if (defaultValue == null)
                    throw new NullReferenceException($"Attribute {attName} does not exist!");
                return defaultValue;
            }

            XmlAttribute att = null;
            try
            {
                att = ndParent.Attributes[attName];

                if (string.IsNullOrEmpty(att?.Value))
                {
                    if (defaultValue == null)
                    {
                        throw new Exception($"Attribute {attName} value is empty!");
                    }

                    return defaultValue;
                }

                // <ibg 2022-08-09/> I'm seing unit test errors from here on a european local. Problem is
                // that the invariant culture is not used. Changing this to ConvertFromInvariantString
                // T val = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(att.Value);
                // new code:
                T val = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(att.Value);
                // </ibg>

                return val;
            }
            catch (Exception exc)
            {
                if (defaultValue == null)
                {
                    throw new Exception(
                        $"The XML Attribute {attName} could not be converted into type {typeof(T)}! (Attribute value is {att?.Value})",
                        exc);
                }

                return defaultValue;
            }
        }

        /// <summary>
        /// Get value from attribute or node.
        /// Old format was build with nodes. New format going to build with attribute.     
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ndParent"></param>
        /// <param name="name"></param>
        /// <param name="def"></param>
        /// <returns>inner text or attribute value</returns>
        public static T ReadValueFromAttributeOrNode<T>(XmlElement ndParent, string name, T def = default(T))
        {
            try
            {
                if (ndParent == null)
                    throw new NullReferenceException("ReadValue: Node is null!");
                //(T)TypeDescriptor.GetConverter(typeof(T)).ConvertTo(ndParent.Attributes[name].Value,)
                if (ndParent.HasAttribute(name))
                {
                    return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(ndParent.Attributes[name].Value);
                }

                var nd = ndParent[name];
                if (nd != null)
                {
                    return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(nd.InnerText);
                }

                return def;
            }
            catch
            {
                if (def == null)
                    throw;
                return def;
            }
        }

        /// <summary>
        /// Read a value from an xml node. Take a default value if the node does not contain the corresponding informations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ndParent">The Xml Node to read from</param>
        /// <param name="nodename">The name of the property to read</param>
        /// <param name="def">The Default value that is used if the reading fails.</param>
        /// <returns></returns>
        public static T ReadValueFromNode<T>(XmlElement ndParent, string nodename, T def)
        {
            if (ndParent == null)
            {
                //throw new NullReferenceException("ReadValue: Node is null!");
                return def;
            }

            XmlElement nd = ndParent.SelectSingleNode(nodename) as XmlElement;
            if (nd == null)
            {
                return def;
            }
            else
            {
                T val = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(nd.InnerText);
                return val;
            }
        }

        /// <summary>
        /// Removes a node from an xml structure passed as a string.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="node"></param>
        public static string RemoveNodeFromString(string xml, string node)
        {
            try
            {
                if (string.IsNullOrEmpty(xml))
                    return xml;

                if (string.IsNullOrEmpty(node))
                    return xml;

                int s = xml.IndexOf($"<{node}>", StringComparison.InvariantCulture);
                if (s <= 0)
                    return xml;

                int len = $"</{node}>".Length;
                int e = xml.IndexOf($"</{node}>", StringComparison.InvariantCulture) + len;
                if (e <= 0)
                    return xml;

                return xml.Remove(s, e - s);
            }
            catch
            {
                return xml;
            }
        }

        /// <summary>
        /// Replace or append a node as a child of a given parent. If the node already exists it will be
        /// replaced otherwise the node will be appended 
        /// </summary>
        /// <param name="ndParent"></param>
        /// <param name="ndNew"></param>
        public static void ReplaceOrCreate(XmlElement ndParent, XmlNode ndNew)
        {
            Debug.Assert(ndParent != null);

            if (ndNew == null)
            {
                return;
            }

            XmlElement ndOld = ndParent[ndNew.Name];
            if (ndOld == null)
            {
                ndParent.AppendChild(ndNew);
            }
            else
            {
                ndParent.ReplaceChild(ndNew, ndOld);
            }
        }

        /// <summary>
        /// Adds a new child node named "nodeName" to ndItem if there is not already such a node.
        /// If the node already exists its content is overwritten. Will also set the innerText property.
        /// </summary>
        /// <param name="doc">The xml document (needed for creating a ne node)</param>
        /// <param name="ndParentItem">The parent item</param>
        /// <param name="nodeName">The name of the node to look for/create</param>
        /// <param name="obj">The text to use as the generic of the node</param>
        /// <param name="format">Formats the value of current instance, like string format</param>
        public static void ReplaceOrCreate<T>(
            XmlDocument doc,
            XmlNode ndParentItem,
            string nodeName,
            T obj,
            string format = "",
            bool isCurCulture = false)
        {
            if (ndParentItem == null)
                throw new ArgumentNullException(nameof(ndParentItem));

            if (doc.DocumentElement == null)
                throw new InvalidOperationException("SelectOrCreate: Document element is null!");
            XmlNode ndP = ndParentItem.SelectSingleNode(nodeName) ?? doc.CreateElement(nodeName);
            IFormatProvider iformat = isCurCulture ? null : CultureInfo.CurrentCulture;
            ndP.InnerText = Convert2Str(obj, format, iformat);
            ndParentItem.AppendChild(ndP);
        }

        /// <summary>
        /// Adds a new Attribute named "attributeName" to ndItem if there is not already such a attribute.
        /// If the attribute already exists its content is overwritten. Will also set the value property.
        /// </summary>
        /// <param name="doc">The xml document (needed for creating a ne node)</param>
        /// <param name="ndParentItem">The parent item</param>
        /// <param name="attributeName">The name of the node to look for/create</param>
        /// <param name="value">The value to use as the inner text of the attribute</param>
        public static void ReplaceOrCreateAttribute<T>(
            XmlDocument doc,
            XmlNode ndParentItem,
            string attributeName,
            T obj,
            string format = "",
            bool isCurCulture = false)
        {
            if (ndParentItem == null)
                throw new ArgumentNullException(nameof(ndParentItem));

            if (doc.DocumentElement == null)
                throw new InvalidOperationException("SelectOrCreate: Document element is null!");
            IFormatProvider iformat = isCurCulture ? CultureInfo.CurrentCulture : CultureInfo.InvariantCulture;
            var text = Convert2Str(obj, format, iformat);
            if (ndParentItem.SelectSingleNode($"@{attributeName}") == null || ndParentItem.Attributes == null
                || ndParentItem.Attributes[attributeName] == null)
            {
                XmlAttribute att = doc.CreateAttribute(attributeName);
                att.Value = text;
                ndParentItem.Attributes.Append(att);
            }
            else
                ndParentItem.Attributes[attributeName].Value = text;
        }

        public static void RetrieveAttributes(XmlElement node, Dictionary<string, Action<string>> dict)
        {
            foreach (string attName in dict.Keys)
            {
                var att = node.Attributes[attName];
                if (att != null)
                {
                    try
                    {
                        dict[attName].Invoke(att.InnerText);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception($"Attribute {attName} could not be read!", exc);
                    }
                }
            }
        }

        public static void RetrieveChildArray(XmlElement parentNode, string childrenName, Action<string> ac)
        {
            XmlNodeList ndChildren = parentNode?.SelectNodes(childrenName);
            if (ndChildren != null)
            {
                foreach (XmlElement ndChild in ndChildren)
                {
                    string strNode = ndChild.InnerText;
                    ac.Invoke(strNode);
                }
            }
        }

        public static void RetrieveChilds(XmlElement parentNode, Dictionary<string, Action<string>> dict)
        {
            foreach (string childName in dict.Keys)
            {
                if (parentNode.SelectSingleNode(childName) is XmlElement ndChild)
                {
                    dict[childName].Invoke(ndChild.InnerText);
                }
            }
        }

        public static XmlElement SelectOrCreate(XmlDocument doc, string path, string nodeName)
        {
            if (doc == null)
                throw new ArgumentNullException(nameof(doc));

            if (doc.DocumentElement == null)
                throw new InvalidOperationException("SelectOrCreate: Document element is null!");

            XmlNode ndParent = doc.DocumentElement.SelectSingleNode(path);
            if (ndParent == null)
            {
                return null;
            }

            XmlNode nd = ndParent[nodeName];
            if (nd == null)
            {
                nd = doc.CreateElement(nodeName);
                ndParent.AppendChild(nd);
            }

            return (XmlElement)nd;
        }

        /// <summary>
        /// Read value from attribute in node in XML file. A return value indicated whether the reading succeeded or failed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ndParent"></param>
        /// <param name="nodename"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool TryReadValueFromAttribute<T>(XmlElement ndParent, string attName, out T val)
        {
            bool result = true;
            val = default(T);
            try
            {
                if (ndParent == null)
                    return false;

                if (!ndParent.HasAttribute(attName))
                    return false;

                var att = ndParent.Attributes[attName];

                if (string.IsNullOrEmpty(att.Value))
                    return false;

                val = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(att.Value);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Read value from attribute/node in node in XML file. A return value indicated whether the reading succeeded or failed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ndParent"></param>
        /// <param name="nodename"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool TryReadValueFromAttributeOrNode<T>(XmlElement ndParent, string name, out T val)
        {
            val = default(T);
            try
            {
                if (ndParent == null)
                    return false;

                if (ndParent.HasAttribute(name))
                {
                    val = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(ndParent.Attributes[name].Value);
                    return true;
                }

                var nd = ndParent[name];

                if (nd == null)
                    return false;

                if (string.IsNullOrEmpty(nd.InnerText))
                    return false;

                val = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(nd.InnerText);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read value from node in XML file. A return value indicated whether the reading succeeded or failed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ndParent"></param>
        /// <param name="nodename"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool TryReadValueFromNode<T>(XmlElement ndParent, string nodename, out T val)
        {
            val = default(T);
            try
            {
                if (ndParent == null)
                    return false;

                var nd = ndParent.SelectSingleNode(nodename) as XmlElement;
                if (nd == null)
                    return false;

                if (string.IsNullOrEmpty(nd.InnerText))
                    return false;

                val = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(nd.InnerText);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static string XmlDocumentToString(XmlDocument doc)
        {
            string project;
            using (var stringWriter = new StringWriter())
            {
                var settings = new XmlWriterSettings { Indent = true };
                using (var xmlTextWriter = XmlWriter.Create(stringWriter, settings))
                {
                    doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    project = stringWriter.GetStringBuilder().ToString();
                }
            }

            return project;
        }
    }
}
