using SV21T1020839.DomainModels;

namespace SV21T1020839.DataLayers
{
    public interface IUserAccountDAL
    {
        UserAccount? Authorize(string username, string password);   

        bool ChangePassword(string username, string password); 
    }
}
