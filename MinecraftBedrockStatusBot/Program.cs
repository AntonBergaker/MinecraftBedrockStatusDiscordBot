using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MinecraftBedrockStatusBot {
    class Program {
        static async Task Main() {
            string token;
            string address;
            string onlineStatus;
            string offlineStatus;
            int port;

            try {
                JObject obj = JObject.Parse(await File.ReadAllTextAsync("settings.json"));
                token = (string)obj["token"];
                address = (string)obj["server_address"];
                onlineStatus = (string)obj["online_message"];
                offlineStatus = (string)obj["offline_message"];
                port = (int) obj["server_port"];

                if (token == "" || address == "") {
                    Console.WriteLine("Please specify the bot token and server address inside settings.json");
                    return;
                }
            }
            catch {
                Console.WriteLine("There was an error reading from settings.json");
                return;
            }

            IPAddress ipAddress = (await Dns.GetHostAddressesAsync(address))[0];

            DiscordBot bot = new DiscordBot(token, ipAddress, port, onlineStatus, offlineStatus);
            await bot.BeginAsync();

            await Task.Delay(Timeout.Infinite);
        }
    }
}
