using System;
using System.Collections.Generic;
using System.Linq;
using EtherCATTestApi.EtherCATTestEngine.ESI;
using EtherCATTestApi.EtherCATTestEngine.Mailbox;

namespace EtherCATTestApi.EtherCATTestEngine.ObjectDictionary
{
    public class ObjectDictionaryValidator
    {
        private readonly CoETester _coETester;

        public ObjectDictionaryValidator(CoETester coETester)
        {
            _coETester = coETester;
        }

        public async Task<ObjectDictionaryValidationResult> ValidateObjectDictionary(ESIInfo esiInfo)
        {
            var result = new ObjectDictionaryValidationResult();
            
            if (esiInfo.ObjectDictionary == null || !esiInfo.ObjectDictionary.Any())
            {
                result.IsValid = false;
                result.Errors.Add("ESI file does not contain object dictionary information");
                return result;
            }

            // Validate each object in the dictionary
            foreach (var objEntry in esiInfo.ObjectDictionary)
            {
                var objectValidation = await ValidateObject(objEntry);
                result.ObjectValidationResults.Add(objectValidation);
                
                if (!objectValidation.IsValid)
                {
                    result.IsValid = false;
                    result.Errors.AddRange(objectValidation.Errors);
                }
            }

            // Calculate statistics
            result.TotalObjects = esiInfo.ObjectDictionary.Count;
            result.ValidObjects = result.ObjectValidationResults.Count(r => r.IsValid);
            result.InvalidObjects = result.ObjectValidationResults.Count(r => !r.IsValid);
            
            return result;
        }

        private async Task<ObjectValidationResult> ValidateObject(ObjectDictionaryEntry objEntry)
        {
            var result = new ObjectValidationResult
            {
                ObjectIndex = objEntry.Index,
                ObjectName = objEntry.Name,
                IsValid = true
            };

            try
            {
                // Read object type from device
                // This is a simplified implementation - actual CoE read would be more complex
                // For demo purposes, we'll just check if we can read the object
                var canRead = await _coETester.TestSDOReadAsync(objEntry.Index, 0x00);
                
                if (!canRead)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Cannot read object {objEntry.Index:X4} from device");
                    return result;
                }

                // Validate subindices if any
                if (objEntry.SubIndices != null && objEntry.SubIndices.Any())
                {
                    foreach (var subEntry in objEntry.SubIndices)
                    {
                        var subValidation = await ValidateSubIndex(objEntry.Index, subEntry);
                        result.SubIndexValidationResults.Add(subValidation);
                        
                        if (!subValidation.IsValid)
                        {
                            result.IsValid = false;
                            result.Errors.AddRange(subValidation.Errors);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Error validating object {objEntry.Index:X4}: {ex.Message}");
            }

            return result;
        }

        private async Task<SubIndexValidationResult> ValidateSubIndex(ushort objectIndex, SubIndexEntry subEntry)
        {
            var result = new SubIndexValidationResult
            {
                ObjectIndex = objectIndex,
                SubIndex = subEntry.SubIndex,
                SubIndexName = subEntry.Name,
                IsValid = true
            };

            try
            {
                // Read subindex from device
                var canRead = await _coETester.TestSDOReadAsync(objectIndex, subEntry.SubIndex);
                
                if (!canRead)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Cannot read subindex {subEntry.SubIndex:X2} from object {objectIndex:X4}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Error validating subindex {subEntry.SubIndex:X2} of object {objectIndex:X4}: {ex.Message}");
            }

            return result;
        }
    }

    public class ObjectDictionaryValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
        public List<ObjectValidationResult> ObjectValidationResults { get; set; } = new List<ObjectValidationResult>();
        public int TotalObjects { get; set; }
        public int ValidObjects { get; set; }
        public int InvalidObjects { get; set; }
    }

    public class ObjectValidationResult
    {
        public ushort ObjectIndex { get; set; }
        public string ObjectName { get; set; }
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
        public List<SubIndexValidationResult> SubIndexValidationResults { get; set; } = new List<SubIndexValidationResult>();
    }

    public class SubIndexValidationResult
    {
        public ushort ObjectIndex { get; set; }
        public byte SubIndex { get; set; }
        public string SubIndexName { get; set; }
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
    }
}