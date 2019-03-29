using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Helper
{
    class HttpWebRequestHelper
    {
        public static async Task<string> GetResponseAsync(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Network request fail.");
                return null;
            }
        }

        public static async Task<string> PostResponseAsync(string url, string post = "")
        {
            try
            {
                byte[] requestBytes = Encoding.ASCII.GetBytes(post);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                Stream requestStream = await request.GetRequestStreamAsync();
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Network request fail.");
                return null;
            }

        }
    }
}
