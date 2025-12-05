using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EtherCATTestApi.EtherCATTestEngine
{
    public class EtherCATCommManager
    {
        private Socket _socket;
        private bool _isConnected;
        
        public event EventHandler<bool> ConnectionStatusChanged;
        
        public bool IsConnected => _isConnected;
        
        public async Task<bool> ConnectAsync(string ipAddress, int port = 34980)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await _socket.ConnectAsync(ipAddress, port);
                _isConnected = true;
                ConnectionStatusChanged?.Invoke(this, true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(this, false);
                return false;
            }
        }
        
        public void Disconnect()
        {
            if (_socket != null && _socket.Connected)
            {
                _socket.Close();
            }
            _isConnected = false;
            ConnectionStatusChanged?.Invoke(this, false);
        }
        
        public async Task<byte[]> SendCommandAsync(byte[] command)
        {
            if (!_isConnected || _socket == null)
            {
                throw new InvalidOperationException("Not connected to EtherCAT device");
            }
            
            await _socket.SendAsync(command, SocketFlags.None);
            
            var buffer = new byte[1024];
            var bytesRead = await _socket.ReceiveAsync(buffer, SocketFlags.None);
            
            var response = new byte[bytesRead];
            Array.Copy(buffer, response, bytesRead);
            
            return response;
        }
    }
}