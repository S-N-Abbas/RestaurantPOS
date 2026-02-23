using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Settings;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.BackOffice.Settings
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;

        private AppSettings _editableSettings;

        public AppSettings EditableSettings
        {
            get => _editableSettings;
            set => SetProperty(ref _editableSettings, value);
        }

        public bool IsDirty => !AreEqual(EditableSettings, _settingsService.Settings);

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand ResetCommand { get; }

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;

            EditableSettings = CloneSettings(_settingsService.Settings);

            SaveCommand = new RelayCommand(OnSave);
            ResetCommand = new RelayCommand(OnReset);
        }

        private void OnSave()
        {
            _settingsService.Save(EditableSettings);

            // Refresh editor with clean persisted state
            EditableSettings = CloneSettings(_settingsService.Settings);
        }

        private void OnReset()
        {
            EditableSettings = CloneSettings(_settingsService.Settings);
        }

        // ✅ Deep Copy Helper
        private AppSettings CloneSettings(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings);
            return JsonSerializer.Deserialize<AppSettings>(json)!;
        }

        private bool AreEqual(AppSettings a, AppSettings b)
        {
            return JsonSerializer.Serialize(a) == JsonSerializer.Serialize(b);
        }
    }
}
