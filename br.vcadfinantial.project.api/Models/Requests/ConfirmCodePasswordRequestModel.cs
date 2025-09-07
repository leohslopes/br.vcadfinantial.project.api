using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace br.vcadfinantial.project.api.Models.Requests
{
    public class ConfirmCodePasswordRequestModel
    {
        [Required(ErrorMessage = "Campo [email] é obrigatório")]
        [JsonPropertyName("email")]
        public required string Email { get; set; }


        [Required(ErrorMessage = "Campo [password] é obrigatório")]
        [JsonPropertyName("password")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Campo [code] é obrigatório")]
        [JsonPropertyName("code")]
        public required string Code { get; set; }
    }
}
