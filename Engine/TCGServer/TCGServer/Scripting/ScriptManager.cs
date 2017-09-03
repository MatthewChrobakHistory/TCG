namespace TCGServer.Scripting
{
    public static class ScriptManager
    {
        private static IScriptSystem _engine;

        public static void Initialize() {
            _engine = new Lua.Engine();
        }

        public static void Run(string input, int index = -1) {
            if (index >= 0) {
                input = input.Replace("index", index.ToString());
            }
            _engine.Run(input);
        }
    }
}
