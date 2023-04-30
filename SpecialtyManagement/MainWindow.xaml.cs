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

            Database.Entities = new SpecialtyManagementEntities(); // Сделать проверку подключения.

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

        private void BtnPerformance_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
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

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SelectButton((Button)sender);
            Navigation.Frame.Navigate(new SettingsPage());
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}