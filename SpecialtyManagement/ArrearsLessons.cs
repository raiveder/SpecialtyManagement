//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SpecialtyManagement
{
    using System;
    using System.Collections.Generic;
    
    public partial class ArrearsLessons
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ArrearsLessons()
        {
            this.LiquidationsCompositions = new HashSet<LiquidationsCompositions>();
        }
    
        public int Id { get; set; }
        public int IdArrear { get; set; }
        public int IdLesson { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
    
        public virtual Arrears Arrears { get; set; }
        public virtual Lessons Lessons { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LiquidationsCompositions> LiquidationsCompositions { get; set; }
    }
}
