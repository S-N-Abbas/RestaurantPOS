using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Home;
using RestaurantPOS.ViewModels.Tables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.Login
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        private readonly UserSessionService _userSessionService;

        private const int MaxPinLength = 4;

        public ObservableCollection<UserViewModel> Users { get; }

        private UserViewModel? _selectedUser;
        public UserViewModel? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        private string _enteredPin = string.Empty;

        public string PinDisplay => new string('●', _enteredPin.Length);

        /* COMMANDS */

        public ICommand AppendDigitCommand { get; }
        public ICommand ClearPinCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand ConfirmPinCommand { get; }
        public ICommand SelectUserCommand { get; }

        public LoginViewModel(
            IUserService userService,
            INavigationService navigationService,
            UserSessionService userSessionService)
        {
            _userService = userService;
            _navigationService = navigationService;
            _userSessionService = userSessionService;

            Users = new ObservableCollection<UserViewModel>(
                _userService.GetAllUsers()
                            .Select(u => new UserViewModel(u)));

            AppendDigitCommand = new RelayCommand<string>(AppendDigit);
            ClearPinCommand = new RelayCommand(ClearPin);
            BackspaceCommand = new RelayCommand(Backspace);
            SelectUserCommand = new RelayCommand<string>(SelectUser);
            ConfirmPinCommand = new RelayCommand(async () => await ConfirmAsync());
        }

        private void SelectUser(string? username)
        {
            SelectedUser = Users.First(u => u.Username == username);
        }

        /* PIN LOGIC */

        private void AppendDigit(string? digit)
        {
            if (SelectedUser == null)
                return;

            if (_enteredPin.Length >= MaxPinLength)
                return;

            _enteredPin += digit;

            OnPropertyChanged(nameof(PinDisplay));
        }

        private void ClearPin()
        {
            _enteredPin = string.Empty;
            OnPropertyChanged(nameof(PinDisplay));
        }

        private void Backspace()
        {
            if (_enteredPin.Length == 0)
                return;

            _enteredPin = _enteredPin[..^1];
            OnPropertyChanged(nameof(PinDisplay));
        }

        /* AUTHENTICATION */

        private async Task ConfirmAsync()
        {
            if (SelectedUser == null)
                return;

            if (_enteredPin.Length != MaxPinLength)
                return;

            try
            {
                bool isValid = await _userService.ValidatePinAsync(
                    SelectedUser.Id,
                    _enteredPin);

                if (!isValid)
                {
                    ClearPin();
                    // Optional: Show error animation/message
                    return;
                }

                ClearPin();

                _userSessionService.SetUser(SelectedUser._user);
                _navigationService.NavigateTo<HomeViewModel>();
            }
            catch (Exception ex)
            {
                ClearPin();
                Debug.WriteLine(ex.Message);
            }
        }
    }

}
