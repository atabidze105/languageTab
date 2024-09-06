using System;
using System.Collections.Generic;

namespace languageTab.Models;

public partial class Tag
{
    public int IdTag { get; set; }

    public string Name { get; set; } = null!;

    public string ColorTag { get; set; } = null!;

    public virtual ICollection<ClientsTag> ClientsTags { get; set; } = new List<ClientsTag>();
}
