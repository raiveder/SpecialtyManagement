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
        private List<Lessons> _lessons = new List<Lessons>(); // Список дисциплин, по которым у студента есть задолженности.
        private List<bool> _isPrimaryArrears = new List<bool>(); // Список типов задолженностей (true - первичная, false - комиссионная).
        private List<bool> _isLiquidated = new List<bool>(); // Список статусов задолженностей (true - ликвидирована, false - нет).
        private List<int?> _reasonsArrears = new List<int?>(); // Список индексов причин неликвидированности задолженностей.

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
                _lessons.Add(item.Lessons);
                _isPrimaryArrears.Add(item.IdType == 1);
                _isLiquidated.Add(item.IsLiquidated);
                _reasonsArrears.Add(item.IdReason);
            }
            LVLessons.ItemsSource = _lessons;
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

            CBStudents.ItemsSource = Database.Entities.Students.ToList();
            CBStudents.SelectedValuePath = "Id";
            CBStudents.DisplayMemberPath = "FullName";

            LBLessons.ItemsSource = Database.Entities.Lessons.ToList();
            LBLessons.SelectedValuePath = "Id";
            LBLessons.DisplayMemberPath = "FullName";
        }

        private void RBCurrentSemester_Checked(object sender, RoutedEventArgs e)
        {
            RBLastSemester.IsChecked = false;
        }

        private void RBLastSemester_Checked(object sender, RoutedEventArgs e)
        {
            RBCurrentSemester.IsChecked = false;
        }

        private void LBLessons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Lessons lesson = LBLessons.SelectedItem as Lessons;

            if (!_lessons.Contains(lesson))
            {
                _lessons.Add(lesson);
                _isPrimaryArrears.Add(true);
                _isLiquidated.Add(false);
                _reasonsArrears.Add(null);

                List<Lessons> tempLessons = new List<Lessons>();
                tempLessons.AddRange(_lessons);
                LVLessons.ItemsSource = tempLessons;
            }
        }

        private void RBPrimary_Loaded(object sender, RoutedEventArgs e)
        {
            RadioButton button = sender as RadioButton;
            int id = Convert.ToInt32(button.Uid);
            int index = _lessons.IndexOf(Database.Entities.Lessons.FirstOrDefault(x => x.Id == id));

            if (_isPrimaryArrears[index])
            {
                button.IsChecked = true;
            }
        }

        private void RBComission_Loaded(object sender, RoutedEventArgs e)
        {
            RadioButton button = sender as RadioButton;
            int id = Convert.ToInt32(button.Uid);
            int index = _lessons.IndexOf(Database.Entities.Lessons.FirstOrDefault(x => x.Id == id));

            if (!_isPrimaryArrears[index])
            {
                button.IsChecked = true;
            }
        }

        private void ChBLiquidated_Loaded(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;
            int id = Convert.ToInt32(box.Uid);
            int index = _lessons.IndexOf(Database.Entities.Lessons.FirstOrDefault(x => x.Id == id));

            if (_isLiquidated[index])
            {
                box.IsChecked = true;
            }
        }

        private void CBReason_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            int id = Convert.ToInt32(box.Uid);
            int index = _lessons.IndexOf(Database.Entities.Lessons.FirstOrDefault(x => x.Id == id));

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
            RadioButton button = sender as RadioButton;
            int id = Convert.ToInt32(button.Uid);
            int index = _lessons.IndexOf(Database.Entities.Lessons.FirstOrDefault(x => x.Id == id));
            _isPrimaryArrears[index] = true;
        }

        private void RBComission_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton button = sender as RadioButton;
            int id = Convert.ToInt32(button.Uid);
            int index = _lessons.IndexOf(Database.Entities.Lessons.FirstOrDefault(x => x.Id == id));
            _isPrimaryArrears[index] = false;
        }

        private void ChBLiquidated_Click(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;
            int id = Convert.ToInt32(box.Uid);
            int index = _lessons.IndexOf(Database.Entities.Lessons.FirstOrDefault(x => x.Id == id));

            if ((bool)box.IsChecked)
            {
                _isLiquidated[index] = true;
            }
            else
            {
                _isLiquidated[index] = false;
                _reasonsArrears[index] = null;
            }

            List<Lessons> tempLessons = new List<Lessons>();
            tempLessons.AddRange(_lessons);
            LVLessons.ItemsSource = tempLessons;
        }

        private void CBReason_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            int id = Convert.ToInt32(box.Uid);
            int index = _lessons.IndexOf(Database.Entities.Lessons.FirstOrDefault(x => x.Id == id));

            _reasonsArrears[index] = (int)box.SelectedValue;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32((sender as Button).Uid);
            Lessons lesson = Database.Entities.Lessons.FirstOrDefault(x => x.Id == id);
            int index = _lessons.IndexOf(lesson);

            _lessons.Remove(lesson);
            _isPrimaryArrears.RemoveAt(index);
            _isLiquidated.RemoveAt(index);
            _reasonsArrears.RemoveAt(index);

            List<Lessons> tempLessons = new List<Lessons>();
            tempLessons.AddRange(_lessons);
            LVLessons.ItemsSource = tempLessons;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFillData())
            {
                bool isUpdate;
                Arrears.GetYearAndSemester(out int year, out int semesterNumber, (bool)RBCurrentSemester.IsChecked);

                if (_arrear == null)
                {
                    _arrear = new Arrears()
                    {
                        IdStudent = (int)CBStudents.SelectedValue,
                        StartYear = year,
                        SemesterNumber = semesterNumber
                    };

                    Database.Entities.Arrears.Add(_arrear);

                    isUpdate = false;
                }
                else
                {
                    _arrear.IdStudent = (int)CBStudents.SelectedValue;
                    _arrear.StartYear = year;
                    _arrear.SemesterNumber = semesterNumber;

                    isUpdate = true;
                }

                try
                {
                    Database.Entities.SaveChanges();
                    SaveLessons();

                    if (isUpdate)
                    {
                        MessageBox.Show("Данные успешно обновлены", "Задолженности", MessageBoxButton.OK, MessageBoxImage.Information);
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

            return true;
        }

        /// <summary>
        /// Сохраняет данные о дисциплинах, по которым студент имеет задолженности.
        /// </summary>
        /// <returns>True - если сохранение прошло успешно, в противном случае - false.</returns>
        private void SaveLessons()
        {
            Database.Entities.ArrearsLessons.RemoveRange(Database.Entities.ArrearsLessons.Where(x => x.IdArrear == _arrear.Id));

            for (int i = 0; i < _lessons.Count; i++)
            {
                Database.Entities.ArrearsLessons.Add(new ArrearsLessons()
                {
                    IdArrear = _arrear.Id,
                    IdLesson = _lessons[i].Id,
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