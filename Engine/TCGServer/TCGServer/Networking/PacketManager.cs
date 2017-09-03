using TCGServer.IO;
using TCGServer.Data.Models;
using System;
using System.Collections.Generic;
using TCGServer.Data;

namespace TCGServer.Networking
{
    public class PacketManager
    {
        private delegate void HandleData(int index, byte[] array);
        private List<HandleData> _handleData;

        public void Initialize() {
            // Add packet handlers in the same order as they appear
            // in the Packets enumeration in Packets.cs on the CLIENT side.
            _handleData = new List<HandleData>();
            _handleData.Add(new HandleData(HandleRequestLogin));
            _handleData.Add(new HandleData(HandleRequestRegister));
            _handleData.Add(new HandleData(HandleRequestChangeClientState));
            _handleData.Add(new HandleData(HandleEndTurn));
        }

        private byte[] RemovePacketHead(byte[] array) {
            // If the size of the entire buffer is 8, all the packet contains is the head.
            // Packets like that are just initiation packets, and don't actually contain
            // any other data. What we return won't be manipulated anyways. Return null.
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

        public void HandlePacket(int index, byte[] array) {
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
                        _handleData[head].Invoke(index, RemovePacketHead(packetbuffer));
                    }

                    packet = new DataBuffer(excessbuffer);
                } else {
                    int head = packet.ReadInt();
                    process = false;
                    if (head >= 0 && head < _handleData.Count) {
                        _handleData[head].Invoke(index, RemovePacketHead(packetbuffer));
                    }
                }
            }

