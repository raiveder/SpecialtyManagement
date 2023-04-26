using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        }

        private void BtnOffset_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}