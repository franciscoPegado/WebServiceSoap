using Benner.Tecnologia.Common.EnterpriseServiceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Casablanca.DataContracts
{
    [DataContract]
    public class ResponseDefault : Response
    {
        [DataMember(Order = 1, Name = "Excecoes")]
        public List<string> Excecoes { get; set; }
    }
}
