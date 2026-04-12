using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Models
{
    public class AssetDto
    {
        public int Id { get; set; }
        public string AssetName { get; set; }
        public string SerialNumber { get; set; }
        public string Description { get; set; }
        public string AssignedDate { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class AssetsResponse
    {
        public int Count { get; set; }
        public List<AssetDto> Assets { get; set; }
    }
}
