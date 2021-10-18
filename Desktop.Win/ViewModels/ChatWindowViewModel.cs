using nexRemote.Shared.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace nexRemote.Desktop.Win.ViewModels
{
    public class ChatWindowViewModel : BrandedViewModelBase
    {
        private string _inputText;
        private string _organizationName = "nex-IT Jakub Potoczny";
        private string _senderName = "Jakub Potoczny";

        public ObservableCollection<ChatMessage> ChatMessages { get; } = new ObservableCollection<ChatMessage>();

        public string InputText
        {
            get
            {
                return _inputText;
            }
            set
            {
                _inputText = value;
                FirePropertyChanged();
            }
        }

        public string OrganizationName
        {
            get
            {
                return _organizationName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value) ||
                    value == _organizationName)
                {
                    return;
                }


                _organizationName = value;
                FirePropertyChanged();
            }
        }

        public StreamWriter PipeStreamWriter { get; set; }
        public string SenderName
        {
            get
            {
                return _senderName;
            }
            set
            {
                if (value == _senderName)
                {
                    return;
                }

                _senderName = value;
                FirePropertyChanged();
            }
        }

        public async Task SendChatMessage()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                return;
            }

            var chatMessage = new ChatMessage(string.Empty, InputText);
            InputText = string.Empty;
            await PipeStreamWriter.WriteLineAsync(JsonSerializer.Serialize(chatMessage));
            await PipeStreamWriter.FlushAsync();
            chatMessage.SenderName = "Ty";
            ChatMessages.Add(chatMessage);
        }
    }
}
