using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class ContentDetail
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int GradeId { get; set; }

    public int SubjectId { get; set; }

    public DateOnly DeliveryDate { get; set; }

    public int OwnerId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? FileId { get; set; }

    public int? StatusId { get; set; }

    public int LanguageId { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<ContentVersion> ContentVersions { get; set; } = new List<ContentVersion>();

    public virtual ContentFile? File { get; set; }

    public virtual Grade Grade { get; set; } = null!;

    public virtual ContentLanguage Language { get; set; } = null!;

    public virtual User Owner { get; set; } = null!;

    public virtual ContentStatus? Status { get; set; }

    public virtual Subject Subject { get; set; } = null!;
}
