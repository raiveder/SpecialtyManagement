namespace SpecialtyManagement
{
    public partial class Teachers
    {
        public int SequenceNumber { get; set; }

        public string FullName
        {
            get
            {
                if (Patronymic == null)
                {
                    return Surname + " " + Name;
                }
                return Surname + " " + Name + " " + Patronymic;
            }
            set { }
        }

        public string ShortName
        {
            get
            {
                if (Patronymic == null)
                {
                    return Surname + " " + Name[0] + ".";
                }
                return Surname + " " + Name[0] + ". " + Patronymic[0] + ".";
            }
            set { }
        }
    }
}