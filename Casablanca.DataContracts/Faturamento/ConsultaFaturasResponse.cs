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
    public class ConsultaFaturasResponse : ResponseDefault
    {
        [DataMember(Order = 1, Name = "Faturas")]
        public List<Fatura> Faturas { get; set; }
    }

    [DataContract]
    public class Fatura
    {
        [DataMember(Order = 1, Name = "CNPJCliente")]
        public string CNPJCliente { get; set; }

        [DataMember(Order = 2, Name = "NomeCliente")]
        public string NomeCliente { get; set; }

        [DataMember(Order = 3, Name = "CodigoCasablanca")]
        public int CodigoCasablanca { get; set; }

        [DataMember(Order = 4, Name = "Numero")]
        public string Numero { get; set; }

        [DataMember(Order = 5, Name = "Status")]
        public StatusFatura Status { get; set; }

        [DataMember(Order = 6, Name = "DataCriacao")]
        public DateTime DataCriacao { get; set; }

        [DataMember(Order = 7, Name = "DataConfirmacao")]
        public DateTime DataConfirmacao { get; set; }

        [DataMember(Order = 8, Name = "DataVencimento")]
        public DateTime DataVencimento { get; set; }

        [DataMember(Order = 9, Name = "Reservas")]
        public List<ItemReserva> Reservas { get; set; }

        [DataMember(Order = 10, Name = "FEEs")]
        public List<ItemFEE> FEEs { get; set; }

        [DataMember(Order = 11, Name = "Reembolsos")]
        public List<ItemReembolso> Reembolsos { get; set; }

        [DataMember(Order = 12, Name = "LinkPDFFatura")]
        public string LinkPDFFatura { get; set; }

        [DataMember(Order = 13, Name = "LinkPDFBoleto")]
        public string LinkPDFBoleto { get; set; }
    }

    [DataContract]
    public enum TipoData
    {
        [EnumMember]
        DataCriacao = 0,
        [EnumMember]
        DataConfirmacao,
        [EnumMember]
        DataVencimento
    }

    [DataContract]
    public enum StatusFatura
    {
        [EnumMember]
        Indefinido = 0,
        [EnumMember]
        Nova,
        [EnumMember]
        Cadastrada,
        [EnumMember]
        Gerada,
        [EnumMember]
        Emitida,
        [EnumMember]
        Cancelada,
        [EnumMember]
        Liquidada
    }

    [DataContract]
    public enum Produto
    {
        [EnumMember]
        Aereo = 1,
        [EnumMember]
        Hotel,
        [EnumMember]
        Carro,
        [EnumMember]
        Servicos
    }
    
    [DataContract]
    public class ItemReserva
    {
        [DataMember(Order = 1, Name = "DataEmissao")]
        public DateTime DataEmissao { get; set; }

		[DataMember(Order = 2, Name = "NumeroReserva")]
		public string NumeroReserva { get; set; }

		[DataMember(Order = 3, Name = "CodigoReserva")]
		public string CodigoReserva { get; set; }

		[DataMember(Order = 3, Name = "InfOS")]
		public string InfOS { get; set; }

		[DataMember(Order = 4, Name = "InfMotivo")]
		public string InfMotivo { get; set; }

		[DataMember(Order = 4, Name = "Observacoes")]
		public string Observacoes { get; set; }

		[DataMember(Order = 5, Name = "Fornecedor")]
        public string Fornecedor { get; set; }

        [DataMember(Order = 6, Name = "Produto")]
        public Produto Produto { get; set; }

        [DataMember(Order = 7, Name = "Solicitante")]
        public string Solicitante { get; set; }

        [DataMember(Order = 8, Name = "Passageiro")]
        public string Passageiro { get; set; }

        [DataMember(Order = 9, Name = "Trecho")]
        public string Trecho { get; set; }

        [DataMember(Order = 10, Name = "Cidade")]
        public string Cidade { get; set; }

		[DataMember(Order = 11, Name = "CentroCusto")]
		public string CentroCusto { get; set; }

		[DataMember(Order = 12, Name = "Projeto")]
		public string Projeto { get; set; }

		[DataMember(Order = 13, Name = "DataInicial")]
        public DateTime DataInicial { get; set; }

        [DataMember(Order = 14, Name = "DataFinal")]
        public DateTime DataFinal { get; set; }

        [DataMember(Order = 15, Name = "QuantidadeDiarias")]
        public int QuantidadeDiarias { get; set; }

        [DataMember(Order = 16, Name = "Tarifa")]
        public decimal Tarifa { get; set; }

        [DataMember(Order = 17, Name = "TaxaEmbarque")]
        public decimal TaxaEmbarque { get; set; }

        [DataMember(Order = 16, Name = "TaxaServico")]
        public decimal TaxaServico { get; set; }

        [DataMember(Order = 17, Name = "TaxaServicoFEE")]
        public decimal TaxaServicoFEE { get; set; }

        [DataMember(Order = 18, Name = "TaxaDU")]
        public decimal TaxaDU { get; set; }

        [DataMember(Order = 19, Name = "Extras")]
        public decimal Extras { get; set; }

        [DataMember(Order = 20, Name = "OutrasTaxas")]
        public decimal OutrasTaxas { get; set; }

        [DataMember(Order = 21, Name = "Desconto")]
        public decimal Desconto { get; set; }
    }

    [DataContract]
    public class ItemFEE
    {
        [DataMember(Order = 1, Name = "DataEmissao")]
        public DateTime DataEmissao { get; set; }

        [DataMember(Order = 2, Name = "NumeroReserva")]
        public string NumeroReserva { get; set; }

        [DataMember(Order = 3, Name = "Fornecedor")]
        public string Fornecedor { get; set; }

        [DataMember(Order = 4, Name = "Produto")]
        public Produto Produto { get; set; }

        [DataMember(Order = 5, Name = "Solicitante")]
        public string Solicitante { get; set; }

        [DataMember(Order = 6, Name = "Passageiro")]
        public string Passageiro { get; set; }

        [DataMember(Order = 7, Name = "CentroCusto")]
        public string CentroCusto { get; set; }

        [DataMember(Order = 8, Name = "ValorFEE")]
        public decimal ValorFEE { get; set; }

        [DataMember(Order = 9, Name = "Trechos")]
        public string Trechos { get; set; }

        [DataMember(Order = 10, Name = "DataIni")]
        public DateTime DataIni { get; set; }

        [DataMember(Order = 11, Name = "DataFim")]
        public DateTime DataFim { get; set; }


    }

    [DataContract]
    public class ItemReembolso
    {
        [DataMember(Order = 1, Name = "DataEmissao")]
        public DateTime DataEmissao { get; set; }

        [DataMember(Order = 2, Name = "NumeroReserva")]
        public string NumeroReserva { get; set; }

        [DataMember(Order = 3, Name = "Fornecedor")]
        public string Fornecedor { get; set; }

        [DataMember(Order = 4, Name = "Produto")]
        public Produto Produto { get; set; }

        [DataMember(Order = 5, Name = "Solicitante")]
        public string Solicitante { get; set; }

        [DataMember(Order = 6, Name = "Passageiro")]
        public string Passageiro { get; set; }

        [DataMember(Order = 7, Name = "Trecho")]
        public string Trecho { get; set; }

        [DataMember(Order = 8, Name = "CentroCusto")]
        public string CentroCusto { get; set; }

        [DataMember(Order = 9, Name = "Tarifa")]
        public decimal Tarifa { get; set; }

        [DataMember(Order = 10, Name = "Taxa")]
        public decimal Taxa { get; set; }

        [DataMember(Order = 11, Name = "Multa")]
        public decimal Multa { get; set; }

        [DataMember(Order = 12, Name = "ValorReembolso")]
        public decimal ValorReembolso { get; set; }
    }
}
