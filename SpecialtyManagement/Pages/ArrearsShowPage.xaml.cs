using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для ArrearsShowPage.xaml
    /// </summary>
    public partial class ArrearsShowPage : Page
    {
        private List<int> _idArrearsWithoutLessons = new List<int>();

        public ArrearsShowPage()
        {
            UploadPage();

            RBCurrentSemester.IsChecked = true;
            CBGroup.SelectedIndex = 0;
            CBType.SelectedIndex = 0;
            CBSort.SelectedIndex = 0;
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
                List<Arrears> arrearsToRemove = new List<Arrears>();

                foreach (Arrears item in arrears) // Поиск задолженностей, у которых нет дисциплин для данного типа.
                {
                    if (Database.Entities.ArrearsLessons.Where(x => x.IdArrear == item.Id && x.IdType == (int)CBType.SelectedValue).Count() == 0)
                    {
                        arrearsToRemove.Add(item);
                    }
                }

                foreach (Arrears item in arrearsToRemove)
                {
                    arrears.Remove(item);
                }
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

            if (DGArrears.Items.Count == 0)
            {
                MessageBox.Show("Подходящих фильтру задолженностей не найдено", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void WPLessons_Loaded(object sender, RoutedEventArgs e)
        {
            WrapPanel panel = sender as WrapPanel;

            int id = Convert.ToInt32(panel.Uid);

            List<ArrearsLessons> arrears = Database.Entities.ArrearsLessons.Where(x => x.IdArrear == id).ToList();

            if (CBType.SelectedIndex > 0)
            {
                arrears = arrears.Where(x => x.IdArrear == id && x.IdType == (int)CBType.SelectedValue).ToList();
            }

            foreach (ArrearsLessons item in arrears)
            {
                TextBlock tb = new TextBlock()
                {
                    Text = item.Lessons.ShortName
                };

                if (!item.IsLiquidated)
                {
                    if (item.IdType == 2)
                    {
                        tb.Foreground = Brushes.Red;
                    }

                    switch (item.Reason)
                    {
                        case 1:
                            tb.Foreground = Brushes.Green;
                            break;
                        case 2:
                            tb.Foreground = Brushes.PaleVioletRed;
                            break;
                        case 3:
                            tb.Foreground = Brushes.Brown;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    tb.Foreground = Brushes.Blue;
                }

                tb.Text += ", ";

                panel.Children.Add(tb);
            }

            if (panel.Children.Count > 0) // Удаление последней запятой.
            {
                TextBlock lastBlock = panel.Children[panel.Children.Count - 1] as TextBlock;
                lastBlock.Text = lastBlock.Text.Substring(0, lastBlock.Text.Length - 2);
            }
        }

        private void DGArrears_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CMArrears.IsOpen = true;
        }

        private void DGArrears_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
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

        private void MISheduleRetakes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MIIndividualSheduleRetakes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MIComissionRetakes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
