using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProductionManagementApp.WPF
{
    /// <summary>
    /// Interaction logic for CreateUser.xaml
    /// </summary>
    public partial class CreateUser : Window
    {
        string userprofilespath;
        List<string> selectedUsers;

        public CreateUser()
        {
            InitializeComponent();
            userprofilespath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\ProductionManagementApp.Core\Users\Users.txt";
            usernames.ItemsSource = File.ReadAllLines(Directory.GetParent(userprofilespath).FullName + @"\Usernames.txt").Select((i) =>
            {
                var resource = i.Split(',');
                return resource[0] + ' ' + resource[1] + ' ' + resource[2];
            });
            selectedUsers = new List<string>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(client.SelectedItem is null)
            {
                MessageBox.Show("Please Select A Client");
                return;
            }
            if(username.Text is "")
            {
                MessageBox.Show("Please Enter a Profile Name First");
                return;
            }
            else
            {                
                List<string> currentUsers = File.ReadAllLines(userprofilespath).ToList();
                if (currentUsers.Contains(username.Text))
                {
                    var window = new OverwriteUser();
                    window.Yes.Click += OnYesClicked;
                    window.Show();
                    return;
                }

                File.AppendAllLines(userprofilespath, new string[] { username.Text });
                WriteUserNames(userprofilespath);
            }
            Close();
        }

        private void OnYesClicked(object sender, RoutedEventArgs e)
        {
            WriteUserNames(userprofilespath);
        }

        private void WriteUserNames(string path)
        {
            var users = usernames.SelectedItems;
            List<string> userslist = new List<string>();
            userslist.Add(((RadioButton)client.SelectedItem).Content.ToString());

            foreach (var user in users)
            {
                var resource = user.ToString().Split(' ');
                
                userslist.Add(resource[0] + ',' + resource[1] + ',' + resource[2]);
            }

            File.WriteAllLines(Directory.GetParent(path).FullName + @"\" + username.Text + @".txt", userslist.ToArray());
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            client.SelectedItem = sender;
        }

        private void search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string usernamespath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\ProductionManagementApp.Core\Users\Usernames.txt";
            var pos = search.CaretIndex;
            search.Text = search.Text.ToUpper();

            List<string> userslike = File.ReadAllLines(usernamespath).Where((i) => i.Contains(search.Text)).Select((i) =>
            {
                var resource = i.Split(',');
                return resource[0] + ' ' + resource[1] + ' ' + resource[2];

            }).ToList();

            usernames.ItemsSource = userslike;

            foreach (var name in selectedUsers)
            {
                usernames.SelectedItems.Add(name);
            }

            search.CaretIndex = pos;

        }
    }
}
