using FluentValidation;
using IGMS.Application.Common.Models;

namespace IGMS.Application.Common.Validators;

public class SaveRiskRequestValidator : AbstractValidator<SaveRiskRequest>
{
    public SaveRiskRequestValidator()
    {
        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("عنوان المخاطرة بالعربي مطلوب.")
            .MaximumLength(200).WithMessage("العنوان لا يتجاوز 200 حرف.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("رمز المخاطرة مطلوب.")
            .MaximumLength(20).WithMessage("الرمز لا يتجاوز 20 حرفاً.");

        RuleFor(x => x.Likelihood)
            .InclusiveBetween(1, 5).WithMessage("الاحتمالية يجب أن تكون بين 1 و 5.");

        RuleFor(x => x.Impact)
            .InclusiveBetween(1, 5).WithMessage("التأثير يجب أن يكون بين 1 و 5.");
    }
}

public class SavePolicyRequestValidator : AbstractValidator<SavePolicyRequest>
{
    public SavePolicyRequestValidator()
    {
        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("عنوان السياسة بالعربي مطلوب.")
            .MaximumLength(200).WithMessage("العنوان لا يتجاوز 200 حرف.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("رمز السياسة مطلوب.")
            .MaximumLength(20).WithMessage("الرمز لا يتجاوز 20 حرفاً.");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.EffectiveDate)
            .WithMessage("تاريخ الانتهاء يجب أن يكون بعد تاريخ السريان.")
            .When(x => x.EffectiveDate.HasValue && x.ExpiryDate.HasValue);
    }
}

public class SaveIncidentRequestValidator : AbstractValidator<SaveIncidentRequest>
{
    public SaveIncidentRequestValidator()
    {
        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("عنوان الحادثة بالعربي مطلوب.")
            .MaximumLength(200).WithMessage("العنوان لا يتجاوز 200 حرف.");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("مستوى الخطورة مطلوب.")
            .Must(v => new[] { "Low", "Medium", "High", "Critical" }.Contains(v))
            .WithMessage("مستوى الخطورة غير صالح.");

        RuleFor(x => x.OccurredAt)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("تاريخ الحادثة لا يمكن أن يكون في المستقبل.");
    }
}

public class SaveTaskRequestValidator : AbstractValidator<SaveTaskRequest>
{
    public SaveTaskRequestValidator()
    {
        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("عنوان المهمة بالعربي مطلوب.")
            .MaximumLength(200).WithMessage("العنوان لا يتجاوز 200 حرف.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("تاريخ الاستحقاق مطلوب.");
    }
}

public class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentRequestValidator()
    {
        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("اسم القسم بالعربي مطلوب.")
            .MaximumLength(100).WithMessage("الاسم لا يتجاوز 100 حرف.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("رمز القسم مطلوب.")
            .MaximumLength(20).WithMessage("الرمز لا يتجاوز 20 حرفاً.");
    }
}

public class UpdateDepartmentRequestValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرّف القسم غير صالح.");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("اسم القسم بالعربي مطلوب.")
            .MaximumLength(100).WithMessage("الاسم لا يتجاوز 100 حرف.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("رمز القسم مطلوب.")
            .MaximumLength(20).WithMessage("الرمز لا يتجاوز 20 حرفاً.");
    }
}

public class SaveKpiRequestValidator : AbstractValidator<SaveKpiRequest>
{
    public SaveKpiRequestValidator()
    {
        RuleFor(x => x.TitleAr)
            .NotEmpty().WithMessage("عنوان المؤشر بالعربي مطلوب.")
            .MaximumLength(200).WithMessage("العنوان لا يتجاوز 200 حرف.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("رمز المؤشر مطلوب.")
            .MaximumLength(20).WithMessage("الرمز لا يتجاوز 20 حرفاً.");

        RuleFor(x => x.TargetValue)
            .GreaterThan(0).WithMessage("القيمة المستهدفة يجب أن تكون أكبر من صفر.");
    }
}
