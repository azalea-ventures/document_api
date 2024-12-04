public class Document
{
    public string DocType { get; set; }
    public int StartPage { get; set; }
    public int EndPage { get; set; }
}

public class DocumentFields
{
    public DocumentFields() { }

    public DocumentFields(List<FieldBase> fields)
    {
        RawFields = fields;
    }

    public List<FieldBase> RawFields { get; set; } = new();
    public List<LessonField> LessonFields { get; set; } = new();
    public List<ModuleOverviewField> ModuleOverviewFields { get; set; } = new();
}

public class FieldBase
{
    public FieldBase(string name, string content)
    {
        FieldName = name;
        FieldContentRaw = content;
    }

    public string FieldName { get; set; }
    public string FieldContentRaw { get; set; }
}

public class LessonField : FieldBase
{
    public LessonField(string name, string content)
        : base(name, content) { }
}

public class ModuleOverviewField : FieldBase
{
    public ModuleOverviewField(string name, string[] content)
        : base(name, string.Join(", ", content))
    {
        FieldContent = content;
    }

    public string[] FieldContent { get; set; }
}
