using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LizhiRedBaoFiddlerPlugin
{
    public static class GetHotListUsers
    {
        static Regex liveIdUrl = new Regex(@"""liveId"":""([0-9]+)""");
        static Regex userIdUrl = new Regex(@"var userId = ""([0-9]+)"";");


        public static string getUserId(string pageContent)
        {
            var match = userIdUrl.Match(pageContent);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }

        public static List<UserId> GetHotUsers(string pageContent)
        {
            var matches = liveIdUrl.Matches(pageContent);
            List<UserId> users = new List<UserId>();
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    users.Add(new UserId()
                    {
                        LiveId = match.Groups[1].Value
                    });
                }
            }
            return users;
        }
    }
}
