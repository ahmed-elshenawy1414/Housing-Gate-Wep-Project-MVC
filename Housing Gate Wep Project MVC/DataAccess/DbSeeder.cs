using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentHousing.Models;

namespace StudentHousing.Data
{
    /// <summary>
    /// Seeds roles, an admin account and realistic demo data so the platform
    /// can be explored immediately after the database is created.
    ///
    /// Demo logins (change passwords in production):
    ///   Admin  -> admin@housinggate.com / Admin@123
    ///   Owner  -> karim.owner@example.com / Owner@123
    ///   Student-> sara.ahmed@example.com   / Student@123
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await EnsureRolesAsync(roleManager);
            await EnsureAdminAsync(userManager);
            await EnsureDemoDataAsync(db, userManager);
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in new[] { "Admin", "Owner", "Student" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task EnsureAdminAsync(UserManager<ApplicationUser> userManager)
        {
            const string email = "admin@housinggate.com";
            if (await userManager.FindByEmailAsync(email) != null)
            {
                return;
            }

            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = "Housing",
                LastName = "Admin",
                IsActive = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        private static async Task EnsureDemoDataAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            await EnsureLookupsAsync(db);

            if (await db.Properties.AnyAsync())
            {
                return;
            }

            // ---------- Owners ----------
            var karim = await CreateUserAsync(userManager, "karim.owner@example.com", "Karim", "Nabil", "Owner@123", "Owner");
            var mona = await CreateUserAsync(userManager, "mona.owner@example.com", "Mona", "El-Sayed", "Owner@123", "Owner");

            var karimProfile = new OwnerProfile
            {
                UserId = karim.Id,
                CompanyName = "Karim Properties",
                Bio = "Reliable landlord managing apartments around Cairo University.",
                Phone = "0100 000 0001",
                LicenseNumber = "LIC-2024-001",
                VerificationStatus = VerificationStatus.Verified,
                VerifiedAt = DateTime.UtcNow.AddMonths(-3)
            };
            var monaProfile = new OwnerProfile
            {
                UserId = mona.Id,
                CompanyName = "Mona Rentals",
                Bio = "Friendly owner renting shared homes across Giza.",
                Phone = "0100 000 0002",
                LicenseNumber = "LIC-2024-002",
                VerificationStatus = VerificationStatus.Verified,
                VerifiedAt = DateTime.UtcNow.AddMonths(-2)
            };
            db.OwnerProfiles.AddRange(karimProfile, monaProfile);
            await db.SaveChangesAsync();

            // ---------- Students ----------
            var sara = await CreateStudentAsync(db, userManager, "sara.ahmed@example.com", "Sara", "Ahmed",
                "Cairo University", "Computer Science", Gender.Female, 3000, 4500,
                isSmoker: false, SleepSchedule.NightOwl, NoiseLevel.Moderate, Cleanliness.Casual, verified: true);
            var layla = await CreateStudentAsync(db, userManager, "layla.hassan@example.com", "Layla", "Hassan",
                "Cairo University", "Medicine", Gender.Female, 3500, 5000,
                isSmoker: false, SleepSchedule.NightOwl, NoiseLevel.Moderate, Cleanliness.Tidy, verified: true);
            var nour = await CreateStudentAsync(db, userManager, "nour.adel@example.com", "Nour", "Adel",
                "Ain Shams University", "Pharmacy", Gender.Female, 3000, 4500,
                isSmoker: false, SleepSchedule.Flexible, NoiseLevel.Quiet, Cleanliness.Casual, verified: true);
            var hana = await CreateStudentAsync(db, userManager, "hana.mostafa@example.com", "Hana", "Mostafa",
                "Cairo University", "Fine Arts", Gender.Female, 2800, 4200,
                isSmoker: false, SleepSchedule.NightOwl, NoiseLevel.Loud, Cleanliness.Casual, verified: false);
            var omarr = await CreateStudentAsync(db, userManager, "omar.farouk@example.com", "Omar", "Farouk",
                "Cairo University", "Engineering", Gender.Male, 2500, 4000,
                isSmoker: false, SleepSchedule.EarlyBird, NoiseLevel.Quiet, Cleanliness.Tidy, verified: true);
            var youssef = await CreateStudentAsync(db, userManager, "youssef.sami@example.com", "Youssef", "Sami",
                "Alexandria University", "Business", Gender.Male, 3000, 6000,
                isSmoker: false, SleepSchedule.Flexible, NoiseLevel.Moderate, Cleanliness.Tidy, verified: true);
            await db.SaveChangesAsync();

            // Preferences for the matching engine.
            await EnsurePreferenceAsync(db, sara, Gender.Female, 3000, 5000, SleepSchedule.NightOwl, NoiseLevel.Moderate, Cleanliness.Casual, "Looking for quiet, non-smoking flatmates near campus.");
            await EnsurePreferenceAsync(db, layla, Gender.Female, 3000, 5000, SleepSchedule.NightOwl, NoiseLevel.Moderate, Cleanliness.Tidy, null);
            await EnsurePreferenceAsync(db, nour, Gender.Female, 3000, 5000, SleepSchedule.Flexible, NoiseLevel.Quiet, Cleanliness.Casual, "Prefers early sleepers, love a tidy shared kitchen.");
            await EnsurePreferenceAsync(db, hana, null, 2500, 5000, SleepSchedule.NightOwl, NoiseLevel.Moderate, Cleanliness.Casual, null);
            await EnsurePreferenceAsync(db, omarr, null, 2500, 4500, SleepSchedule.EarlyBird, NoiseLevel.Quiet, Cleanliness.Tidy, "Early riser, study-focused.");
            await EnsurePreferenceAsync(db, youssef, null, 2500, 6000, SleepSchedule.Flexible, NoiseLevel.Moderate, Cleanliness.Tidy, null);

            // ---------- Properties ----------
            var greenFlat = new Property
            {
                OwnerId = karim.Id,
                Title = "Green Garden Shared Flat",
                Description = "A bright shared flat a 5-minute walk from Cairo University. Includes a fully equipped kitchen, fast internet and weekly cleaning.",
                PropertyType = PropertyType.SharedHouse,
                Address = "12 Gameat El Dewal St.",
                City = "Giza",
                State = "Giza",
                ZipCode = "12613",
                Deposit = 2000,
                Bedrooms = 3,
                Bathrooms = 2,
                IsFurnished = true,
                PetAllowed = true,
                AvailableFrom = DateTime.UtcNow,
                ApprovalStatus = ApprovalStatus.Approved,
                IsActive = true,
                IsFeatured = true
            };
            var zamalekApartment = new Property
            {
                OwnerId = mona.Id,
                Title = "Zamalek Riverside Apartment",
                Description = "Peaceful 2-bedroom apartment overlooking the Nile, ideal for two students. Quiet building with 24h security.",
                PropertyType = PropertyType.Apartment,
                Address = "8 Shagaret El Dor St.",
                City = "Cairo",
                State = "Cairo",
                ZipCode = "11211",
                Deposit = 3000,
                Bedrooms = 2,
                Bathrooms = 2,
                IsFurnished = true,
                PetAllowed = false,
                AvailableFrom = DateTime.UtcNow,
                ApprovalStatus = ApprovalStatus.Approved,
                IsActive = true,
                IsFeatured = true
            };
            var downtownStudio = new Property
            {
                OwnerId = karim.Id,
                Title = "Downtown Sunny Studio",
                Description = "Compact, fully furnished studio in the heart of Downtown. Great for a single student who values location.",
                PropertyType = PropertyType.Studio,
                Address = "22 Talaat Harb St.",
                City = "Cairo",
                State = "Cairo",
                ZipCode = "11511",
                Deposit = 1500,
                Bedrooms = 1,
                Bathrooms = 1,
                IsFurnished = true,
                PetAllowed = false,
                AvailableFrom = DateTime.UtcNow.AddMonths(1),
                ApprovalStatus = ApprovalStatus.Approved,
                IsActive = true
            };
            var gizaParkHouse = new Property
            {
                OwnerId = mona.Id,
                Title = "Giza Park Shared House",
                Description = "Large shared house with a garden near Giza Park. Four rooms, each with its own key and lock.",
                PropertyType = PropertyType.SharedHouse,
                Address = "5 Wadi El Nile St.",
                City = "Giza",
                State = "Giza",
                ZipCode = "12311",
                Deposit = 2500,
                Bedrooms = 4,
                Bathrooms = 2,
                IsFurnished = false,
                PetAllowed = true,
                AvailableFrom = DateTime.UtcNow,
                ApprovalStatus = ApprovalStatus.Approved,
                IsActive = true
            };
            var pendingFlat = new Property
            {
                OwnerId = karim.Id,
                Title = "Nasr City Family Flat (pending review)",
                Description = "Spacious family flat being prepared for student leasing.",
                PropertyType = PropertyType.Apartment,
                Address = "9 Abbas El Akkad St.",
                City = "Cairo",
                State = "Cairo",
                Deposit = 2000,
                Bedrooms = 3,
                Bathrooms = 2,
                ApprovalStatus = ApprovalStatus.Pending,
                IsActive = true
            };

            db.Properties.AddRange(greenFlat, zamalekApartment, downtownStudio, gizaParkHouse, pendingFlat);
            await db.SaveChangesAsync();

            // Rooms inside each property.
            var g1 = AddRoom(greenFlat, "Bedroom 1", RoomType.Single, 3200, true);
            var g2 = AddRoom(greenFlat, "Bedroom 2", RoomType.Single, 2900, true);
            var g3 = AddRoom(greenFlat, "Shared Bedroom", RoomType.Shared, 2100, true);
            var z1 = AddRoom(zamalekApartment, "Master Bedroom", RoomType.Master, 4500, true);
            var z2 = AddRoom(zamalekApartment, "Second Bedroom", RoomType.Single, 3800, true);
            var d1 = AddRoom(downtownStudio, "Studio", RoomType.Single, 3500, true);
            var p1 = AddRoom(gizaParkHouse, "Room A", RoomType.Single, 2600, true);
            var p2 = AddRoom(gizaParkHouse, "Room B", RoomType.Single, 2400, true);
            var p3 = AddRoom(gizaParkHouse, "Room C (shared)", RoomType.Shared, 1800, true);
            var p4 = AddRoom(gizaParkHouse, "Room D (shared)", RoomType.Shared, 1800, true);
            await db.SaveChangesAsync();

            // Placeholder images.
            AddImage(greenFlat, "/images/property-green-garden.svg", true);
            AddImage(greenFlat, "/images/property-green-garden-2.svg", false);
            AddImage(zamalekApartment, "/images/property-zamalek.svg", true);
            AddImage(downtownStudio, "/images/property-downtown.svg", true);
            AddImage(gizaParkHouse, "/images/property-giza-park.svg", true);
            await db.SaveChangesAsync();

            // ---------- Verified past stays + reviews ----------
            // Sara & Layla shared the Green Garden flat (completed stay).
            var app1 = new PropertyApplication
            {
                RoomId = g1.Id,
                StudentProfileId = sara.Id,
                Message = "Interested in moving in!",
                Status = ApplicationStatus.Approved,
                AppliedAt = DateTime.UtcNow.AddMonths(-7),
                RespondedAt = DateTime.UtcNow.AddMonths(-7)
            };
            var app2 = new PropertyApplication
            {
                RoomId = g2.Id,
                StudentProfileId = layla.Id,
                Message = "Love the flat!",
                Status = ApplicationStatus.Approved,
                AppliedAt = DateTime.UtcNow.AddMonths(-7),
                RespondedAt = DateTime.UtcNow.AddMonths(-7)
            };
            db.PropertyApplications.AddRange(app1, app2);
            await db.SaveChangesAsync();

            var staySara = new Stay
            {
                RoomId = g1.Id,
                StudentProfileId = sara.Id,
                ApplicationId = app1.Id,
                Status = StayStatus.Completed,
                StartDate = DateTime.UtcNow.AddMonths(-6),
                EndDate = DateTime.UtcNow.AddMonths(-1)
            };
            var stayLayla = new Stay
            {
                RoomId = g2.Id,
                StudentProfileId = layla.Id,
                ApplicationId = app2.Id,
                Status = StayStatus.Completed,
                StartDate = DateTime.UtcNow.AddMonths(-6),
                EndDate = DateTime.UtcNow.AddMonths(-1)
            };
            db.Stays.AddRange(staySara, stayLayla);
            await db.SaveChangesAsync();

            db.PropertyReviews.AddRange(
                new PropertyReview
                {
                    PropertyId = greenFlat.Id,
                    StayId = staySara.Id,
                    ReviewerId = sara.UserId,
                    Rating = 4,
                    Title = "Great location, solid value",
                    Comment = "Two minutes from university and the landlord is responsive. Kitchen is a bit small but everything else is great.",
                    Status = ReviewStatus.Approved,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new PropertyReview
                {
                    PropertyId = greenFlat.Id,
                    StayId = stayLayla.Id,
                    ReviewerId = layla.UserId,
                    Rating = 5,
                    Title = "Quiet and clean",
                    Comment = "Very peaceful building, internet is fast, and the weekly cleaner keeps everything spotless.",
                    Status = ReviewStatus.Approved,
                    CreatedAt = DateTime.UtcNow.AddDays(-18)
                });

            // Roommate reviews after the shared stay.
            db.UserReviews.AddRange(
                new UserReview
                {
                    StayId = staySara.Id,
                    ReviewerId = sara.UserId,
                    ReviewedUserId = layla.UserId,
                    Rating = 5,
                    Comment = "Layla is super tidy and respectful. Easy to live with.",
                    Status = ReviewStatus.Approved,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new UserReview
                {
                    StayId = stayLayla.Id,
                    ReviewerId = layla.UserId,
                    ReviewedUserId = sara.UserId,
                    Rating = 4,
                    Comment = "Sara is friendly and keeps common areas clean. Would definitely live with her again.",
                    Status = ReviewStatus.Approved,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                });

            sara.VerifiedStayCount = 1;
            layla.VerifiedStayCount = 1;

            await db.SaveChangesAsync();
        }

        private static async Task EnsureLookupsAsync(ApplicationDbContext db)
        {
            var amenityNames = new[]
            {
                ("Wi-Fi", "واي فاي"),
                ("Air Conditioning", "تكييف"),
                ("Washing Machine", "غسالة"),
                ("Refrigerator", "ثلاجة"),
                ("Stove", "موقد"),
                ("Water Heater", "سخان مياه"),
                ("TV", "تلفاز"),
                ("Study Desk", "مكتب دراسة"),
                ("Wardrobe", "دولاب ملابس"),
                ("Bed", "سرير"),
                ("Private Bathroom", "حمام خاص"),
                ("Shared Bathroom", "حمام مشترك"),
                ("Elevator", "مصعد"),
                ("Parking", "موقف سيارات"),
                ("Security", "أمن"),
                ("24-hour Water", "مياه على مدار الساعة"),
                ("Electricity", "كهرباء"),
                ("Gas", "غاز")
            };

            if (!await db.Amenities.AnyAsync())
            {
                db.Amenities.AddRange(amenityNames.Select(a => new Amenity { Name = a.Item1, NameAr = a.Item2 }));
            }
            else
            {
                var existing = await db.Amenities.ToListAsync();
                foreach (var (name, nameAr) in amenityNames)
                {
                    var row = existing.FirstOrDefault(a => a.Name == name);
                    if (row != null && string.IsNullOrEmpty(row.NameAr))
                    {
                        row.NameAr = nameAr;
                    }
                }
            }

            var universityNames = new[]
            {
                ("Cairo University", "جامعة القاهرة"),
                ("Ain Shams University", "جامعة عين شمس"),
                ("Alexandria University", "جامعة الإسكندرية"),
                ("Helwan University", "جامعة حلوان"),
                ("Mansoura University", "جامعة المنصورة"),
                ("Zagazig University", "جامعة الزقازيق"),
                ("Assiut University", "جامعة أسيوط"),
                ("Aswan University", "جامعة أسوان"),
                ("Tanta University", "جامعة طنطا"),
                ("American University in Cairo", "الجامعة الأمريكية بالقاهرة"),
                ("German University in Cairo", "الجامعة الألمانية بالقاهرة"),
                ("Future University in Egypt", "جامعة المستقبل في مصر"),
                ("Misr International University", "جامعة مصر الدولية"),
                ("Suez Canal University", "جامعة قناة السويس"),
                ("Beni Suef University", "جامعة بني سويف"),
                ("Minya University", "جامعة المنيا")
            };

            if (!await db.Universities.AnyAsync())
            {
                var universityCities = new[]
                {
                    ("Cairo University", "Giza"), ("Ain Shams University", "Cairo"),
                    ("Alexandria University", "Alexandria"), ("Helwan University", "Cairo"),
                    ("Mansoura University", "Mansoura"), ("Zagazig University", "Zagazig"),
                    ("Assiut University", "Assiut"), ("Aswan University", "Aswan"),
                    ("Tanta University", "Tanta"), ("American University in Cairo", "New Cairo"),
                    ("German University in Cairo", "New Cairo"), ("Future University in Egypt", "New Cairo"),
                    ("Misr International University", "Cairo"), ("Suez Canal University", "Ismailia"),
                    ("Beni Suef University", "Beni Suef"), ("Minya University", "Minya")
                };
                db.Universities.AddRange(universityNames.Select(u =>
                    new University { Name = u.Item1, NameAr = u.Item2, City = universityCities.First(c => c.Item1 == u.Item1).Item2 }));
            }
            else
            {
                var existing = await db.Universities.ToListAsync();
                foreach (var (name, nameAr) in universityNames)
                {
                    var row = existing.FirstOrDefault(u => u.Name == name);
                    if (row != null && string.IsNullOrEmpty(row.NameAr))
                    {
                        row.NameAr = nameAr;
                    }
                }
            }

            await db.SaveChangesAsync();
        }

        private static Room AddRoom(Property property, string name, RoomType type, int rent, bool available)
        {
            var beds = type == RoomType.Shared ? 2 : 1;
            var room = new Room
            {
                PropertyId = property.Id,
                Name = name,
                RoomType = type,
                RentPerMonth = rent,
                NumberOfBeds = beds,
                AvailableBeds = beds,
                IsAvailable = available
            };
            property.Rooms.Add(room);
            return room;
        }

        private static void AddImage(Property property, string path, bool isPrimary)
        {
            property.Images.Add(new PropertyImage
            {
                PropertyId = property.Id,
                FilePath = path,
                Category = PropertyImageCategory.Exterior,
                IsPrimary = isPrimary
            });
        }

        private static async Task<ApplicationUser> CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            string email, string firstName, string lastName, string password, string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                return existing;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }

            return user;
        }

        private static async Task<StudentProfile> CreateStudentAsync(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            string email, string firstName, string lastName,
            string university, string major, Gender gender,
            int budgetMin, int budgetMax,
            bool isSmoker, SleepSchedule sleep, NoiseLevel noise, Cleanliness clean,
            bool verified)
        {
            var user = await CreateUserAsync(userManager, email, firstName, lastName, "Student@123", "Student");

            var profile = new StudentProfile
            {
                UserId = user.Id,
                University = university,
                Major = major,
                Gender = gender,
                Bio = $"{firstName} is a {major} student at {university} looking for a friendly shared home.",
                BudgetMin = budgetMin,
                BudgetMax = budgetMax,
                MoveInDate = DateTime.UtcNow.AddMonths(1),
                MinLeaseMonths = 6,
                IsSmoker = isSmoker,
                SleepSchedule = sleep,
                NoiseLevel = noise,
                Cleanliness = clean,
                HasPets = false,
                VerificationStatus = verified ? VerificationStatus.Verified : VerificationStatus.Pending,
                VerifiedAt = verified ? DateTime.UtcNow.AddMonths(-4) : null
            };

            db.StudentProfiles.Add(profile);
            return profile;
        }

        private static async Task EnsurePreferenceAsync(
            ApplicationDbContext db, StudentProfile student,
            Gender? preferredGender, int minBudget, int maxBudget,
            SleepSchedule sleep, NoiseLevel noise, Cleanliness clean, string? notes)
        {
            if (student.Preference != null)
            {
                return;
            }

            var preference = new RoommatePreference
            {
                StudentProfileId = student.Id,
                PreferredGender = preferredGender,
                MinBudget = minBudget,
                MaxBudget = maxBudget,
                AcceptsSmokers = false,
                PreferredSleepSchedule = sleep,
                PreferredNoiseLevel = noise,
                PreferredCleanliness = clean,
                AcceptsPets = true,
                Notes = notes
            };
            db.RoommatePreferences.Add(preference);
            await db.SaveChangesAsync();
        }
    }
}
