using Benner.Tecnologia.Common.EnterpriseServiceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Casablanca.DataContracts.Faturamento
{
    [DataContract]
    public class ConsultaFaturasRequest : Request
    {
        [DataMember(Order = 1, Name = "TipoData", EmitDefaultValue = true, IsRequired = false)]
        public TipoData TipoData { get; set; }

        [DataMember(Order = 2, Name = "DataInicial", IsRequired = true)]
        public DateTime DataInicial { get; set; }

        [DataMember(Order = 3, Name = "DataFinal", IsRequired = true)]
        public DateTime DataFinal { get; set; }

        [DataMember(Order = 4, Name = "Status", IsRequired = false)]
        public StatusFatura Status { get; set; }

        public ConsultaFaturasRequest()
        {
            TipoData = Faturamento.TipoData.DataConfirmacao;
            Status = StatusFatura.Indefinido;
        }
    }
}
