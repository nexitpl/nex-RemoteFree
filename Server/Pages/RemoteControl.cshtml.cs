using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nexRemoteFree.Server.Auth;
using nexRemoteFree.Server.Services;
using nexRemoteFree.Shared.Models;

namespace nexRemoteFree.Server.Pages
{
    [ServiceFilter(typeof(RemoteControlFilterAttribute))]
    public class RemoteControlModel : PageModel
    {
        private readonly IDataService _dataService;
        public RemoteControlModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        public nexRemoteFreeUser nexRemoteFreeUser { get; private set; }
        public void OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                nexRemoteFreeUser = _dataService.GetUserByNameWithOrg(base.User.Identity.Name);
            }
        }
    }
}
