using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace EtherCATTestApi.EtherCATTestEngine.ESI
{
    public class ESIParser
    {
        public ESIInfo ParseESIFile(string filePath)
        {
            Console.WriteLine($"Parsing ESI file: {filePath}");
            
            try
            {
                var doc = XDocument.Load(filePath);
                return ParseESI(doc);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse ESI file: {ex.Message}");
                throw;
            }
        }
        
        public ESIInfo ParseESIStream(Stream stream)
        {
            Console.WriteLine("Parsing ESI stream");
            
            try
            {
                var doc = XDocument.Load(stream);
                return ParseESI(doc);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse ESI stream: {ex.Message}");
                throw;
            }
        }
        
        private ESIInfo ParseESI(XDocument doc)
        {
            var esiInfo = new ESIInfo();
            
            // Parse basic device information
            var deviceInfo = doc.Descendants("DeviceInfo").FirstOrDefault();
            if (deviceInfo != null)
            {
                esiInfo.VendorId = ushort.Parse(deviceInfo.Element("VendorID").Value);
                esiInfo.ProductCode = uint.Parse(deviceInfo.Element("ProductCode").Value);
                esiInfo.RevisionNo = ushort.Parse(deviceInfo.Element("RevisionNo").Value);
                esiInfo.OrderCode = deviceInfo.Element("OrderCode").Value;
            }
            
            // Parse XML version and schema
            esiInfo.XmlVersion = doc.Root.Attribute("version").Value;
            
            // Parse Object Dictionary
            ParseObjectDictionary(doc, esiInfo);
            
            // Parse Sync Managers
            ParseSyncManagers(doc, esiInfo);
            
            // Parse PDO Mapping
            ParsePDOMapping(doc, esiInfo);
            
            Console.WriteLine($"ESI File parsed successfully: VendorID={esiInfo.VendorId:X4}, ProductCode={esiInfo.ProductCode:X8}");
            
            return esiInfo;
        }
        
        private void ParseObjectDictionary(XDocument doc, ESIInfo esiInfo)
        {
            esiInfo.ObjectDictionary = new List<ObjectDictionaryEntry>();
            
            var objectEntries = doc.Descendants("ObjectEntry");
            foreach (var entry in objectEntries)
            {
                var objEntry = new ObjectDictionaryEntry
                {
                    Index = ushort.Parse(entry.Attribute("Index").Value),
                    Name = entry.Element("Name").Value,
                    ObjectType = entry.Element("ObjectType").Value,
                    DataType = entry.Element("DataType").Value
                };
                
                // Parse subindices if any
                var subindices = entry.Descendants("SubIndex");
                foreach (var subindex in subindices)
                {
                    var subEntry = new SubIndexEntry
                    {
                        SubIndex = byte.Parse(subindex.Attribute("SubIndex").Value),
                        Name = subindex.Element("Name").Value,
                        DataType = subindex.Element("DataType").Value,
                        Value = subindex.Element("Value")?.Value
                    };
                    objEntry.SubIndices.Add(subEntry);
                }
                
                esiInfo.ObjectDictionary.Add(objEntry);
            }
        }
        
        private void ParseSyncManagers(XDocument doc, ESIInfo esiInfo)
        {
            esiInfo.SyncManagers = new List<SyncManagerEntry>();
            
            var syncManagers = doc.Descendants("SyncManager");
            foreach (var sm in syncManagers)
            {
                var smEntry = new SyncManagerEntry
                {
                    Index = byte.Parse(sm.Attribute("Index").Value),
                    Name = sm.Element("Name").Value,
                    Direction = sm.Element("Direction").Value,
                    WatchdogMode = sm.Element("WatchdogMode").Value
                };
                
                esiInfo.SyncManagers.Add(smEntry);
            }
        }
        
        private void ParsePDOMapping(XDocument doc, ESIInfo esiInfo)
        {
            esiInfo.PDOMappings = new List<PDOMappingEntry>();
            
            var pdoMappings = doc.Descendants("PDOMapping");
            foreach (var pdo in pdoMappings)
            {
                var pdoEntry = new PDOMappingEntry
                {
                    PDOIndex = ushort.Parse(pdo.Attribute("PDOIndex").Value),
                    Direction = pdo.Element("Direction").Value
                };
                
                // Parse PDO entries
                var pdoEntries = pdo.Descendants("PDOEntry");
                foreach (var entry in pdoEntries)
                {
                    var pdoSubEntry = new PDOEntry
                    {
                        ObjectIndex = ushort.Parse(entry.Attribute("ObjectIndex").Value),
                        SubIndex = byte.Parse(entry.Attribute("SubIndex").Value),
                        BitLength = int.Parse(entry.Attribute("BitLength").Value)
                    };
                    pdoEntry.Entries.Add(pdoSubEntry);
                }
                
                esiInfo.PDOMappings.Add(pdoEntry);
            }
        }
    }
    
    public class ESIInfo
    {
        public ushort VendorId { get; set; }
        public uint ProductCode { get; set; }
        public ushort RevisionNo { get; set; }
        public string OrderCode { get; set; }
        public string XmlVersion { get; set; }
        
        // Object Dictionary
        public List<ObjectDictionaryEntry> ObjectDictionary { get; set; }
        
        // Sync Managers
        public List<SyncManagerEntry> SyncManagers { get; set; }
        
        // PDO Mappings
        public List<PDOMappingEntry> PDOMappings { get; set; }
    }
    
    public class ObjectDictionaryEntry
    {
        public ushort Index { get; set; }
        public string Name { get; set; }
        public string ObjectType { get; set; }
        public string DataType { get; set; }
        public List<SubIndexEntry> SubIndices { get; set; } = new List<SubIndexEntry>();
    }
    
    public class SubIndexEntry
    {
        public byte SubIndex { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }
    }
    
    public class SyncManagerEntry
    {
        public byte Index { get; set; }
        public string Name { get; set; }
        public string Direction { get; set; }
        public string WatchdogMode { get; set; }
    }
    
    public class PDOMappingEntry
    {
        public ushort PDOIndex { get; set; }
        public string Direction { get; set; }
        public List<PDOEntry> Entries { get; set; } = new List<PDOEntry>();
    }
    
    public class PDOEntry
    {
        public ushort ObjectIndex { get; set; }
        public byte SubIndex { get; set; }
        public int BitLength { get; set; }
    }
}