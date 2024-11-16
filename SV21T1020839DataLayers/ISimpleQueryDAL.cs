namespace SV21T1020839.DataLayers
{
    public interface ISimpleQueryDAL< T> where T : class
    {
        List<T> List();
    }
}
