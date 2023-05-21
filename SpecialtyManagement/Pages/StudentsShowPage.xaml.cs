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
        private bool _isShowWarnings = false; // Для отсутствия предупреждений о результатах фильтрации при загрузке страницы.

        public StudentsShowPage()
        {
            UploadPage();

            CBGroup.SelectedIndex = 0;
            CBSort.SelectedIndex = 1;

            _isShowWarnings = true;
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

        /// <summary>
        /// Устанавливает фильтр для вывода данных.
        /// </summary>
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
                case 0:
                    students.Sort((x, y) => x.FullName.CompareTo(y.FullName));
                    break;
                case 1:
                    students.Sort((x, y) => x.Groups.Group.CompareTo(y.Groups.Group));
                    break;
                case 2:
                    students.Sort((x, y) => x.Birthday.CompareTo(y.Birthday));
                    break;
                case 3:
                    students.Sort((x, y) => x.FullName.CompareTo(y.FullName));
                    students.Reverse();
                    break;
                case 4:
                    students.Sort((x, y) => x.Groups.Group.CompareTo(y.Groups.Group));
                    students.Reverse();
                    break;
                case 5:
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

            if (_isShowWarnings && students.Count == 0)
            {
                MessageBox.Show("Подходящих фильтру студентов не найдено", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MIAdd_Click(object sender, RoutedEventArgs e)
        {
            if (Database.Entities.Groups.FirstOrDefault() != null)
            {
                Navigation.Frame.Navigate(new StudentAddPage(GetFilter()));
            }
            else
            {
                MessageBox.Show("Сначала добавьте хотя бы 1-у группу, прежде чем добавлять студента", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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

                if (students.Count > 0)
                {
                    Groups group = new Groups();
                    ChoiceElementWindow window = new ChoiceElementWindow(group, "Выберите группу добавляемых студентов");
                    window.ShowDialog();

                    if ((bool)window.DialogResult)
                    {
                        foreach (Students item in students)
                        {
                            item.IdGroup = group.Id;
                        }

                        MessageBoxResult result = MessageBoxResult.Yes;
                        if (IsStudentContainsInGroup(students, false))
                        {
                            result = MessageBox.Show("Некоторые из добавляемых студентов уже есть в этой группе. Вы уверены, что хотите добавить их снова?", "Студенты", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        }

                        if (result == MessageBoxResult.Yes)
                        {
                            Database.Entities.Students.AddRange(students);

                            try
                            {
                                Database.Entities.SaveChanges();
                                CBGroup.SelectedValue = students[0].IdGroup;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("При добавлении студентов возникла ошибка\nТекст ошибки: " + ex.Message, "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Проверяет, содержится ли хотя бы 1 студент из списка в базе данных.
        /// </summary>
        /// <param name="students">список студентов.</param>
        /// <param name="considerNote">true, если примечание учитывается, в противном случае - false.</param>
        /// <returns>True, если совпадение найдено, в противном случае - false.</returns>
        private bool IsStudentContainsInGroup(List<Students> students, bool considerNote)
        {
            int idGroup = students[0].IdGroup;

            if (considerNote)
            {
                foreach (Students studentFromDB in Database.Entities.Students.Where(x => x.IdGroup == idGroup))
                {
                    foreach (Students item in students)
                    {
                        if (studentFromDB.FullName == item.FullName && studentFromDB.Birthday == item.Birthday && studentFromDB.Note == item.Note)
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                foreach (Students studentFromDB in Database.Entities.Students.Where(x => x.IdGroup == idGroup))
                {
                    foreach (Students item in students)
                    {
                        if (studentFromDB.FullName == item.FullName && studentFromDB.Birthday == item.Birthday)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void MIChange_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new StudentAddPage(GetFilter(), DGStudents.SelectedItem as Students));
        }

        /// <summary>
        /// Получает текущие данные фильтра.
        /// </summary>
        /// <returns>Текущий фильтр.</returns>
        private Filter GetFilter()
        {
            return new Filter()
            {
                FindText = TBoxFind.Text,
                IndexGroup = CBGroup.SelectedIndex,
                IndexSort = CBSort.SelectedIndex,
                HasNote = (bool)ChBNote.IsChecked
            };
        }

        private void MIRestore_Click(object sender, RoutedEventArgs e)
        {
            foreach (Students item in DGStudents.SelectedItems)
            {
                item.IsExpelled = false;
            }

            try
            {
                Database.Entities.SaveChanges();
                SetFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show
                (
                    "При восстановлении " + (DGStudents.SelectedItems.Count == 1 ? "студента" : "студентов") + " возникла ошибка\nТекст ошибки: " + ex.Message,
                    "Студенты",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void MIExpel_Click(object sender, RoutedEventArgs e)
        {
            foreach (Students item in DGStudents.SelectedItems)
            {
                item.IsExpelled = true;
            }

            try
            {
                Database.Entities.SaveChanges();
                SetFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show
                (
                    "При отчислении " + (DGStudents.SelectedItems.Count == 1 ? "студента" : "студентов") + " возникла ошибка\nТекст ошибки: " + ex.Message,
                    "Студенты",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void MIDelete_Click(object sender, RoutedEventArgs e)
        {
            List<Students> students = new List<Students>();
            foreach (Students item in DGStudents.SelectedItems)
            {
                students.Add(item);
            }

            MessageBoxResult result;
            if (students.Count == 1)
            {
                result = MessageBox.Show("Вы действительно хотите удалить выбранного студента?", "Студенты", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }
            else
            {
                result = MessageBox.Show("Вы действительно хотите удалить выбранных студентов?", "Студенты", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }

            if (result == MessageBoxResult.Yes)
            {
                foreach (Students item in students)
                {
                    Database.Entities.Students.Remove(item);
                }

                try
                {
                    Database.Entities.SaveChanges();
                    SetFilter();
                }
                catch (Exception ex)
                {
                    if (students.Count == 1)
                    {
                        MessageBox.Show("При удалении студента возникла ошибка\nТекст ошибки: " + ex.Message, "Студенты", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("При удалении студентов возникла ошибка\nТекст ошибки: " + ex.Message, "Студенты", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void DGStudents_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DGStudents.SelectedItems.Count > 0)
            {
                MIChange.Visibility = Visibility.Visible;
                MIExpel.Visibility = Visibility.Visible;
                MIRestore.Visibility = Visibility.Visible;

                if (DGStudents.SelectedItems.Count > 1)
                {
                    MIChange.Visibility = Visibility.Collapsed;
                }

                List<Students> students = new List<Students>();
                foreach (Students item in DGStudents.SelectedItems)
                {
                    students.Add(item);
                }

                if (students.FirstOrDefault(x => x.IsExpelled) != null)
                {
                    MIExpel.Visibility = Visibility.Collapsed;
                }
                if (students.FirstOrDefault(x => !x.IsExpelled) != null)
                {
                    MIRestore.Visibility = Visibility.Collapsed;
                }

                CMStudents.IsOpen = true;
            }
        }

        private void DGStudents_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void CMStudents_Closed(object sender, RoutedEventArgs e)
        {
            DGStudents.SelectedItems.Clear();
        }
    }
}