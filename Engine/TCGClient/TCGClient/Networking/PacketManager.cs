using TCGClient.IO;
using System;
using System.Collections.Generic;
using TCGClient.Data;
using TCGClient.Data.Models;

namespace TCGClient.Networking
{
    public class PacketManager
    {
        private delegate void HandleDataMethod(byte[] array);
        private List<HandleDataMethod> _handleData;

        public void Initialize() {
            // Add packet handlers in the same order as they appear
            // in the Packets enumeration in Packets.cs on the SERVER side.
            _handleData = new List<HandleDataMethod>();

            _handleData.Add(new HandleDataMethod(HandleServerMsg));
            _handleData.Add(new HandleDataMethod(HandleChangeClientState));
            _handleData.Add(new HandleDataMethod(HandleGameData));
            _handleData.Add(new HandleDataMethod(HandleEnemyField));
            _handleData.Add(new HandleDataMethod(HandleEnemyHeld));
            _handleData.Add(new HandleDataMethod(HandleMyField));
            _handleData.Add(new HandleDataMethod(HandleMyHeld));
            _handleData.Add(new HandleDataMethod(HandleEnableTurn));
        }
        private byte[] RemovePacketHead(byte[] array) {
            // If the size of the entire buffer is 8, all the packet contains is the head.
            // Packets like that are just initiation packets, and don't actually contin
            // other data. So, what we return won't be manipulated anyways. Return null.
            if (array.Length == 8) {
                return null;
            }

            // Create a new array to hold the clipped data.
            byte[] clippedArray = new byte[array.Length - 8];

            // Clip the data.
            for (int i = 8; i < array.Length; i++) {
                clippedArray[i - 8] = array[i];
            }

            // Return it.
            return clippedArray;
        }
        public void HandlePacket(byte[] array) {
            bool process = true;
            var packet = new DataBuffer(array);

            while (process) {
                int size = packet.ReadInt();
                byte[] packetbuffer = packet.toArray();

                if (packetbuffer.Length > size) {
                    byte[] excessbuffer = new byte[packetbuffer.Length - size];
                    Array.ConstrainedCopy(packetbuffer, size, excessbuffer, 0, packetbuffer.Length - size);
                    Array.Resize(ref packetbuffer, size);

                    int head = packet.ReadInt();
                    if (head >= 0 && head < _handleData.Count) {
                        _handleData[head].Invoke(RemovePacketHead(packetbuffer));
                    }

                    packet = new DataBuffer(excessbuffer);
                } else {
                    int head = packet.ReadInt();
                    process = false;
                    if (head >= 0 && head < _handleData.Count) {
                        _handleData[head].Invoke(RemovePacketHead(packetbuffer));
                    }
                }
            }

            packet.Dispose();
        }

        #region Handling incoming packets
        private void HandleServerMsg(byte[] array) {
            var packet = new DataBuffer(array);
            string message = packet.ReadString();
            Graphics.GraphicsManager.ServerMessage(message);
        }
        private void HandleChangeClientState(byte[] array) {
            var packet = new DataBuffer(array);
            byte state = packet.ReadByte();

            if (state < (int)GameState.Length) {
                Program.State = state;
            }

            if (Program.State == (int)GameState.Game) {
                var game = Data.DataManager.Game;
                game.Cards = new List<Data.Models.Card>();
            }
        }
        private void HandleGameData(byte[] array) {
            var packet = new DataBuffer(array);
            var game = DataManager.Game;

            // 0 = me
            // 1 = enemy
            for (int i = 0; i < 2; i++) {
                var player = game.Player[i];
                player.Name = packet.ReadString();
                player.Life = packet.ReadInt();
            }

            game.Player[0].Experience = packet.ReadInt();
            game.Player[0].Energy = packet.ReadInt();
        }
        private void HandleEnemyField(byte[] array) {
            var packet = new DataBuffer(array);
            int count = packet.ReadInt();
            DataManager.RemoveAllCards(CardType.EnemyField);
            for (int i = 0; i < count; i++) {
                int id = packet.ReadInt();
                string surface = packet.ReadString();
                int attack = packet.ReadInt();
                int health = packet.ReadInt();
                DataManager.AddCard(CardType.EnemyField, surface, id, attack, health);
            }
        }
        private void HandleEnemyHeld(byte[] array) {
            var packet = new DataBuffer(array);
            int count = packet.ReadInt();
            DataManager.RemoveAllCards(CardType.EnemyHeld);
            for (int i = 0; i < count; i++) {
                int id = packet.ReadInt();
                DataManager.AddCard(CardType.EnemyHeld, "Blank", id, 0, 0);
            }
        }
        private void HandleMyHeld(byte[] array) {
            var packet = new DataBuffer(array);
            int count = packet.ReadInt();
            DataManager.RemoveAllCards(CardType.MyHeld);
            for (int i = 0; i < count; i++) {
                int id = packet.ReadInt();
                string surface = packet.ReadString();
                int attack = packet.ReadInt();
                int health = packet.ReadInt();
                DataManager.AddCard(CardType.MyHeld, surface, id, attack, health);
            }
        }
        private void HandleMyField(byte[] array) {
            var packet = new DataBuffer(array);
            int count = packet.ReadInt();
            DataManager.RemoveAllCards(CardType.MyField);
            for (int i = 0; i < count; i++) {
                int id = packet.ReadInt();
                string surface = packet.ReadString();
                int attack = packet.ReadInt();
                int health = packet.ReadInt();
                DataManager.AddCard(CardType.MyField, surface, id, attack, health);
            }
        }

        private void HandleEnableTurn(byte[] array) {
            var packet = new DataBuffer(array);
            DataManager.Game.myTurn = packet.ReadBool();
            if (DataManager.Game.myTurn) {
                Graphics.GraphicsManager.ServerMessage("It is your turn!");
                Graphics.GraphicsManager.EnableTurn();
            } else {
                Graphics.GraphicsManager.ServerMessage("It is your enemy's turn!");
            }
        }
        #endregion

        #region Sending outgoing packets
        public void SendRequestLogin(string username, string password) {
            var packet = new DataBuffer();
            packet.Write(Packets.SendRequestLogin);
            packet.Write(username);
            packet.Write(password);
            NetworkManager.SendData(packet.toNetworkArray());
        }
        public void SendRequestRegister(string username, string password) {
            var packet = new DataBuffer();
            packet.Write(Packets.SendRequestRegister);
            packet.Write(username);
            packet.Write(password);
            NetworkManager.SendData(packet.toNetworkArray());
        }
        public void SendRequestChangeClientState(GameState state) {
            var packet = new DataBuffer();
            packet.Write(Packets.SendRequestChangeClientState);
            packet.Write((byte)state);
            NetworkManager.SendData(packet.toNetworkArray());
        }
        public void SendEndTurn() {
            if (DataManager.Game.myTurn) {
                var packet = new DataBuffer();
                packet.Write(Packets.SendEndTurn);
                packet.Write(1);
                NetworkManager.SendData(packet.toNetworkArray());
            }
        }
        #endregion
    }
}
