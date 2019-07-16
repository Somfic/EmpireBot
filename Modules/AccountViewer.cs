using Discord;
using Discord.Commands;
using EmpireBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EmpireBot.Modules
{
    public class AccountViewer : ModuleBase<SocketCommandContext>
    {
        public DatabaseService DatabaseService { get; set; }

        [Command("account")]
        [Alias("me", "info", "town", "t", "nation", "n", "allies", "war", "wars")]
        public Task Me()
        {
            EmbedBuilder meEmbed = new EmbedBuilder();
            EmbedBuilder townEmbed = new EmbedBuilder();
            EmbedBuilder nationEmbed = new EmbedBuilder();

            var account = DatabaseService.GetUserByID(Context.Message.Author.Id);
            if(account != null)
            {
                meEmbed.AddField("Account", $"Name: **{account.DiscordTag}**", true);

                if(!string.IsNullOrEmpty(account.TownID))
                {
                    var town = DatabaseService.GetTownByLeaderID(Context.Message.Author.Id.ToString());

                    string allyList = "";
                    var allies = DatabaseService.GetAlliancesByA(town.ID);
                    if(allies.Count == 0) { allyList = "No allies."; }
                    else { allies.ForEach(x => allyList += x.BPartyName + Environment.NewLine); }

                    meEmbed.AddField("Town", $"Name: **{town.Name}**", true);
                    townEmbed.AddField("The town of " + town.Name + " (allies)", allyList);

                    if (!string.IsNullOrEmpty(town.NationID))
                    {
                        var nation = DatabaseService.GetNationByLeaderID(Context.Message.Author.Id.ToString());

                        string allyList2 = "";
                        var allies2 = DatabaseService.GetAlliancesByA(town.ID);
                        if (allies2.Count == 0) { allyList2 = "No allies."; }
                        else { allies2.ForEach(x => allyList2 += x.BPartyName + Environment.NewLine); }

                        meEmbed.AddField("Nation", $"Name: **{nation.Name}**", true);
                        townEmbed.AddField("The nation of " + nation.Name + " (allies)", allyList2);
                    }
                }
            } else
            {
                meEmbed.AddField("No account found!", "Register an account by doing `-register`.");
            }

            ReplyAsync("", false, meEmbed.Build()).Wait();
            if(townEmbed.Fields.Count != 0) { ReplyAsync("", false, townEmbed.Build()).Wait(); }
            if (nationEmbed.Fields.Count != 0) { ReplyAsync("", false, nationEmbed.Build()).Wait(); }
            return Task.CompletedTask;
        }
    }
}
