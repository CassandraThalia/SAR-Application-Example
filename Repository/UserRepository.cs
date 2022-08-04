using SAR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace SAR.Repository
{
    public class UserRepository
    {
        public static int CheckCredential(string wNum, string pWord)
        {
           // connection using a limited access to only fetch data to verify password
            string connectionString = @"Data Source=localhost;Initial Catalog=sar_db; User Id=appAuth; Password=#Authhello";
            string GetDataQuery = "EXEC sp_getInstructorInfo '" + wNum + "'";
            string pWordInDB = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    pWordInDB = reader.GetString(3);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception eSql)
            {
                Task showServerError = ShowErrorDialog(eSql.Message);
                Debug.WriteLine("Exception: " + eSql.StackTrace);
                return 2;
            }

            // to hash user's input
            string hashToCompare = GetHashedStr(pWord);

            if (hashToCompare == pWordInDB)
            {
                return 0;
            } else { return 1; }
        }

        public static void GetUserInfo(User user)
        {
            string GetDataQuery = "EXEC sp_getInstructorInfo '" + user.WNumber + "'";

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (!reader.IsDBNull(1))
                                    {
                                        user.FullName = reader.GetString(1);
                                    } else
                                    {
                                        user.FullName = "";
                                    }
                                    // check if the user is Academic Chair
                                    user.IsChair = reader.GetBoolean(2);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception eSql)
            {
                Task showServerError = ShowErrorDialog(eSql.Message);
                Debug.WriteLine("Exception: " + eSql.StackTrace);
            }
        }

        public static void GetLastLogin(User user)
        {
            string GetDataQuery = "EXEC sp_getLastLoginTimestamp '" + user.WNumber + "'";

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    if (!reader.IsDBNull(0))
                                    {
                                        user.LastLogin = reader.GetDateTime(0);
                                    } else
                                    {
                                        user.LastLogin = DateTime.MinValue;
                                    }
                                        
                                }
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception eSql)
            {
                Task showServerError = ShowErrorDialog(eSql.Message);
                Debug.WriteLine("Exception: " + eSql.StackTrace);
            }
        }

        public static void UpdateLastLoginTimestamp(User user)
        {
            DateTime date = DateTime.Now;
            string GetDataQuery = "EXEC sp_updateLastLoginTimestamp '" + user.WNumber + "', '" + date + "'";

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception eSql)
            {
                Task showServerError = ShowErrorDialog(eSql.Message);
                Debug.WriteLine("Exception: " + eSql.StackTrace);
            }
        }

        public static async Task ShowErrorDialog(string errorMessage)
        {
            MessageDialog messageDialog = new MessageDialog(errorMessage, "Error");
            await messageDialog.ShowAsync();
        }

        //Make sure to hash password BEFORE sending to this function
        public static void UpdatePassword(User user, string newPass)
        {
            string GetDataQuery = "EXEC sp_updateUserPassword '" + user.WNumber + "', '" + newPass + "'";
            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetDataQuery;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception eSql)
            {
                Task showServerError = ShowErrorDialog(eSql.Message);
                Debug.WriteLine("Exception: " + eSql.StackTrace);
            }
        }

        // methods to do the hashing
        // https://docs.microsoft.com/en-us/windows/uwp/security/intro-to-secure-windows-app-development
        public static string GetHashedStr(string str)
        {
            // Create a string that contains the name of the
            // hashing algorithm to use.
            string strAlgName = HashAlgorithmNames.Sha512;

            // Create a HashAlgorithmProvider object.
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);

            // Create a CryptographicHash object. This object can be reused to continually
            // hash new messages.
            CryptographicHash objHash = objAlgProv.CreateHash();

            // hash the string
            IBuffer buffStr = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf16BE);
            objHash.Append(buffStr);
            IBuffer buffHash = objHash.GetValueAndReset();

            // Convert the hashes to string values (for display);
            return CryptographicBuffer.EncodeToBase64String(buffHash);
        }
        
        // batch hashing for testing only
        public static List<string> CreateHashedStrings(List<string> strList)
        {
            // Create a string that contains the name of the
            // hashing algorithm to use.
            string strAlgName = HashAlgorithmNames.Sha512;

            // Create a HashAlgorithmProvider object.
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);

            // Create a CryptographicHash object. This object can be reused to continually
            // hash new messages.
            CryptographicHash objHash = objAlgProv.CreateHash();

            // hash the strings
            List<string> hashedList = new List<string>();
            foreach (string str in strList)
            {
                IBuffer buffStr = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf16BE);
                objHash.Append(buffStr);
                IBuffer buffHash = objHash.GetValueAndReset();

                // Convert the hashes to string values (for display);
                string strHash = CryptographicBuffer.EncodeToBase64String(buffHash);
                hashedList.Add(strHash);
            }

            return hashedList;
        }
    }
}
