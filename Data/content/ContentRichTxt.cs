using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class ContentRichTxt
{
    public int ContentTxtId { get; set; }

    public string? RichTxt { get; set; }

    public virtual ContentTxt ContentTxt { get; set; } = null!;
}
