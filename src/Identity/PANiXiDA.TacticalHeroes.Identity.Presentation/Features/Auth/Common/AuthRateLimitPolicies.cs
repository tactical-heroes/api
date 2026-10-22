namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

internal static class AuthRateLimitPolicies
{
    public const string Login = "Auth.Login";
    public const string Register = "Auth.Register";
    public const string ForgotPassword = "Auth.ForgotPassword";
    public const string ResetPassword = "Auth.ResetPassword";
    public const string ResendConfirmationEmail = "Auth.ResendConfirmationEmail";
    public const string ConfirmEmail = "Auth.ConfirmEmail";
    public const string ChangePassword = "Auth.ChangePassword";
}
