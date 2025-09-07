using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace br.vcadfinantial.project.api.Models.Requests
{
    public class ForgotEmailRequestModel
    {
        [Required(ErrorMessage = "Campo [email] é obrigatório")]
        [JsonPropertyName("email")]
        public required string Email { get; set; }
    }
}
