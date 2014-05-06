/*
 * IHashingProcessor.cs
 * 
 * lgchan, 2014
 * 
 * the interface
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperHashingPasswordGenerator
{
    public interface IHashingProcessor
    {
        String Hash(String toHash);
    }
}
