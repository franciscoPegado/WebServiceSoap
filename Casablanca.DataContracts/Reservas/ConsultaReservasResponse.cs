using Benner.Tecnologia.Common.EnterpriseServiceLibrary;
using Casablanca.DataContracts.Faturamento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Casablanca.DataContracts.Reservas
{
    [DataContract]
    public class ConsultaReservasResponse : ResponseDefault
    {
        [DataMember(Order = 1, Name = "Reservas")]
        public List<Reserva> Reservas { get; set; }
    }

    [DataContract]
    public class Reserva
    {
        [DataMember(Order = 1, Name = "DataEmissao")]
        public DateTime DataEmissao { get; set; }

        [DataMember(Order = 2, Name = "NumeroReserva")]
        public string NumeroReserva { get; set; }

        [DataMember(Order = 3, Name = "GrupoEmpresarial")]
        public string GrupoEmpresarial { get; set; }

        [DataMember(Order = 4, Name = "Cliente")]
        public string Cliente { get; set; }        

        [DataMember(Order = 5, Name = "Passageiro")]
        public string Passageiro { get; set; }

        [DataMember(Order = 6, Name = "Produto")]
        public Produto Produto { get; set; }

        [DataMember(Order = 7, Name = "DataInicial")]
        public DateTime DataInicial { get; set; }

        [DataMember(Order = 8, Name = "DataFinal")]
        public DateTime DataFinal { get; set; }

        [DataMember(Order = 9, Name = "QuantidadeDiarias")]
        public int QuantidadeDiarias { get; set; }

        [DataMember(Order = 10, Name = "Municipio")]
        public string Municipio { get; set; }

        [DataMember(Order = 11, Name = "Estado")]
        public string Estado { get; set; }

        [DataMember(Order = 12, Name = "Regiao")]
        public string Regiao { get; set; }

        [DataMember(Order = 13, Name = "Pais")]
        public string Pais { get; set; }

        [DataMember(Order = 14, Name = "Continente")]
        public string Continente { get; set; }
    }

}
