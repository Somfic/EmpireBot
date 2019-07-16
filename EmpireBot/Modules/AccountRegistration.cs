using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using EmpireBot.Accounts;
using EmpireBot.Services;

namespace EmpireBot.Modules
{
    public class AccountRegistration : ModuleBase<SocketCommandContext>
    {
        public DatabaseService DatabaseService { get; set; }

        [Command("register")]
        [Alias ("reg")]
        public Task RegisterUser()
        {
            UserAccount account = new UserAccount(Context.Message.Author.Id, Context.Message.Author.ToString());

            if (DatabaseService.CheckExistance(account) == DatabaseService.UserExistance.DoesntExist) { 
                DatabaseService.AddEntry(account);
                Context.Message.AddReactionAsync(new Emoji("✅"));
                return ReplyAsync("Your account has been set up. Use `!account` to access your account.");
            } else
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                return ReplyAsync("You already have an account. Use `!account` to access your account.");
            }
        }

        [Command("register town")]
        [Alias("reg town", "register t", "reg t", "town register", "town reg", "t reg", "t register")]
        public Task RegisterTown(string name = null)
        {
            if (name == null)
            {
                return ReplyAsync("Please also specify a town name.");
            }

            UserAccount userAccount = new UserAccount(Context.Message.Author.Id, Context.Message.Author.ToString());
            if(DatabaseService.CheckExistance(userAccount) == DatabaseService.UserExistance.DoesntExist)
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                return ReplyAsync("Please create an account first by using `!register`.");
            }
            userAccount = DatabaseService.GetUserByID(Context.Message.Author.Id);

            TownAccount account = new TownAccount(name, Context.Message.Author.Id);
            userAccount.TownID = account.ID;
            DatabaseService.UpdateEntry(userAccount);

            var response = DatabaseService.CheckExistance(account);

            if (response == DatabaseService.TownExistance.DoesntExist)
            {
                DatabaseService.AddEntry(account);
                Context.Message.AddReactionAsync(new Emoji("✅"));
                return ReplyAsync("Your town has been registered. Use `!town` to access your town.");
            } 
            else if(response == DatabaseService.TownExistance.LeaderAlreadyHasTown)
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                return ReplyAsync("You already have a town registered. Use `!town` to access your town.");
            }
            else if (response == DatabaseService.TownExistance.NameInUse)
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                return ReplyAsync("That name is already in use, try a different name.");
            }

            return Task.CompletedTask;
        }

        [Command("register nation")]
        [Alias("reg nation", "register n", "reg n", "nation register", "nation reg", "n register", "n reg")]
        public Task RegisterNation(string name = null)
        {
            if (name == null)
            {
                return ReplyAsync("Please also specify a nation name.");
            }

            UserAccount userAccount = new UserAccount(Context.Message.Author.Id, Context.Message.Author.ToString());
            if (DatabaseService.CheckExistance(userAccount) == DatabaseService.UserExistance.DoesntExist)
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                return ReplyAsync("Please create an account first by using `!register`.");
            }

            NationAccount account = new NationAccount(name, Context.Message.Author.Id);

            var response = DatabaseService.CheckExistance(account);

            if (response == DatabaseService.NationExistance.DoesntExist)
            {
                DatabaseService.AddEntry(account);
                Context.Message.AddReactionAsync(new Emoji("✅"));
                return ReplyAsync("Your nation has been registered. Use `!nation` to access your nation.");
            }
            else if (response == DatabaseService.NationExistance.LeaderAlreadyHasNation)
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                return ReplyAsync("You already have a nation registered. Use `!nation` to access your nation.");
            }
            else if (response == DatabaseService.NationExistance.NameInUse)
            {
                Context.Message.AddReactionAsync(new Emoji("❎"));
                return ReplyAsync("That name is already in use, try a different name.");
            }

            return Task.CompletedTask;
        }
    }
}
