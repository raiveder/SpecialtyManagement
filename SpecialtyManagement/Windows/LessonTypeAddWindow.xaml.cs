using System;
using System.Linq;
using System.Windows;

namespace SpecialtyManagement.Windows
{
    /// <summary>
    /// Логика взаимодействия для LessonTypeAddWindow.xaml
    /// </summary>
    public partial class LessonTypeAddWindow : Window
    {
        private TypesLessons _typeLesson;
        private bool? _dialogResult;

        public LessonTypeAddWindow()
        {
            InitializeComponent();
        }

        public LessonTypeAddWindow(TypesLessons type)
        {
            InitializeComponent();

            _typeLesson = type;

            TBHeader.Text = "Изменение типа дисциплины";
            TBoxType.Text = _typeLesson.Type;
            BtnAdd.Content = "Сохранить";
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFillData())
            {
                bool isUpdate;
                if (_typeLesson == null)
                {
                    _typeLesson = new TypesLessons()
                    {
                        Type = TBoxType.Text
                    };
                    Database.Entities.TypesLessons.Add(_typeLesson);
                    isUpdate = false;
                }
                else
                {
                    _typeLesson.Type = TBoxType.Text;
                    isUpdate = true;
                }

                try
                {
                    Database.Entities.SaveChanges();
                    _dialogResult = true;

                    if (isUpdate)
                    {
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Тип дисциплины успешно добавлен", "Типы дисциплин", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    _typeLesson = null;
                }
                catch (Exception)
                {
                    if (isUpdate)
                    {
                        MessageBox.Show("При сохранении данных произошла ошибка", "Типы дисциплин", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("При добавлении типа дисциплины произошла ошибка", "Типы дисциплин", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            if (TBoxType.Text.Length == 0)
            {
                MessageBox.Show("Введите тип дисциплины", "Типы дисциплин", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (_typeLesson == null && Database.Entities.TypesLessons.FirstOrDefault(x => x.Type == TBoxType.Text) != null)
            {
                MessageBox.Show("Данный тип дисциплины уже есть в базе данных", "Типы дисциплин", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (_typeLesson != null && Database.Entities.TypesLessons.FirstOrDefault(x => x.Id != _typeLesson.Id && x.Type == TBoxType.Text) != null)
            {
                MessageBox.Show("Данный тип дисциплины уже есть в базе данных", "Типы дисциплин", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult = _dialogResult;
        }
    }
}