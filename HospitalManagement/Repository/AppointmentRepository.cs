using HospitalManagement.Data;
using HospitalManagement.Models;
using HospitalManagement.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HospitalManagement.Repository
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AppointmentRepository> _logger;        

        public AppointmentRepository(ApplicationDbContext db, ILogger<AppointmentRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task AddAppointmentAsync(Appointment appointment)
        {
            try
            {
                _db.Appointments.Add(appointment);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while adding an appointment");
                throw;
            }
        }

        public async Task DeleteAppointmentAsync(int id)
        {   
            try
            {
                var appointment = await _db.Appointments.FindAsync(id);
                if (appointment != null)
                {
                    _db.Appointments.Remove(appointment);
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while deleting an appointment");
                throw;
            }
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
        {
            return await _db.Appointments.ToListAsync();
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int id)
        {
            return await _db.Appointments.FindAsync(id);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsForDoctorAsync(string id)
        {
            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.DoctorUserId == id);
            if(doctor != null)
            {
                return await _db.Appointments
            .Where(a => a.DoctorId == doctor.DoctorId)
            .ToListAsync();
            }
            return null;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsForPatientAsync(string patientId)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(d => d.PatientUserId == patientId);
            if (patient != null)
            {
                return await _db.Appointments
            .Where(a => a.PatientId == patient.PatientId)
            .ToListAsync();
            }
            return null;
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            try
            {
                _db.Entry(appointment).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while updating an appointment");
                throw;
            }
        }
    }
}
