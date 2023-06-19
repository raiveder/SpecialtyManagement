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
        private bool _isShowWarnings = false; // Для отсутствия предупреждений о результатах фильтрации при загрузке страницы.

        public ArrearsShowPage()
        {
            UploadPage();

            RBCurrentSemester.IsChecked = true;
            CBGroup.SelectedIndex = 0;
            CBType.SelectedIndex = 0;
            CBSort.SelectedIndex = 1;

            _isShowWarnings = true;
        }

        public ArrearsShowPage(Filter filter)
        {
            UploadPage();

            if (filter.IsCurrentSemester)
            {
                RBLastSemester.IsChecked = false;
                RBCurrentSemester.IsChecked = true;
            }
            else
            {
                RBLastSemester.IsChecked = true;
                RBCurrentSemester.IsChecked = false;
            }

            TBoxFind.Text = filter.FindText;
            CBGroup.SelectedIndex = filter.IndexGroup;
            CBType.SelectedIndex = filter.IndexType;
            CBSort.SelectedIndex = filter.IndexSort;
        }

        /// <summary>
        /// Настраивает элементы управления страницы.
        /// </summary>
        private void UploadPage()
        {
            InitializeComponent();

            List<Groups> groups = Database.Entities.Groups.ToList();
            groups.Sort((x, y) => x.Group.ToLower().CompareTo(y.Group.ToLower()));
            groups.Insert(0, new Groups()
            {
                Id = 0,
                Group = "Все группы"
            });

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
            Arrears.GetYearAndSemester(out int year, out int semesterNumber, (bool)RBCurrentSemester.IsChecked);

            List<Arrears> arrears = Database.Entities.Arrears.Where(x => x.StartYear == year && x.SemesterNumber == semesterNumber).ToList();

            if (CBType.SelectedIndex > 0)
            {
                Arrears.DeleteArrearsNotMatchByType(arrears, (int)CBType.SelectedValue);
            }
            else
            {
                foreach (Arrears item in arrears)
                {
                    item.CountArrears = Database.Entities.ArrearsLessons.Where(x => x.IdArrear == item.Id).Count();
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
                case 0:
                    arrears.Sort((x, y) => x.Students.FullName.CompareTo(y.Students.FullName));
                    break;
                case 1:
                    arrears.Sort((x, y) => x.Students.Groups.Group.CompareTo(y.Students.Groups.Group));
                    break;
                case 2:
                    arrears.Sort((x, y) => x.CountArrears.CompareTo(y.CountArrears));
                    break;
                case 3:
                    arrears.Sort((x, y) => x.Students.FullName.CompareTo(y.Students.FullName));
                    arrears.Reverse();
                    break;
                case 4:
                    arrears.Sort((x, y) => x.Students.Groups.Group.CompareTo(y.Students.Groups.Group));
                    arrears.Reverse();
                    break;
                case 5:
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

            if (_isShowWarnings && DGArrears.Items.Count == 0)
            {
                MessageBox.Show("Подходящих фильтру задолженностей не найдено", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void WPLessons_Loaded(object sender, RoutedEventArgs e)
        {
            WrapPanel panel = sender as WrapPanel;
            int id = Convert.ToInt32(panel.Uid);
            panel.Children.Clear();

            List<ArrearsLessons> arrears = Database.Entities.ArrearsLessons.Where(x => x.IdArrear == id).ToList();

            if (CBType.SelectedIndex > 0)
            {
                arrears = arrears.Where(x => x.IdType == (int)CBType.SelectedValue).ToList();
            }

            foreach (ArrearsLessons item in arrears)
            {
                TextBlock tb = new TextBlock()
                {
                    Text = item.Lessons.ShortName
                };

                if (item.IsLiquidated)
                {

                    tb.Foreground = Brushes.Blue; // Ликвидированная задолженность.
                }
                else
                {
                    if (item.Arrears.Students.IsExpelled)
                    {
                        tb.Foreground = Brushes.PaleVioletRed; // Задолженность, не сданная по причине отчисления.
                    }
                    else if (item.Arrears.Students.IsAcademic)
                    {
                        tb.Foreground = Brushes.Brown; // Задолженность, не сданная по причине академического отпуска.
                    }
                    else if (item.IsGoodReason)
                    {
                        tb.Foreground = Brushes.Green; // Задолженность, не сданная по уважительной причине.
                    }
                    else if (item.IdType == 2)
                    {
                        tb.Foreground = Brushes.Red; // Комиссионная задолженность.
                    }
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
            if (DGArrears.SelectedItems.Count > 0)
            {
                MIChange.Visibility = Visibility.Visible;
                MIDelete.Visibility = Visibility.Visible;

                if (DGArrears.SelectedItems.Count > 1)
                {
                    MIChange.Visibility = Visibility.Collapsed;
                }

                CMArrears.IsOpen = true;
            }
        }

        private void DGArrears_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void MIChange_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new ArrearAddPage(GetFilter(), DGArrears.SelectedItem as Arrears));
        }

        private void MIDelete_Click(object sender, RoutedEventArgs e)
        {
            foreach (Arrears item in DGArrears.SelectedItems)
            {
                Database.Entities.Arrears.Remove(item);
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
                    "При удалении " + (DGArrears.SelectedItems.Count == 1 ? "задолженности" : "задолженностей") + " возникла ошибка\nТекст ошибки: " + ex.Message,
                    "Задолженности",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void CMArrears_Closed(object sender, RoutedEventArgs e)
        {
            DGArrears.SelectedItems.Clear();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (Database.Entities.Lessons.FirstOrDefault() == null)
            {
                MessageBox.Show("Сначала добавьте хотя бы 1-у дисциплину, прежде чем добавлять задолженность", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Database.Entities.Students.FirstOrDefault() == null)
            {
                MessageBox.Show("Сначала добавьте хотя бы 1-го студента, прежде чем добавлять задолженность", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Database.Entities.DistributionLessons.FirstOrDefault() == null)
            {
                MessageBox.Show("Сначала добавьте хотя бы 1-го преподавателя, который будет вести какие-либо дисциплины, прежде чем добавлять задолженность", "Дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Navigation.Frame.Navigate(new ArrearAddPage(GetFilter()));
        }

        private void MIPrimaryArrears_Click(object sender, RoutedEventArgs e)
        {
            if (CanCreateDocument())
            {
                if (Database.Entities.TypesLessons.FirstOrDefault(x => x.Type == "ПМ") != null)
                {
                    List<Arrears> arrears = new List<Arrears>();
                    foreach (Arrears item in DGArrears.Items)
                    {
                        if (!item.Students.IsExpelled && !IsAllArrearsLiquidated(item, 1))
                        {
                            arrears.Add(item);
                        }
                    }
                    Arrears.DeleteArrearsNotMatchByType(arrears, 1); // 1 - Id первичной задолженности.

                    if (arrears.Count > 0)
                    {
                        Navigation.Frame.Navigate(new ArrearsPrimaryCreateDocumentPage(GetFilter(), arrears));
                    }
                    else
                    {
                        MessageBox.Show("В списке отсутствуют неликвидированные первичные задолженности, которые имеются у неотчисленных студентов", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Отсутствует тип дисциплины \"ПМ\". Добавьте его, прежде чем формировать протокол о первичных задолженностях", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void MIComissionArrears_Click(object sender, RoutedEventArgs e)
        {
            if (CanCreateDocument())
            {
                List<Arrears> arrears = new List<Arrears>();
                foreach (Arrears item in DGArrears.Items)
                {
                    if (!item.Students.IsExpelled && !IsAllArrearsLiquidated(item, 2))
                    {
                        arrears.Add(item);
                    }
                }
                Arrears.DeleteArrearsNotMatchByType(arrears, 2); // 2 - Id комиссионной задолженности.

                if (arrears.Count > 0)
                {
                    Navigation.Frame.Navigate(new ArrearsComissionCreateDocumentPage(GetFilter(), arrears));
                }
                else
                {
                    MessageBox.Show("В списке отсутствуют неликвидированные комисионные задолженности, которые имеются у неотчисленных студентов", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        /// <summary>
        /// Проверяет, есть ли среди задолженностей неликвидированные.
        /// </summary>
        /// <param name="arrear">задолженность.</param>
        /// <param name="idTypeArrear">индекс типа задолженности.</param>
        /// <returns>True, если все задолженности ликвидированы, в противном случае - false.</returns>
        private bool IsAllArrearsLiquidated(Arrears arrear, int idTypeArrear)
        {
            int countArrears = 0;
            foreach (ArrearsLessons item in Database.Entities.ArrearsLessons.Where(x => x.IdArrear == arrear.Id && x.IdType == idTypeArrear))
            {
                if (item.IsLiquidated)
                {
                    countArrears++;
                }
            }

            return countArrears == Database.Entities.ArrearsLessons.Where(x => x.IdArrear == arrear.Id && x.IdType == idTypeArrear).Count();
        }

        /// <summary>
        /// Проверяет, можно ли создавать документ.
        /// </summary>
        /// <returns>True, если все данные для документа есть в базе данных, в противном случае - false.</returns>
        private bool CanCreateDocument()
        {
            if (DGArrears.Items.Count == 0)
            {
                MessageBox.Show("Список задолженностей для формирования документов пуст", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (Database.Entities.Specialty.FirstOrDefault() == null)
            {
                MessageBox.Show("Специальность не указана. Перейдите в пункт меню \"Настройки\"", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (Database.Entities.Lessons.FirstOrDefault() == null)
            {
                MessageBox.Show("Список дисциплин пуст. Добавьте дисциплины", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (Database.Entities.Teachers.FirstOrDefault() == null)
            {
                MessageBox.Show("Список преподавателей пуст. Добавьте преподавателей", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
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
                IndexType = CBType.SelectedIndex,
                IndexGroup = CBGroup.SelectedIndex,
                IsCurrentSemester = (bool)RBCurrentSemester.IsChecked,
                IndexSort = CBSort.SelectedIndex
            };
        }
    }
}