using System;
using System.Collections.Generic;
using System.Linq;

namespace languageTab.Models;

public partial class Client
{
    public int IdClient { get; set; }

    public string Firstname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Surname { get; set; }

    public int IdGender { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateOnly RegistrationDate { get; set; }

    public DateOnly? LastVisit => VisitsLogs.Count > 0 ? VisitsLogs.OrderByDescending(x => x.Date).ToList().First().Date : null ;

    public string? Photo { get; set; }

    public virtual ICollection<ClientsFile> ClientsFiles { get; set; } = new List<ClientsFile>();

    public virtual ICollection<ClientsTag> ClientsTags { get; set; } = new List<ClientsTag>();

    public virtual Gender IdGenderNavigation { get; set; } = null!;

    public virtual ICollection<VisitsLog> VisitsLogs { get; set; } = new List<VisitsLog>();
}
