using SpecialtyManagement.Pages;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Word = Microsoft.Office.Interop.Word;

namespace SpecialtyManagement.Windows
{
    /// <summary>
    /// Логика взаимодействия для CreateDocumentWindow.xaml
    /// </summary>
    public partial class CreateDocumentWindow : Window
    {
        private bool _canClosing; // True, если окно может быть закрыто, в противном случае - false.
        private string _sender;
        private string _recipient;

        public CreateDocumentWindow()
        {
            UploadPage();

            TBHeader.Text = "Формирование документа";
            new Thread(CreateDocumentPrimaryArrears).Start();
        }

        public CreateDocumentWindow(string sender, string recipient)
        {
            UploadPage();

            TBHeader.Text = "Формирование документов";
            _sender = sender;
            _recipient = recipient;
            new Thread(CreateDocumentsComissionArrears).Start();
        }

        /// <summary>
        /// Настраивает элементы управления окна.
        /// </summary>
        private void UploadPage()
        {
            InitializeComponent();
            Navigation.SPDimming.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Формирует документ о первичных задолженностях.
        /// </summary>
        private async void CreateDocumentPrimaryArrears()
        {
            Word.Application app = new Word.Application
            {
                Visible = false
            };

            await Task.Run(() => ArrearsPrimaryCreateDocumentPage.CreateDocument(app));
            app.Visible = true;

            _canClosing = true;
            await Dispatcher.BeginInvoke(new ThreadStart(() => Close()));
        }

        /// <summary>
        /// Формирует документы для комиссионных задолженностей.
        /// </summary>
        private async void CreateDocumentsComissionArrears()
        {
            Word.Application app = new Word.Application
            {
                Visible = false
            };

            Task shedule = Task.Run(() => ArrearsComissionCreateDocumentPage.CreateDocumentShedule(app));
            Task memo = Task.Run(() => ArrearsComissionCreateDocumentPage.CreateDocumentMemo(app, _sender, _recipient));

            await shedule;
            await memo;
            app.Visible = true;

            _canClosing = true;
            await Dispatcher.BeginInvoke(new ThreadStart(() => Close()));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_canClosing)
            {
                Navigation.SPDimming.Visibility = Visibility.Collapsed;
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}