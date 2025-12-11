﻿using System;
using System.Collections.Generic;
using System.Linq;
using EtherCATTestApi.EtherCATTestEngine.ESI;
using EtherCATTestApi.EtherCATTestEngine.DeviceIdentification;

namespace EtherCATTestApi.EtherCATTestEngine.DiffTool
{
    public class ConfigComparator
    {
        private readonly VendorIdValidator _vendorIdValidator;
        private readonly ProductCodeValidator _productCodeValidator;

        public ConfigComparator(
            VendorIdValidator vendorIdValidator,
            ProductCodeValidator productCodeValidator)
        {
            _vendorIdValidator = vendorIdValidator;
            _productCodeValidator = productCodeValidator;
        }

        public async Task<ConfigDiffResult> CompareConfig(ESIInfo esiInfo, ushort actualVendorId, uint actualProductCode)
        {
            var result = new ConfigDiffResult();

            // Compare basic device information
            result.BasicInfoDiff = await CompareBasicInfo(esiInfo, actualVendorId, actualProductCode);
            
            // Compare object dictionary (this would be more detailed in a complete implementation)
            result.ObjectDictionaryDiff = CompareObjectDictionary(esiInfo);
            
            // Compare sync managers
            result.SyncManagerDiff = CompareSyncManagers(esiInfo);
            
            // Compare PDO mappings
            result.PDOMappingDiff = ComparePDOMappings(esiInfo);

            // Calculate overall result
            result.HasDifferences = !result.BasicInfoDiff.IsMatch ||
                                  result.ObjectDictionaryDiff.HasDifferences ||
                                  result.SyncManagerDiff.HasDifferences ||
                                  result.PDOMappingDiff.HasDifferences;

            if (!result.BasicInfoDiff.VendorIdMatch)
            {
                result.OverallDifferences.Add($"VendorId mismatch: expected 0x{result.BasicInfoDiff.VendorIdExpected:X4}, actual 0x{result.BasicInfoDiff.VendorIdActual:X4}");
            }
            if (!result.BasicInfoDiff.ProductCodeMatch)
            {
                result.OverallDifferences.Add($"ProductCode mismatch: expected 0x{result.BasicInfoDiff.ProductCodeExpected:X8}, actual 0x{result.BasicInfoDiff.ProductCodeActual:X8}");
            }
            foreach (var d in result.ObjectDictionaryDiff.Differences)
            {
                result.OverallDifferences.Add($"OD: {d}");
            }
            foreach (var d in result.SyncManagerDiff.Differences)
            {
                result.OverallDifferences.Add($"SM: {d}");
            }
            foreach (var d in result.PDOMappingDiff.Differences)
            {
                result.OverallDifferences.Add($"PDO: {d}");
            }

            return result;
        }

        private async Task<BasicInfoDiff> CompareBasicInfo(ESIInfo esiInfo, ushort actualVendorId, uint actualProductCode)
        {
            var result = new BasicInfoDiff();

            // Compare Vendor ID
            result.VendorIdExpected = esiInfo.VendorId;
            result.VendorIdActual = actualVendorId;
            result.VendorIdMatch = esiInfo.VendorId == actualVendorId;

            // Compare Product Code
            result.ProductCodeExpected = esiInfo.ProductCode;
            result.ProductCodeActual = actualProductCode;
            result.ProductCodeMatch = esiInfo.ProductCode == actualProductCode;

            // Compare revision number if available
            result.RevisionNumberExpected = esiInfo.RevisionNo;
            // In a complete implementation, we would read the actual revision number from the device
            result.RevisionNumberActual = 0;
            result.RevisionNumberMatch = esiInfo.RevisionNo == 0; // Default match if not provided

            // Calculate overall match
            result.IsMatch = result.VendorIdMatch && result.ProductCodeMatch && result.RevisionNumberMatch;

            return result;
        }

        private ObjectDictionaryDiff CompareObjectDictionary(ESIInfo esiInfo)
        {
            var result = new ObjectDictionaryDiff();

            if (esiInfo.ObjectDictionary == null || !esiInfo.ObjectDictionary.Any())
            {
                result.HasDifferences = false;
                result.Differences.Add("ESI file does not contain object dictionary information");
                return result;
            }

            // In a complete implementation, we would compare each object and subindex with the actual device
            // For now, we'll analyze the object dictionary structure
            result.TotalObjects = esiInfo.ObjectDictionary.Count;
            result.TotalSubIndices = esiInfo.ObjectDictionary.Sum(obj => obj.SubIndices.Count);
            
            // Check for mandatory objects
            var mandatoryIndices = new List<ushort> { 0x1000, 0x1001, 0x1008, 0x1018, 0x1019, 0x1020, 0x1021, 0x1600 };
            
            foreach (var index in mandatoryIndices)
            {
                if (!esiInfo.ObjectDictionary.Any(obj => obj.Index == index))
                {
                    result.Differences.Add($"Mandatory object 0x{index:X4} not found in ESI file");
                }
            }
            
            result.HasDifferences = result.Differences.Count > 0;
            return result;
        }

