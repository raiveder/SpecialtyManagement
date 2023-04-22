using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для StudentsShowPage.xaml
    /// </summary>
    public partial class StudentsShowPage : Page
    {
        public StudentsShowPage()
        {
            InitializeComponent();

            List<Groups> groups = new List<Groups>()
            {
                new Groups()
                {
                    Id = 0,
                    Group = "Все группы"
                }
            };

            groups.AddRange(Database.Entities.Groups.ToList());

            CBGroup.ItemsSource = groups;

            CBGroup.SelectedIndex = 0;
            CBSort.SelectedIndex = 0;
        }

        private void TBoxFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetFilter();
        }

        private void CBFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetFilter();
        }

        private void ChBNote_Click(object sender, RoutedEventArgs e)
        {
            SetFilter();
        }

        private void SetFilter()
        {
            List<Students> students = Database.Entities.Students.ToList();

            if (CBGroup.SelectedIndex > 0)
            {
                students = students.Where(x => x.IdGroup == Convert.ToInt32(CBGroup.SelectedValue)).ToList();
            }

            if (TBoxFind.Text.Length > 0)
            {
                students = students.Where(x => x.FullName.ToLower().Contains(TBoxFind.Text.ToLower())).ToList();
            }

            if ((bool)ChBNote.IsChecked)
            {
                students = students.Where(x => x.Note != null).ToList();
            }

            switch (CBSort.SelectedIndex)
            {
                case 1:
                    students.Sort((x, y) => x.FullName.CompareTo(y.FullName));
                    break;
                case 2:
                    students.Sort((x, y) => x.Groups.Group.CompareTo(y.Groups.Group));
                    break;
                case 3:
                    students.Sort((x, y) => x.Birthday.CompareTo(y.Birthday));
                    break;
                case 41:
                    students.Sort((x, y) => x.FullName.CompareTo(y.FullName));
                    students.Reverse();
                    break;
                case 5:
                    students.Sort((x, y) => x.Groups.Group.CompareTo(y.Groups.Group));
                    students.Reverse();
                    break;
                case 6:
                    students.Sort((x, y) => x.Birthday.CompareTo(y.Birthday));
                    students.Reverse();
                    break;
                default:
                    break;
            }

            DGStudents.ItemsSource = students;
        }

        private void MIAdd_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new StudentAddPage());
        }

        private void MIReadFile_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}