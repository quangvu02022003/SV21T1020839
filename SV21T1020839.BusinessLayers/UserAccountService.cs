using DataLayers;
using SV21T1020839.DataLayers;
using SV21T1020839.DomainModels;

namespace SV21T1020839.BusinessLayers
{
    public class UserAccountService
    {
        private static readonly IUserAccountDAL employeeAccountDB;
        private static readonly IUserAccountDAL customerAccountDB;
        static UserAccountService()
        {
            string connectionString = "Server=QUANGVU\\VU;User ID=sa;Password=02022003vu;database=LiteCommerceDB;TrustServerCertificate=True;";
            employeeAccountDB = new DataLayers.SQLServer.EmployeeAccountDAL(connectionString);
            customerAccountDB = new DataLayers.SQLServer.CustomerAccountDAL(connectionString);  
        }

        public static UserAccount? Authorize(UserTypes userType, string username, string password)
        {
            if (userType == UserTypes.Employee)
                return employeeAccountDB.Authorize(username, password);
            else
                return customerAccountDB.Authorize(username, password);
            
        }
    }
    public enum UserTypes
    {
        Employee,
        Customer
    }
}
