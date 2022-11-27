using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Nitro.Core.Domain.Auth;
using Nitro.Kernel.Interfaces;
using Nitro.Kernel.Models;
using System.Security.Claims;
using EFCoreSecondLevelCacheInterceptor;
using Nitro.Core.Data.Domain;
using Nitro.Core.Models.Auth;
using Nitro.Kernel.Interfaces.Data;
using Nitro.Service.MessageSender;

namespace Nitro.Web.Controllers.Auth
{
    [Authorize]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly IQueryRepository _repository;
        private readonly IStringLocalizer<SharedResource> _sharedlocalizer;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            IQueryRepository repository,
            IStringLocalizer<SharedResource> sharedlocalizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _repository = repository;
            _logger = loggerFactory.CreateLogger<AccountController>();

            _sharedlocalizer = sharedlocalizer;
        }

        [HttpPost(nameof(Initialize))]
        [AllowAnonymous]
        public async Task<IActionResult> Initialize()
        {
            try
            {

                var result = new AccountResult();
                if (ModelState.IsValid)
                {
                    var user = new User
                    { DOB = DateTime.Now, Name = "admin", UserName = "admin", Email = "admin@admin.com" };
                    var isExist = _repository.Table<User>().Any(x => x.UserName == "admin");
                    if (!isExist)
                    {
                        var identityResult = await _userManager.CreateAsync(user, "admin");
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

                                emailRequest.Subject = _sharedlocalizer["ConfirmEmail"];
                                emailRequest.Body =
                                    string.Format(
                                        _sharedlocalizer[
                                            "Please confirm your account by clicking this link: <a href='{0}'>link</a>"],
                                        callbackUrl);
                                emailRequest.ToEmail = "admin@admin.com";
                                try
                                {
                                    await _emailSender.SendEmailAsync(emailRequest);

                                    result.Status = AccountStatusEnum.Succeeded;
                                    return Ok(result);
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e.InnerException + "_" + e.Message);
                                    result.Errors.Add(string.Format(_sharedlocalizer["{0} action throws an error"]));
                                    return BadRequest(result);
                                }
                            }
                        }
                        if (identityResult.Errors.Any())
                        {
                            foreach (var error in identityResult.Errors)
                            {
                                _logger.LogError(_sharedlocalizer["{0}; Requested By: {1}"], error.Description,
                                    "admin@admin.com");
                                result.Errors.Add(error.Description);
                            }

                            _logger.LogError(_sharedlocalizer["The user could not create a new account.; Requested By: {0}"],
                                "admin@admin.com");
                            result.Status = AccountStatusEnum.Failed;

                            return BadRequest(result);

                        }
                        else
                        {
                            return Ok(result);
                        }
                    }


                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3,
                        _sharedlocalizer["The user created a new account with the password."]);
                    result.Status = AccountStatusEnum.Succeeded;
                    return Ok(result);

                }

                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                _logger.LogWarning(_sharedlocalizer["Input data are invalid.; Requested By: {0}"], "admin@admin.com");
                result.Status = AccountStatusEnum.Invalid;
                foreach (var error in errors)
                {
                    result.Errors.Add(error);
                }

                return BadRequest(result);
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpGet(nameof(Test))]
        [AllowAnonymous]
        public IActionResult Test()
        {
            var result = _repository.Table<Author>().Cacheable().ToList();
            return Ok(result);
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
                        _logger.LogInformation(_sharedlocalizer["User logged in with {0}."], model.Email);
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
                    result.Errors.Add(_sharedlocalizer["Invalid login attempt."]);
                    return BadRequest(result);
                }
            }

            var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

            _logger.LogWarning(_sharedlocalizer["Email or password are invalid.; Requested By: {0} "] , model.Email);
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

                        emailRequest.Subject = _sharedlocalizer["ConfirmEmail"];
                        emailRequest.Body = string.Format(_sharedlocalizer["Please confirm your account by clicking this link: <a href='{0}'>link</a>"], callbackUrl);
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
                            result.Errors.Add(string.Format(_sharedlocalizer["{0} action throws an error"]));
                            return BadRequest(result);
                        }
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, _sharedlocalizer["The user created a new account with the password."]);
                    result.Status = AccountStatusEnum.Succeeded;
                    return Ok(result);
                }

                _logger.LogError(_sharedlocalizer["The user could not create a new account.; Requested By: {0}"], model.Email);
                result.Status = AccountStatusEnum.Failed;
                foreach (var error in identityResult.Errors)
                {
                    _logger.LogError(_sharedlocalizer["{0}; Requested By: {1}"], error.Description, model.Email);
                    result.Errors.Add(error.Description);
                }

                return BadRequest(result);
            }

            var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
            _logger.LogWarning(_sharedlocalizer["Input data are invalid.; Requested By: {0}"] , model.Email);
            result.Status = AccountStatusEnum.Invalid;
            foreach (var error in errors)
            {
                result.Errors.Add(error);
            }

            return BadRequest(result);
        }

        [HttpPost(nameof(LogOff))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            var result = new AccountResult();
            await _signInManager.SignOutAsync();
            _logger.LogInformation(_sharedlocalizer["The user logged out."]);
            result.Status = AccountStatusEnum.Succeeded;
            return Ok(result);
        }

        [HttpPost(nameof(ExternalLogin))]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet(nameof(ExternalLoginCallback))]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            var result = new AccountResult();
            if (remoteError != null)
            {
                result.Status = AccountStatusEnum.ErrorExternalProvider;
                _logger.LogError(_sharedlocalizer["Error from external provider: {0}"], remoteError);
                result.Errors.Add(string.Format(_sharedlocalizer["Error from external provider: {0}"], remoteError));
                return BadRequest(result);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                result.Status = AccountStatusEnum.NullExternalLoginInfo;
                _logger.LogError(_sharedlocalizer["Could not get info from an external provider"]);
                result.Errors.Add(_sharedlocalizer["Could not get info from an external provider"]);
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
                _logger.LogInformation(_sharedlocalizer["User logged in with {0} provider."], info.LoginProvider);
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

                _logger.LogWarning(_sharedlocalizer["Redirect the user {0} to ExternalLoginConfirmation"], email);

                var redirectUrl = Url.Action("ExternalLoginConfirmation", "Account",
                    new ExternalLoginConfirmationModel { Email = email });
                return Redirect(redirectUrl);
            }
        }

        [HttpPost(nameof(ExternalLoginConfirmation))]
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
                    _logger.LogError(_sharedlocalizer["External Login Failure for user: {0}"], model.Email);
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
                            _sharedlocalizer["User created an account using {0} provider; Requested By: {1}"], info.LoginProvider,
                            model.Email);
                        result.Status = AccountStatusEnum.Succeeded;
                        // Update any authentication tokens as well
                        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                        return Ok(result);
                    }
                }

                _logger.LogError(_sharedlocalizer["The user could not create a new account after external login.;"] + model.Email);
                result.Status = AccountStatusEnum.Failed;
                foreach (var error in userManagerResult.Errors)
                {
                    _logger.LogError(error.Description + "; Requested By: " + model.Email);
                    result.Errors.Add(error.Description);
                }

                return BadRequest(result);
            }

            var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
            _logger.LogWarning(_sharedlocalizer["Input data are invalid.; Requested By: {0} "] , model.Email);
            result.Status = AccountStatusEnum.Invalid;
            foreach (var error in errors)
            {
                result.Errors.Add(error);
            }

            return BadRequest(result);
        }

        // GET: /Account/ConfirmEmail
        [HttpGet(nameof(ConfirmEmail))]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code, string returnUrl)
        {
            if (userId == null || code == null)
            {
                _logger.LogWarning(_sharedlocalizer["Input data are invalid.; Requested By: "] + userId);
                return Redirect(returnUrl + "&status=error");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning(_sharedlocalizer["Input data are invalid.; Requested By: "] + userId);
                return Redirect(returnUrl + "&status=error");
            }
            _logger.LogInformation(_sharedlocalizer["User confirmed the code.; Requested By: {0}"] , userId);
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return Redirect(returnUrl + "&status" + (result.Succeeded ? "succeeded" : "error"));
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost(nameof(ForgotPassword))]
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

                emailRequest.Subject = _sharedlocalizer["ConfirmEmail"];
                emailRequest.Body = string.Format( _sharedlocalizer["Please confirm your account by clicking this link: <a href='{0}'>link</a>"]);
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
                    result.Errors.Add(_sharedlocalizer["RequireConfirmedEmail action throw an error"]);
                    return BadRequest(result);
                }


            }

            // If we got this far, something failed, redisplay form
            var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
            _logger.LogWarning(_sharedlocalizer["Input data are invalid.; Requested By: "] + model.Email);
            result.Status = AccountStatusEnum.Invalid;
            foreach (var error in errors)
            {
                result.Errors.Add(error);
            }

            return BadRequest(result);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost(nameof(ResetPassword))]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            var result = new AccountResult();
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                _logger.LogWarning(_sharedlocalizer["Input data are invalid.; Requested By: "] + model.Email);
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
                _logger.LogWarning(_sharedlocalizer["Input data are invalid.; Requested By: "] + model.Email);
                result.Status = AccountStatusEnum.Failed;
                return BadRequest(result);
            }

            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (resetPasswordResult.Succeeded)
            {
                result.Status = AccountStatusEnum.Succeeded;
                return Ok(result);
            }

            _logger.LogError(_sharedlocalizer["{0} action does not Succeeded"], "ResetPasswordAsync");
            result.Status = AccountStatusEnum.Failed;
            result.Errors.Add(string.Format(_sharedlocalizer["{0} action does not Succeeded"], "ResetPasswordAsync"));
            return BadRequest(result);
        }

        //
        // GET: /Account/SendCode
        [HttpGet(nameof(GetTwoFactorProvidersAsync))]
        [AllowAnonymous]
        public async Task<ActionResult> GetTwoFactorProvidersAsync()
        {
            var result = new AccountResult();
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                _logger.LogWarning(_sharedlocalizer["Input data are invalid.; Requested By: "] + user.Email);
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
        [HttpPost(nameof(SendCode))]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(SendCodeModel model)
        {
            var result = new AccountResult();
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (!ModelState.IsValid)
            {
                _logger.LogError(_sharedlocalizer["Input data are invalid.; Requested By: "] + user ?? user.Email);
                result.Status = AccountStatusEnum.Invalid;
                result.Errors.Add("");

                return BadRequest(result);
            }

            if (user == null)
            {
                _logger.LogError(_sharedlocalizer["User not found.;"]);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(_sharedlocalizer["User not found.; "]);

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
                _logger.LogError(_sharedlocalizer["Token generator returned null.; Requested By: {0}"] , user.Email);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(string.Format(_sharedlocalizer["Token generator returned null.; Requested By: {0}"], user.Email));

                return BadRequest(result);
            }

            var message = _sharedlocalizer["Your security code is: "] + code;
            if (model.SelectedProvider == "Email")
            {
                var emailRequest = new EmailRequestRecord()
                {
                    ToEmail = await _userManager.GetEmailAsync(user),
                    Subject = _sharedlocalizer["Security Code"],
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
                    result.Errors.Add(_sharedlocalizer["Send email action throws an error"]);
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
                    result.Errors.Add(_sharedlocalizer["Send sms action throws an error"]);
                    return BadRequest(result);
                }
            }

            _logger.LogError(_sharedlocalizer["The operation failed.; Requested By: {0}"] , user.Email);
            result.Status = AccountStatusEnum.Failed;
            result.Errors.Add(string.Format(_sharedlocalizer["The operation failed.; Requested By: {0}"], user.Email));

            return BadRequest(result);
        }

        /// <summary>
        /// If Two Factor Authenticator is enabled, the user have to enter 6 digits code by authenticator app
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost(nameof(VerifyAuthenticatorCode))]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAuthenticatorCode(VerifyAuthenticatorCodeModel model)
        {
            var result = new AccountResult();
            if (!ModelState.IsValid)
            {
                _logger.LogError(_sharedlocalizer["Input data are invalid.; Requested By: "] + (await GetCurrentUserAsync()).Email);
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
                _logger.LogWarning(7, _sharedlocalizer["User account locked out..; Requested By: {0}"] , (await GetCurrentUserAsync()).Email);

                result.Status = AccountStatusEnum.IsLockedOut;
                return Ok(result);
            }
            else
            {
                _logger.LogWarning(_sharedlocalizer["Code is invalid.; Requested By: {0}"] , (await GetCurrentUserAsync()).Email);
                result.Status = AccountStatusEnum.InvalidCode;

                return BadRequest(result);
            }
        }

        /// <summary>
        /// If Two Factor is enabled, the user have to enter code which was sent ti them by email or sms
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost(nameof(VerifyCode))]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeModel model)
        {
            var result = new AccountResult();
            if (!ModelState.IsValid)
            {
                _logger.LogError(_sharedlocalizer["Input data are invalid.;"]);
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
        [HttpPost(nameof(UseRecoveryCode))]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseRecoveryCode(UseRecoveryCodeModel model)
        {
            var result = new AccountResult();
            if (!ModelState.IsValid)
            {
                _logger.LogError(_sharedlocalizer["Input data are invalid.;"]);
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