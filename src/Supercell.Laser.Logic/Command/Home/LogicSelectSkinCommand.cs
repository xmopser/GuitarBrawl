namespace Supercell.Laser.Logic.Command.Home
{
    using Supercell.Laser.Logic.Home;
    using Supercell.Laser.Titan.DataStream;

    public class LogicSelectSkinCommand : Command
    {
        public int CharacterId;
        public int SkinId;
        public override void Decode(ByteStream stream)
        {
            base.Decode(stream);
            SkinId = stream.ReadVInt();
            CharacterId = stream.ReadVInt();
        }

        public override int Execute(HomeMode homeMode)
        {
            int globalId = GlobalId.CreateGlobalId(16, CharacterId);
            if (homeMode.Avatar.HasHero(globalId))
            {
                homeMode.Home.CharacterId = globalId;
                homeMode.Home.SkinId = SkinId;
                homeMode.CharacterChanged.Invoke(globalId);
                return 0;
            }
            return 0;
        }

        public override int GetCommandType()
        {
            return 506;
        }
    }
}
