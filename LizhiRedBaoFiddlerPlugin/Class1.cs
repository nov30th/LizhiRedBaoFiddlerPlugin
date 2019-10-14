using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Fiddler;

[assembly: Fiddler.RequiredVersion("2.3.5.0")]
[assembly: AssemblyTitle("HOHOSama的荔枝FM红包机器")]

namespace LizhiRedBaoFiddlerPlugin
{
    public class Class1 : IAutoTamper
    {
        public static bool Started { get; set; } = false;

        public int AutoIncreaseId { get; set; }

        public static bool Logined { get; set; } = false;

        // public static string _receiverLiveUserId = "";
        public static List<string> _tokens { get; set; } = new List<string>();

        public const int _pageNum = 4;
        public static ConcurrentDictionary<string, string> _TokenToDeviceId = new ConcurrentDictionary<string, string>();

        public static ConcurrentDictionary<string, string> _TokenToAccessToken = new ConcurrentDictionary<string, string>();

        public static ConcurrentDictionary<string, string> _UserIdToLiveId = new ConcurrentDictionary<string, string>();
        public static ConcurrentBag<string> _userIds { get; set; } = new ConcurrentBag<string>();
        public static ConcurrentBag<string> _userEmptyRoomIds { get; set; } = new ConcurrentBag<string>();
        public List<RedBaoItem> _waittingForProcessingItems = new List<RedBaoItem>();

        public static Thread WorkingThreadStart { get; set; }
        public static Thread WorkingThreadStart2 { get; set; }
        public static Thread WorkingThreadStart3 { get; set; }

        public static int DelayTime { get; set; } = 250;

        public static int RountCount { get; set; } = 0;


        private DateTime _currentDateTime = DateTime.MinValue;
        private TabPage tabPage; //创建插件的选项卡页
        private UserControl1 myCtrl; //MyControl自定义控件

        private Regex receiverLiveUserIdRegex = new Regex("receiverLiveUserId=([0-9]+)");
        private Regex tokenRegex = new Regex("token=([0-9a-z]+)");
        private Regex liveIdRegex = new Regex("liveId=([0-9a-z]+)");

        //GET /redenvelope/getCandidateLiveRedEnvelop?receiverLiveUserId=2609036106640706092&token=0364896208b5541f323afa9436bcfb3f&r=1498990953959 HTTP/1.1

        public void OnLoad()
        {
            //do nothing
            this.tabPage = new TabPage("荔枝红包FM by HohoSama V0.01"); //选项卡的名字为Test
            this.myCtrl = new UserControl1();

            //将用户控件添加到选项卡中
            this.tabPage.Controls.Add(this.myCtrl);
            //为选项卡添加icon图标，这里使用Fiddler 自带的
            this.tabPage.ImageIndex = (int)Fiddler.SessionIcons.Timeline;
            //将tabTage选项卡添加到Fidder UI的Tab 页集合中
            FiddlerApplication.UI.tabsViews.TabPages.Add(this.tabPage);

//            WorkingThreadStart = new Thread(RunRedBao);
//            WorkingThreadStart.Name = "LiZhi FM Fiddler Plugin WorkingThreadStart By HohoSama";
//            WorkingThreadStart.Start();
//
//            WorkingThreadStart2 = new Thread(RunFetchingRedBao);
//            WorkingThreadStart2.Name = "LiZhi FM Fiddler Plugin WorkingThreadStart2 By HohoSama";
//            WorkingThreadStart2.Start();

            WorkingThreadStart3 = new Thread(RunRedBaoEmptyRoom);
            WorkingThreadStart3.Name = "LiZhi FM Fiddler Plugin WorkingThreadStart3 By HohoSama";
            WorkingThreadStart3.Start();
        }


        public string GetRequest(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                request.Headers["Origin"] = "https://sfestival.lizhi.fm";
                request.Referer =
                    "https://sfestival.lizhi.fm/static/gift_progress.html?userId=-1&isSFestivalOpen=false&isRedEnvelopeOpen=true&redEnvelopeHost=https%3A%2F%2Fredenvelope.lizhi.fm&isValentineDayOpen=false&isHidePendant=true&isFirstChargeWidgetShow=true&isBraveOpen=false&coinLotteryOpen=false"
                    ;
                request.Headers["X-Requested-With"] = "com.android.browser";
                request.UserAgent = "Mozilla/5.0 (Linux; Android 4.4.4; 7 plus Build/KTU84P) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/33.0.0.0 Safari/537.36";
                //接受返回来的数据
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                        {
                            string retString = streamReader.ReadToEnd();
                            return retString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                myCtrl.AddMessage("Err: " + ex.Message);
                return null;
            }
        }


