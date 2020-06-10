using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;

namespace WS_PosData_PMKT.Helpers
{
    internal static class Keys
    {
        internal static string Iv => "IWFvbR1NouI693AnbARpgg==";
        internal static string Key => "R4YT5YLzLdBJE2Ecuz+szulZHsJ+FGKDZasicEvLjO8=";
    }

    public class Crypto
    {
        public T Cypher<T>(T obj, int opt)
        {
            try
            {
                if (ReferenceEquals(obj, null))
                {
                    return default(T);
                }

                var elem = (T)obj;

                foreach (var f in elem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty))
                {
                    var e = f.GetValue(elem);

                    if (f.GetCustomAttributes(typeof(InsurableObjectAttribute), true).Length > 0 && e != null)
                    {

                        var tColl = typeof(ICollection<>);
                        var t = e.GetType();
                        if (t.IsGenericType && tColl.IsAssignableFrom(t.GetGenericTypeDefinition()) ||
                            t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == tColl))
                        {
                            // No es posible aún usar listas heredadas en objetos, se debe usar volviendo
                            // a llamar esta funcion con la lista
                            continue;
                        }

                        Cypher(ref e, opt);


                        f.SetValue(elem, e);
                        continue;
                    }

                    if (f.GetCustomAttributes(typeof(InsurableAttribute), true).Length <= 0 && e != null) continue;

                    if (e is string)
                    {
                        f.SetValue(elem, opt == 1 ? Encrypt(e.ToString()) : Decrypt(e.ToString()));
                    }
                }
                return elem;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Se usa para encriptar o desencriptar objetos, su única limitante son las listas embebidas,
        /// para encriptar listas hay que llamar manualmente .Encrypt(OPT,USERID) sobre cada lista embedida
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="opt"></param>
        /// <param name="user"></param>
        public void Cypher<T>(ref T obj, int opt)
        {
            try
            {
                // ReSharper disable once RedundantCast
                if (obj == null)
                    return;
                var elem = (T)obj;
                foreach (var f in elem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty))
                {
                    var e = f.GetValue(elem);

                    if (f.GetCustomAttributes(typeof(InsurableObjectAttribute), true).Length > 0 && e != null)
                    {

                        var tColl = typeof(ICollection<>);
                        var t = e.GetType();
                        if (t.IsGenericType && tColl.IsAssignableFrom(t.GetGenericTypeDefinition()) ||
                            t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == tColl))
                        {
                            // No es posible aún usar listas heredadas en objetos, se debe usar volviendo
                            // a llamar esta funcion con la lista
                            continue;
                        }
                        Cypher(ref e, opt);


                        f.SetValue(elem, e);
                        continue;
                    }

                    if (f.GetCustomAttributes(typeof(InsurableAttribute), true).Length <= 0 || e == null) continue;

                    if (e is string)
                    {
                        f.SetValue(elem, opt == 1 ? Encrypt(e.ToString()) : Decrypt(e.ToString()));
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// Se usa para encriptar o desencriptar listas de objetos, su única limitante son las listas embebidas,
        /// para encriptar listas hay que llamar manualmente .Encrypt(OPT,USERID) sobre cada lista embedida
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="opt"></param>
        /// <param name="user"></param>
        public void Cypher<T>(ref List<T> objs, int opt)
        {
            try
            {
                foreach (var obj in objs)
                {
                    if (obj == null)
                        continue;
                    // ReSharper disable once RedundantCast
                    var elem = (T)obj;
                    foreach (
                        var f in elem.GetType()
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty))
                    {
                        var e = f.GetValue(elem);

                        if (f.GetCustomAttributes(typeof(InsurableObjectAttribute), true).Length > 0 && e != null)
                        {
                            var tColl = typeof(ICollection<>);
                            var t = e.GetType();
                            if (t.IsGenericType && tColl.IsAssignableFrom(t.GetGenericTypeDefinition()) ||
                                t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == tColl))
                            {
                                // No es posible aún usar listas heredadas en objetos, se debe usar volviendo
                                // a llamar esta funcion con la lista
                                continue;
                            }
                            Cypher(ref e, opt);


                            f.SetValue(elem, e);
                            continue;
                        }

                        if (f.GetCustomAttributes(typeof(InsurableAttribute), true).Length <= 0 || e == null) continue;

                        if (e is string)
                        {
                            f.SetValue(elem, opt == 1 ? Encrypt(e.ToString()) : Decrypt(e.ToString()));
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        public string Encrypt(string plainText)
        {
            try
            {
                return string.IsNullOrEmpty(plainText) ? null : Convert.ToBase64String(EncryptStringToBytes_Aes(plainText, Convert.FromBase64String(Keys.Key), Convert.FromBase64String(Keys.Iv)));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string Decrypt(string cypherText)
        {
            try
            {
                return string.IsNullOrEmpty(cypherText) ? null : DecryptStringFromBytes_Aes(Convert.FromBase64String(cypherText), Convert.FromBase64String(Keys.Key), Convert.FromBase64String(Keys.Iv));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
        {
            byte[] encrypted;
            // Create an Aes object
            // with the specified key and IV.
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // Create a decrytor to perform the stream transform.
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }
        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Declare the string used to hold
            // the decrypted text.
            string plaintext;

            // Create an Aes object
            // with the specified key and IV.
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // Create a decrytor to perform the stream transform.
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }
    }
}