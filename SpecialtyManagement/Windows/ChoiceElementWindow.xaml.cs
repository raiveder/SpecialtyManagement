using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SpecialtyManagement.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChoiceElementWindow.xaml
    /// </summary>
    public partial class ChoiceElementWindow : Window
    {
        private Groups _group;
        private Lessons _lesson;
        private Teachers _teacher;
        private bool? _dialogResult = false;

        public string Text { get; private set; }

        public ChoiceElementWindow(Groups group, string text)
        {
            UploadPage(text);

            _group = group;
            TBName.Text = "Группа";

            List<Groups> groups = Database.Entities.Groups.ToList();
            groups.Sort((x, y) => x.Group.ToLower().CompareTo(y.Group.ToLower()));
            CBItems.ItemsSource = groups;
            CBItems.SelectedValuePath = "Id";
            CBItems.DisplayMemberPath = "Group";
        }

        public ChoiceElementWindow(Teachers teacher, string text)
        {
            UploadPage(text);

            _teacher = teacher;
            TBName.Text = "Преподаватель";

            List<Teachers> teachers = Database.Entities.Teachers.ToList();
            teachers.Sort((x, y) => x.FullName.ToLower().CompareTo(y.FullName.ToLower()));
            CBItems.ItemsSource = teachers;
            CBItems.SelectedValuePath = "Id";
            CBItems.DisplayMemberPath = "FullName";
        }

        public ChoiceElementWindow(Lessons lesson, string text, List<Lessons> lessonsSource)
        {
            UploadPage(text);

            _lesson = lesson;
            TBName.Text = "Дисциплина";

            lessonsSource.Sort((x, y) => x.FullName.ToLower().CompareTo(y.FullName.ToLower()));
            CBItems.ItemsSource = lessonsSource;
            CBItems.SelectedValuePath = "Id";
            CBItems.DisplayMemberPath = "FullName";
        }

        /// <summary>
        /// Настраивает элементы управления окна.
        /// </summary>
        /// <param name="text">текст заголовка окна.</param>
        private void UploadPage(string text)
        {
            InitializeComponent();
            DataContext = this;
            Text = text;
            Navigation.SPDimming.Visibility = Visibility.Visible;
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (CBItems.SelectedIndex != -1)
            {
                if (_group != null)
                {
                    Groups tempGroup = CBItems.SelectedItem as Groups;
                    _group.Id = tempGroup.Id;
                    _group.Group = tempGroup.Group;
                }
                else if (_lesson != null)
                {
                    Lessons lessons = CBItems.SelectedItem as Lessons;
                    _lesson.Id = lessons.Id;
                    _lesson.Name = lessons.Name;
                    _lesson.Code = lessons.Code;
                    _lesson.TypesLessons = lessons.TypesLessons;
                }
                else if (_teacher != null)
                {
                    Teachers teacher = CBItems.SelectedItem as Teachers;
                    _teacher.Id = teacher.Id;
                    _teacher.Surname = teacher.Surname;
                    _teacher.Name = teacher.Name;
                    _teacher.Patronymic = teacher.Patronymic;
                }

                _dialogResult = true;
                Close();
            }
            else
            {
                if (_group != null)
                {
                    MessageBox.Show("Выберите группу", "Выбор группы", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (_lesson != null)
                {
                    MessageBox.Show("Выберите дисциплину", "Выбор дисциплины", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (_lesson != null)
                {
                    MessageBox.Show("Выберите преподавателя", "Выбор преподавателя", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult = _dialogResult;
            Navigation.SPDimming.Visibility = Visibility.Collapsed;
        }
    }
}