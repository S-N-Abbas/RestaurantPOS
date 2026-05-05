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
        private readonly SettingsService _settingsService;
        public ViewModelBase CurrentViewModel
            => _navigationService.CurrentViewModel;

        // Command for the Shell Back Button
        public IRelayCommand GoBackCommand { get; }

        // Visibility helper: Only show back button if history exists
        public Visibility BackButtonVisibility => _navigationService.CanGoBack ? Visibility.Visible : Visibility.Collapsed;

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

        public bool IsLoggedIn => _userSessionService.IsLoggedIn;

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
        
        public ShellViewModel(INavigationService navigationService, UserSessionService userSessionService, SettingsService settingsService)
        {
            _navigationService = navigationService;
            _userSessionService = userSessionService;
            _settingsService = settingsService;

            RestaurantName = _settingsService.Settings.BusinessName;
            TerminalInfo = _settingsService.Settings.TillNo;

            // Subscribe to changes
            _userSessionService.UserChanged += UpdateUserProperties;
            _settingsService.SettingsChanged += SettingsChanged;

            _navigationService.CurrentViewModelChanged += () =>
                OnPropertyChanged(nameof(CurrentViewModel));

            LogoutCommand = new RelayCommand(OnLogout);
            CloseCommand = new RelayCommand(OnClose);

            GoBackCommand = new RelayCommand(() => _navigationService.GoBack());

            // Refresh visibility whenever navigation happens
            _navigationService.CurrentViewModelChanged += () => {
                OnPropertyChanged(nameof(BackButtonVisibility));
            };
        }

        private void SettingsChanged()
        {
            RestaurantName = _settingsService.Settings.BusinessName;
            TerminalInfo = _settingsService.Settings.TillNo;

        }

        private void UpdateUserProperties()
        {
            // Update the properties that the View is bound to
            CurrentUserName = _userSessionService.CurrentUser?.Username ?? "Guest";
            CurrentUserRole = _userSessionService.CurrentUser?.Role ?? "Unknown";
            OnPropertyChanged(nameof(IsLoggedIn));
        }

        private void OnLogout()
        {
            _userSessionService.Logout();
            _navigationService.NavigateTo<LoginViewModel>();
            _navigationService.ClearHistory(); // Clear history to prevent going back to authenticated views
        }

        private void OnClose()
        {
            // Standard WPF shutdown
            Application.Current.Shutdown();
        }
    }
}
