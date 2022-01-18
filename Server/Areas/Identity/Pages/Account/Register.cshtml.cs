﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using nexRemoteFree.Server.Services;
using nexRemoteFree.Shared.Models;

namespace nexRemoteFree.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<nexRemoteFreeUser> _signInManager;
        private readonly UserManager<nexRemoteFreeUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSenderEx _emailSender;
        private readonly IDataService _dataService;
        private readonly IApplicationConfig _appConfig;

        public RegisterModel(
            UserManager<nexRemoteFreeUser> userManager,
            SignInManager<nexRemoteFreeUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSenderEx emailSender,
            IDataService dataService,
            IApplicationConfig appConfig)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _dataService = dataService;
            _appConfig = appConfig;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public int OrganizationCount { get; set; }
        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "E-mail")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "{0} musi mieć co najmniej {2} i maksymalnie {1} znaków.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Hasło")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Potwierdź hasło")]
            [Compare("Password", ErrorMessage = "Hasła różnią się od siebie.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            OrganizationCount = _dataService.GetOrganizationCount();
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            var organizationCount = _dataService.GetOrganizationCount();
            if (_appConfig.MaxOrganizationCount > 0 && organizationCount >= _appConfig.MaxOrganizationCount)
            {
                return NotFound();
            }

            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new nexRemoteFreeUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    IsServerAdmin = organizationCount == 0,
                    Organization = new Organization(),
                    UserOptions = new nexRemoteFreeUserOptions(),
                    IsAdministrator = true
                };

                do
                {
                    user.Organization.RelayCode = new string(Guid.NewGuid().ToString().Take(4).ToArray());
                }
                while (await _dataService.GetOrganizationByRelayCode(user.Organization.RelayCode) != null);

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Utworzono nowe konto z hasłem.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Potwierdź swój e-mail",
                        $"Prosimy o potwierdzenie konta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Klikając tutaj</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