        private SyncManagerDiff CompareSyncManagers(ESIInfo esiInfo)
        {
            var result = new SyncManagerDiff();

            if (esiInfo.SyncManagers == null || !esiInfo.SyncManagers.Any())
            {
                result.HasDifferences = false;
                result.Differences.Add("ESI file does not contain sync manager information");
                return result;
            }

            result.TotalSyncManagers = esiInfo.SyncManagers.Count;
            
            // Check for mandatory sync managers (at least SM0 and SM1)
            if (!esiInfo.SyncManagers.Any(sm => sm.Index == 0))
            {
                result.Differences.Add("Mandatory sync manager SM0 not found in ESI file");
            }
            
            if (!esiInfo.SyncManagers.Any(sm => sm.Index == 1))
            {
                result.Differences.Add("Mandatory sync manager SM1 not found in ESI file");
            }
            
            // Check for input/output sync managers
            var inputSMs = esiInfo.SyncManagers.Count(sm => sm.Direction?.Equals("Input", StringComparison.OrdinalIgnoreCase) == true);
            var outputSMs = esiInfo.SyncManagers.Count(sm => sm.Direction?.Equals("Output", StringComparison.OrdinalIgnoreCase) == true);
            
            if (inputSMs == 0)
            {
                result.Differences.Add("No input sync managers found in ESI file");
            }
            
            if (outputSMs == 0)
            {
                result.Differences.Add("No output sync managers found in ESI file");
            }
            
            result.HasDifferences = result.Differences.Count > 0;
            return result;
        }

        private PDOMappingDiff ComparePDOMappings(ESIInfo esiInfo)
        {
            var result = new PDOMappingDiff();

            if (esiInfo.PDOMappings == null || !esiInfo.PDOMappings.Any())
            {
                result.HasDifferences = false;
                result.Differences.Add("ESI file does not contain PDO mapping information");
                return result;
            }

            result.TotalPDOMappings = esiInfo.PDOMappings.Count;
            result.TotalPDOEntries = esiInfo.PDOMappings.Sum(pdo => pdo.Entries.Count);
            
            // Check for mandatory PDO mappings
            // For example, check that at least one RxPDO and one TxPDO are defined
            var rxPDOs = esiInfo.PDOMappings.Count(pdo => pdo.Direction?.Equals("Input", StringComparison.OrdinalIgnoreCase) == true);
            var txPDOs = esiInfo.PDOMappings.Count(pdo => pdo.Direction?.Equals("Output", StringComparison.OrdinalIgnoreCase) == true);
            
            if (rxPDOs == 0)
            {
                result.Differences.Add("No RxPDO mappings found in ESI file");
            }
            
            if (txPDOs == 0)
            {
                result.Differences.Add("No TxPDO mappings found in ESI file");
            }
            
            // Check PDO entries count
            foreach (var pdo in esiInfo.PDOMappings)
            {
                if (pdo.Entries.Count == 0)
                {
                    result.Differences.Add($"PDO 0x{pdo.PDOIndex:X4} has no entries");
                }
            }
            
            result.HasDifferences = result.Differences.Count > 0;
            return result;
        }
    }

    // Diff result classes
    public class ConfigDiffResult
    {
        public bool HasDifferences { get; set; }
        public BasicInfoDiff BasicInfoDiff { get; set; }
        public ObjectDictionaryDiff ObjectDictionaryDiff { get; set; }
        public SyncManagerDiff SyncManagerDiff { get; set; }
        public PDOMappingDiff PDOMappingDiff { get; set; }
        public List<string> OverallDifferences { get; set; } = new List<string>();
    }

    public class BasicInfoDiff
    {
        public bool IsMatch { get; set; }
        public ushort VendorIdExpected { get; set; }
        public ushort VendorIdActual { get; set; }
        public bool VendorIdMatch { get; set; }
        public uint ProductCodeExpected { get; set; }
        public uint ProductCodeActual { get; set; }
        public bool ProductCodeMatch { get; set; }
        public ushort RevisionNumberExpected { get; set; }
        public ushort RevisionNumberActual { get; set; }
        public bool RevisionNumberMatch { get; set; }
    }

    public class ObjectDictionaryDiff
    {
        public bool HasDifferences { get; set; }
        public int TotalObjects { get; set; }
        public int TotalSubIndices { get; set; }
        public List<string> Differences { get; set; } = new List<string>();
    }

    public class SyncManagerDiff
    {
        public bool HasDifferences { get; set; }
        public int TotalSyncManagers { get; set; }
        public List<string> Differences { get; set; } = new List<string>();
    }

    public class PDOMappingDiff
    {
        public bool HasDifferences { get; set; }
        public int TotalPDOMappings { get; set; }
        public int TotalPDOEntries { get; set; }
        public List<string> Differences { get; set; } = new List<string>();
    }
}
