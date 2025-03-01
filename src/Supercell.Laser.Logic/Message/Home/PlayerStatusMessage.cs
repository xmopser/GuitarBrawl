﻿namespace Supercell.Laser.Logic.Message.Home
{
    using Supercell.Laser.Logic.Message.Home;
    public class PlayerStatusMessage : GameMessage
    {
        public int Status;

        public PlayerStatusMessage() : base()
        {

        }

        public override void Decode()
        {
            Status = Stream.ReadVInt();
            LobbyInfoMessage message = new LobbyInfoMessage();
        }

        public override int GetMessageType()
        {
            return 14366;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }
    }
}
