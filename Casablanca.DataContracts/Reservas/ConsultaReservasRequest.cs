using Benner.Tecnologia.Common.EnterpriseServiceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Casablanca.DataContracts.Reservas
{
    [DataContract]
    public class ConsultaReservasRequest : Request
    {
        [DataMember(Order = 1, Name = "DataInicial", IsRequired = true)]
        public DateTime DataInicial { get; set; }

        [DataMember(Order = 2, Name = "DataFinal", IsRequired = true)]
        public DateTime DataFinal { get; set; }
    }
}
