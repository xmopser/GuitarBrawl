﻿using System.Linq;

namespace Supercell.Laser.Logic.Home
{
    using Newtonsoft.Json;
    using Supercell.Laser.Logic.Command.Home;
    using Supercell.Laser.Logic.Data;
    using Supercell.Laser.Logic.Data.Helper;
    using Supercell.Laser.Logic.Helper;
    using Supercell.Laser.Logic.Home.Gatcha;
    using Supercell.Laser.Logic.Home.Items;
    using Supercell.Laser.Logic.Home.Quest;
    using Supercell.Laser.Logic.Home.Structures;
    using Supercell.Laser.Logic.Message.Home;
    using Supercell.Laser.Titan.DataStream;


    [JsonObject(MemberSerialization.OptIn)]
    public class ClientHome
    {
        public const int DAILYOFFERS_COUNT = 6;

        public static readonly int[] GoldPacksPrice = new int[]
        {
            20, 50, 140, 280
        };

        public static readonly int[] GoldPacksAmount = new int[]
        {
            150, 400, 1200, 2600
        };

        [JsonProperty] public long HomeId;
        [JsonProperty] public int ThumbnailId;
        [JsonProperty] public int CharacterId;
        [JsonProperty] public int SkinId;

        [JsonProperty] public List<OfferBundle> OfferBundles;

        [JsonProperty] public int TrophiesReward;
        [JsonProperty] public int TokenReward;
        [JsonProperty] public int StarTokenReward;

        [JsonProperty] public int BrawlPassProgress;
        [JsonProperty] public int PremiumPassProgress;
        [JsonProperty] public int BrawlPassTokens;
        [JsonProperty] public bool HasPremiumPass;
        [JsonProperty] public List<int> UnlockedEmotes;
        [JsonProperty] public List<int> UnlockedSkins;

        [JsonProperty] public int TrophyRoadProgress;
        [JsonProperty] public int Namecolor;
        [JsonProperty] public int Exp;
        [JsonProperty] public Quests Quests;
        [JsonProperty] public List<string> OffersClaimed;
        [JsonProperty] public string Day;

        [JsonIgnore] public EventData[] Events;

        public PlayerThumbnailData Thumbnail => DataTables.Get(DataType.PlayerThumbnail).GetDataByGlobalId<PlayerThumbnailData>(ThumbnailId);
        public CharacterData Character => DataTables.Get(DataType.Character).GetDataByGlobalId<CharacterData>(CharacterId);

        public HomeMode HomeMode;

        [JsonProperty] public DateTime LastVisitHomeTime;

        public ClientHome()
        {
            ThumbnailId = GlobalId.CreateGlobalId(28, 0);
            CharacterId = GlobalId.CreateGlobalId(16, 0);

            OfferBundles = new List<OfferBundle>();
            LastVisitHomeTime = DateTime.UnixEpoch;
            OffersClaimed = new List<string>();

            TrophyRoadProgress = 1;

            BrawlPassProgress = 1;
            PremiumPassProgress = 1;

            UnlockedEmotes = new List<int>();
            UnlockedSkins = new List<int>();
        }

