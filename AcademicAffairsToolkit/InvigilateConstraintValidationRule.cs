using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace AcademicAffairsToolkit
{
    class InvigilateConstraintValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if ((value as BindingGroup).Items[0] is InvigilateConstraint selected)
            {
                if (selected.From >= selected.To)
                {
                    return new ValidationResult(false, Resource.EndTimeEarlierThanStartTimeErrorTip);
                }
                else if (selected.TROffice == null)
                {
                    return new ValidationResult(false, Resource.EmptyFieldErrorTip);
                }
            }

            return ValidationResult.ValidResult;
        }
    }
}
