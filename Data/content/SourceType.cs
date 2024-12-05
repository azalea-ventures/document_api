using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class SourceType
{
    public int Id { get; set; }

    public string SourceName { get; set; } = null!;

    public virtual ICollection<SourceContent> SourceContents { get; set; } = new List<SourceContent>();
}
