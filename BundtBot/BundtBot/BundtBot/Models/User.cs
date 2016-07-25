using System.Collections.Generic;
using System.Linq;
using BundtBot.BundtBot.Database;
using LiteDB;

namespace BundtBot.BundtBot.Models {
    public class User {
        [BsonId]
        public ulong SnowflakeId { get; set; }

        internal static User New(ulong snowflakeId) {
                var user = new User {
                    SnowflakeId = snowflakeId
                };
                user = DB.Users.FindById(DB.Users.Insert(user));
                return user;
        }

        internal static bool Exists(ulong snowflakeId) {
            return DB.Users.Exists(x => x.SnowflakeId == snowflakeId);
        }

        internal static List<User> All() {
            return DB.Users.FindAll().ToList();
        }

        internal void Save() {
            DB.Users.Update(this);
        }
    }
}
