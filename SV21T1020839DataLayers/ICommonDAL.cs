namespace DataLayers
{
    //Tìm kiếm  và lấy dữ liệu dưới dạng phân trang
    public interface ICommonDAL< T> where T : class
    {
        List<T> List(int page = 1, int pageSize = 0 , string searchValue="");
        int Count(string searchValue = "");
        T? Get(int id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        int Add(T data);
        bool Update(T data);
        bool Delete(int id);
        bool InUsed(int id);
    }
}
