using System.ComponentModel;
using System.Linq;

namespace SpecialtyManagement.Classes
{
    public class Setting : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Specialty
        {
            get
            {
                Specialty specialty = Database.Entities.Specialty.FirstOrDefault();

                if (specialty != null)
                {
                    return specialty.FullName;
                }

                return "Специальность не указана";
            }
            set { }
        }

        public string HeadOfSpecialty
        {
            get
            {
                Specialty specialty = Database.Entities.Specialty.FirstOrDefault();

                if (specialty != null)
                {
                    return specialty.Head;
                }

                return "Зав. отделением не указан";
            }
            set { }
        }

        public void UpdateSettings()
        {
            PropertyChanged(this, new PropertyChangedEventArgs("Specialty"));
            PropertyChanged(this, new PropertyChangedEventArgs("HeadOfSpecialty"));
        }
    }
}