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
        private Word.Application _applicationWord;
        private bool _canClosingWord = true;
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
            _applicationWord = new Word.Application
            {
                Visible = false
            };

            await Task.Run(() => ArrearsPrimaryCreateDocumentPage.CreateDocument(_applicationWord));

            try
            {
                _applicationWord.Visible = true;
            }
            catch
            {
                Thread.Sleep(50); // 50, так как раз в 50 мс. метод прервывания процесса
            }                     // формирования документов опрашивает состояние _applicationWord.

            _canClosingWord = false;
            await Dispatcher.BeginInvoke(new ThreadStart(() => Close()));
        }

        /// <summary>
        /// Формирует документы для комиссионных задолженностей.
        /// </summary>
        private async void CreateDocumentsComissionArrears()
        {
            _applicationWord = new Word.Application
            {
                Visible = false
            };

            Task shedule = Task.Run(() => ArrearsComissionCreateDocumentPage.CreateDocumentShedule(_applicationWord));
            Task memo = Task.Run(() => ArrearsComissionCreateDocumentPage.CreateDocumentMemo(_applicationWord, _sender, _recipient));

            await shedule;
            await memo;

            try
            {
                _applicationWord.Visible = true;
            }
            catch
            {
                Thread.Sleep(50); // 50, так как раз в 50 мс. метод прервывания процесса
            }                     // формирования документов опрашивает состояние _applicationWord.

            _canClosingWord = false;
            await Dispatcher.BeginInvoke(new ThreadStart(() => Close()));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_canClosingWord)
            {
                for (int i = 0; i < 100; i++)
                {
                    if (_applicationWord != null)
                    {
                        object saveOption = Word.WdSaveOptions.wdDoNotSaveChanges;
                        object originalFormat = Word.WdOriginalFormat.wdOriginalDocumentFormat;
                        object routeDocument = false;

                        foreach (Word.Document item in _applicationWord.Documents)
                        {
                            item.Close(ref saveOption, ref originalFormat, ref routeDocument);
                        }

                        _applicationWord.DisplayAlerts = Word.WdAlertLevel.wdAlertsNone;
                        _applicationWord.Quit();
                        break;
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
            }

            Navigation.SPDimming.Visibility = Visibility.Collapsed;
        }
    }
}