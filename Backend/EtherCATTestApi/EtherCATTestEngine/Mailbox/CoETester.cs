﻿using System;
using System.Threading.Tasks;

namespace EtherCATTestApi.EtherCATTestEngine.Mailbox
{
    public class CoETester
    {
        private readonly EtherCATCommManager _commManager;
        
        public CoETester(EtherCATCommManager commManager)
        {
            _commManager = commManager;
        }
        
        public async Task<bool> TestSDOReadAsync(ushort index, byte subIndex)
        {
            if (!_commManager.IsConnected)
            {
                throw new InvalidOperationException("Not connected to EtherCAT device");
            }
            
            Console.WriteLine($"Testing CoE SDO Read: Index={index:X4}, SubIndex={subIndex:X2}");
            
            var command = BuildSDOReadCommand(index, subIndex);
            var response = await _commManager.SendCommandAsync(command);
            
            return ValidateSDOResponse(response);
        }
        
        public async Task<bool> TestSDOWriteAsync(ushort index, byte subIndex, byte[] data)
        {
            if (!_commManager.IsConnected)
            {
                throw new InvalidOperationException("Not connected to EtherCAT device");
            }
            
            Console.WriteLine($"Testing CoE SDO Write: Index={index:X4}, SubIndex={subIndex:X2}");
            
            var command = BuildSDOWriteCommand(index, subIndex, data);
            var response = await _commManager.SendCommandAsync(command);
            
            return ValidateSDOResponse(response);
        }
        
        private byte[] BuildSDOReadCommand(ushort index, byte subIndex)
        {
            // Simplified SDO read command - actual implementation would follow CoE protocol
            var command = new byte[20];
            command[0] = 0x02; // Command type: CoE SDO Read
            command[1] = 0x00; // Node ID (placeholder)
            command[2] = (byte)(index >> 8); // Index high byte
            command[3] = (byte)index; // Index low byte
            command[4] = subIndex;
            return command;
        }
        
        private byte[] BuildSDOWriteCommand(ushort index, byte subIndex, byte[] data)
        {
            // Simplified SDO write command
            var command = new byte[20 + data.Length];
            command[0] = 0x03; // Command type: CoE SDO Write
            command[1] = 0x00; // Node ID (placeholder)
            command[2] = (byte)(index >> 8);
            command[3] = (byte)index;
            command[4] = subIndex;
            Buffer.BlockCopy(data, 0, command, 5, data.Length);
            return command;
        }
        
        private bool ValidateSDOResponse(byte[] response)
        {
            // Simplified response validation
            if (response.Length < 5) return false;
            return response[0] == 0x00; // Success flag
        }
    }
}