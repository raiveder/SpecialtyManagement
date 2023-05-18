using SpecialtyManagement.Pages;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
            InitializeComponent();

            TBHeader.Text = "Формирование документа";
            new Thread(CreateDocumentPrimaryArrears).Start();
        }

        public CreateDocumentWindow(string sender, string recipient)
        {
            InitializeComponent();

            TBHeader.Text = "Формирование документов";
            _sender = sender;
            _recipient = recipient;
            new Thread(CreateDocumentsComissionArrears).Start();
        }

        private void CreateDocumentPrimaryArrears()
        {
            ArrearsPrimaryCreateDocumentPage.CreateDocument();
            _canClosing = true;
            Dispatcher.BeginInvoke(new ThreadStart(() => Close()));
        }

        private async void CreateDocumentsComissionArrears()
        {
            Task shedule = Task.Run(() => ArrearsComissionCreateDocumentPage.CreateDocumentShedule());
            Task memo = Task.Run(() => ArrearsComissionCreateDocumentPage.CreateDocumentMemo(_sender, _recipient));

            await shedule;
            await memo;

            _canClosing = true;
            await Dispatcher.BeginInvoke(new ThreadStart(() => Close()));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_canClosing)
            {
                e.Cancel = true;
            }
        }
    }
}