using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using ProductionManagementApp.Core.Models;
using System.IO;
using System.Linq;

namespace ProductionManagementApp.Core.ViewModels
{
    public class ProfileSelectViewModel : MvxNavigationViewModel
    {
        object _selectedUser;

        public object SelectedUser { get => _selectedUser;
        set 
            {
                _selectedUser = value;
                RaisePropertyChanged(() => SelectedUser);
            }
        }

        private string[] _users;

        public string[] Users
        {
            get { return _users; }
            set 
            {
                _users = value;
                RaisePropertyChanged(() => Users);
            }
        }


        public MvxCommand StartReport { get; }
        public MvxCommand RefreshList { get; }
        public MvxCommand DeleteUser { get; }
        public MvxCommand EditUser { get => new MvxCommand(() => EditUserCommand()); }
        public MvxCommand CreateUser { get => new MvxCommand(() => CreateUserCommand()); }

        private MvxInteraction<CreateUserClass> _createUserInteraction =
            new MvxInteraction<CreateUserClass>();

        public IMvxInteraction<CreateUserClass> CreateUserInteraction { get => _createUserInteraction; }

        private MvxInteraction<EditUserClass> _editUserInteraction =
            new MvxInteraction<EditUserClass>();

        public IMvxInteraction<EditUserClass> EditUserInteraction { get => _editUserInteraction; }

        public ProfileSelectViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\ProductionManagementApp.Core\Users\Users.txt";
            Users = File.ReadAllLines(path);

            RefreshList = new MvxCommand(() => Users = File.ReadAllLines(path));

            StartReport = new MvxCommand(() => OpenReport());

            DeleteUser = new MvxCommand(() =>
            {
                File.Delete(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\ProductionManagementApp.Core\Users\"
                    + SelectedUser + ".txt");
                Users = Users.Where((i) => !i.Equals(SelectedUser)).ToArray();
                File.WriteAllLines(path, Users);
            });
                       
        }

        public void OpenReport()
        {
            ReportUserModel user = new ReportUserModel() { UserId = SelectedUser.ToString() };

            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\ProductionManagementApp.Core\Users\" + SelectedUser.ToString() + ".txt";

            var users = File.ReadAllLines(path).ToList();

            user.Users = users.Where((u) => !u.Equals(users.First())).ToList();

            NavigationService.Navigate<ReportViewModel, ReportUserModel>(user);
        }

        public void CreateUserCommand()
        {
            var request = new CreateUserClass();
            _createUserInteraction.Raise(request);
        }

        public void EditUserCommand()
        {
            var request = new EditUserClass();
            _editUserInteraction.Raise(request);
        }

        public class CreateUserClass
        {
        }
        
        public class EditUserClass
        {
        }
    }
}
