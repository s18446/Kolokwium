using Kolokwium.DTOs.Requests;
using Kolokwium.DTOs.Responses;
using Kolokwium.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Kolokwium.Controllers
{

   
    [Route("api/prescriptions")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s18446;Integrated Security=True";

        /*      private readonly IAnimalsDbService _dbService;

              public AnimalsController(IAnimalsDbService dbService)
              {
                  _dbService = dbService;
              }
      */
        [HttpGet("{idprescription}")]
        public IActionResult GetPrescription(string idprescription)
        {
            GetPrescriptionResponse response = new GetPrescriptionResponse();

            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();
                com.CommandText = "SELECT * from prescription where idprescription=@idprescription ";
                com.Parameters.AddWithValue("idprescription", idprescription);

                var reader = com.ExecuteReader();

                if (reader.Read())
                {

                    response.IdPrescription = (int)reader["IdPrescription"];
                    response.IdPatient = (int)reader["IdPatient"];
                    response.IdDoctor = (int)reader["IdDoctor"];
                    response.Date = (DateTime)reader["Date"];
                    response.DueDate = (DateTime)reader["DueDate"];
                    response.Medicaments = new List<Medicament>();
                }
                else
                {
                    return BadRequest("Brak takiej recepty");
                }
                reader.Close();
                com.Parameters.Clear();
                com.CommandText = "SELECT m.name, m.description, m.type FROM prescription p INNER JOIN prescription_medicament pm ON pm.idprescription = p.idprescription " +
                                              " INNER JOIN medicament m ON pm.idmedicament = m.Idmedicament where p.idprescription = @idprescription ";
                com.Parameters.AddWithValue("idprescription", idprescription);
                reader = com.ExecuteReader();

                while (reader.Read())
                {
                    response.Medicaments.Add(
                        new Medicament
                        {
                            Name = reader["Name"].ToString(),
                            Type = reader["Type"].ToString(),
                            Description = reader["Description"].ToString()

                        });
                }




            }
            return Ok(response);
        }

        [HttpPost]
        public IActionResult AddPrescription(AddPrescriptionRequest request)
        {

            AddPrescriptionResponse response;

            using (var con = new SqlConnection(ConString))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                var tran = con.BeginTransaction();
                com.Transaction = tran;
                int idPrescription = 0;
               
                try
                {

                    if (DateTime.Compare(request.Date, request.DueDate) > 0)
                    {                 
                        tran.Rollback();
                        return BadRequest("Data waznosci starsza niz data wystawienia");
                    }

                    com.CommandText = "INSERT INTO Prescription(idPrescription, Date, DueDate, IdPatient, IdDoctor)  OUTPUT INSERTED.idprescription VALUES((select(select max(idEnrollment) from Enrollment) + 1),@Date, @DueDate, @IdPatient, @IdDoctor) ";                                         
                    com.Parameters.AddWithValue("Date", request.Date);
                    com.Parameters.AddWithValue("DueDate", request.DueDate);
                    com.Parameters.AddWithValue("IdDoctor", request.IdDoctor);
                    com.Parameters.AddWithValue("IdPatient", request.IdPatient);
                    var reader = com.ExecuteReader();

                    if (reader.Read())
                    {
                        idPrescription = (int)reader["idprescritpion"];
                    }
                    reader.Close();

                    response = new AddPrescriptionResponse()
                    {
                        Date = request.Date,
                        DueDate = request.DueDate,
                        IdDoctor = request.IdDoctor,
                        IdPatient = request.IdPatient,
                        IdPrescription = idPrescription
                    };

                    tran.Commit();


                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    tran.Rollback();
                    throw new ArgumentException(ex.Message);
                }

                return Ok(response);


            }
        }
    }
}
