using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace br.vcadfinantial.project.api.Models.Requests
{
    public class ImportFileRequestModel
    {
        [Required(ErrorMessage = "Campo [file] é obrigatório.")]
        [JsonPropertyName("file")]
        public IFormFile? File { get; set; }

        [Required(ErrorMessage = "Campo [userId] é obrigatório.")]
        [JsonPropertyName("userId")]
        public required string UserId { get; set; }
    }
}
