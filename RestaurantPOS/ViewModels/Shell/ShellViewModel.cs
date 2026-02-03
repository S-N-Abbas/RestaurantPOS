using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using RestaurantPOS.ViewModels.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantPOS.ViewModels.Shell
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        public ViewModelBase CurrentViewModel
            => _navigationService.CurrentViewModel;

        public IRelayCommand LogoutCommand { get; }

        public ShellViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            _navigationService.OnCurrentViewModelChanged += () =>
                OnPropertyChanged(nameof(CurrentViewModel));

            LogoutCommand = new RelayCommand(OnLogout);
        }

        private void OnLogout()
        {
            _navigationService.NavigateTo<LoginViewModel>();
        }
    }
}
