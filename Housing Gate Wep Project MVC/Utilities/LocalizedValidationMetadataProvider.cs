using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using StudentHousing.Resources;

namespace StudentHousing.Helpers
{
    /// <summary>
    /// Global DataAnnotations localization hook.
    ///
    /// ASP.NET Core MVC only localizes a built-in validation attribute's error
    /// message when the attribute carries an explicit resource reference, and the
    /// only culture-aware mechanism that works on BOTH the server and the client
    /// (unlike the ErrorMessage key used by the client-side adapters) is
    /// ErrorMessageResourceName + ErrorMessageResourceType. Those two properties
    /// are resolved through a ResourceManager at validation time using the current
    /// UI culture, so no per-attribute boilerplate is needed in the view models.
    ///
    /// This provider stamps every validation attribute that has no explicit message
    /// with a convention key (&lt;AttributeTypeName&gt;_ValidationError) pointing at the
    /// shared resource file, so messages follow the active culture everywhere.
    /// </summary>
    public sealed class LocalizedValidationMetadataProvider : IValidationMetadataProvider
    {
        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            foreach (var attribute in context.ValidationMetadata.ValidatorMetadata)
            {
                if (attribute is not ValidationAttribute validationAttribute)
                {
                    continue;
                }

                if (validationAttribute.ErrorMessageResourceName != null)
                {
                    continue;
                }

                if (validationAttribute.ErrorMessage != null)
                {
                    continue;
                }

                validationAttribute.ErrorMessageResourceName =
                    $"{validationAttribute.GetType().Name}_ValidationError";
                validationAttribute.ErrorMessageResourceType = typeof(SharedResource);
            }
        }
    }
}
