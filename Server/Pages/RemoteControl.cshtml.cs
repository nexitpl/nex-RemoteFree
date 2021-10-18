using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nexRemote.Server.Auth;
using nexRemote.Server.Services;
using nexRemote.Shared.Models;

namespace nexRemote.Server.Pages
{
    [ServiceFilter(typeof(RemoteControlFilterAttribute))]
    public class RemoteControlModel : PageModel
    {
        private readonly IDataService _dataService;
        public RemoteControlModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        public RemotelyUser RemotelyUser { get; private set; }
        public void OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                RemotelyUser = _dataService.GetUserByNameWithOrg(base.User.Identity.Name);
            }
        }
    }
}
