﻿using System;
using System.Threading.Tasks;

namespace EtherCATTestApi.EtherCATTestEngine.DeviceIdentification
{
    public class VendorIdValidator
    {
        private readonly EtherCATCommManager _commManager;
        
        public VendorIdValidator(EtherCATCommManager commManager)
        {
            _commManager = commManager;
        }
        
        public async Task<bool> ValidateVendorIdAsync(ushort expectedVendorId)
        {
            if (!_commManager.IsConnected)
            {
                throw new InvalidOperationException("Not connected to EtherCAT device");
            }
            
            Console.WriteLine($"Validating Vendor ID: Expected={expectedVendorId:X4}");
            
            // Get actual Vendor ID from device
            var actualVendorId = await GetVendorIdFromDeviceAsync();
            
            Console.WriteLine($"Actual Vendor ID: {actualVendorId:X4}");
            return actualVendorId == expectedVendorId;
        }
        
        private async Task<ushort> GetVendorIdFromDeviceAsync()
        {
            // Send command to read Vendor ID (CoE index 0x1018, subindex 0x01)
            var command = new byte[20];
            command[0] = 0x02; // Command type: CoE SDO Read
            command[1] = 0x00; // Node ID
            command[2] = 0x10; // Index 0x1018 (Manufacturer Device Name)
            command[3] = 0x18;
            command[4] = 0x01; // Subindex 0x01
            
            var response = await _commManager.SendCommandAsync(command);
            
            // Extract Vendor ID from response
            if (response.Length >= 8)
            {
                return BitConverter.ToUInt16(response, 4);
            }
            
            throw new InvalidOperationException("Failed to read Vendor ID from device");
        }
    }
}