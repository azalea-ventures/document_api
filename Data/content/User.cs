using System;
using System.Collections.Generic;

namespace DocumentApi.Data.content;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<ContentDetail> ContentDetails { get; set; } = new List<ContentDetail>();

    public virtual ICollection<ContentVersion> ContentVersions { get; set; } = new List<ContentVersion>();
}
