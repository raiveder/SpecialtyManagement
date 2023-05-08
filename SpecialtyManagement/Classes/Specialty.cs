namespace SpecialtyManagement
{
    public partial class Specialty
    {
        public string FullName
        {
            get => Code + " " + Name;
            set { }
        }
    }
}