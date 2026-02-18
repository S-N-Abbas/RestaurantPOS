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
                CanDeleteUser();
            }
        }

        // ✅ Editor Fields

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

        // ✅ Commands

        public ICommand SaveUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ClearEditorCommand { get; }
        public ICommand NewUserCommand { get; }

        public UsersViewModel(IUserService userService, UserSessionService userSessionService)
        {
            _userService = userService;
            _userSessionService = userSessionService;

            LoadUsers();

            SaveUserCommand = new RelayCommand(SaveUser);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanDeleteUser);
            ClearEditorCommand = new RelayCommand(ClearEditor);
            NewUserCommand = new RelayCommand(NewUser);
        }

        private void LoadUsers()
        {
            Users = new ObservableCollection<User>(
                _userService.GetAllUsers());
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
        }

        // ✅ New User

        private void NewUser()
        {
            ClearEditor();
        }
    }

}
