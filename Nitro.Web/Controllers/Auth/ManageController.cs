using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nitro.Core.Domain.Auth;
using Nitro.Core.Models.Auth;
using Nitro.Kernel.Interfaces;
using Nitro.Kernel.Models;

namespace Nitro.Web.Controllers.Auth
{
    [AllowAnonymous]
    [ApiController]
    [Route("Manage/[controller]")]
    public class ManageController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<SharedResource> _sharedlocalizer;

        public ManageController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            IStringLocalizer<SharedResource> sharedlocalizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _sharedlocalizer = sharedlocalizer;
        }
        //
        // POST: /Manage/RemoveLogin
        [HttpPost(nameof(RemoveLogin))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginModel account)
        {
            var result = new AccountResult();
            var user = await GetCurrentUserAsync();
            {
                var userManagerResult = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
                if (userManagerResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    result.Status = AccountStatusEnum.Succeeded;
                    return Ok(result);
                }

            }
            result.Status = AccountStatusEnum.Failed;
            _logger.LogError(_sharedlocalizer["Remove login async operation failed"]);
            return Ok(result);
        }

        // POST: /Manage/AddPhoneNumber
        [HttpPost(nameof(AddPhoneNumber))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberModel model)
        {
            var result = new AccountResult();
            var user = await GetCurrentUserAsync();

            if (user.Email == null)
            {
                _logger.LogError(_sharedlocalizer["User not found.;"]);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(_sharedlocalizer["User not found.; "]);

                return NotFound(result);
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                _logger.LogWarning(_sharedlocalizer["Input data are invalid.; Requested By: "] + user?.Email);
                result.Status = AccountStatusEnum.Invalid;
                foreach (var error in errors)
                {
                    result.Errors.Add(error);
                }

                return BadRequest(result);
            }

            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);

            var smsRequest = new SmsRequestRecord()
            {
                ToNumber = model.PhoneNumber,
                Message = _sharedlocalizer["Your security code is: "] + code
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
                result.Errors.Add(_sharedlocalizer["Send sms action throw an error"]);
                return BadRequest(result);
            }
        }


        //
        // POST: /Manage/ResetAuthenticatorKey
        [HttpPost(nameof(ResetAuthenticatorKey))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAuthenticatorKey()
        {
            var result = new AccountResult();
            var user = await GetCurrentUserAsync();
            if (user.Email == null)
            {
                _logger.LogError(_sharedlocalizer["User not found.;"]);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(_sharedlocalizer["User not found.; "]);

                return NotFound(result);
            }

            await _userManager.ResetAuthenticatorKeyAsync(user);
            _logger.LogInformation(1, _sharedlocalizer["User reset authenticator key."]);

            result.Status = AccountStatusEnum.Succeeded;
            return Ok(result);
        }

        // POST: /Manage/GenerateRecoveryCode
        [HttpPost(nameof(GenerateRecoveryCode))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRecoveryCode()
        {
            var result = new AccountResult();
            var user = await GetCurrentUserAsync();
            if (user.Email == null)
            {
                _logger.LogError(_sharedlocalizer["User not found.;"]);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(_sharedlocalizer["User not found.; "]);

                return NotFound(result);
            }

            var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 5);
                _logger.LogInformation(1, _sharedlocalizer["User generated new recovery code."]);

                return Ok(new RecoveryCodesModel { Codes = codes });
            
        }

        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost(nameof(EnableTwoFactorAuthentication))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            var result = new AccountResult();
            var user = await GetCurrentUserAsync();
            if (user.Email == null)
            {
                _logger.LogError(_sharedlocalizer["User not found.;"]);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(_sharedlocalizer["User not found.; "]);

                return NotFound(result);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation(1, _sharedlocalizer["User enabled two-factor authentication."]);

            result.Status = AccountStatusEnum.Succeeded;
            return Ok(result);
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost(nameof(DisableTwoFactorAuthentication))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactorAuthentication()
        {
            var result = new AccountResult();
            var user = await GetCurrentUserAsync();
            if (user.Email == null)
            {
                _logger.LogError(_sharedlocalizer["User not found.;"]);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(_sharedlocalizer["User not found.; "]);

                return NotFound(result);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation(2, _sharedlocalizer["User disabled two-factor authentication."]);

            result.Status = AccountStatusEnum.Succeeded;
            return Ok(result);
        }
        // GET: /Manage/VerifyPhoneNumber
        [HttpGet(nameof(VerifyPhoneNumber))]
        public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var result = new AccountResult();
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(await GetCurrentUserAsync(), phoneNumber);
            
            // Send an SMS to verify the phone number
            var smsRequest = new SmsRequestRecord()
            {
                ToNumber = phoneNumber,
                Message = _sharedlocalizer["Your security code is: "] + code
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
                result.Errors.Add(_sharedlocalizer["Send sms action throw an error"]);
                return BadRequest(result);
            }
        }

        // POST: /Manage/VerifyPhoneNumber
        [HttpPost(nameof(VerifyPhoneNumber))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberModel model)
        {
            var result = new AccountResult();

            var user = await GetCurrentUserAsync();
            if (user.Email == null)
            {
                _logger.LogError(_sharedlocalizer["User not found.;"]);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(_sharedlocalizer["User not found.; "]);

                return NotFound(result);
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                _logger.LogWarning(_sharedlocalizer["Input data are invalid.; Requested By: "] + user?.Email);
                result.Status = AccountStatusEnum.Invalid;
                foreach (var error in errors)
                {
                    result.Errors.Add(error);
                }

                return BadRequest(result);
            }

            var userManagerResult = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.Code);
            if (userManagerResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                result.Status = AccountStatusEnum.Succeeded;
                return Ok(result);
            }

            // If we got this far, something failed, redisplay the form
            _logger.LogError(_sharedlocalizer["Failed to verify phone number"]);
            result.Errors.Add(_sharedlocalizer["Failed to verify phone number"]);
            result.Status = AccountStatusEnum.Failed;
            return BadRequest(result);
        }

        // GET: /Manage/RemovePhoneNumber
        [HttpPost(nameof(RemovePhoneNumber))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePhoneNumber()
        {
            var result = new AccountResult();

            var user = await GetCurrentUserAsync();

            if (user.Email == null)
            {
                _logger.LogError(_sharedlocalizer["User not found.;"]);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(_sharedlocalizer["User not found.; "]);

                return NotFound(result);
            }

            var userManagerResult = await _userManager.SetPhoneNumberAsync(user, null);
            if (userManagerResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                result.Status = AccountStatusEnum.Succeeded;
                return Ok(result);
            }

            // If we got this far, something failed, redisplay the form
            _logger.LogError(_sharedlocalizer["Failed to remove phone number"]);
            result.Errors.Add(_sharedlocalizer["Failed to remove phone number"]);
            result.Status = AccountStatusEnum.Failed;
            return BadRequest(result);
        }

        // POST: /Manage/ChangePassword
        [HttpPost(nameof(ChangePassword))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            var result = new AccountResult();

            var user = await GetCurrentUserAsync();

            if (user.Email == null)
            {
                _logger.LogError(_sharedlocalizer["User not found.;"]);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(_sharedlocalizer["User not found.; "]);

                return NotFound(result);
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                _logger.LogWarning(_sharedlocalizer["Input data are invalid.; Requested By: "] + user?.Email);
                result.Status = AccountStatusEnum.Invalid;
                foreach (var error in errors)
                {
                    result.Errors.Add(error);
                }

                return BadRequest(result);
            }

            var userManagerResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (userManagerResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(3, _sharedlocalizer["User changed their password successfully."]);
                result.Status = AccountStatusEnum.Succeeded;
                return Ok(result);
            }

            // If we got this far, something failed, redisplay the form
            _logger.LogError(_sharedlocalizer["Failed to change password"]);
            result.Errors.Add(_sharedlocalizer["Failed to change password"]);
            result.Status = AccountStatusEnum.Failed;
            return BadRequest(result);
        }
        // POST: /Manage/SetPassword
        [HttpPost(nameof(SetPassword))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordModel model)
        {
            var result = new AccountResult();

            var user = await GetCurrentUserAsync();

            if (user.Email == null)
            {
                _logger.LogError(_sharedlocalizer["User not found.;"]);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(_sharedlocalizer["User not found.; "]);

                return NotFound(result);
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                _logger.LogWarning(_sharedlocalizer["Input data are invalid.; Requested By: "] + user?.Email);
                result.Status = AccountStatusEnum.Invalid;
                foreach (var error in errors)
                {
                    result.Errors.Add(error);
                }

                return BadRequest(result);
            }

            var userManagerResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (userManagerResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                result.Status = AccountStatusEnum.Succeeded;
                return Ok(result);
            }

            // If we got this far, something failed, redisplay the form
            _logger.LogError(_sharedlocalizer["Failed to change password"]);
            result.Errors.Add(_sharedlocalizer["Failed to change password"]);
            result.Status = AccountStatusEnum.Failed;
            return BadRequest(result);
        }

        //GET: /Manage/ManageLogins
        [HttpGet(nameof(ManageLogins))]
        public async Task<IActionResult> ManageLogins()
        {
            var result = new AccountResult();

            var user = await GetCurrentUserAsync();

            if (user.Email == null)
            {
                _logger.LogError(_sharedlocalizer["User not found.;"]);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(_sharedlocalizer["User not found.; "]);

                return NotFound(result);
            }

            var userLogins = await _userManager.GetLoginsAsync(user);
            var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
            var otherLogins = schemes.Where(auth => userLogins.All(ul => auth.Name != ul.LoginProvider)).ToList();

            return Ok(new ManageLoginsModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }
        // POST: /Manage/LinkLogin
        [HttpPost(nameof(LinkLogin))]
        [ValidateAntiForgeryToken]
        public IActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action("LinkLoginCallback", "Manage");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return Challenge(properties, provider);
        }
        // GET: /Manage/LinkLoginCallback
        [HttpGet(nameof(RemoveLogin))]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var result = new AccountResult();

            var user = await GetCurrentUserAsync();

            if (user.Email == null)
            {
                _logger.LogError(_sharedlocalizer["User not found.;"]);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(_sharedlocalizer["User not found.; "]);

                return NotFound(result);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
            if (info == null)
            {
                _logger.LogError(_sharedlocalizer["External login info not found.;Requested by:"] + user.Email);
                result.Status = AccountStatusEnum.Failed;
                result.Errors.Add(_sharedlocalizer["External login info not found.;Requested by:"] + user.Email);

                return NotFound(result);
            }

            var userManagerResult = await _userManager.AddLoginAsync(user, info);
            if (userManagerResult.Succeeded)
            {
                result.Status = AccountStatusEnum.Succeeded;
                return Ok(result);
            }

            // If we got this far, something failed, redisplay the form
            _logger.LogError(_sharedlocalizer["Add login operation failed."]);
            result.Errors.Add(_sharedlocalizer["Add login operation failed."]);
            result.Status = AccountStatusEnum.Failed;
            return BadRequest(result);

        }

        private Task<User> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }
}
