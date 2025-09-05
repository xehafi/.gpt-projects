
namespace Tigrinya.App.Mobile.Services
{
    internal class QaRecord
    {
        public string Key { get; set; }
        public string ContentId { get; set; }
        public string ItemId { get; set; }
        public bool Approved { get; set; }
        public string Reviewer { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}