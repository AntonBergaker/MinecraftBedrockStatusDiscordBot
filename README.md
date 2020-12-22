# MinecraftBedrockStatusDiscordBot
![preview image](https://raw.githubusercontent.com/AntonBergaker/MinecraftBedrockStatusDiscordBot/master/images/preview.png)  
Discord Bot that sets its status to reflect the player count of a Minecraft Bedrock Server. Tested in 1.16.200 but should be fairly future proof as this part of the network protocol doesn't seem to change a lot.

## Settings
The bot is configurable using settings.json. The following fields are available.

**`token`**: Discord Bot OAuth2 token.

**`server_address`**: Minecraft Bedrock server address. Either an IP or URL.

**`server_port`**: Minecraft Bedrock server port. Default: 19132

**`online_message`**: Status to display on the bot when the server is online. Certain keywords will be replaced with server information.  
*Keywords:*
* `$ServerName$`: Server Name  
* `$Version$`: Server Minecraft Version  
* `$PlayerCount$`: Current amount of online players  
* `$MaxPlayerCount$`: Maximum amount of online players  
* `$GameMode$`: Default gamemode on the server. Either SURVIVAL or CREATIVE.  

**`offline_message`**: Status to display on the bot when the server is offline. Keywords are not available here.
