using System;
using System.Threading.Tasks;

namespace EtherCATTestApi.EtherCATTestEngine.DeviceIdentification
{
    public class ProductCodeValidator
    {
        private readonly EtherCATCommManager _commManager;
        
        public ProductCodeValidator(EtherCATCommManager commManager)
        {
            _commManager = commManager;
        }
        
        public async Task<bool> ValidateProductCodeAsync(uint expectedProductCode)
        {
            if (!_commManager.IsConnected)
            {
                throw new InvalidOperationException("Not connected to EtherCAT device");
            }
            
            Console.WriteLine($"Validating Product Code: Expected={expectedProductCode:X8}");
            
            // Get actual Product Code from device
            var actualProductCode = await GetProductCodeFromDeviceAsync();
            
            Console.WriteLine($"Actual Product Code: {actualProductCode:X8}");
            return actualProductCode == expectedProductCode;
        }
        
        private async Task<uint> GetProductCodeFromDeviceAsync()
        {
            // Send command to read Product Code (CoE index 0x1018, subindex 0x02)
            var command = new byte[20];
            command[0] = 0x02; // Command type: CoE SDO Read
            command[1] = 0x00; // Node ID
            command[2] = 0x10; // Index 0x1018 (Manufacturer Device Name)
            command[3] = 0x18;
            command[4] = 0x02; // Subindex 0x02
            
            var response = await _commManager.SendCommandAsync(command);
            
            // Extract Product Code from response
            if (response.Length >= 10)
            {
                return BitConverter.ToUInt32(response, 4);
            }
            
            throw new InvalidOperationException("Failed to read Product Code from device");
        }
    }
}