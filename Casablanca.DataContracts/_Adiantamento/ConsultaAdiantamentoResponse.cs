using Benner.Tecnologia.Common.EnterpriseServiceLibrary;
using Casablanca.DataContracts.Adiantamento;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Casablanca.DataContracts.Adiantamento
{
    [DataContract]
    public class ConsultaAdiantamentoResponse : ResponseDefault
    {
        [DataMember(Order = 1, Name = "CentrosCusto")]
        public List<CentroCusto> CentrosCusto { get; set; }
    }

    [DataContract]
    public class CentroCusto
    {
        [DataMember(Order = 1, Name = "Nome")]
        public string Nome { get; set; }

        [DataMember(Order = 2, Name = "Cliente")]
        public string Cliente { get; set; }

        [DataMember(Order = 3, Name = "Viajantes")]
        public List<Viajante> Viajantes { get; set; }
    }

    [DataContract]
    public class Viajante
    {
		[DataMember(Order = 1, Name = "Passageiro")]
		public string Passageiro { get; set; }

		[DataMember(Order = 2, Name = "CPF")]
		public string CPF { get; set; }
		
		[DataMember(Order = 3, Name = "Viagens")]
        public List<Viagem> Viagens { get; set; }
	}

    [DataContract]
    public class Viagem
    {
		[DataMember(Order = 1, Name = "Trechos")]
		public string Trechos { get; set; }

		[DataMember(Order = 2, Name = "Origem")]
		public Local Origem { get; set; }

		[DataMember(Order = 3, Name = "Destino")]
		public Local Destino { get; set; }

        [DataMember(Order = 4, Name = "Diarias")]
        public int Diarias { get; set; }

		[DataMember(Order = 5, Name = "Reservas")]
		public List<Reserva> Reservas { get; set; }
    }
    
    [DataContract]
    public class Reserva
    {
		[DataMember(Order = 1, Name = "Produto")]
		public string Produto { get; set; }

		[DataMember(Order = 2, Name = "Tipo")]
		public string Tipo { get; set; }

		[DataMember(Order = 3, Name = "CodigoCasablanca")]
        public string CodigoCasablanca { get; set; }

        [DataMember(Order = 4, Name = "Loc")]
        public string Loc { get; set; }

        [DataMember(Order = 5, Name = "Bilhete")]
        public string Bilhete { get; set; }

        [DataMember(Order = 6, Name = "Confirmacao")]
        public string Confirmacao { get; set; }

		[DataMember(Order = 7, Name = "DataEmissao")]
		public DateTime DataEmissao { get; set; }

		[DataMember(Order = 8, Name = "Emissor")]
		public string Emissor { get; set; }

		[DataMember(Order = 9, Name = "Aprovador")]
		public string Aprovador { get; set; }

		[DataMember(Order = 10, Name = "Consolidador")]
		public string Consolidador { get; set; }

		[DataMember(Order = 11, Name = "Fornecedor")]
		public string Fornecedor { get; set; }

		[DataMember(Order = 12, Name = "Origem")]
		public Local Origem { get; set; }

        [DataMember(Order = 13, Name = "Destino")]
		public Local Destino { get; set; }
    }

    [DataContract]
	public class Local
    {
        [DataMember(Order = 1, Name = "DataHora")]
		public DateTime DataHora { get; set; }

        [DataMember(Order = 2, Name = "Aeroporto")]
        public string Aeroporto { get; set; }

        [DataMember(Order = 3, Name = "Municipio")]
        public string Municipio { get; set; }

        [DataMember(Order = 4, Name = "Estado")]
        public string Estado { get; set; }

        [DataMember(Order = 5, Name = "Regiao")]
        public string Regiao { get; set; }

        [DataMember(Order = 6, Name = "Pais")]
        public string Pais { get; set; }

        [DataMember(Order = 7, Name = "Continente")]
        public string Continente { get; set; }
    }
}