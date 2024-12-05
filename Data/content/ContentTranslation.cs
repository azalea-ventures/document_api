using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class ContentTranslation
{
    public int Id { get; set; }

    public int TargetLanguageId { get; set; }

    public int ContentLanguageId { get; set; }

    public int ContentSubjectId { get; set; }

    public int ContentGradeId { get; set; }

    public virtual Grade ContentGrade { get; set; } = null!;

    public virtual ContentLanguage ContentLanguage { get; set; } = null!;

    public virtual Subject ContentSubject { get; set; } = null!;

    public virtual ContentLanguage TargetLanguage { get; set; } = null!;
}
