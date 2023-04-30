using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для ArrearsShowPage.xaml
    /// </summary>
    public partial class ArrearsShowPage : Page
    {
        public ArrearsShowPage()
        {
            UploadPage();

            RBCurrentSemester.IsChecked = true;
            CBGroup.SelectedIndex= 0;
            CBType.SelectedIndex= 0;
            CBSort.SelectedIndex= 0;
        }

        public ArrearsShowPage(Filter filter)
        {
            UploadPage();

            TBoxFind.Text = filter.FindText;
            CBGroup.SelectedIndex = filter.IndexGroup;
            CBSort.SelectedIndex = filter.IndexSort;
            RBLastSemester.IsChecked = filter.IsLastSemester;
            RBCurrentSemester.IsChecked = filter.IsCurrentSemester;
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

            List<TypesArrears> typesArrears = new List<TypesArrears>()
            {
                new TypesArrears()
                {
                    Id = 0,
                    Type = "Все типы"
                }
            };

            typesArrears.AddRange(Database.Entities.TypesArrears.ToList());

            CBType.ItemsSource = typesArrears;
            CBType.SelectedValuePath = "Id";
            CBType.DisplayMemberPath = "Type";
        }

        private void TBoxFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetFilter();
        }

        private void CBFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetFilter();
        }

        private void RBLastSemester_Checked(object sender, RoutedEventArgs e)
        {
            RBCurrentSemester.IsChecked = false;
            SetFilter();
        }

        private void RBCurrentSemester_Checked(object sender, RoutedEventArgs e)
        {
            RBLastSemester.IsChecked = false;
            SetFilter();
        }

        /// <summary>
        /// Устанавливает фильтр для вывода данных.
        /// </summary>
        private void SetFilter()
        {
            List<Arrears> arrears = new List<Arrears>();

            if ((bool)RBLastSemester.IsChecked)
            {
                if (DateTime.Today >= new DateTime(DateTime.Today.Year, 9, 1) && DateTime.Today <= new DateTime(DateTime.Today.Year, 12, 31))
                {
                    arrears = Database.Entities.Arrears.Where(x => x.StartYear == DateTime.Today.Year && x.SemesterNumber == 2).ToList();
                }
                else
                {
                    arrears = Database.Entities.Arrears.Where(x => x.StartYear == DateTime.Today.Year - 1 && x.SemesterNumber == 1).ToList();
                }
            }
            else
            {
                if (DateTime.Today >= new DateTime(DateTime.Today.Year, 9, 1) && DateTime.Today <= new DateTime(DateTime.Today.Year, 12, 31))
                {
                    arrears = Database.Entities.Arrears.Where(x => x.StartYear == DateTime.Today.Year && x.SemesterNumber == 1).ToList();
                }
                else
                {
                    arrears = Database.Entities.Arrears.Where(x => x.StartYear == DateTime.Today.Year && x.SemesterNumber == 2).ToList();
                }
            }

            if (CBType.SelectedIndex > 0)
            {
                arrears = arrears.Where(x => x.TypesArrears.Id == Convert.ToInt32(CBType.SelectedValue)).ToList();
            }

            if (CBGroup.SelectedIndex > 0)
            {
                arrears = arrears.Where(x => x.Students.IdGroup == Convert.ToInt32(CBGroup.SelectedValue)).ToList();
            }

            if (TBoxFind.Text.Length > 0)
            {
                arrears = arrears.Where(x => x.Students.FullName.ToLower().Contains(TBoxFind.Text.ToLower())).ToList();
            }

            switch (CBSort.SelectedIndex)
            {
                case 1:
                    arrears.Sort((x, y) => x.Students.FullName.CompareTo(y.Students.FullName));
                    break;
                case 2:
                    arrears.Sort((x, y) => x.Students.Groups.Group.CompareTo(y.Students.Groups.Group));
                    break;
                case 3:
                    arrears.Sort((x, y) => x.CountArrears.CompareTo(y.CountArrears));
                    break;
                case 4:
                    arrears.Sort((x, y) => x.Students.FullName.CompareTo(y.Students.FullName));
                    arrears.Reverse();
                    break;
                case 5:
                    arrears.Sort((x, y) => x.Students.Groups.Group.CompareTo(y.Students.Groups.Group));
                    arrears.Reverse();
                    break;
                case 6:
                    arrears.Sort((x, y) => x.CountArrears.CompareTo(y.CountArrears));
                    arrears.Reverse();
                    break;
                default:
                    break;
            }

            int number = 1;
            foreach (Arrears item in arrears)
            {
                item.SequenceNumber = number++;
            }

            DGArrears.ItemsSource = arrears;

            if (arrears.Count == 0)
            {
                MessageBox.Show("Подходящих фильтру задолженностей не найдено", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DGArrears_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void DGArrears_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CMArrears.IsOpen = true;
        }

        private void MIChange_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MIDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CMArrears_Closed(object sender, RoutedEventArgs e)
        {

        }

        private void MISheduleTeachers_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MISheduleRetakes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MIIndividualSheduleRetakes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MIComissionRetakes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MIAddArrear_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MIAddShedule_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
