using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TCGClient.Networking.Net
{
    public class Network : INetwork
    {
        private byte[] _buffer;
        private Socket _client;
        private bool _connected;
        private bool _sending;

        public void Initialize() {
            _client = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            _client.BeginConnect(new IPEndPoint(IPAddress.Parse(Data.DataManager.Settings.Host), Data.DataManager.Settings.Port), new AsyncCallback(ConnectCallBack), null);

            while (!_connected) {
                // waste time.
            }
        }
        public void Destroy() {
            _client.Disconnect(false);
        }

        private void ConnectCallBack(IAsyncResult ar) {
            try {
                _client.EndConnect(ar);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.ReadKey(false);
                Environment.Exit(1);
            }

            _buffer = new byte[_client.ReceiveBufferSize];
            _client.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), null);
            _connected = true;
        }
        private void ReceiveCallBack(IAsyncResult ar) {
            int length = 0;

            try {
                length = _client.EndReceive(ar);
            }
            catch (Exception e) {
                Environment.Exit(1);
            }

            Array.Resize(ref _buffer, length);
            NetworkManager.PacketManager.HandlePacket(_buffer);
            Array.Resize(ref _buffer, _client.ReceiveBufferSize);

            _client.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), null);
        }
        private void SendCallBack(IAsyncResult ar) {
            _client.EndSend(ar);
            _sending = false;
        }

        public void SendData(byte[] array) {
            if (!_connected) {
                return;
            }

            if (array.Length > _client.SendBufferSize) {
                return;
            }

            if (_sending) {
                object packetObject = new object[] { array };
                var thread = new Thread(new ParameterizedThreadStart(SendDataWait));
                thread.Start(packetObject);
                return;
            }

            _sending = true;
            _client.BeginSend(array, 0, array.Length, SocketFlags.None, new AsyncCallback(SendCallBack), null);
        }
        private void SendDataWait(object packetObject) {
            Array packet = new object[1];
            byte[] array = (byte[])packet.GetValue(0);

            int start = Environment.TickCount;

            while (_sending) {
                if (Environment.TickCount - start > 1000) {
                    Console.WriteLine("NETWORK-WARNING: Dropped a packet.");
                    return;
                }
            }

            _sending = true;
            _client.BeginSend(array, 0, array.Length, SocketFlags.None, new AsyncCallback(SendCallBack), null);
        }
    }
}
