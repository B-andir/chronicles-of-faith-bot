using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Discord;
using Discord.WebSocket;
using System.IO;
using System.Collections.Generic;

namespace Chronicles_of_Fate_CS_Bot {

    public class Settings {
        public bool readConsentSheets, trackUsersJoining, trackUsersLeaving;
        public ulong consentSheetsSourceChannelId, consentSheetsTargetChannelId, welcomeMessageChannelId, leftMessageChannelId;
    }

    class Program {
        private readonly DiscordSocketClient _client;

        public static Settings _settings;

        static string settingsFilePath = @"E:\Chronicles-Of-Faith-Data\settings.txt";

        static void Main(string[] args) {
            _settings = new Settings();

            List<string> lines = File.ReadAllLines(settingsFilePath).ToList();

            List<string> settingsTCS = lines[0].Split(",").ToList();
            List<string> settingsTUJ = lines[1].Split(",").ToList();
            List<string> settingsTUL = lines[2].Split(",").ToList();

            //Setting setup
            _settings.readConsentSheets = (settingsTCS[0] == "1");
            _settings.consentSheetsSourceChannelId = ulong.Parse(settingsTCS[1]);
            _settings.consentSheetsTargetChannelId = ulong.Parse(settingsTCS[2]);

            _settings.trackUsersJoining = (settingsTUJ[0] == "1");
            _settings.welcomeMessageChannelId = ulong.Parse(settingsTUJ[1]);

            _settings.trackUsersLeaving = (settingsTUL[0] == "1");
            _settings.leftMessageChannelId = ulong.Parse(settingsTUL[1]);

            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public Program() {
            _client = new DiscordSocketClient();

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;

            _client.UserJoined += UserJoinedAsync;
            _client.UserLeft += UserLeftAsync;
            _client.MessageReceived += MessageRecievedAsync;
        }

        public async Task MainAsync() {
            await _client.LoginAsync(TokenType.Bot, "");  // Fill empty "" with discord bot token
            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log) {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync() {
            Console.WriteLine($"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        // Track users joining
        private async Task UserJoinedAsync(SocketGuildUser user) {
            if (_settings.trackUsersJoining) {
                await user.Guild.GetTextChannel(_settings.welcomeMessageChannelId)
                    .SendMessageAsync($"Hey {user.Mention}! Welcome to Chronicles of Fate!\nGo ahead and get started by reading the Server Rules and following the steps needed to unlock the Discord Server.");

            }
        }

        // Track users leaving
        private async Task UserLeftAsync(SocketGuildUser user) {
            if (_settings.trackUsersLeaving) {
                await user.Guild.GetTextChannel(_settings.leftMessageChannelId)
                    .SendMessageAsync($"{user.Mention} just left the server!");

            }

        }

        // Read Messages
        private async Task MessageRecievedAsync(SocketMessage message) {
            if (message.Author.Id == _client.CurrentUser.Id || message.Author.IsBot) return;

            // Message is a command
            if (message.Content[0] == '!') {

                string[] args = message.Content.ToLower().Split(" ");

                // Admin level commands
                if ((message.Author as SocketGuildUser).GuildPermissions.BanMembers) {

                    // Ping
                    if (args[0] == "!ping") {
                        await message.Channel.SendMessageAsync("pong!");
                    }

                    // Track consent sheets in source channel to [argument[1]] #channel
                    if (args[0] == "!tcs" && args.Length > 1) {
                        _settings.readConsentSheets = true;
                        _settings.consentSheetsSourceChannelId = message.Channel.Id;
                        _settings.consentSheetsTargetChannelId = ulong.Parse(args[1].Substring(2, (args[1].Length - 3)));

                        List<string> lines = File.ReadAllLines(settingsFilePath).ToList();

                        List<string> settings = lines[0].Split(",").ToList();

                        settings[0] = "1";
                        settings[1] = message.Channel.Id.ToString();
                        settings[2] = ulong.Parse(args[1].Substring(2, (args[1].Length - 3))).ToString();

                        string newSetting = settings[0] + "," + settings[1] + "," + settings[2];

                        lines[0] = newSetting;

                        File.WriteAllLines(settingsFilePath, lines);

                        await message.DeleteAsync();
                    }

                    // Track users joining and post message in source channel
                    else if (args[0] == "!tuj") {
                        _settings.trackUsersJoining = true;
                        _settings.welcomeMessageChannelId = message.Channel.Id;

                        List<string> lines = File.ReadAllLines(settingsFilePath).ToList();

                        List<string> settings = lines[1].Split(",").ToList();

                        settings[0] = "1";
                        settings[1] = message.Channel.Id.ToString();

                        string newSetting = settings[0] + "," + settings[1];

                        lines[1] = newSetting;

                        File.WriteAllLines(settingsFilePath, lines);

                        await message.DeleteAsync();
                    }

                    // Track users leaving and post message in source channel
                    else if (args[0] == "!tul") {
                        _settings.trackUsersLeaving = true;
                        _settings.leftMessageChannelId = message.Channel.Id;

                        List<string> lines = File.ReadAllLines(settingsFilePath).ToList();

                        List<string> settings = lines[2].Split(",").ToList();

                        settings[0] = "1";
                        settings[1] = message.Channel.Id.ToString();

                        string newSetting = settings[0] + "," + settings[1];

                        lines[2] = newSetting;

                        File.WriteAllLines(settingsFilePath, lines);

                        await message.DeleteAsync();
                    } else if (args[0] == "!stop") {

                        // Stop tracking consent sheets
                        if (args[1] == "tcs") {
                            _settings.readConsentSheets = true;

                            List<string> lines = File.ReadAllLines(settingsFilePath).ToList();

                            List<string> settings = lines[0].Split(",").ToList();

                            settings[0] = "0";

                            string newSetting = settings[0] + "," + settings[1] + "," + settings[2];

                            lines[0] = newSetting;

                            File.WriteAllLines(settingsFilePath, lines);

                            await message.DeleteAsync();
                        }

                        // Stop tracking users joining
                        else if (args[1] == "tuj") {
                            _settings.trackUsersJoining = true;

                            List<string> lines = File.ReadAllLines(settingsFilePath).ToList();

                            List<string> settings = lines[1].Split(",").ToList();

                            settings[0] = "0";

                            string newSetting = settings[0] + "," + settings[1];

                            lines[0] = newSetting;

                            File.WriteAllLines(settingsFilePath, lines);

                            await message.DeleteAsync();
                        }

                        // Stop tracking users leaving
                        else if (args[1] == "tul") {
                            _settings.trackUsersLeaving = true;

                            List<string> lines = File.ReadAllLines(settingsFilePath).ToList();

                            List<string> settings = lines[2].Split(",").ToList();

                            settings[0] = "0";

                            string newSetting = settings[0] + "," + settings[1];

                            lines[0] = newSetting;

                            File.WriteAllLines(settingsFilePath, lines);

                            await message.DeleteAsync();
                        }
                    }
                }
            }

            // Consent sheet handling
            else if (_settings.readConsentSheets && message.Channel.Id == _settings.consentSheetsSourceChannelId) {
                await _client.GetGuild((message.Channel as SocketGuildChannel).Guild.Id)
                    .GetTextChannel(_settings.consentSheetsTargetChannelId)
                    .SendMessageAsync($"{message.Author.Mention}'s Consent Sheet:\n{message.Content}");

                await message.DeleteAsync();
            }
        }

    }
}
