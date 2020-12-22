using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftBedrockStatus {
    class BedrockStatusClient {
        private readonly int port;
        private readonly IPAddress address;
        private readonly UdpClient client;
        
        // We store the task we use to recieve data, because when TimeOut occurs it will actually still be listening.
        private Task<UdpReceiveResult>? storedResultTask;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">IP Address of the server</param>
        /// <param name="port">Port of the server</param>
        public BedrockStatusClient(IPAddress address, int port) {
            this.port = port;
            this.address = address;
            client = new UdpClient();
        }

        /// <summary>
        /// These bytes are used for identification and are added to some packets.
        /// </summary>
        private static readonly byte[] MagicBytes =
            {0x00, 0xff, 0xff, 0x00, 0xfe, 0xfe, 0xfe, 0xfe, 0xfd, 0xfd, 0xfd, 0xfd, 0x12, 0x34, 0x56, 0x78};

        /// <summary>
        /// Fetches the status from a Minecraft Bedrock server.
        /// </summary>
        /// <param name="timeout">timeout in milliseconds</param>
        /// <returns></returns>
        public async Task<BedrockServerStatus> GetStatusAsync(int timeout = 10000) {
            {
                byte[] data = new byte[25];
                using MemoryStream stream = new MemoryStream(data);
                using BinaryWriter writer = new BinaryWriter(stream);
                writer.Write((byte) 0x01);
                writer.Write(0L);
                writer.Write(MagicBytes);
                
                await client.SendAsync(data, data.Length, new IPEndPoint(address, port));
            }

            Task<UdpReceiveResult> currentResultTask;

            // If an uncompleted listening task still exists, use it instead of making a new one
            if (storedResultTask == null || storedResultTask.IsCompleted) {
                currentResultTask = client.ReceiveAsync();
            }
            else {
                currentResultTask = storedResultTask;
            }

            await Task.WhenAny(currentResultTask, Task.Delay(timeout));

            // If currentResultTask is not completed, the timeout occurred
            if (currentResultTask.IsCompleted == false) {
                // Store the listening task, as it isn't closeable we must reuse it for the next attempt
                storedResultTask = currentResultTask;
                throw new TimeoutException($"Server did not respond within {timeout} ms.");
            }
            
            UdpReceiveResult result = currentResultTask.Result;

            string information;
            {
                using MemoryStream stream = new MemoryStream(result.Buffer);
                using BinaryReader reader = new BinaryReader(stream);

                reader.ReadByte(); // header
                reader.ReadInt64(); // ping id
                reader.ReadInt64(); // server id
                reader.ReadBytes(MagicBytes.Length); // magic
                
                int stringLength = reader.ReadInt16();
                byte[] stringBytes = reader.ReadBytes(stringLength);
                information = Encoding.UTF8.GetString(stringBytes);
            }

            string[] splits = information.Split(";");

            if (splits.Length <= 5) {
                throw new Exception("Invalid data returned from server");
            }

            string minecraftEdition = splits[0];
            string name = splits[1];
            int protocolVersion = int.Parse(splits[2]);
            string version = splits[3];
            int players = int.Parse(splits[4]);
            int maxPlayers = int.Parse(splits[5]);

            string? worldName = splits.Length > 7 ? splits[7] : null;
            string? gameType = splits.Length > 8 ? splits[8] : null;


            return new BedrockServerStatus(minecraftEdition, name, worldName, version, players, maxPlayers, protocolVersion, gameType);
        }
    }

    /// <summary>
    /// Represents the status polled from a Minecraft Bedrock Server
    /// </summary>
    class BedrockServerStatus {
        internal BedrockServerStatus(string minecraftEdition, string name, string? worldName, string version, int playerCount, int maxPlayerCount, int protocolVersion, string? gameType) {
            MinecraftEdition = minecraftEdition;
            Name = name;
            WorldName = worldName;
            Version = version;
            PlayerCount = playerCount;
            MaxPlayerCount = maxPlayerCount;
            ProtocolVersion = protocolVersion;
            GameType = gameType;
        }
        
        /// <summary>
        /// Minecraft Edition. Probably always going to be MCPE
        /// </summary>
        public string MinecraftEdition { get; }
        
        /// <summary>
        /// Name of the server
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Maybe provided name of the server world
        /// </summary>
        public string? WorldName { get; }
        
        /// <summary>
        /// Minecraft version of the server
        /// </summary>
        public string Version { get; }
        
        /// <summary>
        /// Current number of players on the server
        /// </summary>
        public int PlayerCount { get; }
        
        /// <summary>
        /// Maximum possible players on the server
        /// </summary>
        public int MaxPlayerCount { get; }
        
        /// <summary>
        /// Version of the protocol. I have no idea what this means.
        /// </summary>
        public int ProtocolVersion { get; }
        
        /// <summary>
        /// Game mode that's active on the server. As of 2020 this is either "SURVIVAL" or "CREATIVE"
        /// </summary>
        public string? GameType { get; }
    }   
}
