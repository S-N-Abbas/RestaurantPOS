using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RestaurantPOS.ViewModels.Shell
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly UserSessionService _userSessionService;
        public ViewModelBase CurrentViewModel
            => _navigationService.CurrentViewModel;

        // --- NEW BINDABLE PROPERTIES ---
        private string _restaurantName = "NAWAB PALACE";
        public string RestaurantName
        {
            get => _restaurantName;
            set 
            {
                _restaurantName = value;
                OnPropertyChanged(nameof(RestaurantName));
            }
        }

        private string _terminalInfo = "Terminal 01 - Main Floor";
        public string TerminalInfo
        {
            get => _terminalInfo;
            set 
            {
                _terminalInfo = value;
                OnPropertyChanged(nameof(TerminalInfo));
            }
        }

        private string _currentUserName;
        public string CurrentUserName
        {
            get => _currentUserName;
            set 
            {
                _currentUserName = value;
                OnPropertyChanged(nameof(CurrentUserName));
            }
        }

        private string _currentUserRole;
        public string CurrentUserRole
        {
            get => _currentUserRole;
            set 
            {
                _currentUserRole = value;
                OnPropertyChanged(nameof(CurrentUserRole));
            }
        }

        public IRelayCommand LogoutCommand { get; }
        public IRelayCommand CloseCommand { get; }
        
        public ShellViewModel(INavigationService navigationService, UserSessionService userSessionService)
        {
            _navigationService = navigationService;
            _userSessionService = userSessionService;

            // Subscribe to changes
            _userSessionService.UserChanged += UpdateUserProperties;

            _navigationService.CurrentViewModelChanged += () =>
                OnPropertyChanged(nameof(CurrentViewModel));

            LogoutCommand = new RelayCommand(OnLogout);
            CloseCommand = new RelayCommand(OnClose);
        }

        private void UpdateUserProperties()
        {
            // Update the properties that the View is bound to
            CurrentUserName = _userSessionService.CurrentUser?.Username ?? "Guest";
            CurrentUserRole = _userSessionService.CurrentUser?.Role ?? "Unknown";
        }

        private void OnLogout()
        {
            _userSessionService.Logout();
            _navigationService.NavigateTo<LoginViewModel>();
        }

        private void OnClose()
        {
            // Standard WPF shutdown
            Application.Current.Shutdown();
        }
    }
}
