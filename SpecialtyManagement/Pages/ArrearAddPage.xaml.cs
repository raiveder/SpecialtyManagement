using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для ArrearAddPage.xaml
    /// </summary>
    public partial class ArrearAddPage : Page
    {
        private Filter _filter;
        private Arrears _arrear;
        private List<Lessons> _lessons = new List<Lessons>();    // Список дисциплин, которые ведутся у студента.
        private List<Lessons> _lessonsSelected = new List<Lessons>();    // Список дисциплин, по которым у студента есть задолженности.
        private List<bool> _isPrimaryArrears = new List<bool>(); // Список типов задолженностей (true - первичная, false - комиссионная).
        private List<bool> _isLiquidated = new List<bool>();     // Список статусов задолженностей (true - ликвидирована, false - нет).
        private List<int?> _reasonsArrears = new List<int?>();   // Список индексов причин неликвидированности задолженностей.

        public ArrearAddPage(Filter filter)
        {
            UploadPage(filter);

            RBCurrentSemester.IsChecked = true;
        }

        public ArrearAddPage(Filter filter, Arrears arrear)
        {
            UploadPage(filter);

            TBHeader.Text = "Изменение задолженности";
            BtnAdd.Content = "Сохранить";

            _arrear = arrear;
            CBGroups.SelectedValue = _arrear.Students.IdGroup;
            CBStudents.SelectedValue = _arrear.Students.Id;

            Arrears.GetYearAndSemester(out int year, out int semester, true);
            if (_arrear.StartYear == year && _arrear.SemesterNumber == semester)
            {
                RBCurrentSemester.IsChecked = true;
            }
            else
            {
                RBLastSemester.IsChecked = true;
            }

            foreach (ArrearsLessons item in Database.Entities.ArrearsLessons.Where(x => x.IdArrear == _arrear.Id))
            {
                _lessonsSelected.Add(item.Lessons);
                _isPrimaryArrears.Add(item.IdType == 1);
                _isLiquidated.Add(item.IsLiquidated);
                _reasonsArrears.Add(item.IdReason);
            }

            UpdateView(_lessonsSelected, _lessons);
        }

        /// <summary>
        /// Настраивает элементы управления страницы.
        /// </summary>
        /// <param name="filter">Настройки фильтра.</param>
        private void UploadPage(Filter filter)
        {
            InitializeComponent();
            DataContext = this;

            _filter = filter;

            CBGroups.ItemsSource = Database.Entities.Groups.ToList();
            CBGroups.SelectedValuePath = "Id";
            CBGroups.DisplayMemberPath = "Group";

            CBStudents.ItemsSource = Database.Entities.Students.ToList();
            CBStudents.SelectedValuePath = "Id";
            CBStudents.DisplayMemberPath = "FullName";

            LBLessons.SelectedValuePath = "Id";
            LBLessons.DisplayMemberPath = "FullName";
        }

        /// <summary>
        /// Обновляет визуальное отображение списков.
        /// </summary>
        /// <param name="itemsSelected">выбранные элементы.</param>
        /// <param name="itemsSource">элементы для выбора.</param>
        private void UpdateView(List<Lessons> itemsSelected, List<Lessons> itemsSource)
        {
            List<Lessons> tempItems = new List<Lessons>();
            tempItems.AddRange(itemsSource);
            LBLessons.ItemsSource = tempItems;

            List<Lessons> tempSelectedItems = new List<Lessons>();
            tempSelectedItems.AddRange(itemsSelected);
            LVLessons.ItemsSource = tempSelectedItems;
        }

        private void CBGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _lessons.Clear();
            _lessonsSelected.Clear();

            if (CBGroups.SelectedIndex != -1)
            {
                CBStudents.ItemsSource = Database.Entities.Students.Where(x => x.IdGroup == (int)CBGroups.SelectedValue).ToList();

                foreach (DistributionLessons item in Database.Entities.DistributionLessons.Where(x => x.IdGroup == (int)CBGroups.SelectedValue))
                {
                    _lessons.Add(item.Lessons);
                }

                if (_lessons.Count == 0)
                {
                    MessageBox.Show("Дисциплины, которые преподаются в выбранной группе, не указаны", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CBStudents.IsEnabled = false;
                }
                else if (CBStudents.Items.Count == 0)
                {
                    MessageBox.Show("В выбранной группе нет студентов", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _lessons.Clear();
                    CBStudents.IsEnabled = false;
                }
                else
                {
                    CBStudents.IsEnabled = true;
                }
            }
            else
            {
                CBStudents.IsEnabled = false;
            }

            UpdateView(_lessonsSelected, _lessons);
        }

        private void RBPrimary_Loaded(object sender, RoutedEventArgs e)
        {
            RadioButton button = sender as RadioButton;

            if (_isPrimaryArrears[_lessonsSelected.IndexOf(button.DataContext as Lessons)])
            {
                button.IsChecked = true;
            }
        }

        private void RBComission_Loaded(object sender, RoutedEventArgs e)
        {
            RadioButton button = sender as RadioButton;

            if (!_isPrimaryArrears[_lessonsSelected.IndexOf(button.DataContext as Lessons)])
            {
                button.IsChecked = true;
            }
        }

        private void ChBLiquidated_Loaded(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;

            if (_isLiquidated[_lessonsSelected.IndexOf(box.DataContext as Lessons)])
            {
                box.IsChecked = true;
            }
        }

        private void CBReason_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            int index = _lessonsSelected.IndexOf(box.DataContext as Lessons);

            if (_isLiquidated[index])
            {
                box.Visibility = Visibility.Collapsed;
            }
            else
            {
                List<ReasonsArrears> reasons = new List<ReasonsArrears>()
                {
                    new ReasonsArrears()
                    {
                        Id= 0,
                        Reason = "Причина"
                    }
                };
                reasons.AddRange(Database.Entities.ReasonsArrears.ToList());

                box.ItemsSource = reasons;
                box.SelectedValuePath = "Id";
                box.DisplayMemberPath = "Reason";
                box.SelectedValue = Convert.ToInt32(_reasonsArrears[index]);
            }
        }

        private void RBPrimary_Checked(object sender, RoutedEventArgs e)
        {
            int index = _lessonsSelected.IndexOf((sender as RadioButton).DataContext as Lessons);
            _isPrimaryArrears[index] = true;
        }

        private void RBComission_Checked(object sender, RoutedEventArgs e)
        {
            int index = _lessonsSelected.IndexOf((sender as RadioButton).DataContext as Lessons);
            _isPrimaryArrears[index] = false;
        }

        private void ChBLiquidated_Click(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;
            int index = _lessonsSelected.IndexOf(box.DataContext as Lessons);

            if ((bool)box.IsChecked)
            {
                _isLiquidated[index] = true;
            }
            else
            {
                _isLiquidated[index] = false;
                _reasonsArrears[index] = null;
            }

            UpdateView(_lessonsSelected, _lessons);
        }

        private void CBReason_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            _reasonsArrears[_lessonsSelected.IndexOf(box.DataContext as Lessons)] = (int)box.SelectedValue;
        }

        private void LBLessons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBLessons.SelectedIndex != -1)
            {
                Lessons lesson = LBLessons.SelectedItem as Lessons;

                _lessons.Remove(lesson);
                _lessonsSelected.Add(lesson);
                _isPrimaryArrears.Add(true);
                _isLiquidated.Add(false);
                _reasonsArrears.Add(null);

                UpdateView(_lessonsSelected, _lessons);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Lessons lesson = (sender as Button).DataContext as Lessons;
            int index = _lessonsSelected.IndexOf(lesson);

            _lessons.Add(lesson);
            _lessonsSelected.RemoveAt(index);
            _isPrimaryArrears.RemoveAt(index);
            _isLiquidated.RemoveAt(index);
            _reasonsArrears.RemoveAt(index);

            UpdateView(_lessonsSelected, _lessons);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFillData())
            {
                bool isUpdate;
                Arrears.GetYearAndSemester(out int year, out int semesterNumber, (bool)RBCurrentSemester.IsChecked);
                int semesterSequenceNumber = Convert.ToInt32((CBStudents.SelectedItem as Students).Groups.Group[0].ToString()) * 2;
                semesterSequenceNumber = (bool)RBCurrentSemester.IsChecked ? semesterSequenceNumber : semesterSequenceNumber - 1;

                if (_arrear == null)
                {
                    _arrear = new Arrears()
                    {
                        IdStudent = (int)CBStudents.SelectedValue,
                        StartYear = year,
                        SemesterNumber = semesterNumber,
                        SemesterSequenceNumber = semesterSequenceNumber
                    };

                    Database.Entities.Arrears.Add(_arrear);
                    isUpdate = false;
                }
                else
                {
                    _arrear.IdStudent = (int)CBStudents.SelectedValue;
                    _arrear.StartYear = year;
                    _arrear.SemesterNumber = semesterNumber;
                    _arrear.SemesterSequenceNumber = semesterSequenceNumber;

                    isUpdate = true;
                }

                try
                {
                    Database.Entities.SaveChanges();
                    SaveLessons();

                    if (isUpdate)
                    {
                        Navigation.Frame.Navigate(new ArrearsShowPage(_filter));
                    }
                    else
                    {
                        MessageBox.Show("Задолженность успешно добавлена", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    _arrear = null;
                }
                catch (Exception)
                {
                    if (isUpdate)
                    {
                        MessageBox.Show("При сохранении данных произошла ошибка", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("При добавлении задолженности произошла ошибка", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        /// <summary>
        /// Проверяет корректность заполнения полей.
        /// </summary>
        /// <returns>True - если все данные заполнены корректно, в противном случае - false.</returns>
        private bool CheckFillData()
        {
            Arrears.GetYearAndSemester(out int year, out int semesterNumber, (bool)RBCurrentSemester.IsChecked);

            if (CBStudents.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите студента", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (LVLessons.Items.Count == 0)
            {
                MessageBox.Show("Выберите дисциплины, по которым студент имеет задолженности", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (_arrear == null && Database.Entities.Arrears.FirstOrDefault(x => x.IdStudent == (int)CBStudents.SelectedValue &&
            x.StartYear == year && x.SemesterNumber == semesterNumber) != null)
            {
                MessageBox.Show("Данная задолженность уже есть в базе данных. Для изменения статуса или списка дисциплин отредактируйте её", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (_arrear != null)
            {
                Arrears arrear = Database.Entities.Arrears.FirstOrDefault(x => x.IdStudent == (int)CBStudents.SelectedValue &&
                x.StartYear == year && x.SemesterNumber == semesterNumber);
                if (arrear != null && _arrear.Id != arrear.Id)
                {
                    MessageBox.Show("Задолженность у такого же студента в выбранном семестре уже есть в базе данных", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

            }

            return true;
        }

        /// <summary>
        /// Сохраняет данные о дисциплинах, по которым студент имеет задолженности.
        /// </summary>
        private void SaveLessons()
        {
            Database.Entities.ArrearsLessons.RemoveRange(Database.Entities.ArrearsLessons.Where(x => x.IdArrear == _arrear.Id));

            for (int i = 0; i < _lessonsSelected.Count; i++)
            {
                Database.Entities.ArrearsLessons.Add(new ArrearsLessons()
                {
                    IdArrear = _arrear.Id,
                    IdLesson = _lessonsSelected[i].Id,
                    IdType = _isPrimaryArrears[i] ? 1 : 2,
                    IsLiquidated = _isLiquidated[i],
                    IdReason = _reasonsArrears[i] == 0 ? null : _reasonsArrears[i]
                });
            }

            Database.Entities.SaveChanges();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new ArrearsShowPage(_filter));
        }
    }
}