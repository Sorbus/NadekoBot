﻿using Discord;
using System;
using System.IO;
using Newtonsoft.Json;
using Parse;
using Discord.Commands;
using NadekoBot.Modules;
using Discord.Modules;
using Discord.Legacy;
using Discord.Audio;

namespace NadekoBot
{
    class NadekoBot
    {
        public static DiscordClient client;
        public static StatsCollector stats_collector;
        public static string botMention;
        public static string GoogleAPIKey = null;
        public static ulong OwnerID;
        public static string password;

        static void Main()
        {
            //load credentials from credentials.json
            Credentials c;
            try
            {
                c = JsonConvert.DeserializeObject<Credentials>(File.ReadAllText("credentials.json"));
                botMention = c.BotMention;
                if (c.GoogleAPIKey == null || c.GoogleAPIKey == "") {
                    Console.WriteLine("No google api key found. You will not be able to use music and links won't be shortened.");
                }
                OwnerID = c.OwnerID;
                password = c.Password;
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to load stuff from credentials.json, RTFM");
                Console.ReadKey();
                return;
            }

            //create new discord client
            client = new DiscordClient();

            //create a command service
            var commandService = new CommandService(new CommandServiceConfig
            {
                CommandChar = null,
                HelpMode = HelpMode.Disable
            });

            //init parse
            if (c.ParseKey != null && c.ParseID != null && c.ParseID != "" && c.ParseKey != "") {
                ParseClient.Initialize(c.ParseID, c.ParseKey);

                //monitor commands for logging
                stats_collector = new StatsCollector(commandService);
            } else {
                Console.WriteLine("Parse key and/or ID not found. Bot will not log.");
            }


            //add command service
            var commands = client.Services.Add<CommandService>(commandService);
            
            //create module service
            var modules = client.Services.Add<ModuleService>(new ModuleService());

            //add audio service
            var audio = client.Services.Add<AudioService>(new AudioService(new AudioServiceConfig() {
                Channels = 2,
                EnableEncryption = false
            }));

            //install modules
            modules.Install(new Administration(), "Administration", FilterType.Unrestricted);
            modules.Install(new Conversations(), "Conversations", FilterType.Unrestricted);
            modules.Install(new Gambling(), "Gambling", FilterType.Unrestricted);
            modules.Install(new Games(), "Games", FilterType.Unrestricted);
            modules.Install(new Music(), "Music", FilterType.Unrestricted);
            modules.Install(new Searches(), "Searches", FilterType.Unrestricted);

            //run the bot
            client.Run(async () =>
            {
                await client.Connect(c.Username, c.Password);
                Console.WriteLine("Connected!");
            });
            Console.WriteLine("Exiting...");
            Console.ReadKey();
        }
    }
}