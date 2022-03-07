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
                result.Errors.Add("error");
            }

            return BadRequest(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");
            return Ok();
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
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                // Update any authentication tokens if login succeeded
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                _logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                return Redirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["ProviderDisplayName"] = info.ProviderDisplayName;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
            }
        }


    }
}
