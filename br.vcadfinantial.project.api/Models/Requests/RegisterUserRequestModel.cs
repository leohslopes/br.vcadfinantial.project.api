using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace br.vcadfinantial.project.api.Models.Requests
{
    public class RegisterUserRequestModel
    {
        [Required(ErrorMessage = "Campo [fullName] é obrigatório")]
        [JsonPropertyName("fullName")]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Campo [gender] é obrigatório")]
        [JsonPropertyName("gender")]
        public required string Gender { get; set; }

        [Required(ErrorMessage = "Campo [email] é obrigatório")]
        [JsonPropertyName("email")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Campo [password] é obrigatório")]
        [JsonPropertyName("password")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Campo [photo] é obrigatório")]
        [JsonPropertyName("photo")]
        public IFormFile? Photo { get; set; }
    }
}
