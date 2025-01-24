using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class ContentLanguage
{
    public int Id { get; set; }

    public string? Language { get; set; }

    public string? Abbreviation { get; set; }

    public virtual ICollection<ContentDetail> ContentDetails { get; set; } = new List<ContentDetail>();

    public virtual ICollection<ContentTranslation> ContentTranslationContentLanguages { get; set; } = new List<ContentTranslation>();

    public virtual ICollection<ContentTranslation> ContentTranslationTargetLanguages { get; set; } = new List<ContentTranslation>();
}
