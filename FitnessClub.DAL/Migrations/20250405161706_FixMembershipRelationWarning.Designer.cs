﻿// <auto-generated />
using System;
using FitnessClub.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FitnessClub.DAL.Migrations
{
    [DbContext(typeof(FitnessClubContext))]
    [Migration("20250405161706_FixMembershipRelationWarning")]
    partial class FixMembershipRelationWarning
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.3");

            modelBuilder.Entity("FitnessClub.DAL.Entities.Booking", b =>
                {
                    b.Property<int>("BookingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("BookingDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ClassDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("ClassScheduleId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("GuestName")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsMembershipBooking")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("BookingId");

                    b.HasIndex("ClassScheduleId");

                    b.HasIndex("UserId");

                    b.ToTable("Bookings");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.ClassSchedule", b =>
                {
                    b.Property<int>("ClassScheduleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BookedPlaces")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Capacity")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClassType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ClubId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DayOfWeek")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("EndTime")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("TrainerId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ClassScheduleId");

                    b.HasIndex("ClubId");

                    b.HasIndex("TrainerId");

                    b.ToTable("ClassSchedules");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.Club", b =>
                {
                    b.Property<int>("ClubId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("HasPool")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ClubId");

                    b.ToTable("Clubs");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.Membership", b =>
                {
                    b.Property<int>("MembershipId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ClubId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("MembershipTypeId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("MembershipTypeId1")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("MembershipId");

                    b.HasIndex("ClubId");

                    b.HasIndex("MembershipTypeId");

                    b.HasIndex("MembershipTypeId1");

                    b.HasIndex("UserId");

                    b.ToTable("Memberships");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.MembershipType", b =>
                {
                    b.Property<int>("MembershipTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("DurationDays")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsNetwork")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("MembershipTypeId");

                    b.ToTable("MembershipTypes");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.Trainer", b =>
                {
                    b.Property<int>("TrainerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClubId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Specialty")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("TrainerId");

                    b.HasIndex("ClubId");

                    b.ToTable("Trainers");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("UserId");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.Booking", b =>
                {
                    b.HasOne("FitnessClub.DAL.Entities.ClassSchedule", "ClassSchedule")
                        .WithMany("Bookings")
                        .HasForeignKey("ClassScheduleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FitnessClub.DAL.Entities.User", "User")
                        .WithMany("Bookings")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("ClassSchedule");

                    b.Navigation("User");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.ClassSchedule", b =>
                {
                    b.HasOne("FitnessClub.DAL.Entities.Club", "Club")
                        .WithMany("ClassSchedules")
                        .HasForeignKey("ClubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FitnessClub.DAL.Entities.Trainer", "Trainer")
                        .WithMany()
                        .HasForeignKey("TrainerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Club");

                    b.Navigation("Trainer");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.Membership", b =>
                {
                    b.HasOne("FitnessClub.DAL.Entities.Club", "Club")
                        .WithMany("Memberships")
                        .HasForeignKey("ClubId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("FitnessClub.DAL.Entities.MembershipType", "MembershipType")
                        .WithMany()
                        .HasForeignKey("MembershipTypeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("FitnessClub.DAL.Entities.MembershipType", null)
                        .WithMany("Memberships")
                        .HasForeignKey("MembershipTypeId1");

                    b.HasOne("FitnessClub.DAL.Entities.User", "User")
                        .WithMany("Memberships")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Club");

                    b.Navigation("MembershipType");

                    b.Navigation("User");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.Trainer", b =>
                {
                    b.HasOne("FitnessClub.DAL.Entities.Club", "Club")
                        .WithMany("Trainers")
                        .HasForeignKey("ClubId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Club");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.ClassSchedule", b =>
                {
                    b.Navigation("Bookings");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.Club", b =>
                {
                    b.Navigation("ClassSchedules");

                    b.Navigation("Memberships");

                    b.Navigation("Trainers");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.MembershipType", b =>
                {
                    b.Navigation("Memberships");
                });

            modelBuilder.Entity("FitnessClub.DAL.Entities.User", b =>
                {
                    b.Navigation("Bookings");

                    b.Navigation("Memberships");
                });
#pragma warning restore 612, 618
        }
    }
}
