using System;
using System.Collections.Generic;

namespace Projecto.Data
{
    public partial class IssueAttachment
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public string Content { get; set; }
        public string Comment { get; set; }

        public Issue Issue { get; set; }
    }
}
