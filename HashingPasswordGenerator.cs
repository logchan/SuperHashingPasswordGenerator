using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// for SecureString
using System.Security;

namespace SuperHashingPasswordGenerator
{
    /// <summary>
    /// Generator of complicated-hashed password
    /// </summary>
    public class HashingPasswordGenerator
    {
        /// <summary>
        /// A list of ISaltProcessors, used to add salt to the hash
        /// </summary>
        private List<ISaltProcessor> saltProcessors;
        /// <summary>
        /// The IHashingProcessor that do the hashing
        /// </summary>
        private IHashingProcessor hashingProcessor;
        /// <summary>
        /// The IPostHashingProcessor that modify the hashing result
        /// </summary>
        private IPostHashingProcessor postHashingProcessor;
        /// <summary>
        /// The total iterations of hashing
        /// </summary>
        private UInt32 totalHashingTimes;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="hashingProcessor">the hashing processor to use</param>
        /// <param name="postHashingProcessor">the post hashing processor to use</param>
        public HashingPasswordGenerator(IHashingProcessor hashingProcessor, IPostHashingProcessor postHashingProcessor)
        {
            this.saltProcessors = new List<ISaltProcessor>();
            this.hashingProcessor = hashingProcessor;
            this.postHashingProcessor = postHashingProcessor;
            this.totalHashingTimes = 1;
        }

        /// <summary>
        /// Add a salt processor to the hashing operation.
        /// </summary>
        /// <param name="saltProcessor">the salt processor to add</param>
        public void AddSaltProcessor(ISaltProcessor saltProcessor)
        {
            this.saltProcessors.Add(saltProcessor);
        }

        /// <summary>
        /// Remove all the salt processors
        /// </summary>
        public void ResetSaltProcessors()
        {
            this.saltProcessors = new List<ISaltProcessor>();
        }

        /// <summary>
        /// Get or set how many iterations of hashing will happen. Must be greater than zero.
        /// </summary>
        public UInt32 TotalHashingTimes
        {
            get
            {
                return this.totalHashingTimes;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("TotalHashingCount must be greater than zero.");
                }
                else
                {
                    this.totalHashingTimes = value;
                }
            }
        }

        /// <summary>
        /// Do the hashing job
        /// </summary>
        /// <param name="secret">the secret that is going to be hashed</param>
        /// <param name="hashingResult">the hashing result</param>
        /// <param name="postHashingResult">the processed hashing result</param>
        public void DoHashing(String secret, out String hashingResult, out String postHashingResult)
        {
            // initialize outs; not necessary but whatever
            hashingResult = secret;
            postHashingResult = "";

            // do the hashings
            Int32 currentHashingCount = 0;
            while (currentHashingCount < this.totalHashingTimes)
            {
                /* Step 1: Salt */
                if (saltProcessors.Count > 0)
                {
                    foreach (var saltProcessor in saltProcessors)
                    {
                        hashingResult = saltProcessor.AddSalt(hashingResult, currentHashingCount);
                    }
                }
                else
                {
                    // no salt processors
                }

                /* Step 2: Hash */
                hashingResult = hashingProcessor.Hash(hashingResult);

                currentHashingCount++;
            }

            // do the post hashing
            postHashingResult = postHashingProcessor.PostHash(hashingResult);

            return;
        }
    }

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
}
