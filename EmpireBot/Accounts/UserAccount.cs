using System.Collections.Generic;

namespace EmpireBot.Accounts
{
    public class UserAccount
    {
        public UserAccount(ulong discordID, string discordTag)
        {
            DiscordID = discordID.ToString();
            DiscordTag = discordTag;
        }

        public UserAccount(IList<object> objects)
        {
            try { DiscordID = objects[0].ToString(); } catch { }
            try { DiscordTag = objects[1].ToString(); } catch { }
            try { TownID = objects[2].ToString(); } catch { }
        }

        public string DiscordID { get; private set; }
        public string DiscordTag { get; private set; }
        public string TownID { get; set; }

        public List<object> List => new List<object>() { DiscordID, DiscordTag, TownID };
    }
}
