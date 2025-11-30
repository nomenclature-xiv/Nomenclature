using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NomenclatureServer.Models
{
    public class CharacterResponseModel
    {
        public string persistent_key { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string home_world {  get; set; } = string.Empty;
        public string data_center { get; set; } = string.Empty;
        public string lodestone_id { get; set; } = string.Empty;
        public string avatar_url {  get; set; } = string.Empty;
        public string portrait_url {  get; set; } = string.Empty;
        public string created_at { get; set; } = string.Empty;
        public string verified_at { get; set; } = string.Empty;
        public string updated_at { get; set; } = string.Empty;
    }
}
