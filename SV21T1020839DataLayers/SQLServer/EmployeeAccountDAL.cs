using Dapper;
using DataLayers.SQLServer;
using SV21T1020839.DomainModels;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace SV21T1020839.DataLayers.SQLServer
{
    public class EmployeeAccountDAL : BaseDAL, IUserAccountDAL
    {
        public EmployeeAccountDAL(string connectionString) : base(connectionString)
        {
        }

        public UserAccount? Authorize(string username, string password)
        {
            UserAccount? data = null;
            using (var connection = Openconnection())
            {
                var sql = @"select EmployeeID as UserID,
		                            FullName as DisplayName,
		                            Email as UserName,
		                            Photo,
		                            RoleNames
                            from Employees
                            where Email = @Email and Password = @Password";
                var parameters = new
                {
                    Email = username,
                    Password = password
                };
                data = connection.QueryFirstOrDefault<UserAccount>(sql:sql, param: parameters,commandType:CommandType.Text);
                connection.Close();
            }
            return data;
        }

        public bool ChangePassword(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
