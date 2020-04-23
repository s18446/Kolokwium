using Kolokwium.DTOs.Responses;
using Kolokwium.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Kolokwium.Controllers
{
    [Route("api/prescriptions")]
    [ApiController]
    public class PrescriptionsController :ControllerBase
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

                while(reader.Read())
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
        public IActionResult AddPrescription(string request)
    }
}
