using System;
using System.Text.Json.Serialization;

namespace simpliBuild.SWMS.Model
{
    public class SimpliAccessToken
    {
        private string _accessToken;
        private int _expires;


        /// <summary>
        /// Token
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken
        {
            get => "Bearer " + _accessToken;
            set => _accessToken = value;
        }

        /// <summary>
        /// The type of token
        /// </summary>
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }


        /// <summary>
        /// Time in seconds of when the token expires
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int Expires
        {
            get => _expires;
            set
            {
                _expires = value;
                ExpireTime = DateTime.Now.AddSeconds(_expires - 2);
            }
        }

        /// <summary>
        /// Date time of token expiry
        /// </summary>
        //[JsonPropertyName("ExpiresTime")] 
        public DateTime ExpireTime { get; private set; }

        public bool HasExpired => ExpireTime <= DateTime.Now;

    }
}