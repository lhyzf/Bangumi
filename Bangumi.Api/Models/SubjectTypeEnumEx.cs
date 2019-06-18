namespace Bangumi.Api.Models
{
    public static class SubjectTypeEnumEx
    {
        public static string GetValue(this SubjectTypeEnum type)
        {
            switch (type)
            {
                case SubjectTypeEnum.book:
                    return "书籍";
                case SubjectTypeEnum.anime:
                    return "动画";
                case SubjectTypeEnum.music:
                    return "音乐";
                case SubjectTypeEnum.game:
                    return "游戏";
                case SubjectTypeEnum.real:
                    return "三次元";
                default:
                    return type.ToString(); 
            }
        }
    }
}
