using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using System.Windows.Navigation;
using System.Windows.Data;
using System.Collections.ObjectModel;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Microsoft.Phone.Tasks;
using GalaSoft.MvvmLight.Messaging;
using SanzaiGuokr.Messages;

namespace WeiboApi
{
    public class WeiboGroup : ObservableCollection<WeiboItem>
    {
        public string Title { get; set; }
        public bool HasItems
        {
            get
            {
                return Count != 0;
            }
        }

        /*
        public override bool Equals(object obj)
        {
            var that = obj as WeiboGroup;
            return (that != null) && (that.Title == Title);
        }
         */

    }
    public enum UnreadType
    {
        ALL,
        COMMENTS,
        MENTIONS,
        DM,
        FOLLOWERS
    }

    public class WeiboItem : INotifyPropertyChanged
    {
        public Int64 id { get; set; }

        #region commands
        public bool can_repost { get; set; }
        public bool can_comment { get; set; }
        public bool can_delete { get; set; }
        public string repost_qs { get; set; }
        public string comment_qs { get; set; }
        #endregion

        #region concise display elements
        public string cs_type { get; set; }
        public string cs_user_image { get; set; }
        public string cs_user_name { get; set; }
        public string cs_text { get; set; }
        public string cs_quote { get; set; }
        public DateTime cs_datetime { get; set; }
        public Uri cs_uri { get; set; }
        public Visibility cs_has_retweet { get; set; }
        private bool _unread;

        public bool cs_is_unread
        {
            get { return _unread; }
            set
            {
                _unread = value;
                RaisePropertyChange("cs_is_unread");
            }
        }

        #endregion

        internal void OpenItem()
        {
            throw new NotImplementedException();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChange(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }


    }

    public class oauth
    {
        public string oauth_token { get; set; }
        public string oauth_verifier { get; set; }
        public string user_id { get; set; }
    }

    interface INormalizable
    {
        void normalize();
    }

