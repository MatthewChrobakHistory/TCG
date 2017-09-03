using LuaInterface;
using System;

namespace TCGServer.Scripting.Lua
{
    public class Engine : IScriptSystem
    {
        public LuaInterface.Lua Lua = null;

        public Engine() {
            Lua = new LuaInterface.Lua();
            Lua.NewTable("Client");
            Lua.RegisterFunction("Client.ChangeState", this, this.GetType().GetMethod("ChangeClientState"));
            Lua.NewTable("NS");
            Lua.NewTable("NS.Minor");

            Lua.RegisterFunction("NS.Run", this, this.GetType().GetMethod("MethodName"));
            Lua.RegisterFunction("Print", this, this.GetType().GetMethod("Print"));
            Lua.RegisterFunction("print", this, this.GetType().GetMethod("Print"));

        }

        public void Run(string input) {
            try {
                Lua.DoString(input);
            }
            catch (Exception e) {
                Program.Write(e.ToString());
            }
        }

        public void Print(string input) {
            Program.Write("[LUA] " + input);
        }

        public void MethodName(string argument) {

        }

        public string MethodsCanAlsoReturn(string input) {
            return "";
        }

        public void ChangeClientState(int index, byte state) {
            Networking.NetworkManager.PacketManager.SendChangeClientState(index, (ClientState)state);
        }
    }
}
