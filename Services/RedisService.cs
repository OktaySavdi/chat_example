using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.Services
{
    public class RedisService
    {
        private const string CONNECTIONS_KEY = "chat_connections_list";
        IDatabase db;
        public RedisService(IConfiguration configuration)
        {
            IConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(configuration["Redis"]);
            db = multiplexer.GetDatabase();
        }

        ///<summary>
        ///Clears current active connections cache
        /// </summary>
        public void ClearConnectionList()
        {
            db.KeyDelete(CONNECTIONS_KEY);
        }

        ///<summary>
        ///Adds specific Connection Id to active connections list cache
        /// </summary>
        public async Task AddConnectionId(string connectionId)
        {
            await db.SetAddAsync(CONNECTIONS_KEY, connectionId);
        }

        ///<summary>
        ///Removes Connection Id from active connections list cache
        /// </summary>
        public async Task RemoveConnectionId(string connectionId)
        {
            await db.SetRemoveAsync(CONNECTIONS_KEY, connectionId);
        }

        ///<summary>
        ///Lists current active Connection Ids
        /// </summary>
        public async Task<List<string>> GetConnectionIdList()
        {
            RedisValue[] connectionList = await db.SetMembersAsync(CONNECTIONS_KEY);
            return connectionList.Select(c => c.ToString()).ToList();
        }



    }
}
