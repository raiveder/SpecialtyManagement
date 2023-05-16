using SpecialtyManagement.Windows;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для GroupsShowPage.xaml
    /// </summary>
    public partial class GroupsShowPage : Page
    {
        public GroupsShowPage()
        {
            InitializeComponent();

            LVFirstYear.ItemsSource = Database.Entities.Groups.Where(x => x.Group.Substring(0, 1) == "1").ToList();
            LVSecondYear.ItemsSource = Database.Entities.Groups.Where(x => x.Group.Substring(0, 1) == "2").ToList();
            LVThirdYear.ItemsSource = Database.Entities.Groups.Where(x => x.Group.Substring(0, 1) == "3").ToList();
            LVFourthYear.ItemsSource = Database.Entities.Groups.Where(x => x.Group.Substring(0, 1) == "4").ToList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            GroupAddWindow window = new GroupAddWindow();
            window.ShowDialog();

            if ((bool)window.DialogResult)
            {
                Navigation.Frame.Navigate(new GroupsShowPage());
            }
        }

        private void BtnOffset_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnGroup_Click(object sender, RoutedEventArgs e)
        {
            GroupAddWindow window = new GroupAddWindow((sender as Button).DataContext as Groups);
            window.ShowDialog();

            if ((bool)window.DialogResult)
            {
                Navigation.Frame.Navigate(new GroupsShowPage());
            }
        }
    }
}