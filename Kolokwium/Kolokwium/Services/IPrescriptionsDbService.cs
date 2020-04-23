using Kolokwium.DTOs.Requests;
using Kolokwium.DTOs.Responses;
using Kolokwium.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Kolokwium.Services
{
    interface IPrescriptionsDbService
    {
        public GetPrescriptionResponse GetPrescription(string prescription);

        public AddPrescriptionResponse AddPrescription(AddPrescriptionRequest request);
    }
}
