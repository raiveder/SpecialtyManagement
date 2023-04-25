namespace SpecialtyManagement
{
    public partial class Teachers
    {
        public int SequenceNumber { get; set; }

        public string FullName
        {
            get => Surname + " " + Name + " " + Patronymic;
        }
    }
}