# GuitarBrawl
Private brawl stars server with PvP Battles based on RoyaleBrawl v27.269 ([by xeondev](https://ginhub.com/xeondev1337))

![Screenshot](https://github.com/xmopser/GuitarBrawl/blob/main/screenshots/Screenshot_2025-01-26-01-12-48-136_com.miokiru.pianobrawl.jpg)

## [DOWNLOAD CLIENT](https://drive.google.com/file/d/1pZ2p6oQYDy1TWe5joe6C296Wj1EufDe9/view?usp=sharing)

### GuitarBrawl Improvements:
- PvP Battles (3 Game modes working: Gem Grab, Showdown, Bounty)
- Online game rooms
- Friends & Alliances
- Brawl Pass & Trophy Road
- Leaderboards
- Events refresh
- Improved shop logic (purchase brawlers, power points and more...).
- Improvements for bots (random names, use ults, use of all characters).
- Possible to play on all characters (except Bea, Bibi, Rosa, Tick).
- New game mechanics on ohline battles (heal, correct destruction of walls).
- And more...

### Setup
##### Compiling
- Method 1: Open .sln file in Visual Studio and run project `Supercell.Laser.Server`
- Method 2: Compile `Supercell.Laser.Server.csproj` using `dotnet` & run it using `dotnet Supercell.Laser.Server.dll`
- Method 3: Use these commands:
Move to directory: `cd src/Supercell.Laser.Server`
Build: `dotnet build Supercell.Laser.Server.csproj`
Run: dotnet `bin/Debug/net6.0/Supercell.Laser.Server.dll`
##### Database
You need to setup mysql server and import `database.sql` file from this repository.
##### Configuration
- Set your ip in `config.json` -> `udp_host`
- Set your mysql database name, password, and username in `config.json`
##### Client
- Download apk here: [link](https://drive.google.com/file/d/1pZ2p6oQYDy1TWe5joe6C296Wj1EufDe9/view?usp=sharing)
- Open this file: `lib/armeabi-v7a/libreversed.script.so` and put your ip address and port in `HostPatcher`
- Compile apk and play enjoy playing this server.

### Requirements
- dotnet 6.0
- mysql (on windows I prefer *XAMPP* with *phpmyadmin*)
