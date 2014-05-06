/*
 * SimpleComponent.cs
 * 
 * lgchan, 2014
 * 
 * Some classes derive from IHashingProcessor, ISaltProcessor and IPostHashingProcessor
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperHashingPasswordGenerator
{

    /***** Some simple SaltProcessors *****/

    /// <summary>
    /// ISaltProcessor. Simply add salt to the end of the string.
    /// </summary>
    public class SimpleSaltAppender : ISaltProcessor
    {
        string salt = "";
        Boolean saltOnceOnly = false;

        /// <summary>
        /// Constructor. Initialize the salt.
        /// </summary>
        /// <param name="salt">the salt to add</param>
        /// <param name="saltOnceOnly">true if the salt is only added in the first time</param>
        public SimpleSaltAppender(string salt, Boolean saltOnceOnly)
        {
            this.salt = salt;
            this.saltOnceOnly = saltOnceOnly;
        }

        string ISaltProcessor.AddSalt(string toSalt, int hashingCount)
        {
            if (saltOnceOnly && hashingCount > 1)
            {
                return toSalt;
            }
            else
            {
                return toSalt + salt;
            }
        }
    }

    /***** Some simple HashingProcessors *****/

    /// <summary>
    /// IHashingProcessor. Do MD5 hash, return lower-cased hex string.
    /// </summary>
    public class MD5Hasher : IHashingProcessor
    {
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

        string IHashingProcessor.Hash(string toHash)
        {
            StringBuilder sb = new StringBuilder();
            byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(toHash));
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    /***** Some simple PostHashingProcessors *****/

    /// <summary>
    /// IPostHashingProcessor. Do nothing for post hashing.
    /// </summary>
    public class DoNothingPostHashing : IPostHashingProcessor
    {
        string IPostHashingProcessor.PostHash(string hashed)
        {
            return hashed;
        }
    }

    /// <summary>
    /// IPostHashingProcessor. The 1st, 3rd, 5th, 7th, ... letters will become upper-cased.
    /// </summary>
    public class MixUpperLowerCasePostHashing : IPostHashingProcessor
    {
        string IPostHashingProcessor.PostHash(string hashed)
        {
            StringBuilder sb = new StringBuilder();
            bool isUpper = false;
            foreach (char c in hashed)
            {
                if(c>='a' && c<='z')
                {
                    isUpper = !isUpper;
                    if (isUpper)
                        sb.Append((char)(c + ('A' - 'a')));
                    else
                        sb.Append(c);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
