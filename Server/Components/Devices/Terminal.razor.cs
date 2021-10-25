using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using nexRemote.Server.Components.ModalContents;
using nexRemote.Server.Hubs;
using nexRemote.Server.Services;
using nexRemote.Shared.Enums;
using nexRemote.Shared.Models;
using nexRemote.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nexRemote.Server.Components.Devices
{
    public partial class Terminal : AuthComponentBase, IDisposable
    {
        private string _inputText;

        private string _lastCompletionInput;
        private int _lastCursorIndex;
        private ScriptingShell _shell;

        private ElementReference _terminalInput;
        private string _terminalOpenClass;
        private ElementReference _terminalWindow;

        [Inject]
        private IClientAppState AppState { get; set; }

        [Inject]
        private ICircuitConnection CircuitConnection { get; set; }

        [Inject]
        private IDataService DataService { get; set; }

        private string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                if (TryMatchShellShortcuts())
                {
                    _inputText = string.Empty;
                }
            }
        }

        [Inject]
        private IJsInterop JsInterop { get; set; }

        [Inject]
        private ILogger<Terminal> Logger { get; set; }

        [Inject]
        private IModalService ModalService { get; set; }
        private EventCallback<SavedScript> RunQuickScript =>
            EventCallback.Factory.Create<SavedScript>(this, async script =>
            {
                var scriptRun = new ScriptRun()
                {
                    OrganizationID = User.OrganizationID,
                    RunAt = Time.Now,
                    SavedScriptId = script.Id,
                    RunOnNextConnect = false,
                    Initiator = User.UserName,
                    InputType = ScriptInputType.OneTimeScript
                };

                scriptRun.Devices = DataService.GetDevices(AppState.DevicesFrameSelectedDevices);

                await DataService.AddScriptRun(scriptRun);

                await CircuitConnection.RunScript(AppState.DevicesFrameSelectedDevices, script.Id, scriptRun.Id, ScriptInputType.OneTimeScript, false);

                ToastService.ShowToast($"Uruchamianie skryptu na {scriptRun.Devices.Count} urządzeniach.");
            });

        [Inject]
        private IToastService ToastService { get; set; }

        public void Dispose()
        {
            AppState.PropertyChanged -= AppState_PropertyChanged;
            CircuitConnection.MessageReceived -= CircuitConnection_MessageReceived;
            GC.SuppressFinalize(this);
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                JsInterop.PreventTabOut(_terminalInput);
            }
            return base.OnAfterRenderAsync(firstRender);
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            CircuitConnection.MessageReceived += CircuitConnection_MessageReceived;
            AppState.PropertyChanged += AppState_PropertyChanged;
        }
        private void ApplyCompletion(PwshCommandCompletion completion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_lastCompletionInput))
                {
                    return;
                }

                var match = completion.CompletionMatches[completion.CurrentMatchIndex];

                var replacementText = string.Concat(
                    _lastCompletionInput.Substring(0, completion.ReplacementIndex),
                    match.CompletionText,
                    _lastCompletionInput[(completion.ReplacementIndex + completion.ReplacementLength)..]);

                InputText = replacementText;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Błąd podczas wypełniania polecenia.");
            }
        }

        private void AppState_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppState.TerminalLines))
            {
                InvokeAsync(StateHasChanged);
                JsInterop.ScrollToEnd(_terminalWindow);
            }
        }

        private void CircuitConnection_MessageReceived(object sender, Models.CircuitEvent e)
        {
            if (e.EventName == Models.CircuitEventName.PowerShellCompletions)
            {
                var completion = (PwshCommandCompletion)e.Params[0];
                var intent = (CompletionIntent)e.Params[1];

                switch (intent)
                {
                    case CompletionIntent.ShowAll:
                        DisplayCompletions(completion.CompletionMatches);
                        break;
                    case CompletionIntent.NextResult:
                        ApplyCompletion(completion);
                        break;
                    default:
                        break;
                }
                AppState.InvokePropertyChanged(nameof(AppState.TerminalLines));
            }
        }
        private void DisplayCompletions(List<PwshCompletionResult> completionMatches)
        {
            var deviceId = AppState.DevicesFrameSelectedDevices.FirstOrDefault();
            var device = AgentHub.GetDevice(deviceId);

            AppState.AddTerminalLine($"Uzupełnienia dla {device?.DeviceName}", className: "font-weight-bold");

            foreach (var match in completionMatches)
            {
                AppState.AddTerminalLine(match.CompletionText, className: "", title: match.ToolTip);
            }
        }
        private void EvaluateInputKeypress(KeyboardEventArgs ev)
        {
            if (ev.Key.Equals("Enter", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(InputText) || ev.ShiftKey)
                {
                    return;
                }

                var devices = AppState.DevicesFrameSelectedDevices.ToArray();
                if (!devices.Any())
                {
                    ToastService.ShowToast("Musisz wybrać przynajmniej jedno urządzenie.", classString: "bg-warning");
                    return;
                }
                CircuitConnection.ExecuteCommandOnAgent(_shell, InputText, devices);
                AppState.AddTerminalHistory(InputText);
                InputText = string.Empty;
            }
        }

        private async Task EvaluateKeyDown(KeyboardEventArgs ev)
        {
            if (!ev.Key.Equals("Tab", StringComparison.OrdinalIgnoreCase) &&
                !ev.Key.Equals("Shift", StringComparison.OrdinalIgnoreCase))
            {
                _lastCompletionInput = null;
            }


            if (ev.Key.Equals("ArrowUp", StringComparison.OrdinalIgnoreCase))
            {
                InputText = AppState.GetTerminalHistory(false);
            }
            else if (ev.Key.Equals("ArrowDown", StringComparison.OrdinalIgnoreCase))
            {
                InputText = AppState.GetTerminalHistory(true);
            }
            else if (ev.Key.Equals("Tab", StringComparison.OrdinalIgnoreCase))
            {
                if (_shell != ScriptingShell.PSCore && _shell != ScriptingShell.WinPS)
                {
                    ToastService.ShowToast("PowerShell jest wymagany do ukończenia zakładki.", classString: "bg-warning");
                    return;
                }

                if (!AppState.DevicesFrameSelectedDevices.Any())
                {
                    ToastService.ShowToast("Nie wybrano żadnych urządzeń.", classString: "bg-warning");
                    return;
                }

                if (string.IsNullOrWhiteSpace(InputText))
                {
                    return;
                }

                await GetNextCompletion(!ev.ShiftKey);
            }
            else if (ev.CtrlKey && ev.Key.Equals(" ", StringComparison.OrdinalIgnoreCase))
            {
                if (!AppState.DevicesFrameSelectedDevices.Any())
                {
                    return;
                }

                await ShowAllCompletions();
            }
            else if (ev.CtrlKey && ev.Key.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                AppState.TerminalLines.Clear();
                AppState.InvokePropertyChanged(nameof(AppState.TerminalLines));
            }
        }

        private async Task GetNextCompletion(bool forward)
        {
            if (string.IsNullOrWhiteSpace(_lastCompletionInput))
            {
                _lastCompletionInput = InputText;
                _lastCursorIndex = await JsInterop.GetCursorIndex(_terminalInput);
            }

            await CircuitConnection.GetPowerShellCompletions(_lastCompletionInput, _lastCursorIndex, CompletionIntent.NextResult, forward);
        }

        private async Task ShowAllCompletions()
        {
            if (string.IsNullOrWhiteSpace(_lastCompletionInput))
            {
                _lastCompletionInput = InputText;
                _lastCursorIndex = await JsInterop.GetCursorIndex(_terminalInput);
            }

            await CircuitConnection.GetPowerShellCompletions(_lastCompletionInput, _lastCursorIndex, CompletionIntent.ShowAll, false);
        }

        private async Task ShowQuickScripts()
        {
            var quickScripts = await DataService.GetQuickScripts(User.Id);
            if (quickScripts?.Any() != true)
            {
                ToastService.ShowToast("Brak zapisanych szybkich skryptów.", classString: "bg-warning");
                return;
            }

            if (!AppState.DevicesFrameSelectedDevices.Any())
            {
                ToastService.ShowToast("Musisz wybrać przynajmniej jedno urządzenie.", classString: "bg-warning");
                return;
            }


            void showModal(RenderTreeBuilder builder)
            {
                builder.OpenComponent<QuickScriptsSelector>(0);
                builder.AddAttribute(1, "QuickScripts", quickScripts);
                builder.AddAttribute(2, "OnRunClicked", RunQuickScript);
                builder.CloseComponent();
            }

            await ModalService.ShowModal("Quick Scripts", showModal);
        }

        private void ShowTerminalHelp()
        {
            ModalService.ShowModal("Terminal Help", new[]
            {
                "Wprowadź polecenia terminala, które będą wykonywane na wszystkich wybranych urządzeniach.",

                "Uzupełnianie komend jest dostępne dla PowerShell Core (PSCore) i Windows PowerShell (WinPS). Tab i Shift + Tab " +
                "będzie przechodzić przez potencjalne uzupełnienia. Ctrl + spacja pokaże wszystkie dostępne uzupełnienia.",

                "Jeśli wybrano więcej niż jedno urządzenie, system plików pierwszego urządzenia będzie używany podczas  " +
                "automatycznego uzupełniania ścieżek plików i katalogów.",

                "Program PowerShell Core jest wieloplatformowy i jest dostępny we wszystkich klienckich systemach operacyjnych. Bash jest dostępny w systemie " +
                "Windows 10, jeśli zainstalowano WSL (Windows Subsystem for Linux).",

                "Klawisze strzałek w górę i w dół umożliwiają przeglądanie historii wprowadzania terminala. Ctrl + Q wyczyści okno wyjściowe.",

                "Uwaga: pierwsze polecenie lub wypełnienie karty PS Core zajmuje kilka chwil, gdy usługa jest " +
                "jest uruchamiana na zdalnym urządzeniu."
            });
        }
        private void ToggleTerminalOpen()
        {
            if (string.IsNullOrWhiteSpace(_terminalOpenClass))
            {
                _terminalOpenClass = "open";
            }
            else
            {
                _terminalOpenClass = string.Empty;
            }
        }
        private bool TryMatchShellShortcuts()
        {
            var currentText = InputText?.Trim()?.ToLower();

            if (string.IsNullOrWhiteSpace(currentText))
            {
                return false;
            }

            if (currentText.Equals(User.UserOptions.CommandModeShortcutPSCore.ToLower()))
            {
                _shell = ScriptingShell.PSCore;
                return true;
            }
            else if (currentText.Equals(User.UserOptions.CommandModeShortcutCMD.ToLower()))
            {
                _shell = ScriptingShell.CMD;
                return true;
            }
            else if (currentText.Equals(User.UserOptions.CommandModeShortcutWinPS.ToLower()))
            {
                _shell = ScriptingShell.WinPS;
                return true;
            }
            else if (currentText.Equals(User.UserOptions.CommandModeShortcutBash.ToLower()))
            {
                _shell = ScriptingShell.Bash;
                return true;
            }
            return false;
        }
    }
}
