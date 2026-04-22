using CommunityToolkit.Mvvm.Input;
using RestaurantPOS.Domain.Entities;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RestaurantPOS.ViewModels.BackOffice.Users
{
    public class UsersViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly UserSessionService _userSessionService;

        private ObservableCollection<User> _users;
        public ObservableCollection<User> Users { 
            get => _users; 
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        private User? _selectedUser;
        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();

                LoadSelectedUser();
                // Tell WPF to re-check CanExecute
                (DeleteUserCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        // ✅ Editor Fields

        // ─── Keyboard active field tracking ──────────────────────────────────────────

        public enum UserEditorField { Name, Pin, Search }

        private UserEditorField _activeField = UserEditorField.Name;

        public bool IsNameActive => _activeField == UserEditorField.Name;
        public bool IsPinActive => _activeField == UserEditorField.Pin;

        public bool IsSearchActive => _activeField == UserEditorField.Search;

        // ─── PIN display (dots only — never show actual digits) ───────────────────────

        public string PinDisplay => new string('●', EditorPin.Length);

        private string _editorName = string.Empty;
        public string EditorName
        {
            get => _editorName;
            set { _editorName = value; OnPropertyChanged(); }
        }

        public IEnumerable<UserRole> Roles { get; } =
            Enum.GetValues(typeof(UserRole)).Cast<UserRole>();


        private UserRole _editorRole = UserRole.Cashier;
        public UserRole EditorRole
        {
            get => _editorRole;
            set { _editorRole = value; 
                OnPropertyChanged(); }
        }

        private string _editorPin = string.Empty;
        public string EditorPin
        {
            get => _editorPin;
            set { _editorPin = value; OnPropertyChanged(); }
        }

        private bool _editorIsActive = true;
        public bool EditorIsActive
        {
            get => _editorIsActive;
            set { _editorIsActive = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                LoadUsers(SearchText);
            }
        }

        // ✅ Commands

        public ICommand FocusNameCommand { get; }
        public ICommand FocusPinCommand { get; }

        public ICommand FocusSearchCommand { get; }
        public ICommand KeyCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand ClearFieldCommand { get; }

        public ICommand SaveUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ClearEditorCommand { get; }
        public ICommand NewUserCommand { get; }

        // Role setter commands — replaces ComboBox
        public ICommand SetRoleAdminCommand { get; }
        public ICommand SetRoleManagerCommand { get; }
        public ICommand SetRoleCashierCommand { get; }
        // User selection from the list (replaces DataGrid SelectedItem binding)
        public ICommand SelectUserCommand { get; }

        public UsersViewModel(IUserService userService, UserSessionService userSessionService)
        {
            _userService = userService;
            _userSessionService = userSessionService;

            LoadUsers();

            SaveUserCommand = new RelayCommand(SaveUser);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanDeleteUser);
            ClearEditorCommand = new RelayCommand(ClearEditor);
            NewUserCommand = new RelayCommand(NewUser);
            FocusNameCommand = new RelayCommand(() => SetActiveField(UserEditorField.Name));
            FocusPinCommand = new RelayCommand(() => SetActiveField(UserEditorField.Pin));
            FocusSearchCommand = new RelayCommand(() => SetActiveField(UserEditorField.Search));
            KeyCommand = new RelayCommand<string>(AppendKey);
            BackspaceCommand = new RelayCommand(Backspace);
            ClearFieldCommand = new RelayCommand(ClearActive);
            SetRoleAdminCommand = new RelayCommand(() => EditorRole = UserRole.Admin);
            SetRoleManagerCommand = new RelayCommand(() => EditorRole = UserRole.Manager);
            SetRoleCashierCommand = new RelayCommand(() => EditorRole = UserRole.Cashier);

            SelectUserCommand = new RelayCommand<User>(user =>
            {
                if (user != null) SelectedUser = user;
            });
        }

        private void LoadUsers(string filter = "")
        {
            if(filter == string.Empty)
            {
                Users = new ObservableCollection<User>(
                    _userService.GetAllUsers());
            }
            else
            Users = new ObservableCollection<User>(
                _userService.SearchUsers(filter));
        }

        // ✅ Selection Handling

        private void LoadSelectedUser()
        {
            if (SelectedUser == null)
                return;

            UserRole selectedUserRole;
            Enum.TryParse<UserRole>(SelectedUser.Role, out selectedUserRole);

            EditorName = SelectedUser.Username;
            EditorRole = selectedUserRole;
            EditorIsActive = SelectedUser.IsActive;

            // POS systems typically DO NOT reveal PIN
            EditorPin = string.Empty;
        }

        // ✅ Save Logic (Insert / Update)

        private void SaveUser()
        {
            if (string.IsNullOrWhiteSpace(EditorName))
                return;

            if (SelectedUser == null)
            {
                // ✅ CREATE NEW USER

                var newUser = new User
                {
                    Username = EditorName,
                    Role = EditorRole.ToString(),
                    PasscodeHash = EditorPin,
                    IsActive = EditorIsActive
                };

                _userService.CreateUser(newUser);
            }
            else
            {
                // ✅ UPDATE EXISTING USER

                SelectedUser.Username = EditorName;
                SelectedUser.Role = EditorRole.ToString();
                SelectedUser.IsActive = EditorIsActive;

                // Update PIN only if changed
                if (!string.IsNullOrWhiteSpace(EditorPin))
                    SelectedUser.PasscodeHash = EditorPin;

                _userService.UpdateUser(SelectedUser);
            }

            ClearEditor();
            LoadUsers();
        }

        // ✅ Delete Logic

        private bool CanDeleteUser()
        {
            return SelectedUser != null && SelectedUser.Username != _userSessionService.CurrentUser?.Username;
        }

        private void DeleteUser()
        {
            if (SelectedUser == null)
                return;

            var DialogResult = MessageBox.Show("Are you sure you want to delete this user?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if(DialogResult != MessageBoxResult.Yes)
                return;
            _userService.DeleteUser(SelectedUser.Id);


            ClearEditor(); 
            LoadUsers();
        }

        // ✅ Clear Editor

        private void ClearEditor()
        {
            SelectedUser = null;
            EditorName = string.Empty;
            EditorRole = UserRole.Cashier;
            EditorPin = string.Empty;
            EditorIsActive = true;
            OnPropertyChanged(nameof(PinDisplay));
        }

        // ✅ New User

        private void NewUser()
        {
            ClearEditor();
        }

        // ✅ Keyboard Input Handling
        private void SetActiveField(UserEditorField field)
        {
            _activeField = field;
            OnPropertyChanged(nameof(IsNameActive));
            OnPropertyChanged(nameof(IsPinActive));
            OnPropertyChanged(nameof(IsSearchActive));
        }

        private void AppendKey(string? key)
        {
            if (key == null) return;

            switch (_activeField)
            {
                case UserEditorField.Name:
                    EditorName += key;
                    break;

                case UserEditorField.Pin:
                    // PIN: digits only, max 4
                    if (!char.IsDigit(key[0])) return;
                    if (EditorPin.Length >= 4) return;
                    EditorPin += key;
                    OnPropertyChanged(nameof(PinDisplay));
                    break;

                case UserEditorField.Search:
                    SearchText += key;
                    break;
            }
        }

        private void Backspace()
        {
            switch (_activeField)
            {
                case UserEditorField.Name when EditorName.Length > 0:
                    EditorName = EditorName[..^1];
                    break;

                case UserEditorField.Pin when EditorPin.Length > 0:
                    EditorPin = EditorPin[..^1];
                    OnPropertyChanged(nameof(PinDisplay));
                    break;
                case UserEditorField.Search when SearchText.Length > 0:
                    SearchText = SearchText[..^1];
                    OnPropertyChanged(nameof(SearchText));
                    break;
            }
        }

        private void ClearActive()
        {
            switch (_activeField)
            {
                case UserEditorField.Name:
                    EditorName = string.Empty;
                    break;
                case UserEditorField.Pin:
                    EditorPin = string.Empty;
                    OnPropertyChanged(nameof(PinDisplay));
                    break;
                case UserEditorField.Search:
                    SearchText = string.Empty;
                    OnPropertyChanged(nameof(SearchText));
                    break;
            }
        }
    }

}
