using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nitro.Core.Domain.Auth;
using Nitro.Core.Model.Auth;
using Nitro.Kernel.Interfaces;
using Nitro.Kernel.Models;
using System.Security.Claims;

namespace Nitro.Web.Controllers.Auth
{
    [Authorize]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<AccountController>();
        }

        [HttpPost(nameof(Login))]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var result = new AccountResult();
            if (ModelState.IsValid)
            {
                try
                {
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    var signInResult = await _signInManager.PasswordSignInAsync(model.Email, model.Password,
                        model.RememberMe, lockoutOnFailure: false);
                    if (signInResult.Succeeded)
                    {
                        result.Status = AccountStatusEnum.Succeeded;
                        _logger.LogInformation("User logged in with {Email}.", model.Email);
                        return Ok(result);
                    }

                    if (signInResult.RequiresTwoFactor)
                    {
                        result.Status = AccountStatusEnum.RequiresTwoFactor;
                        return Ok(result);
                    }

                    if (signInResult.IsLockedOut)
                    {
                        result.Status = AccountStatusEnum.IsLockedOut;
                        return Ok(result);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.InnerException + "_" + e.Message);
                    result.Status = AccountStatusEnum.Failed;
                    result.Errors.Add("Invalid login attempt.");
                    return BadRequest(result);
                }
            }

            var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

            _logger.LogWarning("Email or password are invalid.; Requested By: " + model.Email);
            result.Status = AccountStatusEnum.Invalid;
            foreach (var error in errors)
            {
                result.Errors.Add(error);
            }

            return BadRequest(result);
        }

        [HttpPost(nameof(Register))]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var result = new AccountResult();
            if (ModelState.IsValid)
            {
                var user = new User { UserName = model.Email, Email = model.Email };
                var identityResult = await _userManager.CreateAsync(user, model.Password);
                if (identityResult.Succeeded)
                {
                    if (_userManager.Options.SignIn.RequireConfirmedEmail)
                    {
                        result.Status = AccountStatusEnum.RequireConfirmedEmail;
                        var emailRequest = new EmailRequestRecord();
                        //For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                        //Send an email with this link
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account",
                            new { userId = user.Id, code = code, returnUrl = "" },
                            protocol: HttpContext.Request.Scheme);

                        emailRequest.Subject = "ConfirmEmail";
                        emailRequest.Body = "Please confirm your account by clicking this link: <a href=\"" +
                                            callbackUrl + "\">link</a>";
                        emailRequest.ToEmail = model.Email;
                        try
                        {
                            await _emailSender.SendEmailAsync(emailRequest);

                            result.Status = AccountStatusEnum.Succeeded;
                            return Ok(result);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e.InnerException + "_" + e.Message);
                            result.Errors.Add("RequireConfirmedEmail action throw an error");
                            return BadRequest(result);
                        }
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User created a new account with password.");
                    result.Status = AccountStatusEnum.Succeeded;
                    return Ok(result);
                }

                _logger.LogError("User could not create a new account.; Requested By: " + model.Email);
                result.Status = AccountStatusEnum.Failed;
                foreach (var error in identityResult.Errors)
                {
                    _logger.LogError(error.Description + "; Requested By: " + model.Email);
                    result.Errors.Add(error.Description);
                }

                return BadRequest(result);
            }

            var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
            _logger.LogWarning("Input data are invalid.; Requested By: " + model.Email);
            result.Status = AccountStatusEnum.Invalid;
            foreach (var error in errors)
            {
                result.Errors.Add(error);
            }

            return BadRequest(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            var result = new AccountResult();
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            result.Status = AccountStatusEnum.Succeeded;
            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            var result = new AccountResult();
            if (remoteError != null)
            {
                result.Status = AccountStatusEnum.ErrorExternalProvider;
                _logger.LogError($"Error from external provider: {remoteError}");
                result.Errors.Add($"Error from external provider: {remoteError}");
                return BadRequest(result);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                result.Status = AccountStatusEnum.NullExternalLoginInfo;
                _logger.LogError("Could not get info from external provider");
                result.Errors.Add("Could not get info from external provider");
                return BadRequest(result);
            }

            // Sign in the user with this external login provider if the user already has a login.
            var externalLoginResult =
                await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
                    isPersistent: false);
            if (externalLoginResult.Succeeded)
            {
                // Update any authentication tokens if login succeeded
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                result.Status = AccountStatusEnum.Succeeded;
                return Ok(result);
            }

            if (externalLoginResult.RequiresTwoFactor)
            {
                result.Status = AccountStatusEnum.RequiresTwoFactor;
                return Ok(result);
            }

            if (externalLoginResult.IsLockedOut)
            {
                result.Status = AccountStatusEnum.IsLockedOut;
                return Ok(result);
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.e;

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                _logger.LogWarning("Redirect the user {Email} to ExternalLoginConfirmation", email);

                var redirectUrl = Url.Action("ExternalLoginConfirmation", "Account",
                    new ExternalLoginConfirmationModel { Email = email });
                return Redirect(redirectUrl);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationModel model)
        {
            var result = new AccountResult();
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    result.Status = AccountStatusEnum.ExternalLoginFailure;
                    _logger.LogError("External Login Failure for user : {Email}", model.Email);
                    return BadRequest(result);
                }

                var user = new User { UserName = model.Email, Email = model.Email };
                var userManagerResult = await _userManager.CreateAsync(user);
                if (userManagerResult.Succeeded)
                {
                    userManagerResult = await _userManager.AddLoginAsync(user, info);
                    if (userManagerResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation(6,
                            "User created an account using {Name} provider; Requested By: {Email}", info.LoginProvider,
                            model.Email);
                        result.Status = AccountStatusEnum.Succeeded;
                        // Update any authentication tokens as well
                        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                        return Ok(result);
                    }
                }

                _logger.LogError("User could not create a new account after external login.;" + model.Email);
                result.Status = AccountStatusEnum.Failed;
                foreach (var error in userManagerResult.Errors)
                {
                    _logger.LogError(error.Description + "; Requested By: " + model.Email);
                    result.Errors.Add(error.Description);
                }

                return BadRequest(result);
            }

            var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
            _logger.LogWarning("Input data are invalid.; Requested By: " + model.Email);
            result.Status = AccountStatusEnum.Invalid;
            foreach (var error in errors)
            {
                result.Errors.Add(error);
            }

            return BadRequest(result);
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code, string returnUrl)
        {
            if (userId == null || code == null)
            {
                _logger.LogWarning("Input data are invalid.; Requested By: " + userId);
                return Redirect(returnUrl + "&status=error");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Input data are invalid.; Requested By: " + userId);
                return Redirect(returnUrl + "&status=error");
            }
            _logger.LogInformation("User confirmed the code.; Requested By: " + userId);
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return Redirect(returnUrl + "&status" + (result.Succeeded ? "succeeded" : "error"));
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            var result = new AccountResult();
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    result.Status = AccountStatusEnum.Failed;
                    return BadRequest(result);
                }

                result.Status = AccountStatusEnum.RequireConfirmedEmail;

                var emailRequest = new EmailRequestRecord();
                //For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                //Send an email with this link
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "Account",
                    new { userId = user.Id, code = code, returnUrl = "" },
                    protocol: HttpContext.Request.Scheme);

                emailRequest.Subject = "ConfirmEmail";
                emailRequest.Body = "Please confirm your account by clicking this link: <a href=\"" +
                                    callbackUrl + "\">link</a>";
                emailRequest.ToEmail = model.Email;
                try
                {
                    await _emailSender.SendEmailAsync(emailRequest);

                    result.Status = AccountStatusEnum.Succeeded;
                    return Ok(result);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.InnerException + "_" + e.Message);
                    result.Errors.Add("RequireConfirmedEmail action throw an error");
                    return BadRequest(result);
                }


            }

            // If we got this far, something failed, redisplay form
            var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
            _logger.LogWarning("Input data are invalid.; Requested By: " + model.Email);
            result.Status = AccountStatusEnum.Invalid;
            foreach (var error in errors)
            {
                result.Errors.Add(error);
            }

            return BadRequest(result);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            var result = new AccountResult();
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                _logger.LogWarning("Input data are invalid.; Requested By: " + model.Email);
                result.Status = AccountStatusEnum.Invalid;
                foreach (var error in errors)
                {
                    result.Errors.Add(error);
                }

                return BadRequest(result);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                _logger.LogWarning("Input data are invalid.; Requested By: " + model.Email);
                result.Status = AccountStatusEnum.Failed;
                return BadRequest(result);
            }

            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (resetPasswordResult.Succeeded)
            {
                result.Status = AccountStatusEnum.Succeeded;
                return Ok(result);
            }

            _logger.LogError("ResetPasswordAsync action does not Succeeded");
            result.Status = AccountStatusEnum.Failed;
            result.Errors.Add("ResetPasswordAsync action does not Succeeded");
            return BadRequest(result);
        }

        //
        // GET: /Account/SendCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetTwoFactorProvidersAsync()
        {
            var result = new AccountResult();
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                _logger.LogWarning("Input data are invalid.; Requested By: " + user.Email);
                result.Status = AccountStatusEnum.Failed;
                return BadRequest(result);
            }

            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
            var factorOptions = userFactors.Select(purpose => new Kernel.Models.SelectListItem { Text = purpose, Value = purpose })
                .ToList();
            return Ok(new SendCodeModel
            { Providers = factorOptions });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(SendCodeModel model)
        {
            var result = new AccountResult();
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (!ModelState.IsValid)
            {
                _logger.LogError("Input data are invalid.; Requested By: " + user ?? user.Email);
                result.Status = AccountStatusEnum.Invalid;
                result.Errors.Add("");

                return BadRequest(result);
            }

            if (user == null)
            {
                _logger.LogError("User not found.;");
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add("User not found.; ");

                return NotFound(result);
            }

            if (model.SelectedProvider == "Authenticator")
            {
                // The following code protects for brute force attacks against the two factor codes.
                // If a user enters incorrect codes for a specified amount of time then the user account
                // will be locked out for a specified amount of time.
                result.Status = AccountStatusEnum.RequiresTwoFactor;
                return Ok(result);
            }

            // Generate the token and send it
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogError("Token generator returned null.; Requested By: " + user.Email);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add("Token generator returned null.; Requested By: " + user.Email);

                return BadRequest(result);
            }

            var message = "Your security code is: " + code;
            if (model.SelectedProvider == "Email")
            {
                var emailRequest = new EmailRequestRecord()
                {
                    ToEmail = await _userManager.GetEmailAsync(user),
                    Subject = "Security Code",
                    Body = message
                };

                try
                {
                    await _emailSender.SendEmailAsync(emailRequest);

                    result.Status = AccountStatusEnum.Succeeded;
                    return Ok(result);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.InnerException + "_" + e.Message);
                    result.Errors.Add("Send email action throw an error");
                    return BadRequest(result);
                }
            }
            else if (model.SelectedProvider == "Phone")
            {
                var smsRequest = new SmsRequestRecord()
                {
                    ToNumber = await _userManager.GetPhoneNumberAsync(user),
                    Message = message
                };
                try
                {
                    await _smsSender.SendSmsAsync(smsRequest);

                    result.Status = AccountStatusEnum.Succeeded;
                    return Ok(result);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.InnerException + "_" + e.Message);
                    result.Errors.Add("Send sms action throw an error");
                    return BadRequest(result);
                }
            }

            _logger.LogError("The operation failed.; Requested By: " + user.Email);
            result.Status = AccountStatusEnum.Failed;
            result.Errors.Add("The operation failed.; Requested By: " + user.Email);

            return BadRequest(result);
        }

        /// <summary>
        /// If Two Factor Authenticator is enabled, the user have to enter 6 digits code by authenticator app
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAuthenticatorCode(VerifyAuthenticatorCodeModel model)
        {
            var result = new AccountResult();
            if (!ModelState.IsValid)
            {
                _logger.LogError("Input data are invalid.; Requested By: " + (await GetCurrentUserAsync()).Email);
                result.Status = AccountStatusEnum.Invalid;
                result.Errors.Add("");

                return BadRequest(result);
            }
            var signInResult =
              await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe,
                  model.RememberBrowser);
            if (signInResult.Succeeded)
            {
                result.Status = AccountStatusEnum.Succeeded;
                return Ok(result);
            }

            if (signInResult.IsLockedOut)
            {
                _logger.LogWarning(7, "User account locked out..; Requested By: " + (await GetCurrentUserAsync()).Email);

                result.Status = AccountStatusEnum.IsLockedOut;
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Code is invalid.; Requested By: " + (await GetCurrentUserAsync()).Email);
                result.Status = AccountStatusEnum.InvalidCode;

                return BadRequest(result);
            }
        }

        /// <summary>
        /// If Two Factor is enabled, the user have to enter code which was sent ti them by email or sms
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeModel model)
        {
            var result = new AccountResult();
            if (!ModelState.IsValid)
            {
                _logger.LogError("Input data are invalid.;");
                result.Status = AccountStatusEnum.Invalid;
                result.Errors.Add("");

                return BadRequest(result);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var signInResult = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe,
                model.RememberBrowser);
            if (signInResult.Succeeded)
            {
                result.Status = AccountStatusEnum.Succeeded;
                return Ok(result);
            }

            if (signInResult.IsLockedOut)
            {
                result.Status = AccountStatusEnum.IsLockedOut;
                return Ok(result);
            }
            else
            {
                result.Status = AccountStatusEnum.InvalidCode;
                return Ok(result);
            }
        }

        /// <summary>
        /// The user can login by Two Factor Recovery code 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseRecoveryCode(UseRecoveryCodeModel model)
        {
            var result = new AccountResult();
            if (!ModelState.IsValid)
            {
                _logger.LogError("Input data are invalid.;");
                result.Status = AccountStatusEnum.Invalid;
                result.Errors.Add("");

                return BadRequest(result);
            }

            var signInResult = await _signInManager.TwoFactorRecoveryCodeSignInAsync(model.Code);
            if (signInResult.Succeeded)
            {
                result.Status = AccountStatusEnum.Succeeded;
                return Ok(result);
            }
            else
            {
                result.Status = AccountStatusEnum.InvalidCode;
                return Ok(result);
            }
        }

        #region Helpers


        private Task<User> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        #endregion
    }
}