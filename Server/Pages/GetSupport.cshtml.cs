using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nexRemoteFree.Server.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace nexRemoteFree.Server.Pages
{
    public class GetSupportModel : PageModel
    {
        private readonly IDataService _dataService;
        private readonly IEmailSenderEx _emailSender;

        public GetSupportModel(IDataService dataService, IEmailSenderEx emailSender)
        {
            _dataService = dataService;
            _emailSender = emailSender;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPost(string deviceId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var orgID = _dataService.GetDevice(deviceId)?.OrganizationID;

            var alertParts = new string[]
            {
                $"{Input.Name} prosi o wsparcie.",
                $"ID urządzenia: {deviceId}",
                $"Email: {Input.Email}.",
                $"Telefon: {Input.Phone}.",
                $"Czat OK: {Input.ChatResponseOk}."
            };

            var alertMessage = string.Join("  ", alertParts);
            await _dataService.AddAlert(deviceId, orgID, alertMessage);

            var orgUsers = await _dataService.GetAllUsersInOrganization(orgID);
            var emailMessage = string.Join("<br />", alertParts);
            foreach (var user in orgUsers)
            {
                await _emailSender.SendEmailAsync(user.Email, "Support Request", emailMessage);
            }

            StatusMessage = "Dziękujemy! Ktoś wkrótce się z Państwem skontaktuje.";

            return RedirectToPage("GetSupport", new { deviceId });
        }

        public class InputModel
        {
            [StringLength(150)]
            [Required]
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public bool ChatResponseOk { get; set; }
        }
    }
}