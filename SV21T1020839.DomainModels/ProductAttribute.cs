namespace SV21T1020839.DomainModels
{
    public class ProductAttribute
    {
        public long AttributeId { get; set; }
        public long ProductId { get; set; }
        public string AttributeName { get; set; } = "";
        public string AttributeValue { get; set; } = "";
        public int DisplayOrder {  get; set; }
    }
}
