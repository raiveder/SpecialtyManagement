using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Windows;

namespace SpecialtyManagement
{
    [RunInstaller(true)]
    public partial class CustomInstaller : System.Configuration.Install.Installer
    {
        private string _logFilePath = "C:\\SetupLog.txt";

        public CustomInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            ExecuteScriptSql(stateSaver);
        }

        public override void Rollback(IDictionary stateSaver)
        {
            base.Rollback(stateSaver);
        }

        private void ExecuteScriptSql(IDictionary stateSaver)
        {
            string connStr = "Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                try
                {
                    Server server = new Server(new ServerConnection(conn));
                    server.ConnectionContext.ExecuteNonQuery(GetScriptSql("ScriptDatabase.sql"));
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.Message.ToLower().Contains("create database"))
                    {
                        MessageBoxResult result = MessageBox.Show("База данных для приложения с таким именем уже существует. Вы действительно хотите пересоздать её?\n\"Да\" - пересоздать БД\n\"Нет\" - работать с сохранённой\n\"Отмена\" - отменить установку", "Test", MessageBoxButton.YesNoCancel);
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                MessageBox.Show("Yes");
                                break;
                            case MessageBoxResult.No:
                                MessageBox.Show("No");
                                break;
                            case MessageBoxResult.Cancel:
                                Rollback(stateSaver);
                                throw new InstallException("Установка была прервана пользователем");
                        }
                    }
                }
            }
        }

        private string GetScriptSql(string fileName)
        {
            try
            {
                Assembly Asm = Assembly.GetExecutingAssembly();
                Stream strm = Asm.GetManifestResourceStream(Asm.GetName().Name + "." + fileName);
                StreamReader reader = new StreamReader(strm);

                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                throw ex;
            }
        }

        private void Log(string str)
        {
            StreamWriter sw;
            try
            {
                sw = File.AppendText(_logFilePath);
                sw.WriteLine(DateTime.Now.ToString() + " " + str);
                sw.Close();
            }
            catch
            { }
        }
    }
}