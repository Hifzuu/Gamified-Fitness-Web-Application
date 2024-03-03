// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ProjectWebApp.Models;

namespace ProjectWebApp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public int Step { get; set; } = 0;

        public class InputModel
        {
            public InputModel()
            {
                SelectedGoals = new List<string>();
                ExerciseFocus = new List<string>();
            }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Select Goals")]
            public List<string> SelectedGoals { get; set; }

            [Required]
            [Display(Name = "Exercise Focus")]
            public List<string> ExerciseFocus { get; set; }

            public List<SelectListItem> Goals { get; set; }
            public List<SelectListItem> ExerciseFocusOptions { get; set; }

            [Required]
            [Display(Name = "Gender")]
            public string Gender { get; set; }

            [Required]
            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }

            [Required]
            [Display(Name = "Country")]
            public string Country { get; set; }

            [Required]
            [Display(Name = "Height (cm)")]
            public int Height { get; set; }

            [Required]
            [Display(Name = "Starting Weight (kg)")]
            public double InitialWeightEntry { get; set; }

            [Required]
            [Display(Name = "Goal Weight (kg)")]
            public double GoalWeight { get; set; }

            [Required]
            [StringLength(25, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Only alphanumeric characters are allowed.")]
            [Display(Name = "Username")]
            public string CustomUserName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            try
            {
                _logger.LogInformation("OnGetAsync: Start");

                ReturnUrl = returnUrl;
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                _logger.LogInformation("OnGetAsync: ExternalLogins retrieved");

                Input = new InputModel
                {
                    Goals = new List<SelectListItem>
                    {
                         new SelectListItem { Value = "Weight Loss", Text = "Weight Loss" },
                         new SelectListItem { Value = "Muscle Building", Text = "Muscle Building" },
                         new SelectListItem { Value = "Cardiovascular Fitness", Text = "Cardiovascular Fitness" },
                         new SelectListItem { Value = "Flexibility Improvement", Text = "Flexibility Improvement" },
                         new SelectListItem { Value = "General Health and Wellness", Text = "General Health and Wellness" },

                    },
                    ExerciseFocusOptions = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "Cardio", Text = "Cardio" },
                        new SelectListItem { Value = "High-Intensity Interval Training", Text = "High-Intensity Interval Training" },
                        new SelectListItem { Value = "Strength Training", Text = "Strength Training" },
                        new SelectListItem { Value = "Running", Text = "Running" },
                        new SelectListItem { Value = "Cycling", Text = "Cycling" },
                        new SelectListItem { Value = "Swimming", Text = "Swimming" },
                        new SelectListItem { Value = "Rowing", Text = "Rowing" },
                        new SelectListItem { Value = "Yoga", Text = "Yoga" },
                        new SelectListItem { Value = "Pilates", Text = "Pilates" },
                        new SelectListItem { Value = "Balanced Workouts", Text = "Balanced Workouts" },
                    }
                };

                _logger.LogInformation("OnGetAsync: InputModel created");

                // Add more logging statements as needed

                _logger.LogInformation("OnGetAsync: End");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in OnGetAsync");
                throw; // Re-throw the exception to ensure it's not silently ignored
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Check if the email is already registered
            var existingUsers = await _userManager.Users.Where(u => u.NormalizedEmail == Input.Email.ToUpper()).ToListAsync();

            _logger.LogInformation($"Found {existingUsers.Count} users with email: {Input.Email}");

            if (existingUsers.Count > 0)
            {
                // Log the details of existing users for investigation
                foreach (var user in existingUsers)
                {
                    _logger.LogInformation($"User Id: {user.Id}, Email: {user.Email}");
                }

                // Email is already registered
                ModelState.AddModelError(string.Empty, "The email address is already registered. Please use a different email.");
                return RedirectToPage(new { returnUrl, error = "email" });
            }

            if (Input.SelectedGoals == null || Input.ExerciseFocus == null ||
     Input.SelectedGoals.Any(string.IsNullOrEmpty) || Input.ExerciseFocus.Any(string.IsNullOrEmpty))
            {
                ModelState.AddModelError(string.Empty, "Please select goals and exercise focus.");
                return RedirectToPage(new { returnUrl, error = "other" });
            }


            try
            {
                _logger.LogInformation("OnPostAsync: Start");

                returnUrl ??= Url.Content("~/");
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("OnPostAsync: ModelState is valid");

                    var user = CreateUser();
                    user.FirstName = Input.FirstName;
                    user.LastName = Input.LastName;
                    user.Gender = Input.Gender;
                    user.DateOfBirth = Input.DateOfBirth;
                    user.Country = Input.Country;
                    user.Height = Input.Height;
                    user.GoalWeight = Input.GoalWeight;
                    user.SelectedGoals = Input.SelectedGoals;
                    user.ExerciseFocus = Input.ExerciseFocus;
                    user.CustomUserName = Input.CustomUserName;
                    user.Points = 50;

                    var InitialWeightEntry = new WeightEntry
                    {
                        Date = DateTime.Now,
                        Weight = Input.InitialWeightEntry
                    };

                    user.WeightEntries.Add(InitialWeightEntry);

                    await _userStore.SetUserNameAsync(user, Input.CustomUserName, CancellationToken.None);
                    await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                    var result = await _userManager.CreateAsync(user, Input.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("OnPostAsync: User created successfully");

                        await _userManager.AddClaimAsync(user, new Claim("FirstName", Input.FirstName));
                        await _userManager.AddClaimAsync(user, new Claim("LastName", Input.LastName));
                        await _userManager.AddClaimAsync(user, new Claim("Email", Input.Email));
                        
                        _logger.LogInformation("User created a new account with password.");

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

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

                _logger.LogInformation("OnPostAsync: ModelState is not valid, returning Page");
                return RedirectToPage(new { returnUrl, error = "other" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in OnPostAsync");

                // Preserve ModelState errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogInformation($"ModelState Error: {error.ErrorMessage}");
                }

                // Add custom error message
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request. Please try again.");

                // Return the page with preserved ModelState
                return RedirectToPage(new { returnUrl, error = "exception" });
            }


        }


        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}