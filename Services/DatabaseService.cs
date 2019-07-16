using ConsoleTableExt;
using EmpireBot.Accounts;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace EmpireBot.Services
{
    public class DatabaseService
    {
        private readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private readonly string ApplicationName = "EmpireBot";
        private readonly string SpreadSheetID = "1cpRzRoqdsaXlbHRg0tpgLzO9NDLArz0fbvGCay_hPwI";

        private readonly SheetsService service;

        public DatabaseService(IServiceProvider services)
        {
            GoogleCredential credential;
            using (FileStream stream = new FileStream("client.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
        }

        public enum UserExistance { DoesntExist, AlreadyRegistered }
        public UserExistance CheckExistance(UserAccount account)
        {
            ValueRange values = GetValues("users");

            foreach (IList<object> row in values.Values.Skip(1))
            {
                if (row[0].ToString() == account.DiscordID) { return UserExistance.AlreadyRegistered; }
            }

            return UserExistance.DoesntExist;
        }

        public enum TownExistance { DoesntExist, NameInUse, LeaderAlreadyHasTown }
        public TownExistance CheckExistance(TownAccount account)
        {
            ValueRange values = GetValues("towns");

            foreach (IList<object> row in values.Values.Skip(1))
            {
                if (row[2].ToString() == account.LeaderID) { return TownExistance.LeaderAlreadyHasTown; }
                if (row[1].ToString() == account.Name) { return TownExistance.NameInUse; }
            }

            return TownExistance.DoesntExist;
        }

        public enum NationExistance { DoesntExist, NameInUse, LeaderAlreadyHasNation }
        public NationExistance CheckExistance(NationAccount account)
        {
            ValueRange values = GetValues("nations");

            foreach (IList<object> row in values.Values.Skip(1))
            {
                if (row[2].ToString() == account.LeaderID) { return NationExistance.LeaderAlreadyHasNation; }
                if (row[1].ToString() == account.Name) { return NationExistance.NameInUse; }
            }

            return NationExistance.DoesntExist;
        }

        public bool CheckExistancePending(AllyEntry entry)
        {
            ValueRange values = GetValues("alliance-pending");

            foreach (IList<object> row in values.Values.Skip(1))
            {
                if (row[1].ToString() == entry.APartyID && row[2].ToString() == entry.BPartyID) { return true; }
                if (row[0].ToString() == entry.ID) { return true; }
            }

            return false;
        }

        public bool CheckExistance(AllyEntry entry)
        {
            ValueRange values = GetValues("alliances");

            foreach (IList<object> row in values.Values.Skip(1))
            {
                if (row[1].ToString() == entry.APartyID && row[2].ToString() == entry.BPartyID) { return true; }
                if (row[0].ToString() == entry.ID) { return true; }
            }

            return false;
        }
        private ValueRange GetValues(string SheetName)
        {
            string range = $"{SheetName}!A:Z";
            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(SpreadSheetID, range);

            ValueRange response = request.Execute();

            return response;
        }

        private void ReadEntries(string SheetName)
        {
            string range = $"{SheetName}!A:F";
            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(SpreadSheetID, range);

            ValueRange response = request.Execute();

            IList<IList<object>> values = response.Values;

            if (values != null && values.Count > 0)
            {
                DataTable table = new DataTable(SheetName);

                foreach (object rowheader in values[0])
                {
                    table.Columns.Add(rowheader.ToString(), typeof(string));
                }

                foreach (IList<object> row in values.Skip(1))
                {
                    table.Rows.Add(row.Cast<string>().ToArray());
                }

                ConsoleTableBuilder
                   .From(table)
                   .WithFormat(ConsoleTableBuilderFormat.MarkDown)
                   .ExportAndWriteLine();
            }
            else
            {
                Console.WriteLine("No data found.");
            }
        }

        public void AddEntry(UserAccount account)
        {
            AddEntry("users", account.List);
        }

        public void AddEntry(TownAccount account)
        {
            AddEntry("towns", account.List);
        }

        public void AddEntry(NationAccount account)
        {
            AddEntry("nations", account.List);
        }
        public void AddEntryPending(AllyEntry entry)
        {
            AddEntry("alliance-pending", entry.List);
        }
        public void AddEntry(AllyEntry entry)
        {
            AddEntry("alliances", entry.List);
        }

        public void UpdateEntry(UserAccount account)
        {
            UpdateEntry("users", account.List);
        }

        public void UpdateEntry(TownAccount account)
        {
            UpdateEntry("towns", account.List);
        }

        public void UpdateEntry(NationAccount account)
        {
            UpdateEntry("nations", account.List);
        }
        public void UpdateEntryPending(AllyEntry entry)
        {
            UpdateEntry("alliance-pending", entry.List);
        }
        public void UpdateEntry(AllyEntry entry)
        {
            UpdateEntry("alliances", entry.List);
        }

        public void RemoveEntryPending(AllyEntry entry)
        {
            RemoveEntry("alliance-pending", entry.List);
        }

        private void AddEntry(string SpreadSheet, List<object> objectArray)
        {
            string range = $"{SpreadSheet}!A:Z";
            ValueRange valueRange = new ValueRange
            {
                Values = new List<IList<object>> { objectArray }
            };

            SpreadsheetsResource.ValuesResource.AppendRequest appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadSheetID, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

            AppendValuesResponse appendRespond = appendRequest.Execute();
        }

        private void UpdateEntry(string SpreadSheet, List<object> objectArray)
        {
            ValueRange values = GetValues(SpreadSheet);

            int i = 1;
            bool hasBeenSet = false;
            foreach (IList<object> row in values.Values.Skip(1))
            {
                i++;
                if (row[0].ToString() == objectArray[0].ToString()) { hasBeenSet = true; break; }
            }

            if(!hasBeenSet) { return; }

            string range = $"{SpreadSheet}!A{i}:Z{i}";
            ValueRange valueRange = new ValueRange
            {
                Values = new List<IList<object>> { objectArray }
            };

            SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadSheetID, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            UpdateValuesResponse updateRespond = updateRequest.Execute();
        }

        private void RemoveEntry(string SpreadSheet, List<object> objectArray)
        {
            ValueRange values = GetValues(SpreadSheet);

            int i = 1;
            bool hasBeenSet = false;
            foreach (IList<object> row in values.Values.Skip(1))
            {
                i++;
                if (row[0].ToString() == objectArray[0].ToString()) { hasBeenSet = true; break; }
            }

            if (!hasBeenSet) { return; }

            string range = $"{SpreadSheet}!A{i}:Z{i}";
            ClearValuesRequest valueRange = new ClearValuesRequest();

            SpreadsheetsResource.ValuesResource.ClearRequest clearRequest = service.Spreadsheets.Values.Clear(valueRange, SpreadSheetID, range);
            ClearValuesResponse updateRespond = clearRequest.Execute();
        }

        public AllyEntry GetAlliancePendingByA(string id)
        {
            var values = GetValues("alliance-pending");

            foreach (var row in values.Values.Skip(1))
            {
                if (row[1].ToString() == id)
                {
                    return new AllyEntry(row);
                }
            }

            return null;
        }

        public AllyEntry GetAlliancePendingByB(string id)
        {
            var values = GetValues("alliance-pending");

            foreach (var row in values.Values)
            {
                if (row[2].ToString() == id)
                {
                    return new AllyEntry(row);
                }
            }

            return null;
        }

        public AllyEntry GetAlliancePending(string id)
        {
            var values = GetValues("alliance-pending");

            foreach (var row in values.Values)
            {
                if (row[0].ToString() == id)
                {
                    return new AllyEntry(row);
                }
            }

            return null;
        }

        public List<AllyEntry> GetAlliancesByA(string id)
        {
            var values = GetValues("alliances");

            List<AllyEntry> entries = new List<AllyEntry>();

            foreach (var row in values.Values)
            {
                if (row[1].ToString() == id)
                {
                    entries.Add(new AllyEntry(row));
                }
            }

            return entries;
        }

        public List<AllyEntry> GetAlliancesByB(string id)
        {
            var values = GetValues("alliances");

            List<AllyEntry> entries = new List<AllyEntry>();

            foreach (var row in values.Values)
            {
                if (row[2].ToString() == id)
                {
                    entries.Add(new AllyEntry(row));
                }
            }

            return entries;
        }

        public UserAccount GetUserByID(ulong id)
        {
            var values = GetValues("users");

            foreach (var row in values.Values)
            {
                if(row[0].ToString() == id.ToString())
                {
                    return new UserAccount(row);
                }
            }

            return null;
        }

        public TownAccount GetTownByName(string name)
        {
            var values = GetValues("towns");

            foreach (var row in values.Values)
            {
                if (row[1].ToString() == name)
                {
                    return new TownAccount(row);
                }
            }

            return null;
        }

        public NationAccount GetNationByName(string name)
        {
            var values = GetValues("nations");

            foreach (var row in values.Values)
            {
                if (row[1].ToString() == name)
                {
                    return new NationAccount(row);
                }
            }

            return null;
        }

        public TownAccount GetTownByLeaderID(string id)
        {
            var values = GetValues("towns");

            foreach (var row in values.Values)
            {
                if (row[2].ToString() == id)
                {
                    return new TownAccount(row);
                }
            }

            return null;
        }

        public NationAccount GetNationByLeaderID(string id)
        {
            var values = GetValues("nations");

            foreach (var row in values.Values)
            {
                if (row[2].ToString() == id)
                {
                    return new NationAccount(row);
                }
            }

            return null;
        }
    }
}
