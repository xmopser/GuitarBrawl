namespace Supercell.Laser.Logic.Message.Club
{ // t.me/xmopser_mops
    public class KickAllianceMemberMessage : GameMessage
    {
        public long AccountId;

        public override void Decode()
        {
            AccountId = Stream.ReadLong();
        }

        public override int GetMessageType()
        {
            return 14307;
        }

        public override int GetServiceNodeType()
        {
            return 11;
        }
    }
}
