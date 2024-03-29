﻿namespace Bangumi.Common
{
    internal static class Constants
    {
        public const string UnauthorizedImgUri = "ms-appx:///Assets/resource/err_401.png";
        public const string NotFoundImgUri = "ms-appx:///Assets/resource/err_404.png";
        public const string EmptyImgUri = "ms-appx:///Assets/resource/empty.png";
        public const string NoAvatarImgUri = "ms-appx:///Assets/resource/akkarin.jpg";
        public const string WelcomeImgUri = "ms-appx:///Assets/resource/welcome.png";
        // 将自己申请的应用相关信息填入，Bangumi 开发者平台： https://bgm.tv/dev/app
        public const string ClientId = "bgm8905c514a1b94ec1";
        public const string ClientSecret = "b678c34dd896203627da308b6b453775";
        public const string RedirectUrl = "bangumiuwp://authorize";

        public const string RefreshTokenTask = "RefreshTokenTask";
        public const string ToastBackgroundTask = "ToastBackgroundTask";
    }
}
