// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ProjectWebApp.Models;
using Microsoft.EntityFrameworkCore;
using ProjectWebApp.Data;

namespace ProjectWebApp.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _dbContext;


        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, ApplicationDbContext dbContext)
        {
            _signInManager = signInManager;
            _logger = logger;
            _dbContext = dbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string CustomUserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.CustomUserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    await UpdateLoginStreakAsync(_signInManager.UserManager.GetUserId(User));
                    returnUrl = Url.Action("Index", "Dashboard");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private async Task UpdateLoginStreakAsync(string userId)
        {
            var currentDate = DateTime.UtcNow.Date;

            // Check if the user has already logged in today
            var hasLoggedToday = await _dbContext.LoginStreaks
                .AnyAsync(ls => ls.UserId == userId && ls.LastLoginTime.Date == currentDate);

            if (!hasLoggedToday)
            {
                var userLoginStreaks = await _dbContext.LoginStreaks
                    .Where(ls => ls.UserId == userId)
                    .OrderByDescending(ls => ls.LastLoginTime)
                    .ToListAsync();

                // Create a new instance of LoginStreak with updated values
                var newLoginStreak = new LoginStreak
                {
                    UserId = userId,
                    CurrentStreak = 1,
                    LastLoginTime = DateTime.UtcNow
                };

                if (userLoginStreaks.Any())
                {
                    var previousStreak = userLoginStreaks.First();

                    // Check if the user logged in on a different day
                    var daysBetween = (currentDate - previousStreak.LastLoginTime.Date).Days;

                    if (daysBetween == 0)
                    {
                        // Increment the current streak
                        newLoginStreak.CurrentStreak = previousStreak.CurrentStreak + 1;

                        // Check if today's streak is greater than the historical longest streak
                        if (newLoginStreak.CurrentStreak > previousStreak.LongestStreak)
                        {
                            // Update the longest streak
                            newLoginStreak.LongestStreak = newLoginStreak.CurrentStreak;
                        }
                        else
                        {
                            // Keep the historical longest streak
                            newLoginStreak.LongestStreak = previousStreak.LongestStreak;
                        }
                    }
                    else if (daysBetween == 1)
                    {
                        // Increment the current streak if there's a login from the previous day
                        newLoginStreak.CurrentStreak = previousStreak.CurrentStreak + 1;
                    }
                    // else: Reset streak if the user didn't log in consecutively
                }

                // Add the new entity to the context
                _dbContext.LoginStreaks.Add(newLoginStreak);

                // Save changes to the database
                await _dbContext.SaveChangesAsync();
            }
        }





    }

}
