using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagement.Models
{
    public class Appointment
    {
        //Can be done by patients upon signup
        [Key]
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }
        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public Doctor? Doctor { get; set; }
        public DateTime DateScheduled { get; set; }
        public bool IsAccepted { get; set; }
        public bool PaymentStatus { get; set; }
        public decimal AmountPaid { get; set; }
    }
}
