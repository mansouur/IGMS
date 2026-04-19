using FluentValidation;
using IGMS.Application.Common.Models;

namespace IGMS.Application.Common.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب.")
            .MinimumLength(3).WithMessage("اسم المستخدم يجب أن يكون 3 أحرف على الأقل.")
            .MaximumLength(50).WithMessage("اسم المستخدم لا يتجاوز 50 حرفاً.")
            .Matches(@"^[a-z0-9._-]+$").WithMessage("اسم المستخدم يقبل أحرف إنجليزية صغيرة وأرقام ونقطة وشرطة فقط.");

        RuleFor(x => x.FullNameAr)
            .NotEmpty().WithMessage("الاسم بالعربي مطلوب.")
            .MaximumLength(100).WithMessage("الاسم بالعربي لا يتجاوز 100 حرف.");

        RuleFor(x => x.FullNameEn)
            .MaximumLength(100).WithMessage("الاسم بالإنجليزي لا يتجاوز 100 حرف.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل.")
            .When(x => !string.IsNullOrWhiteSpace(x.Password));

        RuleFor(x => x.EmiratesId)
            .MaximumLength(20).WithMessage("رقم الهوية الإماراتية لا يتجاوز 20 خانة.")
            .When(x => !string.IsNullOrWhiteSpace(x.EmiratesId));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("رقم الهاتف لا يتجاوز 20 خانة.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرّف المستخدم غير صالح.");

        RuleFor(x => x.FullNameAr)
            .NotEmpty().WithMessage("الاسم بالعربي مطلوب.")
            .MaximumLength(100).WithMessage("الاسم بالعربي لا يتجاوز 100 حرف.");

        RuleFor(x => x.FullNameEn)
            .MaximumLength(100).WithMessage("الاسم بالإنجليزي لا يتجاوز 100 حرف.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

        RuleFor(x => x.EmiratesId)
            .MaximumLength(20).WithMessage("رقم الهوية الإماراتية لا يتجاوز 20 خانة.")
            .When(x => !string.IsNullOrWhiteSpace(x.EmiratesId));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("رقم الهاتف لا يتجاوز 20 خانة.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
