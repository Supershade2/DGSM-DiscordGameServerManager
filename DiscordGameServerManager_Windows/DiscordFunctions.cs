using System;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordGameServerManager_Windows
{
    public class DiscordFunctions
    {
        static readonly DiscordClient discord = new DiscordClient(new DiscordConfiguration
        {
            Token = Config.bot.token,
            TokenType = TokenType.Bot
        });
        static async Task message_send(string str, DiscordChannel discordChannel)
        {
            DiscordMessage discordMessage = await discord.SendMessageAsync(discordChannel, str, false, null);
            await discordMessage.RespondAsync();
        }
    }
}
