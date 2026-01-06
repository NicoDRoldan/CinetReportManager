namespace CinetReportManager.Interfaces
{
    public interface ITemplateService
    {
        Task<string> ApplyPlaceholdersAsync(string templateContent, Dictionary<string, string> placeholders);
    }
}
