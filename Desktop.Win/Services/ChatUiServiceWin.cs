using nexRemoteFree.Desktop.Core.Interfaces;
using nexRemoteFree.Desktop.Win.ViewModels;
using nexRemoteFree.Desktop.Win.Views;
using nexRemoteFree.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace nexRemoteFree.Desktop.Win.Services
{
    public class ChatUiServiceWin : IChatUiService
    {
        private ChatWindowViewModel ChatViewModel { get; set; }

        public event EventHandler ChatWindowClosed;

        public void ReceiveChat(ChatMessage chatMessage)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (chatMessage.Disconnected)
                {
                    MessageBox.Show("Twój partner się rozłączył.", "Partner rozłączony", MessageBoxButton.OK, MessageBoxImage.Information);
                    App.Current.Shutdown();
                    return;
                }

                if (ChatViewModel != null)
                {
                    ChatViewModel.SenderName = chatMessage.SenderName;
                    ChatViewModel.ChatMessages.Add(chatMessage);
                }
            });
        }

        public void ShowChatWindow(string organizationName, StreamWriter writer)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var chatWindow = new ChatWindow();
                chatWindow.Closing += ChatWindow_Closing;
                ChatViewModel = chatWindow.DataContext as ChatWindowViewModel;
                ChatViewModel.PipeStreamWriter = writer;
                ChatViewModel.OrganizationName = organizationName;
                chatWindow.Show();
            });
        }

        private void ChatWindow_Closing(object sender, CancelEventArgs e)
        {
            ChatWindowClosed?.Invoke(this, null);
        }
    }
}
