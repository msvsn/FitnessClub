using FitnessClub.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessClub.DAL.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FitnessClubContext>();

            try
            {
                await context.Database.MigrateAsync();
                if (!context.Clubs.Any())
                {
                    var clubs = configuration.GetSection("SeedData:Clubs").Get<List<Club>>();
                    if (clubs != null && clubs.Any())
                    {
                        await context.Clubs.AddRangeAsync(clubs);
                        await context.SaveChangesAsync();
                    }
                }

                if (!context.MembershipTypes.Any())
                {
                    var membershipTypes = configuration.GetSection("SeedData:MembershipTypes").Get<List<MembershipType>>();
                    if (membershipTypes != null && membershipTypes.Any())
                    {
                        await context.MembershipTypes.AddRangeAsync(membershipTypes);
                        await context.SaveChangesAsync();
                    }
                }

                if (!context.Trainers.Any())
                {
                    var trainers = configuration.GetSection("SeedData:Trainers").Get<List<Trainer>>();
                    if (trainers != null && trainers.Any())
                    {
                        var existingClubIds = await context.Clubs.Select(c => c.ClubId).ToListAsync();
                        var validTrainers = trainers.Where(t => existingClubIds.Contains(t.ClubId)).ToList();
                        var invalidTrainers = trainers.Except(validTrainers).ToList();

                        if (validTrainers.Any())
                        {
                           await context.Trainers.AddRangeAsync(validTrainers);
                           await context.SaveChangesAsync();
                        }
                    }
                }

                if (!context.ClassSchedules.Any())
                {
                    var schedules = configuration.GetSection("SeedData:ClassSchedules").Get<List<ClassSchedule>>();
                    if (schedules != null && schedules.Any())
                    {
                        var existingClubIds = await context.Clubs.Select(c => c.ClubId).ToListAsync();
                        var existingTrainerIds = await context.Trainers.Select(t => t.TrainerId).ToListAsync();

                        var validSchedules = schedules.Where(s => existingClubIds.Contains(s.ClubId) && existingTrainerIds.Contains(s.TrainerId)).ToList();
                        var invalidSchedules = schedules.Except(validSchedules).ToList();

                        if (validSchedules.Any())
                        {
                            await context.ClassSchedules.AddRangeAsync(validSchedules);
                            await context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
} 