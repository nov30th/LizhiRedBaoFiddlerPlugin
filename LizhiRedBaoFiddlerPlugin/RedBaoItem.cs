using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LizhiRedBaoFiddlerPlugin.Annotations;

namespace LizhiRedBaoFiddlerPlugin
{
    public class RedBaoItem : INotifyPropertyChanged, IComparer<RedBaoItem>
    {
        private bool isProcessed;
        public bool IsProcessed
        {
            get => isProcessed;
            set { isProcessed = value; OnPropertyChanged(); }
        }

        public string FieldResultID { get; set; }
        public int Id { get; set; }
        public DateTime DelayDateTime { get; set; }
        public string UserId { get; set; }
        public string SenderId { get; set; }
        /// <summary>
        /// RoomId same as UserId
        /// </summary>
        public string RoomId { get; set; }
        public string RoomName { get; set; }
        private string senderName;
        public string liveId { get; set; }
        public string SenderName
        {
            get => senderName;
            set { senderName = value; OnPropertyChanged(); }
        }
        public string RoomOwnerName { get; set; }
        public string RoomFmNumber { get; set; }

        private int fetchedGoldCount;
        public int FetchedGoldCount
        {
            get => fetchedGoldCount;
            set { fetchedGoldCount = value; OnPropertyChanged(); }
        }
        private string status;

        public string Status
        {
            get => status;
            set { status = value; OnPropertyChanged(); }
        }

        public string SenderPic { get; set; }
        public string RoomOwnerPic { get; set; }
        public string RoomPic { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public int Compare(RedBaoItem x, RedBaoItem y)
        {
            return x.Id - y.Id;
        }
    }
}
