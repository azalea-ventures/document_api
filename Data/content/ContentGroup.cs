using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class ContentGroup
{
    public int Id { get; set; }

    public int ContentVersionId { get; set; }

    public virtual ICollection<ContentTxt> ContentTxts { get; set; } = new List<ContentTxt>();

    public virtual ContentVersion ContentVersion { get; set; } = null!;
}
