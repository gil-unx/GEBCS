using System.Collections.Generic;

namespace GEBCS
{
    public class ResNames
    {
        public List<string> ?Names { get; set; }
    }
    public class Tr2Names
    {
        public List<string> ?Names { get; set; }
    }
    public class PackageContent
    {
        public string? Name { get; set; }
        public int Offset { get; set; }    
    }
    public class PackageFiles
    {
        public List<PackageContent>? Files { get; set; }
    }
}
