using CinetReportManager.Interfaces;

namespace CinetReportManager.Services
{
    public class TemplateService : ITemplateService
    {

        public Task<string> ApplyPlaceholdersAsync(string templateContent, Dictionary<string, string> placeholders)
        {
            var result = templateContent;

            foreach (var placeholder in placeholders)
            {
                var token = "{{" + placeholder.Key + "}}";
                result = result.Replace(token, placeholder.Value ?? string.Empty);
            }

            return Task.FromResult(result);
        }
    }
}
