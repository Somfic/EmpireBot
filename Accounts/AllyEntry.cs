using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EmpireBot.Accounts
{
    public class AllyEntry
    {
        public AllyEntry(string code)
        {
            ID = code;
        }

        public AllyEntry(IList<object> objects)
        {
            try { ID = objects[0].ToString(); } catch { }
            try { APartyID = objects[1].ToString(); } catch { }
            try { BPartyID = objects[2].ToString(); } catch { }
            try { APartyDiscordID = objects[3].ToString(); } catch { }
            try { BPartyDiscordID = objects[4].ToString(); } catch { }
            try { APartyName = objects[5].ToString(); } catch { }
            try { BPartyName = objects[6].ToString(); } catch { }
        }

        public AllyEntry(string aPartyID, string bPartyID, string aPartyDiscordID, string bPartyDiscordID, string aPartyName, string bPartyName)
        {
            ID = RandomString(5);
            APartyID = aPartyID;
            BPartyID = bPartyID;
            APartyDiscordID = aPartyDiscordID;
            BPartyDiscordID = bPartyDiscordID;
            APartyName = aPartyName;
            BPartyName = bPartyName;
        }

        public string ID { get; private set; }
        public string APartyID { get; private set; }
        public string BPartyID { get; private set; }
        public string APartyDiscordID { get; private set; }
        public string BPartyDiscordID { get; private set; }
        public string APartyName { get; private set; }
        public string BPartyName { get; private set; }
        public List<object> List => new List<object>() { ID, APartyID, BPartyID, APartyDiscordID, BPartyDiscordID, APartyName, BPartyName };

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[new Random().Next(s.Length)]).ToArray()).ToLower();
        }
    }
}
