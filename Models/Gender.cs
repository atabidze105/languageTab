using System;
using System.Collections.Generic;

namespace languageTab.Models;

public partial class Gender
{
    public int IdGender { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
}
