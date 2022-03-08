using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nitro.Core.Domain.Auth;
using Nitro.Core.Model.Auth;
using Nitro.Kernel.Interfaces;
using Nitro.Kernel.Models;

namespace Nitro.Web.Controllers.Auth
{
    [Authorize]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
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
                var user = new User {UserName = model.Email, Email = model.Email};
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
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new {userId = user.Id, code = code},
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
                            return Ok(result);
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
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new {ReturnUrl = returnUrl});
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
                    new ExternalLoginConfirmationModel {Email = email});
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
                    return Ok(result);
                }

                var user = new User {UserName = model.Email, Email = model.Email};
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
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                //var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                //await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                //   "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
                //return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }

            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/SendCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
            var factorOptions = userFactors.Select(purpose => new SelectListItem {Text = purpose, Value = purpose})
                .ToList();
            return View(new SendCodeViewModel
                {Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe});
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            if (model.SelectedProvider == "Authenticator")
            {
                return RedirectToAction(nameof(VerifyAuthenticatorCode),
                    new {ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe});
            }

            // Generate the token and send it
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                return View("Error");
            }

            var message = "Your security code is: " + code;
            if (model.SelectedProvider == "Email")
            {
                await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
            }
            else if (model.SelectedProvider == "Phone")
            {
                await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
            }

            return RedirectToAction(nameof(VerifyCode),
                new {Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe});
        }

        //
        // GET: /Account/VerifyCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            return View(new VerifyCodeViewModel {Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe});
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe,
                model.RememberBrowser);
            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return View(model);
            }
        }

        //
        // GET: /Account/VerifyAuthenticatorCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyAuthenticatorCode(bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            return View(new VerifyAuthenticatorCodeViewModel {ReturnUrl = returnUrl, RememberMe = rememberMe});
        }

        //
        // POST: /Account/VerifyAuthenticatorCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAuthenticatorCode(VerifyAuthenticatorCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result =
                await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe,
                    model.RememberBrowser);
            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return View(model);
            }
        }

        //
        // GET: /Account/UseRecoveryCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> UseRecoveryCode(string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            return View(new UseRecoveryCodeViewModel {ReturnUrl = returnUrl});
        }

        //
        // POST: /Account/UseRecoveryCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseRecoveryCode(UseRecoveryCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(model.Code);
            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return View(model);
            }
        }

        #region Helpers


        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        #endregion
    }
}