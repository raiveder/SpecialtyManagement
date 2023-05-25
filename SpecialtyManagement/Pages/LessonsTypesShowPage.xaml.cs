using SpecialtyManagement.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpecialtyManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для LessonsTypesShowPage.xaml
    /// </summary>
    public partial class LessonsTypesShowPage : Page
    {
        private Filter _filter;
        private bool _isShowWarnings = false; // Для отсутствия предупреждений о результатах фильтрации при загрузке страницы.

        public LessonsTypesShowPage(Filter filter)
        {
            InitializeComponent();

            _filter = filter;
            SetFilter();
            _isShowWarnings = true;
        }

        private void TBoxFind_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetFilter();
        }

        /// <summary>
        /// Устанавливает фильтр для вывода данных.
        /// </summary>
        public void SetFilter()
        {
            List<TypesLessons> types = Database.Entities.TypesLessons.ToList();

            if (TBoxFind.Text.Length > 0)
            {
                types = types.Where(x => x.Type.ToLower().Contains(TBoxFind.Text.ToLower())).ToList();
            }
            types.Sort((x, y) => x.Type.ToLower().CompareTo(y.Type.ToLower()));

            int number = 1;
            foreach (TypesLessons item in types)
            {
                item.SequenceNumber = number++;
            }

            DGTypesLessons.ItemsSource = types;

            if (_isShowWarnings && types.Count == 0)
            {
                MessageBox.Show("Подходящих фильтру типов дисциплин не найдено", "Типы дисциплин", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MIChange_Click(object sender, RoutedEventArgs e)
        {
            LessonTypeAddWindow window = new LessonTypeAddWindow(DGTypesLessons.SelectedItem as TypesLessons, this);
            window.ShowDialog();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            LessonTypeAddWindow window = new LessonTypeAddWindow(this);
            window.ShowDialog();
        }

        private void MIDelete_Click(object sender, RoutedEventArgs e)
        {
            foreach (TypesLessons item in DGTypesLessons.SelectedItems)
            {
                Database.Entities.TypesLessons.Remove(item);
            }

            try
            {
                Database.Entities.SaveChanges();
                SetFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show
                (
                    "При удалении " + (DGTypesLessons.SelectedItems.Count == 1 ? "типов дисциплин" : "типа дисциплин") + " возникла ошибка\nТекст ошибки: " + ex.Message,
                    "Типы дисциплин",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void DGTypesLessons_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DGTypesLessons.SelectedItems.Count > 0)
            {
                CMTypesLessons.IsOpen = true;
            }
        }

        private void DGTypesLessons_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void CMTypesLessons_Closed(object sender, RoutedEventArgs e)
        {
            DGTypesLessons.SelectedItems.Clear();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Frame.Navigate(new LessonsShowPage(_filter));
        }
    }
}
