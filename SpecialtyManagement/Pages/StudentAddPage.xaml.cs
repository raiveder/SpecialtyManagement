using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для StudentAddPage.xaml
    /// </summary>
    public partial class StudentAddPage : Page
    {
        private Students _student;
        private Filter _filter;

        public StudentAddPage(Filter filter)
        {
            UploadPage(filter);
        }

        public StudentAddPage(Filter filter, Students student)
        {
            UploadPage(filter);

            TBHeader.Text = "Изменение студента";

            _student = student;

            TBoxSurname.Text = _student.Surname;
            TBoxName.Text = _student.Name;
            TBoxPatronymic.Text = _student.Patronymic;
            DPBirthday.SelectedDate = _student.Birthday;
            TBoxNote.Text = _student.Note;
        }

        private void UploadPage(Filter filter)
        {
            InitializeComponent();

            _filter = filter;

            DPBirthday.DisplayDateEnd = DateTime.Now.AddYears(-14);

            CBGroup.ItemsSource = Database.Entities.Groups.ToList();
            CBGroup.SelectedValuePath = "Id";
            CBGroup.DisplayMemberPath = "Group";
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new StudentsShowPage(_filter));
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFillData())
            {
                if (_student == null)
                {
                    Database.Entities.Students.Add(new Students()
                    {
                        Surname = TBoxSurname.Text,
                        Name = TBoxName.Text,
                        Patronymic = TBoxPatronymic.Text,
                        IdGroup = (int)CBGroup.SelectedValue,
                        Birthday = DPBirthday.SelectedDate.Value,
                        Note = TBoxNote.Text.Length == 0 ? null : TBoxNote.Text
                    });
                }
                else
                {
                    Database.Entities.Students.Add(_student);
                }

                try
                {
                    Database.Entities.SaveChanges();
                    MessageBox.Show("Студент успешно доабвлен", "Студенты", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception)
                {
                    MessageBox.Show("При добавлении студента произошла ошибка", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private bool CheckFillData()
        {
            Regex regexText = new Regex(@"^[А-Я][а-я]*$");

            if (TBoxSurname.Text.Length == 0)
            {
                MessageBox.Show("Введите фамилию студента", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (!regexText.IsMatch(TBoxSurname.Text))
            {
                MessageBox.Show("Введите фамилию студента корректно", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (TBoxName.Text.Length == 0)
            {
                MessageBox.Show("Введите имя студента", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (!regexText.IsMatch(TBoxName.Text))
            {
                MessageBox.Show("Введите имя студента корректно", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (TBoxPatronymic.Text.Length == 0)
            {
                MessageBox.Show("Введите отчество студента", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (!regexText.IsMatch(TBoxPatronymic.Text))
            {
                MessageBox.Show("Введите отчество студента корректно", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (CBGroup.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите группу студента", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (DPBirthday.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату рождения студента", "Студенты", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}