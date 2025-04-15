using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PhoneBookApi.Tests.Helpers
{
    public static class ModelValidator
    {
        public static void ValidateModel(object model, ControllerBase controller)
        {
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(model, context, results, true);

            foreach (var validationResult in results)
            {
                controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage!);
            }
        }
    }
}
