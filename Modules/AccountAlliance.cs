using Discord;
using Discord.Commands;
using EmpireBot.Accounts;
using EmpireBot.Services;
using System;
using System.Threading.Tasks;

namespace EmpireBot.Modules
{
    public class AccountAlliance : ModuleBase<SocketCommandContext>
    {
        public DatabaseService DatabaseService { get; set; }

        [Command("ally add")]
        [Alias("alliance", "request ally", "req ally", "req alliance", "request alliance", "town ally", "town add ally", "t ally", "t add ally", "nation ally", "nation add ally", "n ally", "n add ally")]
        public Task RequestAlly(string name = null)
        {
            if (name == null)
            {
                return ReplyAsync("Please also enter a name.");
            }

            //Check if they're a nation.
            NationAccount nationAccount = new NationAccount(name, 0);
            if (DatabaseService.CheckExistance(nationAccount) == DatabaseService.NationExistance.NameInUse)
            {
                Request(Context, nationAccount, name);
            }
            else
            {
                //Check if they're a town.
                TownAccount townAccount = new TownAccount(name, 0);
                if (DatabaseService.CheckExistance(townAccount) == DatabaseService.TownExistance.NameInUse)
                {
                    Request(Context, townAccount, name);
                }
                else
                {
                    Context.Message.AddReactionAsync(new Emoji("❎"));
                    return ReplyAsync("That nation or town couldn't be found.");
                }
            }

            return Task.CompletedTask;
        }

        private void Request(SocketCommandContext Context, dynamic target, string name)
        {
            //Check if the user has a town or nation.
            NationAccount nationAccount = new NationAccount("", Context.Message.Author.Id);
            if (DatabaseService.CheckExistance(nationAccount) == DatabaseService.NationExistance.LeaderAlreadyHasNation)
            {
                Request2(Context, nationAccount, target, name);
            }
            else
            {
                //Check if they're a town.
                TownAccount townAccount = new TownAccount("", Context.Message.Author.Id);
                if (DatabaseService.CheckExistance(townAccount) == DatabaseService.TownExistance.LeaderAlreadyHasTown)
                {
                    Request2(Context, townAccount, target, name);
                }
                else
                {
                    Context.Message.AddReactionAsync(new Emoji("❎"));
                    ReplyAsync("You do not have a town or nation. Create a town by doing `-town register`.");
                    return;
                }
            }
        }

        private void Request2(SocketCommandContext Context, dynamic user, dynamic target, string name)
        {
            string aID = "";
            string bID = "";

            string aDiscordID = "";
            string bDiscordID = "";

            string nameA = "";
            string nameB = "";

            if (user.GetType() == typeof(TownAccount))
            {
                TownAccount town = DatabaseService.GetTownByLeaderID(Context.Message.Author.Id.ToString());
                aID = town.ID;
                nameA = $"the city of {town.Name}";
                aDiscordID = town.LeaderID;
            }
            else
            {
                var nation = DatabaseService.GetNationByLeaderID(Context.Message.Author.Id.ToString());
                aID = nation.ID;
                nameA = $"the nation of {nation.Name}";
                aDiscordID = nation.LeaderID;
            }

            if (target.GetType() == typeof(TownAccount))
            {
                TownAccount town = DatabaseService.GetTownByName(name);
                bID = town.ID;
                nameB = $"the city of {town.Name}";
                bDiscordID = town.LeaderID;
            }
            else
            {
                NationAccount nation = DatabaseService.GetNationByName(name);
                bID = nation.ID;
                nameB = $"the nation of {nation.Name}";
                bDiscordID = nation.LeaderID;
            }

            AllyEntry ally = new AllyEntry(aID, bID, aDiscordID, bDiscordID, nameA, nameB);

            //Check if the alliance is already pending.
            if (DatabaseService.CheckExistancePending(ally))
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                ReplyAsync($"You already have a pending alliance request with *`{name}`*.");
                return;
            }

            //Check if the alliance already exists.
            if (DatabaseService.CheckExistance(ally))
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                ReplyAsync($"You already have an alliance with *`{name}`*.");
                return;
            }

