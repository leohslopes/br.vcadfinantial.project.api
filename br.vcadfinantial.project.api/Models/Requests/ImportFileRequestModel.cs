using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace br.vcadfinantial.project.api.Models.Requests
{
    public class ImportFileRequestModel
    {
        [Required(ErrorMessage = "Campo [file] é obrigatório.")]
        [JsonPropertyName("file")]
        public IFormFile? File { get; set; }
    }
}
