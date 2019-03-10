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
            await WriteAccessToken(result.access_token);
            await WriteRefreshToken(result.refresh_token);
            await WriteUserId(result.user_id.ToString());
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

        // Write AccessToken to a file and Encryption
        private static async Task WriteAccessToken(string accessToken)
        {
            StorageFile tokenFile = await localFolder.CreateFileAsync("AccessToken.data",
                CreationCollisionOption.ReplaceExisting);
            try
            {
                var EncrytedData = await TokenEncryptionAsync(accessToken);
                await FileIO.WriteBufferAsync(tokenFile, EncrytedData);
            }
            catch (Exception)
            {
                await tokenFile.DeleteAsync();
            }

        }

        // Read AccessToken from a file and Decryption
        public static async Task<string> ReadAccessToken()
        {
            try
            {
                StorageFile tokenFile = await localFolder.GetFileAsync("AccessToken.data");
                IBuffer buffMsg = await FileIO.ReadBufferAsync(tokenFile);
                //if (!IBuffer.Equals(buffMsg, null))
                //    return "";
                return await TokenDecryption(buffMsg);
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

        // Write RefreshToken to a file and Encryption
        private static async Task WriteRefreshToken(string refreshToken)
        {
            StorageFile tokenFile = await localFolder.CreateFileAsync("RefreshToken.data",
                CreationCollisionOption.ReplaceExisting);

            try
            {
                var EncrytedData = await TokenEncryptionAsync(refreshToken);
                await FileIO.WriteBufferAsync(tokenFile, EncrytedData);
            }
            catch (Exception)
            {
                await tokenFile.DeleteAsync();
            }
        }

        // Read RefreshToken from a file and Decryption
        private static async Task<string> ReadRefreshToken()
        {
            try
            {
                StorageFile tokenFile = await localFolder.GetFileAsync("RefreshToken.data");
                IBuffer buffMsg = await FileIO.ReadBufferAsync(tokenFile);
                //if (!IBuffer.Equals(buffMsg, null))
                //    return "";
                return await TokenDecryption(buffMsg);
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

        // Write UserId to a file
        private static async Task WriteUserId(string userId)
        {
            StorageFile file = await localFolder.CreateFileAsync("UserId.data",
                CreationCollisionOption.ReplaceExisting);

            try
            {
                await FileIO.WriteTextAsync(file, userId);
            }
            catch (Exception)
            {
                await file.DeleteAsync();
            }
        }

        // Read UserId from a file
        public static async Task<string> ReadUserId()
        {
            try
            {
                StorageFile File = await localFolder.GetFileAsync("UserId.data");
                return await FileIO.ReadTextAsync(File);
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
        private static async Task RefreshAccessToken()
        {
            return;
        }

        // 查询授权信息
        public static async Task CheckAccessToken()
        {
            var token = await ReadAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                await Authorize();
                return;
            }
            AccessToken postData = new AccessToken
            {
                access_token = token,
            };
            string postUrl = "https://bgm.tv/oauth/token_status";
            HttpClient http = new HttpClient();
            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = http.PostAsync(postUrl, httpContent);
            var jsonMessage = await response.Result.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AccessToken>(jsonMessage);
            if (result.expires_in == 0)
                RefreshAccessToken();
        }

        // 检查用户授权文件
        public static async Task<bool> CheckTokens()
        {
            var accessToken = await ReadAccessToken();
            var refreshToken = await ReadRefreshToken();
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                await DeleteTokens();
                return false;
            }
            return true;
        }

        // 删除Tokens
        public static async Task DeleteTokens()
        {
            StorageFile File = await localFolder.CreateFileAsync("AccessToken.data",
                CreationCollisionOption.ReplaceExisting);
            await File.DeleteAsync();
            File = await localFolder.CreateFileAsync("RefreshToken.data",
                CreationCollisionOption.ReplaceExisting);
            await File.DeleteAsync();
            File = await localFolder.CreateFileAsync("UserId.data",
                CreationCollisionOption.ReplaceExisting);
            await File.DeleteAsync();
            
        }

    }
}
