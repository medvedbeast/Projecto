using System;
using System.Collections.Generic;

namespace Projecto.Data
{
    public partial class ProjectActivity
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int AuthorId { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }

        public User Author { get; set; }
        public Project Project { get; set; }
    }
}
