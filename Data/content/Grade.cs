using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class Grade
{
    public int Id { get; set; }

    public string Grade1 { get; set; } = null!;

    public virtual ICollection<ContentDetail> ContentDetails { get; set; } = new List<ContentDetail>();

    public virtual ICollection<ContentTranslation> ContentTranslations { get; set; } = new List<ContentTranslation>();

    public virtual ICollection<SourceContent> SourceContents { get; set; } = new List<SourceContent>();
}
