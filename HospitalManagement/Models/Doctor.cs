using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }
        public string? DoctorUserId { get; set; }
        [Required(ErrorMessage ="Please Enter Doctor Name")]
        public string? DoctorName { get; set; }
        [Required(ErrorMessage = "Please Enter Specialization")]
        public string? Specialization { get; set; }
        public byte[]? DocImageUrl { get; set; }
    }
}
