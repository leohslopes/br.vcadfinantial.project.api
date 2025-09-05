using Microsoft.AspNetCore.Http;


namespace br.vcadfinantial.project.domain.DTO
{
    public record UserDTO(string FullName, string Gender, string Email, string Password, IFormFile Photo, DateTime CreateDate);
}
