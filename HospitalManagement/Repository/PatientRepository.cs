using HospitalManagement.Data;
using HospitalManagement.Models;
using HospitalManagement.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Numerics;

namespace HospitalManagement.Repository
{
    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PatientRepository> _logger;
        public PatientRepository(ApplicationDbContext db, ILogger<PatientRepository> logger)
        {
            _db = db;
            _logger = logger;

        }
        public async Task AddPatient(Patient patient)
        {
            await _db.Patients.AddAsync(patient);
            await _db.SaveChangesAsync();
            Log.Information("Patient added successfully");
        }

        public async Task AddPatientUser(Patient patient)
        {
            await _db.Patients.AddAsync(patient);
            await _db.SaveChangesAsync();
            Log.Information("Patient added successfully");
        }

        public async Task<List<Patient>> GetAll()
        {
            var patients = await _db.Patients.ToListAsync();
            return patients;
        }

        public async Task<Patient?> GetPatient(int patientId)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(x => x.PatientId == patientId); 
            if(patient != null)
            {
                Log.Information("Patient found -> " + patientId);
                return patient;
            }
            Log.Error("Patient Not found -> " + patientId);
            return null;
        }

        public async Task<bool> RemovePatient(int patientId)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(x => x.PatientId == patientId);
            if (patient != null)
            {
                _db.Patients.Remove(patient);
                Log.Information("Patient Removed Successfully -> " + patient.PatientName);
                await _db.SaveChangesAsync();                
                return true;
            }
            Log.Error("Patient Not found -> " + patientId);
            return false;
        }

        public async Task UpdatePatient(Patient patient)
        {
            if (patient != null)
            {
                _db.Patients.Update(patient);
                await _db.SaveChangesAsync();
                Log.Information("Patient updated Successfully -> " + patient.PatientName);
            }
        }
    }
}
