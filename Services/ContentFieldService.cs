using DocumentApi.Data.content;
using Microsoft.EntityFrameworkCore;

namespace DocumentApi.Services
{
    public interface IContentFieldService
    {
        Task<string> PostModuleOverviewFields(List<FieldBase> fields, string path, string sourceType);
    }

    public class ContentFieldService : IContentFieldService
    {
        private readonly ContentContext _context;

        public ContentFieldService(ContentContext context)
        {
            _context = context;
        }
        public async Task<string> PostModuleOverviewFields(
            List<FieldBase> fields,
            string path,
            string sourceType
        )
        {
            var parts = path.Split('/');
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
                var sourceContent = await _context
                    .SourceContents.Include(sc => sc.SourceType)
                    .ThenInclude(st => st.SourceTypeSubdivisions)
                    .FirstOrDefaultAsync(sc =>
                        sc.Subject.Subject1 == subject && sc.Grade.Grade1 == grade
                    );

                if (sourceContent == null)
                {
                    sourceContent = new SourceContent { Subject = new(subject), Grade = new(grade), };
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
