using SpecialtyManagement.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SpecialtyManagement
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Button> _buttonsMenu; // Список кнопок меню.

        public MainWindow()
        {
            InitializeComponent();

            _buttonsMenu = SPMenu.Children.OfType<Button>().ToList();

            Database.Entities = new SpecialtyManagementEntities();
            Navigation.Frame = MainFrame;
            Navigation.Frame.Navigate(new StudentsShowPage());

            SelectCurrentButton(_buttonsMenu[0]);
        }

        private void BtnStudents_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrentButton((Button)sender);

            Navigation.Frame.Navigate(new StudentsShowPage());
        }

        private void BtnArrears_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrentButton((Button)sender);
        }

        private void BtnPerformance_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrentButton((Button)sender);
        }

        private void BtnLessons_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrentButton((Button)sender);
        }

        private void BtnTeachers_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrentButton((Button)sender);
        }

        private void BtnGroups_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrentButton((Button)sender);
        }

        /// <summary>
        /// Выделяет текущую кнопку на фоне остальных.
        /// </summary>
        /// <param name="currentButton">выбранная кнопка.</param>
        private void SelectCurrentButton(Button currentButton)
        {
            foreach (Button item in _buttonsMenu)
            {
                item.Background = ApplicationColor.ColorSecondary;
                item.FontWeight = FontWeights.Regular;
            }

            currentButton.Background = ApplicationColor.ColorAccent;
            currentButton.FontWeight = FontWeights.SemiBold;
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}