using Bangumi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
        public static StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        // 用户登录
        public static async Task Authorize()
        {
            try
            {
                String URL = "https://bgm.tv/oauth/authorize?client_id=" + Constants.client_id + "&response_type=code";

                Uri StartUri = new Uri(URL);
                // When using the desktop flow, the success code is displayed in the html title of this end uri
                Uri EndUri = new Uri($"https://bgm.tv/oauth/{Constants.redirect_url}");

                //rootPage.NotifyUser("Navigating to: " + GoogleURL, NotifyType.StatusMessage);

                WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, StartUri, EndUri);
                if (WebAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    await GetAccessToken(WebAuthenticationResult.ResponseData.ToString().Replace($"https://bgm.tv/oauth/{Constants.redirect_url}?code=", ""));
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
            catch (Exception)
            {
                //rootPage.NotifyUser(Error.Message, NotifyType.ErrorMessage);
            }
        }

        // 使用返回的的 code 换取 Access Token
        private static async Task GetAccessToken(string codeString)
        {
            string url = "https://bgm.tv/oauth/access_token";
            string postData = "grant_type=authorization_code";
            postData += "&client_id=" + Constants.client_id;
            postData += "&client_secret=" + Constants.client_secret;
            postData += "&code=" + codeString;
            postData += "&redirect_uri=" + Constants.redirect_url;

            try
            {
                byte[] requestBytes = Encoding.ASCII.GetBytes(postData);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                Stream requestStream = await request.GetRequestStreamAsync();
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                // Get response
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                string content;
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    content = reader.ReadToEnd();
                }
                var result = JsonConvert.DeserializeObject<AccessToken>(content);
                //将信息写入本地文件
                await WriteTokens(result);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
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
        private static async Task<bool> WriteToFile(string msg, OAuthFile userFileName, bool encrytion)
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
                return true;
            }
            catch (Exception)
            {
                await storageFile.DeleteAsync();
                return false;
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
            catch (FileNotFoundException)
            {
                // Cannot find file
                Debug.WriteLine("File not found.");
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
                return "";
            }
        }

        // 刷新授权有效期
        public static async Task RefreshAccessToken()
        {
            string url = "https://bgm.tv/oauth/access_token";
            string postData = "grant_type=refresh_token";
            postData += "&client_id=" + Constants.client_id;
            postData += "&client_secret=" + Constants.client_secret;
            postData += "&refresh_token=" + await ReadFromFile(OAuthFile.refresh_token, true);
            postData += "&redirect_uri=" + Constants.redirect_url;

            try
            {
                byte[] requestBytes = Encoding.ASCII.GetBytes(postData);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                Stream requestStream = await request.GetRequestStreamAsync();
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                // Get response
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                string content;
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    content = reader.ReadToEnd();
                }
                var result = JsonConvert.DeserializeObject<AccessToken>(content);
                //将信息写入本地文件
                await WriteTokens(result);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // 查询授权信息
        private static async void CheckAccessToken()
        {
            var token = await ReadFromFile(OAuthFile.access_token, true);
            string url = string.Format("https://bgm.tv/oauth/token_status?access_token={0}", token);

            try
            {
                string response = await HttpHelper.PostAsync(url);
                var result = JsonConvert.DeserializeObject<AccessToken>(response);
                // C# 时间戳为 1/10000000 秒，从0001年1月1日开始；js 时间戳为秒，从1970年1月1日开始
                // 获取两天后的时间戳，离过期不足两天时或过期后更新 access_token
                var aa = (DateTime.Now.AddDays(2).ToUniversalTime().Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000;
                if (string.IsNullOrEmpty(response) || result.expires < aa)
                    await RefreshAccessToken();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // 检查用户授权文件
        public static async Task<bool> CheckTokens()
        {
            var accessToken = await ReadFromFile(OAuthFile.access_token, true);
            var refreshToken = await ReadFromFile(OAuthFile.refresh_token, true);
            var userId = await ReadFromFile(OAuthFile.user_id, false);
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(userId))
            {
                await DeleteTokens();
                return false;
            }
            // 检查是否在有效期内，接近过期或过期则刷新token
            CheckAccessToken();
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
