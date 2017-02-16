using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolHealthManagement.Models
{
    public class FileModel
    {
        public string FileName { get; set; }

        public decimal FileSize { get; set; }

        public DateTime FileLastModified { get; set; }

        public string FileDetails { get; set; }
    }
}