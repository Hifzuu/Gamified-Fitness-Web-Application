// PersonalDetails.cshtml.cs

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectWebApp.Models;

namespace ProjectWebApp.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public PersonalDetailsModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public int Height { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "New Height")]
            public int NewHeight { get; set; }

            [Display(Name = "New Gender")]
            public string NewGender { get; set; }

            [Display(Name = "New Country")]
            public string NewCountry { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            // Load existing details from the user or modify as per your ApplicationUser model
            Height = user.Height;
            Gender = user.Gender;
            Country = user.Country;

            Input = new InputModel
            {
                NewHeight = Height,
                NewGender = Gender,
                NewCountry = Country,
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangePersonalDetailsAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Update user details with the values from the input model
            user.Height = Input.NewHeight;
            user.Gender = Input.NewGender;
            user.Country = Input.NewCountry;

            // Update the user in the database
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                StatusMessage = "Your personal details have been updated.";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadAsync(user);
            return Page();
        }
    }
}