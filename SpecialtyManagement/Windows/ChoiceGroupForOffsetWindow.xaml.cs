
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SpecialtyManagement.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChoiceGroupForOffsetWindow.xaml
    /// </summary>
    public partial class ChoiceGroupForOffsetWindow : Window
    {
        private List<Students> _students;
        private bool? _dialogResult;

        public ChoiceGroupForOffsetWindow(List<Students> students)
        {
            InitializeComponent();
            _students = students;
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFillData())
            {
                Groups group = Database.Entities.Groups.FirstOrDefault(x => x.Group == TBoxGroup.Text);
                if (group == null)
                {
                    group = new Groups()
                    {
                        Group = TBoxGroup.Text
                    };
                    Database.Entities.Groups.Add(group);

                    try
                    {
                        Database.Entities.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("При добавлении группы произошла ошибка. Возможно, длина наименования группы превышает 10 символов\nТекст ошибки: " + ex.Message, "Группы", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                foreach (Students item in _students)
                {
                    item.IdGroup = group.Id;
                }

                try
                {
                    Database.Entities.SaveChanges();
                    _dialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("При сохранении данных произошла ошибка.\nТект ошибки: " + ex.Message, "Группы", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        /// <summary>
        /// Проверяет корректность заполнения полей.
        /// </summary>
        /// <returns>True - если все данные заполнены корректно, в противном случае - false.</returns>
        private bool CheckFillData()
        {
            if (TBoxGroup.Text.Length == 0)
            {
                MessageBox.Show("Введите название группы", "Группы", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (TBoxGroup.Text[0] != '2')
            {
                MessageBox.Show("С 1-го курса студент может быть перемещён только на 2-й курс", "Группы", MessageBoxButton.OK, MessageBoxImage.Warning);
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