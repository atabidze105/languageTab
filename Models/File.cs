using System;
using System.Collections.Generic;

namespace languageTab.Models;

public partial class File
{
    public int IdFile { get; set; }

    public string Link { get; set; } = null!;

    public virtual ICollection<ClientsFile> ClientsFiles { get; set; } = new List<ClientsFile>();
}
