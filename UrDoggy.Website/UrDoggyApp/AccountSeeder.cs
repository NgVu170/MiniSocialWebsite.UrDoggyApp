using Microsoft.AspNetCore.Identity;
using UrDoggy.Core.Models;

namespace UrDoggy.Website
{
    public class AccountSeeder
    {
        public static async Task SeedUsers(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            string defaultPassword = "admin123"; // ✅ CHANGE IF YOU WANT

            var users = new List<User>
        {
            new User { UserName = "minh.nguyen", Email = "minh.nguyen@gmail.com", DisplayName = "Nguyễn Minh", Bio = "Yêu công nghệ và nhiếp ảnh", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "lan.tran", Email = "lan.tran@gmail.com", DisplayName = "Trần Ngọc Lan", Bio = "Thích du lịch và đọc sách", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "tuan.pham", Email = "tuan.pham@gmail.com", DisplayName = "Phạm Quốc Tuấn", Bio = "Quản trị hệ thống", IsAdmin = true, EmailConfirmed = true },
            new User { UserName = "ha.le", Email = "ha.le@gmail.com", DisplayName = "Lê Thu Hà", Bio = "Người yêu chó", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "viet.hoang", Email = "viet.hoang@gmail.com", DisplayName = "Hoàng Quốc Việt", Bio = "Lập trình web", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "anh.do", Email = "anh.do@gmail.com", DisplayName = "Đỗ Ngọc Anh", Bio = "Thiết kế UI/UX", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "khanh.vo", Email = "khanh.vo@gmail.com", DisplayName = "Võ Minh Khánh", Bio = "Đam mê thể thao", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "dao.bui", Email = "dao.bui@gmail.com", DisplayName = "Bùi Quỳnh Dao", Bio = "Sinh viên CNTT", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "nam.lu", Email = "nam.lu@gmail.com", DisplayName = "Lư Hoàng Nam", Bio = "Dev C#", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "trang.phuong", Email = "trang.phuong@gmail.com", DisplayName = "Phương Thảo Trang", Bio = "Yêu thú cưng", IsAdmin = false, EmailConfirmed = true },

            new User { UserName = "dat.ngo", Email = "dat.ngo@gmail.com", DisplayName = "Ngô Thành Đạt", Bio = "Backend developer", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "linh.bui", Email = "linh.bui@gmail.com", DisplayName = "Bùi Thùy Linh", Bio = "Content Creator", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "phuc.vo", Email = "phuc.vo@gmail.com", DisplayName = "Võ Minh Phúc", Bio = "Admin hệ thống", IsAdmin = true, EmailConfirmed = true },
            new User { UserName = "thao.nguyen", Email = "thao.nguyen@gmail.com", DisplayName = "Nguyễn Kim Thảo", Bio = "Designer", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "son.tran", Email = "son.tran@gmail.com", DisplayName = "Trần Minh Sơn", Bio = "Freelancer", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "hoa.le", Email = "hoa.le@gmail.com", DisplayName = "Lê Mỹ Hoa", Bio = "Chăm sóc thú cưng", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "long.pham", Email = "long.pham@gmail.com", DisplayName = "Phạm Thành Long", Bio = "IT Support", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "yen.truong", Email = "yen.truong@gmail.com", DisplayName = "Trương Thanh Yến", Bio = "Sinh viên năm cuối", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "hung.do", Email = "hung.do@gmail.com", DisplayName = "Đỗ Văn Hùng", Bio = "AI Engineer", IsAdmin = false, EmailConfirmed = true },
            new User { UserName = "mai.nguyen", Email = "mai.nguyen@gmail.com", DisplayName = "Nguyễn Thị Mai", Bio = "UI/UX Designer", IsAdmin = false, EmailConfirmed = true }
        };

            foreach (var user in users)
            {
                var existingUser = await userManager.FindByEmailAsync(user.Email);

                if (existingUser == null)
                {
                    var result = await userManager.CreateAsync(user, defaultPassword);

                    if (!result.Succeeded)
                    {
                        throw new Exception("User creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }
    }
}