    public class user : WeiboItem
    {
        public string name { get; set; }
        public string location { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string domain { get; set; }
        public string gender { get; set; }
        public int followers_count { get; set; }
        public int friends_count { get; set; }
        public int statuses_count { get; set; }
        public string profile_image_url { get; set; }
        public bool verified { get; set; }
        public string created_at { get; set; }
        public status status { get; set; }

        public bool following { get; set; } // whether you are following this user
        public bool followed_by { get; set; } // whether he/she is following you

        private string _trn = "";
        public string truncated_name
        {
            get
            {
                if (_trn == "")
                {
                    int length = 0;
                    int i = 0;
                    for (i = 0; i < name.Length; i++)
                    {
                        if (length >= 16)
                            break;
                        var item = name[i];
                        if ((item <= '9' && item >= '0')
                            || (item <= 'z' && item >= 'a')
                            || (item <= 'Z' && item >= 'A'))
                            length += 1;
                        else
                            length += 2;
                    }
                    _trn = name.Substring(0, i);
                    if (i < name.Length)
                        _trn += "..";
                }
                return _trn;
            }
        } // limit to 8 characters at most

        /*
        public string screen_name { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string allow_all_act_msg { get; set; }
        public string in_reply_to_status_id { get; set; }
        public string in_reply_to_user_id { get; set; }
        public string in_reply_to_screen_name { get; set; }
         */

        private DateTime _dca = default(DateTime);
        public DateTime dt_created_at
        {
            get
            {
                if (_dca == default(DateTime))
                    if (created_at != null)
                        _dca = DateTime.ParseExact(created_at, "ddd MMM dd HH:mm:ss K yyyy", System.Globalization.CultureInfo.InvariantCulture);
                return _dca;
            }
        }

        public string thumbnail_image_url { get; set; }

        public string hd_image_url { get; set; }
        public Visibility is_verified
        {
            get
            {
                return verified ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public Uri blog_url { get; set; }

        private bool _is_normalized = false;
        public bool IsNormalized
        {
            get { return _is_normalized; }
            set { _is_normalized = value; }
        }
        public void normalize()
        {
            if (_is_normalized)
                return;
            if (profile_image_url != null)
            {
                thumbnail_image_url = profile_image_url.Replace("/50/", "/30/");
                hd_image_url = profile_image_url.Replace("/50/", "/180/");
            }

            if (name.Contains("Tardis"))
                verified = true; //hack

            if (!string.IsNullOrEmpty(url))
                blog_url = new Uri(url, UriKind.Absolute);

            #region concise display elements
            cs_type = "user";
            cs_datetime = dt_created_at;
            cs_quote = description;
            cs_text = location;
            cs_user_image = thumbnail_image_url;
            cs_user_name = name;
            cs_uri = new Uri("/Pages/User.xaml?id=" + id.ToString(), UriKind.Relative);
            cs_has_retweet = Visibility.Collapsed;
            #endregion

            #region commands
            can_comment = false;
            can_delete = false;
            can_repost = false;
            #endregion
            _is_normalized = true;
        }

        public override string ToString()
        {
            return name;
        }
    }

    public class status : WeiboItem, IComparable, INormalizable
    {
        // id is got from WeiboItem
        //public Int64 id { get; set; }

        public string text { get; set; }
        public user user { get; set; }
        public string mid { get; set; }
        public string thumbnail_pic { get; set; }
        public string bmiddle_pic { get; set; }
        public string original_pic { get; set; }
        public status retweeted_status { get; set; }
        public string source { get; set; }

        public string created_at { get; set; }
        public string meta_timestamp { get; set; }
        public DateTime dt_created_at { get; set; }

        private int _retweet_cnt = 0;
        public int nr_retweet
        {
            get { return _retweet_cnt; }
            set
            {
                var old = _retweet_cnt;
                _retweet_cnt = value;

                if (old != value)
                {
                    normalize_counts();
                    RaisePropertyChange("nr_retweet");
                    RaisePropertyChange("has_retweet_count");
                }
            }
        }
        public Visibility has_retweet_count { get; set; }

        private int _comment_cnt = 0;
        public int nr_comment
        {
            get { return _comment_cnt; }
            set
            {
                var old = _comment_cnt;
                _comment_cnt = value;

                if (old != value)
                {
                    normalize_counts();
                    RaisePropertyChange("nr_comment");
                    RaisePropertyChange("has_comment_count");
                }
            }
        }
        public Visibility has_comment_count { get; set; }


        // todo: clean up. how to bind to a null object
        public System.Windows.Visibility has_picture { get; set; }
        public System.Windows.Visibility has_retweet { get; set; }
        public bool b_has_retweet { get; set; }

        public class StatusReversedComparer : IComparer<status>
        {

            public int Compare(status x, status y)
            {
                // compare the things and return in reverse order
                if (x.created_at == y.created_at)
                {
                    return -Comparer<Int64>.Default.Compare(x.id, y.id);
                }
                else
                {
                    return -DateTime.Compare(x.dt_created_at, y.dt_created_at);
                }
            }
        }

        bool _is_normalized = false;
        public bool IsNormalized
        {
            get
            {
                return _is_normalized;
            }
        }
        public void normalize()
        {
            _normalize();

            // things that you don't want to put into retweeted status
            // UserManager.Manager.AddFriend(user.name);
        }
        public void _normalize()
        {
            if (_is_normalized)
                return;
            if (user == null)
                return;

            dt_created_at = string.IsNullOrEmpty(created_at) ? default(DateTime)
                : DateTime.ParseExact(created_at, "ddd MMM dd HH:mm:ss K yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var timediff = DateTime.Now - dt_created_at;

            if (timediff < TimeSpan.FromSeconds(60))
                meta_timestamp = "刚刚更新";
            else if (timediff < TimeSpan.FromMinutes(60))
                meta_timestamp = ((int)timediff.TotalMinutes).ToString() + "分钟前";
            else if (timediff < TimeSpan.FromHours(24))
                meta_timestamp = ((int)timediff.TotalHours).ToString() + "小时前";
            else
                meta_timestamp = dt_created_at.ToLocalTime().ToString("yyyy/mm/dd hh:mm");

            has_picture = (thumbnail_pic != null ? Visibility.Visible : Visibility.Collapsed);
            has_retweet = (retweeted_status != null ? Visibility.Visible : Visibility.Collapsed);
            b_has_retweet = (retweeted_status != null ? true : false);
            mid = mid.Length < 8 ? mid : mid.Substring(4, 4);
            char[] delim = new char[] { '<', '>' };
            if (!string.IsNullOrEmpty(source))
                source = source.Split(delim)[2];

            if (user != null)
                user.normalize();
            normalize_counts();
            // mid = mid.Substring(5);
            if (retweeted_status != null)
                retweeted_status._normalize();

            #region concise display elements
            cs_type = "status";
            cs_datetime = dt_created_at;
            cs_quote = retweeted_status == null ? null : retweeted_status.name_and_text;
            cs_text = text;
            cs_user_image = user.thumbnail_image_url;
            cs_user_name = user.name;
            cs_uri = new Uri("/Pages/ViewWeibo.xaml?id=" + id.ToString(), UriKind.Relative);
            cs_has_retweet = has_retweet;
            #endregion

            #region layout optimization
            if (retweeted_status != null)
            {
                has_picture = retweeted_status.has_picture;
                thumbnail_pic = retweeted_status.thumbnail_pic;
                bmiddle_pic = retweeted_status.bmiddle_pic;
                original_pic = retweeted_status.original_pic;
            }
            #endregion

            #region commands
            can_comment = true;
            //can_delete = user.id == UserManager.UserProfile.You.id;
            can_repost = true;
            repost_qs = "id=" + id.ToString();
            comment_qs = repost_qs;
            #endregion


            _is_normalized = true;
        }

        private void normalize_counts()
        {
            if (nr_comment == 0)
                has_comment_count = Visibility.Collapsed;
            else
                has_comment_count = Visibility.Visible;

            if (nr_retweet == 0)
                has_retweet_count = Visibility.Collapsed;
            else
                has_retweet_count = Visibility.Visible;
        }

        public string name_and_text
        {
            get
            {
                if (user == null)
                    return text;
                return "@" + user.name + ": " + text;
            }
        }

        // converters
        public static implicit operator status(comment c)
        {
            var s = new status();
            s.retweeted_status = c.status;
            s.text = c.text;
            s.user = c.user;
            s.id = c.id;
            return s;
        }

        // IComparable
        public int CompareTo(object b)
        {
            status bb = b as status;
            return bb.id.CompareTo(id); // reversed
        }

        public void UpdateCount(count item)
        {
            nr_retweet = item.rt;
            nr_comment = item.comments;
        }

        private HtmlDocument _htmlDoc = null;
        public HtmlDocument HtmlDoc
        {
            get
            {
                if (_htmlDoc == null)
                {
                    _htmlDoc = WeiboLinkParse(text);
                }
                return _htmlDoc;
            }
        }
        private HtmlDocument _htmlDocWithName = null;
        public HtmlDocument HtmlDocWithName
        {
            get
            {
                if (_htmlDocWithName == null)
                {
                    _htmlDocWithName = WeiboLinkParse(name_and_text);
                }
                return _htmlDocWithName;
            }
        }
        HtmlDocument WeiboLinkParse(string Text)
        {
            var res = new HtmlDocument();
            if (String.IsNullOrEmpty(Text))
                return res;

            for (int i = 0, j = 0; i < Text.Length; i++)
            {
                if (i == Text.Length - 1)
                    if (j <= i)
                    {
                        var tx = res.CreateTextNode(Text.Substring(j, i - j + 1));
                        res.DocumentNode.AppendChild(tx);
                        break;
                    }

                Match match;
                var c = Text[i];
                switch (c)
                {
                    case '@':
                        match = new Regex(@"@[\-_0-9a-zA-Z\u4e00-\u9fa5]{2,30}").Match(Text, i);
                        if (match.Index == i && match.Length > 0)
                        {
                            if (i + match.Length < Text.Length &&
                                Text[i + match.Length] == '.')
                            {
                                // todo: should recognize email address
                            }
                            else
                            {
                                // create textblock
                                if (j < i)
                                    res.DocumentNode.AppendChild(res.CreateTextNode(Text.Substring(j, i - j)));
                                //create hyperlink button
                                var u = res.CreateElement("u");
                                u.SetAttributeValue("screen_name", match.Value.Substring(1));
                                u.InnerHtml = match.Value;
                                res.DocumentNode.AppendChild(u);
                                //update index
                                i += match.Length - 1;
                                j = i + 1;
                            }
                        }
                        break;
                    case 'h':
                        string pattern = "http://t.cn/";
                        int k = 0;
                        while (k < pattern.Length && i + k < Text.Length && pattern[k] == Text[i + k])
                            k++;
                        if (k == pattern.Length)
                        {
                            match = (new Regex(@"[a-zA-Z0-9]{4,10}")).Match(Text, i + k);
                            if (match.Index == i + k && match.Length > 0)
                            {
                                // flush textblock
                                if (j < i)
                                    res.DocumentNode.AppendChild(res.CreateTextNode(Text.Substring(j, i - j)));
                                // create hyperlink button
                                var h = res.CreateElement("a");
                                h.SetAttributeValue("href", pattern + match.Value);
                                h.InnerHtml = pattern + match.Value;
                                res.DocumentNode.AppendChild(h);
                                // add to the links collection
                                Links.Add(pattern + match.Value);
                                // update index
                                i += pattern.Length + match.Length - 1;
                                j = i + 1;
                                break;
                            }
                        }
                        i += k - 1;
                        break;
                    case '#':
                        int l = i + 1;
                        while (l < Text.Length
                            && (Text[l] >= 'a' && Text[l] <= 'z'
                                || Text[l] >= 'A' && Text[l] <= 'Z'
                                || Text[l] == '-' || Text[l] == '_' || Text[l] == ' ' || Text[l] == '·'
                                || Text[l] >= '\u4e00' && Text[l] <= '\u9fa5'
                                || Text[l] >= '0' && Text[l] <= '9')
                            && Text[l] != '#') l++;

                        if (l == Text.Length)
                        {
                            if (j <= l)
                                res.DocumentNode.AppendChild(res.CreateTextNode(Text.Substring(j, l - j)));
                            i = l;
                            j = i + 1;
                        }
                        else if (Text[l] != '#')
                        {
                            if (j <= l)
                                res.DocumentNode.AppendChild(res.CreateTextNode(Text.Substring(j, l - j + 1)));
                            i = l;
                            j = i + 1;
                        }
                        else
                        {
                            string t = Text.Substring(i, l - i + 1);
                            // flush textblock
                            if (j < i)
                                res.DocumentNode.AppendChild(res.CreateTextNode(Text.Substring(j, i - j)));
                            // create hyperlink button
                            var topic = res.CreateElement("topic");
                            topic.SetAttributeValue("search_string", t.Substring(1, t.Length - 2));
                            topic.InnerHtml = t;
                            res.DocumentNode.AppendChild(topic);
                            // update index
                            i += t.Length - 1;
                            j = i + 1;
                        }
                        break;
                }
            }

            return res;
        }
        private List<string> Links = new List<string>();

        #region Relay Commands
        private RelayCommand _gotolink = null;
        public RelayCommand GoToHyperLink
        {
            get
            {
                if (_gotolink == null)
                    _gotolink = new RelayCommand(() =>
                        {
                            var task = new WebBrowserTask();
                            task.Uri = new Uri(Links[0], UriKind.Absolute);
                            task.Show();
                        }, canGoToHyperLink);
                return _gotolink;
            }
        }
        bool canGoToHyperLink()
        {
            return Links.Count > 0;
        }

        private RelayCommand _viewImage;

        public RelayCommand ViewImage
        {
            get
            {
                if (_viewImage == null)
                {
                    _viewImage = new RelayCommand(()=>
                        {
                            Messenger.Default.Send<ViewImageMessage>(new ViewImageMessage()
                            {
                                small_uri = thumbnail_pic,
                                med_uri = bmiddle_pic,
                                large_uri = original_pic
                            });
                        }, canViewImage);
                }
                return _viewImage;
            }
            set { _viewImage = value; }
        }
        bool canViewImage()
        {
            return thumbnail_pic != null;
        }

        #endregion
    }

    public class count : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChange(string name)
        {
            if (PropertyChanged != null)
                Deployment.Current.Dispatcher.BeginInvoke(() => PropertyChanged(this, new PropertyChangedEventArgs(name)));
        }
        public int new_status { get; set; }

        private int _f;
        public int followers
        {
            get { return _f; }
            set
            {
                _f = value;
                RaisePropertyChange("Summary");
            }
        }
        private int _d;
        public int dm
        {
            get { return _d; }
            set
            {
                _d = value;
                RaisePropertyChange("Summary");
            }
        }

        private int _m;
        public int mentions
        {
            get { return _m; }
            set
            {
                _m = value;
                RaisePropertyChange("Summary");
            }
        }

        private int _c;

        public int comments
        {
            get { return _c; }
            set
            {
                _c = value;
                RaisePropertyChange("Summary");
            }
        }

        public Int64 id { get; set; }
        public int rt { get; set; }
        public string _s;


        public string Summary
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (mentions != 0)
                    sb.AppendFormat("m:{0}", mentions);
                if (comments != 0)
                    sb.AppendFormat(" c:{0}", comments);
                return sb.ToString();
            }
            set
            {
                _s = value;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var val = obj as count;
            if (val == null)
                return false;

            if (val.comments == comments
                && val.dm == dm
                && val.followers == followers
                && val.mentions == mentions)
                return true;
            else
                return false;
        }
        public override int GetHashCode()
        {
            return string.Format("{0}.{1}.{2}.{3}", comments, dm, followers, mentions).GetHashCode();
        }

        public void Copy(count c)
        {
            comments = c.comments;
            followers = c.followers;
            dm = c.dm;
            mentions = c.mentions;
        }
        public void Reset()
        {
            Copy(ZERO);
        }

        public int get(UnreadType t)
        {
            switch (t)
            {
                case UnreadType.ALL:
                    throw new NotImplementedException();
                case UnreadType.COMMENTS:
                    return comments;
                case UnreadType.MENTIONS:
                    return mentions;
                case UnreadType.DM:
                    return dm;
                case UnreadType.FOLLOWERS:
                    return followers;
                default:
                    throw new NotImplementedException();
            }
        }
        public void set(UnreadType t, int val)
        {
            switch (t)
            {
                case UnreadType.ALL:
                    Reset();
                    break;
                case UnreadType.COMMENTS:
                    comments = val;
                    break;
                case UnreadType.MENTIONS:
                    mentions = val;
                    break;
                case UnreadType.DM:
                    dm = val;
                    break;
                case UnreadType.FOLLOWERS:
                    followers = val;
                    break;
                default:
                    break;
            }
        }
        public static count ZERO = new count() { mentions = 0, followers = 0, dm = 0, comments = 0 };
        public static bool operator >=(count a, count b)
        {
            if (a == null || b == null)
                return false;

            if (a.dm >= b.dm
                && a.followers >= b.followers
                && a.mentions >= b.mentions
                && a.comments >= b.comments)
                return true;
            else
                return false;
        }
        public static bool operator <=(count a, count b)
        {
            if (a == null || b == null)
                return false;

            if (a.dm <= b.dm
                && a.followers <= b.followers
                && a.mentions <= b.mentions
                && a.comments <= b.comments)
                return true;
            else
                return false;
        }
        public static count operator -(count a, count b)
        {
            return new count()
            {
                followers = a.followers - b.followers,
                mentions = a.mentions - b.mentions,
                dm = a.dm - b.dm,
                comments = a.comments - b.comments
            };
        }

    }

    public class response
    {
        public string Value { get; set; }
    }

    public class comment : WeiboItem, IComparable, INotifyPropertyChanged
    {
        // id inherites from WeiboItem
        // public Int64 id { get; set; }

        public string text { get; set; }
        public user user { get; set; }
        public status status { get; set; }

        public string created_at { get; set; }
        public DateTime dt_created_at;

        bool _is_normalized = false;
        public bool IsNormalized
        {
            get
            {
                return _is_normalized;
            }
        }
        public void normalize()
        {
            if (_is_normalized)
                return;

            dt_created_at = DateTime.ParseExact(created_at, "ddd MMM dd HH:mm:ss K yyyy", System.Globalization.CultureInfo.InvariantCulture);
            user.normalize();
            status.normalize();

            //UserManager.Manager.AddFriend(user.name);

            #region concise display elements
            cs_type = "comment";
            cs_datetime = dt_created_at;
            cs_quote = status.name_and_text;
            cs_text = text;
            cs_user_image = user.thumbnail_image_url;
            cs_user_name = user.name;
            cs_uri = new Uri("/Pages/ViewWeibo.xaml?id=" + status.id.ToString(), UriKind.Relative);
            cs_has_retweet = Visibility.Visible;
            #endregion

            #region commands
            can_comment = true;
            can_repost = false;
            //can_delete = status.user.id == UserManager.UserProfile.You.id || user.id == UserManager.UserProfile.You.id ? true : false;
            comment_qs = string.Format("id={0}&cid={1}", status.id, id);
            #endregion

            _is_normalized = true;
        }
        public int CompareTo(object b)
        {
            comment bb = b as comment;
            return bb.id.CompareTo(id); // reversed
        }

        public string name_and_text
        {
            get
            {
                return "@" + user.name + ": " + text;
            }
        }
    }

    public class Error
    {
        public string request { get; set; }
        public int error_code { get; set; }
        public string error { get; set; }

        public Error()
        {

        }
    }
    public class relationship_user
    {
        public Int64 id { get; set; }
        public bool following { get; set; }
        public bool followed_by { get; set; }
    }

    public class relationship
    {
        public relationship_user source { get; set; }
        public relationship_user target { get; set; }
    }

    #region api_responses
    public class friends_bilateral_response
    {
        public List<Int64> ids { get; set; }
        public int total_number { get; set; }
    }

    public class Api_Statuses_Followings_repsonse
    {
        public List<user> users { get; set; }
        public int next_cursor { get; set; }
        public int previous_cursor { get; set; }
    }
    #endregion

}
