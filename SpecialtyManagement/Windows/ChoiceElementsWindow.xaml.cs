using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SpecialtyManagement.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChoiceElementsWindow.xaml
    /// </summary>
    public partial class ChoiceElementsWindow : Window
    {
        private List<Teachers> _teachersSelectedTemp = new List<Teachers>();
        private List<Teachers> _teachersSelected;
        private List<Teachers> _teachers;
        private List<Students> _studentsSelectedTemp = new List<Students>();
        private List<Students> _studentsSelected;
        private List<Students> _students;
        private string _headerSelectedItems;
        private bool? _dialogResult = false;

        public string Text { get; private set; }

        public ChoiceElementsWindow(List<Teachers> teachers, string text, List<Teachers> teachersSource)
        {
            _headerSelectedItems = "Выбранные члены комиссии";

            UploadPage(text, teachers, teachersSource);

            TBItems.Text = "Члены комиссии";

            _teachers = teachersSource;
            _teachersSelected = teachers;
            _teachersSelectedTemp.AddRange(_teachersSelected);

            _teachers.Sort((x, y) => x.FullName.ToLower().CompareTo(y.FullName.ToLower()));
            _teachersSelected.Sort((x, y) => x.FullName.ToLower().CompareTo(y.FullName.ToLower()));

            UpdateView(_teachersSelected, _teachers);
        }

        public ChoiceElementsWindow(List<Students> students, string text, List<Students> studentsSource)
        {
            _headerSelectedItems = "Выбранные студенты";

            UploadPage(text, students, studentsSource);

            TBItems.Text = "Студенты";

            _students = studentsSource;
            _studentsSelected = students;
            _studentsSelectedTemp.AddRange(_studentsSelected);

            _students.Sort((x, y) => x.FullName.ToLower().CompareTo(y.FullName.ToLower()));
            _studentsSelected.Sort((x, y) => x.FullName.ToLower().CompareTo(y.FullName.ToLower()));

            UpdateView(_studentsSelected, _students);
        }

        /// <summary>
        /// Настраивает элементы управления страницы.
        /// </summary>
        /// <param name="text">текст заголовка окна.</param>
        /// <param name="itemsSelected">выбранные элементы.</param>
        /// <param name="itemsSource">элементы для выбора.</param>
        private void UploadPage<T>(string text, List<T> itemsSelected, List<T> itemsSource)
        {
            InitializeComponent();
            DataContext = this;
            Text = text;
            Navigation.SPDimming.Visibility = Visibility.Visible;

            if (itemsSelected.Count == 0)
            {
                TBSelectedItems.Text = _headerSelectedItems;
            }
            else
            {
                TBSelectedItems.Text = _headerSelectedItems + " (" + itemsSelected.Count + ")";

                foreach (T item in itemsSelected)
                {
                    if (itemsSource.Contains(item))
                    {
                        itemsSource.Remove(item);
                    }
                }
            }

            LBItems.SelectedValuePath = "Id";
            LBItems.DisplayMemberPath = "FullName";
        }

        /// <summary>
        /// Обновляет визуальное отображение списков.
        /// </summary>
        /// <param name="itemsSelected">выбранные элементы.</param>
        /// <param name="itemsSource">элементы для выбора.</param>
        private void UpdateView<T>(List<T> itemsSelected, List<T> itemsSource)
        {
            List<T> tempItems = new List<T>();
            tempItems.AddRange(itemsSource);
            LBItems.ItemsSource = tempItems;

            List<T> tempSelectedItems = new List<T>();
            tempSelectedItems.AddRange(itemsSelected);
            LVItems.ItemsSource = tempSelectedItems;

            if (tempSelectedItems.Count == 0)
            {
                TBSelectedItems.Text = _headerSelectedItems;
            }
            else
            {
                TBSelectedItems.Text = _headerSelectedItems + " (" + tempSelectedItems.Count + ")";
            }
        }

        private void LBLessons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LBItems.SelectedIndex != -1)
            {
                if (_teachers != null)
                {
                    Teachers teacher = LBItems.SelectedItem as Teachers;
                    _teachersSelectedTemp.Add(teacher);
                    _teachers.Remove(teacher);

                    _teachers.Sort((x, y) => x.FullName.ToLower().CompareTo(y.FullName.ToLower()));
                    _teachersSelectedTemp.Sort((x, y) => x.FullName.ToLower().CompareTo(y.FullName.ToLower()));
                    UpdateView(_teachersSelectedTemp, _teachers);
                }
                else if (_students != null)
                {
                    Students student = LBItems.SelectedItem as Students;
                    _studentsSelectedTemp.Add(student);
                    _students.Remove(student);

                    _students.Sort((x, y) => x.Groups.Group.ToLower().CompareTo(y.Groups.Group.ToLower()) == 0
                    ? x.FullName.ToLower().CompareTo(y.FullName.ToLower())
                    : x.Groups.Group.ToLower().CompareTo(y.Groups.Group.ToLower()));
                    _studentsSelectedTemp.Sort((x, y) => x.Groups.Group.ToLower().CompareTo(y.Groups.Group.ToLower()) == 0
                    ? x.FullName.ToLower().CompareTo(y.FullName.ToLower())
                    : x.Groups.Group.ToLower().CompareTo(y.Groups.Group.ToLower()));
                    UpdateView(_studentsSelectedTemp, _students);
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (_teachers != null)
            {
                Teachers teacher = button.DataContext as Teachers;
                _teachersSelectedTemp.Remove(teacher);
                _teachers.Add(teacher);

                _teachers.Sort((x, y) => x.FullName.ToLower().CompareTo(y.FullName.ToLower()));
                _teachersSelectedTemp.Sort((x, y) => x.FullName.ToLower().CompareTo(y.FullName.ToLower()));
                UpdateView(_teachersSelectedTemp, _teachers);
            }
            else if (_students != null)
            {
                Students student = button.DataContext as Students;
                _studentsSelectedTemp.Remove(student);
                _students.Add(student);

                _students.Sort((x, y) => x.Groups.Group.ToLower().CompareTo(y.Groups.Group.ToLower()) == 0
                    ? x.FullName.ToLower().CompareTo(y.FullName.ToLower())
                    : x.Groups.Group.ToLower().CompareTo(y.Groups.Group.ToLower()));
                _studentsSelectedTemp.Sort((x, y) => x.Groups.Group.ToLower().CompareTo(y.Groups.Group.ToLower()) == 0
                ? x.FullName.ToLower().CompareTo(y.FullName.ToLower())
                : x.Groups.Group.ToLower().CompareTo(y.Groups.Group.ToLower()));
                UpdateView(_studentsSelectedTemp, _students);
            }
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (_teachers != null)
            {
                _teachersSelected.Clear();
                _teachersSelected.AddRange(_teachersSelectedTemp);
            }
            else if (_students != null)
            {
                _studentsSelected.Clear();
                _studentsSelected.AddRange(_studentsSelectedTemp);
            }

            _dialogResult = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult = _dialogResult;
            Navigation.SPDimming.Visibility = Visibility.Collapsed;
        }
    }
}