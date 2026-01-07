using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VortexTCG.Auth.DTOs;
using Xunit;

namespace VortexTCG.Tests.Auth.DTOs
{
    public class RegisterDTOTests
    {
        [Fact]
        public void InvalidEmail_FailsValidation()
        {
            RegisterDTO dto = new RegisterDTO
            {
                first_name = "John",
                last_name = "Doe",
                username = "johndoe",
                email = "not-an-email",
                password = "StrongPass1!",
                password_confirmation = "StrongPass1!"
            };

            List<ValidationResult> results = Validate(dto);

            Assert.NotEmpty(results);
            Assert.Contains(results, result => result.MemberNames.Contains("email"));
        }

        [Fact]
        public void ValidData_PassesValidation()
        {
            RegisterDTO dto = new RegisterDTO
            {
                first_name = "John",
                last_name = "Doe",
                username = "johndoe",
                email = "john@example.com",
                password = "StrongPass1!",
                password_confirmation = "StrongPass1!"
            };

            List<ValidationResult> results = Validate(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void PasswordMismatch_FailsValidation()
        {
            RegisterDTO dto = new RegisterDTO
            {
                first_name = "John",
                last_name = "Doe",
                username = "johndoe",
                email = "john@example.com",
                password = "StrongPass1!",
                password_confirmation = "OtherPass1!"
            };

            List<ValidationResult> results = Validate(dto);

            Assert.NotEmpty(results);
            Assert.Contains(results, result => result.ErrorMessage != null && result.ErrorMessage.Contains("Les mots de passe"));
        }

        private static List<ValidationResult> Validate(RegisterDTO dto)
        {
            List<ValidationResult> results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), results, true);

            if (isValid)
            {
                results.Clear();
            }

            return results;
        }
    }
}
