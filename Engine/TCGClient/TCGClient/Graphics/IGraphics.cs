namespace TCGClient.Graphics
{
    interface IGraphics
    {
        void Initialize();
        void Destroy();
        void Draw();
        void ServerMessage(string message);
        void EnableTurn();
    }
}
