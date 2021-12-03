using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovernmentProcurement_ClassDB.Model
{
    public class Procurement
    {
        [Key]
        [Display(Name = "編號")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(50)]
        [Display(Name = "機關名稱")]
        public string ProcuringEntity { get; set; }

        [MaxLength(500)]
        [Display(Name = "標案案號及名稱")]
        public string SubjectOfProcurement { get; set; }

        [MaxLength(50)]
        [Display(Name = "傳輸次數")]
        public string TransmissionNumber { get; set; }

        [MaxLength(50)]
        [Display(Name = "招標方式")]
        public string TypeOfTendering { get; set; }

        [MaxLength(50)]
        [Display(Name = "採購性質")]
        public string TypeOfProcurement { get; set; }

        [Display(Name = "公告日期")]
        public DateTime? DateOfPublication { get; set; }

        [Display(Name = "截止投標")]
        public DateTime? DateLimitOfTenders { get; set; }

        [Display(Name = "預算金額")]
        public int? Budget { get; set; }
    }
}
