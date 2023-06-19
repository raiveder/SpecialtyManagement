using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private Specialty _specialty = Database.Entities.Specialty.FirstOrDefault();
        List<Button> _buttonsMenu;

        public SettingsPage(List<Button> buttons)
        {
            InitializeComponent();
            _buttonsMenu = buttons;

            if (_specialty != null)
            {
                TBoxDepartament.Text = _specialty.Departament;
                TBoxCode.Text = _specialty.Code;
                TBoxName.Text = _specialty.Name;
                TBoxHead.Text = _specialty.Head;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFillData())
            {
                if (_specialty == null)
                {
                    _specialty = new Specialty();
                    Database.Entities.Specialty.Add(_specialty);
                }

                _specialty.Departament = TBoxDepartament.Text;
                _specialty.Code = TBoxCode.Text;
                _specialty.Name = TBoxName.Text;
                _specialty.Head = TBoxHead.Text;

                try
                {
                    Database.Entities.SaveChanges();
                    Navigation.Setting.UpdateSettings();
                    Navigation.Frame.Navigate(new StudentsShowPage());
                    MainWindow.SelectButton(_buttonsMenu[0]);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("При сохранении данных произошла ошибка\nТекст ошибки: " + ex.Message, "Настройки", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        /// <summary>
        /// Проверяет корректность заполнения полей.
        /// </summary>
        /// <returns>True - если все данные заполнены корректно, в противном случае - false.</returns>
        private bool CheckFillData()
        {
            if (TBoxDepartament.Text.Length == 0)
            {
                MessageBox.Show("Введите наименование отделения", "Настройки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (TBoxCode.Text.Length == 0)
            {
                MessageBox.Show("Введите код специальности", "Настройки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (TBoxName.Text.Length == 0)
            {
                MessageBox.Show("Введите наименование специальности", "Настройки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (!Regex.IsMatch(TBoxHead.Text, @"^[А-Я][а-я]+ [А-Я][а-я]+ [А-Я][а-я]+$"))
            {
                MessageBox.Show("Введите ФИО зав. специальности корректно", "Настройки", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}