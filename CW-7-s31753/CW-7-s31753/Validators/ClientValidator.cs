using FluentValidation;
using CW_7_s31753.Models; 

namespace CW_7_s31753.Validators
{
    public class ClientValidator : AbstractValidator<Client>
    {
        public ClientValidator()
        {
            RuleFor(c => c.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");
                
            RuleFor(c => c.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");
                
            RuleFor(c => c.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
                
            RuleFor(c => c.Telephone)
                .NotEmpty().WithMessage("Telephone is required")
                .MaximumLength(20).WithMessage("Telephone cannot exceed 20 characters");
                
            RuleFor(c => c.Pesel)
                .NotEmpty().WithMessage("PESEL is required")
                .Length(11).WithMessage("PESEL must be 11 characters")
                .Matches("^[0-9]*$").WithMessage("PESEL can only contain numbers");
        }
    }
}