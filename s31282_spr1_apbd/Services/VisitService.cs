using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using s31282_spr1_apbd.Models;
using s31282_spr1_apbd.Exceptions;
using System.Data.Common;

namespace s31282_spr1_apbd.Services
{
    public class VisitService : IVisitService
    {
        private readonly IConfiguration _configuration;
        public VisitService(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public async Task<GetVisitByIdDTO> GetVisitByIdAsync(int id)
        {
            if(id < 0)
            {
                throw new ArgumentException("id < 0");
            }


            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand();

            command.Connection = connection;
            await connection.OpenAsync();

            GetVisitByIdDTO ret = new GetVisitByIdDTO();

            command.Parameters.Clear();
            command.CommandText = "SELECT count(*) FROM Visit WHERE visit_id = @ID";
            command.Parameters.AddWithValue("@ID",id);
            int count = (int)await command.ExecuteScalarAsync();
            if (count == 0)
            {
                throw new EntityNotFoundException("visit not found in db");
            }

            command.Parameters.Clear();
            command.CommandText = "SELECT * FROM Visit WHERE visit_id = @ID";
            command.Parameters.AddWithValue("@ID", id);

            int clientid = -1;

            using (var rdr = await command.ExecuteReaderAsync())
            {
                while (await rdr.ReadAsync())
                {
                    ret.date = rdr.GetDateTime(rdr.GetOrdinal("date"));
                    clientid = rdr.GetInt32(rdr.GetOrdinal("client_id"));
                    ret.mechanic.mechanicId = rdr.GetInt32(rdr.GetOrdinal("mechanic_id"));
                }
            }

            if(clientid == -1)
            {
                throw new EntityNotFoundException("client associated with visit not found");
            }

            command.Parameters.Clear();
            command.CommandText = "SELECT * FROM Client WHERE client_id = @ID";
            command.Parameters.AddWithValue("@ID", clientid);

            using (var rdr = await command.ExecuteReaderAsync())
            {
                while (await rdr.ReadAsync())
                {
                    ret.client.firstName = rdr.GetString(rdr.GetOrdinal("first_name"));
                    ret.client.lastName = rdr.GetString(rdr.GetOrdinal("last_name"));
                    ret.client.dateOfBirth = rdr.GetDateTime(rdr.GetOrdinal("date_of_birth"));
                }
            }

            command.Parameters.Clear();
            command.CommandText = "SELECT licenceNumber FROM Mechanic WHERE mechanic_id = @ID";
            command.Parameters.AddWithValue("@ID", ret.mechanic.mechanicId);

            string? licencenum = (string?)await command.ExecuteScalarAsync();
            if (licencenum != null)
            {
                ret.mechanic.licenceNumber = licencenum;
            }
            else
            {
                throw new EntityNotFoundException("mechanic associated with visit not found");
            }

            command.Parameters.Clear();
            command.CommandText = "SELECT service_fee, name FROM Visit_Service, Service WHERE visit_id = @ID AND Visit_Service.service_id = Service.service_id";
            command.Parameters.AddWithValue("@ID", id);

            using (var rdr = await command.ExecuteReaderAsync())
            {
                while (await rdr.ReadAsync())
                {
                    ret.visitServices.Add(new ServiceVisitDTO
                    {
                        name = rdr.GetString(rdr.GetOrdinal("name")),
                        serviceFee = rdr.GetDecimal(rdr.GetOrdinal("service_fee"))
                    });
                }
            }


            return ret;
        }


        public async Task AddVisitAsync(AddVisitDTO dto)
        {
            if(dto.mechanicLicenceNumber.Length != 14 || !dto.mechanicLicenceNumber.StartsWith("MECH"))
            {
                throw new ArgumentException("invalid mechanic licence number");
            }

            if(dto.services.Count == 0)
            {
                throw new ArgumentException("empty services list");
            }

            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand();

            command.Connection = connection;
            await connection.OpenAsync();

            command.Parameters.Clear();
            command.CommandText = "SELECT COUNT(*) FROM Visit WHERE visit_id = @ID";
            command.Parameters.AddWithValue("@ID", dto.visitID);
            int count = (int)await command.ExecuteScalarAsync();

            if(count > 0)
            {
                throw new EntityConflictException("visit with this id already exists");
            }

            command.Parameters.Clear();
            command.CommandText = "SELECT COUNT(*) FROM Client WHERE client_id = @ID";
            command.Parameters.AddWithValue("@ID", dto.clientID);

            count = (int)await command.ExecuteScalarAsync();

            if (count == 0)
            {
                throw new EntityNotFoundException("client with this id doesnt exist");
            }

            command.Parameters.Clear();
            command.CommandText = "SELECT COUNT(*) FROM Mechanic WHERE licence_number = @ID";
            command.Parameters.AddWithValue("@ID", dto.mechanicLicenceNumber);

            count = (int)await command.ExecuteScalarAsync();

            if (count == 0)
            {
                throw new EntityNotFoundException("mechanic with this id doesnt exist");
            }

            for(int i = 0; i < dto.services.Count; i++)
            {
                command.Parameters.Clear();
                command.CommandText = "SELECT COUNT(*) FROM Service WHERE name = @ID";
                command.Parameters.AddWithValue("@ID", dto.services[i].serviceName);

                count = (int)await command.ExecuteScalarAsync();

                if (count == 0)
                {
                    throw new EntityNotFoundException("service with this name " + dto.services[i].serviceName +  "doesnt exist");
                }
            }


            command.Parameters.Clear();
            command.CommandText = "SELECT mechanic_id FROM Mechanic WHERE licence_number = @ID";
            command.Parameters.AddWithValue("@ID", dto.mechanicLicenceNumber);

            int mechanic_id = (int)await command.ExecuteScalarAsync();


            DbTransaction transaction = await connection.BeginTransactionAsync();
            command.Transaction = transaction as SqlTransaction;

            try
            {

                command.Parameters.Clear();
                command.CommandText = "INSERT INTO Visit (visit_id, client_id, mechanic_id, date) VALUES (@VID,@CID,@MID,@DT)";
                command.Parameters.AddWithValue("@VID", dto.visitID);
                command.Parameters.AddWithValue("@CID", dto.clientID);
                command.Parameters.AddWithValue("@MID", mechanic_id);
                command.Parameters.AddWithValue("@DT", DateTime.Now);
                await command.ExecuteNonQueryAsync();


                for (int i = 0; i < dto.services.Count; i++)
                {
                    command.Parameters.Clear();
                    command.CommandText = "SELECT service_id FROM Service WHERE name = @ID";
                    command.Parameters.AddWithValue("@ID", dto.services[i].serviceName);

                    int service_id = (int)await command.ExecuteScalarAsync();

                  
                    command.Parameters.Clear();
                    command.CommandText = "INSERT INTO Visit_Service (visit_id, service_id, service_fee) VALUES (@VID,@SID,@SF)";
                    command.Parameters.AddWithValue("@VID", dto.visitID);
                    command.Parameters.AddWithValue("@SID", service_id);
                    command.Parameters.AddWithValue("@SF", dto.services[i].serviceFee);
                    await command.ExecuteNonQueryAsync();
                }
                await transaction.CommitAsync();
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
