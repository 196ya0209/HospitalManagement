using HospitalManagement.Models;

namespace HospitalManagement.Repository.IRepository
{
    public interface IAppointmentRepository
    {
        Task AddAppointmentAsync(Appointment appointment);
        Task<Appointment> GetAppointmentByIdAsync(int id);
        Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
        Task<IEnumerable<Appointment>> GetAppointmentsForPatientAsync(string patientId);
        Task<IEnumerable<Appointment>> GetAppointmentsForDoctorAsync(string id);
        Task UpdateAppointmentAsync(Appointment appointment);
        Task DeleteAppointmentAsync(int id);
    }
}
