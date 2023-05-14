using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace SpecialtyManagement.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChoiceElementWindow.xaml
    /// </summary>
    public partial class ChoiceElementWindow : Window
    {
        private Groups _group;
        private Lessons _lesson;

        public string Text { get; set; }

        public ChoiceElementWindow(Groups group, string text)
        {
            InitializeComponent();
            DataContext = this;

            Text = text;
            _group = group;
            TBName.Text = "Группа";

            CBItems.ItemsSource = Database.Entities.Groups.ToList();
            CBItems.SelectedValuePath = "Id";
            CBItems.DisplayMemberPath = "Group";
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
            }
        }
    }
}