using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperHashingPasswordGenerator
{
    public interface ISaltProcessor
    {
        String AddSalt(String toSalt, Int32 hashingCount);
    }
}
