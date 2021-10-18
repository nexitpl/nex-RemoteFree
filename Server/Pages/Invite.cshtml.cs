using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nexRemote.Server.Services;

namespace nexRemote.Server.Pages
{
    [Authorize]
    public class InviteModel : PageModel
    {
        public InviteModel(IDataService dataService)
        {
            DataService = dataService;
        }
        private IDataService DataService { get; }
        public bool Success { get; set; }

        public class InputModel
        {
            public string InviteID { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public void OnGet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                ModelState.AddModelError("Brak ID", "Nie określono identyfikatora zaproszenia.");
            }

            Input.InviteID = id;
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Input?.InviteID))
            {
                Success = false;
                ModelState.AddModelError("Brak ID", "Nie określono identyfikatora zaproszenia.");
                return Page();
            }

            var result = DataService.JoinViaInvitation(User.Identity.Name, Input.InviteID);
            if (result == false)
            {
                Success = false;
                ModelState.AddModelError("Nie znaleziono ID zaproszenia", "Nie znaleziono ID zaproszenia lub dotyczy innego konta.");
            }

            Success = true;
            return Page();
        }
    }
}