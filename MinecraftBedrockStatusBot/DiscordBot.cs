using System;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MinecraftBedrockStatus;

namespace MinecraftBedrockStatusBot {
    class DiscordBot {
        private readonly string token;
        private readonly DiscordSocketClient client;
        private readonly BedrockStatusClient bedrockStatusClient;

        public DiscordBot(string token, IPAddress ipAddress, int port) {
            this.token = token;
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
                    newStatus = $"{result.Name} - {result.PlayerCount}/{result.MaxPlayerCount}";
                }
                catch (TimeoutException) {
                    newStatus = "Server Offline";
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
