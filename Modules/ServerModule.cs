using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmpireBot.Modules
{
    public class ServerModule : ModuleBase<SocketCommandContext>
    {
        [Command("ip")]
        [Alias("server", "mc", "minecraft")]
        public Task ServerIP() => ReplyAsync("Server IP: `MedievalEmpires.aquatis.io`. Minecraft `v1.14.3`.");
    }
}