            if (aID == bID)
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                ReplyAsync($"You're already allied with yourself.");
                return;
            }

            Context.Client.GetUser(ulong.Parse(bDiscordID)).GetOrCreateDMChannelAsync().Result.SendMessageAsync($@"***New alliance request***

*Greetings,*

*`{FirstLetterToUpper(nameA)}`* has requested an alliance with *`{nameB}`*.
Please reply with `-accept {ally.ID}` or `-deny {ally.ID}`.");

            DatabaseService.AddEntryPending(ally);

            Context.Message.AddReactionAsync(new Emoji("✅"));
            ReplyAsync($"The alliance request has been sent to *`{name}`*. I will get back to you with their reaction.");
        }

        [Command("accept")]
        public Task Accept(string code = null)
        {
            if (code == null)
            {
                return ReplyAsync("Please also include the alliance code.");
            }

            if (!DatabaseService.CheckExistancePending(new AllyEntry(code)))
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                return ReplyAsync($"The alliance request could not be found.");
            }

            var entry = DatabaseService.GetAlliancePending(code);
            DatabaseService.AddEntry(entry);
            DatabaseService.AddEntry(new AllyEntry(entry.BPartyID, entry.APartyID, entry.BPartyDiscordID, entry.APartyDiscordID, entry.BPartyName, entry.APartyName));
            DatabaseService.RemoveEntryPending(entry);

            Context.Client.GetUser(ulong.Parse(entry.APartyDiscordID)).GetOrCreateDMChannelAsync().Result
                .SendMessageAsync($@"***New alliance update***

*Greetings,*

It is our pleasure to inform you that *`{entry.BPartyName}`* has accepted the alliance with *`{entry.APartyName}`*.
You are now allies.");

            Context.Message.AddReactionAsync(new Emoji("✅"));
            return ReplyAsync($"The alliance has been accepted.");
        }

        [Command("deny")]
        public Task Deny(string code = null)
        {
            if (code == null)
            {
                return ReplyAsync("Please also include the alliance code.");
            }

            if (!DatabaseService.CheckExistancePending(new AllyEntry(code)))
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                return ReplyAsync($"The alliance request could not be found.");
            }

            var entry = DatabaseService.GetAlliancePending(code);
            DatabaseService.RemoveEntryPending(entry);

            Context.Client.GetUser(ulong.Parse(entry.APartyDiscordID)).GetOrCreateDMChannelAsync().Result
                .SendMessageAsync($@"***New alliance update***

*Greetings,*

We are sorry to inform you that *`{entry.BPartyName}`* has denied the alliance with *`{entry.APartyName}`*.");

            Context.Message.AddReactionAsync(new Emoji("✅"));
            return ReplyAsync($"The alliance has been denied.");
        }

        [Command("ally add")]
        [Alias("cancel alliance", "delete ally", "del ally", "delete alliance", "del alliance", "town del ally", "town remove ally", "t del ally", "t remove ally", "nation del ally", "nation remove ally", "n del ally", "n remove ally")]
        public Task RemoveAlly(string name = null)
        {
            if (name == null)
            {
                return ReplyAsync("Please also enter a name.");
            }

            //Check if they're a nation.
            NationAccount nationAccount = new NationAccount(name, 0);
            if (DatabaseService.CheckExistance(nationAccount) == DatabaseService.NationExistance.NameInUse)
            {
                Remove(Context, nationAccount, name);
            }
            else
            {
                //Check if they're a town.
                TownAccount townAccount = new TownAccount(name, 0);
                if (DatabaseService.CheckExistance(townAccount) == DatabaseService.TownExistance.NameInUse)
                {
                    Remove(Context, townAccount, name);
                }
                else
                {
                    Context.Message.AddReactionAsync(new Emoji("❎"));
                    return ReplyAsync("That nation or town couldn't be found.");
                }
            }

            return Task.CompletedTask;
        }

        private void Remove(SocketCommandContext Contex, dynamic target, string name)
        {
            //Check if the user has a town or nation.
            NationAccount nationAccount = new NationAccount("", Context.Message.Author.Id);
            if (DatabaseService.CheckExistance(nationAccount) == DatabaseService.NationExistance.LeaderAlreadyHasNation)
            {
                Remove2(Context, nationAccount, target, name);
            }
            else
            {
                //Check if they're a town.
                TownAccount townAccount = new TownAccount("", Context.Message.Author.Id);
                if (DatabaseService.CheckExistance(townAccount) == DatabaseService.TownExistance.LeaderAlreadyHasTown)
                {
                    Remove2(Context, townAccount, target, name);
                }
                else
                {
                    Context.Message.AddReactionAsync(new Emoji("❎"));
                    ReplyAsync("You do not have a town or nation. Create a town by doing `-town register`.");
                    return;
                }
            }
        }

        private void Remove2(SocketCommandContext Context, dynamic user, dynamic target, string name)
        {
            string aID = "";
            string bID = "";

            string aDiscordID = "";
            string bDiscordID = "";

            string nameA = "";
            string nameB = "";

            if (user.GetType() == typeof(TownAccount))
            {
                TownAccount town = DatabaseService.GetTownByLeaderID(Context.Message.Author.Id.ToString());
                aID = town.ID;
                nameA = $"the city of {town.Name}";
                aDiscordID = town.LeaderID;
            }
            else
            {
                var nation = DatabaseService.GetNationByLeaderID(Context.Message.Author.Id.ToString());
                aID = nation.ID;
                nameA = $"the nation of {nation.Name}";
                aDiscordID = nation.LeaderID;
            }

            if (target.GetType() == typeof(TownAccount))
            {
                TownAccount town = DatabaseService.GetTownByName(name);
                bID = town.ID;
                nameB = $"the city of {town.Name}";
                bDiscordID = town.LeaderID;
            }
            else
            {
                NationAccount nation = DatabaseService.GetNationByName(name);
                bID = nation.ID;
                nameB = $"the nation of {nation.Name}";
                bDiscordID = nation.LeaderID;
            }

            AllyEntry ally = new AllyEntry(aID, bID, aDiscordID, bDiscordID, nameA, nameB);

            //Check if the alliance exists.
            if (!DatabaseService.CheckExistance(ally))
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                ReplyAsync($"You do not have an alliance with *`{name}`*.");
                return;
            }

            if (aID == bID)
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                ReplyAsync($"You can't betray yourself.");
                return;
            }

            Context.Client.GetUser(ulong.Parse(bDiscordID)).GetOrCreateDMChannelAsync().Result.SendMessageAsync($@"***Alliance terminated***

*Greetings,*

*`{FirstLetterToUpper(nameA)}`* has hereby terminated the alliance with *`{nameB}`*.
Please accept our condolences.
");

            DatabaseService.RemoveEntry(ally);

            Context.Message.AddReactionAsync(new Emoji("✅"));
            ReplyAsync($"The alliance with *`{name}`* has been terminated. I have informed them as well.");
        }
        public string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
    }
}