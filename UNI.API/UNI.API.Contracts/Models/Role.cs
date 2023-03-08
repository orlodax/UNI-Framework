using UNI.Core.Library;

namespace UNI.API.Contracts.Models
{
    [ClassInfo(SQLName = "roles")]
    public class Role : BaseModel
    {
        [ValueInfo(SQLName = "name", IsDisplayProperty = true)]
        public string Name { get; set; }
    }
}
