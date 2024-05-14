using HospitalManagement.Models.ViewModels;

namespace HospitalManagement.CustomExceptions
{
    public class AppointmentConflictException: Exception
    {
        public AppointmentConflictException() : base("Appointment scheduling conflict.")
        {
        }        
    }
}