            packet.Dispose();
        }

        #region Handling incoming packets
        private void HandleRequestLogin(int index, byte[] array) {
            var packet = new DataBuffer(array);
            string username = packet.ReadString();
            string password = packet.ReadString();

            foreach (var user in DataManager.User) {
                if (user != null) {
                    if (user.Username == username) {
                        SendServerMessage(index, "User is already logged in!");
                        return;
                    }
                }
            }

            if (Account.VerifyAccount(username, password)) {
                DataManager.User[index] = Account.Load(username);
                SendChangeClientState(index, ClientState.Main);
            } else {
                SendServerMessage(index, "User does not exist, or password does not match!");
            }
        }
        private void HandleRequestRegister(int index, byte[] array) {
            var packet = new DataBuffer(array);
            string username = packet.ReadString();
            string password = packet.ReadString();

            if (!Account.FileExists(username)) {
                DataManager.User[index].Create(username, password);
                SendChangeClientState(index, ClientState.Main);
            } else {
                SendServerMessage(index, "User already exists!");
            }
        }
        private void HandleRequestChangeClientState(int index, byte[] array) {
            var packet = new DataBuffer(array);

            // Hack: Right now, we just assume every uers is where they have to be. Establish checks so that people can't leave games or stuff.
            switch (packet.ReadByte()) {
                case (int)ClientState.Main:
                    // TODO: Make sure they're not in a game if they try to log out.
                    SendChangeClientState(index, ClientState.Main);
                    break;
                case (int)ClientState.Login:
                    DataManager.SaveUserAccount(index, true);
                    SendChangeClientState(index, ClientState.Login);
                    break;
                case (int)ClientState.Game:
                    for (int i = 0; i < DataManager.User.Count; i++) {
                        if (index != i && NetworkManager.isConnected(index)) {
                            var user = DataManager.User[i];

                            if (user.State == (byte)ClientState.Wait) {

                                SendChangeClientState(i, ClientState.Loading);
                                SendChangeClientState(index, ClientState.Loading);

                                DataManager.CreateGame(i, index);
                                SendGameData(i);
                                SendGameData(index);

                                // Send data here.
                                SendChangeClientState(i, ClientState.Game);
                                SendChangeClientState(index, ClientState.Game);

                                var game = DataManager.Game[user.Game.GameID];
                                for (int id = 0; id < 2; id++) {
                                    SendMyField(game.Player[id].AccountID);
                                    SendMyHeld(game.Player[id].AccountID);
                                    SendEnemyHeld(game.Player[id].AccountID);
                                    SendEnemyField(game.Player[id].AccountID);
                                }
                                SendUpdateTurn(user.Game.GameID);

                                return;
                            }
                        }
                    }

                    SendChangeClientState(index, ClientState.Wait);
                    break;
            }
            
        }

        private void HandleEndTurn(int index, byte[] array) {
            if (DataManager.Game[DataManager.User[index].Game.GameID].CurrentTurn == DataManager.User[index].Game.PlayerID) {
                int otherindex = 0;
                if (DataManager.User[index].Game.PlayerID != otherindex) {
                    otherindex = 1;
                }
                DataManager.Game[DataManager.User[index].Game.GameID].CurrentTurn = otherindex;
                SendUpdateTurn(DataManager.User[index].Game.GameID);
            }
        }
        #endregion

        #region Sending outgoing packets
        public void SendServerMessage(int index, string message) {
            var packet = new DataBuffer();
            packet.Write(Packets.SendServerMsg);
            packet.Write(message);
            NetworkManager.SendDataTo(index, packet.toNetworkArray());
        }
        public void SendChangeClientState(int index, ClientState state) {
            DataManager.User[index].State = (byte)state;

            var packet = new DataBuffer();
            packet.Write(Packets.SendChangeClientState);
            packet.Write((byte)state);
            NetworkManager.SendDataTo(index, packet.toNetworkArray());
        }
        public void SendAccountData(int sendIndex, int accountIndex) {
            var packet = new DataBuffer();
            var player = DataManager.User[accountIndex];
        }
        public void SendGameData(int index) {
            if (DataManager.User[index].State != (byte)ClientState.Loading) {
                return;
            }

            var packet = new DataBuffer();
            var user = DataManager.User[index];
            var game = DataManager.Game[user.Game.GameID];
            var player = game.Player[user.Game.PlayerID];
            packet.Write(Packets.SendGameData);

            if (user.Game.PlayerID == 0) {
                for (int i = 0; i < 2; i++) {
                    packet.Write(DataManager.User[game.Player[i].AccountID].DisplayName);
                    packet.Write(game.Player[i].Life);
                }
            } else {
                for (int i = 1; i >= 0; i--) {
                    packet.Write(DataManager.User[game.Player[i].AccountID].DisplayName);
                    packet.Write(game.Player[i].Life);
                }
            }

            // Send data only relevant for the receiving end.
            if (index == player.AccountID) {
                packet.Write(player.Experience);
                packet.Write(player.Energy);
            }

            NetworkManager.SendDataTo(index, packet.toNetworkArray());
        }
        public void SendEnemyField(int index) {
            if (DataManager.User[index].State != (byte)ClientState.Game) {
                return;
            }

            var packet = new DataBuffer();
            var user = DataManager.User[index];
            var game = DataManager.Game[user.Game.GameID];
            int enemyID = 0;
            if (user.Game.PlayerID == 0) {
                enemyID = 1;
            }
            var enemy = game.Player[enemyID];
            int count = 0;

            packet.Write(Packets.SendEnemyField);
            for (int i = 0; i < enemy.FieldCards.Length; i++) {
                var card = enemy.FieldCards[i];
                if (card.Name != "") {
                    count++;
                }
            }
            packet.Write(count);
            for (int i = 0; i < enemy.FieldCards.Length; i++) {
                var card = enemy.FieldCards[i];
                if (card.Name != "") {
                    packet.Write(i);
                    packet.Write(card.Cover);
                    packet.Write(card.Attack);
                    packet.Write(card.Health);
                }
            }

            NetworkManager.SendDataTo(index, packet.toNetworkArray());
        }
        public void SendEnemyHeld(int index) {
            if (DataManager.User[index].State != (byte)ClientState.Game) {
                return;
            }

            var packet = new DataBuffer();
            var user = DataManager.User[index];
            var game = DataManager.Game[user.Game.GameID];
            int enemyID = 0;
            if (user.Game.PlayerID == 0) {
                enemyID = 1;
            }
            var enemy = game.Player[enemyID];
            int count = 0;

            packet.Write(Packets.SendEnemyHeld);
            for (int i = 0; i < enemy.HeldCards.Length; i++) {
                if (enemy.HeldCards[i].Name != "") {
                    count++;
                }
            }
            packet.Write(count);
            for (int i = 0; i < enemy.HeldCards.Length; i++) {
                if (enemy.HeldCards[i].Name != "") {
                    packet.Write(i);
                }
            }

            NetworkManager.SendDataTo(index, packet.toNetworkArray());
        }
        public void SendMyField(int index) {
            if (DataManager.User[index].State != (byte)ClientState.Game) {
                return;
            }

            var packet = new DataBuffer();
            var user = DataManager.User[index];
            var game = DataManager.Game[user.Game.GameID];
            var player = game.Player[user.Game.PlayerID];
            int count = 0;

            packet.Write(Packets.SendMyField);
            for (int i = 0; i < player.FieldCards.Length; i++) {
                if (player.FieldCards[i].Name != "") {
                    count++;
                }
            }
            packet.Write(count);
            for (int i = 0; i < player.FieldCards.Length; i++) {
                var card = player.FieldCards[i];
                if (card.Name != "") {
                    packet.Write(i);
                    packet.Write(card.Cover);
                    packet.Write(card.Attack);
                    packet.Write(card.Health);
                }
            }

            NetworkManager.SendDataTo(index, packet.toNetworkArray());
        }
        public void SendMyHeld(int index) {
            if (DataManager.User[index].State != (byte)ClientState.Game) {
                return;
            }

            var packet = new DataBuffer();
            var user = DataManager.User[index];
            var game = DataManager.Game[user.Game.GameID];
            var player = game.Player[user.Game.PlayerID];
            int count = 0;

            packet.Write(Packets.SendMyHeld);
            for (int i = 0; i < player.HeldCards.Length; i++) {
                if (player.HeldCards[i].Name != "") {
                    count++;
                }
            }
            packet.Write(count);
            for (int i = 0; i < player.HeldCards.Length; i++) {
                var card = player.HeldCards[i];
                if (card.Name != "") {
                    packet.Write(i);
                    packet.Write(card.Cover);
                    packet.Write(card.Attack);
                    packet.Write(card.Health);
                }
            }

            NetworkManager.SendDataTo(index, packet.toNetworkArray());
        }

        public void SendUpdateTurn(int gameindex) {

            var game = DataManager.Game[gameindex];

            if (game.CurrentTurn == 0) {
                game.CurrentTurn = 1;
            } else {
                game.CurrentTurn = 0;
            }

            for (int i = 0; i < 2; i++) {
                var user = DataManager.User[DataManager.Game[gameindex].Player[i].AccountID];
                var packet = new DataBuffer();
                packet.Write(Packets.SendUpdateTurn);

                if (user.Game.PlayerID == game.CurrentTurn) {
                    packet.Write(true);
                } else {
                    packet.Write(false);
                }
                NetworkManager.SendDataTo(game.Player[i].AccountID, packet.toNetworkArray());
            }
        }
        #endregion
    }
}
