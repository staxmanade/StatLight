using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace StatLight.Core.Reporting.Providers.Xml
{
    public static class XmlSchemaValidatorHelper
    {
        public static bool ValidateSchema(string pathToXmlFileToValidate, string xsdSchemaString, out IList<string> validationErrors)
        {
            validationErrors = new List<string>();

            var currentValidationErrors = new List<string>();


            using (var stringReader = new StringReader(xsdSchemaString))
            using (var xmlReader = XmlReader.Create(stringReader))
            {
                var schema = XmlSchema.Read(xmlReader, null);
                var schemaSet = new XmlSchemaSet();
                schemaSet.Add(schema);

                var settings = new XmlReaderSettings();
                settings.ValidationEventHandler += (sender, e) => currentValidationErrors.Add(e.Message);
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas = schemaSet;

                using (var reader = XmlReader.Create(pathToXmlFileToValidate, settings))
                {
                    while (reader.Read())
                    {
                    }
                }
            }

            if (currentValidationErrors.Count > 0)
            {
                validationErrors = currentValidationErrors;
                return false;
            }
            return true;
        }
    }
}