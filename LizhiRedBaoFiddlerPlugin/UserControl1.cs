using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LizhiRedBaoFiddlerPlugin
{
    public partial class UserControl1 : UserControl
    {


        public bool IsFeakRedPackage { get; set; }
        public BindingList<RedBaoItem> Items { get; } = new BindingList<RedBaoItem>();
        public DateTime ExpiredDatetime { get; private set; } = DateTime.MinValue;
        private int _totalAmount;

        public int MaxGoldPerRoom { get; private set; } = 9;

        public UserControl1()
        {
            InitializeComponent();
            _machineID.Text = VerificationConfig.GetCpuIdHashedString();


            //激活
            ExpiredDatetime = DateTime.MaxValue;
            _txtExpiredDate.Text = ExpiredDatetime.ToString("yyyy MMMM dd");

            MaxGoldPerRoom = decimal.ToInt32(_numericMaxRoom.Value);
            Class1.DelayTime = Convert.ToInt32(_numRoundDelay.Value);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Class1.Started = checkBox1.Checked;
        }

        public void SetRoomOwner(string url)
        {
            if (_picRoomOwner.IsDisposed || _picRoomOwner.Disposing)
                return;
            _picRoomOwner.BeginInvoke(new MethodInvoker(() =>
            {
                if (_picRoomOwner.IsDisposed || _picRoomOwner.Disposing)
                    return;
                _picRoomOwner.ImageLocation = url;
            }));
        }

        public void SetRoomConver(string url)
        {
            if (_picRoom.IsDisposed || _picRoom.Disposing)
                return;
            _picRoom.BeginInvoke(new MethodInvoker(() =>
            {
                if (_picRoom.IsDisposed || _picRoom.Disposing)
                    return;
                _picRoom.ImageLocation = url;
            }));
        }

        public void SetImage(string url)
        {
            if (pictureBox1.IsDisposed || pictureBox1.Disposing)
                return;
            pictureBox1.BeginInvoke(new MethodInvoker(() =>
            {
                if (pictureBox1.IsDisposed || pictureBox1.Disposing)
                    return;
                pictureBox1.ImageLocation = url;
            }));
        }

        public void IncreaseAmount(string amount)
        {
            int currentAmount;
            if (Int32.TryParse(amount, out currentAmount))
                _totalAmount += currentAmount;
            if (label6.IsDisposed || label6.Disposing)
                return;
            label6.BeginInvoke(new MethodInvoker(() => { label6.Text = _totalAmount.ToString(); }));
        }

        public void AddMessage(string message)
        {
            if (listBox1.IsDisposed || listBox1.Disposing)
                return;
            listBox1.BeginInvoke(new MethodInvoker(() =>
            {
                if (listBox1.IsDisposed || listBox1.Disposing)
                    return;
                listBox1.Items.Insert(0, DateTime.Now.ToString("T") + " " + message);
            }));
        }

        //
        //        public void SetCurrentDate(DateTime dateTime)
        //        {
        //            if (_txtExpiredDate.IsDisposed || _txtExpiredDate.Disposing)
        //                return;
        //            listBox1.BeginInvoke(new MethodInvoker(() => { _txtExpiredDate.Text = dateTime.ToString("yyyy MMMM dd"); }));
        //        }


        public void ChangeLoginStatus(bool isLogined)
        {
            if (_labLoginStatus.IsDisposed || _labLoginStatus.Disposing)
                return;
            _labLoginStatus.BeginInvoke(new MethodInvoker(() =>
            {
                if (_labLoginStatus.IsDisposed || _labLoginStatus.Disposing)
                    return;
                _labLoginStatus.Text = isLogined ? "已登录" : "未登录";
            }));
        }

        public void SetRoundCount(int totalCount)
        {
            if (_labCount.IsDisposed || _labCount.Disposing)
                return;
            _labCount.BeginInvoke(new MethodInvoker(() =>
            {
                if (_labCount.IsDisposed || _labCount.Disposing)
                    return;
                _labCount.Text = totalCount.ToString();
            }));
        }


        public void UpdateRoomCount(string roomCount)
        {
            if (label3.IsDisposed || label3.Disposing)
                return;
            label3.BeginInvoke(new MethodInvoker(() =>
            {
                if (label3.IsDisposed || label3.Disposing)
                    return;
                label3.Text = roomCount;
            }));
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0)
                return;
            try
            {
                Clipboard.SetText(listBox1.Items[listBox1.SelectedIndex].ToString());
            }
            catch
            {

            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {

            listBox1.Items.Clear();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton2.Checked)
                return;
            Class1.DelayTime = 1;
            _numRoundDelay.Value = Class1.DelayTime;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton1.Checked)
                return;
            Class1.DelayTime = 500;
            _numRoundDelay.Value = Class1.DelayTime;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton3.Checked)
                return;
            Class1.DelayTime = 1000;
            _numRoundDelay.Value = Class1.DelayTime;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton4.Checked)
                return;
            Class1.DelayTime = 2500;
            _numRoundDelay.Value = Class1.DelayTime;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton5.Checked)
                return;
            Class1.DelayTime = 5000;
            _numRoundDelay.Value = Class1.DelayTime;
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton6.Checked)
                return;
            Class1.DelayTime = 10000;
            _numRoundDelay.Value = Class1.DelayTime;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "HohoSama Configuration File|*.txt";
            saveFileDialog1.Title = "Save processing";
            saveFileDialog1.ShowDialog();
            try
            {
                List<string> temp = Class1._userIds.ToList();
                temp.AddRange(Class1._userEmptyRoomIds);
                if (!string.IsNullOrEmpty(saveFileDialog1.FileName))
                {
                    File.WriteAllText(saveFileDialog1.FileName,
                        String.Join(",", temp)
                    );
                }
            }
            catch (Exception ex)
            {
                AddMessage("Err:" + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string someItem;
            while (!Class1._userIds.IsEmpty)
            {
                Class1._userIds.TryTake(out someItem);
            }

            while (!Class1._userEmptyRoomIds.IsEmpty)
            {
                Class1._userEmptyRoomIds.TryTake(out someItem);
            }
            AddMessage("已经清空队列。");
            UpdateRoomCount("--");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "HohoSama Configuration File|*.txt";
            openFileDialog1.RestoreDirectory = true;

            openFileDialog1.ShowDialog();
            if (!string.IsNullOrEmpty(openFileDialog1.FileName))
            {
                try
                {
                    string content = File.ReadAllText(openFileDialog1.FileName);
                    content.Split(',').ToList().ForEach(item =>
                    {
                        if (!Class1._userEmptyRoomIds.Contains(item) && !Class1._userIds.Contains(item))
                            Class1._userEmptyRoomIds.Add(item);
                    });
                    //    UpdateRoomCount(Class1._userIds.Count.ToString());
                    AddMessage("导入完成.");
                }
                catch (Exception ex)
                {
                    AddMessage("Err:" + ex);
                }
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void _numericMaxRoom_ValueChanged(object sender, EventArgs e)
        {
            MaxGoldPerRoom = decimal.ToInt32(_numericMaxRoom.Value);
        }

        private void _txtVaild_TextChanged(object sender, EventArgs e)
        {
        
            return;



            _txtExpiredDate.Text = string.Empty;
            ExpiredDatetime = DateTime.MinValue;
            var decodedString = string.Empty;
            try
            {
                decodedString = DesUtil.Decrypt(_txtVaild.Text);
            }
            catch
            {
                //do nothing
            }
            if (string.IsNullOrEmpty(decodedString))
                return;
            try
            {
                if (VerificationConfig.GetSoftEndDateAllCpuId(1, decodedString) ==
                    VerificationConfig.GetCpuIdHashedString())
                {
                    //vaild cpuid
                    ExpiredDatetime = DateTime.ParseExact(VerificationConfig.GetSoftEndDateAllCpuId(0, decodedString), "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None);
                    _txtExpiredDate.Text = ExpiredDatetime.ToString("yyyy MMMM dd");
                }
            }
            catch
            {
                //do nothing
            }
        }

        public int GetDelayFetchGoldMillSecs()
        {
            return Convert.ToInt32(_numDelayMillSecs.Value);
        }

        private DataGridViewColumn AddColumnToDGridview(DataGridView gridview, string fieldName, string displayName, int colWidth = 0)
        {
            var retval = new DataGridViewTextBoxColumn
            {
                ReadOnly = true,
                DataPropertyName = fieldName,
                Name = fieldName,
                HeaderText = displayName
            };
            if (colWidth != 0)
                retval.Width = colWidth;
            gridview.Columns.Add(retval);
            return retval;
        }


        private void UserControl1_Load(object sender, EventArgs e)
        {
            _grid.BackgroundColor = BackColor;
            _grid.RowHeadersVisible = false;
            _grid.AllowUserToAddRows = false;
            _grid.AutoGenerateColumns = false;
            _grid.MultiSelect = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            AddColumnToDGridview(_grid, "RoomFmNumber", "房间FM编号");
            AddColumnToDGridview(_grid, "RoomName", "房间名称", 158);
            AddColumnToDGridview(_grid, "SenderName", "金币发送者", 158);
            AddColumnToDGridview(_grid, "RoomOwnerName", "房间拥有者", 158);
            AddColumnToDGridview(_grid, "FetchedGoldCount", "金币数量");
            AddColumnToDGridview(_grid, "Status", "当前状态");

            //            AddImageColumnToDGridview(_grid, "SenderPic", "发送者");
            //            AddImageColumnToDGridview(_grid, "RoomOwnerPic", "房主");
            //            AddImageColumnToDGridview(_grid, "RoomPic", "房间");
            AddColumnToDGridview(_grid, "UserId", "发送者ID");
            AddColumnToDGridview(_grid, "RoomId", "房间ID");
            AddColumnToDGridview(_grid, "DelayDateTime", "金币定时");
            AddColumnToDGridview(_grid, "Id", "ID");
            this._grid.Columns["ID"].SortMode =
                DataGridViewColumnSortMode.Automatic;
            _grid.DataSource = Items;
        }

        public void AddRedBaoItem(RedBaoItem item)
        {
            if (!_grid.IsDisposed && !_grid.Disposing)
            {
                _grid.BeginInvoke(new MethodInvoker(() =>
                {
                    Items.Insert(0, item);
                    _grid.FirstDisplayedScrollingRowIndex = 0;
                }));
            }
        }

        public void ProcessRedBaoItem(RedBaoItem item, int goldAmount, string status, string senderName)
        {
            if (!_grid.IsDisposed && !_grid.Disposing)
            {
                _grid.BeginInvoke(new MethodInvoker(() =>
                {
                    if (_grid.IsDisposed || _grid.Disposing)
                        return;
                    item.SenderName = senderName;
                    item.IsProcessed = true;
                    item.Status = status;
                    item.FetchedGoldCount = goldAmount;
                    _grid.FirstDisplayedScrollingRowIndex = 0;
                }));
            }
        }



        private readonly Regex _regexRoomSubTitleRegex = new Regex(@"<title>([^｜]+)｜荔枝FM</title>"); //<title>([^｜]+)｜荔枝FM</title>
        private readonly Regex _regexRoomTitleRegex = new Regex(@"<div class=""navbar-name"">([^<]+)</div>");//<div class="navbar-name">([^<]+)</div>
        private readonly Regex _regexRoomFMNumberRegex = new Regex(@"<div class=""navbar-user"">([^\s]+)[^<]+</div>");//<div class="navbar-user">([^\s]+)[^<]+</div>
        private readonly Regex _regexRoomUserImageRegex = new Regex(@"var userPortrait = ""([^""]+)"";");
        private readonly string _roomCommentsUserIdPara = @"https://appweb.lizhi.fm/live/comments?liveId={0}&start=0&count=9999";

        private void _grid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //            this._grid.Sort(this._grid.Columns[$"Id"], ListSortDirection.Descending);
        }

        public void SetProgressBar(int precent)
        {
            if (!progressBar1.IsDisposed && !progressBar1.Disposing)
                progressBar1.BeginInvoke(new MethodInvoker(() =>
                    {
                        if (!progressBar1.IsDisposed && !progressBar1.Disposing)
                            progressBar1.Value = precent;
                    }))
            ;

        }

        public void SetProgressBarSecond(int precent)
        {
            if (!progressBar2.IsDisposed && !progressBar2.Disposing)
                progressBar2.BeginInvoke(new MethodInvoker(() =>
                    {
                        if (!progressBar2.IsDisposed && !progressBar2.Disposing)
                            progressBar2.Value = precent;
                    }))
                    ;

        }

        private void _grid_SelectionChanged(object sender, EventArgs e)
        {
            SetRoomOwner(((RedBaoItem)_grid.CurrentRow.DataBoundItem).RoomOwnerPic);
            SetImage(((RedBaoItem)_grid.CurrentRow.DataBoundItem).SenderPic);
            SetRoomConver(((RedBaoItem)_grid.CurrentRow.DataBoundItem).RoomPic);
        }

        private void _numRoundDelay_ValueChanged(object sender, EventArgs e)
        {
            Class1.DelayTime = Convert.ToInt32(_numRoundDelay.Value);
        }

        private void _grid_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var item = (RedBaoItem)_grid.CurrentRow.DataBoundItem;
            Class1.RemoveUserid(item.RoomId);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "HohoSama Configuration File|*.lzlogintoken";
            saveFileDialog1.Title = "Save processing";
            saveFileDialog1.ShowDialog();
            try
            {
                if (!string.IsNullOrEmpty(saveFileDialog1.FileName))
                {
                    File.WriteAllText(saveFileDialog1.FileName,
                        String.Join(",", Class1._tokens)
                    );
                }
            }
            catch (Exception ex)
            {
                AddMessage("Err:" + ex.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "HohoSama Configuration File|*.lzlogintoken";
            openFileDialog1.RestoreDirectory = true;

            openFileDialog1.ShowDialog();
            if (!string.IsNullOrEmpty(openFileDialog1.FileName))
            {
                try
                {
                    string content = File.ReadAllText(openFileDialog1.FileName);
                    content.Split(',').ToList().ForEach(item =>
                    {
                        if (!Class1._tokens.Contains(item))
                            Class1._tokens.Add(item);
                    });
                    AddMessage("导入完成.");
                }
                catch (Exception ex)
                {
                    AddMessage("Err:" + ex);
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                IsFeakRedPackage = true;
            else
                IsFeakRedPackage = false;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
                IsLoadHotLiveRooms = true;
            else
                IsLoadHotLiveRooms = false;
        }

        public bool IsLoadHotLiveRooms { get; set; }
    }
}
