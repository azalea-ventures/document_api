using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class Client
{
    public int Id { get; set; }

    public string ClientName { get; set; } = null!;

    public virtual ICollection<ContentDetail> ContentDetails { get; set; } = new List<ContentDetail>();
}
