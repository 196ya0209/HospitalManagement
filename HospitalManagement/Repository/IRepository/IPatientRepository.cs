using HospitalManagement.Models;

namespace HospitalManagement.Repository.IRepository
{
    public interface IPatientRepository
    {
        public Task<List<Patient>> GetAll();
        public Task<Patient> GetPatient(int patientId);

        public Task AddPatientUser(Patient patient);
        public Task AddPatient(Patient patient);
        public Task UpdatePatient(Patient patient);
        public Task<bool> RemovePatient(int patientId);       
    }
}
