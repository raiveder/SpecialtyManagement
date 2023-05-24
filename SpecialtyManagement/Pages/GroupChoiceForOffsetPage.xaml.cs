using SpecialtyManagement.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для GroupChoiceForOffsetPage.xaml
    /// </summary>
    public partial class GroupChoiceForOffsetPage : Page
    {
        private List<bool> _isAdded = new List<bool>(); // True - студент добавлен в выделение, в противном случае - false.
        private List<Students> _students; // Список студентов 1-го курса.

        public GroupChoiceForOffsetPage()
        {
            InitializeComponent();

            _students = Database.Entities.Students.Where(x => x.Groups.Group.Substring(0, 1) == "1").ToList();
            _students.Sort((x, y) => x.Groups.Group.ToLower().CompareTo(y.Groups.Group.ToLower()) == 0
            ? x.FullName.ToLower().CompareTo(y.FullName.ToLower())
            : x.Groups.Group.ToLower().CompareTo(y.Groups.Group.ToLower()));

            for (int i = 0; i < _students.Count; i++)
            {
                _isAdded.Add(false);
            }

            LVStudents.ItemsSource = _students;
        }

        private void ChBSelected_Loaded(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;
            box.IsChecked = _isAdded[_students.IndexOf(box.DataContext as Students)];
        }

        private void ChBSelected_Click(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;
            _isAdded[_students.IndexOf(box.DataContext as Students)] = (bool)box.IsChecked;

            List<Students> tempStudents = new List<Students>();
            tempStudents.AddRange(_students);
            LVStudents.ItemsSource = tempStudents;

            BtnChoiceGroup.IsEnabled = _isAdded.Contains(true);
        }

        private void BtnChoiceGroup_Click(object sender, RoutedEventArgs e)
        {
            List<Students> students = new List<Students>();
            for (int i = 0; i < _students.Count; i++)
            {
                if (_isAdded[i])
                {
                    students.Add(_students[i]);
                }
            }

            if (students.Count > 0)
            {
                ChoiceGroupForOffsetWindow window = new ChoiceGroupForOffsetWindow(students);
                window.ShowDialog();

                if ((bool)window.DialogResult)
                {
                    foreach (Students item in students)
                    {
                        int index = _students.IndexOf(item);
                        _students.RemoveAt(index);
                        _isAdded.RemoveAt(index);

                        if (_students.Count == 0)
                        {
                            // Удаление лишних групп 2-го курса.
                            Database.Entities.Groups.RemoveRange(Database.Entities.Groups.Where(x => x.Group.Substring(0, 1) == "2" && x.Students.Count == 0).ToList());
                            Database.Entities.SaveChanges();
                            Navigation.Frame.Navigate(new GroupsShowPage());
                            return;
                        }
                    }

                    List<Students> tempStudents = new List<Students>();
                    tempStudents.AddRange(_students);
                    LVStudents.ItemsSource = tempStudents;
                }
            }
            else
            {
                MessageBox.Show("Студенты не выбраны", "Смещение групп", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}