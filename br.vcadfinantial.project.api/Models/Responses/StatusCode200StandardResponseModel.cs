using FluentValidation.Results;
using System.ComponentModel.DataAnnotations;

namespace br.vcadfinantial.project.api.Models.Responses
{
    public class StatusCode200StandardResponseModel
    {
        public StatusCode200StandardResponseModel()
        {
            Errors = new Dictionary<string, string>();
        }
        public StatusCode200StandardResponseModel(FluentValidation.ValidationException ve)
        {
            Success = false;
            Errors = new Dictionary<string, string>();
            var errorsList = ve.Errors.ToList();

            if (!string.IsNullOrEmpty(ve.Message) && !errorsList.Any())
            {
                ValidationFailure error = new("errorMessage", ve.Message)
                {
                    ErrorCode = "errorCode"
                };
                errorsList.Add(error);
            }


            if (errorsList.Count > 0)
            {
                foreach (ValidationFailure item in errorsList)
                {
                    Errors.Add(new KeyValuePair<string, string>($"{item.PropertyName}_{item.ErrorCode}", item.ErrorMessage));
                }
            }
        }

        [Required]
        public bool? Success { get; set; } = true;
        public IDictionary<string, string> Errors { get; set; }
    }
}
