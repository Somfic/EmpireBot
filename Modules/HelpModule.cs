using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmpireBot.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Alias("h")]
        public Task HelpCommand()
        {
            return ReplyAsync("", false, new Discord.EmbedBuilder()
                .AddField("-help", "Displays this message.", true)
                .AddField("-server", "Displays information about the server.", true)
                .AddField("-register", "Registers an account and enables you to create towns and nations.", true)
                .AddField("-register town <name>", "Registers a new town.", true)
                .AddField("-register nation <nation>", "Registers a new nation. You must have a town before registering a nation.", true)
                .AddField("-ally add <town/nation>", "Sends a alliance request to another town or nation.", true)
                .AddField("-ally remove <town/nation>", "Cancels an alliance with another town or nation.", true)
                .AddField("-account", "Displays all information about you.", true)
                .Build());
        }
    }
}
