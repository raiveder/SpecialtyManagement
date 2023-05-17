using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace SpecialtyManagement
{
    [RunInstaller(true)]
    public partial class CustomInstaller : Installer
    {
        private bool _isRenameServerAlready = false;

        public CustomInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            ChangeConnectionString(ExecuteScriptSql("Data Source=.\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True", stateSaver));
        }

        public override void Rollback(IDictionary stateSaver)
        {
            base.Rollback(stateSaver);
        }

        /// <summary>
        /// Выполняет скрипт создания базы данных.
        /// </summary>
        /// <param name="connStr">строка подключения.</param>
        /// <param name="stateSaver">обязательный параметр.</param>
        /// <returns>True, если строка подключения изменялась, в противном случае - false.</returns>
        /// <exception cref="InstallException">Исключение.</exception>
        private bool ExecuteScriptSql(string connStr, IDictionary stateSaver)
        {
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
                    string text = ex.InnerException.Message;

                    if (text.ToLower().Contains("существует"))
                    {
                        MessageBoxResult result = MessageBox.Show("База данных для приложения с таким именем уже существует. Вы действительно хотите пересоздать её?\n\"Да\" - пересоздать БД\n\"Нет\" - работать с сохранённой\n\"Отмена\" - отменить установку", "Установка приложения", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                server.ConnectionContext.ExecuteNonQuery("DROP DATABASE [SpecialtyManagement]");
                                ExecuteScriptSql(connStr, stateSaver);
                                break;
                            case MessageBoxResult.No:
                                break;
                            case MessageBoxResult.Cancel:
                                Rollback(stateSaver);
                                throw new InstallException("Установка была прервана пользователем");
                        }
                    }
                    else if (text.ToLower().Contains("запрещено"))
                    {
                        MessageBox.Show("Запустите установщик от имени администратора", "Установка приложения", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Rollback(stateSaver);
                        throw new InstallException("Установка была прервана");
                    }
                    else if (text.ToLower().Contains("используется"))
                    {
                        MessageBox.Show("База данных в данный момент используется. Перезагрузите компьютер и повторите процесс установки", "Установка приложения", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Rollback(stateSaver);
                        throw new InstallException("Установка была прервана");
                    }
                    else if (_isRenameServerAlready && text.ToLower().Contains("error: 26"))
                    {
                        MessageBox.Show("Возникла ошибка при установке приложения. Задать имя сервера по из одному шаблонов:\n\"имя сервера\\SQLEXPRESS\"\n\"имя сервера\"\nТекст ошибки: " + text, "Установка приложения", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Rollback(stateSaver);
                        throw new InstallException("Установка была прервана");
                    }
                    else if (text.ToLower().Contains("error: 26"))
                    {
                        _isRenameServerAlready = true;
                        ExecuteScriptSql("Data Source=.;Initial Catalog=master;Integrated Security=True", stateSaver);
                        return true;
                    }
                    else if (text.ToLower().Contains("error: 40"))
                    {
                        MessageBox.Show("Не удалось соединиться с MS SQL Server. Убедитесь, что у вас установлен SQL EXPRESS и к нему есть доступ\nТекст ошибки: " + text, "Установка приложения", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Rollback(stateSaver);
                        throw new InstallException("Установка была прервана");
                    }
                    else
                    {
                        MessageBox.Show("Возникла ошибка при установке приложения\nТекст ошибки: " + text, "Установка приложения", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Rollback(stateSaver);
                        throw new InstallException("Установка была прервана");
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Изменяет строку подключения к базе данных.
        /// </summary>
        /// <param name="isReplaced">true - исходная строка изменялась, false - нет.</param>
        private void ChangeConnectionString(bool isReplaced)
        {
            try
            {
                string path = Context.Parameters["assemblypath"];
                path = path.Replace("SpecialtyManagement.exe", "SpecialtyManagement.exe.config");
                string text;

                using (FileStream fstream = File.OpenRead(path))
                {
                    byte[] buffer = new byte[fstream.Length];
                    fstream.Read(buffer, 0, buffer.Length);
                    text = Encoding.Default.GetString(buffer);
                }

                if (isReplaced)
                {
                    text = text.Replace(".\\SQLEXPRESS", Environment.MachineName);
                }
                else
                {
                    text = text.Replace(".\\SQLEXPRESS", Environment.MachineName + "\\SQLEXPRESS");
                }

                using (FileStream fstream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    byte[] buffer = Encoding.Default.GetBytes(text);
                    fstream.Write(buffer, 0, buffer.Length);
                }

                using (FileStream fstream = File.OpenRead(path))
                {
                    byte[] buffer = new byte[fstream.Length];
                    fstream.Read(buffer, 0, buffer.Length);
                    text = Encoding.Default.GetString(buffer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Получает текст скрипта базы данных.
        /// </summary>
        /// <param name="fileName">имя файла скрипта.</param>
        /// <param name="stateSaver">обязательный параметр.</param>
        /// <returns>Текст скрипта базы данных.</returns>
        /// <exception cref="InstallException">Исключение.</exception>
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
                throw new InstallException("Возникла проблема с чтением файла скрипта базы данных. Обратитесь к администратору, чтобы он запустил скрипт вручную. Скрипт находится в корневой папке приложения");
            }
        }
    }
}