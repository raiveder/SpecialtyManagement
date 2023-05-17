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

        public string Text { get; set; }

        public ChoiceElementsWindow(List<Teachers> teachers, string text, List<Teachers> teachersSource)
        {
            UploadPage(text, teachers, teachersSource);

            _teachers = teachers;
            TBItems.Text = "Члены комиссии";
            TBSelectedItems.Text = "Выбранные члены комиссии";

            _teachers = teachersSource;
            _teachersSelected = teachers;
            _teachersSelectedTemp.AddRange(_teachersSelected);

            LBItems.ItemsSource = _teachers;
            LVItems.ItemsSource = _teachersSelected;
        }

        public ChoiceElementsWindow(List<Students> students, string text, List<Students> studentsSource)
        {
            UploadPage(text, students, studentsSource);

            _students = students;
            TBItems.Text = "Студенты";
            TBSelectedItems.Text = "Выбранные студенты";

            _students = studentsSource;
            _studentsSelected = students;
            _studentsSelectedTemp.AddRange(_studentsSelected);

            LBItems.ItemsSource = _students;
            LVItems.ItemsSource = _studentsSelected;
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

            foreach (T item in itemsSelected)
            {
                if (itemsSource.Contains(item))
                {
                    itemsSource.Remove(item);
                }
            }

            LBItems.SelectedValuePath = "Id";
            LBItems.DisplayMemberPath = "FullName";
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

                    UpdateView(_teachersSelectedTemp, _teachers);
                }
                else if (_students != null)
                {
                    Students student = LBItems.SelectedItem as Students;
                    _studentsSelectedTemp.Add(student);
                    _students.Remove(student);

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

                UpdateView(_teachersSelectedTemp, _teachers);
            }
            else if (_students != null)
            {
                Students student = button.DataContext as Students;
                _studentsSelectedTemp.Remove(student);
                _students.Add(student);

                UpdateView(_studentsSelectedTemp, _students);
            }
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
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

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
        }
    }
}