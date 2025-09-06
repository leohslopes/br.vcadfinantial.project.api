using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace br.vcadfinantial.project.api.Models.Requests
{
    public class DownloadReportLogRequestModel
    {
        [Required(ErrorMessage = "Campo [monthKey] é obrigatório")]
        [JsonPropertyName("monthKey")]
        public required string MonthKey { get; set; }
    }
}
