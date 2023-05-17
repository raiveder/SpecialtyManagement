using SpecialtyManagement.Windows;
using System.Collections.Generic;
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
            UpdateView();
        }



        // Сделать группы MenuItem-ами.




        /// <summary>
        /// Обновляет визуальное отображение списков.
        /// </summary>
        /// <param name="itemsSelected">выбранные элементы.</param>
        /// <param name="itemsSource">элементы для выбора.</param>
        private void UpdateView()
        {
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
                UpdateView();
            }
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

        private void BtnOffset_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите осуществить смещение групп?", "Группы", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (Groups item in Database.Entities.Groups.Where(x => x.Group.Substring(0, 1) == "4"))
                {
                    Database.Entities.Groups.Remove(item);
                }

                List<Groups> groups = Database.Entities.Groups.Where(x => x.Group.Substring(0, 1) == "3").ToList();
                foreach (Groups item in groups)
                {
                    Groups group = new Groups() { Group = "4" + item.Group.Substring(1, item.Group.Length - 1) };
                    Database.Entities.Groups.Add(group);
                    Database.Entities.SaveChanges();

                    foreach (Students student in Database.Entities.Students.Where(x => x.IdGroup == item.Id))
                    {
                        student.IdGroup = group.Id;
                    }

                    Database.Entities.Groups.Remove(item);
                }

                groups = Database.Entities.Groups.Where(x => x.Group.Substring(0, 1) == "2").ToList();
                foreach (Groups item in groups)
                {
                    Groups group = new Groups() { Group = "3" + item.Group.Substring(1, item.Group.Length - 1) };
                    Database.Entities.Groups.Add(group);
                    Database.Entities.SaveChanges();

                    foreach (Students student in Database.Entities.Students.Where(x => x.IdGroup == item.Id))
                    {
                        student.IdGroup = group.Id;
                    }

                    Database.Entities.Groups.Remove(item);
                }

                Database.Entities.SaveChanges();
                UpdateView();
            }
        }
    }
}