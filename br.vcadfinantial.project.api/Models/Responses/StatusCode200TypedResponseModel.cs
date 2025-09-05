using System.ComponentModel.DataAnnotations;

namespace br.vcadfinantial.project.api.Models.Responses
{
    public class StatusCode200TypedResponseModel<T> : StatusCode200StandardResponseModel
    {
        public StatusCode200TypedResponseModel()
        {
                
        }

        [Required]
        public T Data { get; set; }
    }
}
