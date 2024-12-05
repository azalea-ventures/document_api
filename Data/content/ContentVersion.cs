using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class ContentVersion
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int OwnerId { get; set; }

    public int ContentDetailsId { get; set; }

    public byte? IsLatest { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ContentDetail ContentDetails { get; set; } = null!;

    public virtual ICollection<ContentGroup> ContentGroups { get; set; } = new List<ContentGroup>();

    public virtual User Owner { get; set; } = null!;
}
