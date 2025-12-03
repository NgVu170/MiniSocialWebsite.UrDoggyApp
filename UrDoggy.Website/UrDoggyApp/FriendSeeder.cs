using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrDoggy.Core.Models;
using UrDoggy.Data;
namespace UrDoggy.Website
{
    public class FriendSeeder
    {
        public static async Task SeedFriends(IServiceProvider serviceProvider)
        {
            var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ✅ Load all users
            var users = await db.Users.ToListAsync();

            int U(string username) => users.First(u => u.UserName == username).Id;

            var friendships = new List<Friend>
        {
            // ✅ Minh <-> Lan
            new Friend { UserId = U("minh.nguyen"), FriendId = U("lan.tran"), Status = "Accepted" },
            new Friend { UserId = U("lan.tran"), FriendId = U("minh.nguyen"), Status = "Accepted" },

            // ✅ Minh <-> Tuấn
            new Friend { UserId = U("minh.nguyen"), FriendId = U("tuan.pham"), Status = "Accepted" },
            new Friend { UserId = U("tuan.pham"), FriendId = U("minh.nguyen"), Status = "Accepted" },

            // ✅ Minh <-> Hà
            new Friend { UserId = U("minh.nguyen"), FriendId = U("ha.le"), Status = "Accepted" },
            new Friend { UserId = U("ha.le"), FriendId = U("minh.nguyen"), Status = "Accepted" },

            // ✅ Việt <-> Anh
            new Friend { UserId = U("viet.hoang"), FriendId = U("anh.do"), Status = "Accepted" },
            new Friend { UserId = U("anh.do"), FriendId = U("viet.hoang"), Status = "Accepted" },

            // ✅ Khánh <-> Dao
            new Friend { UserId = U("khanh.vo"), FriendId = U("dao.bui"), Status = "Accepted" },
            new Friend { UserId = U("dao.bui"), FriendId = U("khanh.vo"), Status = "Accepted" },

            // ✅ Nam <-> Trang
            new Friend { UserId = U("nam.lu"), FriendId = U("trang.phuong"), Status = "Accepted" },
            new Friend { UserId = U("trang.phuong"), FriendId = U("nam.lu"), Status = "Accepted" },

            // ✅ Đạt <-> Linh
            new Friend { UserId = U("dat.ngo"), FriendId = U("linh.bui"), Status = "Accepted" },
            new Friend { UserId = U("linh.bui"), FriendId = U("dat.ngo"), Status = "Accepted" },

            // ✅ Phúc <-> Thảo
            new Friend { UserId = U("phuc.vo"), FriendId = U("thao.nguyen"), Status = "Accepted" },
            new Friend { UserId = U("thao.nguyen"), FriendId = U("phuc.vo"), Status = "Accepted" },

            // ✅ Sơn <-> Hoa
            new Friend { UserId = U("son.tran"), FriendId = U("hoa.le"), Status = "Accepted" },
            new Friend { UserId = U("hoa.le"), FriendId = U("son.tran"), Status = "Accepted" },

            // ✅ Long <-> Yến
            new Friend { UserId = U("long.pham"), FriendId = U("yen.truong"), Status = "Accepted" },
            new Friend { UserId = U("yen.truong"), FriendId = U("long.pham"), Status = "Accepted" },

            // ✅ Pending requests (1-way only)
            new Friend { UserId = U("hung.do"), FriendId = U("minh.nguyen"), Status = "Pending" },
            new Friend { UserId = U("mai.nguyen"), FriendId = U("lan.tran"), Status = "Pending" },

            // ✅ New social connections
            new Friend { UserId = U("viet.hoang"), FriendId = U("nam.lu"), Status = "Accepted" },
            new Friend { UserId = U("nam.lu"), FriendId = U("viet.hoang"), Status = "Accepted" },

            new Friend { UserId = U("anh.do"), FriendId = U("linh.bui"), Status = "Accepted" },
            new Friend { UserId = U("linh.bui"), FriendId = U("anh.do"), Status = "Accepted" },
        };

            foreach (var f in friendships)
            {
                bool exists = await db.Friends.AnyAsync(x =>
                    x.UserId == f.UserId &&
                    x.FriendId == f.FriendId);

                if (!exists)
                {
                    db.Friends.Add(f);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
