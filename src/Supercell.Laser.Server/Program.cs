namespace Supercell.Laser.Server
{
    using Supercell.Laser.Server.Handler;
    using Supercell.Laser.Server.Settings;
    using System.Drawing;

    static class Program
    {
        public const string SERVER_VERSION = "1.0";
        public const string BUILD_TYPE = "Beta";

        private static void Main(string[] args)
        {
            Console.Title = "GuitarBrawl - server emulator";
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            Colorful.Console.WriteWithGradient(
                @"
    ______      _ __             ____                      __
   / ______  __(_/ /_____ ______/ __ )_________ __      __/ /
  / / __/ / / / / __/ __ `/ ___/ __  / ___/ __ `| | /| / / / 
 / /_/ / /_/ / / /_/ /_/ / /  / /_/ / /  / /_/ /| |/ |/ / /  
 \____/\__,_/_/\__/\__,_/_/  /_____/_/   \__,_/ |__/|__/_/      by XMopsER  -  tg: @xmopser_mops" + "\n\n\n", Color.Yellow, Color.Blue, 8);

            Logger.Print("Based on RoyaleBrawl. Github: https://github.com/xmopser/GuitarBrawl");
            Logger.Print("GuitarBrawl now strating...");

            Logger.Init();
            Configuration.Instance = Configuration.LoadFromFile("config.json");

            Resources.InitDatabase();
            Resources.InitLogic();
            Resources.InitNetwork();

            Logger.Print("Server started! Let's play Brawl Stars!");

            ExitHandler.Init();
            CmdHandler.Start();
        }
    }
}

