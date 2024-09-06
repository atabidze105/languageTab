using languageTab.Context;
using languageTab.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static languageTab.Context.Helper;


namespace languageTab
{
    internal static class StaticData
    {
        public static List<Client> AllClients = Database.Clients.Include(x => x.ClientsTags).Include(x => x.VisitsLogs).Include(x => x.ClientsFiles).ToList();
        public static List<Gender> Genders = Database.Genders.ToList();
        public static List<Models.Tag> tags = Database.Tags.ToList();
        public static List<List<Models.Tag>> tagsEvery = [];
        public static List<Models.VisitsLog> visitLog = Database.VisitsLogs.ToList();
        public static List<List<DateOnly>> datesEvery = [];


        public static void GetTags()
        {
            foreach (Client client in AllClients)
            {
                List<Models.Tag> tagsOfClient = [];
                foreach (ClientsTag clientTag in Database.ClientsTags)
                {
                    if (client.IdClient == clientTag.IdClient)
                    {
                        tagsOfClient.Add(tags[clientTag.IdTag]);
                    }
                }
                tagsEvery.Add(tagsOfClient);
            }
        }

        public static void GetDates()
        {
            foreach (Client client in AllClients)
            {
                List<DateOnly> visits = [];
                foreach (VisitsLog visit in Database.VisitsLogs)
                {
                    if (client.IdClient == visit.IdClient)
                    {
                        visits.Add(visit.Date);
                    }
                }
                visits.Sort((a,b) => a.Day - b.Day);
                datesEvery.Add(visits);
            }
        }
    }
}
