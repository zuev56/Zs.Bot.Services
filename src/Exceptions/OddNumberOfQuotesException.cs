using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zs.Bot.Services.Exceptions
{
    public class OddNumberOfQuotesException : Exception
    {
        public OddNumberOfQuotesException()
            : base("Кавычек должно быть чётное количество!")
        {
            
        }
    }
}
