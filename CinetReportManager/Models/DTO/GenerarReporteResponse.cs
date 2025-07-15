namespace CinetReportManager.Models.DTO
{
    public class GenerarReporteResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> ArchivosGenerados { get; set; } = new List<string>();
    }
}
