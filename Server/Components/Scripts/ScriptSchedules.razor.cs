using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using nexRemoteFree.Server.Pages;
using nexRemoteFree.Server.Services;
using nexRemoteFree.Shared.Models;
using nexRemoteFree.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nexRemoteFree.Server.Components.Scripts
{
    [Authorize]
    public partial class ScriptSchedules : AuthComponentBase
    {
        private readonly List<string> _selectedDeviceGroups = new();

        private readonly List<string> _selectedDevices = new();

        private readonly List<ScriptSchedule> _schedules = new();

        private string _alertMessage;

        private DeviceGroup[] _deviceGroups = Array.Empty<DeviceGroup>();

        private Device[] _devices = Array.Empty<Device>();

        private SavedScript _selectedScript;

        private ScriptSchedule _selectedSchedule = new() { StartAt = Time.Now };

        [CascadingParameter]
        private ScriptsPage ParentPage { get; set; }

        [Inject]
        private IDataService DataService { get; set; }

        [Inject]

        private IJsInterop JsInterop { get; set; }

        [Inject]
        private IToastService ToastService { get; set; }

        private bool CanModifyScript => string.IsNullOrWhiteSpace(_selectedSchedule.CreatorId) ||
            _selectedSchedule.CreatorId == User.Id ||
            User.IsAdministrator;

        private bool CanDeleteScript => !string.IsNullOrWhiteSpace(_selectedSchedule.CreatorId) &&
            (_selectedSchedule.CreatorId == User.Id || User.IsAdministrator);

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _deviceGroups = DataService.GetDeviceGroups(User.UserName);
            _devices = DataService
                .GetDevicesForUser(User.UserName)
                .OrderBy(x => x.DeviceName)
                .ToArray();

            await RefreshSchedules();
        }

        private void CreateNew()
        {
            _selectedScript = new();
            _selectedSchedule = new() { StartAt = Time.Now };
            _selectedDeviceGroups.Clear();
            _selectedDevices.Clear();
        }

        private async Task DeleteSelectedSchedule()
        {
            if (User.Id != _selectedSchedule.CreatorId)
            {
                ToastService.ShowToast("Nie możesz usunąć harmonogramów skryptów innych osób.", classString: "bg-warning");
                return;
            }

            var result = await JsInterop.Confirm($"Czy na pewno chcesz usunąć harmonogram {_selectedSchedule.Name}?");
            if (result)
            {
                await DataService.DeleteScriptSchedule(_selectedSchedule.Id);
                ToastService.ShowToast("Harmonogram usunięty.");
                _alertMessage = "Harmonogram usunięty.";
                CreateNew();
                await ParentPage.RefreshScripts();
                await RefreshSchedules();
            }
        }

        private void DeviceGroupSelectedChanged(ChangeEventArgs args, DeviceGroup deviceGroup)
        {
            var isSelected = (bool)args.Value;
            if (isSelected)
            {
                _selectedDeviceGroups.Add(deviceGroup.ID);
            }
            else
            {
                _selectedDeviceGroups.RemoveAll(x => x == deviceGroup.ID);
            }
        }

        private void DeviceSelectedChanged(ChangeEventArgs args, Device device)
        {
            var isSelected = (bool)args.Value;
            if (isSelected)
            {
                _selectedDevices.Add(device.ID);
            }
            else
            {
                _selectedDevices.RemoveAll(x => x == device.ID);
            }
        }

        private async Task OnValidSubmit(EditContext context)
        {
            if (_selectedSchedule is null)
            {
                return;
            }

            if (_selectedScript is null)
            {
                ToastService.ShowToast("Musisz wybrać skrypt do uruchomienia.", classString: "bg-warning");
                return;
            }

            if (!CanModifyScript)
            {
                ToastService.ShowToast("Nie możesz modyfikować harmonogramów innych osób.", classString: "bg-warning");
                return;
            }

            if (!_selectedDevices.Any() && !_selectedDeviceGroups.Any())
            {
                ToastService.ShowToast("Musisz wybrać co najmniej jedno urządzenie lub grupę urządzeń.", classString: "bg-warning");
                return;
            }

            _selectedSchedule.SavedScriptId = _selectedScript.Id;

            if (string.IsNullOrWhiteSpace(_selectedSchedule.CreatorId))
            {
                _selectedSchedule.CreatedAt = Time.Now;
                _selectedSchedule.CreatorId = User.Id;
            }

            _selectedSchedule.OrganizationID = User.OrganizationID;
            _selectedSchedule.NextRun = _selectedSchedule.StartAt;

            _selectedSchedule.Devices = _devices.Where(x => _selectedDevices.Contains(x.ID)).ToList();
            _selectedSchedule.DeviceGroups = _deviceGroups.Where(x => _selectedDeviceGroups.Contains(x.ID)).ToList();

            await DataService.AddOrUpdateScriptSchedule(_selectedSchedule);
            CreateNew();
            await RefreshSchedules();
            ToastService.ShowToast("Harmonogram zapisany.");
            _alertMessage = "Harmonogram zapisany.";
        }

        private async Task RefreshSchedules()
        {
            _schedules.Clear();
            _schedules.AddRange(await DataService.GetScriptSchedules(User.OrganizationID));
        }

        private string GetTableRowClass(ScriptSchedule schedule)
        {
            if (schedule?.Id == _selectedSchedule?.Id)
            {
                return "bg-primary text-white";
            }
            return string.Empty;
        }

        private async Task SelectTableRow(ScriptSchedule schedule)
        {
            _selectedSchedule = schedule;
            _selectedDevices.Clear();
            _selectedDeviceGroups.Clear();

            if (schedule?.Devices?.Any() == true)
            {
                _selectedDevices.AddRange(schedule.Devices.Select(x => x.ID));
            }
            if (schedule?.DeviceGroups?.Any() == true)
            {
                _selectedDeviceGroups.AddRange(schedule.DeviceGroups.Select(x => x.ID));
            }
            _selectedScript = await DataService.GetSavedScript(_selectedSchedule.SavedScriptId);
        }

        private async Task ScriptSelected(ScriptTreeNode viewModel)
        {
            if (viewModel.Script is not null)
            {
                _selectedScript = await DataService.GetSavedScript(User.Id, viewModel.Script.Id);

            }
            else
            {
                _selectedScript = null;
            }
        }
    }
}
