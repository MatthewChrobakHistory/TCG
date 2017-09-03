namespace TCGServer.Networking
{
    interface INetwork
    {
        void Initialize();
        void Destroy();
        void SendDataTo(int index, byte[] array);
        void SendDataToAll(byte[] array);
        void SendDataToAllBut(int exception, byte[] array);
        bool isConnected(int index);
    }
}
