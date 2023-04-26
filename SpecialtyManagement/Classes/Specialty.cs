using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialtyManagement
{
    public partial class Specialty
    {
        public string FullName
        {
            get => Code + " " + Name;
        }
    }
}