using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WpfClient.ChatServiceReference;

namespace WpfClient.ViewModel
{
    public class MainViewModel : ViewModelBase, IDataErrorInfo
    {
        // Connect to server
        static InstanceContext context = new InstanceContext(new ChatCallBack());
        ChatServiceClient server = new ChatServiceClient(context);

        // With Nuget Package PropertyChanged.Fody is no need to define RaisePropertyChanged
        public string Username { get; set; }
        public string ChatText { get; set; }
        public string Userlist { get; set; } = "Teilnehmer";

        ObservableCollection<Message> messages = new ObservableCollection<Message>();
        public ObservableCollection<Message> Messages
        {
            get { return messages; }
            set
            {
                messages = value;
                //RaisePropertyChanged("Messages");
            }
        }

        string windowTitle;
        public string WindowTitle 
        {
            get { return windowTitle; } 
            set
            {
                windowTitle = value;
                //RaisePropertyChanged("WindowTitle");
            }

        }

        public Visibility LoginVisibility { get; set; }

        public Visibility ChatViewVisibility { get; set; }
        public ICommand LoginCommand { get; private set; }
        public ICommand SendCommand { get; private set; }
        public ICommand LogOutCommand { get; private set; }

        public string Error => string.Empty;

        public string this[string propertyName]
        {
            get
            {
                switch (propertyName)
                {
                    case nameof(Username):
                        if (Username == "")
                            return "Username is required!";
                        break;
                }
                return string.Empty;
            }
        }

        public MainViewModel()
        {
            LoginVisibility = Visibility.Visible;
            ChatViewVisibility = Visibility.Collapsed;

            LoginCommand = new RelayCommand(LoginMethod);
            SendCommand = new RelayCommand(SendMethod);

            if (IsInDesignMode)
            {
                WindowTitle = "Chat Application (Design)";
            }
            else
            {
                WindowTitle = "Chat Application";
            }

            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
        }

        public void LoginMethod()
        {
            if (!string.IsNullOrEmpty(Username))
            {
                LoginVisibility = Visibility.Collapsed;
                ChatViewVisibility = Visibility.Visible;

                server.Join(Username);
                var history = server.GetChats();

                Messages = new ObservableCollection<Message>(history);

                server.RefreshAsync();
            }
        }
        public void SendMethod()
        {
            if (!string.IsNullOrEmpty(ChatText))
                server.SendChat(Username, ChatText);
            ChatText = string.Empty;
        }

        public void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to close the application?", "Chat Application", MessageBoxButton.YesNo) == MessageBoxResult.No)
                e.Cancel = true;
            else
            {
                if (!string.IsNullOrEmpty(Username))
                    server.LogOut();
                server.Close();
            }
                
        }
    }
}