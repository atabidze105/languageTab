using System;
using System.Collections.Generic;

namespace languageTab.Models;

public partial class VisitsLog
{
    public int IdClient { get; set; }

    public DateOnly Date { get; set; }

    public int IdLog { get; set; }

    public virtual Client IdClientNavigation { get; set; } = null!;
}
