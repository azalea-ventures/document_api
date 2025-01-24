using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class SourceContentField
{
    public int SourceContentId { get; set; }

    public string FieldName { get; set; } = null!;

    public string? FieldContent { get; set; }

    public virtual SourceContent SourceContent { get; set; } = null!;
}
