using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalAppointmentSystem.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;

namespace HospitalAppointmentSystem.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly HospitalContext _context;

        public AppointmentsController(HospitalContext context)
        {
            _context = context;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient) // Include related Patient data
                .Include(a => a.Doctor)  // Include related Doctor data
                .ToListAsync();
            return View(appointments);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            // Populate dropdown lists for Patients and Doctors
            ViewData["Patients"] = new SelectList(_context.Patients, "PatientID", "Name");
            ViewData["Doctors"] = new SelectList(_context.Doctors, "DoctorID", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientID, DoctorID, AppointmentDate, Status")] Appointment appointment)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Validation Failed!");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Validation Error: " + error.ErrorMessage);
                }

                ViewData["Patients"] = new SelectList(_context.Patients, "PatientID", "Name", appointment.PatientID);
                ViewData["Doctors"] = new SelectList(_context.Doctors, "DoctorID", "Name", appointment.DoctorID);
                return View(appointment);
            }

            try
            {
                // ✅ Fix: Convert the appointment date to the correct format
                appointment.AppointmentDate = DateTime.ParseExact(appointment.AppointmentDate.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null);

                _context.Add(appointment);
                await _context.SaveChangesAsync();

                Console.WriteLine("Appointment Scheduled Successfully!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Saving Appointment: " + ex.Message);
                ModelState.AddModelError("", "An error occurred while scheduling the appointment.");
                ViewData["Patients"] = new SelectList(_context.Patients, "PatientID", "Name", appointment.PatientID);
                ViewData["Doctors"] = new SelectList(_context.Doctors, "DoctorID", "Name", appointment.DoctorID);
                return View(appointment);
            }
        }


        // GET: Appointments/Edit/5
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, [Bind("AppointmentID, PatientID, DoctorID, AppointmentDate, Status")] Appointment appointment)
{
    if (id != appointment.AppointmentID)
    {
        return NotFound();
    }

    // Check if the selected Patient and Doctor exist
    var patientExists = await _context.Patients.AnyAsync(p => p.PatientID == appointment.PatientID);
    var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorID == appointment.DoctorID);

    if (!patientExists)
    {
        ModelState.AddModelError("PatientID", "Selected Patient does not exist.");
    }

    if (!doctorExists)
    {
        ModelState.AddModelError("DoctorID", "Selected Doctor does not exist.");
    }

    if (!ModelState.IsValid)
    {
        // Reload dropdowns if validation fails
        ViewData["Patients"] = new SelectList(_context.Patients, "PatientID", "Name", appointment.PatientID);
        ViewData["Doctors"] = new SelectList(_context.Doctors, "DoctorID", "Name", appointment.DoctorID);
        return View(appointment);
    }

    try
    {
        _context.Update(appointment);
        await _context.SaveChangesAsync();
        Console.WriteLine("Appointment Updated Successfully!");
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error Updating Appointment: " + ex.Message);
        ModelState.AddModelError("", "An error occurred while updating the appointment.");
        return View(appointment);
    }
}


        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointmentID == id);
        }
    }
}
