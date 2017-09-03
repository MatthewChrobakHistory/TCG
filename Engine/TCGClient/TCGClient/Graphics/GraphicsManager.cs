namespace TCGClient.Graphics
{
    public enum SurfaceType
    {
        Card,
        Length
    }

    public class GraphicsManager
    {
        // Surface paths
        public static string SurfacePath = Program.StartupPath + "data\\surfaces\\";
        public static string GuiPath = SurfacePath + "Gui\\";
        public static string CardsPath = SurfacePath + "Cards\\";

        private static IGraphics _graphics;

        public static void Initialize() {
            _graphics = new Sfml.Sfml();
            _graphics.Initialize();
        }

        public static void Destroy() {
            _graphics.Destroy();
        }

        public static void Draw() {
            _graphics.Draw();
        }

        public static void EnableTurn() {
            _graphics.EnableTurn();
        }

        public static void ServerMessage(string message) {
            _graphics.ServerMessage(message);
        }
    }
}
