using FitnessClub.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessClub.DAL.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<FitnessClubContext> logger)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FitnessClubContext>();

            try
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
                if (!context.Clubs.Any())
                {
                    logger.LogInformation("Seeding Clubs data...");
                    var clubs = configuration.GetSection("SeedData:Clubs").Get<List<Club>>();
                    if (clubs != null && clubs.Any())
                    {
                        await context.Clubs.AddRangeAsync(clubs);
                        await context.SaveChangesAsync();
                        logger.LogInformation("{Count} Clubs seeded successfully.", clubs.Count);
                    }
                    else
                    {
                        logger.LogWarning("No Club seed data found in configuration.");
                    }
                }
                else
                {
                    logger.LogInformation("Clubs table already has data. Skipping seeding.");
                }

                if (!context.MembershipTypes.Any())
                {
                     logger.LogInformation("Seeding MembershipTypes data...");
                    var membershipTypes = configuration.GetSection("SeedData:MembershipTypes").Get<List<MembershipType>>();
                    if (membershipTypes != null && membershipTypes.Any())
                    {
                        await context.MembershipTypes.AddRangeAsync(membershipTypes);
                        await context.SaveChangesAsync();
                        logger.LogInformation("{Count} MembershipTypes seeded successfully.", membershipTypes.Count);
                    }
                     else
                    {
                         logger.LogWarning("No MembershipType seed data found in configuration.");
                    }
                }
                else
                {
                    logger.LogInformation("MembershipTypes table already has data. Skipping seeding.");
                }

                if (!context.Trainers.Any())
                {
                    logger.LogInformation("Seeding Trainers data...");
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
                           logger.LogInformation("{Count} Trainers seeded successfully.", validTrainers.Count);
                        }
                        if (invalidTrainers.Any())
                        {
                            logger.LogWarning("Skipped seeding {Count} trainers due to invalid ClubId.", invalidTrainers.Count);
                        }
                    }
                    else
                    {
                        logger.LogWarning("No Trainer seed data found in configuration.");
                    }
                }
                else
                {
                     logger.LogInformation("Trainers table already has data. Skipping seeding.");
                }

                if (!context.ClassSchedules.Any())
                {
                    logger.LogInformation("Seeding ClassSchedules data...");
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
                            logger.LogInformation("{Count} ClassSchedules seeded successfully.", validSchedules.Count);
                        }
                         if (invalidSchedules.Any())
                        {
                            logger.LogWarning("Skipped seeding {Count} schedules due to invalid ClubId or TrainerId.", invalidSchedules.Count);
                        }
                    }
                    else
                    {
                        logger.LogWarning("No ClassSchedule seed data found in configuration.");
                    }
                }
                else
                {
                    logger.LogInformation("ClassSchedules table already has data. Skipping seeding.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing or seeding the database.");
                throw;
            }
        }
    }
} 