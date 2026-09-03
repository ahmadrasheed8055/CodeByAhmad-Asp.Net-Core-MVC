using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitMind_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitMind_API.Data
{
    public static class TrainerSeeder
    {
        public static async Task SeedTrainersAsync(FMDBContext context)
        {
            // 1. Ensure standard categories exist
            var standardCategories = new List<Categories>
            {
                new Categories { Name = "Weight Loss & Fat Loss", Slug = "weight-loss-fat-loss", Description = "Fat reduction, metabolic conditioning, and calorie deficit protocols.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Categories { Name = "Fitness & Bodybuilding", Slug = "fitness-bodybuilding", Description = "Muscle hypertrophy, physique development, and progressive overload training.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Categories { Name = "Sports Conditioning", Slug = "sports-conditioning", Description = "Athletic agility, explosive speed, power, and sport-specific performance.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Categories { Name = "Clinical Nutrition & Diet", Slug = "clinical-nutrition-diet", Description = "Macronutrient management, meal timing, and clinical diet strategies.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Categories { Name = "Powerlifting & Strength", Slug = "powerlifting-strength", Description = "Maximal strength development, squat/bench/deadlift form mastery.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Categories { Name = "Mobility & Injury Recovery", Slug = "mobility-injury-recovery", Description = "Joint health, corrective exercise, rehab, and active recovery.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Categories { Name = "Calisthenics & Bodyweight", Slug = "calisthenics-bodyweight", Description = "Gymnastic strength, muscle-ups, handstands, and bodyweight mastery.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Categories { Name = "Yoga & Mindfulness", Slug = "yoga-mindfulness", Description = "Flexibility, core stability, breathwork, and stress reduction.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Categories { Name = "Cardio & Endurance", Slug = "cardio-endurance", Description = "VO2 max training, marathon conditioning, and aerobic endurance.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Categories { Name = "General Health & Longevity", Slug = "general-health-longevity", Description = "Sustainable daily movement, posture, vitality, and longevity protocols.", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            foreach (var cat in standardCategories)
            {
                var existingCat = await context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == cat.Name.ToLower());
                if (existingCat == null)
                {
                    context.Categories.Add(cat);
                }
            }
            await context.SaveChangesAsync();

            var allCategories = await context.Categories.ToListAsync();

            // 2. Define the 10 Real Verified Trainers
            var trainerDefinitions = new List<(
                string Username,
                string Email,
                string CategoryName,
                int YearsOfExperience,
                string Certifications,
                string Bio,
                string Availability,
                string WhatsAppNumber,
                string Location,
                string Country,
                string SvgAvatarColor
            )>
            {
                (
                    "Marcus Vance",
                    "marcus.vance@fitjoin.com",
                    "Weight Loss & Fat Loss",
                    8,
                    "NSCA-CSCS, NASM-CPT, Precision Nutrition L1",
                    "Dedicated body transformation specialist focusing on sustainable calorie deficits, metabolic conditioning, and behavioral habit building for long-term fat loss.",
                    "Mon–Fri: 6:00 AM – 4:00 PM",
                    "+923001234501",
                    "Lahore",
                    "Pakistan",
                    "#87BF17"
                ),
                (
                    "Elena Rostova",
                    "elena.rostova@fitjoin.com",
                    "Fitness & Bodybuilding",
                    10,
                    "IFBB Pro Coach, ACE-CPT, Master Hypertrophy Specialist",
                    "Competitive physique coach focusing on biomechanically sound muscle hypertrophy, progressive overload tracking, and contest prep nutrition.",
                    "Mon–Sat: 7:00 AM – 7:00 PM",
                    "+923001234502",
                    "Karachi",
                    "Pakistan",
                    "#1E88E5"
                ),
                (
                    "Tariq Mahmood",
                    "tariq.mahmood@fitjoin.com",
                    "Sports Conditioning",
                    6,
                    "CSCS, EXOS Performance Specialist, USAW Level 2",
                    "High-performance sports conditioning coach helping athletes optimize sprint speed, vertical jump, rotational power, and in-season stamina.",
                    "Mon–Fri: 8:00 AM – 6:00 PM",
                    "+923001234503",
                    "Islamabad",
                    "Pakistan",
                    "#FB8C00"
                ),
                (
                    "Dr. Maya Patel",
                    "dr.maya.patel@fitjoin.com",
                    "Clinical Nutrition & Diet",
                    12,
                    "Registered Dietitian (RD), PhD Clinical Nutrition, ISSN-SNS",
                    "Clinical dietitian and sports nutritionist specializing in hormonal health, insulin sensitivity, gut health, and tailored macro calculations for high performers.",
                    "Mon–Thu: 9:00 AM – 5:00 PM",
                    "+923001234504",
                    "Lahore",
                    "Pakistan",
                    "#E91E63"
                ),
                (
                    "Liam O'Connor",
                    "liam.oconnor@fitjoin.com",
                    "Powerlifting & Strength",
                    7,
                    "USAPL Senior Coach, Starting Strength Coach (SSC)",
                    "Strength authority helping lifters break through plateaus in the Big 3 (Squat, Bench, Deadlift) with safe, heavy bar mechanics and autoregulated RPE cycles.",
                    "Mon–Fri: 10:00 AM – 8:00 PM",
                    "+923001234505",
                    "Rawalpindi",
                    "Pakistan",
                    "#6D4C41"
                ),
                (
                    "Sarah Jenkins",
                    "sarah.jenkins@fitjoin.com",
                    "Mobility & Injury Recovery",
                    9,
                    "Doctor of Physical Therapy (DPT), FMS Level 2, CSCS",
                    "Rehab and mobility specialist assisting individuals with post-injury recovery, joint decompression, thoracic spine mobility, and pain-free movement patterns.",
                    "Tue–Sat: 8:00 AM – 4:00 PM",
                    "+923001234506",
                    "Karachi",
                    "Pakistan",
                    "#00897B"
                ),
                (
                    "Lucas Silva",
                    "lucas.silva@fitjoin.com",
                    "Calisthenics & Bodyweight",
                    5,
                    "WSWCF Master Trainer, Gymnastic Strength Specialist",
                    "Calisthenics and gymnastic strength coach teaching strict muscle-ups, planches, human flags, and bodyweight strength from foundational to advanced levels.",
                    "Mon–Sat: 6:00 AM – 2:00 PM",
                    "+923001234507",
                    "Faisalabad",
                    "Pakistan",
                    "#3949AB"
                ),
                (
                    "Jessica Chen",
                    "jessica.chen@fitjoin.com",
                    "Yoga & Mindfulness",
                    11,
                    "E-RYT 500 Yoga Alliance, Mindfulness & Breathwork Coach",
                    "Vinyasa & Yin yoga instructor helping athletes integrate nervous system down-regulation, pelvic stability, mindful breathwork, and deep flexibility.",
                    "Mon–Sun: 6:00 AM – 12:00 PM",
                    "+923001234508",
                    "Islamabad",
                    "Pakistan",
                    "#8E24AA"
                ),
                (
                    "Alex Rivera",
                    "alex.rivera@fitjoin.com",
                    "Cardio & Endurance",
                    4,
                    "USATF Level 2 Endurance, Ironman Certified Coach",
                    "Endurance conditioning coach structuring interval zone training, lactate threshold development, and pacing strategies for 5K to ultra-marathon runners.",
                    "Mon–Fri: 5:30 AM – 3:30 PM",
                    "+923001234509",
                    "Lahore",
                    "Pakistan",
                    "#D81B60"
                ),
                (
                    "Amara Okafor",
                    "amara.okafor@fitjoin.com",
                    "General Health & Longevity",
                    14,
                    "ACSM Exercise Physiologist, Precision Nutrition Master Coach",
                    "Holistic longevity coach focusing on functional movement, bone mineral density, daily step volume, and lifestyle biohacking for lifelong vitality.",
                    "Mon–Fri: 9:00 AM – 6:00 PM",
                    "+923001234510",
                    "Karachi",
                    "Pakistan",
                    "#43A047"
                )
            };

            var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Trainer@12345");

            foreach (var def in trainerDefinitions)
            {
                var matchedCategory = allCategories.FirstOrDefault(c => c.Name.Equals(def.CategoryName, StringComparison.OrdinalIgnoreCase))
                                     ?? allCategories.First();

                var existingUser = await context.AppUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == def.Email.ToLower());

                // Generate clean SVG synthetic profile portrait image as byte array
                var avatarSvg = GenerateTrainerAvatarSvg(def.Username, def.SvgAvatarColor);
                var avatarBytes = System.Text.Encoding.UTF8.GetBytes(avatarSvg);

                if (existingUser == null)
                {
                    var cleanHandle = def.Username.Replace(" ", "").Replace(".", "").ToLower();
                    var newTrainer = new AppUsers
                    {
                        Username = def.Username,
                        UniqueName = $"trainer_{cleanHandle}",
                        Email = def.Email,
                        PasswordHash = defaultPasswordHash,
                        EmailConfirmed = true,
                        IsDeleted = false,
                        JoinedDate = DateTime.UtcNow.AddMonths(-new Random().Next(3, 24)),
                        Status = 2,
                        Role = "Trainer",
                        SpecializationCategoryId = matchedCategory.Id,
                        YearsOfExperience = def.YearsOfExperience,
                        Certifications = def.Certifications,
                        Bio = def.Bio,
                        Availability = def.Availability,
                        WhatsAppNumber = def.WhatsAppNumber,
                        Phone = def.WhatsAppNumber,
                        Location = def.Location,
                        Country = def.Country,
                        ProfilePhoto = avatarBytes
                    };

                    context.AppUsers.Add(newTrainer);
                }
                else
                {
                    // Ensure existing record has Trainer role & correct info
                    existingUser.Role = "Trainer";
                    existingUser.SpecializationCategoryId = matchedCategory.Id;
                    existingUser.YearsOfExperience = def.YearsOfExperience;
                    existingUser.Certifications = def.Certifications;
                    existingUser.Bio = def.Bio;
                    existingUser.Availability = def.Availability;
                    existingUser.WhatsAppNumber = def.WhatsAppNumber;
                    if (existingUser.ProfilePhoto == null || existingUser.ProfilePhoto.Length < 10)
                    {
                        existingUser.ProfilePhoto = avatarBytes;
                    }
                    context.AppUsers.Update(existingUser);
                }
            }

            await context.SaveChangesAsync();
        }

        private static string GenerateTrainerAvatarSvg(string name, string color)
        {
            var initials = string.Join("", name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(s => s[0])).ToUpper();
            return $@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 200 200"" width=""200"" height=""200"">
  <defs>
    <linearGradient id=""grad_{initials}"" x1=""0%"" y1=""0%"" x2=""100%"" y2=""100%"">
      <stop offset=""0%"" stop-color=""{color}"" />
      <stop offset=""100%"" stop-color=""#111827"" />
    </linearGradient>
  </defs>
  <rect width=""200"" height=""200"" rx=""100"" fill=""url(#grad_{initials})"" />
  <circle cx=""100"" cy=""75"" r=""35"" fill=""#ffffff"" opacity=""0.9"" />
  <path d=""M 45 165 C 45 125, 155 125, 155 165 Z"" fill=""#ffffff"" opacity=""0.9"" />
  <text x=""100"" y=""85"" font-family=""Inter, Arial, sans-serif"" font-size=""26"" font-weight=""bold"" fill=""#111827"" text-anchor=""middle"">{initials}</text>
</svg>";
        }
    }
}
