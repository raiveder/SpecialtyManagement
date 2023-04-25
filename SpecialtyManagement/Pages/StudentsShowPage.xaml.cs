using Microsoft.Win32;
using SpecialtyManagement.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            UploadPage();

            CBGroup.SelectedIndex = 0;
            CBSort.SelectedIndex = 0;
        }

        public StudentsShowPage(Filter filter)
        {
            UploadPage();

            TBoxFind.Text = filter.FindText;
            CBGroup.SelectedIndex = filter.IndexGroup;
            CBSort.SelectedIndex = filter.IndexSort;
            ChBNote.IsChecked = filter.HasNote;
        }

        /// <summary>
        /// Настраивает элементы управления страницы.
        /// </summary>
        /// <param name="filter">Настройки фильтра.</param>
        private void UploadPage()
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
            CBGroup.SelectedValuePath = "Id";
            CBGroup.DisplayMemberPath = "Group";
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
                case 4:
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

            int number = 1;
            foreach (Students item in students)
            {
                item.SequenceNumber = number++;
            }

            DGStudents.ItemsSource = students;

            if (students.Count == 0)
            {
                MessageBox.Show("Подходящих фильтру студентов не найдено", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MIAdd_Click(object sender, RoutedEventArgs e)
        {
            Filter filter = new Filter()
            {
                FindText = TBoxFind.Text,
                IndexGroup = CBGroup.SelectedIndex,
                IndexSort = CBSort.SelectedIndex,
                HasNote = (bool)ChBNote.IsChecked
            };

            Navigation.Frame.Navigate(new StudentAddPage(filter));
        }

        private void MIReadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ofd.Filter = "Все файлы|*.*|CSV|*.csv";
            ofd.FilterIndex = 2;
            ofd.ShowDialog();

            if (ofd.FileName.Length > 0)
            {
                List<Students> students = Students.GetStudentsFromFile(ofd.FileName);

                if (students.Count != 0)
                {
                    ChoiceGroupWindow window = new ChoiceGroupWindow(students)
                    {
                        Text = "Добавление студентов"
                    };
                    window.ShowDialog();

                    Database.Entities.Students.AddRange(students);

                    try
                    {
                        Database.Entities.SaveChanges();

                        CBGroup.SelectedValue = students[0].IdGroup;

                        MessageBox.Show("Студенты успешно добавлены", "Студенты", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("При добавлении студентов возникла ошибка", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void MIChange_Click(object sender, RoutedEventArgs e)
        {
            if (DGStudents.SelectedItems.Count == 1)
            {
                Filter filter = new Filter()
                {
                    FindText = TBoxFind.Text,
                    IndexGroup = CBGroup.SelectedIndex,
                    IndexSort = CBSort.SelectedIndex,
                    HasNote = (bool)ChBNote.IsChecked
                };

                Navigation.Frame.Navigate(new StudentAddPage(filter, DGStudents.SelectedItem as Students));
            }
            else
            {
                MessageBox.Show("Выберите одного студента", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MIExpel_Click(object sender, RoutedEventArgs e)
        {
            List<Students> students = new List<Students>();
            List<ExpelledStudents> ecpelledStudents = new List<ExpelledStudents>();

            foreach (Students item in DGStudents.SelectedItems)
            {
                students.Add(item);

                ecpelledStudents.Add(new ExpelledStudents
                {
                    Surname = item.Surname,
                    Name = item.Name,
                    Patronymic = item.Patronymic,
                    IdGroup = item.IdGroup,
                    Birthday = item.Birthday,
                    Note = item.Note
                });
            }


            Database.Entities.Students.RemoveRange(students);
            Database.Entities.ExpelledStudents.AddRange(ecpelledStudents);

            try
            {
                Database.Entities.SaveChanges();
                SetFilter();
            }
            catch (Exception)
            {
                MessageBox.Show
                (
                    "При отчислении " + (DGStudents.SelectedItems.Count == 1 ? "студента" : "студентов") + " возникла ошибка",
                    "Студенты",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void DGStudents_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CMStudents.IsOpen = true;
        }

        private void DGStudents_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void CMStudents_Closed(object sender, RoutedEventArgs e)
        {
            DGStudents.SelectedItems.Clear();
        }

        private void BtnCertification_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}