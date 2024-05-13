using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalManagement.Models.ViewModels
{
    public class MedicalRecordViewModel
    {
        public int MedicalRecordId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public DateTime DateCreated { get; set; }

        // Dropdown lists for Patients and Doctors
        public IEnumerable<SelectListItem> Patients { get; set; }
        public IEnumerable<SelectListItem> Doctors { get; set; }
    }
}
