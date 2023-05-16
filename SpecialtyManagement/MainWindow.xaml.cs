using SpecialtyManagement.Classes;
using SpecialtyManagement.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpecialtyManagement
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Button> _buttonsMenu; // Список кнопок навигации меню.

        public MainWindow()
        {
            InitializeComponent();

            _buttonsMenu = SPMenu.Children.OfType<Button>().ToList();
            _buttonsMenu.Add(BtnSettings);
            SelectButton(_buttonsMenu[0]);


            // Сделать проверку подключения внутри метода CreateEntities().
            // Проверить создание БД по скрипту .sql.
            // Вывести сообщение о варианте запуска от имени администратора.
            // Удалить БД, если такая уже есть, после подтверждения пользователя.



            if (!Database.CreateEntities(out string message)) // Сделать проверку подключения внутри метода.
            {
                MessageBox.Show("При подключении к БД возникла ошибка. Обратитесь к администратору\nТекст ошибки: " + message, "Подключение к базе данных");
                Close();
            }

            Navigation.Frame = MainFrame;
            Navigation.Setting = new Setting();
            Navigation.Frame.Navigate(new StudentsShowPage());

            DataContext = Navigation.Setting;
        }

        private void BtnStudents_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            Navigation.Frame.Navigate(new StudentsShowPage());
        }

        private void BtnArrears_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            Navigation.Frame.Navigate(new ArrearsShowPage());
        }

        private void BtnLessons_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            Navigation.Frame.Navigate(new LessonsShowPage());
        }

        private void BtnTeachers_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            Navigation.Frame.Navigate(new TeahersShowPage());
        }

        private void BtnGroups_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            Navigation.Frame.Navigate(new GroupsShowPage());
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            Navigation.Frame.Navigate(new SettingsPage());
        }

        /// <summary>
        /// Выделяет кнопку на фоне остальных.
        /// </summary>
        /// <param name="currentButton">выбранная кнопка.</param>
        private void SelectButton(Button currentButton)
        {
            foreach (Button item in _buttonsMenu)
            {
                item.Background = ApplicationColor.ColorSecondary;
                item.Foreground = Brushes.Black;
                item.FontWeight = FontWeights.Regular;
            }

            currentButton.Background = ApplicationColor.ColorAccent;
            currentButton.Foreground = Brushes.White;
            currentButton.FontWeight = FontWeights.DemiBold;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}