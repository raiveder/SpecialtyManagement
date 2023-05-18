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

        public string Text { get; private set; }

        public ChoiceElementWindow(Groups group, string text)
        {
            InitializeComponent();
            DataContext = this;

            Text = text;
            _group = group;
            TBName.Text = "Группа";

            List<Groups> groups = Database.Entities.Groups.ToList();
            groups.Sort((x, y) => x.Group.CompareTo(y.Group));
            CBItems.ItemsSource = groups;
            CBItems.SelectedValuePath = "Id";
            CBItems.DisplayMemberPath = "Group";
        }

        public ChoiceElementWindow(Teachers teacher, string text)
        {
            InitializeComponent();
            DataContext = this;

            Text = text;
            _teacher = teacher;
            TBName.Text = "Преподаватель";

            CBItems.ItemsSource = Database.Entities.Teachers.ToList();
            CBItems.SelectedValuePath = "Id";
            CBItems.DisplayMemberPath = "FullName";
        }

        public ChoiceElementWindow(Lessons lesson, string text, List<Lessons> lessonsSource)
        {
            InitializeComponent();
            DataContext = this;

            Text = text;
            _lesson = lesson;
            TBName.Text = "Дисциплина";

            CBItems.ItemsSource = lessonsSource;
            CBItems.SelectedValuePath = "Id";
            CBItems.DisplayMemberPath = "FullName";
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

                DialogResult = true;
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
    }
}