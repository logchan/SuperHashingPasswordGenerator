/*
 * SHPGClass.cs
 * 
 * lgchan, 2014
 * 
 * Some classes for super hashing password generator
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperHashingPasswordGenerator
{
    /// <summary>
    /// salt wrapper for SHPG
    /// </summary>
    public class SHPGSalt
    {
        public string Alias { get; set; }
        public string SaltString { get; set; }
        public bool OnceOnly { get; set; }

        public SHPGSalt()
        {
            this.Alias = "";
            this.SaltString = "";
            this.OnceOnly = false;
        }

        public SHPGSalt(string alias, string salt, bool once)
        {
            this.Alias = alias;
            this.SaltString = salt;
            this.OnceOnly = once;
        }

        public SimpleSaltAppender GetSaltAppender()
        {
            return new SimpleSaltAppender(this.SaltString, this.OnceOnly);
        }
    }

    public class SHPGSalts : List<SHPGSalt>
    {
        public SHPGSalts()
        {
        }
        public new void Add(SHPGSalt salt)
        {
            base.Add(salt);
        }
    }
}
