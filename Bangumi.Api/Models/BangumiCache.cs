using System;
using System.Collections.Generic;
using System.Text;

namespace Bangumi.Api.Models
{
    public class BangumiCache
    {
        /// <summary>
        /// 收藏，含动画、书籍、音乐、游戏、三次元
        /// </summary>
        public Dictionary<string, Collection2> Collections { get; set; }

        /// <summary>
        /// 条目详情
        /// </summary>
        public Dictionary<string, Subject> Subjects { get; set; }

        /// <summary>
        /// 条目收藏信息
        /// </summary>
        public Dictionary<string, SubjectStatus2> SubjectStatus { get; set; }

        /// <summary>
        /// 条目收视进度
        /// </summary>
        public Dictionary<string, Progress> Progresses { get; set; }

        /// <summary>
        /// 用户收视列表
        /// </summary>
        public List<Watching> Watchings { get; set; }

        /// <summary>
        /// 时间表
        /// </summary>
        public List<BangumiTimeLine> TimeLine { get; set; }
    }
}
