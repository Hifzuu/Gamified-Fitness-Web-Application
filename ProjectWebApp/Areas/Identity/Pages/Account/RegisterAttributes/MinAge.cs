using System;
using System.ComponentModel.DataAnnotations;

public class MinAge : ValidationAttribute
{
    private readonly int _minimumAge;

    public MinAge(int minimumAge)
    {
        _minimumAge = minimumAge;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime dateOfBirth)
        {
            if (CalculateAge(dateOfBirth) >= _minimumAge)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult($"You must be at least {_minimumAge} years old.");
            }
        }

        return new ValidationResult("Invalid date of birth.");
    }

    private int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;

        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
