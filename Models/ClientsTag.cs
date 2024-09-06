using System;
using System.Collections.Generic;

namespace languageTab.Models;

public partial class ClientsTag
{
    public int IdClientTag { get; set; }

    public int IdClient { get; set; }

    public int IdTag { get; set; }

    public virtual Client IdClientNavigation { get; set; } = null!;

    public virtual Tag IdTagNavigation { get; set; } = null!;
}
