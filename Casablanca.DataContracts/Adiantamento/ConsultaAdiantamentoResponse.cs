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
		[DataMember(Order = 1, Name = "Nome")]
		public string Nome { get; set; }

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

		[DataMember(Order = 2, Name = "Inicio")]
		public DateTime Inicio { get; set; }

		[DataMember(Order = 3, Name = "Final")]
		public DateTime Final { get; set; }

		[DataMember(Order = 4, Name = "Diarias")]
		public int Diarias { get; set; }

		[DataMember(Order = 5, Name = "Seguro")]
		public ItemProduto Seguro { get; set; }

		[DataMember(Order = 6, Name = "Destinos")]
		public List<Destino> Destinos { get; set; }
	}

	[DataContract]
	public class Destino
	{
		[DataMember(Order = 1, Name = "TrOrigem")]
		public Endereco TrOrigem { get; set; }

		[DataMember(Order = 2, Name = "TrDestino")]
		public Endereco TrDestino { get; set; }

		[DataMember(Order = 3, Name = "Inicio")]
		public DateTime Inicio { get; set; }

		[DataMember(Order = 4, Name = "Final")]
		public DateTime Final { get; set; }

		[DataMember(Order = 5, Name = "Diarias")]
		public int Diarias { get; set; }

		[DataMember(Order = 6, Name = "Reservas")]
		public List<Reserva> Reservas { get; set; }
	}

	[DataContract]
	public class Reserva
	{
		[DataMember(Order = 1, Name = "AéreoIda")]
		public List<ItemProduto> AéreoIda { get; set; }

		[DataMember(Order = 2, Name = "Hotel")]
		public ItemProduto Hotel { get; set; }

		[DataMember(Order = 3, Name = "Carro")]
		public ItemProduto Carro { get; set; }

		[DataMember(Order = 4, Name = "Miscelanio")]
		public List<ItemProduto> Miscelanio { get; set; }

		[DataMember(Order = 5, Name = "AéreoVolta")]
		public List<ItemProduto> AéreoVolta { get; set; }
	}

	[DataContract]
	public class ItemProduto
	{
		[DataMember(Order = 1, Name = "CodigoCasablanca")]
		public string CodigoCasablanca { get; set; }

		[DataMember(Order = 2, Name = "TipoProduto")]
		public string TipoProduto { get; set; }

		[DataMember(Order = 3, Name = "TipoMiscelanio")]
		public string TipoMiscelanio { get; set; }

		[DataMember(Order = 4, Name = "Loc")]
		public string Loc { get; set; }

		[DataMember(Order = 5, Name = "Bilhete")]
		public string Bilhete { get; set; }

		[DataMember(Order = 6, Name = "Reemisao")]
		public string Reemisao { get; set; }

		[DataMember(Order = 7, Name = "Confirmacao")]
		public string Confirmacao { get; set; }

		[DataMember(Order = 8, Name = "Aprovador")]
		public string Aprovador { get; set; }

		[DataMember(Order = 9, Name = "Fornecedor")]
		public string Fornecedor { get; set; }

		[DataMember(Order = 10, Name = "TrOrigem")]
		public Endereco TrOrigem { get; set; }

		[DataMember(Order = 11, Name = "TrDestino")]
		public Endereco TrDestino { get; set; }

		[DataMember(Order = 12, Name = "Inicio")]
		public DateTime Inicio { get; set; }

		[DataMember(Order = 13, Name = "Final")]
		public DateTime Final { get; set; }

		[DataMember(Order = 14, Name = "Diarias")]
		public int Diarias { get; set; }
	}

    [DataContract]
	public class Endereco
    {
        [DataMember(Order = 1, Name = "Aeroporto")]
        public string Aeroporto { get; set; }

        [DataMember(Order = 2, Name = "Municipio")]
        public string Municipio { get; set; }

        [DataMember(Order = 3, Name = "Estado")]
        public string Estado { get; set; }

        [DataMember(Order = 4, Name = "Regiao")]
        public string Regiao { get; set; }

        [DataMember(Order = 5, Name = "Pais")]
        public string Pais { get; set; }

        [DataMember(Order = 6, Name = "Continente")]
        public string Continente { get; set; }
    }
}