using System;
using System.Xml.Schema;
using System.Xml;
using System.Text;

namespace EtherCATTestApi.EtherCATTestEngine.ESI
{
    public class ESIValidator
    {
        public bool ValidateESIFile(string filePath, out string validationErrors)
        {
            validationErrors = string.Empty;
            
            try
            {
                var settings = new XmlReaderSettings
                {
                    ValidationType = ValidationType.Schema,
                    ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints |
                                     XmlSchemaValidationFlags.ProcessInlineSchema |
                                     XmlSchemaValidationFlags.ProcessSchemaLocation |
                                     XmlSchemaValidationFlags.ReportValidationWarnings
                };
                
                var validationErrorsList = new StringBuilder();
                settings.ValidationEventHandler += (sender, e) =>
                {
                    validationErrorsList.AppendLine($"{e.Severity}: {e.Message} at line {e.Exception.LineNumber}, position {e.Exception.LinePosition}");
                };
                
                using (var reader = XmlReader.Create(filePath, settings))
                {
                    while (reader.Read()) { }
                }
                
                validationErrors = validationErrorsList.ToString();
                return string.IsNullOrEmpty(validationErrors);
            }
            catch (Exception ex)
            {
                validationErrors = $"Exception during validation: {ex.Message}";
                return false;
            }
        }
        
        public bool ValidateDeviceInfoConsistency(ESIInfo esiInfo, ushort actualVendorId, uint actualProductCode)
        {
            Console.WriteLine($"Validating device info consistency:");
            Console.WriteLine($"  ESI VendorID: {esiInfo.VendorId:X4}, Actual: {actualVendorId:X4}");
            Console.WriteLine($"  ESI ProductCode: {esiInfo.ProductCode:X8}, Actual: {actualProductCode:X8}");
            
            return esiInfo.VendorId == actualVendorId && esiInfo.ProductCode == actualProductCode;
        }
    }
}