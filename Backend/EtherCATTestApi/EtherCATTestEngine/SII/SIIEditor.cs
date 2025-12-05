using System;
using System.Collections.Generic;

namespace EtherCATTestApi.EtherCATTestEngine.SII
{
    public class SIIEditor
    {
        private byte[] _siiData;
        private const int SII_SIZE = 8192; // Typical SII size for EtherCAT devices

        public SIIEditor()
        {
            _siiData = new byte[SII_SIZE];
        }

        public SIIEditor(byte[] initialData)
        {
            _siiData = new byte[SII_SIZE];
            Array.Copy(initialData, _siiData, Math.Min(initialData.Length, SII_SIZE));
        }

        /// <summary>
        /// Read byte from SII at specified address
        /// </summary>
        public byte ReadByte(uint address)
        {
            if (address >= SII_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address exceeds SII size");
            }
            return _siiData[address];
        }

        /// <summary>
        /// Write byte to SII at specified address
        /// </summary>
        public void WriteByte(uint address, byte value)
        {
            if (address >= SII_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address exceeds SII size");
            }
            _siiData[address] = value;
        }

        /// <summary>
        /// Read word from SII at specified address
        /// </summary>
        public ushort ReadWord(uint address)
        {
            if (address + 1 >= SII_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address + 1 exceeds SII size");
            }
            return BitConverter.ToUInt16(_siiData, (int)address);
        }

        /// <summary>
        /// Write word to SII at specified address
        /// </summary>
        public void WriteWord(uint address, ushort value)
        {
            if (address + 1 >= SII_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address + 1 exceeds SII size");
            }
            var bytes = BitConverter.GetBytes(value);
            _siiData[address] = bytes[0];
            _siiData[address + 1] = bytes[1];
        }

        /// <summary>
        /// Read double word from SII at specified address
        /// </summary>
        public uint ReadDWord(uint address)
        {
            if (address + 3 >= SII_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address + 3 exceeds SII size");
            }
            return BitConverter.ToUInt32(_siiData, (int)address);
        }

        /// <summary>
        /// Write double word to SII at specified address
        /// </summary>
        public void WriteDWord(uint address, uint value)
        {
            if (address + 3 >= SII_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address + 3 exceeds SII size");
            }
            var bytes = BitConverter.GetBytes(value);
            _siiData[address] = bytes[0];
            _siiData[address + 1] = bytes[1];
            _siiData[address + 2] = bytes[2];
            _siiData[address + 3] = bytes[3];
        }

        /// <summary>
        /// Read block of data from SII
        /// </summary>
        public byte[] ReadBlock(uint address, uint length)
        {
            if (address + length > SII_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address + length exceeds SII size");
            }
            var block = new byte[length];
            Array.Copy(_siiData, address, block, 0, length);
            return block;
        }

        /// <summary>
        /// Write block of data to SII
        /// </summary>
        public void WriteBlock(uint address, byte[] data)
        {
            if (address + data.Length > SII_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address + data length exceeds SII size");
            }
            Array.Copy(data, 0, _siiData, address, data.Length);
        }

        /// <summary>
        /// Get entire SII data
        /// </summary>
        public byte[] GetSIIData()
        {
            var copy = new byte[SII_SIZE];
            Array.Copy(_siiData, copy, SII_SIZE);
            return copy;
        }

        /// <summary>
        /// Set entire SII data
        /// </summary>
        public void SetSIIData(byte[] data)
        {
            Array.Copy(data, _siiData, Math.Min(data.Length, SII_SIZE));
        }

        /// <summary>
        /// Clear SII data to zeros
        /// </summary>
        public void Clear()
        {
            Array.Clear(_siiData, 0, SII_SIZE);
        }

        /// <summary>
        /// Validate SII data integrity
        /// </summary>
        public SIIValidationResult Validate()
        {
            var result = new SIIValidationResult();
            
            // Check CRC
            var crcValid = ValidateCRC();
            result.IsValid = crcValid;
            
            if (!crcValid)
            {
                result.Errors.Add("CRC validation failed");
            }
            
            // Check manufacturer info
            var manufacturerValid = ValidateManufacturerInfo();
            if (!manufacturerValid)
            {
                result.Errors.Add("Manufacturer information validation failed");
            }
            
            return result;
        }

        private bool ValidateCRC()
        {
            // Simplified CRC validation - actual implementation would use EtherCAT CRC algorithm
            // For demo purposes, we'll just check that the CRC is not all zeros
            var crc = ReadWord(0x0000); // CRC is typically stored at address 0x0000
            return crc != 0x0000;
        }

        private bool ValidateManufacturerInfo()
        {
            // Check that manufacturer ID is not all zeros
            var manufacturerId = ReadWord(0x0002); // Manufacturer ID is typically at address 0x0002
            return manufacturerId != 0x0000;
        }
    }

    public class SIIValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }

        public SIIValidationResult()
        {
            Errors = new List<string>();
            Warnings = new List<string>();
        }
    }
}