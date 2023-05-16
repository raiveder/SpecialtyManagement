using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Windows;

namespace SpecialtyManagement
{
    [RunInstaller(true)]
    public partial class CustomInstaller : Installer
    {
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
            Server server = new Server();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                try
                {
                    server = new Server(new ServerConnection(conn));
                    server.ConnectionContext.ExecuteNonQuery(GetScriptSql("ScriptDatabase.sql", stateSaver));
                }
                catch (Exception ex)
                {
                    string text = ex.InnerException.Message.ToLower();

                    if (text.Contains("specialtymanagement"))
                    {
                        MessageBoxResult result = MessageBox.Show("База данных для приложения с таким именем уже существует. Вы действительно хотите пересоздать её?\n\"Да\" - пересоздать БД\n\"Нет\" - работать с сохранённой\n\"Отмена\" - отменить установку", "Установка приложения", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                server.ConnectionContext.ExecuteNonQuery("DROP DATABASE [SpecialtyManagement]");
                                ExecuteScriptSql(stateSaver);
                                break;
                            case MessageBoxResult.No:
                                break;
                            case MessageBoxResult.Cancel:
                                Rollback(stateSaver);
                                throw new InstallException("Установка была прервана пользователем");
                        }
                    }
                    else if (text.Contains("create database"))
                    {
                        MessageBox.Show("Запустите установщик от имени администратора", "Установка приложения", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Rollback(stateSaver);
                        throw new InstallException("Установка была прервана");
                    }
                    else if (text.Contains("используется"))
                    {
                        MessageBox.Show("База данных в данный момент используется. Перезагрузите компьютер и повторите процесс установки", "Установка приложения", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Rollback(stateSaver);
                        throw new InstallException("Установка была прервана");
                    }
                }
            }
        }

        private string GetScriptSql(string fileName, IDictionary stateSaver)
        {
            try
            {
                Assembly Asm = Assembly.GetExecutingAssembly();
                Stream strm = Asm.GetManifestResourceStream(Asm.GetName().Name + "." + fileName);
                StreamReader reader = new StreamReader(strm);

                return reader.ReadToEnd();
            }
            catch
            {
                Rollback(stateSaver);
                throw new InstallException("Возникла проблема с чтением файла скрипта базы данных. Обратитесь к администратору");
            }
        }
    }
}