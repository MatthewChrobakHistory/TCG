namespace TCGServer.Networking
{
    public enum Packets
    {
        // Outgoing packets
        SendServerMsg,
        SendChangeClientState,
        SendGameData,
        SendEnemyField,
        SendEnemyHeld,
        SendMyField,
        SendMyHeld,
        SendUpdateTurn,
        Length
    }
}
