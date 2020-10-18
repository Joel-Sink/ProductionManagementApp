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
using System.Windows.Shapes;

namespace ProductionManagementApp.WPF
{
    /// <summary>
    /// Interaction logic for EditUser.xaml
    /// </summary>
    public partial class EditUser : Window
    {
        public string userpath;
        string usernamespath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\ProductionManagementApp.Core\Users\Usernames.txt";

        List<string> selectedUsers;

        public EditUser()
        {
            InitializeComponent();
            usernames.ItemsSource = File.ReadAllLines(usernamespath).Select((i) =>
            {
                var resource = i.Split(',');
                return resource[0] + ' ' + resource[1] + ' ' + resource[2];
            });

            selectedUsers = new List<string>();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            client.SelectedItem = sender;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(newuser.Text.Length > 0)
            {
                WriteUserNames(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\ProductionManagementApp.Core\Users\"
                    + newuser.Text + ".txt");
                File.AppendAllLines(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\ProductionManagementApp.Core\Users\Users.txt", 
                    new string[] { newuser.Text });
            }
            else
            {
                WriteUserNames(userpath);
            }
            
            Close();
        }

        private void WriteUserNames(string path)
        {
            var users = selectedUsers;
            List<string> userslist = new List<string>();
            userslist.Add(((RadioButton)client.SelectedItem).Content.ToString());

            foreach (var user in users)
            {                
                userslist.Add(user.Replace(' ', ','));
            }

            File.WriteAllLines(path, userslist.ToArray());
        }

        private void usernames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach(var added in e.AddedItems)
            {
                if (!selectedUsers.Contains(added.ToString()))
                { 
                    selectedUsers.Add(added.ToString());
                }
                
            }

            foreach(var removed in e.RemovedItems)
            {
                if(usernames.Items.Contains(removed))
                {
                    selectedUsers.Remove(removed.ToString());
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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
