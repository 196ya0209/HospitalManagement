using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalManagement.Models.ViewModels
{
    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime DateScheduled { get; set; }
        public bool IsAccepted { get; set; }
        public bool PaymentStatus { get; set; }
        public decimal AmountPaid { get; set; }

        // Dropdown lists for Patients and Doctors
        public IEnumerable<SelectListItem> Patients { get; set; }
        public IEnumerable<SelectListItem> Doctors { get; set; }
    }
}
