using HospitalManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagement.ViewComponents
{
    public class BookAppointmentViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Appointment appointment)
        {
            return View(appointment);
        }
    }
}
