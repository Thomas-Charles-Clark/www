//using Braintree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;

/*
 * Thanks to Konrad Kokosa
 * http://stackoverflow.com/questions/20554103/recursively-get-properties-child-properties-of-a-class
 */

namespace wwwDesignersGuild.Util
{
    public static class PropertyPrinter
    {
        public static void PrintProperties(object obj, int indent, StringBuilder sb)
        {
            string indentString = new string(' ', indent);

            if (obj == null)
            {
                sb.AppendLine(String.Format("{0}{1}", indentString, "[NULL]"));
                return;
            }

            Type objType = obj.GetType();
            PropertyInfo[] properties = objType.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var indexParameters = property.GetIndexParameters();
                if (indexParameters.Length == 0)
                {
                object propValue = property.GetValue(obj, null);

                var elems = propValue as IList;
                if (elems != null)
                {
                    foreach (var item in elems)
                    {
                        PrintProperties(item, indent + 3, sb);
                    }
                }
                else
                {
                    var dict = propValue as IDictionary<String, String>;
                    if (dict != null)
                    {
                        foreach (var kvp in dict)
                        {
                            sb.AppendLine(String.Format("{0}{1}: {2}", indentString, kvp.Key, kvp.Value));
                        }
                    }
                    else
                    {
                        // This will not cut-off System.Collections because of the first check
                        if (property.PropertyType.Assembly == objType.Assembly)
                        {
                            sb.AppendLine(String.Format("{0}{1}:", indentString, property.Name));

                            PrintProperties(propValue, indent + 2, sb);
                        }
                        else
                        {
                            sb.AppendLine(String.Format("{0}{1}: {2}", indentString, property.Name, propValue));
                        }
                    }
                }
            }
        }
        }

        public static void PrintProperties(object obj, int indent, XmlWriter sb)
        {
            _ = new string(' ', indent);

            if (obj == null)
            {
                sb.WriteElementString("NULL", "NULL");
                return;
            }

            Type objType = obj.GetType();
            PropertyInfo[] properties = objType.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object propValue = property.GetValue(obj, null);

                var elems = propValue as IList;
                if (elems != null)
                {
                    foreach (var item in elems)
                    {
                        PrintProperties(item, indent + 3, sb);
                    }
                }
                else
                {
                    var dict = propValue as IDictionary<String, String>;
                    if (dict != null)
                    {
                        foreach (var kvp in dict)
                        {
                            sb.WriteElementString(kvp.Key.Replace("[", "-").Replace("]", ""), kvp.Value);
                        }
                    }
                    else
                    {
                        // This will not cut-off System.Collections because of the first check
                        if (property.PropertyType.Assembly == objType.Assembly)
                        {
                            sb.WriteStartElement(property.Name.Replace("[", "-").Replace("]", ""));

                            PrintProperties(propValue, indent + 2, sb);

                            sb.WriteEndElement();
                        }
                        else
                        {
                            sb.WriteElementString(property.Name.Replace("[", "-").Replace("]", ""), String.Format("{0}", propValue));
                        }
                    }
                }
            }
        }
    }
}