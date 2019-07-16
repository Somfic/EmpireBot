using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EmpireBot.Accounts
{
    public class TownAccount
    {
        public TownAccount(string name, ulong leaderID)
        {
            Name = name;
            LeaderID = leaderID.ToString();
            ID = DateTime.Now.Ticks.ToString();
        }

        public TownAccount(IList<object> objects)
        {
            try { ID = objects[0].ToString(); } catch { }
            try { Name = objects[1].ToString(); } catch { }
            try { LeaderID = objects[2].ToString(); } catch { }
            try { NationID = objects[3].ToString(); } catch { }
        }

        public string ID { get; private set; }
        public string Name { get; private set; }
        public string LeaderID { get; private set; }
        public string NationID { get; private set; }

        public List<object> List => new List<object>() { ID, Name, LeaderID, NationID };
    }
}
