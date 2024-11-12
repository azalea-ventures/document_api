public class Document
{
    // public Document(string docType, int startPage, int endPage){
    //     DocType = docType;
    //     StartPage = startPage;
    //     EndPage = endPage;
    // }
    public string DocType { get; set; }
    public int StartPage { get; set; }
    public int EndPage { get; set; }
}

public class DocumentFields
{
    public List<LessonField> Fields { get; set; } =
        new ();
}

public class LessonField{

    public LessonField (string name, string content){
        FieldName = name;
        FieldContent = content;
    }
    public string FieldName { get; set;}
    public string FieldContent  { get; set;}
}
