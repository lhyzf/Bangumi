namespace Bangumi.Api.Models
{
    public static class SubjectTypeEnumEx
    {
        public static string GetDescCn(this SubjectTypeEnum type)
        {
            switch (type)
            {
                case SubjectTypeEnum.Book:
                    return "书籍";
                case SubjectTypeEnum.Anime:
                    return "动画";
                case SubjectTypeEnum.Music:
                    return "音乐";
                case SubjectTypeEnum.Game:
                    return "游戏";
                case SubjectTypeEnum.Real:
                    return "三次元";
                default:
                    return type.ToString(); 
            }
        }

        public static string GetValue(this SubjectTypeEnum type)
        {
            switch (type)
            {
                case SubjectTypeEnum.Book:
                    return "book";
                case SubjectTypeEnum.Anime:
                    return "anime";
                case SubjectTypeEnum.Music:
                    return "music";
                case SubjectTypeEnum.Game:
                    return "game";
                case SubjectTypeEnum.Real:
                    return "real";
                default:
                    return type.ToString().ToLower(); 
            }
        }
    }
}