        public int TimerMath(DateTime timer_start, DateTime timer_end)
        {
            {
                DateTime timer_now = DateTime.Now;
                if (timer_now > timer_start)
                {
                    if (timer_now < timer_end)
                    {
                        int time_sec = (int)(timer_end - timer_now).TotalSeconds;
                        return time_sec;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }
            }
        }

        public void HomeVisited()
        {
            RotateShopContent(DateTime.UtcNow, OfferBundles.Count == 0);
            LastVisitHomeTime = DateTime.UtcNow;
            UpdateOfferBundles();

            if (Quests == null && TrophyRoadProgress >= 11)
            {
                Quests = new Quests();
                Quests.AddRandomQuests(HomeMode.Avatar.Heroes, 8);
            }
            else if (Quests != null)
            {
                if (Quests.QuestList.Count < 8) // New quests adds at 07:00 AM UTC
                {
                    Quests.AddRandomQuests(HomeMode.Avatar.Heroes, 8 - Quests.QuestList.Count);
                }
            }
        }

        public void Tick()
        {
            LastVisitHomeTime = DateTime.UtcNow;
            TokenReward = 0;
            TrophiesReward = 0;
            StarTokenReward = 0;
        }

        public void PurchaseOffer(int index, int brawler_reference)
        {
            if (index < 0 || index >= OfferBundles.Count) return;

            OfferBundle bundle = OfferBundles[index];
            if (bundle.Purchased) return;

            if (bundle.Currency == 0)
            {
                if (!HomeMode.Avatar.UseDiamonds(bundle.Cost)) return;
            }
            else if (bundle.Currency == 1)
            {
                if (!HomeMode.Avatar.UseGold(bundle.Cost)) return;
            }

            if (bundle.Claim == "debug")
            {
                ;
            }
            else
            {
                OffersClaimed.Add(bundle.Claim);
            }

            bundle.Purchased = true;

            LogicGiveDeliveryItemsCommand command = new LogicGiveDeliveryItemsCommand();
            Random rand = new Random();

            foreach (Offer offer in bundle.Items)
            {
                if (offer.Type == ShopItem.BrawlBox || offer.Type == ShopItem.FreeBox)
                {
                    for (int x = 0; x < offer.Count; x++)
                    {
                        DeliveryUnit unit = new DeliveryUnit(100);
                        HomeMode.SimulateGatcha(unit);
                        if (x + 1 != offer.Count)
                        {
                            command.Execute(HomeMode);
                        }
                        command.DeliveryUnits.Add(unit);
                    }
                }
                else if (offer.Type == ShopItem.HeroPower)
                {
                    DeliveryUnit unit = new DeliveryUnit(100);
                    GatchaDrop reward = new GatchaDrop(6);
                    reward.DataGlobalId = offer.ItemDataId;
                    reward.Count = offer.Count;
                    unit.AddDrop(reward);
                    command.DeliveryUnits.Add(unit);
                }
                else if (offer.Type == ShopItem.Gems)
                {
                    DeliveryUnit unit = new DeliveryUnit(100);
                    GatchaDrop reward = new GatchaDrop(8);
                    reward.Count = offer.Count;
                    unit.AddDrop(reward);
                    command.DeliveryUnits.Add(unit);
                }
                else if (offer.Type == ShopItem.Ticket)
                {
                    DeliveryUnit unit = new DeliveryUnit(100);
                    GatchaDrop reward = new GatchaDrop(3);
                    reward.Count = offer.Count;
                    unit.AddDrop(reward);
                    command.DeliveryUnits.Add(unit);
                }
                else if (offer.Type == ShopItem.WildcardPower)
                {
                    DeliveryUnit unit = new DeliveryUnit(100);
                    GatchaDrop reward = new GatchaDrop(6);
                    reward.DataGlobalId = brawler_reference;
                    reward.Count = offer.Count;
                    unit.AddDrop(reward);
                    command.DeliveryUnits.Add(unit);
                }
                else if (offer.Type == ShopItem.Coin)
                {
                    DeliveryUnit unit = new DeliveryUnit(100);
                    GatchaDrop reward = new GatchaDrop(7);
                    reward.Count = offer.Count;
                    unit.AddDrop(reward);
                    command.DeliveryUnits.Add(unit);
                }
                else if (offer.Type == ShopItem.Skin)
                {
                    DeliveryUnit unit = new DeliveryUnit(100);
                    GatchaDrop reward = new GatchaDrop(9);
                    reward.Count = offer.Count;
                    reward.DataGlobalId = offer.ItemDataId;
                    reward.PinGlobalId = offer.SkinDataId;
                    unit.AddDrop(reward);
                    command.DeliveryUnits.Add(unit);
                }
                else if (offer.Type == ShopItem.BigBox)
                {
                    for (int x = 0; x < offer.Count; x++)
                    {
                        DeliveryUnit unit = new DeliveryUnit(12);
                        HomeMode.SimulateGatcha(unit);
                        if (x + 1 != offer.Count)
                        {
                            command.Execute(HomeMode);
                        }
                        command.DeliveryUnits.Add(unit);
                    }
                }
                else if (offer.Type == ShopItem.MegaBox)
                {
                    for (int x = 0; x < offer.Count; x++)
                    {
                        DeliveryUnit unit = new DeliveryUnit(11);
                        HomeMode.SimulateGatcha(unit);
                        if (x + 1 != offer.Count)
                        {
                            command.Execute(HomeMode);
                        }
                        command.DeliveryUnits.Add(unit);
                    }
                }
                else if (offer.Type == ShopItem.GuaranteedHero)
                {
                    DeliveryUnit unit = new DeliveryUnit(100);
                    GatchaDrop reward = new GatchaDrop(1);
                    reward.DataGlobalId = offer.ItemDataId;
                    reward.Count = 1;
                    unit.AddDrop(reward);
                    command.DeliveryUnits.Add(unit);
                }
                else
                {
                    // todo...
                }
                UpdateOfferBundles();
            }
            command.Execute(HomeMode);
            AvailableServerCommandMessage message = new AvailableServerCommandMessage();
            message.Command = command;
            HomeMode.GameListener.SendMessage(message);
        }

        private void RotateShopContent(DateTime time, bool isNewAcc)
        {
            if (OfferBundles.Select(bundle => bundle.IsDailyDeals).ToArray().Length > 6)
            {
                OfferBundles.RemoveAll(bundle => bundle.IsDailyDeals);
            }
            OfferBundles.RemoveAll(offer => offer.EndTime <= time);

            if (isNewAcc || DateTime.UtcNow >= DateTime.UtcNow.Date.AddHours(8)) // Daily deals refresh at 08:00 AM UTC
            {
                if (LastVisitHomeTime < DateTime.UtcNow.Date.AddHours(8))
                {
                    UpdateDailyOfferBundles();
                }
            }
        }

        public void DevColorCmd(string color)
        {
            if (color == "12")
            {
                Namecolor = 12;
            }
            if (color == "13")
            {
                Namecolor = 13;
            }
            if (color == "14")
            {
                Namecolor = 14;
            }
        }

        private void UpdateDailyOfferBundles()
        {
            // OfferBundles.Add(GenerateDailyGift());
            // OfferBundles.Add(GenerateGift());
            // OfferBundles.Add(GenerateGift1());
            // OfferBundles.Add(GenerateGift2());

            // bool shouldPowerPoints = false;
            // for (int i = 1; i < DAILYOFFERS_COUNT; i++)
            // {
            //     OfferBundle dailyOffer = GenerateDailyOffer(shouldPowerPoints);
            //     if (dailyOffer != null)
            //     {
            //         if (!shouldPowerPoints) shouldPowerPoints = dailyOffer.Items[0].Type != ShopItem.HeroPower;
            //         OfferBundles.Add(dailyOffer);
            //     }
            // }
            ;
        }

        public void UpdateOfferBundles()
        {
            OfferBundles.RemoveAll(bundle => bundle.IsTrue);

            GenerateOffer(
                new DateTime(2000, 4, 20, 12, 0, 0), new DateTime(2030, 4, 20, 12, 0, 0),
                49, 999, 0, ShopItem.Gems, 
                0, 9999, 0, 
                "offer1", "АКЦИЯ С 1 ПРЕДМЕТОМ", "offer_gems"
            );

            GenerateOffer2(
                new DateTime(2000, 4, 20, 12, 0, 0), new DateTime(2030, 4, 20, 12, 0, 0),
                1, 999, 0, ShopItem.Gems, 
                1, 999, 0, ShopItem.Gems, 
                0, 9999, 0, 
                "offer2", "АКЦИЯ С 2 ПРЕДМЕТАМИ", "offer_gems"
            );

            GenerateOffer3(
                new DateTime(2000, 4, 20, 12, 0, 0), new DateTime(2030, 4, 20, 12, 0, 0),
                1, 999, 0, ShopItem.Coin, 
                1, 999, 0, ShopItem.Coin, 
                1, 999, 0, ShopItem.Coin, 
                19, 9999, 0, 
                "offer3", "АКЦИЯ С 3 ПРЕДМЕТАМИ", "offer_gems"
            );

            GenerateOffer(
                new DateTime(2000, 4, 20, 12, 0, 0), new DateTime(2030, 4, 20, 12, 0, 0),
                1, 9909, 0, ShopItem.Gems, 
                19, 9999, 0, 
                "debug", "БЕСКОНЕЧНАЯ АКЦИЯ", "offer_gems"
            );

            GenerateOffer(
                new DateTime(2000, 4, 20, 12, 0, 0), new DateTime(2030, 4, 20, 12, 0, 0),
                100, 9909, 0, ShopItem.WildcardPower, 
                19, 9999, 0, 
                "zeta", "БЕСКОНЕЧНАЯ АКЦИЯ", "offer_gems"
            );

            for (int l = 0; l < 37; l++){
                if (l == 33) continue;
                GenerateOffer(
                    new DateTime(2000, 4, 20, 12, 0, 0), new DateTime(2030, 4, 20, 12, 0, 0),
                    1, l, 0, ShopItem.GuaranteedHero, 
                    0, 9999, 0, 
                    "debug", "Персонажи!", "offer_gems"
                );
            }
        }

        public void GenerateOffer(
            DateTime OfferStart,
            DateTime OfferEnd,
            int Count,
            int BrawlerID,
            int Extra,
            ShopItem Item,
            int Cost,
            int OldCost,
            int Currency,
            string Claim,
            string Title,
            string BGR
            ){

            OfferBundle bundle = new OfferBundle();
            bundle.IsDailyDeals = false;
            bundle.IsTrue = true;
            bundle.EndTime = OfferEnd;
            bundle.Cost = Cost; 
            bundle.OldCost = OldCost; 
            bundle.Currency = Currency;
            bundle.Claim = Claim;
            bundle.Title = Title;
            bundle.BackgroundExportName = BGR;

            if (OffersClaimed.Contains(bundle.Claim))
            {
                bundle.Purchased = true;
            }
            if (TimerMath(OfferStart, OfferEnd) == -1)
            {
                bundle.Purchased = true;
            }
            if (HomeMode.HasHeroUnlocked(16000000 + BrawlerID))
            {
                bundle.Purchased = true;
            }

            Offer offer = new Offer(Item, Count, (16000000 + BrawlerID), Extra);
            bundle.Items.Add(offer);

            OfferBundles.Add(bundle);
        }

        public void GenerateOffer2(
            DateTime OfferStart,
            DateTime OfferEnd,
            int Count,
            int BrawlerID,
            int Extra,
            ShopItem Item,
            int Count2,
            int BrawlerID2,
            int Extra2,
            ShopItem Item2,
            int Cost,
            int OldCost,
            int Currency,
            string Claim,
            string Title,
            string BGR
            ){

            OfferBundle bundle = new OfferBundle();
            bundle.IsDailyDeals = false;
            bundle.IsTrue = true;
            bundle.EndTime = OfferEnd;
            bundle.Cost = Cost; 
            bundle.OldCost = OldCost; 
            bundle.Currency = Currency;
            bundle.Claim = Claim;
            bundle.Title = Title;
            bundle.BackgroundExportName = BGR;

            if (OffersClaimed.Contains(bundle.Claim))
            {
                bundle.Purchased = true;
            }
            if (TimerMath(OfferStart, OfferEnd) == -1)
            {
                bundle.Purchased = true;
            }
            if (HomeMode.HasHeroUnlocked(16000000 + BrawlerID))
            {
                bundle.Purchased = true;
            }

            Offer offer = new Offer(Item, Count, (16000000 + BrawlerID), Extra);
            bundle.Items.Add(offer);
            Offer offer2 = new Offer(Item2, Count2, (16000000 + BrawlerID2), Extra2);
            bundle.Items.Add(offer2);

            OfferBundles.Add(bundle);
        }

        public void GenerateOffer3(
            DateTime OfferStart,
            DateTime OfferEnd,
            int Count,
            int BrawlerID,
            int Extra,
            ShopItem Item,
            int Count2,
            int BrawlerID2,
            int Extra2,
            ShopItem Item2,
            int Count3,
            int BrawlerID3,
            int Extra3,
            ShopItem Item3,
            int Cost,
            int OldCost,
            int Currency,
            string Claim,
            string Title,
            string BGR
            ){

            OfferBundle bundle = new OfferBundle();
            bundle.IsDailyDeals = false;
            bundle.IsTrue = true;
            bundle.EndTime = OfferEnd;
            bundle.Cost = Cost; 
            bundle.OldCost = OldCost; 
            bundle.Currency = Currency;
            bundle.Claim = Claim;
            bundle.Title = Title;
            bundle.BackgroundExportName = BGR;

            if (OffersClaimed.Contains(bundle.Claim))
            {
                bundle.Purchased = true;
            }
            if (TimerMath(OfferStart, OfferEnd) == -1)
            {
                bundle.Purchased = true;
            }
            if (HomeMode.HasHeroUnlocked(16000000 + BrawlerID))
            {
                bundle.Purchased = true;
            }

            Offer offer = new Offer(Item, Count, (16000000 + BrawlerID), Extra);
            bundle.Items.Add(offer);
            Offer offer2 = new Offer(Item2, Count2, (16000000 + BrawlerID2), Extra2);
            bundle.Items.Add(offer2);
            Offer offer3 = new Offer(Item3, Count3, (16000000 + BrawlerID3), Extra3);
            bundle.Items.Add(offer3);

            OfferBundles.Add(bundle);
        }

        private OfferBundle GenerateDailyGift()
        {
            OfferBundle bundle = new OfferBundle();
            bundle.IsDailyDeals = true;
            bundle.EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(8); // tomorrow at 8:00 utc (11:00 MSK)
            bundle.Cost = 0;

            Offer offer = new Offer(ShopItem.FreeBox, 1);
            bundle.Items.Add(offer);

            return bundle;
        }

        private OfferBundle GenerateGift()
        {
            OfferBundle bundle = new OfferBundle();
            bundle.IsDailyDeals = false;
            bundle.EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(8); // tomorrow at 8:00 utc (11:00 MSK)
            bundle.Cost = 0;
            bundle.Title = "Thanks for 600 subscribers";

            Offer offer = new Offer(ShopItem.Gems, 80);
            bundle.Items.Add(offer);

            return bundle;
        }

        private OfferBundle GenerateGift1()
        {
            OfferBundle bundle = new OfferBundle();
            bundle.IsDailyDeals = false;
            bundle.EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(8); // tomorrow at 8:00 utc (11:00 MSK)
            bundle.Cost = 0;
            bundle.Title = "<cff3200>G<cff6500>i<cff9800>f<cffcb00>t<cffff00> <cccff00>D<c99ff00>a<c66ff00>y<c33ff00> <c01ff00>2</c>";

            Offer offer = new Offer(ShopItem.Skin, 1, 29000000, 52);
            bundle.Items.Add(offer);

            return bundle;
        }

        private OfferBundle GenerateGift2()
        {
            OfferBundle bundle = new OfferBundle();
            bundle.IsDailyDeals = false;
            bundle.EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(8); // tomorrow at 8:00 utc (11:00 MSK)
            bundle.Cost = 0;
            bundle.Title = "<cff3200>G<cff6500>i<cff9800>f<cffcb00>t<cffff00> <cccff00>D<c99ff00>a<c66ff00>y<c33ff00> <c01ff00>3</c>";

            Offer offer = new Offer(ShopItem.Gems, 30);
            bundle.Items.Add(offer);

            return bundle;
        }

        private OfferBundle GenerateDailyOffer(bool shouldPowerPoints)
        {
            OfferBundle bundle = new OfferBundle();
            bundle.IsDailyDeals = true;
            bundle.EndTime = DateTime.UtcNow.Date.AddDays(1).AddHours(8); // tomorrow at 8:00 utc (11:00 MSK)

            Random random = new Random();
            int type = shouldPowerPoints ? 0 : random.Next(0, 2); // getting a type

            switch (type)
            {
                case 0: // Power points
                    List<Hero> unlockedHeroes = HomeMode.Avatar.Heroes;
                    bool heroValid = false;
                    int generateAttempts = 0;
                    int index = -1;
                    while (!heroValid && generateAttempts < 10)
                    {
                        generateAttempts++;
                        index = random.Next(unlockedHeroes.Count);
                        heroValid = unlockedHeroes[index].PowerPoints < 2300 + 1440;
                        if (heroValid)
                        {
                            foreach (OfferBundle b in OfferBundles)
                            {
                                if (!b.IsDailyDeals) continue;

                                if (b.Items.Count > 0)
                                {
                                    if (b.Items[0].Type == ShopItem.HeroPower)
                                    {
                                        if (b.Items[0].ItemDataId == unlockedHeroes[index].CharacterId)
                                        {
                                            heroValid = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!heroValid) return null;

                    int count = random.Next(15, 100) + 1;
                    Offer offer = new Offer(ShopItem.HeroPower, count, unlockedHeroes[index].CharacterId);

                    bundle.Items.Add(offer);
                    bundle.Cost = count * 2;
                    bundle.Currency = 1;

                    break;
                case 1: // mega box
                    Offer megaBoxOffer = new Offer(ShopItem.MegaBox, 1);
                    bundle.Items.Add(megaBoxOffer);
                    bundle.Cost = 40;
                    bundle.OldCost = 80;
                    bundle.Currency = 0;
                    break;
            }

            return bundle;
        }

        public void Encode(ByteStream encoder)
        {
            DateTime utcNow = DateTime.UtcNow;

            encoder.WriteVInt(utcNow.Year * 1000 + utcNow.DayOfYear); // 0x78d4b8
            encoder.WriteVInt(utcNow.Hour * 3600 + utcNow.Minute * 60 + utcNow.Second); // 0x78d4cc

            encoder.WriteVInt(HomeMode.Avatar.Trophies); // 0x78d4e0
            encoder.WriteVInt(HomeMode.Avatar.Trophies); // 0x78d4f4

            encoder.WriteVInt(HomeMode.Avatar.HighestTrophies); // highest trophy again?

            encoder.WriteVInt(TrophyRoadProgress);

            encoder.WriteVInt(Exp + 100); // experience

            ByteStreamHelper.WriteDataReference(encoder, Thumbnail);

            // Name colors not implemented since I used game patch to allow color codes in names and everywhere.
            encoder.WriteVInt(43);
            encoder.WriteVInt(Namecolor);

            encoder.WriteVInt(18); // Played game modes
            for (int i = 0; i < 18; i++)
            {
                encoder.WriteVInt(i);
            }

            encoder.WriteVInt(0); // Selected Skins Dictionary
            // {
            //     encoder.WriteVInt(29);
            //     encoder.WriteVInt(SkinId);
            // }

            encoder.WriteVInt(UnlockedSkins.Count);
            for (int i = 0; i < UnlockedSkins.Count; i++)
            {
                encoder.WriteVInt(29);
                encoder.WriteVInt(UnlockedSkins[i]);
            }

            encoder.WriteVInt(0);

            encoder.WriteVInt(0);
            encoder.WriteVInt(HomeMode.Avatar.HighestTrophies); // Highest Trophies Reachd Icon
            encoder.WriteVInt(0);
            encoder.WriteVInt(1);

            encoder.WriteBoolean(true);
            encoder.WriteVInt(1);
            encoder.WriteVInt(0); // token doubler
            encoder.WriteVInt(0);
            encoder.WriteVInt(0);

            encoder.WriteVInt(0);
            encoder.WriteVInt(0);
            encoder.WriteVInt(0);

            encoder.WriteBoolean(false);
            encoder.WriteBoolean(false);
            encoder.WriteBoolean(false);
            encoder.WriteVInt(2);
            encoder.WriteVInt(2);
            encoder.WriteVInt(2);
            encoder.WriteVInt(0);
            encoder.WriteVInt(0);

            encoder.WriteVInt(OfferBundles.Count); // Shop offers at 0x78e0c4
            foreach (OfferBundle offerBundle in OfferBundles)
            {
                offerBundle.Encode(encoder);
            }
            
            encoder.WriteVInt(0);

            encoder.WriteVInt(200); // Battle Tokens
            encoder.WriteVInt(0); // 0x78e23c
            encoder.WriteVInt(0); // 0x78e250
            encoder.WriteVInt(0); // 0x78e3a4
            encoder.WriteVInt(0); // 0x78e3a4

            ByteStreamHelper.WriteDataReference(encoder, Character);

            encoder.WriteString("RU"); // Z
            encoder.WriteString("GuitarBrawl"); // V

            encoder.WriteVInt(2);
            {
                encoder.WriteInt(3);
                encoder.WriteInt(TokenReward); // tokens

                encoder.WriteInt(4);
                encoder.WriteInt(TrophiesReward); // trophies
            }
            TokenReward = 0;
            TrophiesReward = 0; // ну а где ещё. хотя....
            StarTokenReward = 0;

            encoder.WriteVInt(0); // array

            encoder.WriteVInt(1); // BrawlPassSeasonData
            {
                encoder.WriteVInt(0);
                encoder.WriteVInt(BrawlPassTokens);
                encoder.WriteVInt(PremiumPassProgress);
                encoder.WriteVInt(BrawlPassProgress);
                encoder.WriteBoolean(HasPremiumPass);
                encoder.WriteVInt(0);
            }

            encoder.WriteVInt(0);

            if (Quests != null)
            {
                encoder.WriteBoolean(true);
                Quests.Encode(encoder);
            }
            else
            {
                encoder.WriteBoolean(true);
                encoder.WriteVInt(0);
            }

            encoder.WriteBoolean(true);
            encoder.WriteVInt(UnlockedEmotes.Count);
            for (int i = 0; i < UnlockedEmotes.Count; i++)
            {
                ByteStreamHelper.WriteDataReference(encoder, UnlockedEmotes[i]);
                encoder.WriteVInt(0);
            }

            encoder.WriteVInt(utcNow.Year * 1000 + utcNow.DayOfYear);
            encoder.WriteVInt(100);
            encoder.WriteVInt(10);
            encoder.WriteVInt(30);
            encoder.WriteVInt(3);
            encoder.WriteVInt(80);
            encoder.WriteVInt(10);
            encoder.WriteVInt(40);
            encoder.WriteVInt(1000);
            encoder.WriteVInt(550);
            encoder.WriteVInt(0);
            encoder.WriteVInt(999900);

            encoder.WriteVInt(0); // Array
            
            encoder.WriteVInt(7);
            for (int i = 1; i <= 7; i++)
                encoder.WriteVInt(i);

            encoder.WriteVInt(Events.Length);
            foreach (EventData data in Events)
            {
                data.Encode(encoder);
            }

            encoder.WriteVInt(0);

            encoder.WriteVInt(8);
            {
                encoder.WriteVInt(20);
                encoder.WriteVInt(35);
                encoder.WriteVInt(75);
                encoder.WriteVInt(140);
                encoder.WriteVInt(290);
                encoder.WriteVInt(480);
                encoder.WriteVInt(800);
                encoder.WriteVInt(1250);
            }

            encoder.WriteVInt(8);
            {
                encoder.WriteVInt(1);
                encoder.WriteVInt(2);
                encoder.WriteVInt(3);
                encoder.WriteVInt(4);
                encoder.WriteVInt(5);
                encoder.WriteVInt(10);
                encoder.WriteVInt(15);
                encoder.WriteVInt(20);
            }

            encoder.WriteVInt(3);
            {
                encoder.WriteVInt(10);
                encoder.WriteVInt(30);
                encoder.WriteVInt(80);
            }

            encoder.WriteVInt(3);
            {
                encoder.WriteVInt(6);
                encoder.WriteVInt(20);
                encoder.WriteVInt(60);
            }

            ByteStreamHelper.WriteIntList(encoder, GoldPacksPrice);
            ByteStreamHelper.WriteIntList(encoder, GoldPacksAmount);

            encoder.WriteVInt(2);
            encoder.WriteVInt(200);
            encoder.WriteVInt(20);

            encoder.WriteVInt(8640);
            encoder.WriteVInt(10);
            encoder.WriteVInt(5);

            encoder.WriteBoolean(false);
            encoder.WriteBoolean(false);
            encoder.WriteBoolean(false);

            encoder.WriteVInt(50);
            encoder.WriteVInt(604800);

            encoder.WriteBoolean(true);

            encoder.WriteVInt(1); // Array
            {
                encoder.WriteVInt(16);
                encoder.WriteVInt(8);

                encoder.WriteInt(0);
                encoder.WriteInt(0);
            }

            encoder.WriteVInt(2); // IntValueEntries
            {
                encoder.WriteInt(1);
                encoder.WriteInt(41000012); // theme

                encoder.WriteInt(46);
                encoder.WriteInt(1);
            }

            encoder.WriteVInt(0);

            encoder.WriteLong(HomeId);
            encoder.WriteVInt(0);
            encoder.WriteVInt(0);
            encoder.WriteBoolean(false);
            encoder.WriteVInt(0);
            encoder.WriteVInt(0);
        }
    }
}
