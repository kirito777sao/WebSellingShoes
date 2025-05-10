using System.ComponentModel.DataAnnotations;

namespace WebSellingShoes.Repository.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        //cách 1:
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName); //vd: 123.jpg => .jpg
                string[] extensions = { ".jpg", ".png", ".jpeg" };

                bool result = extensions.Any(x => extension.EndsWith(x));
                if (!result)
                {
                    return new ValidationResult("Allowed Extensions are .jpg or .png or .jpeg");
                }                
            }
            return ValidationResult.Success;
        }

        /*//cách 2:
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName); //vd: 123.jpg => .jpg
                if (extension != ".jpg" && extension != ".png" && extension != ".jpeg")
                {
                    return new ValidationResult("Allowed Extensions are .jpg or .png or .jpeg");
                }
            }
            return ValidationResult.Success;
        }*/
    }
}
