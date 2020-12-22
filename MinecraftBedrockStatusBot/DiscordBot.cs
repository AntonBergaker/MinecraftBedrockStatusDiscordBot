using System;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MinecraftBedrockStatus;

namespace MinecraftBedrockStatusBot {
    class DiscordBot {
        private readonly string token;
        private readonly string onlineStatus;
        private readonly string offlineStatus;
        private readonly DiscordSocketClient client;
        private readonly BedrockStatusClient bedrockStatusClient;

        public DiscordBot(string token, IPAddress ipAddress, int port, string onlineStatus, string offlineStatus) {
            this.token = token;
            this.onlineStatus = onlineStatus;
            this.offlineStatus = offlineStatus;
            client = new DiscordSocketClient();
            client.Ready += ReadyAsync;
            bedrockStatusClient = new BedrockStatusClient(ipAddress, port);
        }

        /// <summary>
        /// Starts the bot and logs it into Discord
        /// </summary>
        /// <returns></returns>
        public async Task BeginAsync() {
            var loginTask = client.LoginAsync(TokenType.Bot, token);

            await loginTask;
            await client.StartAsync();
        }

        /// <summary>
        /// Runs forever polling the server status every 10 seconds and updating the Discord Activity
        /// </summary>
        /// <returns>no</returns>
        private string lastStatus = "";
        private async Task PollServer() {
            while (true) {
                string newStatus;
                try {
                    var result = await bedrockStatusClient.GetStatusAsync();
                    newStatus = onlineStatus
                        .Replace("$ServerName$", result.Name)
                        .Replace("$Version$", result.Version)
                        .Replace("$PlayerCount$", result.PlayerCount.ToString())
                        .Replace("$MaxPlayerCount$", result.MaxPlayerCount.ToString())
                        .Replace("$GameMode$", result.GameType ?? "Undefined");
                }
                catch (TimeoutException) {
                    newStatus = offlineStatus;
                }

                if (lastStatus != newStatus) {
                    lastStatus = newStatus;
                    await client.SetActivityAsync(new Game(newStatus));
                }

                await Task.Delay(10000);
            }
        }


        private Task ReadyAsync() {
            Console.WriteLine($"{client.CurrentUser} is connected!");
            var _ = PollServer();
            return Task.CompletedTask;
        }
    }
}
