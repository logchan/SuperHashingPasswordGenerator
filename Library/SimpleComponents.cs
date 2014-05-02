/*
 * The BSD license
 * Copyright (c) 2014, lgchan
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of lgchan, nor the names of its contributors may be 
 *       used to endorse or promote products derived from this software without
 *       specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE REGENTS AND CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
/*
 * SimpleComponent.cs
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
