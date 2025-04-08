using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{    public class Program
    {
        //URLs for the valid XML file, erroneous XML file, and the XSD schema.
        public static string xmlURL = "https://www.public.asu.edu/~1222005330/Hotels.xml";
        public static string xmlErrorURL = "https://www.public.asu.edu/~1222005330/HotelsErrors.xml";
        public static string xsdURL = "https://www.public.asu.edu/~1222005330/Hotels.xsd";

        //list to store validation errors during the XML schema validation process.
        private static List<string> validationErrors = new List<string>();

        public static void Main(string[] args)
        {
            //validate the correct XML file.
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine("Verification of valid XML:");
            Console.WriteLine(result);
            Console.WriteLine();

            //validate the erroneous XML file.
            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine("Verification of erroneous XML:");
            Console.WriteLine(result);
            Console.WriteLine();

            //convert the valid XML file to JSON format.
            string jsonResult = Xml2Json(xmlURL);
            Console.WriteLine("Converted JSON:");
            Console.WriteLine(jsonResult);
        }
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            //clear previous errors.
            validationErrors.Clear();

            //set up XML reader settings.
            XmlReaderSettings settings = new XmlReaderSettings();
            //add schema from the given URL.
            settings.Schemas.Add(null, xsdUrl);
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

            try
            {
                //create an XML reader that will validate the document.
                using (XmlReader reader = XmlReader.Create(xmlUrl, settings))
                {
                    //reading the entire XML document triggers the validation events.
                    while (reader.Read()) { }
                }
            }
            catch (Exception ex)
            {
                //in case of exceptions during reading, add the exception message to errors.
                validationErrors.Add(ex.Message);
            }

            //if no errors were recorded, return "No Error".
            if (validationErrors.Count == 0)
            {
                return "No Error";
            }
            else
            {
                //return a message that concatenates all error messages.
                return "Validation Errors: " + string.Join("; ", validationErrors);
            }
        }
        private static void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            validationErrors.Add(e.Message);
        }

        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                //load the XML document from the given URL.
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlUrl);

                //serialize the XML document to JSON. By default, XML attributes are represented with "@".
                string json = JsonConvert.SerializeXmlNode(doc, Formatting.Indented);

                //load the JSON into a JObject for further manipulation.
                JObject jObj = JObject.Parse(json);

                //recursively replace attribute names (starting with "@") with "_" to meet the required structure.
                ReplaceAttributePrefixes(jObj);

                //return the final JSON string.
                return jObj.ToString();
            }
            catch (Exception ex)
            {
                return "Error converting XML to JSON: " + ex.Message;
            }
        }

        private static void ReplaceAttributePrefixes(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                //create list of properties to iterate through.
                var properties = new List<JProperty>(token.Children<JProperty>());
                foreach (var prop in properties)
                {
                    //if the property name starts with "@", replace it.
                    if (prop.Name.StartsWith("@"))
                    {
                        string newName = "_" + prop.Name.Substring(1);
                        prop.Replace(new JProperty(newName, prop.Value));
                    }
                    //recursively process child tokens.
                    ReplaceAttributePrefixes(prop.Value);
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (var item in token.Children())
                {
                    ReplaceAttributePrefixes(item);
                }
            }
        }
    }
}
