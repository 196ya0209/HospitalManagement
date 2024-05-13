using HospitalManagement.Data;
using HospitalManagement.Models;
using HospitalManagement.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Repository
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DoctorRepository> _logger;
        public DoctorRepository(ApplicationDbContext db, ILogger<DoctorRepository> logger)
        {
            _db = db;
            _logger = logger;

        }
        public async Task AddDoctor(Doctor doctor)
        {
            await _db.Doctors.AddAsync(doctor);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Doctor added successfully");
        }

        public async Task AddDoctorUser(Doctor doctor)
        {
            await _db.Doctors.AddAsync(doctor);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Doctor added successfully");
        }

        public async Task<List<Doctor>> GetAll()
        {
            var doctors = await _db.Doctors.ToListAsync();
            return doctors;
        }

        public async Task<Doctor?> GetDoctor(int doctorId)
        {
            var doctor = await _db.Doctors.FirstOrDefaultAsync(x => x.DoctorId == doctorId); 
            if(doctor != null)
            {
                _logger.LogInformation("Doctor found -> " + doctorId);
                return doctor;
            }
            _logger.LogError("Doctor Not found -> " + doctorId);
            return null;
        }

        public async Task<bool> RemoveDoctor(int doctorId)
        {
            var doctor = await _db.Doctors.FirstOrDefaultAsync(x => x.DoctorId == doctorId);
            if (doctor != null)
            {
                _db.Doctors.Remove(doctor);
                _logger.LogInformation("Doctor Removed Successfully -> " + doctor.DoctorName);
                await _db.SaveChangesAsync();                
                return true;
            }
            _logger.LogError("Doctor Not found -> " + doctorId);
            return false;
        }

        public async Task UpdateDoctor(Doctor doctor)
        {
            if (doctor != null)
            {
                
                _db.Doctors.Update(doctor);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Doctor updated Successfully -> " + doctor.DoctorName);
            }
        }
    }
}
