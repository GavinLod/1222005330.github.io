using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;

/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 **/

namespace ConsoleApp1
{
    public class Program
    {

        public static string xmlURL = "https://GavinLod.github.io/Hotels.xml";
        public static string xmlErrorURL = "https://GavinLod.github.io/HotelsErrors.xml";
        public static string xsdURL = "https://GavinLod.github.io/Hotels.xsd";

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine("Verification Result (Valid XML):");
            Console.WriteLine(result);
            Console.WriteLine();

            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine("Verification Result (Erroneous XML):");
            Console.WriteLine(result);
            Console.WriteLine();

            result = Xml2Json("Hotels.xml");
            Console.WriteLine("Converted JSON:");
            Console.WriteLine(result);
        }

        // Q2.1
        //this method validates the XML located at xmlUrl against the XSD located at xsdUrl.
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            List<string> errors = new List<string>();

            //set up XML reader settings with the provided XSD schema.
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add(null, xsdUrl);
            settings.ValidationType = ValidationType.Schema;

            //attach a callback that collects any validation errors.
            settings.ValidationEventHandler += (sender, e) =>
            {
                errors.Add(e.Message);
            };

            try
            {
                //create an XmlReader using the settings and read the entire XML document.
                using (XmlReader reader = XmlReader.Create(xmlUrl, settings))
                {
                    while (reader.Read()) { /* Reading triggers validation */ }
                }
            }
            catch (Exception ex)
            {
                errors.Add("Exception: " + ex.Message);
            }

            //return the results.
            if (errors.Count == 0)
            {
                return "No errors are found.";
            }
            else
            {
                return "Validation Errors: " + string.Join("; ", errors);
            }
        }

        // Q2.2
        //this method loads an XML document from xmlUrl, converts it to JSON,
        //and then replaces any attribute identifiers (starting with "@") with "_" 
        public static string Xml2Json(string xmlUrl)
        {
            string jsonText = "";
            try
            {
                //load the XML document.
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlUrl);

                // Remove the XML declaration if present.
                if (doc.FirstChild != null && doc.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
                {
                    doc.RemoveChild(doc.FirstChild);
                }

                // Remove any comment nodes.
                RemoveComments(doc);

                //serialize the XML document to JSON text.
                jsonText = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);

                //parse the JSON into a JObject for modification.
                JObject jObj = JObject.Parse(jsonText);

                //recursively replace attributes names from "@" to "_" in the JSON structure.
                ReplaceAttributePrefixes(jObj);

                //get the final JSON string.
                jsonText = jObj.ToString();
            }
            catch (Exception ex)
            {
                jsonText = "Error converting XML to JSON: " + ex.Message;
            }
            return jsonText;
        }

        //helper method to recursively remove comment nodes from an XmlNode.
        private static void RemoveComments(XmlNode node)
        {
            for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
            {
                XmlNode child = node.ChildNodes[i];
                if (child.NodeType == XmlNodeType.Comment)
                {
                    node.RemoveChild(child);
                }
                else
                {
                    RemoveComments(child);
                }
            }
        }

        //helper method to recursively traverse the JToken tree
        //replace any property names starting with "@" with "_" so that attributes are renamed correctly.
        private static void ReplaceAttributePrefixes(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                //create a list of properties to avoid modification during iteration.
                var properties = new List<JProperty>(token.Children<JProperty>());
                foreach (var prop in properties)
                {
                    if (prop.Name.StartsWith("@"))
                    {
                        string newName = "_" + prop.Name.Substring(1);
                        //replace the property with the new name.
                        prop.Replace(new JProperty(newName, prop.Value));
                    }
                    ReplaceAttributePrefixes(prop.Value);
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (var child in token.Children())
                {
                    ReplaceAttributePrefixes(child);
                }
            }
        }
    }
}
