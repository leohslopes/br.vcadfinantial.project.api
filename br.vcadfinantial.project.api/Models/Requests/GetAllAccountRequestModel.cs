using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace br.vcadfinantial.project.api.Models.Requests
{
    public class GetAllAccountRequestModel
    {
        [Required(ErrorMessage = "Campo [accountKey] é obrigatório.")]
        [JsonPropertyName("accountKey")]
        public long AccountKey { get; set; }

        [Required(ErrorMessage = "Campo [userId] é obrigatório.")]
        [JsonPropertyName("userId")]
        public required string UserId { get; set; }
    }
}
