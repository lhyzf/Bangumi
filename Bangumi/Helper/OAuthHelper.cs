using Bangumi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Bangumi.Helper
{
    class OAuthHelper
    {
        private const string client_id = "bgm8905c514a1b94ec1";
        private const string client_secret = "b678c34dd896203627da308b6b453775";

        public static StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        // 用户登录
        public static async Task Authorize()
        {
            try
            {
                String URL = "https://bgm.tv/oauth/authorize?client_id=" + client_id + "&response_type=code";

                Uri StartUri = new Uri(URL);
                // When using the desktop flow, the success code is displayed in the html title of this end uri
                Uri EndUri = new Uri("https://bgm.tv/oauth/Bangumi.App");

                //rootPage.NotifyUser("Navigating to: " + GoogleURL, NotifyType.StatusMessage);

                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, StartUri, EndUri);
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    await GetAccessToken(WebAuthenticationResult.ResponseData.ToString().Replace("https://bgm.tv/oauth/Bangumi.App?code=", ""));
                }
                else if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    //OutputToken("HTTP Error returned by AuthenticateAsync() : " + WebAuthenticationResult.ResponseErrorDetail.ToString());
                }
                else
                {
                    //OutputToken("Error returned by AuthenticateAsync() : " + WebAuthenticationResult.ResponseStatus.ToString());
                }
            }
            catch (Exception Error)
            {
                //rootPage.NotifyUser(Error.Message, NotifyType.ErrorMessage);
            }
        }

        // 使用返回的的 code 换取 Access Token
        private static async Task GetAccessToken(string codeString)
        {
            Models.Posts.Token postData = new Models.Posts.Token
            {
                grant_type = "authorization_code",
                client_id = client_id,
                client_secret = client_secret,
                code = codeString,
                redirect_uri = "Bangumi.App",
            };
            string postUrl = "https://bgm.tv/oauth/access_token";
            HttpClient http = new HttpClient();
            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");
            var response = http.PostAsync(postUrl, httpContent);
            var jsonMessage = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AccessToken>(jsonMessage);
            //将信息写入本地文件
            await WriteTokens(result);
        }

        // 加密字符串
        private static async Task<IBuffer> TokenEncryptionAsync(string strMsg)
        {
            // Create a DataProtectionProvider object for the specified descriptor.
            DataProtectionProvider Provider = new DataProtectionProvider("LOCAL=user");

            try
            {
                // Encode the plaintext input message to a buffer.
                IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, BinaryStringEncoding.Utf8);

                // Encrypt the message.
                IBuffer buffProtected = await Provider.ProtectAsync(buffMsg);

                return buffProtected;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // 解密字符串
        private static async Task<string> TokenDecryption(IBuffer buffProtected)
        {
            // Create a DataProtectionProvider object.
            DataProtectionProvider Provider = new DataProtectionProvider();

            try
            {
                // Decrypt the protected message specified on input.
                IBuffer buffUnprotected = await Provider.UnprotectAsync(buffProtected);

                // Convert the unprotected message from an IBuffer object to a string.
                String strClearText = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffUnprotected);

                // Return the plaintext string.
                return strClearText;
            }
            catch (Exception)
            {
                return "";
            }

        }

        // 写入文件
        private static async Task WriteToFile(string msg, OAuthFile userFileName, bool encrytion)
        {
            var fileName = getOAuthFileName(userFileName);
            StorageFile storageFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            try
            {
                if (encrytion)
                {
                    var EncrytedData = await TokenEncryptionAsync(msg);
                    await FileIO.WriteBufferAsync(storageFile, EncrytedData);
                }
                else
                {
                    await FileIO.WriteTextAsync(storageFile, msg);
                }
            }
            catch (Exception)
            {
                await storageFile.DeleteAsync();
            }
        }

        // 从文件读取
        public static async Task<string> ReadFromFile(OAuthFile userFileName, bool encrytion)
        {
            try
            {
                var fileName = getOAuthFileName(userFileName);
                StorageFile storageFile = await localFolder.GetFileAsync(fileName);
                if (encrytion)
                {
                    IBuffer buffMsg = await FileIO.ReadBufferAsync(storageFile);
                    return await TokenDecryption(buffMsg);
                }
                else
                {
                    return await FileIO.ReadTextAsync(storageFile);
                }
            }
            catch (FileNotFoundException e)
            {
                // Cannot find file
                Debug.WriteLine(e.ToString());
                return "";
            }
            catch (IOException e)
            {
                // Get information from the exception, then throw
                // the info to the parent method.
                if (e.Source != null)
                {
                    Debug.WriteLine("IOException source: {0}", e.Source);
                }
                throw;
            }
        }

        // 刷新授权有效期
        public static async Task RefreshAccessToken()
        {
            Models.Posts.Token postData = new Models.Posts.Token
            {
                grant_type = "refresh_token",
                client_id = client_id,
                client_secret = client_secret,
                refresh_token = await ReadFromFile(OAuthFile.refresh_token, true),
                redirect_uri = "Bangumi.App",
            };
            string postUrl = "https://bgm.tv/oauth/access_token";
            HttpClient http = new HttpClient();
            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");
            var response = http.PostAsync(postUrl, httpContent);
            var jsonMessage = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AccessToken>(jsonMessage);
            //将信息写入本地文件
            await WriteTokens(result);
            return;
        }

        // 查询授权信息
        public static async Task<bool> CheckAccessToken()
        {
            var token = await ReadFromFile(OAuthFile.access_token, true);
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            string postUrl = string.Format("https://bgm.tv/oauth/token_status?access_token={0}", token);
            HttpClient http = new HttpClient();
            HttpContent httpContent = null;
            var response = http.PostAsync(postUrl, httpContent);
            var responseStatus = response.Result.StatusCode;
            var jsonMessage = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AccessToken>(jsonMessage);
            // C# 时间戳为 1/10000000 秒，从0001年1月1日开始；js 时间戳为秒，从1970年1月1日开始
            // 获取两天后的时间戳，离过期不足两天时或过期后更新 access_token
            var aa = (DateTime.Now.AddDays(2).ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000;
            if (result.expires < aa || responseStatus == System.Net.HttpStatusCode.Unauthorized)
                await RefreshAccessToken();
            return await CheckTokens();
        }

        // 检查用户授权文件
        private static async Task<bool> CheckTokens()
        {
            var accessToken = await ReadFromFile(OAuthFile.access_token, true);
            var refreshToken = await ReadFromFile(OAuthFile.refresh_token, true);
            var userId = await ReadFromFile(OAuthFile.user_id, false);
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(userId))
            {
                await DeleteTokens();
                return false;
            }
            return true;
        }

        // 写入 Tokens
        private static async Task WriteTokens(AccessToken token)
        {
            if (!string.IsNullOrEmpty(token.access_token) && !string.IsNullOrEmpty(token.refresh_token) && !string.IsNullOrEmpty(token.user_id.ToString()))
            {
                await WriteToFile(token.access_token, OAuthFile.access_token, true);
                await WriteToFile(token.refresh_token, OAuthFile.refresh_token, true);
                await WriteToFile(token.user_id.ToString(), OAuthFile.user_id, false);
            }
        }

        // 删除 Tokens
        public static async Task DeleteTokens()
        {
            StorageFile File = await localFolder.CreateFileAsync(getOAuthFileName(OAuthFile.access_token),
                CreationCollisionOption.ReplaceExisting);
            await File.DeleteAsync();
            File = await localFolder.CreateFileAsync(getOAuthFileName(OAuthFile.refresh_token),
                CreationCollisionOption.ReplaceExisting);
            await File.DeleteAsync();
            File = await localFolder.CreateFileAsync(getOAuthFileName(OAuthFile.user_id),
                CreationCollisionOption.ReplaceExisting);
            await File.DeleteAsync();
        }

        private static string getOAuthFileName(OAuthFile fileName)
        {
            string result = string.Empty;
            switch (fileName)
            {
                case OAuthFile.access_token:
                    result = "AccessToken.data";
                    break;
                case OAuthFile.refresh_token:
                    result = "RefreshToken.data";
                    break;
                case OAuthFile.user_id:
                    result = "UserId.data";
                    break;
            }
            return result;
        }

        public enum OAuthFile
        {
            access_token,
            refresh_token,
            user_id,
        };

    }
}
