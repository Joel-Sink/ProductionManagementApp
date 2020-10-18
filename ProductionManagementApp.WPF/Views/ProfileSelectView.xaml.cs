using MvvmCross.Base;
using MvvmCross.Binding.BindingContext;
using MvvmCross.ViewModels;
using ProductionManagementApp.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ProductionManagementApp.Core.ViewModels.ProfileSelectViewModel;

namespace ProductionManagementApp.WPF.Views
{
    /// <summary>
    /// Interaction logic for UserSelectView.xaml
    /// </summary>
    [MvxViewFor(typeof(ProfileSelectViewModel))]
    public partial class UserSelectView
    {
        private IMvxInteraction<CreateUserClass> _createUserInteraction;

        public IMvxInteraction<CreateUserClass> CreateUserInteraction
        {
            get { return _createUserInteraction; }
            set 
            {
                if(_createUserInteraction != null)
                {
                    _createUserInteraction.Requested -= OnCreateUserInteractionRequested;
                }

                _createUserInteraction = value;
                _createUserInteraction.WeakSubscribe(OnCreateUserInteractionRequested);
            }
        }

        private void OnCreateUserInteractionRequested(object sender, MvxValueEventArgs<CreateUserClass> e)
        {
            var window = new CreateUser();
            window.Show();
        }

        private IMvxInteraction<EditUserClass> _editUserInteraction;

        public IMvxInteraction<EditUserClass> EditUserInteraction
        {
            get { return _editUserInteraction; }
            set 
            {
                if (_editUserInteraction != null)
                {
                    _editUserInteraction.Requested -= OnEditUserInteractionRequested;
                }

                _editUserInteraction = value;
                _editUserInteraction.WeakSubscribe(OnEditUserInteractionRequested);
            }
        }

        private void OnEditUserInteractionRequested(object sender, MvxValueEventArgs<EditUserClass> e)
        {
            if (!(list.SelectedItem is null))
            {
                var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\ProductionManagementApp.Core\Users\" + list.SelectedItem.ToString() + ".txt";
                
                var window = new EditUser();                
                window.userpath = path;
                window.user.Text = "User: " + list.SelectedItem.ToString();


                var fileclient = File.ReadAllLines(path)[0];
                var fileusers = File.ReadAllLines(path);
                fileusers = fileusers.Where((i) => i != fileusers.First()).Select((i) => i.Replace(' ', ',')).ToArray();

                foreach(var i in window.client.Items)
                {
                    if (((RadioButton)i).Content.Equals(fileclient))
                    {
                        window.client.SelectedItem = i;
                        ((RadioButton)i).IsChecked = true;
                    }
                }

                foreach (var i in fileusers)
                {
                    foreach(var h in window.usernames.Items)
                    {
                        if (h.ToString().Equals(i.Replace(',', ' ')))
                        {
                            window.usernames.SelectedItems.Add(h);
                        }
                    }
                    window.usernames.Focus();
                }

                window.Show();
            }
            else
            {
                MessageBox.Show("Please Select A User To Edit First");
            }
        }

        public UserSelectView()
        {
            InitializeComponent();
            var set = this.CreateBindingSet<UserSelectView, ProfileSelectViewModel>();
            set.Bind(this).For(view => view.CreateUserInteraction).To(viewmodel => viewmodel.CreateUserInteraction).OneWay();
            set.Apply();
            
            var set1 = this.CreateBindingSet<UserSelectView, ProfileSelectViewModel>();
            set1.Bind(this).For(view => view.EditUserInteraction).To(viewmodel => viewmodel.EditUserInteraction).OneWay();
            set1.Apply();
        }
    }
}