        public string PostRequest(string url, string data)
        {
            //data: id=2610833767616494139&token=0364896208b5541f323afa9436bcfb3f
            //url: https://redenvelope.lizhi.fm/redenvelope/grab?token=0364896208b5541f323afa9436bcfb3f 
            try
            {
                //string response;
                //using (WebClient client = new WebClient())
                //{

                //    response =
                //        client.UploadString(url, data);
                //    return resp
                //    //string result = System.Text.Encoding.UTF8.GetString(response);
                //}
                byte[] payload = Encoding.UTF8.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentLength = payload.Length;
                request.Accept = "text/javascript, text/html, application/xml, text/xml, */*";
                request.ContentType = "application/x-www-form-urlencoded";
                //request.Headers["Origin"] = "https://redenvelope.lizhi.fm";
                request.UserAgent =
"Mozilla/5.0 (Linux; Android 4.4.4; 7 plus Build/KTU84P) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/33.0.0.0 Safari/537.36 LizhiFM/4.0.0_build115326 NetType/WIFI Language/zh";
                request.Referer =
                    "https://redenvelope.lizhi.fm/static/chai_hong_bao.html?redEnvelopeHost=https%3A%2F%2Fredenvelope.lizhi.fm&" + data
                    ;
                request.Headers["X-Requested-With"] = "com.yibasan.lizhifm";
                request.ServicePoint.Expect100Continue = true;
                //request.ContentType = "application/json;charset=UTF-8";


                //发送post的请求
                Stream writer = request.GetRequestStream();
                writer.Write(payload, 0, payload.Length);
                writer.Close();


                //接受返回来的数据
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                        {
                            string retString = streamReader.ReadToEnd();
                            return retString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                myCtrl.AddMessage("Err: " + ex.Message);
                return null;
            }
        }

        // swaps array elements i and j
        public static void exch(List<string> list, int i, int j)
        {

            String swap = list[i];
            list[i] = list[j];
            list[j] = swap;
        }


        // take as input an array of strings and rearrange them in random order
        public static List<string> shuffle(List<string> list)
        {
            int N = list.Count;
            for (int i = 0; i < N; i++)
            {
                int r = (new Random().Next(0, N)); // between i and N-1
                exch(list, i, r);
            }
            return list;
        }

        /// <summary>
        /// Remove userid in fetching list
        /// </summary>
        /// <param name="userid"></param>
        public static void RemoveUserid(string userid)
        {
            var tempList = _userIds.ToList();
            while (!_userIds.IsEmpty)
            {
                string someItem;
                _userIds.TryTake(out someItem);
            }
            tempList.Remove(userid);
            tempList.ForEach(item => _userIds.Add(item));
        }

        public void RunFetchingRedBao()
        {
            while (true)
            {
                Thread.Sleep(100);
                if (_waittingForProcessingItems.Count == 0)
                    continue;
                _waittingForProcessingItems.ToList().ForEach(itemDoing =>
                {
                    if (DateTime.Now > itemDoing.DelayDateTime)
                    {
                        try
                        {
                            int amount = 0;
                            string senderName = string.Empty;

                            //乱序接收人
                            List<string> randomTokens = shuffle(_tokens.ToList());



                            randomTokens.ForEach(token =>
                            {
                                int perAmount = 0;
                                if (!_TokenToDeviceId.ContainsKey(token))
                                {
                                    myCtrl.AddMessage("One of the users doesn't fill the deviceId successful!!!");
                                    myCtrl.AddMessage("Missing token of deviceId: " + token);
                                    return;
                                }


                                //var hongbaoHtml =
                                //    $@"https://redenvelope.lizhi.fm/static/chai_hong_bao.html?id={
                                //            itemDoing.FieldResultID
                                //        }&redEnvelopeHost=https%3A%2F%2Fredenvelope.lizhi.fm&token={token}&deviceId={
                                //            _TokenToDeviceId[token]
                                //        }&accessToken={_TokenToAccessToken[token]}&liveId={
                                //            _UserIdToLiveId[itemDoing.UserId]
                                //        }";

                                //var hongbaoHtmlResult = GetRequest(hongbaoHtml);


                                var postResult = PostRequest(
                                    $"https://redenvelope.lizhi.fm/redenvelope/grab?token={token}",
                                    $"id={itemDoing.FieldResultID}&token={token}&liveId={_UserIdToLiveId[itemDoing.UserId]}&deviceId={_TokenToDeviceId[token]}&"
                                // $"id={itemDoing.FieldResultID}&token={token}&liveId={_UserIdToLiveId[itemDoing.UserId]}&deviceId={_TokenToDeviceId[token]}&accessToken={_TokenToAccessToken[token]}"
                                );
                                if (GetMoneyAmount(postResult, ref senderName, out perAmount) == 0)
                                {

                                    //清空BAG，发生概率低忽略效率
                                    var tempList = _userIds.ToList();
                                    while (!_userIds.IsEmpty)
                                    {
                                        string someItem;
                                        _userIds.TryTake(out someItem);
                                    }
                                    tempList.Remove(itemDoing.UserId);
                                    tempList.ForEach(item => _userIds.Add(item));
                                }
                                amount += perAmount;
                            });
                            if (amount > 0)
                                myCtrl.ProcessRedBaoItem(itemDoing, amount, "完成", senderName);
                            else
                                myCtrl.ProcessRedBaoItem(itemDoing, amount, "失败", senderName);

                            lock (_waittingForProcessingItems)
                                _waittingForProcessingItems.Remove(itemDoing);
                        }
                        catch (Exception ex)
                        {
                            myCtrl.ProcessRedBaoItem(itemDoing, 0, $"错误:{ex.Message}", "Unknown");

                            lock (_waittingForProcessingItems)
                                _waittingForProcessingItems.Remove(itemDoing);
                        }
                    }
                });
            }
        }

        public void RunRedBaoEmptyRoom()
        {
            while (true)
            {
                myCtrl.UpdateRoomCount((_userIds.Count + _userEmptyRoomIds.Count).ToString());
                RountCount++;
                if (!Started)
                {
                    Thread.Sleep(2000);
                    continue;
                }

                //获得热门用户

                List<UserId> hotList = new List<UserId>();
                if (myCtrl.IsLoadHotLiveRooms)
                    GetHotUsersList(hotList);


                if (_userEmptyRoomIds.Count != 0 && _tokens.Count > 0)
                {
                    if (!Logined)
                    {
                        Logined = true;
                        myCtrl.ChangeLoginStatus(Logined);
                    }

                    int totalCount = _userEmptyRoomIds.Count;
                    int currentCount = 0;

                    var pendingAddUsers = new List<string>();

                    _userEmptyRoomIds.ToList().ForEach((userid =>
                    {
                        currentCount++;
                        //                        _tokens.ToList().ForEach(_token =>
                        //                        {

                        if (_waittingForProcessingItems.ToList().Any(item => item.UserId == userid))
                            return;
                        if (!_UserIdToLiveId.ContainsKey(userid) || !_TokenToDeviceId.ContainsKey(_tokens[0]))
                            return;
                        string getResult =
                            GetRequest(
                                $@"https://redenvelope.lizhi.fm/redenvelope/getCandidateLiveRedEnvelop?receiverLiveUserId={
                                        userid
                                    }&token={_tokens[0]}&liveId={_UserIdToLiveId[userid]}&deviceId={_TokenToDeviceId[_tokens[0]]}&r={new Random().NextDouble()}");
                        //      }&token={_tokens[0]}&liveId={_UserIdToLiveId[userid]}&deviceId={ _TokenToDeviceId[_tokens[0]]}&accessToken={_TokenToAccessToken[_tokens[0]]}&r={new Random().NextDouble()}");

                        var redbaoItem = new RedBaoItem();

                        myCtrl.SetProgressBarSecond(currentCount * 100 / totalCount);

                        if (!string.IsNullOrEmpty(getResult) && !getResult.Contains("无红包可抢") &&
                            getResult.Contains(@"""id"":"))
                        {
                            //add to quick fetch process
                            pendingAddUsers.Add(userid);

                            string roomInfoResponse =
                                GetRequest(
                                    $@"http://www.lizhi.fm/user/{userid}");
                            Regex roomUserPhotoAndRoomNameRegex =
                                new Regex(
                                    @"<div class=""user-info-img"">[^<]+<img src=""([^""]+)"" alt=""([^""]+)"" />");
                            Regex roomPicRegex =
                                new Regex(
                                    @"data-cover=""([^""]+)""");
                            //@" < div class=""radioCover left"">[^<]+<img alt=""[^""]+"" src=""([^""]+)"" />");
                            Regex roomFMAndRoomTitleRegex =
                                new Regex("<h1 class=\"user-info-name\">([^ ]+)([^<]+)<i class=");
                            //new Regex(@"div class=""left""><a href=""/"">发现</a><a href=""/([^""]+)/"">([^<]+)</a>");
                            string roomFmId = "未获取";
                            string roomFmName = "未获取";
                            string roomOwner = "未获取";
                            if (!string.IsNullOrEmpty(roomInfoResponse))
                            {
                                var roomUserPhotoAndRoomName = roomUserPhotoAndRoomNameRegex.Match(roomInfoResponse);
                                if (roomUserPhotoAndRoomName.Success)
                                {
                                    //房主图片
                                    redbaoItem.RoomOwnerPic = (roomUserPhotoAndRoomName.Groups[1].Value);
                                    myCtrl.SetRoomOwner(roomUserPhotoAndRoomName.Groups[1].Value);
                                    //房主名字
                                    roomOwner = (roomUserPhotoAndRoomName.Groups[2].Value);
                                }
                                var roomPic = roomPicRegex.Match(roomInfoResponse);
                                if (roomPic.Success)
                                {
                                    //房间图片
                                    redbaoItem.RoomPic = (roomPic.Groups[1].Value);
                                    myCtrl.SetRoomConver(roomPic.Groups[1].Value);
                                }
                                var roomFMAndRoomTitle = roomFMAndRoomTitleRegex.Match(roomInfoResponse);
                                if (roomFMAndRoomTitle.Success)
                                {
                                    //FM ID
                                    roomFmId = roomFMAndRoomTitle.Groups[1].Value;
                                    //房间名字
                                    roomFmName = roomFMAndRoomTitle.Groups[2].Value;
                                }
                            }

                            Regex regex = new Regex(@"\{""id"":""([0-9]+)""");
                            string resultID = regex.Match(getResult).Groups[1].Value;
                            redbaoItem.FieldResultID = resultID;

//                            if (!string.IsNullOrEmpty(resultID))
//                            {
//                                if (!_userIds.Contains(resultID) && !_userEmptyRoomIds.Contains(resultID))
//                                {
//                                    _userEmptyRoomIds.Add(resultID);
//                                    myCtrl.AddMessage($@"Added new userID: {resultID}");
//                                }
//                            }

                            var senderIdMatch = new Regex(@"""senderId"":""([^""]+)""").Match(getResult);
                            if (senderIdMatch.Success)
                            {
                                redbaoItem.RoomFmNumber = roomFmId;
                                redbaoItem.RoomName = roomFmName;
                                redbaoItem.RoomOwnerName = roomOwner;
                                redbaoItem.UserId = userid;
                                redbaoItem.RoomId = userid;
                                redbaoItem.SenderId = senderIdMatch.Groups[1].Value;
                                redbaoItem.Status = "等待抢...";
                                redbaoItem.IsProcessed = false;
                                redbaoItem.Id = AutoIncreaseId++;
                                //myCtrl.AddMessage($"^^^红包,房间FM:{roomFmId} 房间名:{roomFmName} 房主:{roomOwner}");
                                //myCtrl.AddMessage($"^^^红包,发送者ID:{senderIdMatch.Groups[1].Value}, 房间ID:{userid}.^^^");
                            }


                            var senderPhotoMatch = new Regex(@"""senderCover"":""([^""]+)""").Match(getResult);
                            if (senderPhotoMatch.Success)
                            {
                                //发送者图片
                                redbaoItem.SenderPic = (senderPhotoMatch.Groups[1].Value);
                                myCtrl.SetImage(senderPhotoMatch.Groups[1].Value);
                            }

                            redbaoItem.DelayDateTime = DateTime.Now.AddMilliseconds(myCtrl.GetDelayFetchGoldMillSecs());








//
//
//
//                            lock (_waittingForProcessingItems)
//                                _waittingForProcessingItems.Add(redbaoItem);










                            myCtrl.AddRedBaoItem(redbaoItem);


                            /**************************************/
                            List<string> randomTokens = shuffle(_tokens.ToList());
                            int amount = 0;
                            string senderName = string.Empty;


                            randomTokens.ForEach(token =>
                            {
                                int perAmount = 0;
                                if (!_TokenToDeviceId.ContainsKey(token))
                                {
                                    myCtrl.AddMessage("One of the users doesn't fill the deviceId successful!!!");
                                    myCtrl.AddMessage("Missing token of deviceId: " + token);
                                    return;
                                }
                                var postResult = PostRequest(
                                    $"https://redenvelope.lizhi.fm/redenvelope/grab?token={token}",
                                    $"id={redbaoItem.FieldResultID}&token={token}&liveId={_UserIdToLiveId[redbaoItem.UserId]}&deviceId={_TokenToDeviceId[token]}&"
                                // $"id={itemDoing.FieldResultID}&token={token}&liveId={_UserIdToLiveId[itemDoing.UserId]}&deviceId={_TokenToDeviceId[token]}&accessToken={_TokenToAccessToken[token]}"
                                );
                                if (GetMoneyAmount(postResult, ref senderName, out perAmount) == 0)
                                {

                                    //清空BAG，发生概率低忽略效率
                                    var tempList = _userIds.ToList();
                                    while (!_userIds.IsEmpty)
                                    {
                                        string someItem;
                                        _userIds.TryTake(out someItem);
                                    }
                                    tempList.Remove(redbaoItem.UserId);
                                    tempList.ForEach(item => _userIds.Add(item));
                                }
                                amount += perAmount;

                            });
                            if (amount > 0)
                                myCtrl.ProcessRedBaoItem(redbaoItem, amount, "完成", senderName);
                            else
                                myCtrl.ProcessRedBaoItem(redbaoItem, amount, "失败", senderName);

//                            lock (_waittingForProcessingItems)
//                                _waittingForProcessingItems.Remove(itemDoing);

                        }


                    }));







//
//                    // Thread.Sleep(10000);
//                    //Doing quick process transfer
//                    if (pendingAddUsers.Count > 0)
//                    {
//                        pendingAddUsers.ForEach(userid =>
//                        {
//                            if (!_userIds.Contains(userid))
//                                _userIds.Add(userid);
//
//                        });
//                        var tempEmptyUserIds = _userEmptyRoomIds.ToList();
//
//                        //ignore prefermance...just code like this.
//                        while (!_userEmptyRoomIds.IsEmpty)
//                        {
//                            string someItem;
//                            _userEmptyRoomIds.TryTake(out someItem);
//                        }
//
//                        tempEmptyUserIds.ForEach(item =>
//                        {
//                            if (!pendingAddUsers.Contains(item))
//                            {
//                                _userEmptyRoomIds.Add(item);
//                            }
//                        });
//
//                        pendingAddUsers.Clear();
//                    }
//






                    Thread.Sleep(DelayTime);
                }
                else
                {
                    if (Logined)
                    {
                        Logined = false;
                        myCtrl.ChangeLoginStatus(Logined);
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        public void GetHotUsersList(List<UserId> hotList)
        {
            //自动获得热门进入
            const string hotpage = "https://appweb.lizhi.fm/smallApp/getLiveList?pageNum=";
            const string userPage =
                "https://appweb.lizhi.fm/live/share?liveId=LIVEID&from=androidOtherBrowser";


            myCtrl.AddMessage("Loading HotList...");
            Enumerable.Range(0, _pageNum).ToList().ForEach(pageId =>
            {
                string hotpageWithId = hotpage + pageId;
                var content = GetRequest(hotpageWithId);
                if (string.IsNullOrEmpty(content))
                {
                    myCtrl.AddMessage("Err: Hot list error!");
                    return;
                }
                hotList.AddRange(GetHotListUsers.GetHotUsers(content));
            });
            myCtrl.AddMessage("Loading LiveID...");
            hotList.ToList().ForEach(hotLiveId =>
            {
                if (_UserIdToLiveId.Values.Contains(hotLiveId.LiveId))
                {
                    hotList.Remove(hotLiveId);
                }
            });

            myCtrl.AddMessage("Loading UserID...");
            hotList.ForEach(hotUserId =>
            {
                var pageContent = GetRequest(userPage.Replace("LIVEID", hotUserId.LiveId));

                hotUserId.Userid = GetHotListUsers.getUserId(pageContent);
                if (string.IsNullOrEmpty(hotUserId.Userid))
                    return;

                if (_UserIdToLiveId.ContainsKey(hotUserId.Userid))
                    return;

                _UserIdToLiveId.TryAdd(hotUserId.Userid, hotUserId.LiveId);

                _userEmptyRoomIds.Add(hotUserId.Userid);
            });
        }

        public void RunRedBao()
        {
            while (true)
            {
                //myCtrl.UpdateRoomCount(_userIds.Count.ToString());
                RountCount++;
                if (!Started)
                {
                    Thread.Sleep(2000);
                    continue;
                }
                if (_userIds.Count != 0 && _tokens.Count > 0)
                {
                    if (!Logined)
                    {
                        Logined = true;
                        myCtrl.ChangeLoginStatus(Logined);
                    }

                    int totalCount = _userIds.Count;
                    int currentCount = 0;

                    _userIds.ToList().ForEach((userid =>
                    {
                        currentCount++;
                        //                        _tokens.ToList().ForEach(_token =>
                        //                        {

                        if (_waittingForProcessingItems.ToList().Any(item => item.UserId == userid))
                            return;


                        string tempLiveId = string.Empty;
                        if (!_UserIdToLiveId.ContainsKey(userid))
                            return;
                        string getResult =
                            GetRequest(
                                $@"https://redenvelope.lizhi.fm/redenvelope/getCandidateLiveRedEnvelop?receiverLiveUserId={
                                        userid
                                    }&token={_tokens[0]}&liveId={_UserIdToLiveId[userid]}&deviceId={_TokenToDeviceId[_tokens[0]]}&accessToken={_TokenToAccessToken[_tokens[0]]}&r={new Random().NextDouble()}");
                        //}&token={_tokens[0]}&liveId={_UserIdToLiveId[userid]}&deviceId={_TokenToDeviceId[_tokens[0]]}&r={new Random().NextDouble()}");

                        var redbaoItem = new RedBaoItem();

                        myCtrl.SetProgressBar(currentCount * 100 / totalCount);

                        if (!string.IsNullOrEmpty(getResult) && !getResult.Contains("无红包可抢") &&
                            getResult.Contains(@"""id"":"))
                        {
                            string roomInfoResponse =
                                GetRequest(
                                    $@"http://www.lizhi.fm/user/{userid}");
                            Regex roomUserPhotoAndRoomNameRegex =
                                new Regex(
                                    @"<div class=""user-info-img"">[^<]+<img src=""([^""]+)"" alt=""([^""]+)"" />");
                            Regex roomPicRegex =
                                new Regex(
                                    @"data-cover=""([^""]+)""");
                            //@" < div class=""radioCover left"">[^<]+<img alt=""[^""]+"" src=""([^""]+)"" />");
                            Regex roomFMAndRoomTitleRegex =
                                new Regex("<h1 class=\"user-info-name\">([^ ]+)([^<]+)<i class=");
                            //new Regex(@"div class=""left""><a href=""/"">发现</a><a href=""/([^""]+)/"">([^<]+)</a>");
                            string roomFmId = "未获取";
                            string roomFmName = "未获取";
                            string roomOwner = "未获取";
                            if (!string.IsNullOrEmpty(roomInfoResponse))
                            {
                                var roomUserPhotoAndRoomName = roomUserPhotoAndRoomNameRegex.Match(roomInfoResponse);
                                if (roomUserPhotoAndRoomName.Success)
                                {
                                    //房主图片
                                    redbaoItem.RoomOwnerPic = (roomUserPhotoAndRoomName.Groups[1].Value);
                                    myCtrl.SetRoomOwner(roomUserPhotoAndRoomName.Groups[1].Value);
                                    //房主名字
                                    roomOwner = (roomUserPhotoAndRoomName.Groups[2].Value);
                                }
                                var roomPic = roomPicRegex.Match(roomInfoResponse);
                                if (roomPic.Success)
                                {
                                    //房间图片
                                    redbaoItem.RoomPic = (roomPic.Groups[1].Value);
                                    myCtrl.SetRoomConver(roomPic.Groups[1].Value);
                                }
                                var roomFMAndRoomTitle = roomFMAndRoomTitleRegex.Match(roomInfoResponse);
                                if (roomFMAndRoomTitle.Success)
                                {
                                    //FM ID
                                    roomFmId = roomFMAndRoomTitle.Groups[1].Value;
                                    //房间名字
                                    roomFmName = roomFMAndRoomTitle.Groups[2].Value;
                                }
                            }

                            Regex regex = new Regex(@"\{""id"":""([0-9]+)""");
                            string resultID = regex.Match(getResult).Groups[1].Value;
                            redbaoItem.FieldResultID = resultID;

                            if (!string.IsNullOrEmpty(resultID))
                            {
                                if (!_userIds.Contains(resultID))
                                {
                                    _userIds.Add(resultID);
                                    myCtrl.AddMessage($@"Added new userID: {resultID}");
                                }
                            }

                            var senderIdMatch = new Regex(@"""senderId"":""([^""]+)""").Match(getResult);
                            if (senderIdMatch.Success)
                            {
                                redbaoItem.RoomFmNumber = roomFmId;
                                redbaoItem.RoomName = roomFmName;
                                redbaoItem.RoomOwnerName = roomOwner;
                                redbaoItem.UserId = userid;
                                redbaoItem.RoomId = userid;
                                redbaoItem.SenderId = senderIdMatch.Groups[1].Value;
                                redbaoItem.Status = "等待抢...";
                                redbaoItem.IsProcessed = false;
                                redbaoItem.Id = AutoIncreaseId++;
                                //myCtrl.AddMessage($"^^^红包,房间FM:{roomFmId} 房间名:{roomFmName} 房主:{roomOwner}");
                                //myCtrl.AddMessage($"^^^红包,发送者ID:{senderIdMatch.Groups[1].Value}, 房间ID:{userid}.^^^");
                            }


                            var senderPhotoMatch = new Regex(@"""senderCover"":""([^""]+)""").Match(getResult);
                            if (senderPhotoMatch.Success)
                            {
                                //发送者图片
                                redbaoItem.SenderPic = (senderPhotoMatch.Groups[1].Value);
                                myCtrl.SetImage(senderPhotoMatch.Groups[1].Value);
                            }

                            redbaoItem.DelayDateTime = DateTime.Now.AddMilliseconds(myCtrl.GetDelayFetchGoldMillSecs());

                            lock (_waittingForProcessingItems)
                                _waittingForProcessingItems.Add(redbaoItem);
                            myCtrl.AddRedBaoItem(redbaoItem);
                        }

                        //                        });
                    }));
                }
                else
                {
                    if (Logined)
                    {
                        Logined = false;
                        myCtrl.ChangeLoginStatus(Logined);
                    }
                }
                Thread.Sleep(DelayTime);
            }
        }

        public int GetMoneyAmount(string content, ref string senderName, out int moneyAmount)
        {
            moneyAmount = 0;
            try
            {
                if (!string.IsNullOrEmpty(content))
                {
                    if (content.Contains(@"""money"":"))
                    {
                        Regex regex = new Regex(@"""money"":([0-9]+)");
                        string resultMoney = regex.Match(content).Groups[1].Value;
                        Regex senderNameRegex = new Regex(@"""senderName"":""([^""]+)""");


                        //int beginPosition = content.IndexOf(@"""money"":", StringComparison.Ordinal) +
                        //                    @"""money"":".Length;
                        //string money = content.Substring(beginPosition,
                        //    content.IndexOf("}", beginPosition + 1, StringComparison.Ordinal) - beginPosition - 1);
                        int money = 0;
                        if (!int.TryParse(resultMoney, out money))
                            return -4; //金币数量错误
                        moneyAmount = money;
                        myCtrl.AddMessage("获得金币:" + resultMoney);
                        myCtrl.IncreaseAmount(resultMoney);
                        var senderNameMatch = senderNameRegex.Match(content);
                        if (senderNameMatch.Success)
                            senderName = senderNameMatch.Groups[1].Value;
                        senderName = Regex.Unescape(senderName);
                        //myCtrl.AddMessage("金币发送者:" + senderNameMatch.Groups[1].Value);
                        if (money > myCtrl.MaxGoldPerRoom)
                        {
                            //Exit this room
                            myCtrl.AddMessage("红包获得数量超过指定数量，退出该房间");
                            return 0;
                        }


                        return 1;
                    }
                    else
                    {
                        return -1; // "无法获得数据.无字段";
                    }
                }
                else
                {
                    return -2; //"无法获得数据.空返回";
                }
            }
            catch (Exception ex)
            {
                myCtrl.AddMessage("Err:" + ex.Message);
                return -3; //"无法获得数据.异常";
            }
        }

        public void OnBeforeUnload()
        {
            WorkingThreadStart?.Abort();
            WorkingThreadStart2?.Abort();
            WorkingThreadStart3?.Abort();
            //do nothing
        }

        public void AutoTamperRequestBefore(Session oSession)
        {
            if (oSession.HTTPMethodIs("CONNECT"))
            {
                if (oSession.ResponseHeaders.Exists("Connection"))
                {
                    oSession.ResponseHeaders.Remove("Connection");
                    oSession.oResponse.headers["Connection"] = "Keep-Alive";
                }
            }

            if (oSession.uriContains("https://redenvelope.lizhi.fm/redenvelope/getCandidateLiveRedEnvelop"))
            {
                if (!oSession.clientIP.Replace("::ffff:", string.Empty).StartsWith("192.168")
                    && !oSession.clientIP.Replace("::ffff:", string.Empty).StartsWith("172.")
                    && !oSession.clientIP.Replace("::ffff:", string.Empty).StartsWith("10.")
                    && !oSession.clientIP.Replace("::ffff:", string.Empty).StartsWith("127.")
                )
                {
                    myCtrl.AddMessage("Ignore connection... Err:01");
                    return;
                }

                if (myCtrl == null)
                    return;

                if (myCtrl.ExpiredDatetime <= _currentDateTime)
                {
                    myCtrl.AddMessage("Ignore connection... Err:02");
                    return;
                }

                //process the token and liveuserid
                var token = tokenRegex.Match(oSession.fullUrl);
                var userid = receiverLiveUserIdRegex.Match(oSession.fullUrl);
                var liveId = liveIdRegex.Match(oSession.fullUrl);
                if (token.Groups.Count < 2 || token.Groups.Count < 2 || liveId.Groups.Count < 2)
                {
                    myCtrl.AddMessage("Warn: got wrong info of token..");
                    return;
                }
                if (_tokens.ToList().All(item => item != token.Groups[1].Value))
                {
                    _tokens.Add(token.Groups[1].Value);
                }
                if (!_userIds.Contains(userid.Groups[1].Value) && !_userEmptyRoomIds.Contains(userid.Groups[1].Value))
                //  if (!_userIds.Contains(userid.Groups[1].Value))
                {
                    myCtrl.UpdateRoomCount(_userIds.Count.ToString());
                    _userEmptyRoomIds.Add(userid.Groups[1].Value);
                    _UserIdToLiveId.AddOrUpdate(userid.Groups[1].Value, liveId.Groups[1].Value, (oldkey, oldvalue) => liveId.Groups[1].Value);
                }
            }

            if (oSession.uriContains("https://redenvelope.lizhi.fm/redenvelope/grab"))
            {
                Regex tokenRegex = new Regex("&token=([a-z0-9]+)");
                Regex deviceIdRegex = new Regex("&deviceId=([^&]+)");
                Regex accessTokenRegex = new Regex("&accessToken=([A-Z0-9]+)");
                var tokenMatch = tokenRegex.Match(oSession.GetRequestBodyAsString());
                var deviceIdMatch = deviceIdRegex.Match(oSession.GetRequestBodyAsString());
                var accessTokenMatch = accessTokenRegex.Match(oSession.GetRequestBodyAsString());
                if (tokenMatch.Success && deviceIdMatch.Success && accessTokenMatch.Success)
                {
                    //所有成功match
                    string deviceId = string.Empty;
                    if (_TokenToDeviceId.ContainsKey(tokenMatch.Groups[1].Value) && _TokenToDeviceId.TryGetValue(tokenMatch.Groups[1].Value, out deviceId) && deviceId == deviceIdMatch.Groups[1].Value)
                        return;
                    _TokenToDeviceId.AddOrUpdate(tokenMatch.Groups[1].Value, deviceIdMatch.Groups[1].Value, (s, s1) => deviceIdMatch.Groups[1].Value);
                    _TokenToAccessToken.AddOrUpdate(tokenMatch.Groups[1].Value, accessTokenMatch.Groups[1].Value, (s, s1) => accessTokenMatch.Groups[1].Value);
                    myCtrl.AddMessage("Device info is added successful!");
                }
            }


        }

        public void AutoTamperRequestAfter(Session oSession)
        {
            //do nothing
        }

        public void AutoTamperResponseBefore(Session oSession)
        {
            if (oSession.uriContains("https://redenvelope.lizhi.fm/redenvelope/getCandidateLiveRedEnvelop"))
            {
                if (myCtrl == null)
                    return;
                if (myCtrl.ExpiredDatetime.CompareTo(DateTime.MinValue) <= 0)
                {
                    if (oSession.ResponseHeaders.Exists("Date"))
                    {
                        _currentDateTime = DateTime.Parse(oSession.ResponseHeaders["Date"]);
                        //                        myCtrl.SetCurrentDate(_currentDateTime);
                    }
                }
                if (myCtrl.IsFeakRedPackage)
                    oSession.ResponseBody = System.Text.Encoding.Default.GetBytes("{\"rCode\":0,\"data\":{\"id\":\"123\",\"senderId\":\"123\",\"senderName\":\"123\",\"senderCover\":\"https://cdn.lizhi.fm/user/2017/1/1/123.jpg\"},\"msg\":\"success\"}");
            }
            //do nothing
        }

        public void AutoTamperResponseAfter(Session oSession)
        {
            //do nothing
        }

        public void OnBeforeReturningError(Session oSession)
        {
            //do nothing
        }

    }
}