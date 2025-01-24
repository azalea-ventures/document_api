using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class ContentFile
{
    public Guid Id { get; set; }

    public string? FileName { get; set; }

    public string? FileId { get; set; }

    public string? SourceFileId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ContentDetail> ContentDetails { get; set; } = new List<ContentDetail>();
}
