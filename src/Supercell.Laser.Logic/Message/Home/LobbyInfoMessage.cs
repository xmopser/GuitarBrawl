namespace Supercell.Laser.Logic.Message.Home
{
    //using Supercell.Laser.Server.Networking.Session;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class LobbyInfoMessage : GameMessage
    {
        public int PlayersOnline;
        public string Version;
        public int Ping;
        public string Pinged;
        public override void Encode()
        {
            if (Ping >= 0 && Ping <= 49)
            {
                Pinged = " ▂▄▅▆ " + "(" + Ping + "ms)"; 
            }
            if (Ping >= 50 && Ping <= 99)
            {
                Pinged = " ▂▄▅   " + "(" + Ping + "ms)"; 
            }
            if (Ping >= 100 && Ping <= 199)
            {
                Pinged = " ▂▄   " + "(" + Ping + "ms)"; 
            }
            if (Ping >= 200 && Ping <= 299)
            {
                Pinged = " ▂    " + "(" + Ping + "ms)"; 
            }
            if (Ping >= 300)
            {
                Pinged = " ▁    " + "(" + Ping + "ms)"; 
            }
            Stream.WriteVInt(PlayersOnline);
            Stream.WriteString("AlahBrawl" + "\nt.me/alahservers" + "\nPing: " + Pinged);

            Stream.WriteVInt(0); // Event count
        }
        public override int GetMessageType()
        {
            return 23457;
        }

        public override int GetServiceNodeType()
        {
            return 9;
        }
    }
}
