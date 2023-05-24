using SpecialtyManagement.Windows;
using System;
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

        private void MIChange_Click(object sender, RoutedEventArgs e)
        {
            GroupAddWindow window = new GroupAddWindow((sender as MenuItem).DataContext as Groups);
            window.ShowDialog();

            if ((bool)window.DialogResult)
            {
                Navigation.Frame.Navigate(new GroupsShowPage());
            }
        }

        private void MIDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("При удалении группы удалится список её студентов. Вы действительно хотите удалить группу?", "Группы", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Database.Entities.Groups.Remove((sender as MenuItem).DataContext as Groups);
                    Database.Entities.SaveChanges();
                    UpdateView();
                }
                catch (Exception ex)
                {
                    MessageBox.Show
                    (
                        "При удалении группы возникла ошибка\nТекст ошибки: " + ex.Message, "Группы", MessageBoxButton.OK, MessageBoxImage.Warning
                    );
                }
            }
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

        private void MIAll_Click(object sender, RoutedEventArgs e)
        {
            if (Database.Entities.Groups.FirstOrDefault() == null)
            {
                MessageBox.Show("Отсутствуют группы для смещения", "Группы", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Вы действительно хотите осуществить смещение групп?", "Группы", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Удаление студентов текущего 4-го курса.
                    Database.Entities.Students.RemoveRange(Database.Entities.Students.Where(x => x.Groups.Group.Substring(0, 1) == "4"));

                    // Перевод студентов с 3-го курса на 4-ый.
                    List<Groups> groups = Database.Entities.Groups.Where(x => x.Group.Substring(0, 1) == "3").ToList();
                    foreach (Groups item in groups)
                    {
                        string nextGroup = 4 + item.Group.Substring(1, item.Group.Length - 1);
                        Groups group = Database.Entities.Groups.FirstOrDefault(x => x.Group == nextGroup);
                        if (group == null)
                        {
                            group = new Groups() { Group = "4" + item.Group.Substring(1, item.Group.Length - 1) };
                            Database.Entities.Groups.Add(group);
                            Database.Entities.SaveChanges();
                        }

                        foreach (Students student in Database.Entities.Students.Where(x => x.IdGroup == item.Id))
                        {
                            student.IdGroup = group.Id;
                        }
                    }
                    Database.Entities.SaveChanges();

                    // Удаление лишних групп 4-го курса.
                    Database.Entities.Groups.RemoveRange(Database.Entities.Groups.Where(x => x.Group.Substring(0, 1) == "4" && x.Students.Count == 0).ToList());

                    // Перевод студентов со 2-го курса на 3-ый.
                    groups = Database.Entities.Groups.Where(x => x.Group.Substring(0, 1) == "2").ToList();
                    foreach (Groups item in groups)
                    {
                        string nextGroup = 3 + item.Group.Substring(1, item.Group.Length - 1);
                        Groups group = Database.Entities.Groups.FirstOrDefault(x => x.Group == nextGroup);
                        if (group == null)
                        {
                            group = new Groups() { Group = "3" + item.Group.Substring(1, item.Group.Length - 1) };
                            Database.Entities.Groups.Add(group);
                            Database.Entities.SaveChanges();
                        }

                        foreach (Students student in Database.Entities.Students.Where(x => x.IdGroup == item.Id))
                        {
                            student.IdGroup = group.Id;
                        }
                    }
                    Database.Entities.SaveChanges();

                    // Удаление лишних групп 3-го курса.
                    Database.Entities.Groups.RemoveRange(Database.Entities.Groups.Where(x => x.Group.Substring(0, 1) == "3" && x.Students.Count == 0).ToList());
                    Database.Entities.SaveChanges();

                    UpdateView();

                    if (Database.Entities.Students.FirstOrDefault(x => x.Groups.Group.Substring(0, 1) == "1") != null)
                    {
                        Navigation.Frame.Navigate(new GroupChoiceForOffsetPage());
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("При осуществлении смещения групп возникла ошибка\nТекст ошибки: " + ex.Message, "Группы", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                }
            }
        }

        private void MIFirstYear_Click(object sender, RoutedEventArgs e)
        {
            if (Database.Entities.Students.FirstOrDefault(x => x.Groups.Group.Substring(0, 1) == "1") != null)
            {
                Navigation.Frame.Navigate(new GroupChoiceForOffsetPage());
            }
            else
            {
                MessageBox.Show("Отсутствуют студенты 1-го курса", "Группы", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}