using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для TeacherAddPage.xaml
    /// </summary>
    public partial class TeacherAddPage : Page
    {
        private List<Lessons> _lessons = new List<Lessons>();
        private Teachers _teacher;
        private Filter _filter;

        public TeacherAddPage(Filter filter)
        {
            InitializeComponent();
            _filter = filter;

            LBLessons.ItemsSource = Database.Entities.Lessons.ToList();
            LBLessons.SelectedValuePath = "Id";
            LBLessons.DisplayMemberPath = "FullName";
        }

        public TeacherAddPage(Filter filter, Teachers teacher)
        {
            InitializeComponent();
            _filter = filter;

            TBHeader.Text = "Изменение преподавателя";

            _teacher = teacher;

            TBoxSurname.Text = _teacher.Surname;
            TBoxName.Text = _teacher.Name;
            TBoxPatronymic.Text = _teacher.Patronymic;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new TeahersShowPage(_filter));
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LBLessons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Lessons lesson = LBLessons.SelectedItem as Lessons;

            if (!_lessons.Contains(lesson))
            {
                _lessons.Add(Database.Entities.Lessons.FirstOrDefault(x => x.Id == lesson.Id));

                List<Lessons> tempLessons = new List<Lessons>();
                tempLessons.AddRange(_lessons);

                LVLessons.ItemsSource = tempLessons;
            }
        }
    }
}