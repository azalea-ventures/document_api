using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class ContentTxt
{
    public int Id { get; set; }

    public string ObjectId { get; set; } = null!;

    public int ContentGroupId { get; set; }

    public string? Txt { get; set; }

    public virtual ContentGroup ContentGroup { get; set; } = null!;
}
