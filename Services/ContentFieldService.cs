using DocumentApi.Data.content;
using Microsoft.EntityFrameworkCore;

namespace DocumentApi.Services
{
    public interface IContentFieldService
    {
        Task<string> PostModuleOverviewFields(List<FieldBase> fields, string path, string sourceType);
        Task<SourceContent?> GetSourceContentByPath(string path); // New method
    }

    public class ContentFieldService : IContentFieldService
    {
        private readonly ContentContext _context;

        public ContentFieldService(ContentContext context)
        {
            _context = context;
        }

        public async Task<SourceContent?> GetSourceContentByPath(string path)
        {
            return await _context.SourceContents.FirstOrDefaultAsync(sc => sc.SourceContentName == path);
        }

        public async Task<string> PostModuleOverviewFields(
            List<FieldBase> fields,
            string path,
            string sourceType
        )
        {
            var uri = new Uri(path);
            var parts = uri.AbsolutePath.Split(new[] { "/source-files/" }, StringSplitOptions.None).Last().Split('/');
            if (parts.Length < 4 || parts.Last() != "module_overview.pdf")
            {
                return "no match on module";
            }

            string subject = parts[0];
            string grade = parts[1];
            string module = parts[2];
            string lesson = parts[3];

            if (sourceType.ToUpper() == "EUREKA")
            {
                // Check if a record exists with the same SourceContentName
                var existingSourceContent = await GetSourceContentByPath(path);
                if (existingSourceContent != null)
                {
                    return "record already exists";
                }

                // try to find a matching source content record based on grade and subject
                var sourceContent = await _context
                    .SourceContents.Include(sc => sc.SourceType)
                    .ThenInclude(st => st.SourceTypeSubdivisions)
                    .FirstOrDefaultAsync(sc =>
                        sc.Subject.Subject1 == subject && sc.Grade.Grade1 == grade
                    );

                // or create a new one if it doesn't exist
                if (sourceContent == null)
                {
                    var existingSubject = await _context.Subjects.FirstOrDefaultAsync(s => s.Subject1 == subject);
                    if (existingSubject == null)
                    {
                        return $"Subject '{subject}' not found";
                    }

                    var existingGrade = await _context.Grades.FirstOrDefaultAsync(g => g.Grade1 == grade);
                    if (existingGrade == null)
                    {
                        return $"Grade '{grade}' not found";
                    }

                    sourceContent = new SourceContent
                    {
                        Subject = existingSubject,
                        Grade = existingGrade,
                        SourceContentName = path // Set the SourceContentName property
                    };
                    _context.SourceContents.Add(sourceContent);
                    await _context.SaveChangesAsync();
                }

                var subdivision = sourceContent.SourceType.SourceTypeSubdivisions.FirstOrDefault(sts =>
                    sts.SubdivName.ToUpper() == module.ToUpper()
                );

                if (subdivision == null)
                {
                    subdivision = new SourceTypeSubdivision
                    {
                        SourceTypeId = sourceContent.SourceTypeId,
                        SubdivName = module,
                        SubdivLevel = 1
                    };
                    _context.SourceTypeSubdivisions.Add(subdivision);
                    await _context.SaveChangesAsync();
                }

                if (!string.IsNullOrEmpty(lesson))
                {
                    return "this is a lesson document";
                }

                foreach (var field in fields)
                {
                    var sourceContentField = new SourceContentField
                    {
                        SourceContentId = sourceContent.Id,
                        FieldName = field.FieldName,
                        FieldContent = field.FieldContentRaw
                    };
                    _context.SourceContentFields.Add(sourceContentField);
                }

                await _context.SaveChangesAsync();
                return "success";
            }

            return "source type not supported";
        }
    }
}
