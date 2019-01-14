using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Data.Model.EFDbModel
{
    public class CustomWebService
    {
        public int ID { get; set; }
        public ICollection<CustomWsBinding> Bindings { get; set; }
        [Required] public string Name { get; set; }

        //Some examples of ServiceStr valid content:
        //http://{node_ip}:{param1}/{param2}
        //https://example.com/{param1}/{param2}/{param3}
        [Required] public string ServiceStr { get; set; }
        // ReSharper disable IdentifierTypo
        public string Parametr1Name { get; set; }
        public string Parametr2Name { get; set; }
        public string Parametr3Name { get; set; }
    }
}