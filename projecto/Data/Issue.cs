using System;
using System.Collections.Generic;

namespace Projecto.Data
{
    public partial class Issue
    {
        public Issue()
        {
            IssueAssignments = new HashSet<IssueAssignment>();
            IssueAttachments = new HashSet<IssueAttachment>();
        }

        public int Id { get; set; }
        public int TypeId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public int PriorityId { get; set; }
        public int? ParentIssueId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int EstimatedTime { get; set; }
        public int Done { get; set; }
        public int ProjectVersionId { get; set; }

        public Issue ParentIssue { get; set; }
        public IssuePriority Priority { get; set; }
        public ProjectVersion ProjectVersion { get; set; }
        public IssueStatus Status { get; set; }
        public IssueType Type { get; set; }
        public ICollection<IssueAssignment> IssueAssignments { get; set; }
        public ICollection<IssueAttachment> IssueAttachments { get; set; }
    }
}
