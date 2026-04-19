using FluentValidation;
using IGMS.Application.Common.Models;

namespace IGMS.Application.Common.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة.");
    }
}

public class VerifyOtpRequestValidator : AbstractValidator<VerifyOtpRequest>
{
    public VerifyOtpRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("معرّف المستخدم غير صالح.");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("رمز التحقق مطلوب.")
            .Length(6).WithMessage("رمز التحقق يجب أن يتكون من 6 أرقام.")
            .Matches(@"^\d{6}$").WithMessage("رمز التحقق أرقام فقط.");
    }
}
