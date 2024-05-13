using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }
        public string? PatientUserId { get; set; }
        [Required]
        public string? PatientName { get; set; }
        [Required]
        public string? Gender { get; set; }
        [Required]
        public DateTime AdmissionDate { get; set; }
        [Required]
        public int WardNumber { get; set; }
        [Required]
        public int PatientAge { get; set; }
        public bool IsActive { get; set; }
    }
}
