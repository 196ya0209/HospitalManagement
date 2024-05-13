namespace HospitalManagement.Models.ViewModels
{
    public class ReceiptViewModel
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public decimal AmountPaid { get; set; }
    }
}
