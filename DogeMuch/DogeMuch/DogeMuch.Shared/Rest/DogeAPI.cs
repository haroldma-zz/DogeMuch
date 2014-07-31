/**
 * Inspired by DogeAPI.NET
 * Modified to work with Block.io api by HArry
 * 
 * Originally written by Hung Ly (hungly@xenfinite.com)
 * 
 * This file is released under the MIT license
 */

#region

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using DogeMuch.Model;
using DogeMuch.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace DogeMuch.Rest
{
    public class DogeApi
    {
        //Consts
        // ReSharper disable InconsistentNaming

        #region ApiCalls enum

        public enum ApiCalls
        {
            //V1 API Calls


            get_balance,
            withdraw,
            get_new_address,
            get_my_addresses,
            get_address_received,
            get_address_by_label,
            get_difficulty,
            get_current_block,
            get_current_price,

            //V2 API CAlls

            create_user,
            get_user_address,
            get_user_balance,
            withdraw_from_user,
            move_to_user,
            get_users,
            get_transactions,
            get_network_hashrate,
            get_info
        }

        #endregion

        // ReSharper restore InconsistentNaming

        private const String Apiurl = "https://block.io";

        private const String ApiRoute = "/api/v1/";

        public DogeApi(string key)
        {
            ApiKey = key;
        }

        public String ApiKey { get; set; }

        public void ChangeApiKey(string key)
        {
            ApiKey = key;
        }

        private double ToDouble(object doubleStr)
        {
            try
            {
                return Convert.ToDouble(doubleStr);
            }
            catch (Exception e)
            {
                throw DogeException.Generate("couldn't parse double", e);
            }
        }


        /// <summary>
        ///     Account-wide Balance
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetBalanceAsync()
        {
            var response = await MakeApiCall(ApiCalls.get_balance, new Dictionary<string, string>());
            return Convert.ToDouble(response["available_balance"]);
        }

        /// <summary>
        ///     Gets api key network
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetNetworkForKeyAsync()
        {
            var response = await MakeApiCall(ApiCalls.get_balance, new Dictionary<string, string>());
            return response["network"].ToString();
        }

        /// <summary>
        ///     Withdraw Doge from DogeAPI to Payment Address
        /// </summary>
        /// <param name="amount">Amount of DogeCoin</param>
        /// <param name="pin">DogeAPI Pin</param>
        /// <param name="paymentAddress">Address to send Coins to</param>
        public async Task WithdrawAsync(double amount, string pin, String paymentAddress)
        {
            await MakeApiCall(ApiCalls.withdraw,
                new Dictionary<string, string>
                {
                    {"amount", Convert.ToString(amount)},
                    {"pin", pin},
                    {"payment_address", paymentAddress}
                });
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("doge", "withdrawal", null, (long)amount);
        }

        /// <summary>
        ///     List of all addresses
        /// </summary>
        /// <returns></returns>
        public async Task<List<BlockAddress>> GetMyAddressesAsync()
        {
            var response = await MakeApiCall(ApiCalls.get_my_addresses, new Dictionary<string, string>());
            var addresses = JsonConvert.DeserializeObject<List<BlockAddress>>(response["addresses"].ToString());

            return addresses;
        }

        /// <summary>
        ///     Creates a new address with no label
        /// </summary>
        /// <returns>Address String</returns>
        public async Task<string> GetNewAddressAsync()
        {
            return await GetNewAddressAsync(null);
        }

        /// <summary>
        ///     Creates a new address with assoc. label
        /// </summary>
        /// <param name="label">Designation Label</param>
        /// <returns>Address String</returns>
        public async Task<string> GetNewAddressAsync(String label)
        {
            var response =
                await MakeApiCall(ApiCalls.get_new_address, 
                new Dictionary<string, string> {{
                                                    "label", label.Replace(" ", "")}});
            return Convert.ToString(response["address"]);
        }

        /// <summary>
        ///     List of addresses with label
        /// </summary>
        /// <param name="label">Selection Label</param>
        /// <returns>List of Addresses</returns>
        public async Task<List<string>> GetAddressByLabel(String label)
        {
            var response = await MakeApiCall(ApiCalls.get_address_by_label,
                new Dictionary<string, string> {{"address_label", label}});
            return JsonConvert.DeserializeObject<List<String>>(response["addresses"].ToString());
        }

        /// <summary>
        ///     Network Hashing Difficulty
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetDifficulty()
        {
            var response = await MakeApiCall(ApiCalls.get_difficulty, new Dictionary<string, string>());
            return Convert.ToDouble(response["difficulty"]);
        }

        /// <summary>
        ///     Current Network Block
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetCurrentBlock()
        {
            var response = await MakeApiCall(ApiCalls.get_current_block, new Dictionary<string, string>());
            return Convert.ToDouble(response["current_block"]);
        }

        /// <summary>
        ///     Get current dogecoin conversion price in USD/Coin
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetCurrentPriceAsync()
        {
            return await GetCurrentPriceAsync(0);
        }

        /// <summary>
        ///     Get current dogecoin conversion price in USD/# of Coins
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<double> GetCurrentPriceAsync(long amount)
        {
            return await GetCurrentPriceAsync(amount, "USD");
        }

        /// <summary>
        ///     Get curreny dogecoin conversion price in Currency/# of Coins
        /// </summary>
        /// <param name="conversionType">USD/BTC</param>
        /// <param name="amount">Amount of Dogecoins</param>
        /// <returns></returns>
        public async Task<double> GetCurrentPriceAsync(long amount, String conversionType)
        {
            if (conversionType.ToLower() != "btc" && conversionType.ToLower() != "usd") throw new Exception("Invalid currency type! Options: btc, usd");

            var Params = new Dictionary<String, String> {{"convert_to", conversionType}};
            if (amount > 0) Params.Add("amount_doge", Convert.ToString(amount));

            var response = await MakeApiCall(ApiCalls.get_current_price, Params);
            return Convert.ToDouble(response["amount"]);
        }

        /// <summary>
        ///     Get amount of coins received from label or address
        /// </summary>
        /// <param name="type">Label or Address</param>
        /// <param name="identifier"></param>
        /// <returns>Amount of Coins</returns>
        public async Task<double> GetAddressReceivedAsync(String type, String identifier)
        {
            if (type != "payment_address" && type != "address_label")
            {
                throw new Exception("Incorrect Address Type! Options: payment_address, address_label");
            }
            var response = await MakeApiCall(ApiCalls.get_address_received,
                new Dictionary<string, string> {{type, identifier}});
            return Convert.ToDouble(response["received"]);
        }

        //WARNING THE FOLLOWING METHODS HAVE NOT BEEN MODIFY OR TESTED TO WORK ON BLOCK.IO

        //////////////////////////////////////////
        //  Account related API Calls
        //////////////////////////////////////////

        public async Task<string> CreateUserAsync(String userId)
        {
            var response = await MakeApiCall(ApiCalls.create_user, new Dictionary<string, string> {{"user_id", userId}});
            return Convert.ToString(response["address"]);
        }

        public async Task<string> GetUserAddressAsync(String userId)
        {
            var response =
                await MakeApiCall(ApiCalls.get_user_address, new Dictionary<string, string> {{"user_id", userId}});
            return Convert.ToString(response["address"]);
        }

        public async Task<double> GetUserBalanceAsync(String userId)
        {
            var response =
                await MakeApiCall(ApiCalls.get_user_balance, new Dictionary<string, string> {{"user_id", userId}});
            return ToDouble(response["balance"]);
        }

        public async Task MoveTouserAsync(String fromUserId, String toUserId, int amount)
        {
            await MakeApiCall(ApiCalls.move_to_user,
                new Dictionary<string, string>
                {
                    {"to_user_id", toUserId},
                    {"from_user_id", fromUserId},
                    {"amount_doge", Convert.ToString(amount)}
                });
        }

        public async Task<List<DogeUser>> GetUsersInfoAsync()
        {
            var response = await MakeApiCall(ApiCalls.get_users, new Dictionary<string, string>());
            return JsonConvert.DeserializeObject<List<DogeUser>>(response["users"].ToString());
        }

        public async Task<double> GetNetworkHashRateAsync()
        {
            var response = await MakeApiCall(ApiCalls.get_network_hashrate, new Dictionary<string, string>());
            return ToDouble(response["network_hashrate"]);
        }

        public async Task<
            Dictionary<string, double>> GetInfoAsync()
        {
            var response = await MakeApiCall(ApiCalls.get_info, new Dictionary<string, string>());
            return JsonConvert.DeserializeObject<Dictionary<String, Double>>(response["info"].ToString());
        }

        public async Task<List<Transaction>> GetTransactionsAsync(int count, String type)
        {
            return await GetTransactionsAsync(count, type, null, null);
        }

        public async Task<List<Transaction>> GetTransactionsAsync(int count, String type, String label,
                                                                  String identifier)
        {
            if (type != "send" && type != "receive" && type != "move" && type != "fee") throw new Exception("Invalid Transaction type! Options: send, recieve, move, fee");
            if (label != "payment_address" && label != "label") throw new Exception("Invalid Identifier! Options: payment_address, label");

            var response = await MakeApiCall(ApiCalls.get_transactions,
                new Dictionary<string, string> {{"num", Convert.ToString(count)}, {"type", type}, {label, identifier}});

            return JsonConvert.DeserializeObject<List<Transaction>>(response["transactions"].ToString());
        }

        public async Task WithdrawFromUserAsync(String userId, String paymentAddress, int amount, int pin)
        {
            await MakeApiCall(ApiCalls.move_to_user,
                new Dictionary<string, string>
                {
                    {"payment_address", paymentAddress},
                    {"user_id", userId},
                    {"amount_doge", Convert.ToString(amount)},
                    {"pin", Convert.ToString(pin)}
                });
        }

        //////////////////////////////////////////
        //  Helper Methods
        //////////////////////////////////////////

        /// <summary>
        ///     Custom web request function
        /// </summary>
        /// <param name="call">Type of API Call</param>
        /// <param name="args">GET parameters</param>
        /// <returns></returns>
        public async Task<Dictionary<string, object>> MakeApiCall(ApiCalls call, Dictionary<String, String> args)
        {
            try
            {
                //Build URL
                var Params = new StringBuilder();
                foreach (var kvp in args)
                {
                    Params.AppendFormat("{0}={1}&", kvp.Key, kvp.Value);
                }

                var url = String.Format("{0}{1}{2}/?api_key={3}&{4}", Apiurl, ApiRoute, call, ApiKey, Params);
                if (url.EndsWith("&")) url = url.Remove(url.Length - 1);

                using (var client = new HttpClient())
                {
                    var resp = await client.GetAsync(url);
                    var responseText = await resp.Content.ReadAsStringAsync();
                    var responseDict = JsonConvert.DeserializeObject<Dictionary<String, Object>>(responseText);

                    if (!resp.IsSuccessStatusCode && responseDict == null) throw new Exception("Problem with response.");

                    var data = JsonConvert.DeserializeObject<Dictionary<String, Object>>(responseDict["data"].ToString());

                    if (data.ContainsKey("error_message")) throw DogeException.Generate("error from server", new Exception((String) data["error_message"]));
                    
                    return data;
                }
            }
            catch (DogeException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw DogeException.Generate("couldn't connect to server", e);
            }
        }
    }
}