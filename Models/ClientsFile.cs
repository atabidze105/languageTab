using System;
using System.Collections.Generic;

namespace languageTab.Models;

public partial class ClientsFile
{
    public int IdFile { get; set; }

    public int IdClient { get; set; }

    public int IdClientfile { get; set; }

    public virtual Client IdClientNavigation { get; set; } = null!;

    public virtual File IdFileNavigation { get; set; } = null!;
}
