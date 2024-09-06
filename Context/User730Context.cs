using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using languageTab.Models;

namespace languageTab.Context;

public partial class User730Context : DbContext
{
    public User730Context()
    {
    }

    public User730Context(DbContextOptions<User730Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ClientsFile> ClientsFiles { get; set; }

    public virtual DbSet<ClientsTag> ClientsTags { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<VisitsLog> VisitsLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=192.168.2.159;Database=user730;Username=user730;password=27791");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.IdClient).HasName("clients_pk");

            entity.ToTable("clients");

            entity.Property(e => e.IdClient)
                .ValueGeneratedNever()
                .HasColumnName("id_client");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.Firstname)
                .HasColumnType("character varying")
                .HasColumnName("firstname");
            entity.Property(e => e.IdGender).HasColumnName("id_gender");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasColumnType("character varying")
                .HasColumnName("phone");
            entity.Property(e => e.Photo)
                .HasColumnType("character varying")
                .HasColumnName("photo");
            entity.Property(e => e.RegistrationDate).HasColumnName("registration_date");
            entity.Property(e => e.Surname)
                .HasColumnType("character varying")
                .HasColumnName("surname");

            entity.HasOne(d => d.IdGenderNavigation).WithMany(p => p.Clients)
                .HasForeignKey(d => d.IdGender)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("clients_gender_fk");
        });

        modelBuilder.Entity<ClientsFile>(entity =>
        {
            entity.HasKey(e => e.IdClientfile).HasName("clients_files_pk");

            entity.ToTable("clients_files");

            entity.Property(e => e.IdClientfile)
                .ValueGeneratedNever()
                .HasColumnName("id_clientfile");
            entity.Property(e => e.IdClient).HasColumnName("id_client");
            entity.Property(e => e.IdFile).HasColumnName("id_file");

            entity.HasOne(d => d.IdClientNavigation).WithMany(p => p.ClientsFiles)
                .HasForeignKey(d => d.IdClient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("clients_files_clients_fk");

            entity.HasOne(d => d.IdFileNavigation).WithMany(p => p.ClientsFiles)
                .HasForeignKey(d => d.IdFile)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("clients_files_files_fk");
        });

        modelBuilder.Entity<ClientsTag>(entity =>
        {
            entity.HasKey(e => e.IdClientTag).HasName("clients_tags_pk");

            entity.ToTable("clients_tags");

            entity.Property(e => e.IdClientTag)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_client_tag");
            entity.Property(e => e.IdClient).HasColumnName("id_client");
            entity.Property(e => e.IdTag).HasColumnName("id_tag");

            entity.HasOne(d => d.IdClientNavigation).WithMany(p => p.ClientsTags)
                .HasForeignKey(d => d.IdClient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("clients_tags_clients_fk");

            entity.HasOne(d => d.IdTagNavigation).WithMany(p => p.ClientsTags)
                .HasForeignKey(d => d.IdTag)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("clients_tags_tags_fk");
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.HasKey(e => e.IdFile).HasName("files_pk");

            entity.ToTable("files");

            entity.Property(e => e.IdFile)
                .ValueGeneratedNever()
                .HasColumnName("id_file");
            entity.Property(e => e.Link)
                .HasColumnType("character varying")
                .HasColumnName("link");
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.IdGender).HasName("gender_pk");

            entity.ToTable("gender");

            entity.Property(e => e.IdGender)
                .ValueGeneratedNever()
                .HasColumnName("id_gender");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.IdTag).HasName("tags_pk");

            entity.ToTable("tags");

            entity.Property(e => e.IdTag)
                .ValueGeneratedNever()
                .HasColumnName("id_tag");
            entity.Property(e => e.ColorTag)
                .HasColumnType("character varying")
                .HasColumnName("color_tag");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<VisitsLog>(entity =>
        {
            entity.HasKey(e => e.IdLog).HasName("visits_log_pk");

            entity.ToTable("visits_log");

            entity.Property(e => e.IdLog)
                .ValueGeneratedNever()
                .HasColumnName("id_log");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.IdClient).HasColumnName("id_client");

            entity.HasOne(d => d.IdClientNavigation).WithMany(p => p.VisitsLogs)
                .HasForeignKey(d => d.IdClient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("visits_log_clients_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
