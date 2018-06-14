using Microsoft.EntityFrameworkCore;
using Projecto.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.Models
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<Issue> Issue { get; set; }
        public virtual DbSet<IssueAssignment> IssueAssignment { get; set; }
        public virtual DbSet<IssueAssignmentRole> IssueAssignmentRole { get; set; }
        public virtual DbSet<IssueAttachment> IssueAttachment { get; set; }
        public virtual DbSet<IssuePriority> IssuePriority { get; set; }
        public virtual DbSet<IssueStatus> IssueStatus { get; set; }
        public virtual DbSet<IssueType> IssueType { get; set; }
        public virtual DbSet<Project> Project { get; set; }
        public virtual DbSet<ProjectActivity> ProjectActivity { get; set; }
        public virtual DbSet<ProjectAssignment> ProjectAssignment { get; set; }
        public virtual DbSet<ProjectAssignmentRole> ProjectAssignmentRole { get; set; }
        public virtual DbSet<ProjectVersion> ProjectVersion { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserGroup> UserGroup { get; set; }

        public DatabaseContext(DbContextOptions options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Issue>(entity =>
            {
                entity.ToTable("issue");

                entity.HasIndex(e => e.ParentIssueId)
                    .HasName("issue_fk3");

                entity.HasIndex(e => e.PriorityId)
                    .HasName("issue_fk2");

                entity.HasIndex(e => e.ProjectVersionId)
                    .HasName("issue_fk4");

                entity.HasIndex(e => e.StatusId)
                    .HasName("issue_fk1");

                entity.HasIndex(e => e.TypeId)
                    .HasName("issue_fk0");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasMaxLength(4096);

                entity.Property(e => e.Done)
                    .HasColumnName("done")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.DueDate)
                    .HasColumnName("due_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.EstimatedTime)
                    .HasColumnName("estimated_time")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ParentIssueId)
                    .HasColumnName("parent_issue_id")
                    .HasColumnType("int(11)")
                    .IsRequired(false);

                entity.Property(e => e.PriorityId)
                    .HasColumnName("priority_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ProjectVersionId)
                    .HasColumnName("project_version_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.StatusId)
                    .HasColumnName("status_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasColumnName("subject")
                    .HasMaxLength(256);

                entity.Property(e => e.TypeId)
                    .HasColumnName("type_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.ParentIssue)
                    .WithMany()
                    .HasForeignKey(d => d.ParentIssueId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("issue_fk3");

                entity.HasOne(d => d.Priority)
                    .WithMany(p => p.Issues)
                    .HasForeignKey(d => d.PriorityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("issue_fk2");

                entity.HasOne(d => d.ProjectVersion)
                    .WithMany(p => p.Issues)
                    .HasForeignKey(d => d.ProjectVersionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("issue_fk4");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Issues)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("issue_fk1");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Issues)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("issue_fk0");
            });

            modelBuilder.Entity<IssueAssignment>(entity =>
            {
                entity.ToTable("issue_assignment");

                entity.HasIndex(e => e.AssigneeId)
                    .HasName("issue_assignment_fk1");

                entity.HasIndex(e => e.IssueId)
                    .HasName("issue_assignment_fk0");

                entity.HasIndex(e => e.RoleId)
                    .HasName("issue_assignment_fk2");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AssigneeId)
                    .HasColumnName("assignee_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IssueId)
                    .HasColumnName("issue_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RoleId)
                    .HasColumnName("role_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Assignee)
                    .WithMany(p => p.IssueAssignments)
                    .HasForeignKey(d => d.AssigneeId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("issue_assignment_fk1");

                entity.HasOne(d => d.Issue)
                    .WithMany(p => p.IssueAssignments)
                    .HasForeignKey(d => d.IssueId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("issue_assignment_fk0");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.IssueAssignments)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("issue_assignment_fk2");
            });

            modelBuilder.Entity<IssueAssignmentRole>(entity =>
            {
                entity.ToTable("issue_assignment_role");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<IssueAttachment>(entity =>
            {
                entity.ToTable("issue_attachment");

                entity.HasIndex(e => e.IssueId)
                    .HasName("issue_attachment_fk0");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Comment)
                    .IsRequired()
                    .HasColumnName("comment")
                    .HasMaxLength(512);

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasColumnName("content")
                    .HasColumnType("mediumtext");

                entity.Property(e => e.IssueId)
                    .HasColumnName("issue_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Issue)
                    .WithMany(p => p.IssueAttachments)
                    .HasForeignKey(d => d.IssueId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("issue_attachment_fk0");
            });

            modelBuilder.Entity<IssuePriority>(entity =>
            {
                entity.ToTable("issue_priority");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<IssueStatus>(entity =>
            {
                entity.ToTable("issue_status");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<IssueType>(entity =>
            {
                entity.ToTable("issue_type");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("project");

                entity.HasIndex(e => e.ParentProjectId)
                    .HasName("project_fk0");

                entity.HasIndex(e => e.Url)
                    .HasName("url")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DueDate)
                    .HasColumnName("due_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(128);

                entity.Property(e => e.Overview)
                    .IsRequired()
                    .HasColumnName("overview")
                    .HasMaxLength(4096);

                entity.Property(e => e.ParentProjectId)
                    .HasColumnName("parent_project_id")
                    .HasColumnType("int(11)")
                    .IsRequired(false);

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnName("url")
                    .HasMaxLength(64);

                entity.Property(e => e.Wiki)
                    .IsRequired()
                    .HasColumnName("wiki")
                    .HasColumnType("mediumtext");

                entity.HasOne(d => d.ParentProject)
                    .WithMany()
                    .HasForeignKey(d => d.ParentProjectId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("project_fk0");
            });

            modelBuilder.Entity<ProjectActivity>(entity =>
            {
                entity.ToTable("project_activity");

                entity.HasIndex(e => e.AuthorId)
                    .HasName("project_activity_fk1");

                entity.HasIndex(e => e.ProjectId)
                    .HasName("project_activity_fk0");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AuthorId)
                    .HasColumnName("author_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasColumnName("content")
                    .HasMaxLength(1024);

                entity.Property(e => e.ProjectId)
                    .HasColumnName("project_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Time)
                    .HasColumnName("time")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.HasOne(d => d.Author)
                    .WithMany(p => p.ProjectActivities)
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("project_activity_fk1");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectActivities)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("project_activity_fk0");
            });

            modelBuilder.Entity<ProjectAssignment>(entity =>
            {
                entity.ToTable("project_assignment");

                entity.HasIndex(e => e.AssigneeId)
                    .HasName("project_assignment_fk1");

                entity.HasIndex(e => e.ProjectId)
                    .HasName("project_assignment_fk0");

                entity.HasIndex(e => e.RoleId)
                    .HasName("project_assignment_fk2");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AssigneeId)
                    .HasColumnName("assignee_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ProjectId)
                    .HasColumnName("project_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RoleId)
                    .HasColumnName("role_id")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Assignee)
                    .WithMany(p => p.ProjectAssignments)
                    .HasForeignKey(d => d.AssigneeId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("project_assignment_fk1");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectAssignments)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("project_assignment_fk0");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.ProjectAssignments)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("project_assignment_fk2");
            });

            modelBuilder.Entity<ProjectAssignmentRole>(entity =>
            {
                entity.ToTable("project_assignment_role");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<ProjectVersion>(entity =>
            {
                entity.ToTable("project_version");

                entity.HasIndex(e => e.ProjectId)
                    .HasName("project_version_fk0");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DueDate)
                    .HasColumnName("due_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(128);

                entity.Property(e => e.Overview)
                    .IsRequired()
                    .HasColumnName("overview")
                    .HasMaxLength(4096);

                entity.Property(e => e.ProjectId)
                    .HasColumnName("project_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasColumnName("url")
                    .HasMaxLength(16);

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectVersions)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("project_version_fk0");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.HasIndex(e => e.Email)
                    .HasName("email")
                    .IsUnique();

                entity.HasIndex(e => e.GroupId)
                    .HasName("user_fk0");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(128);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("first_name")
                    .HasMaxLength(128);

                entity.Property(e => e.GroupId)
                    .HasColumnName("group_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("last_name")
                    .HasMaxLength(128);

                entity.Property(e => e.LastSeenOn)
                    .HasColumnName("last_seen_on")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Password)
                    .IsRequired(false)
                    .HasColumnName("password")
                    .HasMaxLength(256);

                entity.Property(e => e.RegisteredOn)
                    .HasColumnName("registered_on")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_fk0");
            });

            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.ToTable("user_group");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(64);
            });
        }
    }
}
