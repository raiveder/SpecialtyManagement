using System;
using System.Linq;

namespace SpecialtyManagement
{
    public class Database
    {
        public static SpecialtyManagementEntities Entities;

        /// <summary>
        /// Создаёт соединение с БД.
        /// </summary>
        /// <param name="message">текст исключения в случае его возникновения.</param>
        /// <returns>True, если соединение создано успешно, в противном случае - false.</returns>
        public static bool CreateEntities(out string message)
        {
            try
            {
                Entities = new SpecialtyManagementEntities();
                Students student = Entities.Students.FirstOrDefault();
                message = string.Empty;
                return true;
            }
            catch (Exception e)
            {
                message = e.Message;
                return false;
            }
        }
    }
}