using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для TeahersShowPage.xaml
    /// </summary>
    public partial class TeahersShowPage : Page
    {
        private bool _isShowWarnings = false; // Для отсутствия предупреждений о результатах фильтрации при загрузке страницы.

        public TeahersShowPage()
        {
            InitializeComponent();

            CBSort.SelectedIndex = 0;
            _isShowWarnings = true;
        }

        public TeahersShowPage(Filter filter)
        {
            InitializeComponent();

            TBoxFind.Text = filter.FindText;
            CBSort.SelectedIndex = filter.IndexSort;
        }

        private void TBoxFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetFilter();
        }

        private void CBSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetFilter();
        }

        /// <summary>
        /// Устанавливает фильтр для вывода данных.
        /// </summary>
        private void SetFilter()
        {
            List<Teachers> teachers = Database.Entities.Teachers.ToList();

            if (TBoxFind.Text.Length > 0)
            {
                teachers = teachers.Where(x => x.FullName.ToLower().Contains(TBoxFind.Text.ToLower())).ToList();
            }

            switch (CBSort.SelectedIndex)
            {
                case 1:
                    teachers.Sort((x, y) => x.FullName.CompareTo(y.FullName));
                    break;
                case 2:
                    teachers.Sort((x, y) => x.FullName.CompareTo(y.FullName));
                    teachers.Reverse();
                    break;
                default:
                    break;
            }

            int number = 1;

            foreach (Teachers item in teachers)
            {
                item.SequenceNumber = number++;
            }

            DGTeachers.ItemsSource = teachers;

            if (_isShowWarnings && teachers.Count == 0)
            {
                MessageBox.Show("Подходящих фильтру преподавателей не найдено", "Преподаватели", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new TeacherAddPage(GetFilter()));
        }

        private void MIChange_Click(object sender, RoutedEventArgs e)
        {
            if (DGTeachers.SelectedItems.Count == 1)
            {
                Navigation.Frame.Navigate(new TeacherAddPage(GetFilter(), DGTeachers.SelectedItem as Teachers));
            }
            else
            {
                MessageBox.Show("Выберите одного преподавателя", "Преподаватели", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
                IndexSort = CBSort.SelectedIndex,
            };
        }

        private void MIDelete_Click(object sender, RoutedEventArgs e)
        {
            List<Teachers> teachers = new List<Teachers>();

            foreach (Teachers item in DGTeachers.SelectedItems)
            {
                teachers.Add(item);
            }

            Database.Entities.Teachers.RemoveRange(teachers);

            try
            {
                Database.Entities.SaveChanges();
                SetFilter();
            }
            catch (Exception)
            {
                MessageBox.Show
                (
                    "При удалении " + (DGTeachers.SelectedItems.Count == 1 ? "преподавателя" : "преподавателей") + " возникла ошибка",
                    "Преподаватели",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void DGTeachers_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CMTeachers.IsOpen = true;
        }

        private void DGTeachers_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void CMTeachers_Closed(object sender, RoutedEventArgs e)
        {
            DGTeachers.SelectedItems.Clear();
        }
    }
}