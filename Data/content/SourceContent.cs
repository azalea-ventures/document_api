using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class SourceContent
{
    public int Id { get; set; }

    public int SubjectId { get; set; }

    public int GradeId { get; set; }

    public int SourceTypeId { get; set; }

    public string SourceContentName { get; set; } = null!;

    public virtual Grade Grade { get; set; } = null!;

    public virtual SourceType SourceType { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
